using System.Windows;
using ApexTelemetry.Services;
using ApexTelemetry.ViewModels;

namespace ApexTelemetry
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }
        private readonly TelemetryService _telemetryService;
        private readonly RaceTrackerService _raceTrackerService;

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;

            _raceTrackerService = new RaceTrackerService();
            _telemetryService = new TelemetryService();

            // Subscribe to Telemetry File Changes
            _telemetryService.StatusUpdated += status =>
            {
                Dispatcher.Invoke(() =>
                {
                    ViewModel.UpdateStatus(status);
                    _raceTrackerService.ProcessStatusUpdate(status);
                });
            };

            // Subscribe to Speed Decay & Race Tracker Updates
            _raceTrackerService.TraceUpdated += trace =>
            {
                Dispatcher.Invoke(() =>
                {
                    ViewModel.CurrentSpeed = trace.SpeedMS;
                });
            };
        }
    }
}