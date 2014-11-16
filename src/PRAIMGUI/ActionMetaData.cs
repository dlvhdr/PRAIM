using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PRAIM
{
    public enum Priority { High, Medium, Low }
    public enum ProjectID { ICT256}

    public class ActionMetaData
    {
        public int ProjectID { get; set; }
        public Priority Priority { get; set; }
        public double Version { get; set; }
        public DateTime DateTime { get; set; }
        public String Comments { get; set; }
        //StringBuilder Comments;
    }
}
