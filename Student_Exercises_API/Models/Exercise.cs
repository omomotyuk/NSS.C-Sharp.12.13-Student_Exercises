using Student_Exercises_API.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Student_Exercises_API.Model
{
    public class Exercise: IRecord
    {
        public int Id { get; set; }

        [Required (ErrorMessage = "Exercise Name is required")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Exercise Name should be between 2 and 10 characters")]
        public string Name { get; set; }
        public string Language { get; set; }

        public void Set(Dictionary<string, string> data)
        {
            foreach (KeyValuePair<string, string> item in data)
            {
                switch (item.Key)
                {
                    case "Id":
                        Id = Int32.Parse(item.Value);
                        break;
                    case "Name":
                        Name = item.Value;
                        break;
                    case "Language":
                        Language = item.Value;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
