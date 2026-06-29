// AlertPollerService.cs — .NET Framework 4.8 compatible version
// Uses Newtonsoft.Json (already in the BKP project) instead of System.Text.Json

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bantay_Kalamidad_Pilipinas.Services
{
    // ── Data models ────────────────────────────────────────────────────────────

    public class KalamidadAlert
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("timestamp")] public string Timestamp { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("body")] public string Body { get; set; }
        [JsonProperty("source")] public string Source { get; set; }
        [JsonProperty("location")] public string Location { get; set; }
        [JsonProperty("read")] public bool Read { get; set; }
    }

    public class AlertResponse
    {
        [JsonProperty("success")] public bool Success { get; set; }
        [JsonProperty("count")] public int Count { get; set; }
        [JsonProperty("alerts")] public List<KalamidadAlert> Alerts { get; set; }
    }

    // ── Poller ─────────────────────────────────────────────────────────────────

    public class AlertPollerService : IDisposable
    {
        private const string WEB_APP_URL =
            "https://script.google.com/macros/s/AKfycbxN92V-lJ2-IOS4WVmeITHafUD99B-2Xhb9Av8mmUqjf3AiSWYRZlXkynMisJlA6FwemA/exec";

        private const int POLL_INTERVAL_SECONDS = 30;

        private readonly HttpClient _http;
        private Timer _timer;
        private string _lastSeenTimestamp;
        private bool _disposed;

        /// <summary>Fires (on a thread-pool thread) when new alerts arrive.</summary>
        public event EventHandler<List<KalamidadAlert>> NewAlertsReceived;

        /// <summary>Fires when a poll request fails.</summary>
        public event EventHandler<string> PollError;

        public AlertPollerService()
        {
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            _lastSeenTimestamp = DateTime.UtcNow.ToString("o");
        }

        public void Start()
        {
            _timer = new Timer(
                async state => await PollAsync(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(POLL_INTERVAL_SECONDS));
        }

        public void Stop() => _timer?.Change(Timeout.Infinite, Timeout.Infinite);

        private async Task PollAsync()
        {
            try
            {
                var url = string.Format("{0}?action=getAlerts&since={1}",
                                             WEB_APP_URL,
                                             Uri.EscapeDataString(_lastSeenTimestamp));
                var json = await _http.GetStringAsync(url);
                var response = JsonConvert.DeserializeObject<AlertResponse>(json);

                if (response == null || !response.Success ||
                    response.Alerts == null || response.Alerts.Count == 0)
                    return;

                _lastSeenTimestamp = DateTime.UtcNow.ToString("o");
                NewAlertsReceived?.Invoke(this, response.Alerts);
            }
            catch (Exception ex)
            {
                PollError?.Invoke(this, ex.Message);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _timer?.Dispose();
            _http?.Dispose();
        }
    }
}