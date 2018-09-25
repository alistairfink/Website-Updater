using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Specialized;

namespace WebsiteUpdater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly string _environment;
        public App()
        {
            _environment = ConfigurationSettings.AppSettings["Environment"];
        }

    }
}
