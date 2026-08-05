Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Web.Services

Module ModProcessJournal
    Dim PC As New Palabras_Clave
    Dim strTextoUltimaTxn As String = Read_Ini("Data", "U_Texto_Txn_Repor", strIniJournal)
    Dim strId_Inicio_Txn As String = Read_Ini("Data", "ID_INICIO_TXN", strIniJournal)
    Dim strPalabaInicial_Txn As String = Read_Ini("Data", "strPalabaInicial_Txn", strIniJournal)
    Dim strExtractoTxn As String = ""

    Dim tarjeta As String
    Dim folio As String
    Dim sExtra As String
    Dim onTxn As Boolean = False
    Dim sMsgIn As String = String.Empty
    Dim bMsgIn As Boolean = False
    Dim skipContactlessTransactionContinuation As Boolean = False

    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)


    Public Function ProccessJournal(arr() As String, strNameFileModified As String, iUltimaLinea As Integer) As Boolean
        Try
            For Each strLine As String In arr
                'log.Debug("Linea a procesar para buscar transaccion: " & strLine)
                If strLine <> "" And strLine <> Nothing Then
                    obtenTransaccion(strLine, arr)
                Else
                    log.Debug("No se obtiene transaccion por null")
                End If
            Next
        Catch ex As Exception
            log.Error("error en ProcessJournal: " + ex.Message)
        End Try

        WritePrivateProfileString("Data", "U_Texto_Txn_Repor", arr.Length.ToString, strIniJournal)
        WritePrivateProfileString("Data", "U_No_Linea", (iUltimaLinea + arr.Length).ToString, strIniJournal)

        Return True

    End Function

    Public Function ProccessMerge(arr() As String, strNameFileModified As String, iUltimaLinea As Integer) As Boolean

        Try
            For Each strLine As String In arr
                If strLine <> "" And strLine <> Nothing Then
                    obtenerDataMerge(strLine, arr)
                Else
                End If
            Next
        Catch ex As Exception
            log.Error("error en ProccessMerge: " + ex.Message)
        End Try

        WritePrivateProfileString("Data", "U_Texto_Txn_Repor", arr.Length.ToString, strIniMerge)
        WritePrivateProfileString("Data", "U_No_Linea", (iUltimaLinea + arr.Length).ToString, strIniMerge)

        Return True

    End Function




    Private Function getFechaTxn(strExtracto As String) As String
        Try
            Return Format(Now(), "yyyy-MM-dd HH:mm:ss")
        Catch ex As Exception
            log.Error("error getFechaTxn: " & ex.Message)
            Return Format(Now(), "yyyy-MM-dd HH:mm:ss")
        End Try
    End Function

    Private Function convertFechaDiebold(strFecha) As String
        '040314185912
        Try
            Dim dia As String = strFecha.Substring(2, 2)
            Dim mes As String = strFecha.Substring(0, 2)
            Dim year As String = strFecha.Substring(4, 2)

            Dim hora As String = strFecha.Substring(6, 2)
            Dim minute As String = strFecha.Substring(8, 2)
            Dim sec As String = strFecha.Substring(10, 2)


            year = "20" & year

            Return year & "-" & mes & "-" & dia & " " & hora & ":" & minute & ":" & sec
        Catch ex As Exception
            log.Error("error convertFechaDiebold:  " & ex.Message)
            Return ""
        End Try

    End Function

    Private Sub obtenerDataMerge(strTexto As String, arr() As String)

        Dim bactive As Boolean = False

        Try

            'log.Debug("Procesando Merge linea: " + strTexto)

            If strTexto.Contains("MX2111") Then
                strTexto = strTexto.Replace("MX2111_", "").Replace("<DEBUG>Applica.. (MAIN)", "")
                TraceApplication(strTexto)
            End If

            If strTexto.Contains("<APPIN>") Then
                bMsgIn = True
            End If

            If strTexto.Contains("</APPIN>") Then
                CreaMsg(sMsgIn)
                bMsgIn = False
                sMsgIn = String.Empty
            End If

            If bMsgIn Then
                If strTexto.Contains("<APPIN>") Then
                    'No hace nada
                Else
                    sMsgIn = sMsgIn + strTexto + vbCrLf
                End If
            End If


            If strTexto.Contains("Screen Number           [") Then
                Dim spage As String = ObtenerEntreCorchetes(strTexto)
                log.Debug("Numero de pagina: " + spage)
                WritePrivateProfileString("SCREENS", "NUM", spage, file_Work)
            End If

            If strTexto.Contains("Screen #         [") Then
                Dim spage As String = ObtenerEntreCorchetes(strTexto)
                log.Debug("Numero de pagina por #: " + spage)
            End If



            If strTexto.Contains("Screen #         [500]") Then
                bactive = True
                flagback = False
                log.Debug("Welcome")
                If statusATM = False Then
                    statusATM = True
                    reportSwStatus("3", "", "")
                    WritePrivateProfileString("APP", "STATUSAPP", "", file_Work)
                End If
                WritePrivateProfileString("SCREENS", "NUM", "welcome", file_Work)
            End If



            If strTexto.Contains("Screen Number           [150]") Then
                bactive = True
                log.Debug("Entre NIP")
                WritePrivateProfileString("SCREENS", "NUM", "150", file_Work)
            End If

            If strTexto.Contains("WFS_CMD_CDM_RESET") Then
                writeJournal("*RESET APLICADO AL DISPENSADOR*")
                log.Debug("Aplica Reset al Dispensador")
            End If

            If strTexto.Contains("Please Wait Screen      [200]") Then
                bactive = True
                log.Debug("Wait Read Card")
                WritePrivateProfileString("SCREENS", "NUM", "200", file_Work)
            End If

            If strTexto.Contains("CUSTOMER CANCELLED") Then
                bactive = True
                log.Debug("Cancelado")
                WritePrivateProfileString("SCREENS", "NUM", "hide", file_Work)
            End If

            If strTexto.Contains("Key press for F6") Then

                Dim sKey As String = Read_Ini("APP", "TIMEOUT", file_Work)
                If sKey = "ON" Then
                    log.Debug("Regreso de pagina anterior")
                    WritePrivateProfileString("SCREENS", "NUM", "back", file_Work)
                End If

            End If

            If strTexto.Contains("WFS_EXEE_IDC_MEDIAINSERTED)") Then
                bactive = True
                log.Debug("WFS_EXEE_IDC_MEDIAINSERTED")
                WritePrivateProfileString("SCREENS", "NUM", "hide", file_Work)
                WritePrivateProfileString("APP", "STATUSAPP", "EN_USO", file_Work)
            End If

            If strTexto.Contains("[J] Close</STATEDATA>") Then
                log.Debug("Close State")
                WritePrivateProfileString("APP", "STATUSAPP", "", file_Work)
                WritePrivateProfileString("SCREENS", "NUM", "513", file_Work)
            End If

            If strTexto.Contains("SECONDARY CARD TRACK 2") Then
                log.Debug("ContacLess Lectura")
                WritePrivateProfileString("APP", "STATUSAPP", "EN_USO", file_Work)
                WritePrivateProfileString("SCREENS", "NUM", "hide", file_Work)
            End If

            If strTexto.Contains("Screen #                [810]") Then
                log.Debug("Tarjeta administrativa Ingresada")
                WritePrivateProfileString("APP", "STATUSAPP", "EN_USO", file_Work)
                WritePrivateProfileString("SCREENS", "NUM", "hide", file_Work)
            End If

            If strTexto.Contains("OFFLINE Detected") Then
                log.Debug("OFFLINE")
                WritePrivateProfileString("APP", "STATUSAPP", "", file_Work)
                statusATM = False
                reportSwStatus("4", "", "")
                WritePrivateProfileString("SCREENS", "NUM", "hide", file_Work)
            End If

            If strTexto.Contains("SUPERVISOR MODE REQUESTED") Then
                log.Debug("supervisor")
                WritePrivateProfileString("APP", "STATUSAPP", "", file_Work)
                WritePrivateProfileString("SCREENS", "NUM", "hide", file_Work)
            End If

            If strTexto.Contains("HW/User Error Data SrvcAddr") Then
                'log.Debug("Error Data")
                TraceError(strTexto)
            End If

            If strTexto.Contains("WFS_ERR") Or strTexto.Contains("HARDWARE_ERROR") Or strTexto.Contains("CASHUNITERROR") Then
                'log.Debug("Error Data")
                If strTexto.Contains("WFS_ERR_CANCELED") Then
                    'No hace nada
                Else
                    TraceError(strTexto)
                End If

            End If

            If strTexto.Contains("ILLEGAL_KEY_ACCESS") Or strTexto.Contains("PIN_ACCESSDENIED") Then
                TraceError(strTexto)
                reportHwStatus("", sPinPadDevices, sStatusError, "", "")
            End If

            If strTexto.Contains("HOST TX TIMEOUT") Then
                log.Debug("Time Out detectado")
                TraceError("Error Time Out")
                WritePrivateProfileString("SCREENS", "NUM", "hide", file_Work)
            End If


            If strTexto.Contains("<MESSAGEIN>") Then
                strTexto.Replace("<MESSAGEIN>", "").Replace("</MESSAGEIN>", "")
                strTexto.Replace("<MESSAGEOUT>", "").Replace("</MESSAGEOUT>", "")
                TraceComms(strTexto)
            End If


            If strTexto.Contains("<MESSAGEOUT>") Then
                strTexto.Replace("<MESSAGEOUT>", "").Replace("</MESSAGEOUT>", "")
                strTexto.Replace("<MESSAGEIN>", "").Replace("</MESSAGEIN>", "")
                TraceComms(strTexto, 1)
            End If

            If strTexto.Contains("(WFS_EXEE_PIN_KEY)") Then
                log.Debug("KEYPIN")
                WritePrivateProfileString("SCREENS", "DATA", "KEYPIN", file_Work)
            End If


        Catch ex As Exception

        End Try
    End Sub

    Function ObtenerEntreCorchetes(input As String) As String
        Dim inicio As Integer = input.IndexOf("[")
        Dim fin As Integer = input.IndexOf("]")

        If inicio >= 0 AndAlso fin > inicio Then
            Return input.Substring(inicio + 1, fin - inicio - 1).Trim()
        Else
            Return ""
        End If
    End Function


    Public Sub TraceComms(ByVal strTexto As String, Optional ByVal level As Integer = 0)
        Try
            Dim sTipo As String = ""

            If level = 0 Then sTipo = "IN"
            If level = 1 Then sTipo = "OUT"

            strTexto = strTexto.Replace(vbLf, "")
            'If bTrace Then
            strTexto = "[" & sTipo & "] " & strTexto & vbCrLf
            File.AppendAllText("C:\appMain\log\" & Format(Now(), "yyyyMMdd") & "_appComms.log", strTexto)
            'End If
        Catch ex As Exception
        End Try
    End Sub
    Public Sub CreaMsg(ByVal strTexto As String)
        Try
            'log.Debug("APPIN " + strTexto)
            File.AppendAllText("C:\appMain\msg\In_" & Format(Now(), "yyMMddhhmmss") & ".msg", strTexto)
            'End If
        Catch ex As Exception
        End Try
    End Sub
    Public Sub TraceApplication(ByVal strTexto As String, Optional ByVal level As Integer = 0)
        Try

            'If bTrace Then
            strTexto = Format(Now(), "dd-MM-yyyy HH:mm:ss.fff tt ") & "      " & strTexto & vbCrLf
            File.AppendAllText("C:\appMain\log\" & Format(Now(), "yyyyMMdd") & "application.log", strTexto)
            'End If
        Catch ex As Exception
        End Try
    End Sub
    Public Sub TraceError(ByVal strTexto As String, Optional ByVal level As Integer = 0)
        Try
            Dim sTipo As String = ""

            'If bTrace Then
            strTexto = Format(Now(), "dd-MM-yyyy HH:mm:ss.fff tt") & "      " & strTexto & vbCrLf
            File.AppendAllText("C:\appMain\log\" & Format(Now(), "yyyyMMdd") & "_appError.log", strTexto)
            'End If
        Catch ex As Exception
        End Try
    End Sub

    Function ExtraerDesde1(ByVal entrada As String) As String
        ' Buscar posición del campo *1*

        If entrada.Contains("*2*") Then entrada = entrada.Replace("*2*", "*1*")

        Dim indiceInicio As Integer = entrada.IndexOf("*1*")
        If indiceInicio = -1 Then Return "Campo *1* no encontrado"

        ' Extraer desde *1* hasta el final
        Dim subcadena As String = entrada.Substring(indiceInicio)

        ' Limpiar caracteres no deseados (conserva letras, números, comas, guiones y asteriscos)
        Dim regex As New System.Text.RegularExpressions.Regex("[^A-Za-z0-9,*\-]")
        Dim limpio As String = regex.Replace(subcadena, "")

        Return limpio
    End Function


    Private Sub obtenTransaccion(strTexto As String, arr() As String)
        Dim bwrite As Boolean
        Dim sLineJournal As String = String.Empty

        bwrite = True

        ' Este bloque no se escribe en nuestro journal porque contactless no se utiliza:
        '   *INITIALIZED CONTACTLESS
        '   TRANSACTION*
        ' El archivo de origen se conserva sin cambios.
        If strTexto.IndexOf("*INITIALIZED CONTACTLESS", StringComparison.OrdinalIgnoreCase) >= 0 Then
            bwrite = False
            skipContactlessTransactionContinuation = True
        ElseIf skipContactlessTransactionContinuation AndAlso
               String.Equals(strTexto.Trim(), "TRANSACTION*", StringComparison.OrdinalIgnoreCase) Then
            bwrite = False
            skipContactlessTransactionContinuation = False
        Else
            skipContactlessTransactionContinuation = False
        End If

        If strTexto.Contains("DEBUG NO INICIALIZADO") Then
            log.Debug("DEBUG NO INICIALIZADO Aplica Reinicio")

            Try
                Dim psi As New ProcessStartInfo()
                psi.FileName = "wscript.exe"
                psi.Arguments = """" & "C:\appMain\PC RESET Local.bat" & """"
                psi.WindowStyle = ProcessWindowStyle.Hidden
                psi.CreateNoWindow = True
                psi.UseShellExecute = False

                Process.Start(psi)
            Catch ex As Exception
                log.Debug("Error al ejecutar reinicio ATM: " + ex.Message)
            End Try

        End If

        Try

            Try
                sLineJournal = strTexto

                Dim denom1 As String = Read_Ini("CDM", "DENOM_C1", file_ConfiAtm).Trim()
                Dim denom2 As String = Read_Ini("CDM", "DENOM_C2", file_ConfiAtm).Trim()
                Dim denom3 As String = Read_Ini("CDM", "DENOM_C3", file_ConfiAtm).Trim()
                Dim denom4 As String = Read_Ini("CDM", "DENOM_C4", file_ConfiAtm).Trim()

                If sLineJournal.Contains("DENOMINATION") Then
                    ' Evaluamos si tiene la configuración de doble casetera de 500
                    If denom1 = "100" AndAlso denom2 = "200" AndAlso denom3 = "500" AndAlso denom4 = "500" Then
                        sLineJournal = "DENOMINATION          100   200   500   500"
                    End If
                    ' (Si es 50, 100, 200, 500 no entra al If y la línea pasa normalita)
                End If

                If sLineJournal.Contains("EMV LEVEL 2") Then bwrite = False
                If sLineJournal.Contains("INT 04.") Then bwrite = False
                If sLineJournal.Contains("CAM 04.00") Then bwrite = False
                If sLineJournal.Contains("GENAC 1 :") Then bwrite = False
                If sLineJournal.Contains("GENAC 2 :") Then bwrite = False
                If sLineJournal.Contains("ATR RECEIVED T") Then bwrite = False
                If sLineJournal.Contains("EXTERNAL AUTHENTICATE") Then bwrite = False
                If sLineJournal.Contains("COMPONENT VERSIONS") Then bwrite = False
                If sLineJournal.Contains("CURRENT CONFIG CHECKSUM") Then bwrite = False
                If sLineJournal.Contains("*R*") Then bwrite = False
                If sLineJournal.Contains("APTRA ADVANCE") Then
                    writeJournal("DSC APPLICATION: " + Read_Ini("APPLICATION", "VERSION", file_ConfiAtm))
                    bwrite = False
                End If

                If sLineJournal.Contains("REINICIAR EL ATM") Then
                    If Read_Ini("PARAM", "AUTORESET_MSG", file_ConfiAtm) = "TRUE" Then
                        writeJournal("REINICIO POR MENSAJE DEL HOST")
                        log.Debug("REINICIAR EL ATM Aplica Reinicio")
                        Try
                            Dim psi As New ProcessStartInfo()
                            psi.FileName = "wscript.exe"
                            psi.Arguments = """" & "C:\appMain\PC RESET Local.bat" & """"
                            psi.WindowStyle = ProcessWindowStyle.Hidden
                            psi.CreateNoWindow = True
                            psi.UseShellExecute = False
                            Process.Start(psi)
                        Catch ex As Exception
                            log.Debug("Error al ejecutar reinicio ATM: " + ex.Message)
                        End Try
                    End If
                End If

                If sLineJournal.Contains("LECTORA ACTIVADA") Then
                    log.Debug("LECTORA ACTIVADA")
                    'WritePrivateProfileString("SCREENS", "NUM", "500", file_Work)
                End If

                If sLineJournal.Contains("ICC 04.0") Then bwrite = False
                If sLineJournal.Contains("EMV KERNEL") Then bwrite = False
                If sLineJournal.Contains("EB0C96302BF") Then bwrite = False
                If sLineJournal.Contains("71181857DA5A1") Then bwrite = False
                If sLineJournal.Contains("4FDD34AF479") Then bwrite = False
                If sLineJournal.Contains("X[0rX(1X)2X[000pX") Then bwrite = False
                If sLineJournal.Contains("[0r(1)2[000p") Then bwrite = False

                ' Cambiamos la 'X' o 'x' de vuelta al caracter ESC (ASCII 27)
                If sLineJournal.Contains("X[") Then sLineJournal = sLineJournal.Replace("X[", Chr(27) & "[")
                If sLineJournal.Contains("x[") Then sLineJournal = sLineJournal.Replace("x[", Chr(27) & "[")
                If sLineJournal.Contains("X(") Then sLineJournal = sLineJournal.Replace("X(", Chr(27) & "(")
                If sLineJournal.Contains("x(") Then sLineJournal = sLineJournal.Replace("x(", Chr(27) & "(")

                If sLineJournal.Contains("X)") Then sLineJournal = sLineJournal.Replace("X)", Chr(27) & "]")
                If sLineJournal.Contains("x)") Then sLineJournal = sLineJournal.Replace("x)", Chr(27) & "]")

                ' Por si alguna secuencia llega con un espacio en lugar de X
                If sLineJournal.Contains(" [") Then sLineJournal = sLineJournal.Replace(" [", Chr(27) & "[")

                ' --- 3. TRADUCCIÓN DE FECHA ---
                sLineJournal = sLineJournal.Replace("FECHA", "DATE")
                sLineJournal = sLineJournal.Replace("Fecha", "Date")

                ' --- 4. ALINEACIÓN FORZADA Y EXACTA PARA EL TICKET DE CONTADORES ---
                Try
                    ' 1. Alinear los prefijos a 13 posiciones exactas.
                    If Not sLineJournal.Contains("CASSETTE INFORMATION") Then
                        sLineJournal = Regex.Replace(sLineJournal, "^\s*CASSETTE\s+", " CASSETTE    ")
                    End If
                    sLineJournal = Regex.Replace(sLineJournal, "^\s*\+REJECTED\s+", "+REJECTED    ")
                    sLineJournal = Regex.Replace(sLineJournal, "^\s*=REMAINING\s+", "=REMAINING   ")
                    sLineJournal = Regex.Replace(sLineJournal, "^\s*\+DISPENSED\s+", "+DISPENSED   ")
                    sLineJournal = Regex.Replace(sLineJournal, "^\s*=TOTAL\s+", "=TOTAL       ")

                    ' 2. Alinear TYPE 1 y TYPE 2:
                    If sLineJournal.Contains("TYPE 1") AndAlso sLineJournal.Contains("TYPE 2") Then
                        If sLineJournal.Contains("p") Then
                            ' ¡CORREGIDO! Restamos 10 espacios para compensar el comando (ESC[020tESC[05p)
                            ' Solo dejamos 3 espacios para que visualmente se alinee en el TXT.
                            sLineJournal = Regex.Replace(sLineJournal, "p\s*TYPE 1\s*TYPE 2", "p   TYPE 1     TYPE 2")
                        Else
                            ' Si no trae comando (ticket del Config), sí lleva sus 13 espacios normales
                            sLineJournal = Regex.Replace(sLineJournal, "^\s*TYPE 1\s*TYPE 2", "             TYPE 1     TYPE 2")
                        End If
                    End If

                    ' 3. Alinear TYPE 3 y TYPE 4
                    If sLineJournal.Contains("TYPE 3") AndAlso sLineJournal.Contains("TYPE 4") Then
                        sLineJournal = Regex.Replace(sLineJournal, "^\s*TYPE 3\s*TYPE 4", "             TYPE 3     TYPE 4")
                    End If

                    ' 4. LAST CLEARED (con un espacio para que cuadre con CASSETTE)
                    If sLineJournal.Contains("LAST CLEARED") Then
                        sLineJournal = Regex.Replace(sLineJournal, "^\s*LAST CLEARED\s+", " LAST CLEARED  ")
                    End If
                    If sLineJournal.Contains("BORRADOS") Then
                        sLineJournal = Regex.Replace(sLineJournal, "^\s*BORRADOS\s+", " LAST CLEARED  ")
                    End If
                Catch ex As Exception
                End Try
                ' -----------------------------------------------------------

                ' --- 5. RELLENAR CON CEROS LOS CONTADORES DE CASSETTES ---
                ' Convierte "CASS 1 =   100" a "CASS 1 = 00100"
                If sLineJournal.Contains("CASS ") AndAlso sLineJournal.Contains("=") Then
                    For c As Integer = 1 To 8
                        Dim pref As String = "CASS " & c & " ="
                        ' Reemplazamos los espacios sobrantes por la cantidad exacta de ceros
                        sLineJournal = sLineJournal.Replace(pref & "     ", pref & " 0000")
                        sLineJournal = sLineJournal.Replace(pref & "    ", pref & " 000")
                        sLineJournal = sLineJournal.Replace(pref & "   ", pref & " 00")
                        sLineJournal = sLineJournal.Replace(pref & "  ", pref & " 0")
                    Next
                End If
                ' -----------------------------------------------------------

                sLineJournal = sLineJournal + vbCrLf
            Catch ex As Exception

            End Try

            Try
                If bwrite Then

                    ' --------------- INICIO DEL FIX ----------------
                    ' Interceptamos la linea para quitar la fecha de los CASSETTES
                    If sLineJournal.ToUpper().Contains("CASSETTE") Then
                        If IsCounterCassetteLine(sLineJournal) Then
                            ' Retiramos el timestamp crudo de esta línea
                            sLineJournal = StripJournalTimestamp(sLineJournal)
                        End If
                    End If
                    ' --------------- FIN DEL FIX -------------------

                    File.AppendAllText(sDSCJournal, sLineJournal)

                    If onTxn Then
                        sExtra = sExtra + strTexto + vbCrLf
                    End If

                End If
            Catch ex As Exception

            End Try

            'Revisa si es una linea de Error
            Try
                Dim sMensajeError As String

                If strTexto.Contains("*1*") Or strTexto.Contains("*2*") Then

                    If strTexto.Contains("*G") Then
                        sMensajeError = ExtraerDesde1(strTexto)
                        TraceError(sMensajeError)
                        reportHwStatus("", sImpresoraDevices, "M", sMensajeError, "")
                    End If

                    If strTexto.Contains("*E") Then
                        sMensajeError = ExtraerDesde1(strTexto)
                        TraceError(sMensajeError)
                        reportHwStatus("", sDispensadorDevices, "M", sMensajeError, "")
                    End If

                    If strTexto.Contains("*D") Then
                        sMensajeError = ExtraerDesde1(strTexto)
                        TraceError(sMensajeError)
                        reportHwStatus("", sLectoraDevices, "M", sMensajeError, "")
                    End If

                    'Sensores
                    If strTexto.Contains("*P") Then
                        sMensajeError = ExtraerDesde1(strTexto)
                        TraceError(sMensajeError)
                        reportHwStatus("", "13", "M", sMensajeError, "")
                    End If

                    If strTexto.Contains("*Y") Then
                        sMensajeError = ExtraerDesde1(strTexto)
                        TraceError(sMensajeError)
                        reportHwStatus("", sDispensadorMonedasDevices, "M", sMensajeError, "")
                    End If

                    If strTexto.Contains("*f") Then
                        sMensajeError = ExtraerDesde1(strTexto)
                        TraceError(sMensajeError)
                        reportHwStatus("", sBarCodeDevices, "M", sMensajeError, "")
                    End If

                    If strTexto.Contains("*L") Then
                        sMensajeError = ExtraerDesde1(strTexto)
                        TraceError(sMensajeError)
                        reportHwStatus("", sPinPadDevices, "M", sMensajeError, "")
                    End If

                    If strTexto.Contains("*w") Then
                        sMensajeError = ExtraerDesde1(strTexto)
                        TraceError(sMensajeError)
                        reportHwStatus("", sDepositadorDevices, "M", sMensajeError, "")
                    End If

                End If
            Catch ex As Exception

            End Try

            If strTexto.Contains("F.") Then
                Dim arrFolio() As String
                arrFolio = strTexto.Split(".")
                folio = arrFolio(1)
                log.Debug("Se obtiene el folio: " + folio)
            End If

            If strTexto.Contains("TARJ:") Then
                Dim arrTar() As String
                arrTar = strTexto.Split(":")
                tarjeta = arrTar(1)
                log.Debug("Se obtiene la tarjeta: " + tarjeta)

            End If

            '############# Buscando Estatus de SW #################
            'log.Debug("List SW Estatus")
            For Each oEst As EstatuSW_Data In def_B_TXN.listEstatusSw
                For Each strPalabrDefClave As String In oEst.listPC_Estatus
                    If InStr(strTexto, strPalabrDefClave) <> 0 Then
                        'Encontre un estatus
                        log.Debug("Encontre un estatus: " & oEst.strIdEstatus)

                        If oEst.strIdEstatus = "5" Then
                            WritePrivateProfileString("SCREENS", "NUM", "hide", file_Work)
                            WritePrivateProfileString("NDC", "SUPER", "SUPERVISOR", file_Interface)
                            log.Debug("Supervisor Levanta")
                        End If

                        Dim strTempFecha As String = getFechaTxn(strTexto)

                        log.Info("Reportando Estatus SW: " & oEst.strDescripcion & " ID Estatus: " & oEst.strIdEstatus & " strFechaTxn:" & strTempFecha)
                        reportSwStatus(oEst.strIdEstatus, strTempFecha, "")

                        Threading.Thread.Sleep(200)
                    End If
                Next
            Next
        Catch ex As Exception
            log.Error("error en obtenTransaccion.BuscandoEstatusSW: " & ex.Message)
        End Try

        '############# Buscando NDC Estatus de Hw ############
        Try
            If def_B_TXN.bUseNDCHwStatus = "TRUE" Then
                Dim b As Boolean = NDC_Hw_Status.findHwStatus(strTexto)
            End If

        Catch ex As Exception
            log.Error("find NDC Status", ex)
        End Try

        '############# Buscando Estatus de HW #################
        Try
            log.Debug("List HW Estatus")
            For Each oEst As EstatuHW_Data In def_B_TXN.listEstatusHw
                For Each strPalabrDefClave As String In oEst.listPC_Estatus
                    If InStr(strTexto, strPalabrDefClave) <> 0 Then
                        'Validacion de M Status y Suplies, la condicion es que el string debe tener *
                        If strPalabrDefClave.Contains("*") Then
                            If oEst.uM_or_S.ToUpper = "TRUE" Then
                                Dim posisionIn As Integer = InStr(strTexto, strPalabrDefClave)
                                Dim txtTemp As String = Mid(strTexto, posisionIn, strTexto.Length)

                                If oEst.m_Data <> "" Then
                                    'Validación M-Estatus
                                    Dim posisionInM As Integer = InStr(txtTemp, "M-")
                                    Dim txtTempM As String = Mid(txtTemp, posisionInM + 2, 2)

                                    If oEst.m_Data.Contains(",") Then
                                        'Multiples mStatus a validar. (<m_Data>07,06,05,03</m_Data>)
                                        Dim bEncontroEstatus As Boolean = False
                                        Dim arrMStatus() As String
                                        Dim stringSeparators() As String = {","}

                                        arrMStatus = oEst.m_Data.Split(stringSeparators, StringSplitOptions.None)
                                        For Each strMstatus As String In arrMStatus
                                            If strMstatus = txtTempM Then
                                                bEncontroEstatus = True
                                            End If
                                        Next

                                        If bEncontroEstatus = False Then

                                            Threading.Thread.Sleep(200)
                                            Exit For
                                        End If


                                    Else
                                        'Solo esta configurado un Mstatus, lo validamos...
                                        If oEst.m_Data <> txtTempM Then
                                            Threading.Thread.Sleep(200)
                                            Exit For
                                        End If
                                    End If


                                Else
                                    If oEst.r_Posicion <> "" Then
                                        'Validación R-Supplies
                                        Dim posisionInR As Integer = InStr(txtTemp, "R-")
                                        Dim txtTempR As String = Mid(txtTemp, posisionInR + 2, 5)
                                        If Mid(txtTempR, CInt(oEst.r_Posicion), 1) <> oEst.r_Valor Then

                                            Threading.Thread.Sleep(200)
                                            Exit For
                                        End If
                                    Else

                                        Threading.Thread.Sleep(200)
                                        Exit For
                                    End If

                                End If
                            End If
                        End If

                        'Encontre un estatus
                        Dim sstCloudApi As New mvCloudAPI.API_Interface
                        Dim arrPizarra(0) As String
                        Dim strTempFecha As String = getFechaTxn(strTexto)
                        log.Info("Reportando Estatus HW: " & oEst.strDescripcion & " ID Dispositivo: " & oEst.strIdDisp & " ID Estatus: " & oEst.strIdEstatus & " ID EstatusExt: " & oEst.strIdEstatusExt & " strFechaTxn:" & strTempFecha)
                        reportHwStatus(oEst.strDescripcion, oEst.strIdDisp, oEst.strIdEstatus, oEst.strIdEstatusExt, strTempFecha)

                        If oEst.strAlerta <> "" Then
                            sstCloudApi.ReportAlert(oEst.strAlerta, strTempFecha, "")
                        End If

                        Threading.Thread.Sleep(200)
                    End If
                Next
            Next
        Catch ex As Exception
            log.Error("error en obtenTransaccion.BuscandoEstatusHW: " & ex.Message)
        End Try


        Try


            log.Debug("---------------- Buscando Transacciones ------------------")
            For Each oTxn As Transaccion_Data In def_B_TXN.listTxnData
                For Each strPalabrDefClave As String In oTxn.listPalabrasI
                    Dim palabraClaveI As String = strPalabrDefClave
                    'log.Debug("palabraClaveI: " & palabraClaveI)

                    'If InStr(strTexto, palabraClaveI) <> 0 Then
                    If strTexto.Contains(palabraClaveI) = True Then
                        log.Debug("strTexto: " & strTexto & " y el valor es diferente de 0")
                        If oTxn.listFinTxn.Count > 0 Then
                            log.Debug("Se encontro un inicio de transaccion")
                            onTxn = True
                            sExtra = ""
                            sExtra = palabraClaveI + vbCrLf
                            'strFechaTxn = Format(Now(), "dd-MM-yyyy HH:mm:ss")
                            strId_Inicio_Txn = oTxn.strId_Txn
                            strPalabaInicial_Txn = strPalabrDefClave
                            WritePrivateProfileString("Data", "ID_INICIO_TXN", strId_Inicio_Txn, strIniJournal)
                            WritePrivateProfileString("Data", "strPalabaInicial_Txn", strPalabaInicial_Txn, strIniJournal)

                            strExtractoTxn = ""
                            strExtractoTxn = strExtractoTxn & strTexto & strEOL
                            log.Debug("strExtractoTxn: " & strExtractoTxn)
                            Exit Sub
                        Else
                            log.Debug("No tiene fin de transacción")
                            Dim sstCloudApi As New mvCloudAPI.API_Interface
                            Dim arrPizarra(0) As String
                            Dim strFechaTxn As String = getFechaTxn(strTexto)
                            strExtractoTxn = ""

                            log.Info("Reportando TXN sin fin cofigurado: " & oTxn.strDescr & " ID TXN: " & oTxn.strId_Txn & " Id_Status: 0, strFechaTxn: " & strFechaTxn)

                            For Each s As String In arrPizarra
                                'log.Debug("Valor: " + s)
                            Next

                            sstCloudApi.ReportTransaction(arrPizarra, "0", oTxn.strId_Txn, strFechaTxn, strFechaTxn)
                            WritePrivateProfileString("Data", "ID_INICIO_TXN", "", strIniJournal)
                            WritePrivateProfileString("Data", "strPalabaInicial_Txn", "", strIniJournal)
                            strId_Inicio_Txn = ""
                            strExtractoTxn = ""
                            strPalabaInicial_Txn = ""


                            folio = ""
                            tarjeta = ""



                            Threading.Thread.Sleep(1000)

                        End If
                    Else
                        'poner lo opcode

                    End If
                Next
            Next

            'Ya hay un inicio de transaccion, buscamos su fin
            If strId_Inicio_Txn <> "" Then
                'log.Debug("Ya hay un inicio de transaccion, buscamos su fin")
                'Concatenamos el extracto de la transaccion en un cadena.
                strExtractoTxn = strExtractoTxn & strTexto & strEOL
                For Each oTxn As Transaccion_Data In def_B_TXN.listTxnData
                    'If oTxn.strId_Txn = strId_Inicio_Txn Then

                    For Each strInicioTxn As String In oTxn.listPalabrasI

                        If strPalabaInicial_Txn = strInicioTxn Then
                            'Vamos a iterar todos las transacciones que coincidan con la palabra de inicio que se encontró antes. Despues validara la palabra de fin y por ultimo los caracteres mandatorios
                            For Each oFin_Txn As Fin_Txn In oTxn.listFinTxn
                                'Buscamos la palabra clave fin de la transaccion que encontramos antes
                                'log.Debug("strFinalConfig: " & oFin_Txn.strPal_Cve_F)
                                If strTexto.Contains(oFin_Txn.strPal_Cve_F) = True Then
                                    'End If
                                    'log.Debug("Se tiene una coincidencia en el final de txn y texto encontrado")
                                    'If InStr(strTexto, oFin_Txn.strPal_Cve_F) <> 0 Then
                                    Dim bEncontroCaracteresMandatorios As Boolean = True
                                    For Each caracteresMand As String In oTxn.listCaracMandat
                                        'J.L 09/03/2016, cambie para que busque en strExtracto y no en strText
                                        If strExtractoTxn.Contains(caracteresMand) = False Then
                                            'log.Debug("Descartando transaccion, no se encontro el caracter mandatorio: " + caracteresMand)
                                            bEncontroCaracteresMandatorios = False
                                        End If
                                    Next

                                    If bEncontroCaracteresMandatorios Then
                                        'log.Debug("Encontre FIN de transaccion, reportando la tranascción que cumple con todos los caracteres mandatorios.")
                                        Dim sstCloudApi As New mvCloudAPI.API_Interface
                                        Dim arrPizarra() As String
                                        Dim strFechaTxn = getFechaTxn(strExtractoTxn)


                                        arrPizarra = getParams(oTxn)

                                        arrPizarra(0) = folio
                                        arrPizarra(1) = tarjeta

                                        For Each strValue As String In arrPizarra
                                            If strValue = "" Then
                                                'log.Debug("Valor encontrado en el arreglo viene en blanco")
                                                strValue = oTxn.defaultCharacterForEmptyValues
                                            End If
                                        Next


                                        'log.Info("Reportando Fin TXN: " & oTxn.strDescr & " ID TXN: " & oTxn.strId_Txn & " Id_Status: " & oFin_Txn.strId_Status & " strFechaTxn: " & strFechaTxn)

                                        reportTxn(arrPizarra, oTxn.strDescr, oTxn.strId_Txn, oFin_Txn.strId_Status, strFechaTxn, strFechaTxn, sExtra)

                                        onTxn = False
                                        sExtra = ""



                                        'log.Debug("extractoTxn: " & strExtractoTxn)
                                        WritePrivateProfileString("Data", "ID_INICIO_TXN", "", strIniJournal)
                                        strId_Inicio_Txn = ""
                                        strExtractoTxn = ""



                                        folio = ""
                                        tarjeta = ""

                                        Threading.Thread.Sleep(1000)
                                    Else
                                        log.Debug("No se encontro el caracter mandatorio: ")
                                    End If
                                End If
                            Next
                            'End If
                        End If
                    Next
                Next
            End If

        Catch ex As Exception
            log.Error("error en obtenTransaccion.BuscandoTransacciones: " & ex.Message)
        End Try
    End Sub

    Public Sub reportSwStatus(ByVal id As String, ByVal fechaI As String, ByVal fechaT As String)

        Dim contenido As String = String.Empty

        Try
            ' Genera nombre con fecha, hora y milisegundos
            Dim timestamp As String = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")
            Dim nombreArchivo As String = $"{timestamp}.sw"
            Dim rutaCompleta As String = Path.Combine(strPathMessage, nombreArchivo)

            If fechaI = "" Then fechaI = Format(Now(), "yyyy-MM-dd HH:mm:ss")

            contenido = $"ID: {id}{Environment.NewLine}" &
                        $"Fecha Inicio: {fechaI}{Environment.NewLine}" &
                        $"Fecha Termino: {fechaT}{Environment.NewLine}"

            ' Escribe el contenido
            log.Debug("Contenido del archivo Sw: " & contenido)
            File.WriteAllText(rutaCompleta, contenido)

        Catch ex As Exception
            Console.WriteLine("Error al crear el archivo: " & ex.Message)
        End Try
    End Sub

    Public Sub reportHwStatus(ByRef descripcion As String, ByVal id As String, ByVal idStatus As String, ByVal idEstatusExt As String, ByVal fechaI As String)
        Dim contenido As String = String.Empty

        Try
            ' Genera nombre con fecha, hora y milisegundos
            Dim timestamp As String = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")
            Dim nombreArchivo As String = $"{timestamp}.hw"
            Dim rutaCompleta As String = Path.Combine(strPathMessage, nombreArchivo)

            If fechaI = "" Then fechaI = Format(Now(), "yyyy-MM-dd HH:mm:ss")

            contenido = $"Descripcion: {descripcion}{Environment.NewLine}" &
                        $"ID: {id}{Environment.NewLine}" &
                        $"ID Status: {idStatus}{Environment.NewLine}" &
                        $"ID Estatus Ext: {idEstatusExt}{Environment.NewLine}" &
                        $"Fecha Inicio: {fechaI}{Environment.NewLine}"

            ' Escribe el contenido
            log.Debug("Contenido del archivo Hw: " & contenido)
            File.WriteAllText(rutaCompleta, contenido)

        Catch ex As Exception
            Console.WriteLine("Error al crear el archivo: " & ex.Message)
        End Try
    End Sub

    Private Sub reportTxn(ByRef arrPizarra() As String, ByVal descripcion As String, ByVal id As String, ByVal idStatus As String, ByVal fechaI As String, ByVal fechaT As String, sExt As String)
        Dim contenido As String = String.Empty

        Try
            ' Genera nombre con fecha, hora y milisegundos
            Dim timestamp As String = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")
            Dim nombreArchivo As String = $"{timestamp}.txn"
            Dim rutaCompleta As String = Path.Combine(strPathMessage, nombreArchivo)

            Try
                For i As Integer = 0 To arrPizarra.Length - 1
                    'log.Debug($"Índice {i}: {arrPizarra(i)}")
                    If arrPizarra(i) <> If(Nothing, "") Then
                        contenido += $"Valor {i}: {arrPizarra(i)}{Environment.NewLine}"
                    End If
                Next

            Catch ex As Exception
                log.Error("Error al construir el contenido de la transacción: " & ex.Message)
            End Try


            contenido += $"Descripcion: {descripcion}{Environment.NewLine}" &
                        $"ID: {id}{Environment.NewLine}" &
                        $"ID Status: {idStatus}{Environment.NewLine}" &
                        $"Fecha Inicio: {fechaI}{Environment.NewLine}" &
                        $"Fecha Termino: {fechaT}{Environment.NewLine}" &
                        $"Extra: {sExt}{Environment.NewLine}"

            ' Escribe el contenido
            'log.Debug("Contenido del archivo Txn: " & contenido)
            File.WriteAllText(rutaCompleta, contenido)

        Catch ex As Exception
            log.Debug("Error al crear el archivo: " + ex.Message)
        End Try
    End Sub

    Private Function getParams(oTxn As Transaccion_Data) As String()
        Dim arrPizarra(oTxn.sizeOfParamArray) As String
        Dim iParam As Integer = 0
        For Each oPrmTxn As Parametros_Txn In oTxn.listParametros
            Dim TextLines() As String = strExtractoTxn.Split(Environment.NewLine)
            If TextLines.Length < 2 Then TextLines = strExtractoTxn.Split(Chr(10))
            log.Debug("TextLines.length: " + TextLines.Length.ToString)
            For Each strLineExtract In TextLines
                Try
                    'Buscamos en todas las lineas del extracto, los parametros..
                    Dim iInicioPalabra As Integer = InStr(strLineExtract, oPrmTxn.strP_I)
                    If iInicioPalabra > 0 Then
                        'log.Debug("Encontramos inicio de parametro: " + oPrmTxn.strP_I + "Coincide con: " + strLineExtract)
                        Dim strValueParam As String
                        'Encontro el inicio de la palabra
                        iInicioPalabra = iInicioPalabra + oPrmTxn.strP_I.Length - 1
                        'Buscamos el fin de la palabra
                        Dim extractoCortado As String = Mid(strLineExtract, iInicioPalabra, strLineExtract.Length)
                        'log.Debug("extractoCortado: " + extractoCortado)
                        'log.Debug("oPrmTxn.strP_F: " + oPrmTxn.strP_F)

                        Dim iFinPalabra As Integer = InStr(extractoCortado, oPrmTxn.strP_F)
                        'log.Debug("iFinPalabra: " + iFinPalabra.ToString)
                        If iFinPalabra > 0 Then
                            'Tiene configurada palabra de fin y Encontramos inicio y fin 
                            'log.Debug("Encontramos fin de parametro,strLineExtract: " + strLineExtract)
                            'log.Debug("iInicioPalabra: " + iInicioPalabra.ToString)
                            'log.Debug("iFinPalabra: " + iFinPalabra.ToString)

                            strValueParam = LTrim(RTrim(strLineExtract.Substring(iInicioPalabra, iFinPalabra - 2).Replace(strEOL, "")))
                            log.Debug(strValueParam)

                            arrPizarra(oPrmTxn.strPos) = strValueParam

                            'log.Debug("Se graba la variable")

                        End If
                    End If
                Catch ex As Exception
                    log.Error("error buscando parametros de TXN: " + ex.Message)
                End Try
                'If arrPizarra(iParam) = Nothing Or arrPizarra(iParam) = "" Then arrPizarra(iParam) = "null"
            Next
            iParam = iParam + 1
        Next
        Return arrPizarra
    End Function

    ''' <summary>
    ''' Elimina un prefijo de fecha y hora que aparece en el journal.
    ''' </summary>
    Private Function StripJournalTimestamp(input As String) As String
        Try
            ' Elimina formato tipo "24/03/26 09:32:31 " al inicio del string
            Dim rx As New Regex("^\d{2}/\d{2}/\d{2}\s+\d{2}:\d{2}:\d{2}\s*")
            Return rx.Replace(input, "", 1)
        Catch ex As Exception
            Return input
        End Try
    End Function

    ''' <summary>
    ''' Devuelve true si la línea parece ser de contador de cassette.
    ''' </summary>
    Private Function IsCounterCassetteLine(input As String) As Boolean
        Try
            ' Busca la palabra CASSETTE seguida de varios números (formato de contadores)
            Dim rx As New Regex("CASSETTE\s+\d{2,}\s+\d{2,}", RegexOptions.IgnoreCase)
            Return rx.IsMatch(input)
        Catch ex As Exception
            Return False
        End Try
    End Function

End Module
