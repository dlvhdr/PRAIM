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
using System.IO;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using PRAIM.Models;

namespace PRAIM
{
    public class PRAIMViewModel : INotifyPropertyChanged
    {
        #region Commands

        public Command SaveCommand { get; set; }

        #endregion Commands

        #region Public Properties

        public PRAIMDataBase DB { get { return _DB; } }

        public ProjectsManagerViewModel ProjectsViewModel { get; set; }

        public ObservableCollection<Project> Projects { get { return ProjectsViewModel.Projects; } }

        public Project SelectedSearchProject
        {
            get
            {
                return _SelectedSearchProject;
            }
            set
            {
                if (value != _SelectedSearchProject) {
                    _SelectedSearchProject = value;
                    SearchMetadata.ProjectName = _SelectedSearchProject.Name;
                    UpdateSelectedProjectVersions();
                }
            }
        }
        
        public ObservableCollection<string> SelectedProjectVersions { get; set; }

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
            get { return (InsertMetadata != null) ? InsertMetadata.DateTime : null; }
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

        public string WorkingProjectName
        {
            get { return _WorkingProjectName; }
            set
            {
                if (_WorkingProjectName != value) {
                    _WorkingProjectName = value;
                    InsertMetadata.ProjectName = value;
                    SaveCommand.UpdateCanExecuteState();
                    NotifyPropertyChanged("WorkingProjectName");
                }
            }
        }

        public string WorkingProjectVersion
        {
            get { return _WorkingProjectVersion; }
            set
            {
                if (_WorkingProjectVersion != value) {
                    _WorkingProjectVersion = value;
                    InsertMetadata.Version = value;
                    SaveCommand.UpdateCanExecuteState();
                    NotifyPropertyChanged("WorkingProjectVersion");
                }
            }
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
        /// The selected item in the data grid
        /// </summary>
        public ActionItem SelectedActionItem
        {
            get { return _SelectedActionItem; }
            set
            {
                if (_SelectedActionItem != value) {
                    _SelectedActionItem = value;
                    UpdateActionItemThumbnail();
                    NotifyPropertyChanged("SelectedActionItem");
                }
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

        public BitmapSource PreviewImage
        {
            get { return _PreviewImage; }
            set
            {
                if (value != _PreviewImage) {
                    _PreviewImage = value;
                    NotifyPropertyChanged("PreviewImage");
                }
            }
        }

        #endregion Public Properties

        /// <summary>
        /// constructor
        /// </summary>
        public PRAIMViewModel()
        {
            //-----------------------
            // Boot from XML
            //-----------------------
            BootFromXml();
            _DB = new PRAIMDataBase((int)_Config.CurrentActionItemID);

            //-----------------------
            // Initialize Properties
            //-----------------------
            PossiblePriorities = new List<Priority> { Priority.Low, Priority.Medium, Priority.High };
            InsertActionItem = new ActionItem();
            InsertActionItem.metaData = new ActionMetaData();
            SearchActionItem = new ActionItem();
            SearchActionItem.metaData = new ActionMetaData();

            //-----------------------
            // Initialize Commands
            //-----------------------
            SaveCommand = new Command(SaveCanExec, SaveExec);

            //---------------------------------------
            // Initialize ProjectsManager view model
            //---------------------------------------
            ProjectsViewModel = new ProjectsManagerViewModel(_DB);
            ProjectsViewModel.WorkingProjectChanged += OnWorkingProjectChanged;
            ProjectsViewModel.VersionAdded += OnVersionAdded;
            ProjectsViewModel.VersionRemoved += OnVersionRemoved;
        }

        #region Public Methods

        public void SaveConfig()
        {
            _Config.CurrentActionItemID = _DB.currentID;
            using (FileStream fs = new FileStream(_XmlLocation, FileMode.Open)) {
                XmlSerializer serializer = new XmlSerializer(typeof(BootConfig));
                serializer.Serialize(fs, _Config);
            }
        }

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

        public BitmapSource GetSnapshotSource(object item)
        {
            ActionItem action_item = item as ActionItem;
            if (action_item == null || action_item.snapShot == null) return null;

            BitmapImage image = new BitmapImage();
            using (MemoryStream ms = new MemoryStream(action_item.snapShot)) {
                ms.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = ms;
                image.EndInit();
            }

            image.Freeze();
            return image;
        }

        public void OnWorkingProjectChanged(string project, string version)
        {
            WorkingProjectName = project;
            WorkingProjectVersion = version;
        }

        #endregion Public Methods

        #region Private Methods

        private void OnVersionRemoved(string version)
        {
            if (SelectedProjectVersions == null) return;

            SelectedProjectVersions.Add(version);
        }

        private void OnVersionAdded(string version)
        {
            if (SelectedProjectVersions == null) return;

            if (SelectedProjectVersions.Contains(version)) {
                SelectedProjectVersions.Add(version);
            }
        }

        private void UpdateSelectedProjectVersions()
        {
            SelectedProjectVersions = new ObservableCollection<string>();
            NotifyPropertyChanged("SelectedProjectVersions");

            if (SelectedSearchProject == null) return;

            foreach (string version in SelectedSearchProject.Versions) {
                SelectedProjectVersions.Add(version);
            }
        }

        private void UpdateActionItemThumbnail()
        {
            PreviewImage = GetSnapshotSource(SelectedActionItem);
        }

        /// <summary>
        /// Initialize boot info
        /// </summary>
        private void BootFromXml()
        {
            if (!File.Exists(_XmlLocation)) {
                _Config = new BootConfig();
                XmlSerializer serializer = new XmlSerializer(typeof(BootConfig));
                FileStream fs = new FileStream(_XmlLocation, FileMode.CreateNew);
                serializer.Serialize(fs, _Config);
                return;
            } else {
                XmlSerializer serializer = new XmlSerializer(typeof(BootConfig));
                using (StreamReader sr = new StreamReader(_XmlLocation)) {
                    _Config = (BootConfig)serializer.Deserialize(sr);
                }
            }

        }

        /// <summary>
        /// Save command can execute handler
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool SaveCanExec(object p)
        {
            return (CroppedImage != null && WorkingProjectName != null && WorkingProjectVersion != null);
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
        private ActionItem _SelectedActionItem;
        private string _WorkingProjectName;
        private string _WorkingProjectVersion;
        private BootConfig _Config;
        string _XmlLocation = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "boot.xml");
        private Project _SelectedSearchProject;
        private BitmapSource _PreviewImage;

        #endregion Private Fields
    }
}
