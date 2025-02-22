using System.IO;
using System.Xml;
using Wox.Plugin;
using Wox.Plugin.Logger;


namespace Community.PowerToys.Run.Plugin.JetBrains_Recent_Plugin
{
    public class RecentProjectInfo
    {
        public string ProjectPath { get; set; }
        public string Opened { get; set; }
        public string ProjectWorkspaceId { get; set; }
        public string ProductIcon { get; set; }
        public string ProductName { get; set; }
        public long ProjectOpenTimestamp { get; set; }
        public long ActivationTimestamp { get; set; }
        public string ProductCodeName { get; set; }
        public string ProjectName { get; set; }
        public Dictionary<string, string> Options { get; set; } = new();

        public bool IsLastOpenedProject { get; set; }

        public override string ToString()
        {
            var optionsStr = Options.Count > 0
                ? string.Join(", ", Options.Select(kv => $"{kv.Key}: {kv.Value}"))
                : "None";

            return $"Recent Project Info:\n" +
                   $"- Project Path: {ProjectPath}\n" +
                   $"- Opened: {Opened}\n" +
                   $"- Workspace ID: {ProjectWorkspaceId}\n" +
                   $"- Developer Tool Icon: {ProductIcon}\n" +
                   $"- Product Name: {ProductName}\n" +
                   $"- Project Name: {ProductName}\n" +
                   $"- Options: [{optionsStr}]\n" +
                   $"- Is Last Opened Project: {IsLastOpenedProject}";
        }
    }

    public class JetBrainsUtils
    {
        private static readonly HashSet<string> Producers = new()
        {
            "idea64.exe", "pycharm64.exe", "clion64.exe", "goland64.exe", "rider64.exe",
            "appcode64.exe", "dataspell64.exe", "fleet64.exe", "phpstorm64.exe", "rubymine64.exe",
            "webstorm64.exe"
        };

        private static readonly Dictionary<string, string> CODE_PRODUCT_ICON_DICT = new()
        {
            { "IC", "ideac.png" },
            { "IE", "idea.png" },
            { "PS", "phpstorm.png" },
            { "WS", "webstorm.png" },
            { "PY", "pycharm.png" },
            { "PC", "pycharmc.png" },
            { "PE", "pycharmc.png" },
            { "RM", "rubymine.png" },
            { "OC", "appcode.png" },
            { "CL", "clion.png" },
            { "GO", "goland.png" },
            // { "DB", "DataGrip" },
            { "RD", "rider.png" },
            // { "AI", "Android Studio" },
            { "RR", "rustrover.png" },
            // { "QA", "Aqua" }
        };

        private static readonly Dictionary<string, string> CODE_PRODUCT_DICT = new Dictionary<string, string>()
        {
            { "IU", "IntelliJ IDEA Ultimate" },
            { "IC", "IntelliJ IDEA Community" },
            { "IE", "IntelliJ IDEA Educational" },
            { "PS", "PhpStorm" },
            { "WS", "WebStorm" },
            { "PY", "PyCharm Professional" },
            { "PC", "PyCharm Community" },
            { "PE", "PyCharm Educational" },
            { "RM", "RubyMine" },
            { "OC", "AppCode" },
            { "CL", "CLion" },
            { "GO", "GoLand" },
            { "DB", "DataGrip" },
            { "RD", "Rider" },
            { "AI", "Android Studio" },
            { "RR", "RustRover" },
            { "QA", "Aqua" }
        };


