Imports System.Timers

Public Class JSsetTimeout

    Public res As Object = Nothing
    Dim WithEvents tm As Timer = Nothing
    Dim _MethodName As String
    Dim _args() As Object
    Dim _ClassInstacne As Object = Nothing

    Public Shared Sub SetTimeout(ByVal ClassInstacne As Object, ByVal obj As String, ByVal TimeSpan As Integer, ByVal ParamArray args() As Object)
        Dim jssto As New JSsetTimeout(ClassInstacne, obj, TimeSpan, args)
    End Sub

    Public Sub New(ByVal ClassInstacne As Object, ByVal obj As String, ByVal TimeSpan As Integer, ByVal ParamArray args() As Object)
        If obj IsNot Nothing Then
            _MethodName = obj
            _args = args
            _ClassInstacne = ClassInstacne
            tm = New Timer With {.Interval = TimeSpan, .Enabled = False}
            AddHandler tm.Elapsed, AddressOf tm_Tick
            tm.Start()
        End If
    End Sub

    Private Sub tm_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tm.Elapsed
        tm.Stop()
        RemoveHandler tm.Elapsed, AddressOf tm_Tick
        If Not String.IsNullOrEmpty(_MethodName) AndAlso _ClassInstacne IsNot Nothing Then
            res = CallByName(_ClassInstacne, _MethodName, CallType.Method, _args)
        Else
            res = Nothing
        End If
    End Sub
End Class
