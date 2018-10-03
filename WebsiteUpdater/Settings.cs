using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteUpdater
{
    public class Settings
    {
        public string BaseUrl { get; set; }
        public string APIKey { get; set; }
        public List<Page> Pages { get; set; }
    }
    public class Page
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string GetLink { get; set; }
        public string GetListItemLink { get; set; }
        public string EditLink { get; set; }
        public string AddLink { get; set; }
        public List<Dictionary<string, dynamic>> Fields { get; set; }
    }
}
