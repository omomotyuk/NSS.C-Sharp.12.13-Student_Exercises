using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_Exercises_API.Models
{
    public class CohortStudentsInstructors
    {
        public string CohortId { get; set; }
        public string CohortName { get; set; }
        public string StudentId { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentSlackHandle { get; set; }
        public string StudentCohortId { get; set; }
        public string ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public string Language { get; set; }
        public string InstructorId { get; set; }
        public string InstructorFirstName { get; set; }
        public string InstructorLastName { get; set; }
        public string InstructorSlackHandle { get; set; }
        public string InstructorCohortId { get; set; }
    }
}
