using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRAIM
{
    public class PRAIMViewModel
    {
        public Priority DefaultPriority { get; set; }
        public string Version { get; set; }
        public int ProjectID { get; set; }

        //PRAIM constructor. Provide default values for the project under development.
        public PRAIMViewModel(int projectID, string version, Priority defaultPriority)
        {
            this.ProjectID = projectID;
            this.DefaultPriority = defaultPriority;
            this.Version = version;
        }
	
	    //open the PRAIM dialog
        public bool open() { return true; }
	
	    //Take snapshot handler
        bool takeSnapshot() { return true; }
	
	    // return true if succeed & false if fail
        bool insertActionItem(ActionItem actionItem) { return true; }
	
	    // return the list of images 
       // List<ActionItem> getActionItem(ActionMetaData metaData) { }
        
    }
}
