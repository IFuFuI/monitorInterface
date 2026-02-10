Imports System.IO
Imports System.Xml.Serialization

Public Module F_Update
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Public Function reportUpdate(strPath As String, Id_Method As String) As Boolean


        Dim oPAct As New sstsWebService.P_U
        Dim objStreamReader As StreamReader
        Dim oSsstsWS As New sstsWebService.Service1
        Dim strUrl As String = Read_Ini("CONFIGURATION", "URL_WS", Constants.strPathAgentConfig)
        Dim resp As Integer = WS_Resp_Codes.errUnknown
        Try


            oSsstsWS.Url = strUrl
            For Each strFile As String In My.Computer.FileSystem.GetFiles(strPath, FileIO.SearchOption.SearchTopLevelOnly, "*upd")
                log.Debug("-----------------------  Update  ------------------------------")
                If iStatusDeviceOnWS <> WS_Resp_Codes.OK Then
                    log.Debug("No podemos hablar con el WS, estatus del dispositivo: " & iStatusDeviceOnWS.ToString)
                    If Id_Method = "0" Then moverArchivo(strFile, Constants.strPathTxnSAF)
                    Return False
                End If

                Try
                    log.Info("File Message: " & strFile)

                    'Imprimimos datos
                    Call printOjectFields(oPAct)

                    'Deserializa
                    objStreamReader = New StreamReader(strFile)
                    Dim x As New XmlSerializer(oPAct.GetType)
                    oPAct = x.Deserialize(objStreamReader)
                    objStreamReader.Close()
                Catch ex As Exception
                    log.Error("", ex)
                    objStreamReader.Close()
                    moverArchivo(strFile, Constants.strPathNoProc)
                    Return True
                End Try
                Try
                    resp = oSsstsWS.ReportUpdate(oPAct, customerId)
                Catch ex As Exception
                    resp = WS_Resp_Codes.errUnknown
                    log.Error("", ex)
                End Try
                Dim b As Boolean = manageWSAnswer(strFile, resp, Id_Method)
                log.Debug("----------------------- End Update  ------------------------------")
            Next
        Catch ex As Exception
            log.Error("", ex)
            Return False
        End Try
        Return True
    End Function
End Module

