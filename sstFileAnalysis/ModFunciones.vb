Imports System.IO
Imports System.Xml

Module ModFunciones
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Private Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Integer, ByVal lpFileName As String) As Integer
    Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Integer

    Public Function Read_Ini(ByVal strSeccion As String, ByVal strLlave As String, ByVal strPath As String) As String
        Dim bufer As String
        Dim Len_Value As Integer
        Read_Ini = ""
        Try
            bufer = New String(Chr(0), 4000)

            Len_Value = GetPrivateProfileString(strSeccion, strLlave, 0, bufer, Len(bufer), strPath)

            Read_Ini = Microsoft.VisualBasic.Strings.Left(bufer, Len_Value)
        Catch ex As Exception
            log.Error("error en Read_Ini: " & ex.Message)
        End Try
    End Function

    Public Sub writeJournal(ByVal strTexto As String)

        Try
            strTexto = "*" & strTexto & "*" & vbCrLf
            File.AppendAllText("C:\appMain\Journal\Journal.log", strTexto)
        Catch ex As Exception
            log.Error("[Trace] [Error Journal]" + ex.Message)
        End Try
    End Sub

    Private Function isProcessAlive(strProcess As String) As Boolean
        Dim p As Process
        Try
            isProcessAlive = False
            For Each p In Process.GetProcesses
                If UCase(p.ProcessName) = UCase(strProcess) Then
                    'Existe proceso ncrmonitor
                    isProcessAlive = True
                    Exit For
                End If
            Next
            Return isProcessAlive
        Catch ex As Exception
            isProcessAlive = True
        End Try
    End Function

End Module
