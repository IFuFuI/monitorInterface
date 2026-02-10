Imports System.Threading
Imports System.Timers

Public Class Performance_Windwos
    Private Shared cpuUsage As PerformanceCounter
    Private tmSetPerformance As System.Timers.Timer = Nothing
    Private tmSendPerformanceWS As System.Timers.Timer = Nothing



    Private Const _categoryName As String = "Processor"
    Private Const _counterName As String = "% Processor Time"
    Private Const _instanceName As String = "_Total"

    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private Sub sendPerformanceToWS()
        Try
            Dim strFile_Total_RAM As String = Read_Ini("Performance", "Total_RAM_Used", Constants.datPerformance)
            Dim strFile_NoConsultas_RAM As String = Read_Ini("Performance", "Number_Checks_RAM", Constants.datPerformance)

            Dim strFile_Total_CPU As String = Read_Ini("Performance", "Total_CPU_Used", Constants.datPerformance)
            Dim strFile_NoConsultas_CPU As String = Read_Ini("Performance", "Number_Checks_CPU", Constants.datPerformance)

            If strFile_Total_RAM <> "" And strFile_NoConsultas_RAM <> "" And strFile_Total_CPU <> "" And strFile_NoConsultas_CPU <> "" Then
                Dim Promedio_RAM As Double = Math.Round(CDbl(strFile_Total_RAM) / CDbl(strFile_NoConsultas_RAM), 2)
                Dim Promedio_CPU As Double = Math.Round(CDbl(strFile_Total_CPU) / CDbl(strFile_NoConsultas_CPU), 2)

                Dim TotalRamAvailable As Double = Math.Round(My.Computer.Info.TotalPhysicalMemory / 1073741824, 2)
                'En la variable total ram es la disponible, a continuacion se calcula la cantidad usada.
                Promedio_RAM = TotalRamAvailable - Promedio_RAM
                log.Info("*** Enviando Performance***")
                log.Info("Promedio_RAM: " & Promedio_RAM)
                log.Info("Promedio_CPU: " & Promedio_CPU)

                'Enviamos info al WS
                Dim oSsstsWS As New sstsWebService.Service1
                Dim oP_Performance As New sstsWebService.P_P
                oP_Performance.pau = Promedio_CPU
                oP_Performance.rau = Promedio_RAM
                oP_Performance.ldt = Format(Now(), "yyyy-MM-dd HH:mm:ss")
                oP_Performance.di = oApi.GetDeviceID

                Dim strUrl As String = Read_Ini("CONFIGURATION", "URL_WS", Constants.strPathAgentConfig)
                oSsstsWS.Url = strUrl
                Dim strRespuesta As String = oSsstsWS.ReportPerformance(oP_Performance, customerId)
                If strRespuesta = "" Then
                    'OK !! - Reseteamos contadores
                    WritePrivateProfileString("Performance", "Total_RAM_Used", "", Constants.datPerformance)
                    WritePrivateProfileString("Performance", "Number_Checks_RAM", "", Constants.datPerformance)
                    WritePrivateProfileString("Performance", "Total_CPU_Used", "", Constants.datPerformance)
                    WritePrivateProfileString("Performance", "Number_Checks_CPU", "", Constants.datPerformance)
                End If
            Else
                log.Error("Uno de los contadores de Performance estan en blanco, no se envia info al WS.")
                log.Info("strFile_Total_RAM: " & strFile_Total_RAM)
                log.Info("strFile_NoConsultas_RAM: " & strFile_NoConsultas_RAM)
                log.Info("strFile_Total_CPU: " & strFile_Total_CPU)
                log.Info("strFile_NoConsultas_CPU: " & strFile_NoConsultas_CPU)

            End If
        Catch ex As Exception
            log.Error("Error sendPerformanceToWS: ", ex)
        End Try
    End Sub

    Private Sub setPerformanceToFile()
        'Almacena el historial del performane RAM y CPI
        Try
            Dim dble_AvailablePhysicalMemory As Double = Math.Round(My.Computer.Info.AvailablePhysicalMemory / 1073741824, 2)
            Dim dbleCPU_Usage = Math.Round(cpuUsage.NextValue())

            Dim strFile_Total_RAM As String = Read_Ini("Performance", "Total_RAM_Used", Constants.datPerformance)
            Dim strFile_NoConsultas_RAM As String = Read_Ini("Performance", "Number_Checks_RAM", Constants.datPerformance)

            Dim strFile_Total_CPU As String = Read_Ini("Performance", "Total_CPU_Used", Constants.datPerformance)
            Dim strFile_NoConsultas_CPU As String = Read_Ini("Performance", "Number_Checks_CPU", Constants.datPerformance)

            If strFile_Total_RAM = "" Or strFile_Total_RAM = Nothing Then strFile_Total_RAM = 0
            If strFile_NoConsultas_RAM = "" Or strFile_NoConsultas_RAM = Nothing Then strFile_NoConsultas_RAM = 0
            If strFile_Total_CPU = "" Or strFile_Total_CPU = Nothing Then strFile_Total_CPU = 0
            If strFile_NoConsultas_CPU = "" Or strFile_NoConsultas_CPU = Nothing Then strFile_NoConsultas_CPU = 0

            WritePrivateProfileString("Performance", "Total_RAM_Used", (dble_AvailablePhysicalMemory + CDbl(strFile_Total_RAM)).ToString, Constants.datPerformance)
            WritePrivateProfileString("Performance", "Number_Checks_RAM", CInt(strFile_NoConsultas_RAM) + 1, Constants.datPerformance)

            WritePrivateProfileString("Performance", "Total_CPU_Used", (dbleCPU_Usage + CDbl(strFile_Total_CPU).ToString), Constants.datPerformance)
            WritePrivateProfileString("Performance", "Number_Checks_CPU", CInt(strFile_NoConsultas_CPU) + 1, Constants.datPerformance)


            Console.WriteLine("CPUUSAGE: " & dbleCPU_Usage & " %")
            Console.WriteLine("RAM AVAILABLE: " & dble_AvailablePhysicalMemory & " %")
            Console.Read()
        Catch ex As Exception
            log.Error("", ex)
        End Try

    End Sub

    Public Sub New()
        Try
            cpuUsage = New PerformanceCounter("Processor", "% Processor Time", "_Total")


            'tmSetPerformance
            Dim strTimer As String = Read_Ini("PERFORMANCE", "tm_get_Performance_Data", Constants.strPathAgentConfig)
            If strTimer = "" Or strTimer = Nothing Then strTimer = 60000
            log.Info("tm_get_Performance_Data timer: " & strTimer)
            tmSetPerformance = New System.Timers.Timer
            AddHandler tmSetPerformance.Elapsed, AddressOf tmSetPerformance_Elapsed
            tmSetPerformance.Interval = strTimer
            tmSetPerformance.Enabled = True

            'tmSendPerformanceWS
            strTimer = Read_Ini("PERFORMANCE", "tm_Send_Performance_Data", Constants.strPathAgentConfig)
            If strTimer = "" Or strTimer = Nothing Then strTimer = 60
            'Convert Minutes to Miliseconds
            strTimer = CInt(strTimer) * 60000
            log.Info("tm_get_Performance_Data timer: " & strTimer)
            tmSendPerformanceWS = New System.Timers.Timer
            AddHandler tmSendPerformanceWS.Elapsed, AddressOf tmSendPerformanceWS_Elapsed
            tmSendPerformanceWS.Interval = strTimer
            tmSendPerformanceWS.Enabled = True
        Catch ex As Exception
            log.Error("Error en Performance Windows New: ", ex)
        End Try

    End Sub

    Private Sub tmSetPerformance_Elapsed(ByVal sender As System.Object, ByVal e As ElapsedEventArgs)
        setPerformanceToFile()
    End Sub

    Private Sub tmSendPerformanceWS_Elapsed(ByVal sender As System.Object, ByVal e As ElapsedEventArgs)
        sendPerformanceToWS()
    End Sub
End Class
