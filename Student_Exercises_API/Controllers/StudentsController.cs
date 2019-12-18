using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using Student_Exercises_API.Model;
using Microsoft.AspNetCore.Http;

namespace Student_Exercises_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        //[HttpGet]
        //[Route("GetAllStudents")]
        public async Task<IActionResult> GetAllStudents()
        //public async Task<List<Student>> GetAllStudents()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, FirstName, LastName, SlackHandle, CohortId FROM Student";
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Student> students = new List<Student>();

                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };

                        students.Add(student);
                    }
                    reader.Close();

                    return Ok(students);
                    //return students;
                }
            }
        }

        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> Get([FromQuery] string include)
        {
            if (include == "exercises")
            {
                var students = await GetStudentWithExercises();
                return Ok(students);
            }
            else if ( include == "lastname" )          {
                var students = await GetStudentByLastName( include );
                return Ok(students);
            }
           else
            {
                var students = await GetAllStudents();
                return Ok( students );
            }
        }

        //[HttpGet]
        //[Route("GetStudentWithExercises")]
        public async Task<IActionResult> GetStudentWithExercises()
        //public async Task<List<Student>> GetStudentWithExercises()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId,
                               e.Id AS ExerciseId, e.[Name], e.[Language]
                        FROM Student s
                        LEFT JOIN StudentExercise se
                        ON s.Id = se.StudentId
                        JOIN Exercise e
                        ON se.ExerciseId = e.Id
                        ";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Student> students = new List<Student>();

                    int StudentIdOrdinal = reader.GetOrdinal("Id");
                    int StudentFirstNameOrdinal = reader.GetOrdinal("FirstName");
                    int StudentLastNameOrdinal = reader.GetOrdinal("LastName");
                    int StudentSlackHandleOrdinal = reader.GetOrdinal("SlackHandle");
                    int StudentCohortIdOrdinal = reader.GetOrdinal("CohortId");
                    int ExerciseIdOrdinal = reader.GetOrdinal("ExerciseId");
                    int ExerciseNameOrdinal = reader.GetOrdinal("Name");
                    int ExerciseLanguageOrdinal = reader.GetOrdinal("Language");

                    while (reader.Read())
                    {
                        //
                        var studentId = reader.GetInt32(StudentIdOrdinal);
                        var studentAlreadyAdded = students.FirstOrDefault(s => s.Id == studentId);

                        if (studentAlreadyAdded == null)
                        {
                            Student student = new Student
                            {
                                Id = studentId,
                                FirstName = reader.GetString(StudentFirstNameOrdinal),
                                LastName = reader.GetString(StudentLastNameOrdinal),
                                SlackHandle = reader.GetString(StudentSlackHandleOrdinal),
                                CohortId = reader.GetInt32(StudentCohortIdOrdinal),
                                Exercises = new List<Exercise>()
                            };

                            var hasExercise = !reader.IsDBNull(ExerciseIdOrdinal);

                            if (hasExercise)
                            {
                                student.Exercises.Add(new Exercise()
                                {
                                    Id = reader.GetInt32(ExerciseIdOrdinal),
                                    Name = reader.GetString(ExerciseNameOrdinal),
                                    Language = reader.GetString(ExerciseLanguageOrdinal),
                                });
                            }

                            students.Add(student);
                        }
                        else
                        {
                            var hasExercise = !reader.IsDBNull(ExerciseIdOrdinal);

                            if (hasExercise)
                            {
                                Exercise exercise = new Exercise()
                                {
                                    Id = reader.GetInt32(ExerciseIdOrdinal),
                                    Name = reader.GetString(ExerciseNameOrdinal),
                                    Language = reader.GetString(ExerciseLanguageOrdinal),
                                };

                                if (!studentAlreadyAdded.Exercises.Exists(e => e.Id == exercise.Id))
                                {
                                    studentAlreadyAdded.Exercises.Add(exercise);
                                }
                            }
                        }
                    }

                    reader.Close();

                    return Ok(students);
                    //return students;
                }
            }
        }

        /**/[HttpGet]
        public async Task<List<Student>> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, SlackHandle, CohortId
                        FROM Student";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Student> students = new List<Student>();

                    while ( reader.Read() )
                    {

                    }

                    return students;
                }
            }
        }/**/

        //
        //
        /**/
        //[HttpGet("{q}", Name = "GetStudentAndCohortByLastname")]
        //[HttpGet]
        //[Route("GetStudentAndCohortByLastname")]
        //public async Task<IActionResult> GetStudentByLastName([FromQuery] string q)
        public async Task<IActionResult> GetStudentByLastName( string q )
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId as StudentCohortId, 
                                               c.Id, c.Name
                                        FROM Student s
                                        LEFT JOIN Cohort c 
                                        ON s.CohortId = c.Id
                                        WHERE s.LastName LIKE @q";

                    cmd.Parameters.Add(new SqlParameter("@q", "%" + q + "%"));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Student> students = new List<Student>();
                    while (reader.Read())
                    {
                        var studentId = reader.GetInt32(reader.GetOrdinal("Id"));
                        var studentAlreadyAdded = students.FirstOrDefault(s => s.Id == studentId);

                        if (studentAlreadyAdded == null)
                        {
                            Student student = new Student
                            {
                                Id = studentId,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Exercises = new List<Exercise>()
                            };
                            students.Add(student);

                            var hasEmployee = !reader.IsDBNull(reader.GetOrdinal("ExerciseId"));

                            if (hasEmployee)
                            {
                                student.Exercises.Add(new Exercise()
                                {
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Language = reader.GetString(reader.GetOrdinal("Language")),
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId"))
                                });
                            }
                        }
                        else
                        {
                            var hasEmployee = !reader.IsDBNull(reader.GetOrdinal("ExerciseId"));

                            if (hasEmployee)
                            {
                                studentAlreadyAdded.Exercises.Add(new Exercise()
                                {
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Language = reader.GetString(reader.GetOrdinal("Language")),
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId"))
                                });
                            }
                        }
                    }
                    reader.Close();

                    return Ok(students);
                }
            }
        }/**/
        //
        //

        //[HttpGet("{id}")] //, Name = "GetStudentAndCohortByStudentId")]
        [HttpGet("{id}")] //, Name = "GetStudentAndCohortByStudentId")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId as StudentCohortId,
                               c.Id as CohortId, c.Name
                        FROM Student s
                        LEFT JOIN Cohort c
                        ON s.CohortId = c.Id
                        WHERE s.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Student student = null;

                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohortId")),
                            Cohort = new Cohort()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        };
                    }
                    reader.Close();

                    return Ok(student);
                }
            }
        }

        [HttpGet]
        [Route("GetStudentsByCohortIdAndLastname")]
        public async Task<IActionResult> Get([FromQuery] int? cohortId, [FromQuery] string lastName)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT 
                                        Id, 
                                        FirstName, 
                                        LastName, 
                                        SlackHandle, 
                                        CohortId 
                                        FROM Student
                                        WHERE 1=1";
                    if (cohortId != null)
                    {
                        cmd.CommandText += " AND CohortId = @cohortId";
                        cmd.Parameters.Add(new SqlParameter("@cohortId", cohortId));
                    }
                    if (lastName != null)
                    {
                        cmd.CommandText += " AND LastName LIKE @lastName";
                        cmd.Parameters.Add(new SqlParameter("@lastName", "%" + lastName + "%"));
                    }

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Student> students = new List<Student>();
                    
                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };
                        
                        students.Add(student);
                    }
                    reader.Close();

                    return Ok(students);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student student)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Student (FirstName, LastName, SlackHandle, CohortId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstName, @lastName, @slackHandle, @cohortId)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", student.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", student.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));

                    int newId = (int) await cmd.ExecuteScalarAsync();
                    student.Id = newId;
                    return CreatedAtRoute("GetStudent", new { id = newId }, student);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Student student)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Student
                                            SET FirstName = @firstName,
                                                LastName = @lastName,
                                                SlackHandle = @slackHandle,
                                                CohortId = @cohortId,
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", student.SlackHandle)); ;
                        cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId)); ;
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                bool exists = await StudentExists(id);
                if (!exists)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Student WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                bool exists = await StudentExists(id);
                if ( !exists )
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<bool> StudentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, SlackHandle, CohortId
                        FROM Student
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    return reader.Read();
                }
            }
        }
    }
}