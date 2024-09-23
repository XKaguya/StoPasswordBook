using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace StoPasswordBook;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static MainWindow? MainWindowInstance { get; private set; }
    
    protected override async void OnStartup(StartupEventArgs ev)
    {
        try
        {
            base.OnStartup(ev);
              
            KillExistingInstances();
          
            MainWindowInstance = new MainWindow();
            MainWindowInstance.Show();
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message + e.StackTrace);
            throw;
        }
    }
    
    private void KillExistingInstances()
    {
        var currentProcess = Process.GetCurrentProcess();
        var processes = Process.GetProcessesByName(currentProcess.ProcessName);

        foreach (var process in processes)
        {
            if (process.Id != currentProcess.Id)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to kill process {process.Id}: {ex.Message}");
                }
            }
        }
    }
}