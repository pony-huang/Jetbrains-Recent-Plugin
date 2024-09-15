using Community.PowerToys.Run.Plugin.Jetbrains_Recent_Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Community.PowerToys.Run.Plugin.Jetbrains_Recent_Plugin.Tests
{
    [TestClass()]
    public class MainTests
    {

        [TestMethod()]
        public void UserRoamingTest()
        {
            Trace.Listeners.Add(new TextWriterTraceListener("TextWriterOutput.log", "myListener"));
            // Get User AppData\Roaming Path
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Trace.TraceInformation($"Roaming Path of {roamingPath}");

            // 构建 Roaming 下 JetBrains 文件夹的完整路径
            string jetBrainsFolderPath = Path.Combine(roamingPath, "JetBrains");

            // 检查 JetBrains 文件夹是否存在
            if (Directory.Exists(jetBrainsFolderPath))
            {
                Trace.TraceInformation($"JetBrains Folder Path: {jetBrainsFolderPath}\n");

                // 获取 JetBrains 文件夹中的所有文件和子文件夹
                string[] files = Directory.GetFiles(jetBrainsFolderPath);
                string[] directories = Directory.GetDirectories(jetBrainsFolderPath);

                // 列出所有子文件夹
                Trace.TraceInformation("Directories:");

                // 初始化一个列表来存储解析的项目信息
                List<RecentProjectInfo> projects = new List<RecentProjectInfo>();

                foreach (string dir in directories)
                {
                    string recentProjectsPath = Path.Combine(dir, "options", "recentProjects.xml");

                    if (File.Exists(recentProjectsPath))
                    {
                        Trace.TraceInformation($"Found recentProjects.xml in: {recentProjectsPath}");

                        // 读取并输出 recentProjects.xml 文件内容
                        if (File.Exists(recentProjectsPath))
                        {
                            Trace.TraceInformation($"Found recentProjects.xml in: {recentProjectsPath}");
                            var parsedProjects = JetBrainsUtils.ParseRecentProjectsXml(recentProjectsPath);
                            projects.AddRange(parsedProjects);



                        }
                        else
                        {
                            Trace.TraceInformation($"recentProjects.xml not found in: {dir}");
                        }
                    }
                    else
                    {
                        Trace.TraceInformation($"recentProjects.xml not found in: {dir}");
                    }
                }

                // 列出所有文件
                Trace.TraceInformation("\nFiles:");
                foreach (string file in files)
                {
                    Trace.TraceInformation($"  {file}");
                }

                // 打印解析的结果
                foreach (var project in projects)
                {
                    Trace.TraceInformation(project.ToString());
                }
            }
            else
            {
                Trace.TraceInformation("JetBrains folder not found in the Roaming directory.");
            }


        }

        [TestMethod()]
        public void RecentXMLFileTest()
        {
            Trace.Listeners.Add(new TextWriterTraceListener("TextWriterOutput.log", "myListener"));

            string xmlContent = @"
<application>
  <component name='RecentProjectsManager'>
    <option name='additionalInfo'>
      <map>
        <entry key='D:/CLionProjects/redis'>
          <value>
            <RecentProjectMetaInfo opened='true' projectWorkspaceId='2XAmAKvzK0sEZpUPdIPYPdLGT4G'>
              <option name='activationTimestamp' value='1698569699315' />
              <option name='binFolder' value='$APPLICATION_HOME_DIR$/bin' />
              <option name='build' value='CL-232.9921.42' />
              <option name='buildTimestamp' value='1694512422856' />
              <option name='colorInfo'>
                <RecentProjectColorInfo associatedIndex='2' />
              </option>
              <option name='productionCode' value='CL' />
              <option name='projectOpenTimestamp' value='1698080114404' />
            </RecentProjectMetaInfo>
          </value>
        </entry>
        <entry key='D:/CLionProjects/demo1'>
          <value>
            <RecentProjectMetaInfo opened='true' projectWorkspaceId='2XQmbOY2VoqAkcZfIdUsMRTdtRH'>
              <option name='activationTimestamp' value='1698569748720' />
              <option name='binFolder' value='$APPLICATION_HOME_DIR$/bin' />
              <option name='build' value='CL-232.9921.42' />
              <option name='buildTimestamp' value='1694512412041' />
              <option name='colorInfo'>
                <RecentProjectColorInfo associatedIndex='3' />
              </option>
              <option name='productionCode' value='CL' />
              <option name='projectOpenTimestamp' value='1698569749074' />
            </RecentProjectMetaInfo>
          </value>
        </entry>
      </map>
    </option>
    <option name='lastOpenedProject' value='D:/CLionProjects/demo1' />
    <option name='lastProjectLocation' value='D:/CLionProjects' />
  </component>
</application>";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);

            XmlNodeList projectEntries = xmlDoc.SelectNodes("//entry");

            foreach (XmlNode entry in projectEntries)
            {
                string projectPath = entry.Attributes["key"].Value;
                XmlNode projectInfo = entry.SelectSingleNode("value/RecentProjectMetaInfo");

                string opened = projectInfo.Attributes["opened"].Value;
                string projectWorkspaceId = projectInfo.Attributes["projectWorkspaceId"].Value;

                Trace.TraceInformation($"Project Path: {projectPath}");
                Trace.TraceInformation($"  Opened: {opened}");
                Trace.TraceInformation($"  Workspace ID: {projectWorkspaceId}");

                foreach (XmlNode option in projectInfo.SelectNodes("option"))
                {
                    string optionName = option.Attributes["name"].Value;
                    string optionValue = option.Attributes["value"]?.Value ?? "N/A";
                    Trace.TraceInformation($"  {optionName}: {optionValue}");
                }

                Trace.TraceInformation("\n");
            }
            Trace.Flush();
        }

        [TestMethod()]
        public void UpdateSettingsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void LoadContextMenusTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void QueryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void QueryTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InitTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetTranslatedPluginTitleTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetTranslatedPluginDescriptionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateSettingPanelTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ReloadDataTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DisposeTest()
        {
            Assert.Fail();
        }
    }
}