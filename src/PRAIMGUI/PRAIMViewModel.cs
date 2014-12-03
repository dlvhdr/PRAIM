using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

using PRAIMDB;
using Common;

namespace PRAIM
{
    public class PRAIMViewModel : INotifyPropertyChanged
    {
        public List<Priority> PossiblePriorities { get; private set; }
        
        public ActionItem ActionItem { get; set; }
        public ActionMetaData Metadata
        {
            get { return (ActionItem != null)? ActionItem.metaData : null; }
            set { ActionItem.metaData = value; }
        }

        public byte[] CroppedImageBytes
        {
            get { return _CroppedImageBytes; }
            set
            {
                _CroppedImageBytes = value;
                NotifyPropertyChanged("CroppedImageBytes");
            }
        }

        //PRAIM constructor. Provide default values for the project under development.
        public PRAIMViewModel(int ProjectName, string version, Priority defaultPriority)
        {
            _DB = new PRAIMDataBase();
            PossiblePriorities = new List<Priority> { Priority.Low, Priority.Medium, Priority.High };

            ActionItem = new ActionItem();
            ActionItem.metaData = new ActionMetaData();
        }
	
	    //open the PRAIM dialog
        public bool open() { return true; }

        public void SaveActionItem()
        {
            ActionItem.snapShot = CroppedImageBytes;
            if (_DB.InsertActionItem(this.ActionItem) == true) return;

            MessageBox.Show("Error insering to DB");
        }

	    // return the list of images 
       // List<ActionItem> getActionItem(ActionMetaData metaData) { }

        private byte[] _CroppedImageBytes;
        private PRAIMDataBase _DB;

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
