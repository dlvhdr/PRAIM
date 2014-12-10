using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using PRAIM.SnapshotManager;
using Common;
using System.IO;
using System.Globalization;



namespace PRAIM
{
    /// <summary>
    /// Interaction logic for PRAIMWindow.xaml
    /// </summary>
    public partial class PRAIMWindow : Window
    {
        public PRAIMViewModel MainViewModel { get { return this.DataContext as PRAIMViewModel; } }
        public ProjectsManagerViewModel ProjectsManagerViewModel { get; set; }

        public ICollectionView DummyDBItems { get; private set; }

        public PRAIMWindow()
        {
            InitializeComponent();

            this.DataContext = new PRAIMViewModel(1, "1.0", Priority.Low);
            ProjectsManagerView.DataContextChanged += OnProjectsManagerDataContextChanged;
            ProjectsManagerView.DataContext = new ProjectsManagerViewModel();
        }

        private void OnProjectsManagerDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ProjectsManagerView.DataContext != null) {
                ProjectsManagerViewModel projects_vm = ProjectsManagerView.ViewModel;
                if (projects_vm != null) projects_vm.WorkingProjectChanged += OnWorkingProjectChanged;
            }
        }

        private void OnWorkingProjectChanged(string project, string version)
        {
            MainViewModel.OnWorkingProjectChanged(project, version);
        }

        private void OnTakeSnapshot(object sender, RoutedEventArgs e)
        {
            this.LayoutUpdated += RunSnapshotMgr;
            this.Hide();
        }

        private void RunSnapshotMgr(object sender, EventArgs e)
        {
            if (IsVisible == true) return;
            Thread.Sleep(200);

            SnapshotManagerWindow snapshotMgr = new SnapshotManagerWindow();
            snapshotMgr.Closed += SnapshotMgrClosed;
            snapshotMgr.Show();

            this.LayoutUpdated -= RunSnapshotMgr;
        }


        private void SnapshotMgrClosed(object sender, EventArgs e)
        {
            SnapshotManagerWindow snapshotMgr = sender as SnapshotManagerWindow;
            this.Show();

            byte[] image_bytes;
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            if (snapshotMgr.CroppedImage != null) {
                encoder.Frames.Add(BitmapFrame.Create(snapshotMgr.CroppedImage));
                using(MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    image_bytes = ms.ToArray();
                    MainViewModel.CroppedImageBytes = image_bytes;
                    MainViewModel.CroppedImage = snapshotMgr.CroppedImage;
                    MainViewModel.DateTime = DateTime.Now;
                }
            }
        }

        private void SearchDB(object sender, RoutedEventArgs e)
        {
            MainViewModel.SearchDB();
        }

        private void ShowImageHandler(object sender, RoutedEventArgs e)
        {
            BitmapSource source = MainViewModel.GetSnapshotSource((sender as Button).DataContext);

            ViewSnapshotDlg dlg = new ViewSnapshotDlg() { SnapshotSource = source };
            dlg.Show();
        }

        private void OnExit(object sender, EventArgs e)
        {
            MainViewModel.SaveConfig();
            this.Close();
        }
    }
}
