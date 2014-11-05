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
        public Priority Priority { get; set; }
        public string Version { get; set; }
        public string Comments { get; set; }
        public int ProjectID { get; set; }
        public TimeSpan Time { get; set; }
        public List<Priority> PossiblePriorities { get; private set; }

        //PRAIM constructor. Provide default values for the project under development.
        public PRAIMViewModel(int projectID, string version, Priority defaultPriority)
        {
            PossiblePriorities = new List<Priority> { Priority.Low, Priority.Medium, Priority.High };
            this.ProjectID = projectID;
            this.DefaultPriority = defaultPriority;
            this.Priority = DefaultPriority;
            this.Version = version;
            

            //Mockup code
            this.Time = DateTime.Now.TimeOfDay;

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
