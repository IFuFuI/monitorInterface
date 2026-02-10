Imports System.IO
Imports System.Xml.Serialization
Imports Microsoft.Win32

Module F_Transactions
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Private arrP_Txn() As sstsWebService.P_T

    Public Function sendTxns(strPath As String, Id_Method As String) As Boolean


        'trace("------- Transaccion -------")
        If cargaArregloTxn(strPath, Id_Method) Then
            log.Debug("-----------------------  Txn  ------------------------------")
            'Se cargo correctamente un archivo o mas en el arreglo.
            If iStatusDeviceOnWS <> WS_Resp_Codes.OK Then
                log.Debug("No podemos hablar con el WS, estatus del dispositivo: " & iStatusDeviceOnWS.ToString)
                procesaRespuestaWSExcep(arrP_Txn, Id_Method)
                Return False
            End If

            Try
                Dim oSsstsWS As New sstsWebService.Service1
                Dim arrResp() As sstsWebService.R
                Dim strUrl As String = Read_Ini("CONFIGURATION", "URL_WS", Constants.strPathAgentConfig)
                oSsstsWS.Url = strUrl
                arrResp = oSsstsWS.ReportTransaction(arrP_Txn, customerId)
                If arrResp.Length > 0 Then
                    procesaRespuestaWS(arrResp, Id_Method)
                End If
            Catch ex As Exception
                log.Error("error al procesar respuesta: " & ex.Message)
                procesaRespuestaWSExcep(arrP_Txn, Id_Method)
            End Try
            log.Debug("-----------------------  End Txn  ------------------------------")
        End If
        Return True

    End Function

    Private Function cargaArregloTxn(strPath As String, Id_Method As Integer) As Boolean

        '### Se guarda la informacion de los archivos en el arreglo de Transacciones arrP_Txn ###

        cargaArregloTxn = False
        Dim i As Integer = 0

        If My.Computer.FileSystem.DirectoryExists(strPath) = False Then My.Computer.FileSystem.CreateDirectory(strPath)
        For Each strFile As String In My.Computer.FileSystem.GetFiles(strPath, FileIO.SearchOption.SearchTopLevelOnly, "*.txn")
            'Cargamos la informacion en el arreglo
            'trace("Send method: " & Id_Method)
            Dim objStreamReader As StreamReader
            Dim oTxn As sstsWebService.P_T
            Try
                oTxn = New sstsWebService.P_T
                'trace("File Message: " & strFile)
                'Deserializa
                objStreamReader = New StreamReader(strFile)
                Dim x As New XmlSerializer(oTxn.GetType)
                oTxn = x.Deserialize(objStreamReader)
                objStreamReader.Close()
            Catch ex As Exception
                log.Error("error al deserealizar archivo:  " & strFile, ex)
                objStreamReader.Close()
                moverArchivo(strFile, Constants.strPathNoProc)
                cargaArregloTxn = False
                Exit Function
            End Try
            Try
                oTxn.im = Id_Method
                oTxn.fn = strFile


                'Imprimimos datos
                Call printOjectFields(oTxn)

                'Cargamos arreglo
                ReDim Preserve arrP_Txn(i)
                arrP_Txn(i) = oTxn
                i = i + 1
                '0 - Txn , 1 - SAF , 2 -  Conciliacion 
                If Id_Method = "0" Then copiarArchivo(strFile, Constants.strPathTxnConc)
                cargaArregloTxn = True
            Catch ex As Exception
                log.Error("error al llenar el arreglo: ", ex)
                cargaArregloTxn = False
                Exit Function
            End Try
        Next
    End Function

    Private Sub procesaRespuestaWS(arrResp() As sstsWebService.R, Id_Method As String)
        '### Procesa la respuesta cuando el arreglo de respuesta del server esta comnpleto ###
        For Each oResp As sstsWebService.R In arrResp
            Dim b As Boolean = manageWSAnswer(oResp.fn, oResp.iCode, Id_Method)
        Next
    End Sub

    Private Sub procesaRespuestaWSExcep(arrP_Txn() As sstsWebService.P_T, Id_Method As String)
        '### Cuando el arreglo del Server viene incorrecto, por ejemplo regreso Nothing ###
        Try
            If Id_Method = "0" Then
                ' Si es error en txn en linea
                For Each oTxn As sstsWebService.P_T In arrP_Txn
                    moverArchivo(oTxn.fn, Constants.strPathTxnSAF)
                Next
            End If
        Catch ex As Exception
            log.Error("", ex)
        End Try
    End Sub

End Module
