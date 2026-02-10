Public Class Def_Configuration_Attributes
    Public strVersion As String
    Public strComments As String
    Public registryPathToMonitor As String
    Public listIniFiles As New List(Of IniFile)
    Public listIniFileExceptions As List(Of String)
    Public listRegistry As New List(Of Registry)
End Class

Public Class IniFile
    Public iAttributeId As Integer
    Public strFileName As String
    Public strSection As String
    Public strFieldName As String
End Class

Public Class Registry
    Public iAttributeId As Integer
    Public strPath As String
    Public strKey As String
End Class

