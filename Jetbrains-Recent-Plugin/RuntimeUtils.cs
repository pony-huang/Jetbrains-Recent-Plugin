using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.JetBrains_Recent_Plugin
{
    internal class RuntimeUtils
    {

        public static bool RunCmd(ActionContext context)
        {
            Log.Info($"{context}", null);
            return true;
        }
    }
}
