using Common;
using PRAIM.Models;
using PRAIMDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PRAIM.ViewModels
{
    /// <summary>
    /// View model for Project
    /// </summary>
    public class ProjectViewModel : INotifyPropertyChanged
    {
        #region Commands

        public ICommand AddVersion { get; set; }
        public ICommand RemoveVersion { get; set; }

        #endregion Commands

        #region Public Properties

        /// <summary>
        /// The project model
        /// </summary>
        public Project Model
        {
            get { return _Model; }
            set
            {
                if (_Model != value) {
                    _Model = value;
                    UpdateVersions();
                    NotifyPropertyChanged(null);
                }
            }
        }

        /// <summary>
        /// The name of the project
        /// </summary>
        public string Name
        {
            get { return (Model != null) ? Model.Name : null; }
            set
            {
                if (Model != null) {
                    Model.Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// The versions of the project
        /// </summary>
        public ObservableCollection<string> Versions { get; set; }

        /// <summary>
        /// The selected version
        /// </summary>
        public string SelectedVersion { get; set; }

        /// <summary>
        /// The description of the project
        /// </summary>
        public string Description
        {
            get { return (Model != null) ? Model.Description : null; }
            set
            {
                if (Model != null) {
                    Model.Description = value;
                }
            }
        }

        #endregion Public Properties

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectViewModel()
        {
            Versions = new ObservableCollection<string>();
            AddVersion = new Command(CommandsCanExec, OnAddVersion);
            RemoveVersion = new Command(CommandsCanExec, OnRemoveVersion);
        }

        #region Private Methods


        /// <summary>
        /// Add version command handler
        /// </summary>
        /// <param name="o"></param>
        private void OnAddVersion(object o)
        {
            string version = o as string;
            if (version == null) return;

            Versions.Add(version);
            Model.Versions.Add(version);
        }

        /// <summary>
        /// Remove version command handler
        /// </summary>
        /// <param name="o"></param>
        private void OnRemoveVersion(object o)
        {
            Model.Versions.Remove(SelectedVersion);
            Versions.Remove(SelectedVersion);
        }

        /// <summary>
        /// Various commands can execute handler
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool CommandsCanExec(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Called when the model is changed. Rebuilds the versions collection.
        /// </summary>
        private void UpdateVersions()
        {
            Versions.Clear();
            if (Model == null || Model.Versions == null) return;

            foreach (string version in Model.Versions) {
                Versions.Add(version);
            }
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

        private Project _Model;

        #endregion Private Fields
    }
}
