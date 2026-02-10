Imports System.Management

Public Class Windows_Info
    'Windows INFO
    Public HDInfo_List As List(Of HardDrivesInfo)
    Public strOS_FUllName As String
    Public str_TotalPhysicalMemory As Double
    Public str_Idioma As String
    Public strNameMachine As String
    Public strScreenResolution As String
    Public strNameProcesador As String

    Public Sub New()
        ' ****************** J.L Al instanciar carga la informacion de Windows y de Discos duros***************
        '******************Solo hay que instanciar la clase Windows_Info para obtener toda la informacion de Windows ***************
        Dim HDInfo As New HardDrivesInfo
        HDInfo_List = HDInfo.getHardDrivesInfo

        strOS_FUllName = My.Computer.Info.OSFullName.ToString()
        str_TotalPhysicalMemory = CStr(Math.Round(My.Computer.Info.TotalPhysicalMemory / 1073741824, 2))
        str_Idioma = My.Computer.Info.InstalledUICulture.NativeName
        strNameMachine = My.Computer.Name
        strScreenResolution = My.Computer.Screen.Bounds.Size.Width & " x " & My.Computer.Screen.Bounds.Size.Height

        Try

            Dim searcher As New ManagementObjectSearcher("root\CIMV2", "SELECT * FROM Win32_Processor")

            For Each queryObj As ManagementObject In searcher.[Get]()
                strNameProcesador = queryObj("Name")
            Next
        Catch e As ManagementException
            Console.WriteLine("An error occurred while querying for WMI data: " & e.Message)
        End Try
    End Sub
End Class
