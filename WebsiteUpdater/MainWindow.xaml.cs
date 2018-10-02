using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

namespace WebsiteUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Settings _settings;
        public MainWindow()
        {
            InitializeComponent();
            _settings = InitSettings();
            InitButtons();
        }
        private Settings InitSettings()
        {
            var jsonText = File.ReadAllText("./Settings.json");
            return JsonConvert.DeserializeObject<Settings>(jsonText);           
        }
        private void InitButtons()
        {
            foreach (Page page in _settings.Pages)
            {
                TabItem newTab = new TabItem();
                string newName = "     " + page.Name + "     ";
                newTab.Header = newName;
                newTab.Name = page.Name;
                newTab.Content = "Loading...";
                tabs.Items.Add(newTab);
            }
        }
        private void TabsChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem tab = tabs.SelectedItem as TabItem;
            Page currPage = _settings.Pages[tabs.SelectedIndex];
            if (currPage.Fields == null)
            {
                currPage.Fields = new List<List<KeyValuePair<string, dynamic>>>();
                using (HttpClient client = new HttpClient())
                {
                    if (currPage.Type.ToLower() == "single")
                    {
                        var result = client.GetStringAsync(_settings.BaseUrl + currPage.GetLink).Result;
                        var fields = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result);
                        List<KeyValuePair<string, dynamic>> tempList = new List<KeyValuePair<string, dynamic>>();
                        foreach (string fieldName in fields.Keys)
                        {
                            var fieldVal = fields[fieldName];
                            if (fieldVal.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
                            {
                                var fieldValString = fieldVal.ToString();
                                fieldVal = JsonConvert.DeserializeObject<string[]>(fieldValString);
                            }
                            tempList.Add(new KeyValuePair<string, dynamic>(fieldName, fieldVal));
                        }
                        currPage.Fields.Add(tempList);
                    }
                    else if (currPage.Type.ToLower() == "array")
                    {

                    }
                    else if(currPage.Type.ToLower() == "listed")
                    {

                    }
                    else
                    {
                        throw new Exception("Incorrect page type " + currPage.Type.ToString());
                    }
                }
            }
            if(currPage.Type.ToLower() == "single")
            {
                ListedContents(currPage.Fields.First(), tab);
            }
            //tab.Header = _settings.Pages[tabs.SelectedIndex].Name + "Tested";
        }
        private void ListedContents(List<KeyValuePair<string,dynamic>> contents, TabItem tab)
        {
            foreach(KeyValuePair<string, dynamic> item in contents)
            {
                tab.Content = tab.Content + "\n" + item.Key;
            }
        }
    }
}
