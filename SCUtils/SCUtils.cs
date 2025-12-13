using SCUtils.DevToolUtils;
using SCUtils.SCDevTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils
{
    public static class SCUtils
    {
        public static void Init()
        {
            PlacedObjectExt.Init();
            SCDevToolsEntry.Init();
        }


        public static string logString;
        public static void Log(string log)
        {
            logString += $"\n[{System.DateTime.Now}]{log}";
        }
    }
}