        private static List<RecentProjectInfo> ParseRecentProjectsXml(string filePath)
        {
            var projectInfos = new List<RecentProjectInfo>();

            if (!Path.IsPathRooted(filePath) || !Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                throw new ArgumentException($"Invalid file path, filepath: {filePath}", nameof(filePath));
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            var recentProjectsManagerNode = xmlDoc.SelectSingleNode("//component[@name='RecentProjectsManager']");
            if (recentProjectsManagerNode == null) return projectInfos;

            var additionalInfoNode = recentProjectsManagerNode.SelectSingleNode("option[@name='additionalInfo']/map");
            if (additionalInfoNode == null) return projectInfos;

            var entryNodes = additionalInfoNode.SelectNodes("entry");
            if (entryNodes == null || entryNodes.Count == 0) return projectInfos;

            try
            {
                var lastOpenedProject =
                    recentProjectsManagerNode.SelectSingleNode("option[@name='lastOpenedProject']")?.Attributes["value"]
                        ?.Value ?? string.Empty;

                // Log.Info("Last Opened Project: {lastOpenedProject}", typeof(JetBrainsUtils));

                foreach (XmlNode entryNode in entryNodes)
                {
                    if (entryNode.Attributes == null) continue;
                    var projectPath = entryNode.Attributes["key"]?.Value ?? "N/A";
                    var projectInfoNode = entryNode.SelectSingleNode("value/RecentProjectMetaInfo");
                    if (projectInfoNode == null) continue;

                    var projectName = projectPath.Substring(projectPath.LastIndexOf("/") + 1);

                    var opened = "N/A";
                    if (projectInfoNode.Attributes != null)
                        opened = projectInfoNode.Attributes["path"]?.Value ?? "N/A";

                    var projectWorkspaceId = "N/A";
                    if (projectInfoNode.Attributes != null)
                        projectWorkspaceId = projectInfoNode.Attributes["projectWorkspaceId"]?.Value ?? "N/A";

                    var projectInfo = new RecentProjectInfo
                    {
                        Opened = opened,
                        ProjectPath = projectPath,
                        ProjectName = projectName,
                        ProjectWorkspaceId = projectWorkspaceId,
                        IsLastOpenedProject = projectPath == lastOpenedProject,
                    };

                    var nodes = projectInfoNode.SelectNodes("option");
                    if (nodes == null) continue;

                    foreach (XmlNode optionNode in nodes)
                    {
                        var optionName = "Unknown";
                        if (optionNode.Attributes != null)
                            optionName = optionNode.Attributes["name"]?.Value ?? "Unknown";

                        var optionValue = "N/A";
                        if (optionNode.Attributes != null)
                            optionValue = optionNode.Attributes["value"]?.Value ?? "N/A";
                        projectInfo.Options[optionName] = optionValue;
                    }

                    if (projectInfo.Options.Count > 0)
                    {
                        if (projectInfo.Options.TryGetValue("productionCode", out var productionCode))
                        {
                            projectInfo.ProductIcon = CODE_PRODUCT_ICON_DICT.GetValueOrDefault(productionCode, "ideac.png");
                            projectInfo.ProductCodeName = CODE_PRODUCT_DICT.GetValueOrDefault(productionCode, "Unknown");
                        }
                        if (projectInfo.Options.TryGetValue("projectOpenTimestamp", out var projectOpenTimestampStr))
                        {
                            projectInfo.ProjectOpenTimestamp = long.Parse(projectOpenTimestampStr);
                        }
                        if (projectInfo.Options.TryGetValue("activationTimestamp", out var activationTimestampStr))
                        {
                            projectInfo.ActivationTimestamp = long.Parse(activationTimestampStr);
                        }
                    }


                    projectInfos.Add(projectInfo);
                }
            }
            catch (Exception ex)
            {
                Log.Info($"Error processing project: {ex.Message}", typeof(JetBrainsUtils));
            }

            return projectInfos;
        }

        public static List<RecentProjectInfo> FindJetBrainsRecentProjects()
        {
            var projects = new List<RecentProjectInfo>();
            var roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var jetBrainsFolderPath = Path.Combine(roamingPath, "JetBrains");

            if (Directory.Exists(jetBrainsFolderPath))
            {
                foreach (var dir in Directory.GetDirectories(jetBrainsFolderPath))
                {
                    var recentProjectsPath = Path.Combine(dir, "options", "recentProjects.xml");
                    if (!File.Exists(recentProjectsPath)) continue;

                    var productName = Path.GetFileName(dir);

                    var parsedProjects = ParseRecentProjectsXml(recentProjectsPath);
                    foreach (var project in parsedProjects)
                    {
                        project.ProductName = productName;
                    }

                    projects.AddRange(parsedProjects);
                }
            }

            return projects;
        }


        public static Dictionary<string, string> FindJetBrainsProducts()
        {
            var products = new Dictionary<string, string>();
            var localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var jetBrainsFolderPath = Path.Combine(localPath, "JetBrains");

            if (Directory.Exists(jetBrainsFolderPath))
            {
                foreach (var dir in Directory.GetDirectories(jetBrainsFolderPath))
                {
                    var localProjectPath = Path.Combine(dir, ".home");

                    if (!File.Exists(localProjectPath)) continue;

                    try
                    {
                        var productName = Path.GetFileName(dir);
                        using var sr = new StreamReader(localProjectPath);
                        var productPath = sr.ReadLine();
                        if (string.IsNullOrEmpty(productPath)) continue;

                        var files = Directory.GetFiles(productPath, "*64.exe", SearchOption.AllDirectories);
                        if (files.Length > 0)
                        {
                            foreach (var file in files)
                            {
                                var suffixName = Path.GetFileName(file);
                                if (Producers.Contains(suffixName))
                                {
                                    products[productName] = file;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Failed to read .home. err: {e.Message}", typeof(JetBrainsUtils));
                    }
                }
            }

            return products;
        }
    }
}
