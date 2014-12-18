using Common;
using PRAIM.Models;
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
    public class ProjectViewModel : INotifyPropertyChanged
    {
        #region Commands

        public ICommand AddVersion { get; set; }
        public ICommand RemoveVersion { get; set; }

        #endregion Commands

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

        public string Name
        {
            get { return (Model != null)? Model.Name : null; }
            set {
                if (Model != null) {
                    Model.Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public ObservableCollection<string> Versions { get; set; }

        public string SelectedVersion { get; set; }

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

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectViewModel()
        {
            Versions = new ObservableCollection<string>();
            AddVersion = new Command(CommandsCanExec, OnAddVersion);
            RemoveVersion = new Command(CommandsCanExec, OnRemoveVersion);
        }

        private void OnAddVersion(object o)
        {
            string version = o as string;
            if (version == null) return;

            Versions.Add(version);
            Model.Versions.Add(version);
        }

        private void OnRemoveVersion(object o)
        {
            Model.Versions.Remove(SelectedVersion);
            Versions.Remove(SelectedVersion);
        }

        private bool CommandsCanExec(object parameter)
        {
            return true;
        }

        private void UpdateVersions()
        {
            Versions.Clear();
            if (Model == null || Model.Versions == null) return;

            foreach (string version in Model.Versions) {
                Versions.Add(version);
            }
        }

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
