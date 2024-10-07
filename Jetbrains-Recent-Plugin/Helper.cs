using System.Diagnostics;
using System.IO;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.JetBrains_Recent_Plugin
{
    internal class Helper
    {
        public static bool OpenProject(string path, string jetBrainsIDEPath, bool runAsAdmin = false)
        {
            if (!File.Exists(jetBrainsIDEPath))
            {
                Log.Info($"JetBrains IDE executable not found at {jetBrainsIDEPath}", null);
                return false;
            }
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = jetBrainsIDEPath,
                    Arguments = $"\"{path}\"",
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
    }
}
