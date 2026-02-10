Public Class WS_Resp_Codes
    Public Const OK As Integer = 0
    Public Const errDevIsNotRegistered As Integer = -1
    Public Const errCanNoRetrieveCustomerLicense As Integer = -2
    Public Const errCanNoRetrieveDataBaseConnection As Integer = -3
    Public Const errInvalidLicense As Integer = -4
    Public Const errDevIsNotActive As Integer = -5
    Public Const errCanNotCreateFieldKey As Integer = -6
    Public Const errDuplicatedKey As Integer = -7
    Public Const errCanNotOpenDBConn As Integer = -8
    Public Const errNotRowsFound As Integer = -9
    Public Const errSqlError As Integer = -10
    Public Const noPendingCommands As Integer = -11
    Public Const invalidDeviceId As Integer = -12
    Public Const errUnknown As Integer = -100
End Class
