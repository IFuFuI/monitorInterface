Imports System.IO
Imports System.Xml.Serialization

Module F_Configuration
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Public Function reportConfiguration(strPath As String, Id_Method As String) As Boolean



        Dim oPConf As New sstsWebService.P_C
        Dim objStreamReader As StreamReader
        Dim oSsstsWS As New sstsWebService.Service1
        Dim strUrl As String = Read_Ini("CONFIGURATION", "URL_WS", Constants.strPathAgentConfig)
        Try
            oSsstsWS.Url = strUrl
            For Each strFile As String In My.Computer.FileSystem.GetFiles(strPath, FileIO.SearchOption.SearchTopLevelOnly, "*.conf")
                log.Debug("-----------------------  Config  ------------------------------")
                If iStatusDeviceOnWS = WS_Resp_Codes.errDevIsNotActive Or iStatusDeviceOnWS = WS_Resp_Codes.errCanNoRetrieveCustomerLicense Or iStatusDeviceOnWS = WS_Resp_Codes.errInvalidLicense Then
                    eliminaArchivo(strFile)
                    log.Debug("No podemos hablar con el WS, estatus del dispositivo: " & iStatusDeviceOnWS.ToString)
                    Return False
                End If
                Try
                    'Deserializa
                    objStreamReader = New StreamReader(strFile)
                    Dim x As New XmlSerializer(oPConf.GetType)
                    oPConf = x.Deserialize(objStreamReader)
                    objStreamReader.Close()

                Catch ex As Exception
                    log.Error("", ex)
                    objStreamReader.Close()
                    moverArchivo(strFile, Constants.strPathNoProc)
                    Return True
                    Exit Function
                End Try
                Try
                    'Imprimimos datos
                    Call printOjectFields(oPConf)

                    Dim resp As Integer = oSsstsWS.ReportConfiguration(oPConf, customerId)
                    Dim b As Boolean = manageWSAnswerConfiguration(strFile, resp, Id_Method)
                Catch ex As Exception
                    If Id_Method = "0" Then moverArchivo(strFile, Constants.strPathTxnSAF)
                    log.Error("error al enviar al WS: ", ex)
                End Try
                log.Debug("-----------------------  End Config  ------------------------------")
            Next
        Catch ex As Exception
            log.Error("", ex)
        End Try
        Return True
    End Function
End Module
