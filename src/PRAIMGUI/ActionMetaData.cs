using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PRAIM
{
    public enum Priority { High, Medium, Low }

    class ActionMetaData
    {
        int projectID;
        Priority priority;
        double version;
        DateTime dateTime;
        StringBuilder comments;
    }
}
