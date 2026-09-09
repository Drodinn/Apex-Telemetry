using System;
using System.IO;
using ApexTelemetry.Models;
using Newtonsoft.Json;

namespace ApexTelemetry.Services
{
    public class TelemetryService
    {
        private FileSystemWatcher _watcher;
        private readonly string _statusFilePath;

        public event Action<StatusModel> StatusUpdated;

        public TelemetryService()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string journalPath = Path.Combine(userProfile, "Saved Games", "Frontier Developments", "Elite Dangerous");
            _statusFilePath = Path.Combine(journalPath, "Status.json");

            if (Directory.Exists(journalPath))
            {
                _watcher = new FileSystemWatcher(journalPath, "Status.json")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };

                _watcher.Changed += OnStatusFileChanged;
            }
        }

        private void OnStatusFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Small delay to allow Elite Dangerous to finish writing the file
                System.Threading.Thread.Sleep(50);
                if (File.Exists(_statusFilePath))
                {
                    string json = File.ReadAllText(_statusFilePath);
                    StatusModel status = JsonConvert.DeserializeObject<StatusModel>(json);
                    if (status != null)
                    {
                        StatusUpdated?.Invoke(status);
                    }
                }
            }
            catch (IOException)
            {
                // Handles temporary file locking during active game writes
            }
        }
    }
}