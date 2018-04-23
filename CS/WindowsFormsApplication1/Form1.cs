using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraScheduler;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static Random RandomInstance = new Random();

        private BindingList<CustomResource> CustomResourceCollection = new BindingList<CustomResource>();
        private BindingList<CustomAppointment> CustomEventList = new BindingList<CustomAppointment>();

        private void Form1_Load(object sender, EventArgs e)
        {
            
            InitResources();
            InitAppointments();
            schedulerControl1.Start = DateTime.Now;
            schedulerControl1.GroupType = DevExpress.XtraScheduler.SchedulerGroupType.Resource;
        }

        private void InitResources()
        {
            ResourceMappingInfo mappings = this.schedulerStorage1.Resources.Mappings;
            mappings.Id = "ResID";
            mappings.Caption = "Name";

            CustomResourceCollection.Add(CreateCustomResource(1, "Max Fowler", Color.PowderBlue));
            CustomResourceCollection.Add(CreateCustomResource(2, "Nancy Drewmore", Color.PaleVioletRed));
            CustomResourceCollection.Add(CreateCustomResource(3, "Pak Jang", Color.PeachPuff));
            this.schedulerStorage1.Resources.DataSource = CustomResourceCollection;
        }

        private CustomResource CreateCustomResource(int res_id, string caption, Color ResColor)
        {
            CustomResource cr = new CustomResource();
            cr.ResID = res_id;
            cr.Name = caption;
            return cr;
        }



        private void InitAppointments()
        {
            AppointmentMappingInfo mappings = this.schedulerStorage1.Appointments.Mappings;
            mappings.Start = "StartTime";
            mappings.End = "EndTime";
            mappings.Subject = "Subject";
            mappings.AllDay = "AllDay";
            mappings.Description = "Description";
            mappings.Label = "Label";
            mappings.Location = "Location";
            mappings.RecurrenceInfo = "RecurrenceInfo";
            mappings.ReminderInfo = "ReminderInfo";
            mappings.ResourceId = "OwnerId";
            mappings.Status = "Status";
            mappings.Type = "EventType";

            //GenerateEvents(CustomEventList);
            GenerateEvents1stIssue(CustomEventList);
            this.schedulerStorage1.Appointments.DataSource = CustomEventList;
        }


        private void GenerateEvents(BindingList<CustomAppointment> eventList)
        {
            int count = schedulerStorage1.Resources.Count;

            for (int i = 0; i < count; i++)
            {
                Resource resource = schedulerStorage1.Resources[i];
                string subjPrefix = resource.Caption + "'s ";
                eventList.Add(CreateEvent(subjPrefix + "meeting", resource.Id, 2, 5, 2));
                eventList.Add(CreateEvent(subjPrefix + "travel", resource.Id, 3, 6, 6));
                eventList.Add(CreateEvent(subjPrefix + "phone call", resource.Id, 0, 10, 10));
            }
        }

        private void GenerateEvents1stIssue(BindingList<CustomAppointment> eventList) {
            int count = schedulerStorage1.Resources.Count;

            Resource resource = schedulerStorage1.Resources[0];
            string subjPrefix = resource.Caption + "'s ";
            eventList.Add(CreateEvent("1", resource.Id, 2, 5, DateTime.Now.Date, DateTime.Now.Date.AddHours(1)));
            eventList.Add(CreateEvent("2", resource.Id, 3, 6, DateTime.Now.Date.AddHours(1), DateTime.Now.Date.AddHours(1).AddMinutes(30)));
            eventList.Add(CreateEvent("3", resource.Id, 0, 10, DateTime.Now.Date.AddHours(1).AddMinutes(30), DateTime.Now.Date.AddHours(2)));

            eventList.Add(CreateEvent("4", resource.Id, 2, 5, DateTime.Now.Date.AddHours(2), DateTime.Now.Date.AddHours(3)));
            eventList.Add(CreateEvent("5", resource.Id, 3, 6, DateTime.Now.Date.AddHours(3), DateTime.Now.Date.AddHours(5).AddMinutes(30)));
            eventList.Add(CreateEvent("6", resource.Id, 0, 10, DateTime.Now.Date.AddHours(5).AddMinutes(30), DateTime.Now.Date.AddHours(7).AddMinutes(30)));

            eventList.Add(CreateEvent("7", resource.Id, 2, 5, DateTime.Now.Date.AddHours(7).AddMinutes(30), DateTime.Now.Date.AddHours(9).AddMinutes(30)));
            eventList.Add(CreateEvent("8", resource.Id, 3, 6, DateTime.Now.Date.AddHours(9).AddMinutes(30), DateTime.Now.Date.AddHours(10).AddMinutes(30)));
            eventList.Add(CreateEvent("9", resource.Id, 0, 10, DateTime.Now.Date.AddHours(10).AddMinutes(30), DateTime.Now.Date.AddHours(11).AddMinutes(30)));

            eventList.Add(CreateEvent("10", resource.Id, 0, 10, DateTime.Now.Date.AddHours(11).AddMinutes(30), DateTime.Now.Date.AddHours(12).AddMinutes(30)));
        }

        private CustomAppointment CreateEvent(string subject, object resourceId, int status, int label, int hoursShift)
        {
            CustomAppointment apt = new CustomAppointment();
            apt.Subject = subject;
            apt.OwnerId = resourceId;
            apt.StartTime = DateTime.Today.AddHours(hoursShift);
            apt.EndTime = apt.StartTime.AddHours(2);
            apt.Status = status;
            apt.Label = label;
            return apt;
        }

        private CustomAppointment CreateEvent(string subject, object resourceId, int status, int label, DateTime start, DateTime end) {
            CustomAppointment apt = new CustomAppointment();
            apt.Subject = subject;
            apt.OwnerId = resourceId;
            apt.StartTime = start;
            apt.EndTime = end;
            apt.Status = status;
            apt.Label = label;
            return apt;
        }

        private void schedulerControl1_QueryWorkTime(object sender, QueryWorkTimeEventArgs e)
        {

        }

        private void schedulerControl1_AppointmentDrag(object sender, AppointmentDragEventArgs e)
        {

        }

        AppointmentsComparer comparer = new AppointmentsComparer();
        private void schedulerControl1_AppointmentDrop(object sender, AppointmentDragEventArgs e)
        {
            BeginInvoke(new MethodInvoker(delegate {
                TimeInterval dragAppointmentInterval = new TimeInterval(e.SourceAppointment.Start, e.SourceAppointment.End);
                ReOrderOtherAppointments(e.SourceAppointment.ResourceId, dragAppointmentInterval, e.SourceAppointment);            
            }));
        }

        void ReOrderOtherAppointments(object resourceID, TimeInterval dragAppointmentInterval, Appointment currentApt)
        {
            schedulerControl1.BeginUpdate();
            AppointmentBaseCollection appointments = schedulerStorage1.GetAppointments(dragAppointmentInterval);
            appointments.Sort(comparer);
            foreach (Appointment apt in appointments)
            {
                if (Convert.ToInt32(apt.ResourceId) == Convert.ToInt32(resourceID) && apt != currentApt)
                {  
                    if (apt.Start >= currentApt.Start)
                        MoveOtherAppointments(apt.ResourceId, dragAppointmentInterval, apt, true);
                    else
                        MoveOtherAppointments(apt.ResourceId, dragAppointmentInterval, apt, false);
                }
            }
            schedulerControl1.EndUpdate();
        }

        void MoveOtherAppointments(object resourceID, TimeInterval dragAppointmentInterval, Appointment currentApt, bool shiftDown) {
            // change current appointments settings 
            TimeSpan currentDuration = currentApt.Duration;
            if(shiftDown) {
                currentApt.Start = dragAppointmentInterval.End;
                currentApt.End = currentApt.Start + currentDuration;
            }
            else {
                currentApt.Start = dragAppointmentInterval.Start - currentDuration;
                currentApt.End = dragAppointmentInterval.Start;
            }

            TimeInterval newDragInterval = new TimeInterval(currentApt.Start, currentApt.End);

            // move other appointments
            AppointmentBaseCollection appointments = schedulerStorage1.GetAppointments(newDragInterval);
            appointments.Sort(comparer);
            foreach(Appointment apt in appointments) {
                if(Convert.ToInt32(apt.ResourceId) == Convert.ToInt32(resourceID) && apt != currentApt) {
                    MoveOtherAppointments(apt.ResourceId, newDragInterval, apt, shiftDown);
                }
            }
        }
    }

    public class AppointmentsComparer : IComparer<Appointment> {

        #region IComparer<Appointment> Members

        public int Compare(Appointment x, Appointment y) {
            return y.Start.CompareTo(x.Start);
        }

        #endregion
    }
}
