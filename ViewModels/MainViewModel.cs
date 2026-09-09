using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ApexTelemetry.Models;
using ApexTelemetry.Views;

namespace ApexTelemetry.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private StatusModel? _latestStatus;
        private double _currentSpeed;
        private double _currentAltitude;
        private string _statusMessage = "Telemetry System Active - Awaiting Status updates...";
        private string _raceStatus = "NO ACTIVE RACE";
        private bool _isDebugExpanded;

        private OverlayWindow? _overlayWindow;

        public StatusModel? LatestStatus
        {
            get => _latestStatus;
            set
            {
                _latestStatus = value;
                OnPropertyChanged();
            }
        }

        public double CurrentSpeed
        {
            get => _currentSpeed;
            set
            {
                _currentSpeed = value;
                OnPropertyChanged();
            }
        }

        public double CurrentAltitude
        {
            get => _currentAltitude;
            set
            {
                _currentAltitude = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public string RaceStatus
        {
            get => _raceStatus;
            set
            {
                _raceStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsDebugExpanded
        {
            get => _isDebugExpanded;
            set
            {
                _isDebugExpanded = value;
                OnPropertyChanged();
            }
        }

        public ICommand ToggleOverlayCommand { get; }

        public MainViewModel()
        {
            ToggleOverlayCommand = new RelayCommand(ToggleOverlay);
        }

        public void ToggleOverlay()
        {
            if (_overlayWindow == null || !_overlayWindow.IsLoaded)
            {
                _overlayWindow = new OverlayWindow
                {
                    DataContext = this
                };
                _overlayWindow.Show();
                StatusMessage = "Overlay Display Enabled";
            }
            else
            {
                _overlayWindow.Close();
                _overlayWindow = null;
                StatusMessage = "Overlay Display Disabled";
            }
        }

        public void UpdateStatus(StatusModel newStatus)
        {
            LatestStatus = newStatus;

            if (newStatus.Altitude.HasValue)
            {
                CurrentAltitude = newStatus.Altitude.Value;
            }

            StatusMessage = $"Status.json Updated: {newStatus.Timestamp:HH:mm:ss}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}