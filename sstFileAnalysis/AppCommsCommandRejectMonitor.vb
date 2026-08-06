Imports System.IO
Imports System.Text

Public Class CommandRejectDetectedEventArgs
    Inherits EventArgs

    Public ReadOnly Property Code As String
    Public ReadOnly Property Luno As String
    Public ReadOnly Property MessageTime As String
    Public ReadOnly Property RawLine As String

    Public Sub New(ByVal code As String, ByVal luno As String, ByVal messageTime As String, ByVal rawLine As String)
        Me.Code = code
        Me.Luno = luno
        Me.MessageTime = messageTime
        Me.RawLine = rawLine
    End Sub
End Class

Public Class AppCommsCommandRejectMonitor
    Implements IDisposable

    Private Shared ReadOnly FileSeparator As Char = ChrW(&H1C)
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Private ReadOnly syncRoot As New Object()
    Private ReadOnly pollTimer As System.Timers.Timer
    Private watcher As FileSystemWatcher = Nothing
    Private currentPath As String = String.Empty
    Private lastByteOffset As Long = 0
    Private started As Boolean = False
    Private processing As Boolean = False
    Private readAgain As Boolean = False
    Private disposed As Boolean = False

    Public Event CommandRejectDetected As EventHandler(Of CommandRejectDetectedEventArgs)

    Public Sub New()
        pollTimer = New System.Timers.Timer(1000)
        pollTimer.AutoReset = False
        AddHandler pollTimer.Elapsed, AddressOf OnPollElapsed
    End Sub

    Public Sub Start()
        SyncLock syncRoot
            If started Then Exit Sub
            started = True
            ConfigureCurrentFileLocked(True)
            pollTimer.Start()
        End SyncLock

        RequestProcess()
        log.Debug("Monitor appComms iniciado")
    End Sub

    Public Sub [Stop]()
        SyncLock syncRoot
            started = False
            pollTimer.Stop()
            DisposeWatcherLocked()
        End SyncLock

        log.Debug("Monitor appComms detenido")
    End Sub

    Private Sub OnPollElapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs)
        Try
            RequestProcess()
        Finally
            SyncLock syncRoot
                If started Then pollTimer.Start()
            End SyncLock
        End Try
    End Sub

    Private Sub OnWatcherChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
        RequestProcess()
    End Sub

    Private Sub OnWatcherError(ByVal source As Object, ByVal e As ErrorEventArgs)
        log.Error("FileSystemWatcher appComms error: " & e.GetException().Message)
        SyncLock syncRoot
            DisposeWatcherLocked()
            ConfigureCurrentFileLocked(False)
        End SyncLock
        RequestProcess()
    End Sub

    Private Sub RequestProcess()
        SyncLock syncRoot
            If Not started Then Exit Sub

            If processing Then
                readAgain = True
                Exit Sub
            End If

            processing = True
        End SyncLock

        Threading.ThreadPool.QueueUserWorkItem(AddressOf ProcessWorker)
    End Sub

    Private Sub ProcessWorker(ByVal state As Object)
        Do
            Try
                ProcessNewBytes()
            Catch ex As Exception
                log.Error("Error procesando appComms.log: " & ex.Message)
            End Try

            SyncLock syncRoot
                If readAgain Then
                    readAgain = False
                Else
                    processing = False
                    Exit Do
                End If
            End SyncLock
        Loop
    End Sub

    Private Sub ProcessNewBytes()
        Dim targetPath As String
        Dim startOffset As Long

        SyncLock syncRoot
            ConfigureCurrentFileLocked(False)
            targetPath = currentPath
            startOffset = lastByteOffset
        End SyncLock

        If String.IsNullOrEmpty(targetPath) OrElse Not File.Exists(targetPath) Then Exit Sub

        Dim newOffset As Long = startOffset
        Dim lineBytes As New List(Of Byte)()

        Using fs As New FileStream(targetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            If fs.Length < startOffset Then
                startOffset = 0
                newOffset = 0
            End If

            fs.Seek(startOffset, SeekOrigin.Begin)

            Dim buffer(8191) As Byte
            Dim absolutePosition As Long = startOffset
            Dim bytesRead As Integer = fs.Read(buffer, 0, buffer.Length)

            While bytesRead > 0
                For i As Integer = 0 To bytesRead - 1
                    Dim b As Byte = buffer(i)
                    absolutePosition += 1

                    If b = 10 Then
                        ProcessLine(DecodeLine(lineBytes))
                        lineBytes.Clear()
                        newOffset = absolutePosition
                    Else
                        lineBytes.Add(b)
                    End If
                Next

                bytesRead = fs.Read(buffer, 0, buffer.Length)
            End While
        End Using

        SyncLock syncRoot
            If String.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase) Then
                lastByteOffset = newOffset
            End If
        End SyncLock
    End Sub

    Private Function DecodeLine(ByVal bytes As List(Of Byte)) As String
        While bytes.Count > 0 AndAlso bytes(bytes.Count - 1) = 13
            bytes.RemoveAt(bytes.Count - 1)
        End While

        If bytes.Count = 0 Then Return String.Empty
        Return Encoding.Default.GetString(bytes.ToArray())
    End Function

    Private Sub ProcessLine(ByVal line As String)
        Dim detected As CommandRejectDetectedEventArgs = Nothing

        If TryParseCommandReject(line, detected) Then
            log.Warn("Specific Command Reject detectado. Codigo=" & detected.Code & " LUNO=" & detected.Luno & " Hora=" & detected.MessageTime)
            RaiseEvent CommandRejectDetected(Me, detected)
        End If
    End Sub

    Private Function TryParseCommandReject(ByVal line As String, ByRef detected As CommandRejectDetectedEventArgs) As Boolean
        detected = Nothing

        If String.IsNullOrEmpty(line) Then Return False
        If Not line.StartsWith("[OUT]", StringComparison.OrdinalIgnoreCase) Then Return False

        Dim messageStart As Integer = line.IndexOf("<MESSAGEOUT>", StringComparison.OrdinalIgnoreCase)
        If messageStart < 0 Then Return False

        Dim messageEnd As Integer = line.IndexOf("</MESSAGEOUT>", messageStart, StringComparison.OrdinalIgnoreCase)
        If messageEnd < 0 Then Return False

        Dim payloadStart As Integer = line.LastIndexOf("[", messageEnd)
        If payloadStart < 0 OrElse payloadStart <= messageStart Then Return False

        Dim payloadEnd As Integer = line.IndexOf("]", payloadStart + 1)
        If payloadEnd < 0 OrElse payloadEnd > messageEnd Then Return False

        Dim payload As String = line.Substring(payloadStart + 1, payloadEnd - payloadStart - 1)
        Dim fields() As String = payload.Split(New Char() {FileSeparator})

        If fields.Length <> 5 Then Return False
        If fields(0) <> "22" Then Return False
        If String.IsNullOrEmpty(fields(1)) Then Return False
        If fields(2) <> String.Empty Then Return False
        If fields(3) <> "C" Then Return False
        If Not IsCommandRejectCode(fields(4)) Then Return False

        detected = New CommandRejectDetectedEventArgs(fields(4), fields(1), ExtractMessageTime(line, messageStart), line)
        Return True
    End Function

    Private Function IsCommandRejectCode(ByVal code As String) As Boolean
        If String.IsNullOrEmpty(code) OrElse code.Length <> 3 Then Return False
        If code(0) <> "A"c Then Return False

        For i As Integer = 1 To 2
            If Not Char.IsLetterOrDigit(code(i)) Then Return False
        Next

        Return True
    End Function

    Private Function ExtractMessageTime(ByVal line As String, ByVal messageStart As Integer) As String
        Dim timeStart As Integer = messageStart + "<MESSAGEOUT>".Length
        Dim timeEnd As Integer = line.IndexOf(" - ", timeStart, StringComparison.Ordinal)

        If timeEnd > timeStart Then
            Return line.Substring(timeStart, timeEnd - timeStart).Trim()
        End If

        Return Format(Now(), "HH:mm:ss.fff")
    End Function

    Private Sub ConfigureCurrentFileLocked(ByVal initialStart As Boolean)
        Dim targetPath As String = CurrentAppCommsLogPath()
        If String.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase) Then Exit Sub

        currentPath = targetPath
        If initialStart AndAlso File.Exists(currentPath) Then
            lastByteOffset = GetFileLength(currentPath)
        Else
            lastByteOffset = 0
        End If

        DisposeWatcherLocked()
        ConfigureWatcherLocked(currentPath)
    End Sub

    Private Function GetFileLength(ByVal path As String) As Long
        Try
            Using fs As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
                Return fs.Length
            End Using
        Catch ex As Exception
            log.Error("No se pudo obtener longitud de appComms.log: " & ex.Message)
            Return 0
        End Try
    End Function

    Private Sub ConfigureWatcherLocked(ByVal path As String)
        Try
            Dim directory As String = System.IO.Path.GetDirectoryName(path)
            If String.IsNullOrEmpty(directory) OrElse Not System.IO.Directory.Exists(directory) Then
                log.Warn("Directorio appComms no existe: " & directory)
                Exit Sub
            End If

            watcher = New FileSystemWatcher()
            watcher.Path = directory
            watcher.Filter = System.IO.Path.GetFileName(path)
            watcher.IncludeSubdirectories = False
            watcher.InternalBufferSize = 65536
            watcher.NotifyFilter = NotifyFilters.LastWrite Or NotifyFilters.Size Or NotifyFilters.FileName
            AddHandler watcher.Changed, AddressOf OnWatcherChanged
            AddHandler watcher.Created, AddressOf OnWatcherChanged
            AddHandler watcher.Error, AddressOf OnWatcherError
            watcher.EnableRaisingEvents = True
        Catch ex As Exception
            log.Error("No se pudo iniciar FileSystemWatcher appComms: " & ex.Message)
        End Try
    End Sub

    Private Sub DisposeWatcherLocked()
        If watcher Is Nothing Then Exit Sub

        Try
            watcher.EnableRaisingEvents = False
            RemoveHandler watcher.Changed, AddressOf OnWatcherChanged
            RemoveHandler watcher.Created, AddressOf OnWatcherChanged
            RemoveHandler watcher.Error, AddressOf OnWatcherError
            watcher.Dispose()
        Catch ex As Exception
            log.Error("Error liberando FileSystemWatcher appComms: " & ex.Message)
        Finally
            watcher = Nothing
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If disposed Then Exit Sub
        disposed = True
        [Stop]()
        pollTimer.Dispose()
    End Sub
End Class
