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
using System.Windows.Forms;
using Microsoft.Win32;



namespace PRAIM
{
    /// <summary>
    /// Main application View model
    /// </summary>
    public class PRAIMViewModel : INotifyPropertyChanged
    {
        #region Commands

        public Command SaveCommand { get; set; }
        public Command RemoveActionItemCommand { get; set; }

        #endregion Commands

        #region Public Properties

        /// <summary>
        /// The Database of ActionItems and Projects
        /// </summary>
        public PRAIMDataBase DB 
        { 
            get { return _DB; } 
            set { _DB = value; } 
        }

        /// <summary>
        /// The projects manager view model. Register to changes on it.
        /// </summary>
        public ProjectsManagerViewModel ProjectsViewModel {
            get { return _ProjectsViewModel; }
            set 
            {
                _ProjectsViewModel = value;

                if (_ProjectsViewModel != null) {
                    _ProjectsViewModel.VersionAdded += OnVersionAdded;
                    _ProjectsViewModel.VersionRemoved += OnVersionRemoved;
                    _ProjectsViewModel.WorkingProjectChanged += OnWorkingProjectChanged;
                }
            } 
        }

        /// <summary>
        /// Projects available for selection at search combobox.
        /// </summary>
        public ObservableCollection<Project> Projects { get { return ProjectsViewModel.Projects; } }

        /// <summary>
        /// Selected project at search combobox
        /// </summary>
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

                    SearchMetadata.ProjectName = (_SelectedSearchProject != null)? _SelectedSearchProject.Name : null;
                    UpdateSelectedProjectVersions();
                }
            }
        }
        
        /// <summary>
        /// Version list for the selected project at combobox.
        /// </summary>
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
        public Priority? InsertMetadataPriority
        {
            get { return (InsertMetadata != null) ? InsertMetadata.Priority : null; }
            set { InsertMetadata.Priority = value; SaveCommand.UpdateCanExecuteState(); }
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

        /// <summary>
        /// The working project name
        /// </summary>
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

        /// <summary>
        /// The working project version
        /// </summary>
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
        public ObservableCollection<ActionItem> ResultDBItems
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
                    RemoveActionItemCommand.UpdateCanExecuteState();
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
        /// Constructor
        /// </summary>
        public PRAIMViewModel()
        {
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
            RemoveActionItemCommand = new Command(RemoveCanExec, OnRemoveActionItem);

            ResultDBItems = new ObservableCollection<ActionItem>();
        }

        #region Public Methods

        /// <summary>
        /// Save an action item to the DB
        /// </summary>
        public void SaveActionItem()
        {
            InsertActionItem.snapShot = CroppedImageBytes;
            if (_DB.InsertActionItem(this.InsertActionItem) == true) {
                System.Windows.MessageBox.Show("Action Item was saved successfully.", "Success", MessageBoxButton.OK);
            } else {
                System.Windows.MessageBox.Show("Error insering to DB", "Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Search the DB by the SearchMetadata
        /// </summary>
        public void SearchDB()
        {
            List<ActionItem> items = _DB.GetActionItems(SearchMetadata);
            ResultDBItems.Clear();
            if (items == null) return;

            foreach (ActionItem item in items) {
                ResultDBItems.Add(item);
            }
        }

        /// <summary>
        /// Generate an CSV report from the search results
        /// </summary>
        public void GenerateReport()
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.FileName = "Report.csv";
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!CreateReport.CreateCSVReport(ResultDBItems, saveFileDialog.FileName))
                {
                    System.Windows.MessageBox.Show("Error generating report", "Error", MessageBoxButton.OK);
                }
            }           
        }

        /// <summary>
        /// Build a BitmapSource from the given action item's snapshot.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Save action item snapshot to a file
        /// </summary>
        public void ExportActionItem()
        {
            BitmapSource source = GetSnapshotSource(SelectedActionItem);

            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.Filter = "JPEG (*.jpeg)|*.jpeg|PNG (*.png)|*.png|BMP (*.bmp)|*.bmp";

            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                BitmapEncoder encoder;

                if (dlg.FileName.EndsWith("jpeg")) {
                    encoder = new JpegBitmapEncoder();
                } else if (dlg.FileName.EndsWith("bmp")) {
                    encoder = new BmpBitmapEncoder();
                } else {
                    encoder = new PngBitmapEncoder();
                }

                encoder.Frames.Add(BitmapFrame.Create(source));
                using (FileStream fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write)) {
                    encoder.Save(fs);
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Remove selected action item can execute.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool RemoveCanExec(object parameter)
        {
            return this.SelectedActionItem != null;
        }

        /// <summary>
        /// Handler for when removing an action item
        /// </summary>
        /// <param name="parameter"></param>
        private void OnRemoveActionItem(object parameter)
        {
            MessageBoxResult res = System.Windows.MessageBox.Show("Are you sure?", "Please Confirm Action", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;

            _DB.DeleteActionItems(this.SelectedActionItem);
            if (ResultDBItems != null) {
                ResultDBItems.Remove(this.SelectedActionItem);
            }

            this.SelectedActionItem = null;
        }

        /// <summary>
        /// Handler for when a version was removed from a project
        /// </summary>
        /// <param name="version"></param>
        private void OnVersionRemoved(string project, string version)
        {
            if (SelectedSearchProject != null && SelectedSearchProject.Name == project &&
                SelectedProjectVersions != null && SelectedProjectVersions.Contains(version)) {
                    SelectedProjectVersions.Remove(version);
            }

            if (WorkingProjectName == project && WorkingProjectVersion == version) {
                WorkingProjectName = null;
                WorkingProjectVersion = null;
            }
        }

        /// <summary>
        /// Handler for when a verion is added to a project
        /// </summary>
        /// <param name="version"></param>
        private void OnVersionAdded(string project, string version)
        {
            if (SelectedProjectVersions == null) return;

            if (SelectedSearchProject.Name == project && SelectedProjectVersions.Contains(version)) {
                SelectedProjectVersions.Add(version);
            }
        }

        /// <summary>
        /// Handler for when the working project is changed.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="version"></param>
        private void OnWorkingProjectChanged(string project, string version)
        {
            WorkingProjectName = project;
            WorkingProjectVersion = version;
        }

        /// <summary>
        /// Rebuild the selected project versions (for searching)
        /// </summary>
        private void UpdateSelectedProjectVersions()
        {
            SelectedProjectVersions = new ObservableCollection<string>();
            NotifyPropertyChanged("SelectedProjectVersions");

            if (SelectedSearchProject == null) return;

            foreach (string version in SelectedSearchProject.Versions) {
                SelectedProjectVersions.Add(version);
            }
        }

        /// <summary>
        /// Build the selected action item thumbnail
        /// </summary>
        private void UpdateActionItemThumbnail()
        {
            PreviewImage = GetSnapshotSource(SelectedActionItem);
        }

        /// <summary>
        /// Save command can execute handler
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool SaveCanExec(object p)
        {
            return (CroppedImage != null && WorkingProjectName != null && WorkingProjectVersion != null && InsertMetadata.Priority != null);
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
        private ObservableCollection<ActionItem> _ResultDBItems;
        private ActionItem _SelectedActionItem;
        private string _WorkingProjectName;
        private string _WorkingProjectVersion;
        private Project _SelectedSearchProject;
        private BitmapSource _PreviewImage;
        private ProjectsManagerViewModel _ProjectsViewModel;

        #endregion Private Fields
    }
}
