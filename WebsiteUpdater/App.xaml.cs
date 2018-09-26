using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;

namespace WebsiteUpdater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var jsonText = File.ReadAllText("./Settings.json");
            var sponsors = JsonConvert.DeserializeObject<Settings>(jsonText);
            string test = sponsors.Test;
        }

    }
}
