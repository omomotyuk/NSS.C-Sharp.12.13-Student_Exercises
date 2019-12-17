using System;
using System.Collections.Generic;
using System.Text;

namespace Student_Exercises_API.Model
{
    public class Cohort
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Instructor> Instructors { get; set; } = new List<Instructor>();
        public List<Student> Students { get; set; } = new List<Student>();
    }
}
