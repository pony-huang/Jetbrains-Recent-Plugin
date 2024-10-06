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
        public void UserLocalTest()
        {
            
            foreach(var data in JetBrainsUtils.FindJetBrainsProducts())
            {
                Console.WriteLine(data);
            }
            
        }

        [TestMethod()]
        public void UserRoamingTest()
        {
            var result = JetBrainsUtils.FindJetBrainsRecentProjects();
            foreach(var data in result)
            {
                Console.WriteLine(data);
            }

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