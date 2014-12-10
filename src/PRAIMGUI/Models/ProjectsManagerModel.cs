using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRAIM.Models
{
    public class ProjectsManagerModel
    {
        public List<Project> Projects { get; set; }

        public ProjectsManagerModel()
        {
            Projects = new List<Project>();
        }
    }
}
