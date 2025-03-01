using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using System.Windows.Controls;
using Community.PowerToys.Run.Plugin.JetBrains_Recent_Plugin.Properties;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.JetBrains_Recent_Plugin
{
    public class Main : IPlugin, IPluginI18n, IContextMenu, ISettingProvider, IReloadable, IDisposable,
        IDelayedExecutionPlugin
    {
        private const string Setting = nameof(Setting);

        private PluginInitContext _context;

        private string _iconPath;

        private bool _disposed;

        public string Name => Resources.plugin_name;

        public string Description => Resources.plugin_description;

        public static string PluginID => "5BF8F7B836D50007D3139BDF93D41B9D";

        private Dictionary<string, string> _product;

        private ContextMenuLoader _contextMenuLoader;

        // add additional options (optional)
        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>()
        {
            new()
            {
                Key = nameof(IsDisplayProjectName),
                DisplayLabel = "Show folder names only",
                DisplayDescription = "if true else show the folder name instead of the entire folder path",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Checkbox,
                Value = IsDisplayProjectName,
            },
            new()
            {
                Key = nameof(IsDisplayProjectOpenLastTime),
                DisplayLabel = "Last project opening time",
                DisplayDescription = "if true else show project last project opening time",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Checkbox,
                Value = IsDisplayProjectOpenLastTime,
            }
        };

        private bool IsDisplayProjectName { get; set; }

        private bool IsDisplayProjectOpenLastTime { get; set; }

        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            IsDisplayProjectName =
                settings.AdditionalOptions.SingleOrDefault(x => x.Key == nameof(IsDisplayProjectName))?.Value ?? false;
            IsDisplayProjectOpenLastTime =
                settings.AdditionalOptions.SingleOrDefault(x => x.Key == nameof(IsDisplayProjectOpenLastTime))?.Value ??
                false;
        }

        // return context menus for each Result (optional)
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            return _contextMenuLoader.LoadContextMenus(selectedResult);
        }

        // return query results
        public List<Result> Query(Query query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var search = query.Search;
            Log.Info("Query: " + search, GetType());

            var results = new List<Result>();

            var recentProjects = JetBrainsUtils.FindJetBrainsRecentProjects();

            // sort by activationTimestamp
            recentProjects.Sort((x, y) => y.ActivationTimestamp.CompareTo(x.ActivationTimestamp));

            if (string.IsNullOrEmpty(search))
            {
                foreach (var rp in recentProjects)
                {
                    results.Add(CreateResultFromProject(rp));
                }

                return results;
            }

            // Search by project path
            var filteredProjects = recentProjects
                .Where(rp => rp.ProjectPath.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            foreach (var rp in filteredProjects)
            {
                results.Add(CreateResultFromProject(rp));
            }

            return results;
        }

        private Result CreateResultFromProject(RecentProjectInfo rp)
        {
            string cmd = "";
            if (_product.ContainsKey(rp.ProductName))
            {
                cmd = _product[rp.ProductName];
            }

            string subTitle = "";
            if (!IsDisplayProjectOpenLastTime)
            {
                subTitle += $"{rp.ProductCodeName}";
            }
            else
            {
                DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(rp.ActivationTimestamp).DateTime;
                string formattedDate = dateTime.ToString("yy-MM-dd HH:mm:ss");
                subTitle += $"{rp.ProductCodeName} {formattedDate}";
            }

            return new Result
            {
                Title = !IsDisplayProjectName ? rp.ProjectPath : rp.ProjectName,
                SubTitle = subTitle,
                QueryTextDisplay = string.Empty,
                IcoPath = $"Images/{rp.ProductIcon}",
                Action = action =>
                {
                    if (!cmd.Equals(""))
                    {
                        Helper.OpenProject(rp.ProjectPath, cmd);
                    }

                    return true;
                },
                ContextData = new SearchResult()
                {
                    ProductName = rp.ProductName,
                    ProjectPath = rp.ProjectPath,
                    JetBrainsCmdPath = cmd
                }
            };
        }

        private long GetValueOrDefault(Dictionary<string, string> dict, string key, long defaultValue)
        {
            if (dict.TryGetValue(key, out string value))
            {
                try
                {
                    long number = long.Parse(value);
                    return number;
                }
                catch (FormatException)
                {
                    Log.Error($"The string is not in a valid format. value: {value}", GetType());
                }
                catch (OverflowException)
                {
                    Log.Error($"The number is too large or too small for a long. value: {value}", GetType());
                }
            }

            return defaultValue;
        }

        // return delayed query results (optional)
        public List<Result> Query(Query query, bool delayedExecution)
        {
            ArgumentNullException.ThrowIfNull(query);

            var results = new List<Result>();

            // empty query
            if (string.IsNullOrEmpty(query.Search))
            {
                return results;
            }

            return results;
        }

        public void Init(PluginInitContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.API.ThemeChanged += OnThemeChanged;
            _product = JetBrainsUtils.FindJetBrainsProducts();
            _contextMenuLoader = new ContextMenuLoader(context);
            UpdateIconPath(_context.API.GetCurrentTheme());
        }

        public string GetTranslatedPluginTitle()
        {
            return Resources.plugin_name;
        }

        public string GetTranslatedPluginDescription()
        {
            return Resources.plugin_description;
        }

        private void OnThemeChanged(Theme oldtheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        private void UpdateIconPath(Theme theme)
        {
            Log.Info("Update Icon Path", GetType());
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                _iconPath = "Images/Icon.light.png";
            }
            else
            {
                _iconPath = "Images/Icon.dark.png";
            }
        }

        public Control CreateSettingPanel()
        {
            throw new NotImplementedException();
        }

        public void ReloadData()
        {
            if (_context is null)
            {
                return;
            }

            UpdateIconPath(_context.API.GetCurrentTheme());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_context != null && _context.API != null)
                {
                    _context.API.ThemeChanged -= OnThemeChanged;
                }

                _disposed = true;
            }
        }
    }
}
