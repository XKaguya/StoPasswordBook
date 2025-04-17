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
        
        [Description("Never show the hints of auto update. \nDefault value: false")]
        public static bool NeverShowAgain { get; set; } = false;
        
        [IgnoreSetting]
        public static int DebugPort { get; set; } = 0;
        
        [IgnoreSetting]
        public static string DebugUrl { get; set; } = $"http://127.0.0.1:{DebugPort}";

        [IgnoreSetting]
        public static string WebSocketUrl { get; set; } = "null";
        
        [IgnoreSetting]
        public static string ObjectId { get; set; } = "null";
        
        [IgnoreSetting]
        public static int SetColumnNum { get; set; } = 0;
        
        [Description("Define how long the program will wait to send the initial credential data. \nDefault value: 2")]
        public static int WaitInterval { get; set; } = 2;
        
        [IgnoreSetting]
        public static bool IsInitSubmit { get; set; } = false;
    }
}