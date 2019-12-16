using Student_Exercises_API.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Student_Exercises_API.Model
{
    class Record : IRecord
    {
        public Dictionary<string, string> fields = new Dictionary<string, string>();

        public void Set(Dictionary<string, string> data)
        {
            fields = data;

            /*foreach (KeyValuePair<string, string> item in data)
            {
            }*/
        }
    }
}
