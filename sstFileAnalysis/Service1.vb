Imports System.IO
Imports System.Xml.Serialization
Imports System.Timers

Public Class Service1
    Dim strHoraTxn As Date
    Dim strRutaJournal As String = String.Empty
    Dim strRutaMerge As String = String.Empty
    Dim secondPath As String = "C:\appMain\work\process\"
    Dim copyPathMerge As String = "C:\appCloudAgent\temp\MergeTemp.log"
    Dim strRutaInterface As String = "C:\appMain\work\mvInterface.ini"
    Private timerTxn As Timer = Nothing
    Private timerInterface As Timer = Nothing
    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Protected Overrides Sub OnStart(ByVal args() As String)
        ' Add code here to start your service. This method should set things
        ' in motion so your service can do its work.
        'testFileConfig()
        log4net.Config.XmlConfigurator.Configure()
        cargaDefTxn()
        strRutaJournal = fileJounral

        ' Normalizar el Journal existente al arrancar (reemplazar X[ por ESC y hacer backup)
        Try
            NormalizeExistingJournal()
        Catch ex As Exception
            log.Error("Error al normalizar Journal existente: " & ex.Message)
        End Try

        bProcessing = False
        bProcessingTxn = False
        log.Debug("Service Iniciado appWhere v2.0.2")

        log.Debug("Escuchando...")
        createEventFileWatcher(strRutaJournal)

        Try
            log.Debug("Activa timer Txn")
            timerTxn = New Timer
            AddHandler timerTxn.Elapsed, AddressOf timerTxn_Elapsed
            timerTxn.Interval = CInt(def_B_TXN.iTimeCheck)
            timerTxn.Start()
        Catch ex As Exception
            log.Error("Error al iniciar timerTxn: " + ex.Message)
        End Try

        Try
            log.Debug("Activa timer Interface")
            timerInterface = New Timer
            AddHandler timerInterface.Elapsed, AddressOf timerInterface_Elapsed
            timerInterface.Interval = CInt(def_B_TXN.iTimeCheck)
            timerInterface.Start()
        Catch ex As Exception
            log.Error("Error al iniciar timerTxn: " + ex.Message)
        End Try

    End Sub

    Private Sub timerInterface_Elapsed(ByVal sender As System.Object, ByVal e As ElapsedEventArgs)

        timerInterface.Enabled = False
        Try
            Dim sData As String = ""

            sData = Read_Ini("NDC", "EVENT", strRutaInterface)

            If sData <> "" Then

                log.Debug("Evento en Interface: " + sData)

                Try
                    If sData <> "1" Then WritePrivateProfileString("NDC", "EVENT", "", strRutaInterface)
                Catch ex As Exception
                    log.Error("Error al limpiar evento en interface: " + ex.Message)
                End Try

                Select Case sData
                    Case "5"
                        'borra contadores
                    Case "6"
                        reportHwStatus("", sImpresoraDevices, sStatusError, "", "")
                    Case "7"
                        reportHwStatus("", sImpresoraDevices, sStatusOk, "", "")
                    Case "8"
                        reportHwStatus("", sDispensadorDevices, sStatusError, "", "")
                    Case "9"
                        reportHwStatus("", sDispensadorDevices, "2", "", "")
                    Case "10"
                        reportHwStatus("", sDispensadorDevices, sStatusOk, "", "")
                    Case "11"
                        reportHwStatus("", sContacLessDevices, sStatusError, "", "")
                    Case "12"
                        reportHwStatus("", sContacLessDevices, sStatusOk, "", "")
                    Case "13"
                        reportHwStatus("", sLectoraDevices, sStatusError, "", "")
                    Case "14"
                        reportHwStatus("", sLectoraDevices, sStatusOk, "", "")
                    Case "15"
                        reportHwStatus("", sPinPadDevices, sStatusError, "", "")
                    Case "16"
                        reportHwStatus("", sPinPadDevices, sStatusOk, "", "")
                    Case "17"
                        reportHwStatus("", sAlarmaVibracion, sStatusError, "", "")
                    Case "18"
                        reportHwStatus("", sAlarmaVibracion, sStatusOk, "", "")
                    Case "19"
                        reportHwStatus("", sAlarmaContacto, sStatusError, "", "")
                    Case "20"
                        reportHwStatus("", sAlarmaContacto, sStatusOk, "", "")
                    Case "21"
                        reportHwStatus("", sAlarmaSilenciosa, sStatusError, "", "")
                    Case "22"
                        reportHwStatus("", sAlarmaSilenciosa, sStatusOk, "", "")
                    Case "23"
                        reportHwStatus("", sAlarmaElectonica, sStatusError, "", "")
                    Case "24"
                        reportHwStatus("", sAlarmaElectonica, sStatusOk, "", "")
                    Case "25"
                        log.Debug("OFFLINE")
                        WritePrivateProfileString("APP", "STATUSAPP", "", file_Work)
                        statusATM = False
                        reportSwStatus("4", "", "")
                        WritePrivateProfileString("SCREENS", "NUM", "hide", file_Work)
                    Case "26"
                        Try
                            Dim nombreProceso As String = "appConfigurador.exe" ' Reemplaza con el nombre real del proceso

                            ' Buscar procesos con ese nombre
                            Dim procesos() As Process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(nombreProceso))

                            If procesos.Length > 0 Then
                                For Each p As Process In procesos
                                    Try
                                        p.Kill()
                                    Catch ex As Exception
                                        log.Debug("Error al terminar el proceso: " & ex.Message)
                                    End Try
                                Next
                            End If

                        Catch ex As Exception
                            log.Debug("Error al buscar proceso: " & ex.Message)
                        End Try
                        log.Debug("Salida de supervisor")
                    Case Else
                        log.Info("Evento no manejado: " + sData)
                End Select

            End If

        Catch ex As Exception
            log.Error("Error en timerInterface_Elapsed: " + ex.Message)
        End Try
        timerInterface.Enabled = True

    End Sub

    Private Sub timerTxn_Elapsed(ByVal sender As System.Object, ByVal e As ElapsedEventArgs)
        'Codigo Customizado del api
        If bProcessingTxn Then Exit Sub

        strRutaMerge = fileMerge
        Dim diaActual As Integer = DateTime.Now.Day

        timerTxn.Enabled = False

        Try
            My.Computer.FileSystem.CopyFile(strRutaMerge & "MergedTrace_" & diaActual.ToString & ".log", copyPathMerge, True)
            readTxn()
        Catch ex As Exception

        End Try

        timerTxn.Enabled = True
        'readJournal(secondPath, def_B_TXN.oneJournalFile)
    End Sub


    Private Sub createEventFileWatcher(ByVal strDirectory)
        Try
            Dim watcher As New FileSystemWatcher()
            watcher.Path = strDirectory
            watcher.IncludeSubdirectories = False
            watcher.NotifyFilter = (NotifyFilters.LastAccess Or NotifyFilters.LastWrite Or NotifyFilters.FileName Or NotifyFilters.DirectoryName)
            AddHandler watcher.Changed, AddressOf OnChanged
            watcher.EnableRaisingEvents = True
        Catch ex As Exception
            log.Error("error en createEventFileWatcher: " & ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Normaliza el archivo Journal existente en disco:
    ''' - reemplaza los placeholders "X["/"x[" por la secuencia ESC (Chr(27) & "[")
    ''' - elimina las secuencias "X)"/"x)"
    ''' No añade timestamps a líneas históricas; sólo corrige los prefijos para que
    ''' el archivo se muestre igual que el EJ sample.
    ''' </summary>
    Private Sub NormalizeExistingJournal()
        Try
            Dim journalPath As String = "C:\appMain\Journal\Journal.txt"
            If Not File.Exists(journalPath) Then
                log.Debug("NormalizeExistingJournal: Journal no existe: " & journalPath)
                Exit Sub
            End If

            Dim original As String = File.ReadAllText(journalPath)
            Dim normalized As String = original.Replace("x[", Chr(27) & "[").Replace("X[", Chr(27) & "[") _
                                     .Replace("x(", Chr(27) & "(").Replace("X(", Chr(27) & "(") _
                                     .Replace("X)", Chr(27) & "(").Replace("x)", Chr(27) & "(")

            If Not normalized.Equals(original) Then
                File.WriteAllText(journalPath, normalized)
                log.Debug("NormalizeExistingJournal: Journal normalizado")
            Else
                log.Debug("NormalizeExistingJournal: Journal ya normalizado (no cambios)")
            End If
        Catch ex As Exception
            log.Error("NormalizeExistingJournal error: " & ex.Message)
        End Try
    End Sub


    Private Function isFileAJournal(strNameFile) As Boolean
        Try
            If def_B_TXN.oneJournalFile <> "" And def_B_TXN.oneJournalFile <> Nothing Then
                'Es una journal de un solo archivo 
                If UCase(strNameFile) = def_B_TXN.oneJournalFile Then
                    'log.Debug("Journal correcta, procesando: " & strNameFile)
                    Return True
                Else
                    Return False
                End If
            Else
                'Journal de multiples archivos
                For Each strNameProcNotReq As String In def_B_TXN.listFilesNotApplicables
                    Dim nameFIle As String = UCase(strNameFile)
                    If nameFIle.Contains(UCase(strNameProcNotReq)) Then
                        log.Debug("Archivo " & nameFIle & " No es un archivo No aplicable, no se procesa")
                        Return False
                    End If
                Next
                Return True
            End If
        Catch ex As Exception
            log.Error("", ex)
            Return False
        End Try



    End Function

    Private Sub OnChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
        If isFileAJournal(e.Name) = False Then Exit Sub
        'Validamos si el archivo que cambio, no esta en la lista de archivos que no se procesan.
        If bProcessing Then Exit Sub

        If e.Name = "EJDATA.LOG" Then
            'log.Debug("ARCHIVO A LEER: " & e.Name)
            'log.Debug("JOURNAL CAMBIO: " & e.Name)

            'Se realiza proceso de copiar la journal para mejor lecutra 
            My.Computer.FileSystem.CopyFile(strRutaJournal & def_B_TXN.oneJournalFile, secondPath & def_B_TXN.oneJournalFile, True)
            readJournal(secondPath, e.Name)


        Else
            log.Debug("El nombre no es de la Journal" + e.Name)
        End If

        'End If
    End Sub


    Private Function ExtractMerge(ByVal strPathMergeFile As String, ByRef strFinalLineLastMerege As String) As String()
        Dim intNumLinesCopyFile As Integer = 0
        Dim strLinesFiles As String = ""
        Dim arrayStrLines(0) As String ' Este es el array que utilizaremos para checar las transacciones
        Dim objReader As StreamReader

        Try
            'valida que exista la copia
            If File.Exists(strPathMergeFile) Then
                'Agregamos un caracter en blanco porque si la ultima linea esta en blanco no la cuenta la funcion ReadAllLines
                File.AppendAllText(strPathMergeFile, " ")

                objReader = New StreamReader(strPathMergeFile)
                intNumLinesCopyFile = 0
                intNumLinesCopyFile = File.ReadAllLines(strPathMergeFile).Length

                Try
                    If def_B_TXN.bRemoveLastLine Then intNumLinesCopyFile = intNumLinesCopyFile - 1 'Le resto 1 porque mas adelante le quito la ultima linea al arreglo (Nulos banjercito)
                Catch ex As Exception

                End Try

                ' ################ El archivo no ha cambiado ###############
                If CInt(strFinalLineLastMerege) = intNumLinesCopyFile Then
                    objReader.Close()
                    objReader.Dispose()
                    objReader = Nothing
                    ReDim arrayStrLines(-1)
                    'log.Info("Se envio lineas extractMerge archivo no ha cambiado")
                    Return arrayStrLines
                End If
                '############################################################

                ' ###### Cambiaron el numero de lineas ######
                If CInt(strFinalLineLastMerege) < intNumLinesCopyFile Then ' Esta linea nos indica si tenemos más lineas en la variable del ini que en la journal, lo que la vuelve JOURNAL INICIAL y copiamos todo el arreglo
                    'log.Info("File Merge Changed")
                    Dim intCounterLinesCopyFile As Integer = 0 'Variable de control para leer la primera linea
                    'Leer linea a linea hasta el final
                    Do While Not objReader.EndOfStream
                        If intCounterLinesCopyFile = CInt(strFinalLineLastMerege) Then 'Si el contador es igual al numero total de lineas de la Journal 
                            'Encontro la ultima linea que leimos, leyendo las siguientes lineas y terminamos
                            'log.Info("Encontro la ultima linea leida")
                            strLinesFiles = objReader.ReadToEnd
                            'log.Debug("Linea Encontrada Merge: " + strLinesFiles)

                            arrayStrLines = strLinesFiles.Split(vbNewLine)
                            objReader.Close()
                            objReader.Dispose()
                            objReader = Nothing
                            Exit Do
                        End If
                        objReader.ReadLine()
                        intCounterLinesCopyFile += 1
                    Loop
                    'log.Info("Se envio lineas extractMerge")
                    Return arrayStrLines

                Else 'El Merge acaba de iniciar, se le pasa todo el archivo en el array (Tiene mas lineas la ultima journal leida que la actual)
                    log.Info("########### Nuevo Merge ################")
                    log.Info("strFinalLineLastMerge: " + strFinalLineLastMerege)
                    log.Info("intNumLinesCopyFile: " + intNumLinesCopyFile.ToString)


                    arrayStrLines = File.ReadAllLines(strPathMergeFile)

                    objReader.Close()
                    objReader.Dispose()
                    objReader = Nothing

                    strFinalLineLastMerege = "0"
                    initMergeINI()
                    log.Info("Se envio lineas extractMerge")
                    Return arrayStrLines
                End If
            Else
                ReDim arrayStrLines(-1)
                arrayStrLines(0) = ""
                log.Error("Journal inexistente o no se pudo realizar la copia")
                log.Info("Se envio lineas extractMerge")
                Return arrayStrLines
            End If


        Catch ex As Exception
            Try
                objReader.Close()
                objReader.Dispose()
                objReader = Nothing
            Catch ex2 As Exception

            End Try
            ReDim arrayStrLines(-1)
            arrayStrLines(0) = ""
            log.Error("error extrac Merge: " + ex.Message)
            log.Info("Se envio lineas extractMerge")
            Return arrayStrLines
        End Try



    End Function

    Private Function ExtractJournal(ByVal strPathJorunalFile As String, ByVal strTmpOriginalJournalFile As String, ByRef strFinalLineLastJournal As String) As String()
        'Pasar la ruta del archivo a una variable

        'Variable con el numero de lineas del archivo
        Dim intNumLinesCopyFile As Integer = 0
        Dim strLinesFiles As String = ""
        Dim arrayStrLines(0) As String ' Este es el array que utilizaremos para checar las transacciones
        Dim objReader As StreamReader

        Try
            My.Computer.FileSystem.CopyFile(strPathJorunalFile, strTmpOriginalJournalFile, True)
        Catch ex As Exception
            log.Error("No se pudó realizar la copia del archivo: " + ex.Message)
            ReDim arrayStrLines(-1)
            Return arrayStrLines
        End Try



        Try
            'valida que exista la copia
            If File.Exists(strTmpOriginalJournalFile) Then
                'Agregamos un caracter en blanco porque si la ultima linea esta en blanco no la cuenta la funcion ReadAllLines
                File.AppendAllText(strTmpOriginalJournalFile, " ")

                objReader = New System.IO.StreamReader(strTmpOriginalJournalFile)
                intNumLinesCopyFile = 0
                intNumLinesCopyFile = File.ReadAllLines(strTmpOriginalJournalFile).Length
                Try
                    If def_B_TXN.bRemoveLastLine Then intNumLinesCopyFile = intNumLinesCopyFile - 1 'Le resto 1 porque mas adelante le quito la ultima linea al arreglo (Nulos banjercito)
                Catch ex As Exception

                End Try

                ' ################ El archivo no ha cambiado ###############
                If CInt(strFinalLineLastJournal) = intNumLinesCopyFile Then
                    objReader.Close()
                    objReader.Dispose()
                    objReader = Nothing
                    ReDim arrayStrLines(-1)
                    'log.Info("Se envio lineas extractJournal archivo no ha cambiado")
                    Return arrayStrLines
                End If
                '############################################################

                ' ###### Cambiaron el numero de lineas ######
                If CInt(strFinalLineLastJournal) < intNumLinesCopyFile Then ' Esta linea nos indica si tenemos más lineas en la variable del ini que en la journal, lo que la vuelve JOURNAL INICIAL y copiamos todo el arreglo
                    'log.Info("File Changed")
                    Dim intCounterLinesCopyFile As Integer = 0 'Variable de control para leer la primera linea
                    'Leer linea a linea hasta el final
                    Dim bwrite As Boolean = True
                    Do While Not objReader.EndOfStream
                        If intCounterLinesCopyFile = CInt(strFinalLineLastJournal) Then 'Si el contador es igual al numero total de lineas de la Journal 
                            'Encontro la ultima linea que leimos, leyendo las siguientes lineas y terminamos
                            'log.Info("Encontro la ultima linea leida")
                            bwrite = True
                            strLinesFiles = objReader.ReadToEnd
                            'log.Debug("Linea Encontrada: " + strLinesFiles)

                            arrayStrLines = strLinesFiles.Split(vbNewLine)
                            objReader.Close()
                            objReader.Dispose()
                            objReader = Nothing
                            Exit Do
                        End If
                        objReader.ReadLine()
                        intCounterLinesCopyFile += 1
                    Loop
                    'log.Info("Se envio lineas extractJournal")
                    Return arrayStrLines

                Else 'La journal acaba de iniciar, se le pasa todo el archivo en el array (Tiene mas lineas la ultima journal leida que la actual)
                    'log.Info("########### Nueva Journal ################")
                    'log.Info("strFinalLineLastJournal: " + strFinalLineLastJournal)
                    'log.Info("intNumLinesCopyFile: " + intNumLinesCopyFile.ToString)
                    log.Debug("Nuevo archivo")

                    arrayStrLines = File.ReadAllLines(strTmpOriginalJournalFile)

                    objReader.Close()
                    objReader.Dispose()
                    objReader = Nothing

                    strFinalLineLastJournal = "0"
                    initJournalINI()
                    'log.Info("Se envio lineas extractJournal")
                    Return arrayStrLines
                End If
            Else
                ReDim arrayStrLines(-1)
                arrayStrLines(0) = ""
                log.Error("Journal inexistente o no se pudo realizar la copia")
                log.Info("Se envio lineas extractJournal")
                Return arrayStrLines
            End If


        Catch ex As Exception
            Try
                objReader.Close()
                objReader.Dispose()
                objReader = Nothing
            Catch ex2 As Exception

            End Try
            ReDim arrayStrLines(-1)
            arrayStrLines(0) = ""
            log.Error("error extrac Journal: " + ex.Message)
            log.Info("Se envio lineas extractJournal")
            Return arrayStrLines
        End Try


    End Function

    Private Sub readTxn()
        'Dim pathSource As String = strRutaJournal & strNameFile
        Dim pathSource As String = copyPathMerge
        Dim strText As String = ""
        bProcessingTxn = True
        'Threading.Thread.Sleep(10000)
        Try
            If My.Computer.FileSystem.FileExists(pathSource) Then

                Dim strFinalLineLastJournal As String = Read_Ini("Data", "U_No_Linea", strIniMerge)
                Dim strTextoUltimaTxn As String = Read_Ini("Data", "U_Texto_Txn_Repor", strIniMerge)
                If strTextoUltimaTxn = "" Or strTextoUltimaTxn = Nothing Then strTextoUltimaTxn = "0"
                If strFinalLineLastJournal = "" Or strFinalLineLastJournal = Nothing Then strFinalLineLastJournal = "0"

                'log.Debug("Final de linea Jounral: " + strFinalLineLastJournal)
                'log.Debug("se manda a extracto lineas: " + strTextoUltimaTxn)

                arr = ExtractMerge(pathSource, strFinalLineLastJournal)


                'log.Debug("se empieza a recorre el Arr string extracto")
                Dim i As Integer = 0
                For Each strLine In arr
                    'log.Debug("Se encuentra la linea en el extracto: " + strLine)
                    'log.Debug("Elimando caracteres especiales")
                    'arr(i) = LTrim(RTrim(strLine)).Replace(Chr(27), "X")
                    'log.Debug("L" + i.ToString.PadLeft(3, "0") + ": " + arr(i))
                    i = i + 1
                Next

                'log.Debug("Se manda a ProccessMerge")
                'log.Debug("arreglo size: " + arr.Length.ToString)


                If arr.Length > 0 Then
                    Dim b As Boolean = ProccessMerge(arr, pathSource, CInt(strFinalLineLastJournal))
                    ReDim arr(-1)
                End If

            Else
                log.Error("pathNotFound: " & pathSource)
            End If


            bProcessingTxn = False
        Catch ex As Exception
            log.Error("error en readTxn: " & ex.Message)
            bProcessingTxn = False
        End Try
    End Sub


    Private Sub readJournal(strPath, strNameJournal)
        'Dim pathSource As String = strRutaJournal & strNameJournal
        Dim pathSource As String = strPath & strNameJournal
        Dim strText As String = ""

        bProcessing = True
        Threading.Thread.Sleep(10000)

        Try
            If My.Computer.FileSystem.FileExists(pathSource) Then
                'Dim strTextFile As String
                'Using fs As FileStream = New FileStream(pathSource, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                '    Dim sr As New StreamReader(fs)

                '    strTextFile = sr.ReadToEnd
                '    fs.Close()
                'End Using,
                'If strTextFile <> "" Then
                '    Dim arr() As String = strTextFile.Split(Environment.NewLine)
                '    Dim b As Boolean = ProccessJournal(arr, strNameJournal)

                Dim strFinalLineLastJournal As String = Read_Ini("Data", "U_No_Linea", strIniJournal)
                Dim strTextoUltimaTxn As String = Read_Ini("Data", "U_Texto_Txn_Repor", strIniJournal)
                If strTextoUltimaTxn = "" Or strTextoUltimaTxn = Nothing Then strTextoUltimaTxn = "0"
                If strFinalLineLastJournal = "" Or strFinalLineLastJournal = Nothing Then strFinalLineLastJournal = "0"

                'log.Debug("Final de linea Jounral: " + strFinalLineLastJournal)
                'log.Debug("se manda a extracto lineas: " + strTextoUltimaTxn)

                'If def_B_TXN.debug = "TRUE" Then trace("Journal correcta, procesando: " & strNameFile)
                arr = ExtractJournal(pathSource, strPathCopyJournal, strFinalLineLastJournal)


                'ELiminamos la ultima linea, banjercito escribe despues de cada linea puros nulos
                'If def_B_TXN.bRemoveLastLine Then
                '    'log.Debug("se elimina linea TRUE")
                '    If arr.Length > 0 Then
                '        'log.Debug("arreglo size: " + arr.Length.ToString)
                '        Array.Resize(arr, arr.Length - 1)
                '    Else
                '        'log.Debug("[readJounal] el arreglo no trae nada")
                '    End If
                'End If

                'log.Debug("se empieza a recorre el Arr string extracto")
                Dim i As Integer = 0
                For Each strLine In arr
                    'log.Debug("Se encuentra la linea en el extracto: " + strLine)
                    'log.Debug("Elimando caracteres especiales")
                    arr(i) = LTrim(RTrim(strLine)).Replace(Chr(27), "X")
                    'log.Debug("L" + i.ToString.PadLeft(3, "0") + ": " + arr(i))
                    i = i + 1
                Next

                'log.Debug("Se manda a ProccessJournal")
                'log.Debug("arreglo size: " + arr.Length.ToString)


                If arr.Length > 0 Then
                    Dim b As Boolean = ProccessJournal(arr, strNameJournal, CInt(strFinalLineLastJournal))
                    ReDim arr(-1)
                End If
            Else
                'log.Error("pathNotFound: " & pathSource)
            End If
            bProcessing = False
        Catch ex As Exception
            log.Error("error en readFile: " & ex.Message)
            bProcessing = False
        End Try

        Try
            File.Delete(strPathCopyJournal)
            'log.Debug("Se elimino LOG temporal")
        Catch ex As Exception
        End Try

        Try
            'log.Debug("Se elimina journal temporal DSC: " + secondPath + "EJDATA.LOG")
            File.Delete(secondPath + "EJDATA.LOG")
        Catch ex As Exception

        End Try
    End Sub

    Private Sub cargaDefTxn()
        Dim objStreamReader As StreamReader
        Try
            'Deserializa
            objStreamReader = New StreamReader(strFileDefinicionXML)
            Dim x As New XmlSerializer(def_B_TXN.GetType)
            def_B_TXN = x.Deserialize(objStreamReader)
            objStreamReader.Close()

            log.Info("strComentarios " & def_B_TXN.strComentarios)
            log.Info("strModifiedBy " & def_B_TXN.strModifiedBy)
            log.Info("strVersion " & def_B_TXN.strVersion)

            For Each oTxn As Transaccion_Data In def_B_TXN.listTxnData
                For Each oPrmTxn As Parametros_Txn In oTxn.listParametros
                    'log.Debug("---------------------------")
                    oPrmTxn.strP_I = oPrmTxn.strP_I.Replace("%B", " ")
                    oPrmTxn.strP_F = oPrmTxn.strP_F.Replace("%B", " ")
                    'log.Debug("strP_I: " + oPrmTxn.strP_I)
                    'log.Debug("strP_F: " + oPrmTxn.strP_F)
                Next
            Next

        Catch ex As Exception
            log.Fatal("error al cargar la definición de Transacciones, terminando programa: " & ex.Message)
            End
        End Try
    End Sub

    Private Sub initJournalINI()
        WritePrivateProfileString("Data", "U_Texto_Txn_Repor", "", strIniJournal)
        WritePrivateProfileString("Data", "ID_INICIO_TXN", "", strIniJournal)
        WritePrivateProfileString("Data", "U_No_Linea", "", strIniJournal)
    End Sub

    Private Sub initMergeINI()
        WritePrivateProfileString("Data", "U_Texto_Txn_Repor", "", strIniMerge)
        WritePrivateProfileString("Data", "ID_INICIO_TXN", "", strIniMerge)
        WritePrivateProfileString("Data", "U_No_Linea", "", strIniMerge)
    End Sub



End Class
