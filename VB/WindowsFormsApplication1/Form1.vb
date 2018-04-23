Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports DevExpress.XtraScheduler

Namespace WindowsFormsApplication1
	Partial Public Class Form1
		Inherits Form
		Public Sub New()
			InitializeComponent()
		End Sub

		Public Shared RandomInstance As New Random()

		Private CustomResourceCollection As New BindingList(Of CustomResource)()
		Private CustomEventList As New BindingList(Of CustomAppointment)()

		Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load

			InitResources()
			InitAppointments()
			schedulerControl1.Start = DateTime.Now
			schedulerControl1.GroupType = DevExpress.XtraScheduler.SchedulerGroupType.Resource
		End Sub

		Private Sub InitResources()
			Dim mappings As ResourceMappingInfo = Me.schedulerStorage1.Resources.Mappings
			mappings.Id = "ResID"
			mappings.Caption = "Name"

			CustomResourceCollection.Add(CreateCustomResource(1, "Max Fowler", Color.PowderBlue))
			CustomResourceCollection.Add(CreateCustomResource(2, "Nancy Drewmore", Color.PaleVioletRed))
			CustomResourceCollection.Add(CreateCustomResource(3, "Pak Jang", Color.PeachPuff))
			Me.schedulerStorage1.Resources.DataSource = CustomResourceCollection
		End Sub

		Private Function CreateCustomResource(ByVal res_id As Integer, ByVal caption As String, ByVal ResColor As Color) As CustomResource
			Dim cr As New CustomResource()
			cr.ResID = res_id
			cr.Name = caption
			Return cr
		End Function



		Private Sub InitAppointments()
			Dim mappings As AppointmentMappingInfo = Me.schedulerStorage1.Appointments.Mappings
			mappings.Start = "StartTime"
			mappings.End = "EndTime"
			mappings.Subject = "Subject"
			mappings.AllDay = "AllDay"
			mappings.Description = "Description"
			mappings.Label = "Label"
			mappings.Location = "Location"
			mappings.RecurrenceInfo = "RecurrenceInfo"
			mappings.ReminderInfo = "ReminderInfo"
			mappings.ResourceId = "OwnerId"
			mappings.Status = "Status"
			mappings.Type = "EventType"

			'GenerateEvents(CustomEventList);
			GenerateEvents1stIssue(CustomEventList)
			Me.schedulerStorage1.Appointments.DataSource = CustomEventList
		End Sub


		Private Sub GenerateEvents(ByVal eventList As BindingList(Of CustomAppointment))
			Dim count As Integer = schedulerStorage1.Resources.Count

			For i As Integer = 0 To count - 1
				Dim resource As Resource = schedulerStorage1.Resources(i)
				Dim subjPrefix As String = resource.Caption & "'s "
				eventList.Add(CreateEvent(subjPrefix & "meeting", resource.Id, 2, 5, 2))
				eventList.Add(CreateEvent(subjPrefix & "travel", resource.Id, 3, 6, 6))
				eventList.Add(CreateEvent(subjPrefix & "phone call", resource.Id, 0, 10, 10))
			Next i
		End Sub

		Private Sub GenerateEvents1stIssue(ByVal eventList As BindingList(Of CustomAppointment))
			Dim count As Integer = schedulerStorage1.Resources.Count

			Dim resource As Resource = schedulerStorage1.Resources(0)
			Dim subjPrefix As String = resource.Caption & "'s "
			eventList.Add(CreateEvent("1", resource.Id, 2, 5, DateTime.Now.Date, DateTime.Now.Date.AddHours(1)))
			eventList.Add(CreateEvent("2", resource.Id, 3, 6, DateTime.Now.Date.AddHours(1), DateTime.Now.Date.AddHours(1).AddMinutes(30)))
			eventList.Add(CreateEvent("3", resource.Id, 0, 10, DateTime.Now.Date.AddHours(1).AddMinutes(30), DateTime.Now.Date.AddHours(2)))

			eventList.Add(CreateEvent("4", resource.Id, 2, 5, DateTime.Now.Date.AddHours(2), DateTime.Now.Date.AddHours(3)))
			eventList.Add(CreateEvent("5", resource.Id, 3, 6, DateTime.Now.Date.AddHours(3), DateTime.Now.Date.AddHours(5).AddMinutes(30)))
			eventList.Add(CreateEvent("6", resource.Id, 0, 10, DateTime.Now.Date.AddHours(5).AddMinutes(30), DateTime.Now.Date.AddHours(7).AddMinutes(30)))

			eventList.Add(CreateEvent("7", resource.Id, 2, 5, DateTime.Now.Date.AddHours(7).AddMinutes(30), DateTime.Now.Date.AddHours(9).AddMinutes(30)))
			eventList.Add(CreateEvent("8", resource.Id, 3, 6, DateTime.Now.Date.AddHours(9).AddMinutes(30), DateTime.Now.Date.AddHours(10).AddMinutes(30)))
			eventList.Add(CreateEvent("9", resource.Id, 0, 10, DateTime.Now.Date.AddHours(10).AddMinutes(30), DateTime.Now.Date.AddHours(11).AddMinutes(30)))

			eventList.Add(CreateEvent("10", resource.Id, 0, 10, DateTime.Now.Date.AddHours(11).AddMinutes(30), DateTime.Now.Date.AddHours(12).AddMinutes(30)))
		End Sub

		Private Function CreateEvent(ByVal subject As String, ByVal resourceId As Object, ByVal status As Integer, ByVal label As Integer, ByVal hoursShift As Integer) As CustomAppointment
			Dim apt As New CustomAppointment()
			apt.Subject = subject
			apt.OwnerId = resourceId
			apt.StartTime = DateTime.Today.AddHours(hoursShift)
			apt.EndTime = apt.StartTime.AddHours(2)
			apt.Status = status
			apt.Label = label
			Return apt
		End Function

		Private Function CreateEvent(ByVal subject As String, ByVal resourceId As Object, ByVal status As Integer, ByVal label As Integer, ByVal start As DateTime, ByVal [end] As DateTime) As CustomAppointment
			Dim apt As New CustomAppointment()
			apt.Subject = subject
			apt.OwnerId = resourceId
			apt.StartTime = start
			apt.EndTime = [end]
			apt.Status = status
			apt.Label = label
			Return apt
		End Function

		Private Sub schedulerControl1_QueryWorkTime(ByVal sender As Object, ByVal e As QueryWorkTimeEventArgs)

		End Sub

		Private Sub schedulerControl1_AppointmentDrag(ByVal sender As Object, ByVal e As AppointmentDragEventArgs) Handles schedulerControl1.AppointmentDrag

		End Sub

		Private comparer As New AppointmentsComparer()
		Private Sub schedulerControl1_AppointmentDrop(ByVal sender As Object, ByVal e As AppointmentDragEventArgs) Handles schedulerControl1.AppointmentDrop
            BeginInvoke(New MethodInvoker(Function() AsynchronousAppointmentDropMethod(e)))
		End Sub
		
        Private Function AsynchronousAppointmentDropMethod(ByVal e As AppointmentDragEventArgs) As Boolean
            Dim dragAppointmentInterval As New TimeInterval(e.SourceAppointment.Start, e.SourceAppointment.End)
            ReOrderOtherAppointments(e.SourceAppointment.ResourceId, dragAppointmentInterval, e.SourceAppointment)
            Return True
        End Function

		Private Sub ReOrderOtherAppointments(ByVal resourceID As Object, ByVal dragAppointmentInterval As TimeInterval, ByVal currentApt As Appointment)
			schedulerControl1.BeginUpdate()
			Dim appointments As AppointmentBaseCollection = schedulerStorage1.GetAppointments(dragAppointmentInterval)
			appointments.Sort(comparer)
			For Each apt As Appointment In appointments
				If Convert.ToInt32(apt.ResourceId) = Convert.ToInt32(resourceID) AndAlso apt IsNot currentApt Then
					If apt.Start >= currentApt.Start Then
						MoveOtherAppointments(apt.ResourceId, dragAppointmentInterval, apt, True)
					Else
						MoveOtherAppointments(apt.ResourceId, dragAppointmentInterval, apt, False)
					End If
				End If
			Next apt
			schedulerControl1.EndUpdate()
		End Sub

		Private Sub MoveOtherAppointments(ByVal resourceID As Object, ByVal dragAppointmentInterval As TimeInterval, ByVal currentApt As Appointment, ByVal shiftDown As Boolean)
			' change current appointments settings 
			Dim currentDuration As TimeSpan = currentApt.Duration
			If shiftDown Then
				currentApt.Start = dragAppointmentInterval.End
				currentApt.End = currentApt.Start + currentDuration
			Else
				currentApt.Start = dragAppointmentInterval.Start - currentDuration
				currentApt.End = dragAppointmentInterval.Start
			End If

			Dim newDragInterval As New TimeInterval(currentApt.Start, currentApt.End)

			' move other appointments
			Dim appointments As AppointmentBaseCollection = schedulerStorage1.GetAppointments(newDragInterval)
			appointments.Sort(comparer)
			For Each apt As Appointment In appointments
				If Convert.ToInt32(apt.ResourceId) = Convert.ToInt32(resourceID) AndAlso apt IsNot currentApt Then
					MoveOtherAppointments(apt.ResourceId, newDragInterval, apt, shiftDown)
				End If
			Next apt
		End Sub
	End Class

	Public Class AppointmentsComparer
		Implements IComparer(Of Appointment)

		#Region "IComparer<Appointment> Members"

		Public Function Compare(ByVal x As Appointment, ByVal y As Appointment) As Integer Implements IComparer(Of Appointment).Compare
			Return y.Start.CompareTo(x.Start)
		End Function

		#End Region
	End Class
End Namespace
