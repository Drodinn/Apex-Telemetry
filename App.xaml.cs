using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using ApexTelemetry.Models;
using ApexTelemetry.ViewModels;

namespace ApexTelemetry
{
    public partial class App : Application
    {
        private FileSystemWatcher? _statusWatcher;
        private MainViewModel? _mainViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Initialize ViewModel and MainWindow
            _mainViewModel = new MainViewModel();
            var mainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };

            // 2. Resolve Elite Dangerous Saved Games directory for Status.json
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string statusFolderPath = Path.Combine(userProfile, "Saved Games", "Frontier Developments", "Elite Dangerous");
            string statusFilePath = Path.Combine(statusFolderPath, "Status.json");

            // 3. Perform immediate initial read before starting watcher
            ReadStatusFile(statusFilePath);

            // 4. Start FileSystemWatcher for real-time changes
            StartWatcher(statusFolderPath, "Status.json");

            // 5. Show single MainWindow instance
            mainWindow.Show();
        }

        private void StartWatcher(string folderPath, string fileName)
        {
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            _statusWatcher = new FileSystemWatcher(folderPath, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _statusWatcher.Changed += (s, e) => OnStatusFileUpdated(e.FullPath);
            _statusWatcher.Created += (s, e) => OnStatusFileUpdated(e.FullPath);
        }

        private void OnStatusFileUpdated(string filePath)
        {
            // Give OS time to release file handle
            System.Threading.Thread.Sleep(50);

            Dispatcher.Invoke(() =>
            {
                ReadStatusFile(filePath);
            });
        }

        private void ReadStatusFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            var status = JsonConvert.DeserializeObject<StatusModel>(json);
                            if (status != null && _mainViewModel != null)
                            {
                                _mainViewModel.UpdateStatus(status);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading Status.json: {ex.Message}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_statusWatcher != null)
            {
                _statusWatcher.EnableRaisingEvents = false;
                _statusWatcher.Dispose();
            }
            base.OnExit(e);
        }
    }
}