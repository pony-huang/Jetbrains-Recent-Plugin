using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Wox.Infrastructure;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.Jetbrains_Recent_Plugin
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

        // TODO: add additional options (optional)
        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>()
        {
            new PluginAdditionalOption()
            {
                Key = Setting,
                DisplayLabel = Properties.Resources.plugin_setting,
                Value = false,
            },
        };

        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            _setting = settings?.AdditionalOptions?.FirstOrDefault(x => x.Key == Setting)?.Value ?? false;
        }

        // TODO: return context menus for each Result (optional)
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            return new List<ContextMenuResult>(0);
        }

        // TODO: return query results
        public List<Result> Query(Query query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var search = query.Search;
            Log.Info("Query: " + search, GetType());

            var results = new List<Result>();

            var recentProjects = JetBrainsUtils.FindJetBrains();
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
            return new Result
            {
                Title = rp.ProjectPath,
                SubTitle = rp.ProjectPath,
                QueryTextDisplay = string.Empty,
                IcoPath = _iconPath,
                Action = action =>
                {
                    // Implement the action to open the project
                    JetBrainsUtils.OpenProject(rp.ProjectPath);
                    return true;
                },
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
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                _iconPath = "Images/JetBrainsProducts/icon_48.svg";
            }
            else
            {
                _iconPath = "Images/JetBrainsProducts/icon_48.svg";
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
