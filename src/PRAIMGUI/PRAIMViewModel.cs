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
using System.Windows.Input;

namespace PRAIM
{
    public class PRAIMViewModel : INotifyPropertyChanged
    {
        #region Commands

        public Command SaveCommand { get; set; }

        #endregion Commands

        #region Public Properties

        /// <summary>
        /// List of possible priorities to choose from for an Action Item
        /// </summary>
        public List<Priority> PossiblePriorities { get; private set; }

        /// <summary>
        /// The Action Item to be inserted to the DBs
        /// </summary>
        public ActionItem InsertActionItem { get; set; }
        public ActionMetaData InsertMetadata
        {
            get { return (InsertActionItem != null) ? InsertActionItem.metaData : null; }
            set { InsertActionItem.metaData = value; }
        }

        /// <summary>
        /// The date and time when taking the snapshot
        /// </summary>
        public Nullable<DateTime> DateTime
        {
            get { return (InsertMetadata != null)? InsertMetadata.DateTime : null; }
            set
            {
                if (InsertMetadata != null && value != InsertMetadata.DateTime) {
                    InsertMetadata.DateTime = value;
                    NotifyPropertyChanged("DateTime");
                }
            }
        }

        /// <summary>
        /// The meta data to be searched for in the DB
        /// </summary>
        public ActionItem SearchActionItem { get; set; }
        public ActionMetaData SearchMetadata
        {
            get { return (SearchActionItem != null) ? SearchActionItem.metaData : null; }
            set { SearchActionItem.metaData = value; }
        }

        /// <summary>
        /// List of action items returned as search results from the DB
        /// </summary>
        public List<ActionItem> ResultDBItems
        {
            get { return _ResultDBItems; }
            set
            {
                _ResultDBItems = value;
                NotifyPropertyChanged("ResultDBItems");
            }
        }

        /// <summary>
        /// The cropped image bytes data
        /// </summary>
        public byte[] CroppedImageBytes { get; set; }

        /// <summary>
        /// The cropped image
        /// </summary>
        public BitmapSource CroppedImage
        {
            get { return _CroppedImage; }
            set
            {
                _CroppedImage = value;
                SaveCommand.UpdateCanExecuteState();
                NotifyPropertyChanged("CroppedImage");
            }
        }

        #endregion Public Properties

        /// <summary>
        /// Constructor. 
        /// Provide default values for the project under development.
        /// </summary>
        /// <param name="projectID"></param>
        /// <param name="version"></param>
        /// <param name="defaultPriority"></param>
        public PRAIMViewModel(int projectID, string version, Priority defaultPriority)
        {
            _DB = new PRAIMDataBase();
            PossiblePriorities = new List<Priority> { Priority.Low, Priority.Medium, Priority.High };

            InsertActionItem = new ActionItem();
            InsertActionItem.metaData = new ActionMetaData();

            SearchActionItem = new ActionItem();
            SearchActionItem.metaData = new ActionMetaData();

            SaveCommand = new Command(SaveCanExec, SaveExec);
        }

        #region Public Methods

        /// <summary>
        /// Save an action item to the DB
        /// </summary>
        public void SaveActionItem()
        {
            InsertActionItem.snapShot = CroppedImageBytes;
            if (_DB.InsertActionItem(this.InsertActionItem) == true) {
                MessageBox.Show("Action Item was saved successfully.", "Success", MessageBoxButton.OK);
            } else {
                MessageBox.Show("Error insering to DB", "Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Search the DB by the SearchMetadata
        /// </summary>
        public void SearchDB()
        {
            ResultDBItems = _DB.GetActionItems(SearchMetadata);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Save command can execute handler
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool SaveCanExec(object p)
        {
            return CroppedImage != null;
        }

        /// <summary>
        /// Save command execute handler
        /// </summary>
        /// <param name="p"></param>
        private void SaveExec(object p)
        {
            SaveActionItem();
        }

        #endregion Private Methods

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion INotifyPropertyChanged

        #region Private Fields

        private PRAIMDataBase _DB;
        private BitmapSource _CroppedImage;
        private List<ActionItem> _ResultDBItems;

        #endregion Private Fields
    }
}
