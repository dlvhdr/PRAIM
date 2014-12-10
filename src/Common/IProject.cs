using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRAIM.Interfaces
{
    public interface IProject
    {
        string Name { get; set; }
        List<string> Versions { get; set; }
        string Description { get; set; }
    }
}
