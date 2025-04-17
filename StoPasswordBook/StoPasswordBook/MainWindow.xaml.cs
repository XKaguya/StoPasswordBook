using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using log4net;
using log4net.Config;
using StoPasswordBook.Extern;
using StoPasswordBook.Generic;
using Wpf.Ui.Controls;

namespace StoPasswordBook;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    public static readonly string Version = "1.0.7";
    
    private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));
    
    private ObservableDictionary<string, string> Accounts { get; set; }
    
    public MainWindow()
    {
        InitializeComponent();
        
        DataContext = this;

        Accounts = new ObservableDictionary<string, string>();
        
        AccountComboBox.ItemsSource = Accounts;
        
        Task.Run(InitMethod);
    }
    
    private void LoadAccounts()
    {
        if (!File.Exists("Shadow.xml"))
        {
            var doc = new XDocument(
                new XElement("Accounts",
                    new XElement("Account",
                        new XElement("Name", "Account1"),
                        new XElement("Password", "Password1")
                    ),
                    new XElement("Account",
                        new XElement("Name", "Account2"),
                        new XElement("Password", "Password2")
                    ),
                    new XElement("Account",
                        new XElement("Name", "Account3"),
                        new XElement("Password", "Password3")
                    )
                )
            );

            doc.Save("Shadow.xml");
            Log.Info("Default Shadow.xml created.");
            LoadAccounts();
        }
        
        try
        {
            XDocument doc = XDocument.Load("Shadow.xml");
            var accountElements = doc.Descendants("Account");

            foreach (var account in accountElements)
            {
                string accountName = account.Element("Name")?.Value;
                string password = account.Element("Password")?.Value;

                if (!string.IsNullOrEmpty(accountName) && !string.IsNullOrEmpty(password))
                {
                    if (!Accounts.ContainsKey(accountName))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Accounts.Add(accountName, password);
                        });
                    }
                }
            }
            
            UpdateText("Accounts loaded.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error loading accounts from Shadow.xml {ex.Message}");
            UpdateText("Error loading accounts from Shadow.xml");
        }
    }

    private async Task ApplyAccount()
    {
        var selectedAccount = AccountComboBox.SelectedItem as KeyValuePair<string, string>?;

        if (selectedAccount != null && selectedAccount.HasValue)
        {
            var accountName = selectedAccount.Value.Key;
            var selectedPassword = selectedAccount.Value.Value;
            
            if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(selectedPassword))
            {
                Log.Error("Empty account or password.");
                UpdateText("Empty account or password.");
                return;
            }

            if (NewWebSocketManager.WebSocket == null || !NewWebSocketManager.WebSocket.IsAlive)
            {
                Api.KillExistingInstances("Star Trek Online");
                await Api.InitApi();
                await NewWebSocketManager.SetCredentials(accountName, selectedPassword);
            }
        
            await NewWebSocketManager.SetCredentials(accountName, selectedPassword);
        }
        else
        {
            UpdateText("ERROR While accessing passwords.");
        }
    }

    private async void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        if (GlobalVariables.IsInitSubmit)
        {
            GlobalVariables.IsInitSubmit = false;
        }
        
        await ApplyAccount();
    }

    private async Task InitMethod()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "StoPasswordBook.log4net.config";
        
        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            XmlConfigurator.Configure(stream);
        }
        else
        {
            Log.Error($"Failed to find embedded resource: {resourceName}");
        }
        
        Api.ParseConfig();
        AutoUpdate.StartAutoUpdateTask();
        
        Log.Info("Initializing Api...");
        UpdateText("Initializing Api...");
        LoadAccounts();
        GlobalVariables.IsInitSubmit = true;
        await Api.InitApi();
        await Task.Delay(TimeSpan.FromSeconds(GlobalVariables.WaitInterval));
        
        var selectedAccount = await Application.Current.Dispatcher.InvokeAsync(() =>
            AccountComboBox.SelectedItem as KeyValuePair<string, string>?
        );
        var accountName = selectedAccount.Value.Key;
        var selectedPassword = selectedAccount.Value.Value;

        do
        {
            await NewWebSocketManager.SetCredentials(accountName, selectedPassword);
            await NewWebSocketManager.GetCurrentCredentials();
        } while (GlobalVariables.SetColumnNum != 2);
    }

    private void UpdateTextEx(string str, Brush? brushes = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            LogTextBlock.Text = str;
            LogTextBlock.Foreground = brushes ?? Brushes.Black;
        });
    }

    public static void UpdateText(string str, Brush? brushes = null)
    {
        App.MainWindowInstance!.UpdateTextEx(str, brushes);
    }
}