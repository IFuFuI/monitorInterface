Imports System.IO
Imports System.Xml
Module F_Keep_Alive
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Public Function reportKeepAlive() As Boolean
        'Se encarga de enviar el keep alive de comunicaciones y validar si estan las app arriba
        'para mandar el keep alive de aplicacion

        log.Debug("-----------------------  Keep Alive  ------------------------------")

        If iStatusDeviceOnWS <> WS_Resp_Codes.OK Then
            log.Debug("No podemos hablar con el WS, estatus del dispositivo: " & iStatusDeviceOnWS.ToString)
            Return False
        End If

        keep_Alive()
        If isAppTrabada() Then
            log.Debug("One or more application processs are not running")
            oAPI.ReportSwStatus("3")
        Else
            log.Debug("Device application is ok")
            oAPI.ReportSwStatus("-3")
        End If
        log.Debug("-----------------------  End Keep Alive  ------------------------------")
        Return True
    End Function

    Private Function keep_Alive() As Boolean
        Dim oSsstsWS As New sstsWebService.Service1
        Dim resp As Integer
        Dim deviceId As String = oApi.GetDeviceID()
        Dim strUrl As String = Read_Ini("CONFIGURATION", "URL_WS", Constants.strPathAgentConfig)
        oSsstsWS.Url = strUrl
        Try
            resp = oSsstsWS.ReportKeepAlive(deviceId, customerId)
            If resp = WS_Resp_Codes.OK Then
                log.Debug("Keep Alive reported OK")
            Else
                log.Error("KA.Respuesta del WS: " & resp)
            End If
        Catch ex As Exception
            log.Error("Excepcion reporting KA", ex)
        End Try
        Return True
    End Function

    Public Function isAppTrabada() As Boolean
        Try
            Dim m_xmld As XmlDocument
            Dim m_nodelist As XmlNodeList
            Dim m_node As XmlNode
            Dim iTotalProcOpcionales As Integer = 0
            Dim iContTempProcOpcionalesAbajo As Integer = 0
            m_xmld = New XmlDocument()
            m_xmld.Load(Constants.strXML_Proc_App)
            m_nodelist = m_xmld.SelectNodes("/Procesos_Requeridos/proceso")
            'Obtiene del xml todos los procesos que tienen que estar ejecutandose
            For Each m_node In m_nodelist
                Dim strProceso As String = m_node.ChildNodes.Item(0).InnerText
                Dim TypeProcess As String = m_node.Attributes(0).InnerXml

                '1-Mandatorio, 2-Opcional
                Select Case TypeProcess
                    Case "1"
                        'Mandatorio
                        If isProcessAlive(strProceso) = False Then
                            log.Info("No se encontro Proceso mandatorio: " & strProceso)
                            Return True
                        End If
                    Case "2"
                        'Opcional
                        iTotalProcOpcionales = iTotalProcOpcionales + 1
                        If isProcessAlive(strProceso) = False Then
                            log.Debug("No se encontro Proceso opcional: " & strProceso)
                            iContTempProcOpcionalesAbajo = iContTempProcOpcionalesAbajo + 1
                        End If
                    Case Else
                        log.Error("se encontro un proceso con configuracion erronea en el parametro TypeProcess, proceso: " & strProceso)
                End Select
            Next
            If iTotalProcOpcionales > 0 Then
                If iTotalProcOpcionales = iContTempProcOpcionalesAbajo Then
                    'No se encontraron procesos mandatorios abajo, checando procesos opcionales.
                    log.Info("Todos los procesos opcionales estan abajo.")
                    log.Info("iTotalProcOpcionales: " & iTotalProcOpcionales)
                    log.Info("iContTempProcOpcionales: " & iContTempProcOpcionalesAbajo)
                    Return True
                Else
                    Return False
                End If
            Else
                log.Debug("not optional process configured")
                Return False
            End If


        Catch ex As Exception
            log.Error("", ex)
            Return False
        End Try
    End Function

    Private Function isProcessAlive(strProcess As String) As Boolean
        Dim p As Process
        Try
            isProcessAlive = False
            For Each p In Process.GetProcesses
                If UCase(p.ProcessName) = UCase(strProcess) Then
                    'Existe proceso
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
