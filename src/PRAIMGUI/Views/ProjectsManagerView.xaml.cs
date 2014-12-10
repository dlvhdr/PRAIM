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
        public ProjectsManagerViewModel ViewModel { get { return DataContext as ProjectsManagerViewModel; } }

        public ProjectsManager()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

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

        private void OnRemoveVersion(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveVersion();
        }
    }
}
