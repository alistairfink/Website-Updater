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
        public string Name { get; set; }
    }
}
