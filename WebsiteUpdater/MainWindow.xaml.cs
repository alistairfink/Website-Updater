using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                        ItemContents(currPage.Fields.First()["_id"], tab, false);
                    }
                    else if (currPage.Type.ToLower() == "array" || currPage.Type.ToLower() == "listed")
                    {
                        var pageContents = JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(result);
                        foreach (Dictionary<string, dynamic> content in pageContents)
                        {
                            Dictionary<string, dynamic> tempList = new Dictionary<string, dynamic>();
                            foreach (string key in content.Keys)
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
        }
        private void ListedContents(List<Dictionary<string, dynamic>> contents, TabItem tab, string name)
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
            Button addButton = new Button();
            addButton.Click += AddButton;
            addButton.Content = "Add";
            stack.Children.Add(addButton);
            foreach (Dictionary<string, dynamic> contentDictionary in contents)
            {
                Button button = new Button();
                button.Click += new RoutedEventHandler(ContentButtonClick);
                button.Margin = new Thickness(50, 20, 50, 0);
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
            TabItem tab = tabs.SelectedItem as TabItem;
            ItemContents(id, tab, true);
        }
        private void ItemContents(string id, TabItem tab, bool listed)
        {
            Page currPage = _settings.Pages[tabs.SelectedIndex];
            List<Dictionary<string, dynamic>> contents = currPage.Fields;
            var fields = contents.Find(x => x["_id"] == id);
            ScrollViewer scroll = new ScrollViewer();
            StackPanel stack = new StackPanel();
            if (listed)
            {
                Button button = new Button
                {
                    Margin = new Thickness(50, 20, 50, 0),
                    Content = "Back"
                };
                button.Click += new RoutedEventHandler(BackButton);
                stack.Children.Add(button);
            }
            if (currPage.Type.ToLower() == "listed")
            {
                using (HttpClient client = new HttpClient())
                {
                    string requestContent = "{'_id':'" + id + "'}";
                    JObject json = JObject.Parse(requestContent);
                    var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                    var result = client.PostAsync(_settings.BaseUrl + currPage.GetListItemLink, content).Result;
                    string stringResult = result.Content.ReadAsStringAsync().Result;
                    var listedFields = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(stringResult);
                    foreach (string key in listedFields.Keys)
                    {
                        var fieldVal = listedFields[key];
                        if (fieldVal.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
                        {
                            fieldVal = JsonConvert.DeserializeObject<string[]>(fieldVal.ToString());
                        }
                        TextBlock title = new TextBlock
                        {
                            Text = key
                        };
                        TextBox text = new TextBox
                        {
                            Tag = key,
                            Text = fieldVal.GetType().IsArray ? String.Join("\n\n", fieldVal) : fieldVal
                        };
                        if (fieldVal.GetType().IsArray)
                        {
                            text.Tag += ",array";
                            text.AcceptsReturn = true;
                        }
                        else
                            text.Tag += ",notArray";
                        if (key == "_id")
                        {
                            text.IsReadOnly = true;
                            text.Background = Brushes.LightGray;
                        }
                        stack.Children.Add(title);
                        stack.Children.Add(text);
                    }

                }
            }
            else
            {
                foreach (string key in fields.Keys)
                {
                    TextBlock title = new TextBlock
                    {
                        Text = key
                    };
                    TextBox text = new TextBox
                    {
                        Tag = key,
                        Text = fields[key].GetType().IsArray ? String.Join("\n\n", fields[key]) : fields[key]
                    };
                    if (fields[key].GetType().IsArray)
                    {
                        text.Tag += ",array";
                        text.AcceptsReturn = true;
                    }
                    else
                        text.Tag += ",notArray";
                    if (key == "_id")
                    {
                        text.IsReadOnly = true;
                        text.Background = Brushes.LightGray;
                    }
                    stack.Children.Add(title);
                    stack.Children.Add(text);
                }
            }
            Button updateButton = new Button
            {
                Content = "Update",
                Tag = id
            };
            updateButton.Click += UpdateButton;
            stack.Children.Add(updateButton);
            scroll.Content = stack;
            tab.Content = scroll;
        }
        private void BackButton(object sender, EventArgs e)
        {
            Page currPage = _settings.Pages[tabs.SelectedIndex];
            List<Dictionary<string, dynamic>> contents = currPage.Fields;
            TabItem tab = tabs.SelectedItem as TabItem;
            ListedContents(contents, tab, currPage.Name);
        }
        private void UpdateButton(object sender, EventArgs e)
        {
            //Grabs stuff for setup and setups content string
            Page currPage = _settings.Pages[tabs.SelectedIndex];
            StackPanel parent = VisualTreeHelper.GetParent(sender as DependencyObject) as StackPanel;
            string requestContent = "{";
            requestContent += "'apiKey':'" + _settings.APIKey + "',";
            //Loops through and builds json fields
            foreach (var child in parent.Children)
            {
                if (child.GetType().ToString() == "System.Windows.Controls.TextBox")
                {
                    var textBoxChild = child as TextBox;
                    string[] tags = (textBoxChild.Tag as string).Split(',');
                    string key = tags[0];
                    //Checks if it's supposed to be array.
                    string array = tags[1];
                    string[] value = textBoxChild.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    //If it is supposed to be an array then it checks the length and sets either an array or empty array.
                    requestContent += "'" + key + "':" + (array == "notArray" ? "'" + value[0] + "'" : "[" + (value.Length > 0 ? "\"" + String.Join("\",\"", value) + "\"" : "") + "]") + ",";
                }
            }
            requestContent += "}";
            //Posts to edit link
            using (HttpClient client = new HttpClient())
            {
                JObject json = JObject.Parse(requestContent);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                var result = client.PostAsync(_settings.BaseUrl + currPage.EditLink, content).Result;
                string response = result.Content.ReadAsStringAsync().Result;
                JObject jsonResponse = JObject.Parse(response);
                JToken responseMessage;
                if (jsonResponse.TryGetValue("message", out responseMessage) || jsonResponse.TryGetValue("error", out responseMessage))
                {
                    MessageBox.Show(responseMessage.ToString());
                }
            }
            if(currPage.Type.ToLower() == "listed" || currPage.Type.ToLower() == "array")
            {
                UpdatePage();
            }
        }
        private void AddButton(object sender, EventArgs e)
        {
            Page currPage = _settings.Pages[tabs.SelectedIndex];
            List<Dictionary<string, dynamic>> contents = currPage.Fields;
            Dictionary<string, dynamic> fields = contents.First();
            ScrollViewer scroll = new ScrollViewer();
            StackPanel stack = new StackPanel();
            TabItem tab = tabs.SelectedItem as TabItem;
            Button button = new Button
            {
                Margin = new Thickness(50, 20, 50, 0),
                Content = "Back"
            };
            button.Click += new RoutedEventHandler(BackButton);
            stack.Children.Add(button);
            if (currPage.Type.ToLower() == "listed")
            {
                using (HttpClient client = new HttpClient())
                {

                }
            }
            else
            {
                foreach (string key in fields.Keys)
                {
                    if (key != "_id")
                    {
                        TextBlock title = new TextBlock
                        {
                            Text = key
                        };
                        TextBox text = new TextBox
                        {
                            Tag = key,
                            Text = ""
                        };
                        if (fields[key].GetType().IsArray)
                        {
                            text.Tag += ",array";
                            text.AcceptsReturn = true;
                        }
                        else
                            text.Tag += ",notArray";
                        stack.Children.Add(title);
                        stack.Children.Add(text);
                    }
                }
            }
            Button updateButton = new Button
            {
                Content = "Add"
            };
            updateButton.Click += SendAddButton;
            stack.Children.Add(updateButton);
            scroll.Content = stack;
            tab.Content = scroll;
        }
        private void SendAddButton(object sender, EventArgs e)
        {
            //Grabs stuff for setup and setups content string
            Page currPage = _settings.Pages[tabs.SelectedIndex];
            StackPanel parent = VisualTreeHelper.GetParent(sender as DependencyObject) as StackPanel;
            string requestContent = "{";
            requestContent += "'apiKey':'" + _settings.APIKey + "',";
            //Loops through and builds json fields
            foreach (var child in parent.Children)
            {
                if (child.GetType().ToString() == "System.Windows.Controls.TextBox")
                {
                    var textBoxChild = child as TextBox;
                    string[] tags = (textBoxChild.Tag as string).Split(',');
                    string key = tags[0];
                    //Checks if it's supposed to be array.
                    string array = tags[1];
                    string[] value = textBoxChild.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    //If it is supposed to be an array then it checks the length and sets either an array or empty array.
                    requestContent += "'" + key + "':" + (array == "notArray" ? "'" + value[0] + "'" : "[" + (value.Length > 0 ? "'" + String.Join("','", value) + "'" : "") + "]") + ",";
                }
            }
            requestContent += "}";
            //Posts to edit link
            using (HttpClient client = new HttpClient())
            {
                JObject json = JObject.Parse(requestContent);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                var result = client.PostAsync(_settings.BaseUrl + currPage.AddLink, content).Result;
                string response = result.Content.ReadAsStringAsync().Result;
                JObject jsonResponse = JObject.Parse(response);
                JToken responseMessage;
                if (jsonResponse.TryGetValue("ok", out responseMessage) || jsonResponse.TryGetValue("error", out responseMessage) || jsonResponse.TryGetValue("message", out responseMessage))
                {
                    if (responseMessage.ToString() == "1")
                        MessageBox.Show("Success");
                    else
                        MessageBox.Show(responseMessage.ToString());
                }
            }
            UpdatePage();
        }
        private void UpdatePage()
        {
            Page currPage = _settings.Pages[tabs.SelectedIndex];
            currPage.Fields = new List<Dictionary<string, dynamic>>();
            using (HttpClient client = new HttpClient())
            {
                var result = client.GetStringAsync(_settings.BaseUrl + currPage.GetLink).Result;
                var pageContents = JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(result);
                foreach (Dictionary<string, dynamic> content in pageContents)
                {
                    Dictionary<string, dynamic> tempList = new Dictionary<string, dynamic>();
                    foreach (string key in content.Keys)
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
            }
        }
    }
}
