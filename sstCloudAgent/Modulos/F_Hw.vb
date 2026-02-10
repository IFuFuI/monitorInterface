Imports System.IO
Imports System.Xml.Serialization
Imports Microsoft.Win32

Module F_Hw
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Private arrP_Hw() As sstsWebService.P_Hw

    Public Function sendHw(strPath As String, Id_Method As String) As Boolean


        'trace("------- Hw -------")
        If cargaArregloTxn(strPath, Id_Method) Then
            'Se cargo correctamente un archivo o mas en el arreglo.
            log.Debug("-----------------------  Hw Status  ------------------------------")
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
                arrResp = oSsstsWS.ReportStatus_Hw(arrP_Hw, customerId)
                If arrResp.Length > 0 Then
                    procesaRespuestaWS(arrResp, Id_Method)
                End If
            Catch ex As Exception
                log.Error("error al procesar respuesta: ", ex)
                procesaRespuestaWSExcep(arrP_Hw, Id_Method)
            End Try
            log.Debug("-----------------------  End Hw Status  ------------------------------")
        End If

        Return True

    End Function

    Private Function cargaArregloTxn(strPath As String, Id_Method As Integer) As Boolean

        '### Se guarda la informacion de los archivos en el arreglo de Transacciones arrP_Txn ###

        cargaArregloTxn = False
        Dim i As Integer = 0

        If My.Computer.FileSystem.DirectoryExists(strPath) = False Then My.Computer.FileSystem.CreateDirectory(strPath)
        For Each strFile As String In My.Computer.FileSystem.GetFiles(strPath, FileIO.SearchOption.SearchTopLevelOnly, "*.hw")
            'Cargamos la informacion en el arreglo
            Dim objStreamReader As StreamReader
            Dim oHw As sstsWebService.P_Hw
            Try
                oHw = New sstsWebService.P_Hw
                log.Debug("File Message: " & strFile)
                'Deserializa
                objStreamReader = New StreamReader(strFile)
                Dim x As New XmlSerializer(oHw.GetType)
                oHw = x.Deserialize(objStreamReader)
                objStreamReader.Close()
            Catch ex As Exception
                log.Error("error: " & ex.Message & "al deserealizar archivo:  " & strFile)
                objStreamReader.Close()
                moverArchivo(strFile, Constants.strPathNoProc)
                cargaArregloTxn = False
                Exit Function
            End Try
            Try
                'oHw.Id_Method = Id_Method
                oHw.fn = strFile

                'Imprimimos datos
                Call printOjectFields(oHw)

                ReDim Preserve arrP_Hw(i)
                arrP_Hw(i) = oHw
                i = i + 1
                '0 - Txn , 1 - SAF , 2 -  Conciliacion 
                'If Id_Method = "0" Then copiarArchivo(strFile, Files_Paths.strPathTxnConc)
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

    Private Sub procesaRespuestaWSExcep(arrP_Hw() As sstsWebService.P_Hw, Id_Method As String)
        '### Cuando el arreglo del Server viene incorrecto, por ejemplo regreso Nothing ###
        Try
            If Id_Method = "0" Then
                ' Si es error en txn en linea
                For Each oHw As sstsWebService.P_Hw In arrP_Hw
                    moverArchivo(oHw.fn, Constants.strPathTxnSAF)
                Next
            End If
        Catch ex As Exception
            log.Error("error procesaRespuestaWSExcep: ", ex)
        End Try
    End Sub

End Module
