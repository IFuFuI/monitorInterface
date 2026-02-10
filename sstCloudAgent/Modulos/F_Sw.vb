Imports System.IO
Imports System.Xml.Serialization

Module F_Sw
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Public Function reportSWStatus(strPath As String, Id_Method As String) As Boolean

        Try
            For Each strFileEst As String In My.Computer.FileSystem.GetFiles(strPath, FileIO.SearchOption.SearchTopLevelOnly, "*.sw")

                log.Debug("-----------------------  SW Status  ------------------------------")

                If Id_Method = "0" Then
                    'Si se generó un estatus nuevo de SW, eliminamos todos los archivos de SW del SAF para que no se registren status de sw anteriores depues del ultimo.
                    For Each strSafFile As String In My.Computer.FileSystem.GetFiles(Constants.strPathTxnSAF, FileIO.SearchOption.SearchTopLevelOnly, "*.sw")
                        log.Info("Eliminando status de SW del SAF porque llegó un nuevo status: " & strSafFile)
                        eliminaArchivo(strSafFile)
                    Next

                End If
                If iStatusDeviceOnWS <> WS_Resp_Codes.OK Then
                    log.Debug("No podemos hablar con el WS, estatus del dispositivo: " & iStatusDeviceOnWS.ToString)
                    If Id_Method = "0" Then moverArchivo(strFileEst, Constants.strPathTxnSAF)
                    Return False
                End If

                Dim objStreamReader As StreamReader
                Dim oPEst As New sstsWebService.P_S
                Try
                    'Deserializa
                    objStreamReader = New StreamReader(strFileEst)
                    Dim x As New XmlSerializer(oPEst.GetType)
                    oPEst = x.Deserialize(objStreamReader)
                    objStreamReader.Close()
                Catch ex As Exception
                    log.Error("error: al deserealizar archivo:  " & strFileEst, ex)
                    objStreamReader.Close()
                    moverArchivo(strFileEst, Constants.strPathNoProc, "Estatus_" & Format(Now(), "ddMMyyyHHmmss"))
                    Return False
                End Try
                Try
                    Dim oSsstsWS As New sstsWebService.Service1
                    Dim strUrl As String = Read_Ini("CONFIGURATION", "URL_WS", Constants.strPathAgentConfig)
                    oSsstsWS.Url = strUrl
                    log.Info("Reporting Idstatus: " & oPEst.idSt)
                    Dim resp As Integer = oSsstsWS.ReportStatus_SW(oPEst, customerId)
                    Dim b As Boolean = manageWSAnswer(strFileEst, resp, Id_Method)
                Catch ex As Exception
                    log.Error("error al enviar evento al WS: ", ex)
                    Dim b As Boolean = manageWSAnswer(strFileEst, WS_Resp_Codes.errUnknown, Id_Method)
                End Try
                log.Debug("-----------------------  End Sw Status  ------------------------------")
            Next
        Catch ex As Exception
            log.Error("", ex)
        End Try
        Return True
    End Function

    Public Function reportSWStatus(oPEst As sstsWebService.P_S) As Boolean
        Try
            Try
                Dim oSsstsWS As New sstsWebService.Service1
                Dim strUrl As String = Read_Ini("CONFIGURATION", "URL_WS", Constants.strPathAgentConfig)

                'Imprimimos datos
                Call printOjectFields(oPEst)

                oSsstsWS.Url = strUrl
                Dim resp As Integer = oSsstsWS.ReportStatus_SW(oPEst, customerId)
                log.Info("reportSWStatus(oEST), resp: " & resp.ToString)
            Catch ex As Exception
                log.Error("error al enviar evento al WS: ", ex)
            End Try

        Catch ex As Exception
            log.Error("", ex)
        End Try
        Return True
    End Function

End Module
