using PRAIM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRAIM.Models
{
    public class Project : IProject
    {
        public string Name { get; set; }
        public List<string> Versions { get; set; }
        public string Description { get; set; }
        
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
