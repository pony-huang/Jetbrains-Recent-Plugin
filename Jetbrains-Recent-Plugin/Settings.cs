using System.IO;
using System.Reflection;

namespace Community.PowerToys.Run.Plugin.JetBrains_Recent_Plugin
{
    public class Settings
    {
        public string Context { get; set; } = "012345";
        public bool Default { get; set; }
        public bool MatchPath { get; set; }
        public bool Preview { get; set; } = true;
        public bool QueryText { get; set; }
        public bool RegEx { get; set; }
        public bool EnvVar { get; set; }
        public bool Updates { get; set; } = true;
        public string Skip { get; set; }
        public string Prefix { get; set; }
        public string EverythingPath { get; set; }
        public bool ShowMore { get; set; } = true;

        // Get Filters from settings.toml
        public Dictionary<string, string> Filters { get; } = [];
        internal void Getfilters()
        {
            string[] strArr;
            try { strArr = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.toml")); }
            catch (Exception e)
            {
                return;
            }

            foreach (string str in strArr)
            {
                if (str.Length == 0 || str[0] == '#') continue;
                string[] kv = str.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (kv.Length != 2) continue;

                if (kv[0].Contains(':'))
                    Filters.TryAdd(kv[0].ToLowerInvariant(), kv[1] + (kv[1].EndsWith(';') ? ' ' : string.Empty));
            }

        }
    }
}
