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
                currPage.Fields = new List<Dictionary<string, dynamic>>();
                using (HttpClient client = new HttpClient())
                {
                    var result = client.GetStringAsync(_settings.BaseUrl + currPage.GetLink).Result;
                    if (currPage.Type.ToLower() == "single")
                    {
                        var fields = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result);
                        Dictionary<string, dynamic> tempList = new Dictionary<string, dynamic>();
                        foreach (string fieldName in fields.Keys)
                        {
                            var fieldVal = fields[fieldName];
                            if (fieldVal.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
                            {
                                fieldVal = JsonConvert.DeserializeObject<string[]>(fieldVal.ToString());
                            }
                            tempList.Add(fieldName, fieldVal);
                        }
                        currPage.Fields.Add(tempList);
                        //ListedContents(currPage.Fields.First(), tab);
                    }
                    else if (currPage.Type.ToLower() == "array" || currPage.Type.ToLower() == "listed")
                    {
                        var pageContents = JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(result);
                        foreach (Dictionary<string, dynamic> content in pageContents)
                        { 
                            Dictionary<string, dynamic> tempList = new Dictionary<string, dynamic>();
                            foreach(string key in content.Keys)
                            {
                                var fieldVal = content[key];
                                if (fieldVal.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
                                {
                                    fieldVal = JsonConvert.DeserializeObject<string[]>(fieldVal.ToString());
                                }
                                tempList.Add(key, fieldVal);
                            }
                            currPage.Fields.Add(tempList);
                        }
                        ListedContents(currPage.Fields, tab);
                    }
                    else
                    {
                        throw new Exception("Incorrect page type " + currPage.Type.ToString());
                    }
                }
            }
            //tab.Header = _settings.Pages[tabs.SelectedIndex].Name + "Tested";
        }
        private void ListedContents(List<Dictionary<string,dynamic>> contents, TabItem tab)
        {
            foreach(Dictionary<string, dynamic> contentDictionary in contents)
            { 
                tab.Content = tab.Content + "\n";
            }
        }
    }
}
