Imports System.IO
Imports System.Xml.Serialization
Imports System.Xml

Module F_Command
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Public Function getCommand() As Boolean
        If iStatusDeviceOnWS <> WS_Resp_Codes.OK Then
            log.Debug("No podemos hablar con el WS, estatus del dispositivo: " & iStatusDeviceOnWS.ToString)
            Return False
        End If

        Dim strDeviceId As String = oAPI.GetDeviceID
        Dim oSsstsWS As New sstsWebService.Service1
        Dim strUrl As String = Read_Ini("CONFIGURATION", "URL_WS", Constants.strPathAgentConfig)
        'trace("-------     GetComando    -------")
        Try
            Dim iResp As Integer
            oSsstsWS.Url = strUrl
            log.Debug("Device ID: " & strDeviceId)
            iResp = oSsstsWS.GetTerminalActiveCommandType(strDeviceId, customerId)

            Select Case iResp
                Case WS_Resp_Codes.noPendingCommands
                    'No hay comandos pendientes.
                    Return True
                Case Is > 0
                    'Comando pendiente de procesar
                    log.Info("Command received: " & iResp.ToString)
                    Select Case iResp
                        Case CommandTypes.ConfigurationUpdate
                            Dim oConfig As sstsWebService.P_C_Resp
                            Try

                                log.Info("-------------------------- Actualizando configuracion -----------------------------")
                                oConfig = oSsstsWS.GetDeviceConfiguration(strDeviceId, customerId)
                                WritePrivateProfileString("Configuracion", "Location_Id", oConfig.li, Constants.strConfigIniFile)
                                WritePrivateProfileString("Configuracion", "Address", oConfig.adds, Constants.strConfigIniFile)
                                WritePrivateProfileString("Configuracion", "Device_Name", oConfig.n, Constants.strConfigIniFile)
                                WritePrivateProfileString("Configuracion", "Device_Model", oConfig.m, Constants.strConfigIniFile)
                                WritePrivateProfileString("Configuracion", "Branch_Name", oConfig.b, Constants.strConfigIniFile)


                                Dim i As Integer = 0
                                For Each strArray As String In oConfig.a
                                    Dim oIni As IniFile
                                    oIni = oHandleCOnfiguration.oDefConfigAttributes.listIniFiles.Find(Function(o As IniFile) o.iAttributeId = i)
                                    If oIni IsNot Nothing Then
                                        log.Info("Actualizando configuracion de parametro")
                                        log.Info("strSection: " & oIni.strSection)
                                        log.Info("strFieldName: " & oIni.strFieldName)
                                        log.Info("strFileName: " & oIni.strFileName)
                                        log.Info("Value: " & oConfig.a(i))

                                        WritePrivateProfileString(oIni.strSection, oIni.strFieldName, oConfig.a(i), oIni.strFileName)
                                    Else
                                        'No se encontró en el ini, vamos a buscar en la lista de Registry
                                        Dim oRegistry As Registry
                                        oRegistry = oHandleCOnfiguration.oDefConfigAttributes.listRegistry.Find(Function(o As Registry) o.iAttributeId = i)
                                        If oRegistry IsNot Nothing Then
                                            Try
                                                Dim autoshell As Microsoft.Win32.RegistryKey
                                                log.Info("Actualizando configuracion de parametro")
                                                log.Info("strKey: " & oRegistry.strKey)
                                                log.Info("strPath: " & oRegistry.strPath)
                                                log.Info("Value: " & oConfig.a(i))
                                                If oRegistry.strPath.Contains("HKEY_CURRENT_USER\") Then
                                                    autoshell = My.Computer.Registry.CurrentUser.OpenSubKey(oRegistry.strPath.Replace("HKEY_CURRENT_USER\",""), True)
                                                    autoshell.SetValue(oRegistry.strKey, oConfig.a(i))
                                                    autoshell.Close()
                                                    log.Info("Attributo actualizado OK")
                                                End If

                                                If oRegistry.strPath.Contains("HKEY_LOCAL_MACHINE\") Then
                                                    autoshell = My.Computer.Registry.LocalMachine.OpenSubKey(oRegistry.strPath.Replace("HKEY_LOCAL_MACHINE\", ""), True)
                                                    autoshell.SetValue(oRegistry.strKey, oConfig.a(i))
                                                    autoshell.Close()
                                                    log.Info("Attributo actualizado OK")
                                                End If

           
                                            Catch ex As Exception
                                                log.Error("Trying to update Registry", ex)
                                            End Try

                                            '


                                        End If
                                    End If

                                    i = i + 1
                                Next

                                If My.Computer.FileSystem.FileExists(Constants.strDefConfigurationAttributes) Then

                                    If oHandleCOnfiguration.oDefConfigAttributes.listIniFiles.Count > 0 Then

                                    End If

                                    If oHandleCOnfiguration.oDefConfigAttributes.listRegistry.Count > 0 Then

                                    End If
                                End If
                                oAPI.ReportCommandCompleted(iResp)
                                log.Info("-------------------------- Actualizando configuracion Finish -----------------------------")
                            Catch ex As Exception
                                log.Error("Error al obtener configuración del equipo: ", ex)
                            End Try

                        Case CommandTypes.Reset
                            Dim strAppStatus As String = Read_Ini("APP_INFORMATION", "App_Status", Constants.strExec_Comando).Replace(" ", "")
                            log.Info("strAppStatus: " & strAppStatus)

                            oAPI.ReportCommandCompleted(iResp)
                            log.Info("Reset ATM, command received and APP is not in use")
                            Dim b As Boolean = oAPI.RestartTerminal()

                            Select Case strAppStatus
                                Case "APP_NOT_IN_USE"

                            End Select



                        Case CommandTypes.sinEfectivo
                            'Comando para caja inmaculada
                            log.Info("Se recibe comando para cambiar a sin efectivo")
                            log.Debug("Se recibe comando para cambiar a sin efectivo")
                            My.Computer.FileSystem.CopyFile("C:\Program Files\NCR APTRA\Advance NDC\MediaChange\sinEFE\PIC014.jpg", "C:\Program Files\NCR APTRA\Advance NDC\Media\PIC014.jpg", True)
                            My.Computer.FileSystem.CopyFile("C:\Program Files\NCR APTRA\Advance NDC\MediaChange\sinEFE\PIC100.jpg", "C:\Program Files\NCR APTRA\Advance NDC\Media\PIC100.jpg", True)
                            My.Computer.FileSystem.CopyFile("C:\Program Files\NCR APTRA\Advance NDC\MediaChange\sinEFE\PIC102.jpg", "C:\Program Files\NCR APTRA\Advance NDC\Media\PIC102.jpg", True)
                            My.Computer.FileSystem.CopyFile("C:\Program Files\NCR APTRA\Advance NDC\MediaChange\sinEFE\PIC103.jpg", "C:\Program Files\NCR APTRA\Advance NDC\Media\PIC103.jpg", True)
                            oAPI.ReportCommandCompleted(iResp)

                            Dim strAppStatus As String = Read_Ini("APP_INFORMATION", "App_Status", Constants.strExec_Comando).Replace(" ", "")
                            log.Info("strAppStatus: " & strAppStatus)
                            Select Case strAppStatus
                                Case "APP_NOT_IN_USE"
                                    log.Info("Reset ATM, command received and APP is not in use")
                                    Dim b As Boolean = oAPI.RestartTerminal()
                            End Select


                        Case CommandTypes.conEfectivo
                            'Comando para caja inmaculada
                            log.Info("Se recibe comando para cambiar a con efectivo")
                            log.Debug("Se recibe comando para cambiar a con efectivo")
                            My.Computer.FileSystem.CopyFile("C:\Program Files\NCR APTRA\Advance NDC\MediaChange\conEFE\PIC014.jpg", "C:\Program Files\NCR APTRA\Advance NDC\Media\PIC014.jpg", True)
                            My.Computer.FileSystem.CopyFile("C:\Program Files\NCR APTRA\Advance NDC\MediaChange\conEFE\PIC100.jpg", "C:\Program Files\NCR APTRA\Advance NDC\Media\PIC100.jpg", True)
                            My.Computer.FileSystem.CopyFile("C:\Program Files\NCR APTRA\Advance NDC\MediaChange\conEFE\PIC102.jpg", "C:\Program Files\NCR APTRA\Advance NDC\Media\PIC102.jpg", True)
                            My.Computer.FileSystem.CopyFile("C:\Program Files\NCR APTRA\Advance NDC\MediaChange\conEFE\PIC103.jpg", "C:\Program Files\NCR APTRA\Advance NDC\Media\PIC103.jpg", True)
                            oAPI.ReportCommandCompleted(iResp)

                            Dim strAppStatus As String = Read_Ini("APP_INFORMATION", "App_Status", Constants.strExec_Comando).Replace(" ", "")
                            log.Info("strAppStatus: " & strAppStatus)
                            Select Case strAppStatus
                                Case "APP_NOT_IN_USE"
                                    log.Info("Reset ATM, command received and APP is not in use")
                                    Dim b As Boolean = oAPI.RestartTerminal()
                            End Select

                        Case CommandTypes.ResetServices
                            'Se reincian los servicios del agente si existieron cambios en los archivos de configuracion 
                            log.Info("Reiniciando Servicios del Agente")
                            Process.Start("C:\sstCloudAgent\bin\resetserv.vbs")
                        Case Else
                            WritePrivateProfileString("Comando", "Id_Comando", iResp, Constants.strExec_Comando)
                    End Select

            End Select

        Catch ex As Exception
            log.Error("" & ex.Message)
        End Try
        Return True
    End Function
End Module

Public Class CommandTypes
    Public Const ConfigurationUpdate As Integer = 1
    Public Const Reset As Integer = 2
    Public Const sinEfectivo As Integer = 3
    Public Const conEfectivo As Integer = 4
    Public Const ResetServices As Integer = 5
End Class
