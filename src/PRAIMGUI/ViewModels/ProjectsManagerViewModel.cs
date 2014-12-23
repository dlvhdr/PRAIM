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

        public delegate void VersionAddDelegate(string version);
        public event VersionAddDelegate VersionAdded;

        public delegate void VersionRemovedDelegate(string version);
        public event VersionRemovedDelegate VersionRemoved;

        #endregion Events

        #region Public Properties

        public ProjectsManagerModel Model { get; set; }

        public string SelectedVersion
        {
            get { return (SelectedProject != null) ? SelectedProject.SelectedVersion : null; }
            set
            {
                if (SelectedProject != null && SelectedProject.SelectedVersion != value && value != null) {
                    SelectedProject.SelectedVersion = value;
                    SetWorkingProjectCommand.UpdateCanExecuteState();
                    NotifyPropertyChanged("SelectedVersion");
                }
            }
        }

        public List<string> SelectedProjectVersions
        {
            get
            {
                Project project = Projects.FirstOrDefault(x => x.Name == SelectedProject.Name);
                if (project == null) return null;
                return project.Versions.ToList();
            }
        }

        public ObservableCollection<Project> Projects { get; set; }

        public ProjectViewModel SelectedProject { get; set; }

        #endregion Public Properties

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectsManagerViewModel(PRAIMDataBase _db)
        {
            _DB = _db;
            List<Project> projects = _DB.GetAllProjectProperties();
            BuildProjects(projects);

            AddProjectCommand = new Command(CommandsCanExec, OnAddProject);
            RemoveProjectCommand = new Command(CommandsCanExec, OnRemoveProject);
            SetWorkingProjectCommand = new Command(CanSetWorkingProject, OnSetWorkingProject);

            Model = new ProjectsManagerModel();
            SelectedProject = new ProjectViewModel();
        }

        private void BuildProjects(List<Project> projects)
        {
            Projects = new ObservableCollection<Project>();
            if (projects == null) return;

            foreach (Project project in projects) {
                Projects.Add(project);
            }
        }

        #region Public Methods

        private bool CommandsCanExec(object parameter)
        {
            return true;
        }

        public void AddVersion(string version)
        {
            SelectedProject.AddVersion.Execute(version);
            _DB.InsertVersion(SelectedProject.Name, version);
            
            SelectedVersion = version;
            if (VersionAdded != null) VersionAdded(version);
        }

        public void RemoveVersion()
        {
            string removed_version = SelectedProject.SelectedVersion;
            SelectedProject.RemoveVersion.Execute(null);
            _DB.DeleteVersion(removed_version);

            if (VersionRemoved != null) VersionRemoved(removed_version);
        }

        #endregion Public Methods

        #region Private Methods

        private void OnAddProject(object parameter)
        {
            Project new_project = parameter as Project;
            Model.Projects.Add(new_project);
            Projects.Add(new_project);
            SelectedProject.Model = new_project;
            _DB.InsertProject(new_project.Name, new_project.Description);
        }

        private void OnRemoveProject(object parameter)
        {
            if (SelectedProject.Model == null) return;

            _DB.DeleteProject(SelectedProject.Model.Name);
            Model.Projects.Remove(SelectedProject.Model);
            Projects.Remove(SelectedProject.Model);
            SelectedProject.Model = Projects.FirstOrDefault();
        }

        private bool CanSetWorkingProject(object parameter)
        {
            if (SelectedProject == null || SelectedProject.Name == null ||
                SelectedProject.SelectedVersion == null) {
                    return false;
            }

            return true;
        }

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

        #endregion Private Fields
    }
}
