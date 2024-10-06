using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using System.Windows;
using System.Windows.Controls;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.JetBrains_Recent_Plugin
{
    public class Main : IPlugin, IPluginI18n, IContextMenu, ISettingProvider, IReloadable, IDisposable, IDelayedExecutionPlugin
    {
        private const string Setting = nameof(Setting);

        // current value of the setting
        private bool _setting;

        private PluginInitContext _context;

        private string _iconPath;

        private bool _disposed;

        public string Name => Properties.Resources.plugin_name;

        public string Description => Properties.Resources.plugin_description;

        // TODO: remove dash from ID below and inside plugin.json
        public static string PluginID => "e4df183d-1167-4fe0-ab12-cdfbff053e57";

        private Settings _settings = new();

        private Dictionary<string, string> _product;

        private ContextMenuLoader _contextMenuLoader;

        // TODO: add additional options (optional)
        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>()
        {
            //new PluginAdditionalOption()
            //{
            //    Key = nameof(_settings.Default),
            //    DisplayLabel = "Count spaces",
            //    DisplayDescription = "Count spaces as characters",
            //    PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Checkbox,
            //    Value = (bool)_setting.Default
            //}
        };

        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            //_setting = settings?.AdditionalOptions?.FirstOrDefault(x => x.Key == Setting)?.Value ?? false;
            // TODO
        }

        // TODO: return context menus for each Result (optional)
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            return _contextMenuLoader.LoadContextMenus(selectedResult);
        }

        private static bool CopyToClipboard(string? value)
        {
            if (value != null)
            {
                Clipboard.SetText(value);
            }

            return true;
        }

        // TODO: return query results
        public List<Result> Query(Query query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var search = query.Search;
            Log.Info("Query: " + search, GetType());

            var results = new List<Result>();

            var recentProjects = JetBrainsUtils.FindJetBrainsRecentProjects();
            // empty query
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
            string Cmd = "";
            if (_product.ContainsKey(rp.ProductName))
            {
                Cmd = _product[rp.ProductName];
            }

            return new Result
            {
                Title = rp.ProjectPath,
                SubTitle = rp.ProjectPath,
                QueryTextDisplay = string.Empty,
                IcoPath = $"Images/{rp.DeveloperTool}.svg",
                Action = action =>
                {
                    if (!Cmd.Equals(""))
                    {
                        JetBrainsUtils.OpenProject(rp.ProjectPath, Cmd);
                    }

                    return true;
                },
                ContextData = new SearchResult()
                {
                    ProductName = rp.ProductName,
                    ProjectPath = rp.ProjectPath,
                    JetBrainsCmdPath = Cmd
                }
            };
        }

        // TODO: return delayed query results (optional)
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
            return Properties.Resources.plugin_name;
        }

        public string GetTranslatedPluginDescription()
        {
            return Properties.Resources.plugin_description;
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
                _iconPath = "Images/idea.svg";
            }
            else
            {
                _iconPath = "Images/idea.svg";
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
