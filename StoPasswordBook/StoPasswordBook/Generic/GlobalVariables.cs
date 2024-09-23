using System.ComponentModel;
using static StoPasswordBook.Generic.Api;

namespace StoPasswordBook.Generic
{
    public class GlobalVariables
    {
        [Description("Launcher path. Do not touch this unless you're reinstalled your STO. \nDefault value: null")]
        public static string LauncherPath { get; set; } = "null";
        
        [IgnoreSetting]
        public static int DebugPort { get; set; } = 0;
        
        [IgnoreSetting]
        public static string DebugUrl { get; set; } = $"http://127.0.0.1:{DebugPort}";

        [IgnoreSetting]
        public static string WebSocketUrl { get; set; } = "null";
        
        [Description("How many seconds the program will wait. \nDefault value: 5")]
        public static int WaitInterval { get; set; } = 10;
    }
}