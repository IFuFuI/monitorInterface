Imports System.Xml
Imports System.IO
Imports System.Timers
Imports System.Xml.Serialization

Public Class HandleConfiguration
    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Private timerEvitaDobleEvento As System.Timers.Timer = New System.Timers.Timer
    Private listCarpetasMonitorear As New List(Of String)
    Private bActualizandoConf As Boolean = False
    Private WithEvents oRegistryUtils As RegistryUtils.RegistryMonitor
    Public oDefConfigAttributes As New Def_Configuration_Attributes
    Public Sub init()
        Try
            Call loadConfigurationAttributes()
            'Monitoreamos el archivo de versiones
            Dim strDirectory As String = Directory.GetParent(Constants.strAppVersion).FullName
            If validaSiYaSeAgregoCarpeta(strDirectory) = False Then createEventFileWatcher(strDirectory)

            'Monitoreamos el archivo 'Device Configuration'
            strDirectory = Directory.GetParent(Constants.strConfigIniFile).FullName
            If validaSiYaSeAgregoCarpeta(strDirectory) = False Then createEventFileWatcher(strDirectory)


            If oDefConfigAttributes.listIniFiles.Count > 0 Then
                For Each oIni In oDefConfigAttributes.listIniFiles
                    For Each strIniFileException In oDefConfigAttributes.listIniFileExceptions
                        'Si el archivo que se modifico, esta dado de alta como excepcion, no se monitorea
                        If strIniFileException <> oIni.strFileName Then
                            strDirectory = Directory.GetParent(oIni.strFileName).FullName
                            If validaSiYaSeAgregoCarpeta(strDirectory) = False Then createEventFileWatcher(strDirectory)
                        End If
                    Next
                Next
            Else
                log.Info("Utilizando configuración básica, no se cuenta con atributos")
            End If

            log.Info("Manager Configuration - Carpetas a monitorear: ")
            For Each strCarpetaLista In listCarpetasMonitorear
                log.Info("* " & strCarpetaLista)
            Next

            AddHandler timerEvitaDobleEvento.Elapsed, AddressOf timerEvitaDobleEvento_Elapsed
            timerEvitaDobleEvento.Interval = 500
            timerEvitaDobleEvento.Enabled = False

            'Add registry Monitor
            log.Debug("Inicializando Registry Monitor, path: " & oDefConfigAttributes.registryPathToMonitor)
            oRegistryUtils = New RegistryUtils.RegistryMonitor(oDefConfigAttributes.registryPathToMonitor)
            oRegistryUtils.Start()
            log.Debug("init Handle COnfiguration Complete")
        Catch ex As Exception
            log.Error("", ex)
            'Return ex.Message
        End Try
    End Sub


    Private Sub loadConfigurationAttributes()
        Dim objStreamReader As StreamReader
        Try
            'Deserializa
            objStreamReader = New StreamReader(Constants.strDefConfigurationAttributes)
            Dim x As New XmlSerializer(oDefConfigAttributes.GetType)
            oDefConfigAttributes = x.Deserialize(objStreamReader)
            objStreamReader.Close()

            log.Info("-------------- Attributes Configuration Definition --------------")
            log.Info("strComments: " & oDefConfigAttributes.strComments)
            log.Info("strVersion: " & oDefConfigAttributes.strVersion)
            log.Info("registryPathToMonitor: " & oDefConfigAttributes.registryPathToMonitor)

            For Each oIni In oDefConfigAttributes.listIniFiles
                log.Info("Ini iAttributeId: " & oIni.iAttributeId.ToString)
                log.Debug("Ini strFieldName: " & oIni.strFieldName)
                log.Debug("Ini strFileName: " & oIni.strFileName)
                log.Debug("Ini strSection: " & oIni.strSection)
            Next

            For Each strIniFileException In oDefConfigAttributes.listIniFileExceptions
                log.Debug("strIniFileException: " & strIniFileException)
            Next

            For Each oReg In oDefConfigAttributes.listRegistry
                log.Info("Reg iAttributeId: " & oReg.iAttributeId.ToString)
                log.Debug("Reg strKey: " & oReg.strKey)
                log.Debug("Reg strPath: " & oReg.strPath)
            Next
            log.Info("-----------------------------------------------------------------")
        Catch ex As Exception
            log.Fatal("error al cargar la definición de atributos: " & ex.Message)
        End Try
    End Sub
    Private Sub timerEvitaDobleEvento_Elapsed(ByVal sender As System.Object, ByVal e As ElapsedEventArgs)
        'Evita que se procesen eventos de archivos duplicados
        timerEvitaDobleEvento.Enabled = False
        bActualizandoConf = False

    End Sub
    Private Sub createEventFileWatcher(strDirectory)
        Try
            log.Debug("J.l. R.C Fix System.ArgumentException: The directory name C:\sstApplication\config is invalid.")
            strDirectory = strDirectory & "\"
            Dim watcher As New FileSystemWatcher()
            watcher.Path = strDirectory
            watcher.IncludeSubdirectories = False
            watcher.NotifyFilter = (NotifyFilters.LastAccess Or NotifyFilters.LastWrite Or NotifyFilters.FileName Or NotifyFilters.DirectoryName)
            AddHandler watcher.Changed, AddressOf OnChanged
            watcher.EnableRaisingEvents = True
        Catch ex As Exception
            log.Error("", ex)
        End Try

    End Sub
    Private Sub OnChanged(source As Object, e As FileSystemEventArgs)
        If bActualizandoConf = False Then FileChanged(e.FullPath)
        timerEvitaDobleEvento.Enabled = True
        bActualizandoConf = True
    End Sub
    Private Function validaSiYaSeAgregoCarpeta(strCarpeta As String) As Boolean
        validaSiYaSeAgregoCarpeta = False
        For Each strCarpetaLista In listCarpetasMonitorear
            If strCarpetaLista = strCarpeta Then Return True
        Next
        listCarpetasMonitorear.Add(strCarpeta)
    End Function
    Public Sub FileChanged(file As String)

        Try
            Select Case file
                Case Constants.strAppVersion
                    log.Info("El archivo:  " & file & " ha cambiado, acutalizando configuracion")
                    appVersionChanged()
                    sendConfiguration()
                    Exit Sub
                Case Constants.strConfigIniFile
                    log.Info("El archivo:  " & file & " ha cambiado, acutalizando configuracion")
                    sendConfiguration()
            End Select
            If My.Computer.FileSystem.FileExists(Constants.strDefConfigurationAttributes) Then
                If oDefConfigAttributes.listIniFiles.Count > 0 Then
                    For Each oIni In oDefConfigAttributes.listIniFiles
                        If file = oIni.strFileName Then
                            log.Info("El archivo:  " & oIni.strFileName & " ha cambiado, acutalizando configuracion")
                            sendConfiguration()
                            Exit Sub
                        End If
                    Next
                End If

            End If
        Catch ex As Exception
            log.Error("", ex)
        End Try

    End Sub
    Private Sub appVersionChanged()
        Dim strVersion As String = Read_Ini("Version", "Version", Constants.strAppVersion)
        Dim strRes_Act As String = Read_Ini("Version", "Resultado_Act", Constants.strInstallationVersion)
        'Dim strIdTerminal As String = Read_Ini("Configuracion", "IdTerminal", strConfigIniFile).Replace(" ", "")
        Dim b As Boolean = oApi.ReportUpdate(strRes_Act, strVersion, "")
        If b = False Then
            log.Error("API ReportUpdate return false")
        End If
    End Sub
    Public Function sendConfiguration() As String
        '## Envia la configuracion a la base de datos, cuando un archivo de confiuguracion cambia.
        Dim array(0) As String

        'Este sleep lo puse por si se actualiza de forma remota y se modifican varios archivos, de tiempo antes de enviar la config
        System.Threading.Thread.Sleep(2000)
        Try
            If My.Computer.FileSystem.FileExists(Constants.strDefConfigurationAttributes) Then

                If oDefConfigAttributes.listIniFiles.Count > 0 Then
                    For Each oIni In oDefConfigAttributes.listIniFiles
                        If oIni.iAttributeId > array.Length() - 1 Then
                            ReDim Preserve array(oIni.iAttributeId)
                        End If
                        Dim strTemp As String = Read_Ini(oIni.strSection, oIni.strFieldName, oIni.strFileName)
                        If strTemp = "" Then strTemp = "-"
                        array(oIni.iAttributeId) = strTemp
                        log.Debug("a(" & oIni.iAttributeId & ")=" & strTemp)
                    Next
                End If

                If oDefConfigAttributes.listRegistry.Count > 0 Then
                    For Each oRegistry In oDefConfigAttributes.listRegistry
                        If oRegistry.iAttributeId > array.Length() - 1 Then
                            ReDim Preserve array(oRegistry.iAttributeId)
                        End If
                        Dim strTemp As String = My.Computer.Registry.GetValue(oRegistry.strPath, oRegistry.strKey, "")
                        If strTemp = "" Then strTemp = "-"
                        log.Debug("a(" & oRegistry.iAttributeId & ")=" & strTemp)
                        array(oRegistry.iAttributeId) = strTemp
                    Next
                End If
            End If
        Catch ex As Exception
            log.Error("", ex)
            'Return ex.Message
        End Try

        Dim b As Boolean = oApi.ReportConfiguration(array, "")
        If b = False Then
            log.Error("API ReportConfiguration return false")
        End If
        Return ""
    End Function

    Private Sub oRegistryUtils_Error(sender As Object, e As ErrorEventArgs) Handles oRegistryUtils.Error
        log.Error("oRegistryUtils_Error", e.GetException())
    End Sub

    Private Sub oRegistryUtils_RegChanged(sender As Object, e As EventArgs) Handles oRegistryUtils.RegChanged
        log.Info("Registry changed ")
        sendConfiguration()
    End Sub

    Public Sub New()

    End Sub
End Class
