using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils
{
    public static class Utils
    {
        static bool logInit;
        static string path;
        public static void Log(string msg)
        {
            if (!logInit)
            {
                path = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ShadedCanopyLog.txt";
                File.WriteAllText(path, "");
                logInit = true;
            }
            File.AppendAllText(path, msg + "\n");
        }
    }
}
