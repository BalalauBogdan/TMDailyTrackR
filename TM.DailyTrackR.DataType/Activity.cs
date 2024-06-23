using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM.DailyTrackR.DataType
{
    public class Activity
    {
        public int No { get; set; }
        public string ProjectType { get; set; }
        public string TaskType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string User {  get; set; } 
    }
}

