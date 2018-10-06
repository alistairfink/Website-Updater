using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

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
                        ListedContents(currPage.Fields, tab, currPage.Name);
                    }
                    else
                    {
                        throw new Exception("Incorrect page type " + currPage.Type.ToString());
                    }
                }
            }
            //tab.Header = _settings.Pages[tabs.SelectedIndex].Name + "Tested";
        }
        private void ListedContents(List<Dictionary<string,dynamic>> contents, TabItem tab, string name)
        {
            /*Layout:
              <ScrollViewer => scroll>
                <TextBlock => text>
                <Button>
                <Button>
                <Button>
                etc...
              </ScrollViewer>
             */
            ScrollViewer scroll = new ScrollViewer();
            StackPanel stack = new StackPanel();
            ItemsControl itemsControl = new ItemsControl();
            List<Button> buttonList = new List<Button>();
            TextBlock header = new TextBlock
            {
                Height = 30,
                Text = name
            };
            TextBlock footer = new TextBlock
            {
                Height = 50
            };
            stack.Children.Add(header);
            foreach (Dictionary<string, dynamic> contentDictionary in contents)
            {
                Button button = new Button();
                button.Click += new RoutedEventHandler(ContentButtonClick);
                button.Margin = new Thickness(50,20,50,0);
                button.Content = contentDictionary.First(x => x.Key != "_id").Value;
                button.Tag = contentDictionary.First(x => x.Key == "_id").Value;
                buttonList.Add(button);
            }
            itemsControl.ItemsSource = buttonList;
            stack.Children.Add(itemsControl);
            stack.Children.Add(footer);
            scroll.Content = stack;
            tab.Content = scroll;
        }
        private void ContentButtonClick(object sender, EventArgs e)
        {
            string id = (sender as Button).Tag.ToString();
            Page currPage = _settings.Pages[tabs.SelectedIndex];
            List<Dictionary<string, dynamic>> contents = currPage.Fields;
            var fields = contents.Find(x => x["_id"] == id);
            if(currPage.Type.ToLower() == "listed")
            {
                using (HttpClient client = new HttpClient())
                {

                }
            }
            else
            {

            }
        }

    }
}
