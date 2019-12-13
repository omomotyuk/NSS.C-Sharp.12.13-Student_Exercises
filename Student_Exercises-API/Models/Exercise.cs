using Student_Exercises_ADO.NET.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Student_Exercises_ADO.NET.Model
{
    class Exercise: IRecord
    {
        public int Id { get; set; }
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
