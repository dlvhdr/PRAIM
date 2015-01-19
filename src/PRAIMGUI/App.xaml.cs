using PRAIMDB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace PRAIM
{
    /// <summary>
    /// PRAIM app initialization
    /// </summary>
    public partial class App : Application
    {
        public App()
            : base()
        {
            this.Startup += OnStartup;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            MainWindow = new PRAIMWindow();

            //------------------------------------
            // Initialize boot config from XML
            //------------------------------------
            BootFromXml();

            //------------------------------------
            // Initialize the Data Base
            //------------------------------------
            _DB = new PRAIMDataBase((int)_Config.CurrentActionItemID);

            //---------------------------
            // Initialize view models
            //---------------------------
            _MainVM = new PRAIMViewModel();
            _MainVM.DB = _DB;
            _MainVM.ProjectsViewModel = new ProjectsManagerViewModel(_DB);
            _MainVM.WorkingProjectName = _Config.LastProject;
            _MainVM.WorkingProjectVersion = _Config.LastVersion;

            //------------------------------------
            // Set DataContext to PRAIMViewModel
            //------------------------------------
            MainWindow.DataContext = _MainVM;

            MainWindow.Closed += SaveConfig;

            MainWindow.Show();
        }

        /// <summary>
        /// Initialize boot info
        /// </summary>
        private void BootFromXml()
        {
            if (!File.Exists(_XmlLocation)) {
                _Config = new BootConfig();
                XmlSerializer serializer = new XmlSerializer(typeof(BootConfig));
                FileStream fs = new FileStream(_XmlLocation, FileMode.CreateNew);
                serializer.Serialize(fs, _Config);
                return;
            } else {
                XmlSerializer serializer = new XmlSerializer(typeof(BootConfig));
                using (StreamReader sr = new StreamReader(_XmlLocation)) {
                    _Config = (BootConfig)serializer.Deserialize(sr);
                    if (_Config.LastVersion == "") _Config.LastVersion = null;
                    if (_Config.LastProject == "") _Config.LastProject = null;
                }
            }

        }

        /// <summary>
        /// Save boot info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveConfig(object sender, EventArgs e)
        {
            _Config.CurrentActionItemID = _DB.currentID;
            _Config.LastProject = _MainVM.WorkingProjectName;
            _Config.LastVersion = _MainVM.WorkingProjectVersion;
            using (FileStream fs = new FileStream(_XmlLocation, FileMode.Open)) {
                fs.SetLength(0);
                fs.Flush();
                XmlSerializer serializer = new XmlSerializer(typeof(BootConfig));
                serializer.Serialize(fs, _Config);
            }
        }

        string _XmlLocation = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "boot.xml");
        private BootConfig _Config;
        private PRAIMDataBase _DB;
        private PRAIMViewModel _MainVM;
    }
}
