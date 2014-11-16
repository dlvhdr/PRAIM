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

namespace PRAIM
{
    /// <summary>
    /// Interaction logic for PRAIMWindow.xaml
    /// </summary>
    public partial class PRAIMWindow : Window
    {
        public ICollectionView DummyDBItems { get; private set; }

        public PRAIMWindow()
        {
            List<ActionMetaData> dummyDBItems = new List<ActionMetaData> {
                new ActionMetaData { DateTime = new DateTime(2010,10,10), Comments = "Comment #1", Priority=Priority.High, ProjectID = 1, Version = 1.1 },
                new ActionMetaData { DateTime = new DateTime(2010,10,10), Comments = "Comment #2", Priority=Priority.High, ProjectID = 1, Version = 1.1 },
                new ActionMetaData { DateTime = new DateTime(2010,10,10), Comments = "Comment #3", Priority=Priority.High, ProjectID = 1, Version = 1.1 },
                new ActionMetaData { DateTime = new DateTime(2010,10,10), Comments = "Comment #4", Priority=Priority.High, ProjectID = 1, Version = 1.1 },
                new ActionMetaData { DateTime = new DateTime(2010,10,10), Comments = "Comment #5", Priority=Priority.High, ProjectID = 1, Version = 1.1 },
                new ActionMetaData { DateTime = new DateTime(2010,10,10), Comments = "Comment #6", Priority=Priority.High, ProjectID = 1, Version = 1.1 },
            };

            DummyDBItems = CollectionViewSource.GetDefaultView(dummyDBItems);
            InitializeComponent();

            this.DataContext = new PRAIMViewModel(1, "1.0", Priority.Low);
        }

        private void OnTakeSnapshot(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
        }
    }
}
