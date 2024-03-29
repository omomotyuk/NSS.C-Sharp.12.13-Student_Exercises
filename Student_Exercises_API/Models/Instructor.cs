﻿using Student_Exercises_API.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Student_Exercises_API.Model
{
    public class Instructor: IRecord
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SlackHandle { get; set; }
        public int CohortId { get; set; }
        public string Speciality { get; set; }
        public Cohort Cohort { get; set; }

        public Instructor()
        {
            Cohort = new Cohort();
        }

        public void Set(Dictionary<string, string> data)
        {
            foreach (KeyValuePair<string, string> item in data)
            {
                switch (item.Key)
                {
                    case "Id":
                        Id = Int32.Parse(item.Value);
                        break;
                    case "FirstName":
                        FirstName = item.Value;
                        break;
                    case "LastName":
                        LastName = item.Value;
                        break;
                    case "SlackHandle":
                        SlackHandle = item.Value;
                        break;
                    case "CohortId":
                        CohortId = Int32.Parse(item.Value);
                        break;
                    case "Speciality":
                        Speciality = item.Value;
                        break;
                    /*case "[Name]":
                        CohortName = item.Value;
                        break;*/
                    default:
                        break;
                }
            }
        }

    }
}
