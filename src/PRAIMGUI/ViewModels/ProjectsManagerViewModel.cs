using Common;
using PRAIM.Models;
using PRAIM.ViewModels;
using PRAIMDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PRAIM
{
    /// <summary>
    /// View model for Projects Manager
    /// </summary>
    public class ProjectsManagerViewModel : INotifyPropertyChanged
    {
        #region Commands

        public Command SetWorkingProjectCommand { get; set; }
        public Command AddProjectCommand { get; set; }
        public Command RemoveProjectCommand { get; set; }

        #endregion Commands

        #region Events

        public delegate void WorkingProjectChangedDelegate(string project, string version);
        public event WorkingProjectChangedDelegate WorkingProjectChanged;

        public delegate void VersionAddDelegate(string project, string version);
        public event VersionAddDelegate VersionAdded;

        public delegate void VersionRemovedDelegate(string project, string version);
        public event VersionRemovedDelegate VersionRemoved;

        #endregion Events

        #region Public Properties

        /// <summary>
        /// The projects manager model
        /// </summary>
        public ProjectsManagerModel Model 
        { 
            get { return _Model; } 
            set 
            { 
                _Model = value; NotifyPropertyChanged(null); 
            } 
        }

        /// <summary>
        /// The database containing the projects and versions
        /// </summary>
        public PRAIMDataBase DB
        {
            get { return _DB; }
            set 
            { 
                _DB = value;
                UpdateProjectsFromDB();
            }
        }

        /// <summary>
        /// The selected version
        /// </summary>
        public string SelectedVersion
        {
            get { return (SelectedProject != null) ? SelectedProject.SelectedVersion : null; }
            set
            {
                if (SelectedProject != null && SelectedProject.SelectedVersion != value) {
                    SelectedProject.SelectedVersion = value;
                    SetWorkingProjectCommand.UpdateCanExecuteState();
                    NotifyPropertyChanged("SelectedVersion");
                }
            }
        }

        /// <summary>
        /// The selected project's versions
        /// </summary>
        public List<string> SelectedProjectVersions
        {
            get
            {
                Project project = Projects.FirstOrDefault(x => x.Name == SelectedProject.Name);
                if (project == null) return null;
                return project.Versions.ToList();
            }
        }

        /// <summary>
        /// The list of projects in the DB
        /// </summary>
        public ObservableCollection<Project> Projects { get; set; }

        public ProjectViewModel SelectedProject 
        { 
            get { return _SelectedProject; } 
            set {
                if (value != _SelectedProject) {
                    _SelectedProject = value;
                    NotifyPropertyChanged("SelectedVersion");
                    NotifyPropertyChanged("SelectedProject");
                }
            } 
        }

        public Project SelectedProjectModel
        {
            get { return _SelectedProject.Model; }
            set
            {
                if (value != _SelectedProject.Model) {
                    _SelectedProject.Model = value;
                    NotifyPropertyChanged("SelectedProjectModel");
                    NotifyPropertyChanged("SelectedVersion");
                }
            }
        }

        #endregion Public Properties

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectsManagerViewModel(PRAIMDataBase _db)
        {
            Projects = new ObservableCollection<Project>();
            DB = _db;
            AddProjectCommand = new Command(CommandsCanExec, OnAddProject);
            RemoveProjectCommand = new Command(CommandsCanExec, OnRemoveProject);
            SetWorkingProjectCommand = new Command(CanSetWorkingProject, OnSetWorkingProject);

            Model = new ProjectsManagerModel();
            SelectedProject = new ProjectViewModel();
        }

        #region Public Methods

        /// <summary>
        /// Various commands generic can execute.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool CommandsCanExec(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Add version public method
        /// </summary>
        /// <param name="version"></param>
        public void AddVersion(string version)
        {
            SelectedProject.AddVersion.Execute(version);
            _DB.InsertVersion(SelectedProject.Name, version);

            SelectedVersion = version;
            if (VersionAdded != null) VersionAdded(SelectedProject.Name, version);
        }

        /// <summary>
        /// Remove version public method
        /// </summary>
        /// <param name="version"></param>
        public void RemoveVersion()
        {
            string removed_version = SelectedProject.SelectedVersion;
            SelectedProject.RemoveVersion.Execute(null);
            _DB.DeleteVersion(removed_version);

            if (VersionRemoved != null) VersionRemoved(SelectedProject.Name, removed_version);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// DB was changed. Rebuild the projects from the DB.
        /// </summary>
        private void UpdateProjectsFromDB()
        {
            List<Project> projects = (DB != null) ? _DB.GetAllProjectProperties() : null;

            Projects.Clear();
            if (projects == null) return;

            foreach (Project project in projects) {
                Projects.Add(project);
            }
        }

        /// <summary>
        /// Add project command handler
        /// </summary>
        /// <param name="parameter"></param>
        private void OnAddProject(object parameter)
        {
            Project new_project = parameter as Project;
            Model.Projects.Add(new_project);
            Projects.Add(new_project);
            SelectedProjectModel = new_project;
            _DB.InsertProject(new_project.Name, new_project.Description);
        }

        /// <summary>
        /// Remove project command handler
        /// </summary>
        /// <param name="parameter"></param>
        private void OnRemoveProject(object parameter)
        {
            if (SelectedProjectModel == null) return;

            _DB.DeleteProject(SelectedProjectModel.Name);
            Model.Projects.Remove(SelectedProjectModel);
            Projects.Remove(SelectedProjectModel);
            SelectedProjectModel = Projects.FirstOrDefault();
        }

        /// <summary>
        /// Checks if you can set the working project (for the button)
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool CanSetWorkingProject(object parameter)
        {
            if (SelectedProject == null || SelectedProject.Name == null ||
                SelectedProject.SelectedVersion == null) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set working project handler
        /// </summary>
        /// <param name="parameter"></param>
        private void OnSetWorkingProject(object parameter)
        {
            string project_name = SelectedProject.Name;
            string version = SelectedProject.SelectedVersion;

            if (WorkingProjectChanged != null) WorkingProjectChanged(project_name, version);
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

        private ProjectViewModel _SelectedProject;
        private PRAIMDataBase _DB;
        private ProjectsManagerModel _Model;

        #endregion Private Fields
    }
}
