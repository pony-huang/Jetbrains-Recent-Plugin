using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.JetBrains_Recent_Plugin.Tests
{
    [TestClass()]
    public class MainTests
    {
        [TestMethod()]
        public void UserRoamingTest()
        {
            var recentProjects = JetBrainsUtils.FindJetBrainsRecentProjects();
            Console.WriteLine($"beffore count: {recentProjects.Count()}");
            Log.Info($"beffore count: {recentProjects.Count()}", typeof(JetBrainsUtils));

            long GetValueOrDefault(Dictionary<string, string> dict, string key, long defaultValue)
            {
                if (dict.TryGetValue(key, out string value))
                {
                    try
                    {
                        long number = long.Parse(value);
                        return number;
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"The string is not in a valid format. value: {value}");
                    }
                    catch (OverflowException)
                    {
                        Console.WriteLine($"The number is too large or too small for a long. value: {value}");
                    }
                }

                return defaultValue;
            }

            // sort by activationTimestamp
            recentProjects.Sort((x, y) =>
                GetValueOrDefault(y.Options, "activationTimestamp", 0)
                    .CompareTo(GetValueOrDefault(x.Options, "activationTimestamp", 0)));


            var distinctRecentProjects = recentProjects.GroupBy(p => p.ProjectPath).Select(g => g.First()).ToList();
            Console.WriteLine($"affter count: {distinctRecentProjects.Count()}");


            foreach (var item in distinctRecentProjects)
            {
                Console.WriteLine(item);
            }
        }
    }
}