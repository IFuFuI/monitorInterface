Imports System.IO
Imports System.Xml.Serialization
Imports Microsoft.Win32

Public Module F_Alerts
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Private arrP_Hw() As sstsWebService.P_A

    Public Function reportAlert(strPath As String, Id_Method As String) As Boolean
        Try
            If cargaArregloTxn(strPath, Id_Method) Then
                log.Debug("-----------------------  Alerta ------------------------------")
                'Se cargo correctamente un archivo o mas en el arreglo.
                If iStatusDeviceOnWS <> WS_Resp_Codes.OK Then
                    log.Debug("No podemos hablar con el WS, estatus del dispositivo: " & iStatusDeviceOnWS.ToString)
                    procesaRespuestaWSExcep(arrP_Hw, Id_Method)
                    Return False
                End If


                Try
                    Dim oSsstsWS As New sstsWebService.Service1
                    Dim arrResp() As sstsWebService.R
                    Dim strUrl As String = Read_Ini("CONFIGURATION", "URL_WS", Constants.strPathAgentConfig)
                    oSsstsWS.Url = strUrl
                    arrResp = oSsstsWS.ReportAlert(arrP_Hw, customerId)
                    If arrResp.Length > 0 Then
                        procesaRespuestaWS(arrResp, Id_Method)
                    End If
                Catch ex As Exception
                    log.Error("", ex)
                    procesaRespuestaWSExcep(arrP_Hw, Id_Method)
                End Try
                log.Debug("-----------------------  End Alerta ------------------------------")
            End If
        Catch ex As Exception
            log.Error("", ex)
        End Try
        Return True
    End Function

    Private Function cargaArregloTxn(strPath As String, Id_Method As Integer) As Boolean
        '### Se guarda la informacion de los archivos en el arreglo de Transacciones arrP_Txn ###

        cargaArregloTxn = False
        Dim i As Integer = 0

        If My.Computer.FileSystem.DirectoryExists(strPath) = False Then My.Computer.FileSystem.CreateDirectory(strPath)
        For Each strFile As String In My.Computer.FileSystem.GetFiles(strPath, FileIO.SearchOption.SearchTopLevelOnly, "*.alt")
            'Cargamos la informacion en el arreglo
            log.Debug("Send method: " & Id_Method)
            Dim objStreamReader As StreamReader
            Dim oHw As sstsWebService.P_A
            Try
                oHw = New sstsWebService.P_A
                log.Debug("File Message: " & strFile)
                'Deserializa
                objStreamReader = New StreamReader(strFile)
                Dim x As New XmlSerializer(oHw.GetType)
                oHw = x.Deserialize(objStreamReader)
                objStreamReader.Close()
            Catch ex As Exception
                log.Error("file: " & strFile, ex)
                moverArchivo(strFile, Constants.strPathNoProc)
                cargaArregloTxn = False
                objStreamReader.Close()
                Return False
            End Try
            Try
                oHw.fn = strFile
                'Imprimimos datos
                Call printOjectFields(oHw)

                'Cargamos arreglo
                ReDim Preserve arrP_Hw(i)
                arrP_Hw(i) = oHw
                i = i + 1
                cargaArregloTxn = True
            Catch ex As Exception
                log.Error("error al llenar el arreglo: ", ex)
                cargaArregloTxn = False
                Return False
            End Try
        Next
    End Function
    'Dim b As Boolean = manageWSAnswer(strFile, resp, Id_Method)

    Private Sub procesaRespuestaWS(arrResp() As sstsWebService.R, Id_Method As String)
        '### Procesa la respuesta cuando el arreglo de respuesta del server esta comnpleto ###
        For Each oResp As sstsWebService.R In arrResp
            Dim b As Boolean = manageWSAnswer(oResp.fn, oResp.iCode, Id_Method)
        Next
    End Sub

    Private Sub procesaRespuestaWSExcep(arrP_Hw() As sstsWebService.P_A, Id_Method As String)
        '### Cuando el arreglo del Server viene incorrecto, por ejemplo regreso Nothing ###
        Try
            If Id_Method = "0" Then
                ' Si es error en txn en linea
                For Each oHw As sstsWebService.P_A In arrP_Hw
                    moverArchivo(oHw.fn, Constants.strPathTxnSAF)
                Next
            End If
        Catch ex As Exception
            log.Error("", ex)
        End Try
    End Sub

End Module
