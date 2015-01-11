using PRAIM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRAIM.Models
{
    /// <summary>
    /// Project model
    /// </summary>
    public class Project : IProject
    {
        public string Name { get; set; }
        public List<string> Versions { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Project() 
        {
            Versions = new List<string>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
