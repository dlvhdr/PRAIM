using Common;
using PRAIM.Models;
using PRAIM.ViewModels;
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

        #endregion Events

        #region Public Properties

        public ProjectsManagerModel Model { get; set; }

        public Project CurrentProject { get; set; }

        public ObservableCollection<Project> Projects { get; set; }

        public ProjectViewModel SelectedProject
        {
            get { return _SelectedProject; }
            set
            {
                if (value != _SelectedProject) {
                    _SelectedProject = value;
                    SetWorkingProjectCommand.UpdateCanExecuteState();
                    NotifyPropertyChanged("SelectedProject");
                }
            }
        }

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

        #endregion Public Properties

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectsManagerViewModel()
        {
            //TODO: initialize from DB

            AddProjectCommand = new Command(CommandsCanExec, OnAddProject);
            RemoveProjectCommand = new Command(CommandsCanExec, OnRemoveProject);
            SetWorkingProjectCommand = new Command(CanSetWorkingProject, OnSetWorkingProject);

            Model = new ProjectsManagerModel();
            Projects = new ObservableCollection<Project>();
            SelectedProject = new ProjectViewModel();
        }

        #region Public Methods

        private bool CommandsCanExec(object parameter)
        {
            return true;
        }

        public void AddVersion(string version)
        {
            SelectedProject.AddVersion.Execute(version);
            SelectedVersion = version;
            //TODO:Update DB.
        }

        public void RemoveVersion()
        {
            SelectedProject.RemoveVersion.Execute(null);
            //TODO:Update DB.
        }

        #endregion Public Methods

        #region Private Methods

        private void OnAddProject(object parameter)
        {
            Project new_project = parameter as Project;
            Model.Projects.Add(new_project);
            Projects.Add(new_project);
            SelectedProject.Model = new_project;
            //TODO: Add to DB
        }

        private void OnRemoveProject(object parameter)
        {
            Model.Projects.Remove(SelectedProject.Model);
            Projects.Remove(SelectedProject.Model);
            SelectedProject.Model = Projects.FirstOrDefault();
            //TODO: Remove from DB
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

        #endregion Private Fields
    }
}
