using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Common
{
    public enum Priority { High = 0, Medium, Low }

    /// <summary>
    /// Action Item Meta-Data model
    /// </summary>
    public class ActionMetaData
    {
        public string ProjectName { get; set; }
        public Priority? Priority { get; set; }
        public string Version { get; set; }
        public Nullable<DateTime> DateTime { get; set; }
        public String Comments { get; set; }

        public Nullable<DateTime> FromDate { get; set; }
        public Nullable<DateTime> ToDate { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionMetaData()
        {
            Comments = String.Empty;
        }
    }
}
