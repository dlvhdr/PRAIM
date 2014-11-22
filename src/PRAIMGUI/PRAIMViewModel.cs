using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PRAIM
{
    public class PRAIMViewModel : INotifyPropertyChanged
    {
        public Priority DefaultPriority { get; set; }
        public Priority Priority { get; set; }
        public string Version { get; set; }
        public string Comments { get; set; }
        public int ProjectID { get; set; }
        public TimeSpan Time { get; set; }
        public List<Priority> PossiblePriorities { get; private set; }
        
        public CroppedBitmap CroppedImage
        {
            get { return _CroppedImage; }
            set
            {
                _CroppedImage = value;
                NotifyPropertyChanged("CroppedImage");
            }
        }

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

        private CroppedBitmap _CroppedImage;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion INotifyPropertyChanged
    }
}
