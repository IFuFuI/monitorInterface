Imports System.Text.RegularExpressions

Public Class NDC_Hw_Status
    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    '    Private Shared oSstCloudApi As New sstCloudAPI.API_Interface
    Public Shared oSstCloudApi As New mvCloudAPI.API_Interface

    Public Sub New()
        'Reference Manual Page 911
        'D ‐ Card reader/writer
        'E ‐ Cash handler
        'F ‐ Depository
        'G ‐ Receipt printer
        'L ‐ Encryptor
        'Y ‐ Coin dispenser
        'w ‐ Bunch Note Acceptor (BNA)

    End Sub



    Public Shared Function findHwStatus(strLine As String) As Boolean

        Try
            '*0007*1*E*000000000,S-0,M-00,R-12232    
            '*0042*1*G*0848040000,S-4   
            'log.Debug("findHwStatus")

            Dim strNDCString As String
            strNDCString = Regex.Match(strLine, "\*(\d{4})\*(\d{1})\*([A-Za-z])\*").ToString()
            If strNDCString <> "" Then
                Dim sSeverity As String
                Dim iSeverity As Integer
                'Es una cadena de NDC
                log.Debug("Cadena NDC: " & strNDCString)
                sSeverity = Regex.Match(strLine, "\S-[0-9]").ToString.Replace("S-", "")
                If Integer.TryParse(sSeverity, iSeverity) Then
                    log.Info("iSeverity: " & iSeverity.ToString)
                    Dim iCodeError As Integer
                    If iSeverity > 0 Then
                        'Se encontró una severidad mayor que cero.
                        iCodeError = 1
                    Else
                        iCodeError = 0
                    End If

                    Dim strDeviceLetter As String = Regex.Match(strLine, "\*([A-Za-z])\*").ToString.Replace("*", "")
                    If strDeviceLetter <> "" Then
                            Select Case strDeviceLetter
                                Case "D"
                                'Card Reader
                                oSstCloudApi.ReportHwStatus("1", iCodeError, strNDCString)
                                'oSstCloudApi.ReportHwStatus(oSstCloudApi.HwDevicesDef.CardReader, iCodeError, strNDCString)
                                log.Info("Report CardReader Fail, NDC Status: " & strNDCString)
                                Case "E"
                                'Cash Handler
                                oSstCloudApi.ReportHwStatus("3", iCodeError, strNDCString)
                                'oSstCloudApi.ReportHwStatus(oSstCloudApi.HwDevicesDef.BillDispenser, iCodeError, strNDCString)
                                log.Info("Report Cash Handler Fail, NDC Status: " & strNDCString)
                                Case "G"
                                'Receipt Printer
                                oSstCloudApi.ReportHwStatus("4", iCodeError, strNDCString)
                                'oSstCloudApi.ReportHwStatus(oSstCloudApi.HwDevicesDef.Printer, iCodeError, strNDCString)
                                log.Info("Report Receipt Printer Fail, NDC Status: " & strNDCString)
                                Case "Y"
                                'Coin Dispenser
                                oSstCloudApi.ReportHwStatus("6", iCodeError, strNDCString)
                                'oSstCloudApi.ReportHwStatus(oSstCloudApi.HwDevicesDef.CoinDispenser, iCodeError, strNDCString)
                                log.Info("Report Coin Dispenser Fail, NDC Status: " & strNDCString)
                                Case "w"
                                'BNA
                                oSstCloudApi.ReportHwStatus("2", iCodeError, strNDCString)
                                'oSstCloudApi.ReportHwStatus(oSstCloudApi.HwDevicesDef.BillAcceptor, iCodeError, strNDCString)
                                log.Info("Report BNA Fail, NDC Status: " & strNDCString)
                            Case Else
                                log.Warn("Device is not in the report case, strDeviceLetter: " & strDeviceLetter.ToString)
                        End Select
                        End If


                    End If

            End If

        Catch ex As Exception
            log.Error("", ex)
        End Try
        Return True
    End Function


End Class
