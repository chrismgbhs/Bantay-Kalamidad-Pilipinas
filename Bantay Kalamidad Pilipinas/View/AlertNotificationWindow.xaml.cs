// AlertNotificationWindow.xaml.cs — .NET Framework 4.8 / C# 7.3 compatible
using System;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Bantay_Kalamidad_Pilipinas.Services;

namespace Bantay_Kalamidad_Pilipinas.Views
{
    public partial class AlertNotificationWindow : Window
    {
        private readonly DispatcherTimer _autoCloseTimer;

        public AlertNotificationWindow(KalamidadAlert alert, int autoCloseSeconds = 60)
        {
            InitializeComponent();

            TitleText.Text = alert.Title;
            BodyText.Text = alert.Body;
            SourceLabel.Text = alert.Source != null ? alert.Source.ToUpper() : "ALERT";

            DateTime ts;
            if (DateTime.TryParse(alert.Timestamp, out ts))
                TimestampLabel.Text = ts.ToLocalTime().ToString("MMM d, h:mm tt");

            Color color;
            string icon, soundType;
            GetAlertStyle(alert.Title, out color, out icon, out soundType);
            AccentBrush.Color = color;
            TopBarBrush.Color = color;
            IconText.Text = icon;

            PlayAlertSound(soundType);

            _autoCloseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(autoCloseSeconds)
            };
            _autoCloseTimer.Tick += AutoClose_Tick;
            _autoCloseTimer.Start();

            PositionBottomRight();
        }

        private void AutoClose_Tick(object sender, EventArgs e)
        {
            _autoCloseTimer.Stop();
            Close();
        }

        private static void PlayAlertSound(string soundType)
        {
            try
            {
                switch (soundType)
                {
                    case "critical": SystemSounds.Exclamation.Play(); break;
                    case "warning": SystemSounds.Hand.Play(); break;
                    default: SystemSounds.Asterisk.Play(); break;
                }
            }
            catch { /* fail silently */ }
        }

        private void PositionBottomRight()
        {
            var screen = SystemParameters.WorkArea;
            Left = screen.Right - Width - 16;
            Top = screen.Bottom - Height - 16;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => PositionBottomRight();

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _autoCloseTimer.Stop();
            Close();
        }

        private static void GetAlertStyle(string title, out Color color, out string icon, out string soundType)
        {
            var t = title != null ? title.ToLower() : "";

            if (t.Contains("earthquake"))
            { color = Color.FromRgb(0xE0, 0x5C, 0x5C); icon = "🔴"; soundType = "critical"; return; }

            if (t.Contains("signal no. 3") || t.Contains("signal no. 4") || t.Contains("signal no. 5"))
            { color = Color.FromRgb(0xE0, 0x5C, 0x5C); icon = "🌀"; soundType = "critical"; return; }

            if (t.Contains("signal no. 2"))
            { color = Color.FromRgb(0xE0, 0x8C, 0x30); icon = "🌀"; soundType = "warning"; return; }

            if (t.Contains("signal no. 1"))
            { color = Color.FromRgb(0xE0, 0xCC, 0x30); icon = "🌀"; soundType = "info"; return; }

            if (t.Contains("red"))
            { color = Color.FromRgb(0xE0, 0x5C, 0x5C); icon = "🌧️"; soundType = "critical"; return; }

            if (t.Contains("orange"))
            { color = Color.FromRgb(0xE0, 0x8C, 0x30); icon = "🌧️"; soundType = "warning"; return; }

            if (t.Contains("yellow"))
            { color = Color.FromRgb(0xCC, 0xCC, 0x30); icon = "🌧️"; soundType = "info"; return; }

            if (t.Contains("thunderstorm"))
            { color = Color.FromRgb(0x70, 0x70, 0xCC); icon = "⛈️"; soundType = "info"; return; }

            color = Color.FromRgb(0x50, 0xA0, 0xCC); icon = "ℹ️"; soundType = "info";
        }
    }
}