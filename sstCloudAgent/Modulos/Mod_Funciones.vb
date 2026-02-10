Imports System.IO
Imports Microsoft.Win32
Imports System.Net
Imports System.Xml.Serialization

Module Mod_Funciones
    Private Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Integer, ByVal lpFileName As String) As Integer
    Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Integer
    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Public customerId As String = "-1"
    Public iStatusDeviceOnWS As Integer = WS_Resp_Codes.errUnknown
    Public oHandleCOnfiguration As New HandleConfiguration
    Public oAPI As New mvCloudAPI.API_Interface


    Public Function manageWSAnswer(strFile As String, iResp As Integer, Id_Method As String) As Boolean
        Try
            'iStatusDeviceOnWS
            log.Info("Respuesta: " & iResp.ToString)
            Select Case iResp
                Case WS_Resp_Codes.OK
                    'Everything OK
                    eliminaArchivo(strFile)
                Case WS_Resp_Codes.errDuplicatedKey
                    'The information is alredy in the database
                    eliminaArchivo(strFile)
                Case WS_Resp_Codes.invalidDeviceId
                    'The device id is not valid.
                    eliminaArchivo(strFile)
                Case WS_Resp_Codes.errSqlError
                    'It was a problem with SQL Error, move to SAF
                    If Id_Method = "0" Then moverArchivo(strFile, Constants.strPathTxnSAF)
                Case Else
                    If Id_Method = "0" Then moverArchivo(strFile, Constants.strPathTxnSAF)
            End Select
        Catch ex As Exception
            log.Error("", ex)
        End Try
        Return True
    End Function

    Public Function manageWSAnswerConfiguration(strFile As String, iResp As Integer, Id_Method As String) As Boolean
        Try
            'iStatusDeviceOnWS
            log.Info("Respuesta: " & iResp.ToString)

            If iResp > 0 Then
                'Success
                'Escribimos el id del cajero
                log.Info("Se escribe ID del cajero: " & iResp.ToString)
                WritePrivateProfileString("Information", "DeviceID", iResp.ToString, Constants.strAgentWork)
                eliminaArchivo(strFile)
            Else
                'Si la configuracion tiene error la movemos al store and foward
                Select Case iResp
                    'Case WS_Resp_Codes.errSqlError
                    '    moverArchivo(strFile, Constants.strPathNoProc)
                    Case Else
                        If Id_Method = "0" Then moverArchivo(strFile, Constants.strPathTxnSAF)
                End Select
            End If

        Catch ex As Exception
            log.Error("", ex)
        End Try
        Return True
    End Function
    Public Sub eliminaArchivo(strFile As String)
        Try
            My.Computer.FileSystem.DeleteFile(strFile, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)
        Catch ex As Exception
            log.Error("file: " & strFile, ex)
        End Try
    End Sub
    Public Sub moverArchivo(strFile As String, strDestino As String)
        Try
            'Creamos el directorio si no existe
            If My.Computer.FileSystem.DirectoryExists(strDestino) = False Then My.Computer.FileSystem.CreateDirectory(strDestino)
            My.Computer.FileSystem.MoveFile(strFile, strDestino & Path.GetFileName(strFile), True)
        Catch ex As Exception
            log.Error("file: " & strFile, ex)
        End Try
    End Sub
    Public Sub moverArchivo(strFile As String, strDestino As String, strNewName As String)
        Try
            'Creamos el directorio si no existe
            If My.Computer.FileSystem.DirectoryExists(strDestino) = False Then My.Computer.FileSystem.CreateDirectory(strDestino)
            My.Computer.FileSystem.MoveFile(strFile, strDestino & strNewName, True)
        Catch ex As Exception
            log.Error("file: " & strFile, ex)
        End Try
    End Sub
    Public Sub copiarArchivo(strFile As String, strDestino As String)
        Try
            My.Computer.FileSystem.CopyFile(strFile, strDestino & Path.GetFileName(strFile), True)
        Catch ex As Exception
            log.Error("file: " & strFile, ex)
        End Try
    End Sub
    Private Function gerProperty(ByVal f As String, pn As String)
        Dim props As New Properties()
        Dim sr As New StreamReader(f)
        Dim someProp As String = ""
        Try
            props.Load(sr)
            someProp = props.GetProperty(pn)
            sr.Close()
            sr.Dispose()
            props = Nothing

        Catch ex As Exception
            someProp = ""
            log.Error("", ex)
        End Try
        Return someProp

    End Function
    Public Function Read_Ini(ByVal strSeccion As String, ByVal strLlave As String, ByVal strPath As String) As String
        Dim strReturn As String = ""
        Try
            If strSeccion = "" Then
                'Read as properties

                strReturn = gerProperty(strPath, strLlave)
            Else
                Dim bufer As String
                Dim Len_Value As Integer
                bufer = New String(Chr(0), 4000)
                Len_Value = GetPrivateProfileString(strSeccion, strLlave, 0, bufer, Len(bufer), strPath)
                strReturn = Microsoft.VisualBasic.Strings.Left(bufer, Len_Value)
            End If
        Catch ex As Exception
            log.Error("Error en Read_Ini: ", ex)
            strReturn = ""
        End Try

        Return strReturn
    End Function
    Public Sub deleteOldFiles()
        Try
            Dim sNumeroDias As String
            Dim iNumeroDiasFilesLive As Integer

            log.Info("Executing deleteOldFiles")
            sNumeroDias = Read_Ini("CONFIGURATION", "DAYS_MESSAGES_FILES_LIVE", Constants.strPathAgentConfig)
            If Integer.TryParse(sNumeroDias, iNumeroDiasFilesLive) = False Then
                iNumeroDiasFilesLive = 14
            End If
            log.Info("DAYS_MESSAGES_FILES_LIVE: " & iNumeroDiasFilesLive.ToString)

            deleteFilesFromPathTime(Constants.strPathMessage, iNumeroDiasFilesLive)
            deleteFilesFromPathTime(Constants.strPathTxnConc, iNumeroDiasFilesLive)
            deleteFilesFromPathTime(Constants.strPathTxnSAF, iNumeroDiasFilesLive)
            deleteFilesFromPathTime(Constants.strPathNoProc, iNumeroDiasFilesLive)
        Catch ex As Exception
            log.Error("", ex)
        End Try

    End Sub

    Public Sub deleteOldJournalsBackups()
        Try
            Dim sNumeroDias As String
            Dim iNumeroDiasFilesLive As Integer

            log.Info("Executing DAYS_JOURNALS_BACKUPS_LIVE")
            sNumeroDias = Read_Ini("CONFIGURATION", "DAYS_JOURNALS_BACKUPS_LIVE", Constants.strPathAgentConfig)
            If Integer.TryParse(sNumeroDias, iNumeroDiasFilesLive) = False Then
                iNumeroDiasFilesLive = 60
            End If
            log.Info("DAYS_JOURNALS_BACKUPS_LIVE: " & iNumeroDiasFilesLive.ToString)

            deleteFilesFromPathTime(Constants.pathJournalsBackup, iNumeroDiasFilesLive)
        Catch ex As Exception
            log.Error("", ex)
        End Try

    End Sub
    Private Function deleteFilesFromPathTime(strPath As String, numeroDias As Integer) As Boolean

        Dim resp As Boolean = False
        Dim fileDate As DateTime
        Dim fechaActual As DateTime = DateTime.Now
        Try
            For Each sFile As String In My.Computer.FileSystem.GetFiles(strPath, FileIO.SearchOption.SearchTopLevelOnly)
                fileDate = My.Computer.FileSystem.GetFileInfo(sFile).LastWriteTime
                Dim difDate As Integer = (CType(fechaActual, DateTime) - CType(fileDate, DateTime)).TotalDays
                If difDate >= numeroDias Then
                    log.Debug("Eliminando archivo 'viejo': " & sFile)
                    log.Debug("fileDate: " & fileDate)
                    log.Debug("difDate: " & difDate.ToString)
                    log.Debug("numeroDias: " & numeroDias.ToString)
                    log.Debug("fechaActual: " & fechaActual.ToString)
                    File.Delete(sFile)
                End If
            Next
            Return True
        Catch ex As Exception
            resp = False
            log.Error("", ex)
        End Try
        Return resp
    End Function

    Public Sub printOjectFields(o As Object)
        Try
            For Each p As System.Reflection.FieldInfo In o.GetType().GetFields()
                Try
                    log.Debug(p.Name.ToString & " " & p.GetValue(o))
                Catch ex As Exception
                    log.Debug("", ex)
                End Try

            Next
        Catch ex As Exception
            log.Warn("Error when trying to print object properties", ex)
        End Try
    End Sub


    Public Sub getDeviceStatusOnWS()
        Dim strDeviceId As String = oAPI.GetDeviceID
        Dim oSsstsWS As New sstsWebService.Service1
        Dim strUrl As String = Read_Ini("CONFIGURATION", "URL_WS", Constants.strPathAgentConfig)
        Try
            'Possible Values
            'Constants.OK
            'Constants.errDevIsNotActive
            'Constants.errDevIsNotRegistered
            'Constants.errInvalidLicense
            'Constants.errCanNoRetrieveCustomerLicense
            'Constants.errSqlError
            'Constants.errCanNotOpenDBConn
            'Constants.errUnknown

            oSsstsWS.Url = strUrl
            'If Integer.TryParse(strDeviceId, iIdDevice) = False Then
            '    log.Info("El id no esta configurado, solicitando configuracion del equipo")
            '    oHandleCOnfiguration.sendConfiguration()
            'End If

            iStatusDeviceOnWS = oSsstsWS.getDeviceStatus(customerId, strDeviceId)
            Select Case iStatusDeviceOnWS
                Case WS_Resp_Codes.errDevIsNotRegistered
                    log.Info("The device is not registered, send config")
                    oHandleCOnfiguration.sendConfiguration()
                Case Else
                    log.Warn("getDeviceStatusOnWS.The device has a status: " & iStatusDeviceOnWS.ToString)
            End Select
        Catch ex As Exception
            log.Error("", ex)
            iStatusDeviceOnWS = WS_Resp_Codes.errUnknown
        End Try
        'log.Debug("Write Status in DAT")
        WritePrivateProfileString("Information", "Device_Status", iStatusDeviceOnWS.ToString, Constants.strAgentWork)

    End Sub






End Module
