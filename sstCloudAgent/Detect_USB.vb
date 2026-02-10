Imports System.Management
Imports sstInterface.ComClass1

Public Class Detect_USB
    Private WithEvents m_MediaConnectWatcher As ManagementEventWatcher
    Public USBDriveName As String
    Public USBDriveLetter As String
    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Public Sub StartDetection()
        ' __InstanceOperationEvent will trap both Creation and Deletion of class instances
        log.Info("Detect_USB -  Start Detection")
        Try
            'Dim query2 As New WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent'")
            Dim query2 As New WqlEventQuery("SELECT * FROM __InstanceOperationEvent WITHIN 1 " & "WHERE TargetInstance ISA 'Win32_DiskDrive'")

            m_MediaConnectWatcher = New ManagementEventWatcher
            m_MediaConnectWatcher.Query = query2
            m_MediaConnectWatcher.Start()
        Catch ex As Exception
            log.Error("", ex)
        End Try

    End Sub


    Private Sub Arrived(ByVal sender As Object, ByVal e As System.Management.EventArrivedEventArgs) Handles m_MediaConnectWatcher.EventArrived
        log.Debug("USB -  Arrived")
        Try
            Dim mbo, obj As ManagementBaseObject

            ' the first thing we have to do is figure out if this is a creation or deletion event
            mbo = CType(e.NewEvent, ManagementBaseObject)
            ' next we need a copy of the instance that was either created or deleted
            obj = CType(mbo("TargetInstance"), ManagementBaseObject)
            log.Debug("mbo.ClassPath.ClassName: " & mbo.ClassPath.ClassName)
            log.Debug(" obj('InterfaceType') : " & obj("InterfaceType"))
            Select Case mbo.ClassPath.ClassName
                Case "__InstanceCreationEvent"
                    If obj("InterfaceType") = "USB" Then
                        Dim strName As String = GetDriveLetterFromDisk(obj("Name")) & " : " & obj("Caption")
                        log.Info("Se conecto USB: " & strName)
                        log.Debug("Name: " & GetDriveLetterFromDisk(obj("Name")))
                        log.Debug("Caption: " & obj("Caption"))
                        log.Debug("Se conecto USB: " & strName)
                        oApi.ReportAlert("900", "", strName.ToString)
                    Else
                        log.Debug("Conectaron otro tipo de dispositivo: " & obj("InterfaceType"))
                    End If
                Case "__InstanceDeletionEvent"
                    If obj("InterfaceType") = "USB" Then
                        log.Debug("Se desconecto USB Caption: " & obj("Caption"))
                        Dim strName As String = obj("Caption")
                        oApi.ReportAlert("901", "", strName.ToString)

                    Else
                        log.Debug("Desconectaron otro tipo de dispositivo: " & obj("InterfaceType"))
                    End If
                Case Else
            End Select
        Catch ex As Exception
            log.Error("", ex)
        End Try
    End Sub

    Private Function GetDriveLetterFromDisk(ByVal Name As String) As String
        Dim oq_part, oq_disk As ObjectQuery
        Dim mos_part, mos_disk As ManagementObjectSearcher
        Dim obj_part, obj_disk As ManagementObject
        Dim ans As String = ""

        Try
            ' WMI queries use the "\" as an escape charcter
            Name = Replace(Name, "\", "\\")
            ' First we map the Win32_DiskDrive instance with the association called
            ' Win32_DiskDriveToDiskPartition. Then we map the Win23_DiskPartion
            ' instance with the assocation called Win32_LogicalDiskToPartition

            oq_part = New ObjectQuery("ASSOCIATORS OF {Win32_DiskDrive.DeviceID=""" & Name & """} WHERE AssocClass = Win32_DiskDriveToDiskPartition")
            mos_part = New ManagementObjectSearcher(oq_part)
            For Each obj_part In mos_part.Get()

                oq_disk = New ObjectQuery("ASSOCIATORS OF {Win32_DiskPartition.DeviceID=""" & obj_part("DeviceID") & """} WHERE AssocClass = Win32_LogicalDiskToPartition")
                mos_disk = New ManagementObjectSearcher(oq_disk)
                For Each obj_disk In mos_disk.Get()
                    ans &= obj_disk("Name") & ","
                Next
            Next

            Return ans.Trim(","c)
        Catch ex As Exception
            log.Error("", ex)
            Return ""
        End Try
    End Function
    Public Sub stopMonitoring()
        m_MediaConnectWatcher.Stop()
    End Sub

    Public Sub New()
        StartDetection()
    End Sub
End Class
