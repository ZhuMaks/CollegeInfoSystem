using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Models;

public class Schedule
{
    public int ScheduleID { get; set; }
    public int GroupID { get; set; }
    public int TeacherID { get; set; }
    public string Subject { get; set; }
    public string DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Room { get; set; }

    public Group Group { get; set; }
    public Teacher Teacher { get; set; }
}

