using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Community.PowerToys.Run.Plugin.Jetbrains_Recent_Plugin
{
    public class RecentProjectInfo
    {
        public string ProjectPath { get; set; }
        public string Opened { get; set; }
        public string ProjectWorkspaceId { get; set; }
        public string DeveloperTool { get; set; }
        public string DeveloperToolIcon { get; set; }
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();

        public bool IsLastOpenedProject { get; set; }

        public override string ToString()
        {
            string optionsStr = string.Join(", ", Options.Select(kv => $"{kv.Key}: {kv.Value}"));
            return $"Project Path: {ProjectPath}, Opened: {Opened}, Workspace ID: {ProjectWorkspaceId}, Options: [{optionsStr}]";
        }
    }

    public class JetBrainsUtils
    {
        public static List<RecentProjectInfo> ParseRecentProjectsXml(string filePath)
        {
            var projectInfos = new List<RecentProjectInfo>();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            // 获取 RecentProjectsManager 节点
            var recentProjectsManagerNode = xmlDoc.SelectSingleNode("//component[@name='RecentProjectsManager']");

            if (recentProjectsManagerNode == null)
            {
                return projectInfos; // 如果找不到对应节点，直接返回空列表
            }

            // 获取 additionalInfo 选项节点
            var additionalInfoNode = recentProjectsManagerNode.SelectSingleNode("option[@name='additionalInfo']/map");
            if (additionalInfoNode == null)
            {
                return projectInfos; // 如果找不到 additionalInfo，直接返回空列表
            }

            // 获取 lastOpenedProject 和 lastProjectLocation
            string lastOpenedProject = recentProjectsManagerNode.SelectSingleNode("option[@name='lastOpenedProject']")?.Attributes["value"]?.Value ?? string.Empty;
            string lastProjectLocation = recentProjectsManagerNode.SelectSingleNode("option[@name='lastProjectLocation']")?.Attributes["value"]?.Value ?? string.Empty;

            // 遍历 additionalInfo 节点
            foreach (XmlNode entryNode in additionalInfoNode.SelectNodes("entry"))
            {
                var projectPath = entryNode.Attributes["key"]?.Value;
                if (string.IsNullOrEmpty(projectPath))
                {
                    continue; // 如果 projectPath 为空，跳过
                }

                var projectInfoNode = entryNode.SelectSingleNode("value/RecentProjectMetaInfo");
                if (projectInfoNode == null)
                {
                    continue; // 如果没有找到 RecentProjectMetaInfo 节点，跳过
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

                    // 处理 option 节点
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
                    // 记录日志或其他异常处理方式
                    Console.WriteLine($"Error processing project: {ex.Message}");
                }
            }

            return projectInfos;
        }


        public static List<RecentProjectInfo> FindJetBrains()
        {
            // 初始化一个列表来存储解析的项目信息
            List<RecentProjectInfo> projects = new List<RecentProjectInfo>();
            // Get User AppData\Roaming Path
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // 构建 Roaming 下 JetBrains 文件夹的完整路径
            string jetBrainsFolderPath = Path.Combine(roamingPath, "JetBrains");
            // 检查 JetBrains 文件夹是否存在
            if (Directory.Exists(jetBrainsFolderPath))
            {

                // 获取 JetBrains 文件夹中的所有文件和子文件夹
                string[] files = Directory.GetFiles(jetBrainsFolderPath);
                string[] directories = Directory.GetDirectories(jetBrainsFolderPath);



                foreach (string dir in directories)
                {
                    string recentProjectsPath = Path.Combine(dir, "options", "recentProjects.xml");

                    if (File.Exists(recentProjectsPath))
                    {

                        // 读取并输出 recentProjects.xml 文件内容
                        if (File.Exists(recentProjectsPath))
                        {
                            var parsedProjects = JetBrainsUtils.ParseRecentProjectsXml(recentProjectsPath);
                            projects.AddRange(parsedProjects);

                        }
                    }
                }
            }
            return projects;

        }

        public static void OpenProject(string path)
        {

        }

    }

}
