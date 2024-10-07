using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.JetBrains_Recent_Plugin
{
    public class ContextMenuLoader : IContextMenu
    {

        private readonly PluginInitContext _context;

        public ContextMenuLoader(PluginInitContext context)
        {
            _context = context;
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            var contextMenus = new List<ContextMenuResult>();
            if (selectedResult.ContextData is SearchResult record)
            {


                contextMenus.Add(CreateRunAsAdminContextMenu(record));
                contextMenus.Add(CreateRunAsUserContextMenu(record));

                contextMenus.Add(new ContextMenuResult
                {
                    PluginName = Assembly.GetExecutingAssembly().GetName().Name,
                    Title = "Copy (Enter)",
                    Glyph = "\xE8C8",
                    FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                    AcceleratorKey = Key.C,
                    AcceleratorModifiers = ModifierKeys.Control,

                    Action = (context) =>
                    {
                        try
                        {
                            Clipboard.SetText(record.ProjectPath);
                            return true;
                        }
                        catch (Exception e)
                        {
                            var message = "Fail to set text in clipboard";
                            Log.Exception(message, e, GetType());

                            _context.API.ShowMsg(message);
                            return false;
                        }
                    },
                });
            }

            return contextMenus;
        }

        private static ContextMenuResult CreateRunAsAdminContextMenu(SearchResult record)
        {
            return new ContextMenuResult
            {
                PluginName = Assembly.GetExecutingAssembly().GetName().Name,
                Title = "Run as administrator (Ctrl+Shift+Enter)",
                Glyph = "\xE7EF",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                AcceleratorKey = Key.Enter,
                AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                Action = _ =>
                {
                    try
                    {
                        Task.Run(() =>
                        {
                            Helper.OpenProject(record.ProjectPath, record.JetBrainsCmdPath, true);
                        });
                        return true;
                    }
                    catch (Exception e)
                    {
                        Log.Exception($"Failed to run {record.ProjectPath} as admin, {e.Message}", e, MethodBase.GetCurrentMethod().DeclaringType);
                        return false;
                    }
                },
            };
        }

        // Function to add the context menu item to run as admin
        private static ContextMenuResult CreateRunAsUserContextMenu(SearchResult record)
        {
            return new ContextMenuResult
            {
                PluginName = Assembly.GetExecutingAssembly().GetName().Name,
                Title = "Run as different user (Ctrl+Shift+U)",
                Glyph = "\xE7EE",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                AcceleratorKey = Key.U,
                AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                Action = _ =>
                {
                    try
                    {
                        Task.Run(() =>
                        {
                            Helper.OpenProject(record.ProjectPath, record.JetBrainsCmdPath);
                        });
                        return true;
                    }
                    catch (Exception e)
                    {
                        Log.Exception($"Failed to run {record.ProjectPath} as different user, {e.Message}", e, MethodBase.GetCurrentMethod().DeclaringType);
                        return false;
                    }
                },
            };
        }

    }
}
