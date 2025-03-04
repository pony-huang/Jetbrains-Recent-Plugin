using System.Diagnostics;
using System.IO;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.JetBrains_Recent_Plugin
{
    public class Helper
    {
        public static bool OpenProject(string projectPath, string exeJetBrainsIdePath, bool runAsAdmin = false)
        {
            if (!File.Exists(exeJetBrainsIdePath))
            {
                Log.Info($"JetBrains IDE executable not found at {exeJetBrainsIdePath}", null);
                return false;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exeJetBrainsIdePath,
                    Arguments = $"\"{projectPath}\"",
                    UseShellExecute = true
                };

                //  runAsAdmin
                if (runAsAdmin)
                {
                    startInfo.Verb = "runas";
                }

                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                Log.Info($"Error opening project: {ex.Message}", null);
            }

            return false;
        }

        public static bool OpenProjectInExplorer(string projectPath)
        {
            try
            {
                if (File.Exists(projectPath))
                {
                   // rider project  xxx.sln
                    if (projectPath.EndsWith(".sln"))
                    {
                        projectPath = projectPath.Substring(0, projectPath.LastIndexOf("/"));
                    }
                }
                projectPath = projectPath.Replace("/", "\\");
                Process.Start("explorer.exe", projectPath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Info($"Error opening project: {ex.Message}", null);
            }

            return false;
        }
    }
}
