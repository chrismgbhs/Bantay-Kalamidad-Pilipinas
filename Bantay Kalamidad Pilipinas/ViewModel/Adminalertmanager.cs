// AdminAlertManager.cs — .NET Framework 4.8 / C# 7.3 compatible
using System.Collections.Generic;
using System.Windows;
using Bantay_Kalamidad_Pilipinas.Services;
using Bantay_Kalamidad_Pilipinas.Views;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    /// <summary>
    /// Static manager that owns the alert poller for the admin session.
    /// Call Start() after a successful admin login and Stop() on logout.
    /// </summary>
    public static class AdminAlertManager
    {
        private static AlertPollerService _poller;
        private static double _nextPopupOffsetY = 0;

        public static void Start()
        {
            Stop(); // clean up any previous session
            _nextPopupOffsetY = 0;

            _poller = new AlertPollerService();
            _poller.NewAlertsReceived += Poller_NewAlertsReceived;
            _poller.PollError += Poller_PollError;
            _poller.Start();
        }

        public static void Stop()
        {
            if (_poller != null)
            {
                _poller.Stop();
                _poller.Dispose();
                _poller = null;
            }
            _nextPopupOffsetY = 0;
        }

        private static void Poller_NewAlertsReceived(object sender, List<KalamidadAlert> alerts)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null) return;
            dispatcher.Invoke(() => ShowAlerts(alerts));
        }

        private static void Poller_PollError(object sender, string err)
        {
            System.Diagnostics.Debug.WriteLine("[AlertPoller] " + err);
        }

        private static void ShowAlerts(List<KalamidadAlert> alerts)
        {
            foreach (var alert in alerts)
            {
                var popup = new AlertNotificationWindow(alert);
                // Show first so WPF can measure the window, then offset upward
                popup.Show();
                // Offset each successive popup so they stack without overlapping
                popup.Top -= _nextPopupOffsetY;
                _nextPopupOffsetY += popup.ActualHeight > 0 ? popup.ActualHeight + 8 : 140;
            }

            // Reset offset after a full batch so the next poll starts fresh
            // (old popups will have auto-closed by then)
            _nextPopupOffsetY = 0;
        }
    }
}