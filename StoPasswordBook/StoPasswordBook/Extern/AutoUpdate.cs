using System.Diagnostics;
using System.IO;
using System.Windows;
using log4net;
using StoPasswordBook;
using StoPasswordBook.Generic;

namespace STOTool.Feature
{
    public class AutoUpdate
    {
        private static readonly string Author = "Xkaguya";
        private static readonly string Project = "StoPasswordBook";
        private static readonly string ExeName = "StoPasswordBook.exe";
        private static readonly string CurrentExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ExeName);
        private static readonly string NewExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StoPasswordBook-New.exe");
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private static readonly ILog Log = LogManager.GetLogger(typeof(AutoUpdate));

        public static void StartAutoUpdateTask()
        {
            Task.Run(async () => await AutoUpdateTask(CancellationTokenSource.Token));
        }

        private static async Task AutoUpdateTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                CheckAndUpdate();
                await Task.Delay(TimeSpan.FromHours(1), token);
            }
        }

        public static void CheckAndUpdate()
        {
            if (!GlobalVariables.AutoUpdate)
            {
                return;
            }
            
            try
            {
                string commonUpdaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CommonUpdater.exe");

                if (!File.Exists(commonUpdaterPath))
                {
                    Log.Info("There's no CommonUpdater in the folder. Failed to update.");
                    MessageBox.Show("There's no CommonUpdater in the folder. Failed to update.\nIf you want get more supports, Please use the AutoUpdate feature");
                    return;
                }
                
                string arguments = $"{Project} {ExeName} {Author} {MainWindow.Version} \"{CurrentExePath}\" \"{NewExePath}\"";
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = commonUpdaterPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true
                };

                Log.Debug($"Starting CommonUpdater with arguments: {arguments}");
                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    Log.Error("Failed to start CommonUpdater: Process.Start returned null.");
                    return;
                }
                
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (!string.IsNullOrEmpty(error))
                {
                    Log.Error($"CommonUpdater error: {error}");
                }
                    
                if (process.ExitCode != 0)
                {
                    Log.Error($"CommonUpdater exited with code {process.ExitCode}");
                }
                else
                {
                    Log.Debug("CommonUpdater started successfully.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to start CommonUpdater: {ex.Message}");
            }
        }
    }
}