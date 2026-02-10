Imports System.IO
Imports System.Xml

Public Class HardDrivesInfo
    Public strFreeSpace As String
    Public strTotalSpace As String
    Public strLabelVolume As String


    Private ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Public Function getHardDrivesInfo() As List(Of HardDrivesInfo)
        'Regresa la informacion de los discos duros, siempre que esten datos de alta en el arch
        Dim listDiscos As New List(Of HardDrivesInfo)
        For Each curDrive As DriveInfo In My.Computer.FileSystem.Drives
            If curDrive.DriveType = DriveType.Fixed Then
                If isHardDiskOnDefinition(curDrive.Name.ToString) Then
                    Dim theFreeSpace As Double = Math.Round(curDrive.TotalFreeSpace / 1073741824, 2)
                    Dim totalSpace As Double = Math.Round(curDrive.TotalSize / 1073741824, 2)
                    Dim HD As New HardDrivesInfo
                    listDiscos.Add(HD)
                    HD.strFreeSpace = theFreeSpace.ToString
                    HD.strTotalSpace = totalSpace.ToString
                    HD.strLabelVolume = Mid(curDrive.Name.ToString, 1, 1)
                End If
            End If
        Next
        Return listDiscos
    End Function

    Private Function isHardDiskOnDefinition(strHardDiskCPU) As Boolean
        Try
            Dim m_xmld As XmlDocument
            Dim m_nodelist As XmlNodeList
            Dim m_node As XmlNode
            m_xmld = New XmlDocument()
            m_xmld.Load(Constants.strXML_HD)
            m_nodelist = m_xmld.SelectNodes("/Hard_Drives/HD")
            'Obtiene del xml todos los procesos que tienen que estar ejecutandose
            For Each m_node In m_nodelist
                Dim strHardDiskDefinition As String = m_node.ChildNodes.Item(0).InnerText
                If Mid(strHardDiskCPU, 1, 1) = strHardDiskDefinition Then Return True
            Next
            Return False
        Catch ex As Exception
            log.Error("", ex)
            Return False
        End Try
    End Function
End Class
