using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Community.PowerToys.Run.Plugin.JetBrains_Recent_Plugin
{
    public class SearchResult
    {
        public SearchResult() { }

        public string ProductName { get; set; }
        public string ProjectPath { get; set; }
        public string JetBrainsCmdPath  { get; set; }
    }
}
