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
using PRAIM;




namespace DummyGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PRAIMViewModel praim = new PRAIMViewModel(1, "1.0", Priority.High);
        PRAIMWindow praimWin = new PRAIMWindow();

        public MainWindow()
        {
            InitializeComponent();
            praimWin.DataContext = praim;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            praimWin.Show();
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string value = comboBox.SelectedItem as string;
            Priority defaultPriority;
            switch (value)
            {
                case "High":
                default:
                    defaultPriority = Priority.High;
                    break;
                case "Medium":
                    defaultPriority = Priority.Medium;
                    break;
                case "Low":
                    defaultPriority = Priority.Low;
                    break;
            }
            praim.DefaultPriority = defaultPriority;
        }

        private void ComboBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            praim.Version = comboBox.SelectedItem as string;


        }
    }
}
