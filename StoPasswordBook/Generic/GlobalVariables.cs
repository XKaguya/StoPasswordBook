using System.ComponentModel;
using static StoPasswordBook.Generic.Api;

namespace StoPasswordBook.Generic
{
    public class GlobalVariables
    {
        [Description("Set to true to allow program self update. \nDefault value: true")]
        public static bool AutoUpdate { get; set; } = true;
        
        [Description("Launcher path. Do not touch this unless you're reinstalled your STO. \nDefault value: null")]
        public static string LauncherPath { get; set; } = "null";
        
        [IgnoreSetting]
        public static int DebugPort { get; set; } = 0;
        
        [IgnoreSetting]
        public static string DebugUrl { get; set; } = $"http://127.0.0.1:{DebugPort}";

        [IgnoreSetting]
        public static string WebSocketUrl { get; set; } = "null";
    }
}