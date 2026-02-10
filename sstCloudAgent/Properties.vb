Public Class Properties

    Private m_Properties As New Hashtable

    Public Sub New()

    End Sub

    Private Sub Add(ByVal key As String, ByVal value As String)
        m_Properties.Add(key, value)
    End Sub



    Public Sub Load(ByRef sr As IO.StreamReader)

        Dim line As String
        Dim key As String
        Dim value As String

        Do While sr.Peek <> -1
            line = sr.ReadLine
            If line = Nothing OrElse line.Length = 0 OrElse line.StartsWith("#") Then
                Continue Do
            End If

            key = line.Split("=")(0)
            value = line.Split("=")(1)

            Add(key, value)

        Loop

    End Sub

    Public Function GetProperty(ByVal key As String)

        Return m_Properties.Item(key)

    End Function

    Public Function GetProperty(ByVal key As String, ByVal defValue As String) As String

        Dim value As String = GetProperty(key)
        If value = Nothing Then
            value = defValue
        End If

        Return value

    End Function

End Class