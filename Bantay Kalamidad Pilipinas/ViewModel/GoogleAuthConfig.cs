using System;
using System.IO;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    /// <summary>
    /// Google OAuth client credentials for this app's "Sign in with Google"
    /// buttons.
    ///
    /// WHY THIS FILE NO LONGER HOLDS THE VALUES DIRECTLY:
    /// An earlier version of this file had ClientId/ClientSecret hardcoded
    /// as constants, with a comment saying "don't commit this once filled
    /// in." That advice doesn't actually work — once a file is already
    /// tracked by git, adding it to .gitignore does nothing; git keeps
    /// tracking changes to files it already knows about. That's exactly
    /// what happened: real credentials got committed and GitHub's push
    /// protection correctly blocked the push.
    ///
    /// This version reads the credentials from a separate file,
    /// "google-auth.local.json", which is NOT tracked by git from the
    /// start (see .gitignore) and is never created with real values by
    /// this codebase — only by you, locally, by hand. There's no
    /// "remember to remove it before committing" step to forget, because
    /// the file with real secrets in it never enters git's tracking in
    /// the first place.
    ///
    /// SETUP — do this once, locally, after creating your Desktop app OAuth
    /// client in Google Cloud Console:
    ///   1. In the project root (same folder as the .csproj), create a file
    ///      named exactly: google-auth.local.json
    ///   2. Put this in it, with your real values:
    ///      {
    ///        "ClientId": "123456789-abc...apps.googleusercontent.com",
    ///        "ClientSecret": "GOCSPX-..."
    ///      }
    ///   3. In Visual Studio, right-click the project → Add → Existing Item
    ///      → select google-auth.local.json → in its Properties, set
    ///      "Copy to Output Directory" = "Copy if newer". This makes sure
    ///      it ends up next to the .exe at build time, where this class
    ///      looks for it.
    ///   4. That's it — this file is already listed in .gitignore, so it
    ///      will never show up in `git status` or get committed.
    /// </summary>
    public static class GoogleAuthConfig
    {
        private const string LocalConfigFileName = "google-auth.local.json";

        private static string _clientId;
        private static string _clientSecret;
        private static bool _loaded;

        public static string ClientId
        {
            get { EnsureLoaded(); return _clientId; }
        }

        public static string ClientSecret
        {
            get { EnsureLoaded(); return _clientSecret; }
        }

        /// <summary>
        /// True once a local config file has actually been found and
        /// parsed. GoogleAuthHelper uses this (rather than comparing
        /// against a placeholder string) to decide whether Google Sign-In
        /// is ready to use.
        /// </summary>
        public static bool IsConfigured
        {
            get { EnsureLoaded(); return !string.IsNullOrWhiteSpace(_clientId) && !string.IsNullOrWhiteSpace(_clientSecret); }
        }

        private static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;

            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalConfigFileName);

                if (!File.Exists(path))
                {
                    return; // _clientId/_clientSecret stay null — IsConfigured will be false
                }

                string json = File.ReadAllText(path);

                // Minimal hand-rolled JSON read for exactly two string
                // fields — avoids pulling in a JSON library just for a
                // two-key local config file. Expects simple, flat JSON
                // like the example in the class doc comment above.
                _clientId = ExtractJsonStringValue(json, "ClientId");
                _clientSecret = ExtractJsonStringValue(json, "ClientSecret");
            }
            catch (Exception)
            {
                // Any read/parse failure leaves Client/Secret as null,
                // which IsConfigured correctly reports as "not configured."
                // GoogleAuthHelper shows a clear message in that case
                // rather than this throwing somewhere unexpected.
            }
        }

        private static string ExtractJsonStringValue(string json, string key)
        {
            string marker = "\"" + key + "\"";
            int keyIndex = json.IndexOf(marker, StringComparison.Ordinal);
            if (keyIndex < 0) return null;

            int colonIndex = json.IndexOf(':', keyIndex + marker.Length);
            if (colonIndex < 0) return null;

            int firstQuote = json.IndexOf('"', colonIndex + 1);
            if (firstQuote < 0) return null;

            int secondQuote = json.IndexOf('"', firstQuote + 1);
            if (secondQuote < 0) return null;

            return json.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
        }
    }
}