Imports System.IO
Imports System.Xml.Serialization
Imports Microsoft.Win32
Imports System.Timers
Imports System.ServiceProcess
Imports System.Net.Security
Imports System.Net

Public Class Service1
    Private WithEvents detectUsb As Detect_USB
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Private timerKAs As Timer = Nothing
    Private timerSF As Timer = Nothing
    Private timerCM As Timer = Nothing
    Private timer_Get_Comm As Timer = Nothing
    Private timerCamDIa As Timer = Nothing
    Private timerDevStatus As Timer = Nothing
    Private timer_Get_Pending_Jobs As Timer = Nothing
    Private timer_ExecuteUpdater As Timer = Nothing


    Private resp As Boolean
    Private strFecha As String = ""
    Private oPerformWindows As Performance_Windwos
    Public Function AcceptAllCertifications(ByVal sender As Object, ByVal certification As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal sslPolicyErrors As System.Net.Security.SslPolicyErrors) As Boolean
        Return True
    End Function
    Protected Overrides Sub OnStart(ByVal args() As String)
        ' Add code here to start your service. This method should set things
        ' in motion so your service can do its work.
        log4net.Config.XmlConfigurator.Configure()
        ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf AcceptAllCertifications)
        Call inicia()
        detectUsb = New Detect_USB
        oPerformWindows = New Performance_Windwos
    End Sub

    Protected Overrides Sub OnStop()
        log.Info("mvAgent Stop")
        Try
            detectUsb.stopMonitoring()
        Catch ex As Exception
            log.Warn("error trying to stop Detect_USB: " & ex.Message)
        End Try
        ' Add code here to perform any tear-down necessary to stop your service.
    End Sub
    Protected Overrides Sub OnSessionChange(changeDescription As SessionChangeDescription)
        log.Debug("OnSessionChange: " & changeDescription.ToString)
    End Sub

    Protected Overrides Sub OnShutdown()
        log.Info("***** Shut down")


        'trace("termine")
        ''Dim base As New ServiceBase
        ''base.RequestAdditionalTime(20000)
        ''base.CanShutdown = False

        'Me.RequestAdditionalTime(15000)
        ''Me.OnShutdown()
        'Me.CanShutdown = False

        Dim oEstatus As New sstsWebService.P_S
        oEstatus.ldt = Format(Now(), "yyyy-MM-dd HH:mm:ss")
        oEstatus.idSt = "1"
        oEstatus.di = oAPI.GetDeviceID
        log.Info("sending Shutdown")
        resp = reportSWStatus(oEstatus)



        log.Info("***** Finish shutdown")

        Try
            'detectUsb.stopMonitoring()
        Catch ex As Exception
            log.Error("", ex)
        End Try
        'Stop



        ' Add code here to perform any tear-down necessary to stop your service.
    End Sub

    Private Sub inicia()
        Try

            log.Info("--- Start appServicesMonitor v: " & My.Application.Info.Version.ToString & "  ---")
            customerId = oAPI.getCustomerId
            If customerId = "-1" Then
                log.Error("Customer ID is not valid: " & customerId.ToString)
                Exit Sub
            Else
                log.Info("CustomerID: " & customerId.ToString)
            End If

            strFecha = Format(Now(), "ddMMyyyy")

            Dim ACTIVE_CONC As String = Read_Ini("CONFIGURATION", "ACTIVE_CONC", Constants.strPathAgentConfig)
            log.Info("ACTIVE_CONC: " & ACTIVE_CONC)

            If ACTIVE_CONC = "1" Then
                resp = sendTxns(Constants.strPathTxnConc, "2")
            Else
                log.Info("Conciliacion no esta activa")
            End If

            oHandleCOnfiguration.init()


            Dim strTimer As String
            strTimer = Read_Ini("CONFIGURATION", "timerKAs", Constants.strPathAgentConfig)
            log.Info("timerKAs: " & strTimer)
            timerKAs = New System.Timers.Timer
            AddHandler timerKAs.Elapsed, AddressOf timerKAs_Elapsed
            timerKAs.Interval = strTimer
            timerKAs.Enabled = True

            strTimer = Read_Ini("CONFIGURATION", "timerSF", Constants.strPathAgentConfig)
            log.Info("timerSF: " & strTimer)
            timerSF = New System.Timers.Timer
            AddHandler timerSF.Elapsed, AddressOf timerSF_Elapsed
            timerSF.Interval = strTimer
            timerSF.Enabled = True

            strTimer = Read_Ini("CONFIGURATION", "timerCM", Constants.strPathAgentConfig)
            log.Info("timerCM: " & strTimer)
            timerCM = New System.Timers.Timer
            AddHandler timerCM.Elapsed, AddressOf timerCM_Elapsed
            timerCM.Interval = strTimer
            timerCM.Enabled = True

            strTimer = Read_Ini("CONFIGURATION", "timer_Get_Comm", Constants.strPathAgentConfig)
            log.Info("timer_Get_Comm: " & strTimer)
            timer_Get_Comm = New System.Timers.Timer
            AddHandler timer_Get_Comm.Elapsed, AddressOf timer_Get_Comm_Elapsed
            timer_Get_Comm.Interval = strTimer
            timer_Get_Comm.Enabled = True

            strTimer = Read_Ini("CONFIGURATION", "timerCamDIa", Constants.strPathAgentConfig)
            log.Info("timerCamDIa: " & strTimer)
            timerCamDIa = New System.Timers.Timer
            AddHandler timerCamDIa.Elapsed, AddressOf timerCamDIa_Elapsed
            timerCamDIa.Interval = strTimer
            timerCamDIa.Enabled = True


            strTimer = Read_Ini("CONFIGURATION", "timerDevStatus", Constants.strPathAgentConfig)
            log.Info("timerDevStatus: " & strTimer)
            strTimer = CInt(strTimer) * 60000
            timerDevStatus = New System.Timers.Timer
            AddHandler timerDevStatus.Elapsed, AddressOf timerDevStatus_Elapsed
            timerDevStatus.Interval = strTimer
            timerDevStatus.Enabled = True


            strTimer = 30000
            timer_ExecuteUpdater = New System.Timers.Timer
            AddHandler timer_ExecuteUpdater.Elapsed, AddressOf timer_ExecuteUpdater_Elapsed
            timer_ExecuteUpdater.Interval = strTimer
            timer_ExecuteUpdater.Enabled = True



            'Validate if folder exists
            If My.Computer.FileSystem.DirectoryExists(Constants.strPathMessage) = False Then My.Computer.FileSystem.CreateDirectory(Constants.strPathMessage)
            If My.Computer.FileSystem.DirectoryExists(Constants.strPathTxnConc) = False Then My.Computer.FileSystem.CreateDirectory(Constants.strPathTxnConc)
            If My.Computer.FileSystem.DirectoryExists(Constants.strPathTxnSAF) = False Then My.Computer.FileSystem.CreateDirectory(Constants.strPathTxnSAF)
            If My.Computer.FileSystem.DirectoryExists(Constants.strPathNoProc) = False Then My.Computer.FileSystem.CreateDirectory(Constants.strPathNoProc)

            'Dim Timer_Check_Jobs_Status As String
            'Dim File_Mg_Max_Size As String
            'Dim Time_Out_Job_Minutes As String

            Call oHandleCOnfiguration.sendConfiguration()
            Call getDeviceStatusOnWS()
            Call deleteOldFiles()
            Call deleteOldJournalsBackups()

            'Close Shutdown Status (The server handle if it is necesary to close or not)
            oAPI.ReportSwStatus("-1")
        Catch ex As Exception
            log.Error("Error en inicia: ", ex)
        End Try

    End Sub

    Private Sub timerDevStatus_Elapsed(ByVal sender As System.Object, ByVal e As ElapsedEventArgs)
        If iStatusDeviceOnWS <> WS_Resp_Codes.OK Then
            Call getDeviceStatusOnWS()
        End If
    End Sub

    Private Sub timerCamDIa_Elapsed(ByVal sender As System.Object, ByVal e As ElapsedEventArgs)
        If strFecha <> Format(Now(), "ddMMyyyy") Then
            'Cambio de dia!!
            log.Info("********Cambio de día************")
            strFecha = Format(Now(), "ddMMyyyy")

            Call deleteOldFiles()
            Call deleteOldJournalsBackups()
            Call getDeviceStatusOnWS()

            Dim ACTIVE_CONC As String = Read_Ini("CONFIGURATION", "ACTIVE_CONC", Constants.strPathAgentConfig)
            log.Info("ACTIVE_CONC: " & ACTIVE_CONC)

            If ACTIVE_CONC = "1" Then
                resp = sendTxns(Constants.strPathTxnConc, "2")
            Else
                log.Info("Conciliacion no esta activa")
            End If
        End If
    End Sub
    Private Sub timer_Get_Comm_Elapsed(ByVal sender As System.Object, ByVal e As ElapsedEventArgs)
        'Obtiene comando si es que existen
        timer_Get_Comm.Enabled = False
        resp = getCommand()
        timer_Get_Comm.Enabled = True
    End Sub
    Private Sub timer_ExecuteUpdater_Elapsed(ByVal sender As System.Object, ByVal e As ElapsedEventArgs)
        'Se migro al sstUpdater


        'Obtiene comando si es que existen
        'Try
        '    timer_ExecuteUpdater.Enabled = False
        '    Dim strAppStatus As String = Read_Ini("APP_INFORMATION", "App_Status", Constants.strExec_Comando).Replace(" ", "")
        '    log.Info("strAppStatus: " & strAppStatus)
        '    Select Case strAppStatus
        '        Case "APP_NOT_IN_USE"
        '            log.Info("Executing: " & Constants.sstUpdaterExe)
        '            Dim b As Boolean = oAPI.executeSstUpdater()
        '            log.Info("Updater Result: " & b.ToString)
        '        Case Else
        '            log.Debug("Device is being used, can not execute updater.")
        '    End Select
        '    timer_ExecuteUpdater.Enabled = True
        'Catch ex As Exception
        '    log.Error("", ex)
        'End Try

    End Sub


    Private Sub timerKAs_Elapsed(ByVal sender As System.Object, ByVal e As ElapsedEventArgs)
        'Manda keep alive de app y keep alive de comms
        timerKAs.Enabled = False
        resp = reportKeepAlive()
        timerKAs.Enabled = True
    End Sub
    Private Sub timerSF_Elapsed(ByVal sender As System.Object, ByVal e As ElapsedEventArgs)
        'Envia todos los SAF
        'setLicencia()
        timerCM.Enabled = False
        timerSF.Enabled = False
        resp = checkMessages(Constants.strPathTxnSAF, "1")
        timerSF.Enabled = True
        timerCM.Enabled = True
    End Sub
    Private Sub timerCM_Elapsed(ByVal sender As System.Object, ByVal e As ElapsedEventArgs)
        'Envia comandos de la carpeta mensajes
        timerCM.Enabled = False
        resp = checkMessages(Constants.strPathMessage, "0")
        timerCM.Enabled = True
    End Sub

    Private Function checkMessages(strPath, strType) As Boolean
        Try
            resp = sendTxns(strPath, strType)
            resp = sendHw(strPath, strType)
            resp = reportUpdate(strPath, strType)
            resp = reportConfiguration(strPath, strType)
            resp = reportAlert(strPath, strType)
            'resp = completeComando(strPath, strType)
            resp = reportSWStatus(strPath, strType)
        Catch ex As Exception
            log.Error("Error en checkMessages strPath: " & strPath, ex)
        End Try
        Return True
    End Function


End Class
