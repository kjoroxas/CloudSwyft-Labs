using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace CloudSwyft.Web.Api.Models
{
    public class TimeSchedules
    {
        [Key]
        public int TimeScheduleId { get; set; }
        public int MachineLabsId { get; set; }
        public int VEProfileID { get; set; }
        public int UserId { get; set; }
        public string TimeZone { get; set; }
        public DateTime StartTime { get; set; }
        public int IdleTime { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime DateCreated { get; set; }
        public int ScheduledBy { get; set; }
        public DateTime DateModified { get; set; }
    }

    public class TimeSchedulesParam
    {
        public int TimeScheduleId { get; set; }
        public string Email { get; set; }
        public string CourseName { get; set; }
        public DateTime StartTime { get; set; }
        public string TimeZone { get; set; }
        public string CourseEmail { get; set; }
    }
    
}