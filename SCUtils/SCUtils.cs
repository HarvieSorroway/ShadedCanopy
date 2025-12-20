using BepInEx.Logging;
using SCUtils.DevToolUtils;
using SCUtils.SCDevTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace SCUtils
{
    public static class SCUtils
    {
        static ManualLogSource logger;
        public static void Init(ManualLogSource manualLogSource)
        {
            logger = manualLogSource;
            PlacedObjectExt.Init();
            SCDevToolsEntry.Init();
        }

        public static void Log(string log)
        {
            logger.LogDebug(log);
        }
    }
}
