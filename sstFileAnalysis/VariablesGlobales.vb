Module VariablesGlobales
    Public Const strIniJournal As String = "C:\appCloudAgent\dat\sstFileAnalysis.dat"
    Public Const strIniMerge As String = "C:\appCloudAgent\dat\sstFileAnalysisMerge.dat"
    Public Const strFileDefinicionXML As String = "C:\appCloudAgent\xml\FileAnalysisConfigJournal.xml"
    Public Const strPathCopyJournal As String = "C:\appCloudAgent\temp\EjT.txt"
    Public Const sDSCJournal As String = "C:\appMain\Journal\Journal.txt"
    Public Const strPathMessage As String = "C:\appCloudAgent\queue\"
    Public Const file_Work As String = "C:\appMain\work\work.ini"
    Public Const fileJounral As String = "C:\Program Files (x86)\NCR APTRA\Advance NDC\Data\"
    Public Const fileMerge As String = "C:\Program Files (x86)\NCR APTRA\Advance NDC\Debug\"
    Public Const file_Interface As String = "C:\appMain\work\mvInterface.ini"
    Public Const file_ConfiAtm As String = "C:\appMain\config\appConfigAtm.ini"

    Public Const strEOL As String = "%EOL%"
    Public statusATM As Boolean = False 'Variable que indica el estatus del ATM
    Public flagback As Boolean = False 'Variable que indica el regreso de la pagina anterior del ATM

    Public bProcessing As Boolean = False 'Variable que indica que se esta procesando el journal
    Public bProcessingTxn As Boolean = False 'Variable que indica que se esta procesando una transaccion
    Public def_B_TXN As New Def_Busq_Transacciones
    Public arr() As String

    Public Const sLectoraDevices As String = "1"
    Public Const sDepositadorDevices As String = "2"
    Public Const sDispensadorDevices As String = "3"
    Public Const sImpresoraDevices As String = "4"
    Public Const sPinPadDevices As String = "5"
    Public Const sContacLessDevices As String = "6"
    Public Const sBarCodeDevices As String = "7"
    Public Const sDispensadorMonedasDevices As String = "8"
    Public Const sAlarmaVibracion As String = "9"
    Public Const sAlarmaContacto As String = "10"
    Public Const sAlarmaSilenciosa As String = "11"
    Public Const sAlarmaElectonica As String = "12"

    Public Const sStatusError As String = "1"
    Public Const sStatusOk As String = "0"

End Module
