Imports System.Net

Module Mod_Funciones
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Private Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Integer, ByVal lpFileName As String) As Integer
    Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Integer
    Public customerId As String
    Public Function Read_Ini(ByVal strSeccion As String, ByVal strLlave As String, ByVal strPath As String) As String
        Dim bufer As String
        Dim Len_Value As Integer
        Read_Ini = ""
        Try
            bufer = New String(Chr(0), 4000)

            Len_Value = GetPrivateProfileString(strSeccion, strLlave, "", bufer, Len(bufer), strPath)

            Read_Ini = Microsoft.VisualBasic.Strings.Left(bufer, Len_Value)
        Catch ex As Exception
            log.Error("", ex)
        End Try
    End Function

    Public Function getCustomerId() As String
        Try
            Return Read_Ini("Configuracion", "Customer_Id", Constants.strConfigIniFile).Replace(" ", "")
        Catch ex As Exception
            Return ""
            log.Error("", ex)
        End Try
    End Function

    Public Function isValueInteger(value As String) As Boolean
        Dim i As Integer
        Dim bReturn As Boolean
        If Integer.TryParse(value, i) Then
            bReturn = True
        Else
            bReturn = False
        End If
        Return bReturn
    End Function
    Public Function getIpAddress() As String
        Dim ipadress As String = ""
        Try
            'No se cuenta con id de terminal, lo sacamos.
            Dim nombreHost As String = System.Net.Dns.GetHostName
            Dim hostInfo As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(nombreHost)
            ipadress = "127.0.0.1"

            Dim strComputer As String = My.Computer.Name
            ipadress = New List(Of IPAddress)(Dns.GetHostEntry(strComputer).AddressList).Find(Function(f) f.AddressFamily = Sockets.AddressFamily.InterNetwork).ToString

            If ipadress = "127.0.0.1" Or ipadress = "" Or ipadress = Nothing Then
                'No se pudo obtener la ip, seteamos el nombre de maquina
                ipadress = Read_Ini("Information", "IP_ADDRESS", Constants.strAgentWork).Replace(" ", "")
            End If
            'Actualizamos el ID del equipo en el archivo de configuracion.
            WritePrivateProfileString("Information", "IP_ADDRESS", ipadress, Constants.strAgentWork)
        Catch ex As Exception
            log.Error("Regresando el valor: " & ipadress, ex)
            ipadress = Read_Ini("Information", "IP_ADDRESS", Constants.strAgentWork).Replace(" ", "")
        End Try
        Return ipadress
    End Function

End Module
