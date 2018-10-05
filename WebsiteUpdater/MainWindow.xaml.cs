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
                Grid newTabContent = new Grid();
                string loadingText = "Loading...";
                TextBlock text = new TextBlock
                {
                    Text = loadingText
                };
                newTabContent.Children.Add(text);
                newTab.Content = newTabContent;
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
            //TODO: Fix Positioning.
            Grid grid = new Grid();
            int column = 1;
            int row = 1;
            //grid.Background = new SolidColorBrush(Color.FromRgb(50,50,50));
            foreach (Dictionary<string, dynamic> contentDictionary in contents)
            {
                Button button = new Button();
                button.Click += new RoutedEventHandler(ContentButtonClick);
                button.Content = contentDictionary.First(x => x.Key != "_id").Value;
                button.Width = 200;
                button.Height = 20;
                button.Tag = contentDictionary.First(x => x.Key == "_id").Value;
                //button.CommandParameter = "test1";
                //tab.Content.Children.Add(button);
                Grid.SetColumn(button, column);
                Grid.SetRow(button, row);
                grid.Children.Add(button);
                column++;
                if(column >= 5)
                {
                    row++;
                    column = 1;
                }
            }
            tab.Content = grid;
        }
        private void ContentButtonClick(object sender, EventArgs e)
        {
        }
        private void test(string test)
        {
            (tabs.SelectedItem as TabItem).Header = test;
        }
    }
}
