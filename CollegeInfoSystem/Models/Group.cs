using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Models;

public class Group
{
    public int GroupID { get; set; }
    public string GroupName { get; set; }
    public int FacultyID { get; set; }
    public int? CuratorID { get; set; }

    public Faculty Faculty { get; set; }
    public Teacher Curator { get; set; }
    public List<Student> Students { get; set; }
}

