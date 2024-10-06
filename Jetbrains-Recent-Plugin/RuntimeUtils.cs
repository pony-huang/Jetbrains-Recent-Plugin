using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
