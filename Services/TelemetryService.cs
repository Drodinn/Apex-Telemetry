using System;
using System.IO;
using Newtonsoft.Json;
using ApexTelemetry.Models;

namespace ApexTelemetry.Services
{
    public class TelemetryService : IDisposable
    {
        private readonly string _statusFilePath;
        private FileSystemWatcher? _watcher;

        public event Action<StatusModel>? StatusUpdated;

        public TelemetryService()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string statusFolderPath = Path.Combine(userProfile, "Saved Games", "Frontier Developments", "Elite Dangerous");
            _statusFilePath = Path.Combine(statusFolderPath, "Status.json");

            StartWatcher(statusFolderPath, "Status.json");
        }

        /// <summary>
        /// Reads and parses Status.json immediately upon application launch.
        /// </summary>
        public void ReadInitialStatus()
        {
            try
            {
                if (File.Exists(_statusFilePath))
                {
                    // Open with FileShare.ReadWrite to prevent locking conflicts with Elite Dangerous
                    using (var stream = new FileStream(_statusFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            var status = JsonConvert.DeserializeObject<StatusModel>(json);
                            if (status != null)
                            {
                                StatusUpdated?.Invoke(status);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading initial Status.json: {ex.Message}");
            }
        }

        private void StartWatcher(string folderPath, string fileName)
        {
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            _watcher = new FileSystemWatcher(folderPath, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnStatusFileChanged;
            _watcher.Created += OnStatusFileChanged;
        }

        private void OnStatusFileChanged(object sender, FileSystemEventArgs e)
        {
            // Brief pause to ensure the game has finished writing the file
            System.Threading.Thread.Sleep(50);
            ReadInitialStatus();
        }

        public void StopWatcher()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnStatusFileChanged;
                _watcher.Created -= OnStatusFileChanged;
            }
        }

        public void Dispose()
        {
            StopWatcher();
            _watcher?.Dispose();
        }
    }
}