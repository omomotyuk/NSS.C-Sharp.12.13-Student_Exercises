﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using Student_Exercises_API.Model;
using Microsoft.AspNetCore.Http;
using Student_Exercises_API.Models;

namespace Student_Exercises_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CohortsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CohortsController(IConfiguration config)
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

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT c.Id AS CohortId, c.Name AS CohortName, 
                               s.Id AS StudentId, s.FirstName AS StudentFirstName, s.LastName AS StudentLastName, s.SlackHandle AS StudentSlackHandle, s.CohortId AS StudentCohortId, 
                               e.Id AS ExerciseId, e.Name AS ExerciseName, e.Language,
                               i.Id AS InstructorId, i.FirstName AS InstructorFirstName, i.LastName AS InstructorLastName, i.SlackHandle AS InstructorSlackHandle, i.CohortId AS InstructorCohortId
                        FROM Cohort c
                        INNER JOIN Student s ON c.Id = s.CohortId
                        LEFT JOIN StudentExercise se ON s.Id = se.StudentId
                        LEFT JOIN Exercise e ON e.Id = se.ExerciseId
                        LEFT JOIN Instructor i ON c.Id = i.CohortId";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Cohort> cohorts = new List<Cohort>();
                    while (reader.Read())
                    {
                        var cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        var cohortAlreadyAdded = cohorts.FirstOrDefault(c => c.Id == cohortId);

                        if (cohortAlreadyAdded == null)
                        {
                            Cohort cohort = new Cohort
                            {
                                Id = cohortId,
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            };

                            var hasStudent = !reader.IsDBNull(reader.GetOrdinal("StudentId"));

                            if (hasStudent)
                            {
                                var student = new Student()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohortId"))
                                };

                                if (!cohort.Students.Contains(student))
                                {
                                    cohort.Students.Add( student );
                                }

                                var exerciseId = reader.GetInt32(reader.GetOrdinal("ExerciseId"));
                                var exerciseName = reader.GetString(reader.GetOrdinal("ExerciseName"));
                                var exerciseLanguage = reader.GetString(reader.GetOrdinal("Language"));
                            }

                            var hasInstructor = !reader.IsDBNull(reader.GetOrdinal("InstructorId"));

                            if (hasInstructor)
                            {
                                Instructor instructor = new Instructor()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("InstructorCohortId"))
                                };

                                if( !cohort.Instructors.Exists( i => i.Id == instructor.Id ))
                                {
                                    cohort.Instructors.Add( instructor );
                                }
                            }

                            cohorts.Add(cohort);
                        }
                        else
                        {
                            var hasStudent = !reader.IsDBNull(reader.GetOrdinal("StudentId"));

                            if (hasStudent)
                            {
                                Student student = new Student()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohortId"))
                                };
                                
                                if( !cohortAlreadyAdded.Students.Exists(s => s.Id == student.Id))
                                {
                                    cohortAlreadyAdded.Students.Add( student );
                                }

                                var exerciseId = reader.GetInt32(reader.GetOrdinal("ExerciseId"));
                                var exerciseName = reader.GetString(reader.GetOrdinal("ExerciseName"));
                                var exerciseLanguage = reader.GetString(reader.GetOrdinal("Language"));
                            }

                            var hasInstructor = !reader.IsDBNull(reader.GetOrdinal("InstructorId"));

                            if (hasInstructor)
                            {
                                Instructor instructor = new Instructor()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("InstructorCohortId"))
                                };

                                if( !cohortAlreadyAdded.Instructors.Exists(i => i.Id == instructor.Id))
                                {
                                    cohortAlreadyAdded.Instructors.Add( instructor );
                                }
                            }
                        }
                    }
                    reader.Close();

                    return Ok(cohorts);
                }
            }
        }

        /*[HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT i.Id, i.FirstName, i.LastName, i.SlackHandle, i.CohortId as CohortCohortId, i.Speciality,
                               c.Id as CohortId, c.Name
                        FROM Cohort i
                        LEFT JOIN Cohort c
                        ON i.CohortId = c.Id
                        WHERE i.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Cohort cohort = null;

                    if (reader.Read())
                    {
                        cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortCohortId")),
                            Speciality = reader.GetString(reader.GetOrdinal("Speciality")),
                            Cohort = new Cohort()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        };
                    }
                    reader.Close();

                    return Ok(cohort);
                }
            }
        }

        [HttpGet]
        [Route("GetCohortsByCohortIdAndLastname")]
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
                                        CohortId,
                                        Speciality
                                        FROM Cohort
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
                    List<Cohort> cohorts = new List<Cohort>();
                    
                    while (reader.Read())
                    {
                        Cohort cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Speciality = reader.GetString(reader.GetOrdinal("Speciality")),
                        };
                        
                        cohorts.Add(cohort);
                    }
                    reader.Close();

                    return Ok(cohorts);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Cohort cohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Cohort (FirstName, LastName, SlackHandle, CohortId, Speciality)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstName, @lastName, @slackHandle, @cohortId, @speciality)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", cohort.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", cohort.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", cohort.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", cohort.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@speciality", cohort.Speciality));

                    int newId = (int) await cmd.ExecuteScalarAsync();
                    cohort.Id = newId;
                    return CreatedAtRoute("GetCohort", new { id = newId }, cohort);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Cohort cohort)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Cohort
                                            SET FirstName = @firstName,
                                                LastName = @lastName,
                                                SlackHandle = @slackHandle,
                                                CohortId = @cohortId,
                                                Speciality = @speciality,
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", cohort.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", cohort.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", cohort.SlackHandle)); ;
                        cmd.Parameters.Add(new SqlParameter("@cohortId", cohort.CohortId)); ;
                        cmd.Parameters.Add(new SqlParameter("@speciality", cohort.Speciality)); ;
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
                bool exists = await CohortExists(id);
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
                        cmd.CommandText = @"DELETE FROM Cohort WHERE Id = @id";
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
                bool exists = await CohortExists(id);
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

        private async Task<bool> CohortExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, SlackHandle, CohortId, Speciality
                        FROM Cohort
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    return reader.Read();
                }
            }
        }*/
    }
}