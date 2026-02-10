Imports System.IO
Imports System.Xml.Serialization


Public Class API_Interface
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Public Function ReportTransaction(attribuesArray As String(), Id_Status As String, Type_Id As String, Optional DateTime_Start As String = "", Optional DateTime_End As String = "") As Boolean
        Dim strExtension As String = ".txn"
        Dim o As New SSTS_WebService.P_T
        Dim deviceId As String

        log.Debug("ReportTransaction")

        Try
            If DateTime_Start = "" Then DateTime_Start = Format(Now(), "yyyy-MM-dd HH:mm:ss")
            If DateTime_End = "" Then DateTime_End = Format(Now(), "yyyy-MM-dd HH:mm:ss")
            deviceId = GetDeviceID()

            If isValueInteger(Id_Status) = False Then
                log.Error("Id_Status inválido: " & Id_Status)
                Return False
            End If
            If isValueInteger(Type_Id) = False Then
                log.Error("Type_Id inválido: " & Type_Id)
                Return False
            End If
            If isValueInteger(deviceId) = False Then
                log.Error("deviceId inválido: " & deviceId)
                Return False
            End If
            o.a = attribuesArray
            o.di = deviceId
            o.idS = Id_Status
            o.idT = Type_Id
            o.ldt_s = DateTime_Start
            o.ldt_e = DateTime_End

            'Serialize object to a text file.
            Dim strNameFile As String = Type_Id.PadLeft(3, "0") & "_" & Id_Status.PadLeft(2, "0") & "_" & Format(Now(), "ddMMyyHHmmss.fff")
            Dim x As New XmlSerializer(o.GetType)
            Dim objStreamWriter As New StreamWriter(Constants.strPathMessage & strNameFile & strExtension)
            x.Serialize(objStreamWriter, o)
            objStreamWriter.Close()
            Return True
        Catch ex As Exception
            log.Error("", ex)
            Return False
        End Try
    End Function



    Public Function ReportTransactionDetails(attribuesArray As String(), detailsArray As String(), Id_Status As String, Type_Id As String, Optional DateTime_Start As String = "", Optional DateTime_End As String = "") As Boolean
        Dim strExtension As String = ".txn"
        Dim o As New SSTS_WebService.P_T
        Dim deviceId As String

        Try
            If DateTime_Start = "" Then DateTime_Start = Format(Now(), "yyyy-MM-dd HH:mm:ss")
            If DateTime_End = "" Then DateTime_End = Format(Now(), "yyyy-MM-dd HH:mm:ss")
            deviceId = GetDeviceID()

            If isValueInteger(Id_Status) = False Then
                log.Error("Id_Status inválido: " & Id_Status)
                Return False
            End If
            If isValueInteger(Type_Id) = False Then
                log.Error("Type_Id inválido: " & Type_Id)
                Return False
            End If
            If isValueInteger(deviceId) = False Then
                log.Error("deviceId inválido: " & deviceId)
                Return False
            End If
            o.a = attribuesArray
            o.aD = detailsArray
            o.di = deviceId
            o.idS = Id_Status
            o.idT = Type_Id
            o.ldt_s = DateTime_Start
            o.ldt_e = DateTime_End

            'Serialize object to a text file.
            Dim strNameFile As String = Type_Id.PadLeft(3, "0") & "_" & Id_Status.PadLeft(2, "0") & "_" & Format(Now(), "ddMMyyHHmmss.fff")
            Dim x As New XmlSerializer(o.GetType)
            Dim objStreamWriter As New StreamWriter(Constants.strPathMessage & strNameFile & strExtension)
            x.Serialize(objStreamWriter, o)
            objStreamWriter.Close()
            Return True
        Catch ex As Exception
            log.Error("", ex)
            Return False
        End Try
    End Function

    Public Function ReportSwStatus(Id_Status As String, Optional DateTime As String = "", Optional description As String = "") As Boolean
        Dim strExtension As String = ".sw"
        Dim o As New SSTS_WebService.P_S
        Dim deviceId As String

        'Quité esta validación solamente de los estatus de SW porque en el server se generan estatus de desconectado, por lo que el cliente no se entera y se desincroniza
        'Hay que pensar una buena forma de mantener el cliente y el server sincronziados utilizando la validacion del ultimo estatus enviado.


        'Dim lastStatus As String = Read_Ini("SW", "Id_Status", Constants.strPathContolLastStatus).Replace(" ", "")
        'If lastStatus = Id_Status Then
        '    log.Debug("El estatus de SW recibido es igual al último estatus enviado: " & Id_Status.ToString)
        '    Return True
        'Else
        '    'Enviamos el estatus y actualizamos la variable en el archivo
        '    WritePrivateProfileString("SW", "Id_Status", Id_Status, Constants.strPathContolLastStatus)
        'End If

        Try
            If DateTime = "" Then DateTime = Format(Now(), "yyyy-MM-dd HH:mm:ss")
            deviceId = GetDeviceID()

            'If isValueInteger(Id_Status) = False Then
            '    log.Error("Id_Status inválido: " & Id_Status)
            '    Return False
            'End If
            'If isValueInteger(deviceId) = False Then
            '    log.Error("deviceId inválido: " & deviceId)
            '    Return False
            'End If
            o.di = deviceId
            o.d = description
            o.idSt = Id_Status
            o.ldt = DateTime

            'Serialize object to a text file.
            Dim strNameFile As String = "SwStatus"
            Dim x As New XmlSerializer(o.GetType)
            Dim objStreamWriter As New StreamWriter(Constants.strPathMessage & strNameFile & strExtension)
            x.Serialize(objStreamWriter, o)
            objStreamWriter.Close()
            Return True
        Catch ex As Exception
            log.Error("", ex)
            Return False
        End Try
    End Function

    Public Function ReportHwStatus(Hw_Device_Id As String, Error_Code As String, Optional Error_Code_Detail As String = "", Optional DateTime As String = "", Optional description As String = "") As Boolean
        Dim strExtension As String = ".hw"
        Dim o As New SSTS_WebService.P_Hw
        Dim deviceId As String

        Dim lastDeviceId As String = Read_Ini("HW", "lastDeviceId", Constants.strPathContolLastStatus).Replace(" ", "")
        Dim lastError_Code As String = Read_Ini("HW", "lastError_Code", Constants.strPathContolLastStatus).Replace(" ", "")
        Dim lastError_Code_Detail As String = Read_Ini("HW", "lastError_Code_Detail", Constants.strPathContolLastStatus).Replace(" ", "")

        'Validate Error code
        If isValueInteger(Error_Code_Detail) = False Then Error_Code_Detail = "0"


        If lastDeviceId = Hw_Device_Id And lastError_Code = Error_Code And lastError_Code_Detail = Error_Code_Detail Then
            log.Debug("El estatus de Hw recibido es igual al último estatus enviado")
            log.Debug("lastDeviceId: " & lastDeviceId)
            log.Debug("lastError_Code: " & lastError_Code)
            log.Debug("lastError_Code_Detail: " & lastError_Code_Detail)
            Return True
        Else
            'Enviamos el estatus y actualizamos la variable en el archivo
            WritePrivateProfileString("HW", "lastDeviceId", lastDeviceId, Constants.strPathContolLastStatus)
            WritePrivateProfileString("HW", "lastError_Code", lastError_Code, Constants.strPathContolLastStatus)
            WritePrivateProfileString("HW", "lastError_Code_Detail", lastError_Code_Detail, Constants.strPathContolLastStatus)
        End If

        Try
            If DateTime = "" Then DateTime = Format(Now(), "yyyy-MM-dd HH:mm:ss")
            deviceId = GetDeviceID()

            If isValueInteger(Hw_Device_Id) = False Then
                log.Error("Hw_Device_Id inválido: " & Hw_Device_Id)
                Return False
            End If
            If isValueInteger(Error_Code) = False Then
                log.Error("Error_Code inválido: " & Error_Code)
                Return False
            End If
            If isValueInteger(deviceId) = False Then
                log.Error("deviceId inválido: " & deviceId)
                Return False
            End If
            o.di = deviceId
            o.d = description
            o.ec = Error_Code
            o.ecd = Error_Code_Detail
            o.idHD = Hw_Device_Id
            o.ldt_s = DateTime


            'Serialize object to a text file.
            Dim strNameFile As String = Hw_Device_Id.PadLeft(2, "0") & "_" & Error_Code.PadLeft(3, "0") & "_" & Format(Now(), "ddMMyyHHmmss.fff")
            Dim x As New XmlSerializer(o.GetType)
            Dim objStreamWriter As New StreamWriter(Constants.strPathMessage & strNameFile & strExtension)
            x.Serialize(objStreamWriter, o)
            objStreamWriter.Close()
            Return True
        Catch ex As Exception
            log.Error("", ex)
            Return False
        End Try
    End Function

    Public Function ReportUpdate(Result As String, Version As String, Optional DateTime As String = "") As Boolean
        Dim strExtension As String = ".upd"
        Dim o As New SSTS_WebService.P_U
        Dim deviceId As String

        Try
            If DateTime = "" Then DateTime = Format(Now(), "yyyy-MM-dd HH:mm:ss")
            deviceId = GetDeviceID()

            If Version = "" Then
                log.Error("Version is Blank")
                Return False
            End If
            o.di = deviceId
            o.ldt = DateTime
            o.r = Result
            o.v = Version

            'Serialize object to a text file.
            Dim strNameFile As String = "Update" & Version.Replace(".", "")
            Dim x As New XmlSerializer(o.GetType)
            Dim objStreamWriter As New StreamWriter(Constants.strPathMessage & strNameFile & strExtension)
            x.Serialize(objStreamWriter, o)
            objStreamWriter.Close()
            Return True
        Catch ex As Exception
            log.Error("", ex)
            Return False
        End Try
    End Function

    Public Function ReportConfiguration(attribuesArray As String(), Optional DateTime As String = "") As Boolean
        Dim strExtension As String = ".conf"
        Dim o As New SSTS_WebService.P_C
        Dim deviceId As String
        Dim oWindowsIfo As New Windows_Info


        Try
            If DateTime = "" Then DateTime = Format(Now(), "yyyy-MM-dd HH:mm:ss")
            deviceId = GetDeviceID()

            o.a = attribuesArray
            o.di = deviceId
            o.adds = Read_Ini("Configuracion", "Address", Constants.strConfigIniFile)
            o.b = Read_Ini("Configuracion", "Branch_Name", Constants.strConfigIniFile)
            o.ip = getIpAddress()
            o.iv = Read_Ini("Version", "Version", Constants.strInstallationVersion)
            o.L_dt = DateTime
            o.li = Read_Ini("Configuracion", "Location_Id", Constants.strConfigIniFile)
            o.m = Read_Ini("Configuracion", "Device_Model", Constants.strConfigIniFile)
            o.n = Read_Ini("Configuracion", "Device_Name", Constants.strConfigIniFile)
            o.mac = LTrim(RTrim(o.n)) & "_" & LTrim(RTrim(getCPUId()))
            log.Debug("machineIdentifier: " & o.mac)
            o.vapp = Read_Ini("Version", "Version", Constants.strAppVersion)

            'Windows Info
            o.oWI = New SSTS_WebService.Info_Windows
            o.oWI.tm = oWindowsIfo.str_TotalPhysicalMemory
            o.oWI.l = oWindowsIfo.str_Idioma
            o.oWI.wn = oWindowsIfo.strNameMachine
            o.oWI.p = oWindowsIfo.strNameProcesador
            o.oWI.OSn = oWindowsIfo.strOS_FUllName
            o.oWI.r = oWindowsIfo.strScreenResolution
            o.tz = TimeZoneInfo.Local.Id

            'HD INFO
            Dim wsList(oWindowsIfo.HDInfo_List.Count - 1) As SSTS_WebService.HD_I
            Dim i As Integer = 0
            For Each oHDiskDriveInfo As HardDrivesInfo In oWindowsIfo.HDInfo_List
                Dim oWs_Hd As New SSTS_WebService.HD_I

                oWs_Hd.fs = oHDiskDriveInfo.strFreeSpace
                oWs_Hd.vn = oHDiskDriveInfo.strLabelVolume
                oWs_Hd.ts = oHDiskDriveInfo.strTotalSpace
                wsList(i) = oWs_Hd
                i = i + 1
            Next
            o.oWI.HDI = wsList



            'Serialize object to a text file.
            Dim strNameFile As String = "Configuration"
            Dim x As New XmlSerializer(o.GetType)
            Dim objStreamWriter As New StreamWriter(Constants.strPathMessage & strNameFile & strExtension)
            x.Serialize(objStreamWriter, o)
            objStreamWriter.Close()
            Return True
        Catch ex As Exception
            log.Error("", ex)
            Return False
        End Try
    End Function

    Public Function ReportCommandCompleted(idCommand As Integer) As Boolean
        Dim oSsstsWS As New SSTS_WebService.Service1
        Dim strUrl As String = Read_Ini("CONFIGURATION", "URL_WS", Constants.strPathIniAgentConfig)
        oSsstsWS.Url = strUrl
        'trace("-------     Complete Comando    -------")

        Try
            Dim deviceId As String = GetDeviceID()
            Dim customerId As String = getCustomerId()

            If customerId = "" Or deviceId = "" Then
                log.Error("Invalid device or customer Id")
            Else
                log.Info("reportCommandCompleted, deviceId: " & deviceId & ", idCommand: " & idCommand)
                Dim iresp As Integer = oSsstsWS.ReportCommandCompleted(deviceId, idCommand, Format(Now(), "yyyy-MM-dd HH:mm:ss"), customerId)
                If iresp = 0 Then
                    log.Debug("Command Reported OK")
                Else
                    log.Error("Error trying to execute 'ReportCommandCompleted': " & iresp.ToString)
                End If
            End If
        Catch ex As Exception
            log.Error("", ex)
        End Try

        'trace("---------------------------------------------------------------------")
        Return True
    End Function

    Public Function ReportAlert(Alert_Id As String, Optional DateTime As String = "", Optional description As String = "") As Boolean
        Dim strExtension As String = ".alt"
        Dim o As New SSTS_WebService.P_A
        Dim deviceId As String

        Try
            If DateTime = "" Then DateTime = Format(Now(), "yyyy-MM-dd HH:mm:ss")
            deviceId = GetDeviceID()

            If isValueInteger(Alert_Id) = False Then
                log.Error("Alert_Id inválido: " & Alert_Id)
                Return False
            End If
            If isValueInteger(deviceId) = False Then
                log.Error("deviceId inválido: " & deviceId)
                Return False
            End If

            o.di = deviceId
            o.d = description
            o.idT = Alert_Id
            o.ldt = DateTime

            'Serialize object to a text file.
            Dim strNameFile As String = o.idT.PadLeft(3, "0") & "_" & Format(Now(), "ddMMyyHHmmss.fff")
            Dim x As New XmlSerializer(o.GetType)
            Dim objStreamWriter As New StreamWriter(Constants.strPathMessage & strNameFile & strExtension)
            x.Serialize(objStreamWriter, o)
            objStreamWriter.Close()
            Return True
        Catch ex As Exception
            log.Error("", ex)
            Return False
        End Try
    End Function

    'Public Function getMacAddress() As String
    '    'Get Mac Address
    '    Try
    '        Dim macAdd As String
    '        Dim nics() As NetworkInterface = NetworkInterface.GetAllNetworkInterfaces
    '        log.Info("Number of Physical Interfaces: " & nics.Count)
    '        For Each n As NetworkInterface In nics
    '            log.Info("Physical Address: " & n.GetPhysicalAddress().ToString)
    '        Next
    '        macAdd = nics(0).GetPhysicalAddress.ToString
    '        log.Info("MacAddress: " & macAdd)
    '        Return macAdd
    '    Catch ex As Exception
    '        log.Error("", ex)
    '        Return ""
    '    End Try
    'End Function
    Private Function getCPUId() As String
        Dim cpu_ids As String = ""
        Try
            Dim computer As String = "."
            Dim wmi As Object = GetObject("winmgmts:" &
                "{impersonationLevel=impersonate}!\\" &
                computer & "\root\cimv2")
            Dim processors As Object = wmi.ExecQuery("Select * from " &
                "Win32_Processor")
            For Each cpu As Object In processors
                cpu_ids = cpu_ids & ", " & cpu.ProcessorId
            Next cpu
            If cpu_ids.Length > 0 Then cpu_ids =
                cpu_ids.Substring(2)

        Catch ex As Exception
            log.Error("", ex)
        End Try
        Return cpu_ids
    End Function

    Public Function GetDeviceID() As String
        Dim deviceId As String = Read_Ini("Configuracion", "Device_Name", Constants.strPathIniDeviConfig).Replace(" ", "")
        If deviceId <> "" And deviceId <> Nothing And deviceId <> "0" Then
            Return deviceId
        Else
            Return "-1"
        End If
    End Function
    ''' <summary>
    ''' Returns the Cusomer Id, If the device has not configured a valid Customer id it returns -1
    ''' </summary>
    Public Function getCustomerId() As String
        Try
            Dim i As Integer
            customerId = Read_Ini("Configuracion", "Customer_Id", Constants.strConfigIniFile).Replace(" ", "")
            If Integer.TryParse(customerId, i) Then
                If i > 0 Then
                    Return i.ToString
                Else
                    log.Error("Invalid Customer id: " & customerId)
                    Return "-1"
                End If
            Else
                log.Error("Invalid Customer id: " & customerId)
                Return "-1"
            End If
        Catch ex As Exception
            log.Error("", ex)
            Return "-1"
        End Try
    End Function

    Public Function getDeviceName() As String
        Try
            Return Read_Ini("Configuracion", "Device_Name", Constants.strConfigIniFile).Replace(" ", "")
        Catch ex As Exception
            log.Error("", ex)
            Return "-1"
        End Try
    End Function

    ''' <summary>
    ''' Indicate to sstcloud the application is not in used
    ''' </summary>
    Public Function setApplicationStatus_NotUsed() As String
        Try
            WritePrivateProfileString("APP_INFORMATION", "App_Status", "APP_NOT_IN_USE", Constants.strExec_Comando)
            Return "0"
        Catch ex As Exception
            log.Error("", ex)
            Return "-1"
        End Try
    End Function
    ''' <summary>
    ''' Indicate to sstcloud the application is not being used
    ''' </summary>
    Public Function setApplicationStatus_InUse() As String
        Try
            WritePrivateProfileString("APP_INFORMATION", "App_Status", "APP_IS_BEING_USED", Constants.strExec_Comando)
            Return "0"
        Catch ex As Exception
            log.Error("", ex)
            Return "-1"
        End Try

    End Function

    Public Function RestartTerminal() As Boolean
        Try
            Shell("Shutdown -r -f -t 0")
            '            System.Diagnostics.Process.Start("shutdown -t 0 -r -f")
            Return True
        Catch ex As Exception
            log.Error("", ex)
            Return False
        End Try

    End Function



    Public Enum HwDevicesDef
        CardReader = 1
        BillAcceptor = 2
        BillDispenser = 3
        Printer = 4
        EPP = 5
        CoinDispenser = 6
        BarCodeReader = 7
        CoinAcceptor = 8

    End Enum

    Public Sub New()
        log4net.Config.XmlConfigurator.Configure()
    End Sub
End Class
