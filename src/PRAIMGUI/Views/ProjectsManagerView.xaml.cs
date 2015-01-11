using PRAIM.Models;
using PRAIM.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PRAIM.Views
{
    /// <summary>
    /// Interaction logic for ProjectsManager.xaml
    /// </summary>
    public partial class ProjectsManager : UserControl
    {
        /// <summary>
        /// The view's view model.
        /// </summary>
        public ProjectsManagerViewModel ViewModel { get { return DataContext as ProjectsManagerViewModel; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectsManager()
        {
            InitializeComponent();
        }

        #region Handlers

        /// <summary>
        /// Add project handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddProject(object sender, RoutedEventArgs e)
        {
            ProjectViewModel new_project = new ProjectViewModel() { Model = new Project() };

            NewProjectDlg dlg = new NewProjectDlg()
            {
                DataContext = new_project,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                TakenNames = ViewModel.Projects.Select(x => x.Name).ToList()
            };

            dlg.ShowDialog();

            if (dlg.Result) {
                ViewModel.AddProjectCommand.Execute(new_project.Model);
            }
        }

        /// <summary>
        /// Add version handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddVersion(object sender, RoutedEventArgs e)
        {
            NewVersionDlg dlg = new NewVersionDlg()
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                TakenVersions = ViewModel.SelectedProject.Versions.ToList()
            };

            dlg.ShowDialog();

            if (dlg.Result) {
                ViewModel.AddVersion(dlg.Version);
            }
        }

        /// <summary>
        /// Remove version handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRemoveVersion(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveVersion();
        }

        #endregion Handlers
    }
}
