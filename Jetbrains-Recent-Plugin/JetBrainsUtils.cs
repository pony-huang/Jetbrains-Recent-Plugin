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
        static class Constants
        {
            public static readonly string[] Producers =
            {
                "idea64.exe", "pycharm64.exe", "clion64.exe", "goland64.exe", "rider64.exe",
                "appcode64.exe", "dataspell64.exe", "fleet64.exe", "phpstorm.png", "rubymine64.exe",
                "webstorm64.exe"
            };

            public static readonly string[] IconTypes =
            {
                "idea", "pycharm", "clion", "goland", "rider",
                "appcode", "dataspell", "fleet", "phpstorm", "rubymine", "webstorm"
            };
        }


        private static List<string> _producers = new List<string>(Constants.Producers);

        private static List<string> _iconType = new List<string>(Constants.IconTypes);


        private static List<RecentProjectInfo> ParseRecentProjectsXml(string filePath)
        {
            var projectInfos = new List<RecentProjectInfo>();

            // 验证 filePath 是否合法
            if (!Path.IsPathRooted(filePath) || !Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                throw new ArgumentException("Invalid file path", nameof(filePath));
            }

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

            XmlNodeList entryNodes = additionalInfoNode.SelectNodes("//entry");
            if (entryNodes == null || entryNodes.Count == 0)
            {
                return projectInfos;
            }


            try
            {
                // Get lastOpenedProject and lastProjectLocation
                string lastOpenedProject =
                    recentProjectsManagerNode.SelectSingleNode("option[@name='lastOpenedProject']")?.Attributes["value"]
                        ?.Value ?? string.Empty;
                string lastProjectLocation =
                    recentProjectsManagerNode.SelectSingleNode("option[@name='lastProjectLocation']")
                        ?.Attributes["value"]
                        ?.Value ?? string.Empty;
                foreach (XmlNode entryNode in entryNodes)
                {
                    string projectPath = entryNode.Attributes["key"]?.Value ?? "N/A";
                    var projectInfoNode = entryNode.SelectSingleNode("value/RecentProjectMetaInfo");
                    if (projectInfoNode == null)
                    {
                        continue;
                    }


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
            }
            catch (Exception ex)
            {
                Log.Info($"Error processing project: {ex.Message}", null);
            }

            return projectInfos;
        }


        public static List<RecentProjectInfo> FindJetBrainsRecentProjects()
        {
            List<RecentProjectInfo> projects = new List<RecentProjectInfo>();
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string jetBrainsFolderPath = Path.Combine(roamingPath, "JetBrains");

            if (Directory.Exists(jetBrainsFolderPath))
            {
                string[] directories = Directory.GetDirectories(jetBrainsFolderPath);

                foreach (string dir in directories)
                {
                    string recentProjectsPath = Path.Combine(dir, "options", "recentProjects.xml");

                    if (File.Exists(recentProjectsPath))
                    {
                        string productName = Path.GetFileName(dir);
                        string devToolName = GetDevToolName(productName);

                        var parsedProjects = ParseRecentProjectsXml(recentProjectsPath);
                        foreach (var project in parsedProjects)
                        {
                            project.ProductName = productName;
                            if (devToolName == "")
                            {
                                project.DeveloperToolIcon = productName.ToLower();
                            }
                            else
                            {
                                project.DeveloperTool = devToolName.ToLower();
                            }
                        }

                        projects.AddRange(parsedProjects);
                    }
                }
            }

            return projects;
        }

        private static string GetDevToolName(string productName)
        {
            foreach (string value in _iconType)
            {
                if (productName.Contains(value, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            return "";
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
                            string productName = Path.GetFileName(dir);
                            using (StreamReader sr = new StreamReader(localProjectPath))
                            {
                                string productPath = sr.ReadLine();

                                string[] files =
                                    Directory.GetFiles(productPath, "*64.exe", SearchOption.AllDirectories);
                                if (files != null && files.Length > 0)
                                {
                                    foreach (string file in files)
                                    {
                                        int idx = file.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase);
                                        string suffixName = file.Substring(idx + 1);
                                        if (_producers.Contains(suffixName))
                                        {
                                            products[productName] = file;
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
