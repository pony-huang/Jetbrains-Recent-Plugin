using System.IO;
using System.Xml;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.JetBrains_Recent_Plugin
{
    public class RecentProjectInfo
    {
        public string ProjectPath { get; set; }
        public string Opened { get; set; }
        public string ProjectWorkspaceId { get; set; }
        public string DeveloperTool { get; set; }
        public string DeveloperToolIcon { get; set; }
        public string ProductName { get; set; }
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();

        public bool IsLastOpenedProject { get; set; }

        public override string ToString()
        {
            string optionsStr = Options.Count > 0
                ? string.Join(", ", Options.Select(kv => $"{kv.Key}: {kv.Value}"))
                : "None";

            return $"Recent Project Info:\n" +
                   $"- Project Path: {ProjectPath}\n" +
                   $"- Opened: {Opened}\n" +
                   $"- Workspace ID: {ProjectWorkspaceId}\n" +
                   $"- Developer Tool: {DeveloperTool}\n" +
                   $"- Developer Tool Icon: {DeveloperToolIcon}\n" +
                   $"- Product Name: {ProductName}\n" +
                   $"- Options: [{optionsStr}]\n" +
                   $"- Is Last Opened Project: {IsLastOpenedProject}";
        }

    }

    public class JetBrainsUtils
    {

        private static List<string> PRODUCERS = new List<string>(new string[] { "idea64.exe", "pycharm64.exe", "clion64.exe", "goland64.exe", "rider64.exe" });

        private static List<string> ICON_TYPE = new List<string>(new string[] { "idea", "pycharm", "clion", "goland", "rider" });



        public static List<RecentProjectInfo> ParseRecentProjectsXml(string filePath)
        {
            var projectInfos = new List<RecentProjectInfo>();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            // get RecentProjectsManager node
            var recentProjectsManagerNode = xmlDoc.SelectSingleNode("//component[@name='RecentProjectsManager']");

            if (recentProjectsManagerNode == null)
            {
                return projectInfos;
            }

            // get additionalInfo node
            var additionalInfoNode = recentProjectsManagerNode.SelectSingleNode("option[@name='additionalInfo']/map");
            if (additionalInfoNode == null)
            {
                return projectInfos;
            }

            // Get lastOpenedProject and lastProjectLocation
            string lastOpenedProject = recentProjectsManagerNode.SelectSingleNode("option[@name='lastOpenedProject']")?.Attributes["value"]?.Value ?? string.Empty;
            string lastProjectLocation = recentProjectsManagerNode.SelectSingleNode("option[@name='lastProjectLocation']")?.Attributes["value"]?.Value ?? string.Empty;

            foreach (XmlNode entryNode in additionalInfoNode.SelectNodes("entry"))
            {
                var projectPath = entryNode.Attributes["key"]?.Value;
                if (string.IsNullOrEmpty(projectPath))
                {
                    continue;
                }

                var projectInfoNode = entryNode.SelectSingleNode("value/RecentProjectMetaInfo");
                if (projectInfoNode == null)
                {
                    continue;
                }

                try
                {
                    var projectInfo = new RecentProjectInfo
                    {
                        ProjectPath = projectPath,
                        Opened = projectInfoNode.Attributes["opened"]?.Value ?? "N/A",
                        ProjectWorkspaceId = projectInfoNode.Attributes["projectWorkspaceId"]?.Value ?? "N/A",
                        IsLastOpenedProject = projectPath == lastOpenedProject
                    };

                    // option node
                    foreach (XmlNode optionNode in projectInfoNode.SelectNodes("option"))
                    {
                        var optionName = optionNode.Attributes["name"]?.Value ?? "Unknown";
                        var optionValue = optionNode.Attributes["value"]?.Value ?? "N/A";
                        projectInfo.Options[optionName] = optionValue;
                    }

                    projectInfos.Add(projectInfo);
                }
                catch (Exception ex)
                {
                    Log.Info($"Error processing project: {ex.Message}", null);
                }
            }

            return projectInfos;
        }


        public static List<RecentProjectInfo> FindJetBrainsRecentProjects()
        {

            List<RecentProjectInfo> projects = new List<RecentProjectInfo>();
            // Get User AppData\Roaming Path
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // ../Roaming//JetBrains
            string jetBrainsFolderPath = Path.Combine(roamingPath, "JetBrains");
            if (Directory.Exists(jetBrainsFolderPath))
            {
                string[] files = Directory.GetFiles(jetBrainsFolderPath);
                string[] directories = Directory.GetDirectories(jetBrainsFolderPath);

                foreach (string dir in directories)
                {
                    string recentProjectsPath = Path.Combine(dir, "options", "recentProjects.xml");

                    if (File.Exists(recentProjectsPath))
                    {

                        var ProductName = dir.Substring(dir.LastIndexOf("\\") + 1);
                        var DevToolName = "";

                        foreach (string value in ICON_TYPE)
                        {
                            if (ProductName.Contains(value, StringComparison.OrdinalIgnoreCase))
                            {
                                DevToolName = value;
                                break;
                            }
                        }

                        if (File.Exists(recentProjectsPath))
                        {
                            var parsedProjects = JetBrainsUtils.ParseRecentProjectsXml(recentProjectsPath);
                            foreach (var project in parsedProjects)
                            {
                                project.ProductName = ProductName;
                                project.DeveloperTool = DevToolName.ToLower();
                            }
                            projects.AddRange(parsedProjects);

                        }
                    }
                }
            }
            return projects;

        }


        public static Dictionary<string, string> FindJetBrainsProducts()
        {

            Dictionary<string, string> products = new Dictionary<string, string>();
            string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string jetBrainsFolderPath = Path.Combine(localPath, "JetBrains");
            if (Directory.Exists(jetBrainsFolderPath))
            {

                string[] directories = Directory.GetDirectories(jetBrainsFolderPath);

                foreach (string dir in directories)
                {
                    string localProjectPath = Path.Combine(dir, ".home");

                    if (File.Exists(localProjectPath))
                    {

                        try
                        {
                            string ProductName = Path.GetFileName(dir);
                            using (StreamReader sr = new StreamReader(localProjectPath))
                            {
                                // only on line
                                string ProductPath = sr.ReadLine();

                                string[] files = Directory.GetFiles(ProductPath, "*64.exe", SearchOption.AllDirectories);
                                if (files != null && files.Length > 0)
                                {
                                    foreach (string file in files)
                                    {
                                        var idx = file.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase);
                                        var SuffixName = file.Substring(idx + 1);
                                        if (PRODUCERS.Contains(SuffixName))
                                        {
                                            products[ProductName] = file;
                                        }
                                    }

                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error($"Failed to read .home. err: {e.Message} ", null);
                        }

                    }
                }
            }
            return products;

        }

    }

}

