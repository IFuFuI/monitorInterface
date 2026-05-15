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

    ' 1. AQUÍ AGREGAMOS EL PARÁMETRO OPCIONAL "omitirTimestamp"
    Public Sub writeJournal(ByVal strTexto As String, Optional ByVal omitirTimestamp As Boolean = False)

        Try
            Dim timestamp As String = Format(Now(), "dd/MM/yy HH:mm:ss")
            ' Reemplazar prefijos de placeholder "X[" por la secuencia ESC real (Chr(27) + "[")
            Try
                If strTexto.Contains("X[") Then
                    strTexto = strTexto.Replace("X[", Chr(27) & "[")
                End If
                If strTexto.Contains("x[") Then
                    strTexto = strTexto.Replace("x[", Chr(27) & "[")
                End If
                ' También soportar placeholder X( -> ESC(
                If strTexto.Contains("X(") Then
                    strTexto = strTexto.Replace("X(", Chr(27) & "(")
                End If
                If strTexto.Contains("x(") Then
                    strTexto = strTexto.Replace("x(", Chr(27) & "(")
                End If
                If strTexto.Contains("X)") Then
                    strTexto = strTexto.Replace("X)", Chr(27) & ")")
                End If
                If strTexto.Contains("x)") Then
                    strTexto = strTexto.Replace("x)", Chr(27) & ")")
                End If
            Catch ex As Exception
                ' Si falla la conversión, continuamos y escribimos el texto tal cual
            End Try

            ' 2. VALIDAMOS EL SWITCH ANTES DE ESCRIBIR
            If omitirTimestamp Then
                strTexto = strTexto & vbCrLf
            Else
                strTexto = timestamp & " " & strTexto & vbCrLf
            End If

            File.AppendAllText("C:\appMain\Journal\Journal.txt", strTexto)
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