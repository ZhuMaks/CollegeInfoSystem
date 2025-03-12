using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Models;

public class Faculty
{
    public int FacultyID { get; set; }
    public string FacultyName { get; set; }

    public List<Group> Groups { get; set; }
}
