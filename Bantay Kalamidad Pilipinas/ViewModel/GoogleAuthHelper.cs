using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    /// <summary>
    /// Wraps Google's OAuth 2.0 "installed app" flow (RFC 8252 loopback
    /// redirect) using the official Google.Apis.Auth library. This is the
    /// flow Google's own documentation recommends for desktop apps —
    /// GoogleWebAuthorizationBroker opens the user's default system browser,
    /// listens on a random local loopback port for the redirect, and
    /// performs the PKCE code exchange automatically. No client secret is
    /// embedded as a real secret here because installed apps are "public
    /// clients" per Google's docs — they cannot keep a secret confidential,
    /// so the OAuth client must be registered in Google Cloud Console as a
    /// "Desktop app" client type, which does not require secrecy of the
    /// client_secret value the same way a server-side web app would.
    ///
    /// NOTE: this only requires the Google.Apis.Auth NuGet package — NOT
    /// Google.Apis.Oauth2.v2. The user's email and name are read directly
    /// from the signed ID token (a JWT) that Google returns alongside the
    /// access token when the "openid" scope is requested, decoded and
    /// cryptographically verified by GoogleJsonWebSignature.ValidateAsync
    /// (built into Google.Apis.Auth itself). This avoids an extra HTTP
    /// round-trip to a separate userinfo endpoint and an extra package.
    ///
    /// SETUP REQUIRED (cannot be done from code — see the README this
    /// shipped with):
    ///   1. A Google Cloud project with the OAuth consent screen configured.
    ///   2. An OAuth 2.0 Client ID of type "Desktop app" created in
    ///      Google Cloud Console > APIs & Services > Credentials.
    ///   3. The resulting Client ID and Client Secret pasted into
    ///      GoogleAuthConfig.cs (NOT committed to source control with real
    ///      values checked in — see that file's notes).
    /// </summary>
    public static class GoogleAuthHelper
    {
        // "openid" is what makes Google return a signed ID token (a JWT) we
        // can decode locally for the user's identity — "email" and
        // "profile" add the email/name claims to that token.
        private static readonly string[] Scopes = { "openid", "email", "profile" };

        // Must match the folder name used in SignInAsync() below — kept as
        // a shared constant so SignOutAsync() can never drift out of sync
        // with where the credential actually gets cached.
        private const string DataStoreFolderName = "BantayKalamidadPilipinas.GoogleAuth";

        /// <summary>
        /// Launches the system browser for the user to sign in to Google,
        /// and returns their verified email + display name on success.
        /// Throws if the user cancels/denies, or if GoogleAuthConfig hasn't
        /// been filled in with real credentials yet.
        /// </summary>
        public static async Task<GoogleSignInResult> SignInAsync()
        {
            if (!GoogleAuthConfig.IsConfigured)
            {
                throw new InvalidOperationException(
                    "Google Sign-In has not been configured yet. Create " +
                    "\"google-auth.local.json\" in the project root with your Client ID and " +
                    "Client Secret from Google Cloud Console — see the comment at the top of " +
                    "GoogleAuthConfig.cs for the exact steps.");
            }

            var secrets = new ClientSecrets
            {
                ClientId = GoogleAuthConfig.ClientId,
                ClientSecret = GoogleAuthConfig.ClientSecret
            };

            // FileDataStore caches the refresh token locally (per Windows
            // user, under %AppData%) so the user isn't forced to re-consent
            // in the browser every single time they click "Sign in with
            // Google." Passing a per-app folder name keeps this isolated
            // from any other app's cached Google credentials on the same
            // machine.
            var dataStore = new FileDataStore(DataStoreFolderName, true);

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                Scopes,
                "user", // local cache key — single-user desktop app, so a fixed key is fine
                CancellationToken.None,
                dataStore);

            string idToken = credential.Token.IdToken;

            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new InvalidOperationException(
                    "Google did not return an ID token. Make sure the \"openid\" scope is " +
                    "included and that the OAuth client is configured correctly.");
            }

            // Cryptographically verifies the token was really issued by
            // Google for this exact ClientId and hasn't been tampered with
            // or expired, then decodes the user's email/name claims from it.
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { GoogleAuthConfig.ClientId }
                });

            return new GoogleSignInResult
            {
                Email = payload.Email,
                DisplayName = string.IsNullOrWhiteSpace(payload.Name)
                    ? payload.Email
                    : payload.Name
            };
        }

        /// <summary>
        /// Clears the locally cached Google credential (refresh token).
        ///
        /// Without this, GoogleWebAuthorizationBroker.AuthorizeAsync silently
        /// reuses the cached token on the next "Sign in with Google" click —
        /// it never re-shows the browser or account picker as long as a
        /// valid cached token exists, regardless of which BKP login screen
        /// the button was clicked from. That's what caused this bug: logging
        /// out of BKP cleared BKP's own session (CurrentUser), but the
        /// separate Google-side cache was untouched, so clicking "Sign in
        /// with Google" on the *other* portal silently signed back in as the
        /// same previously-used Google account instead of prompting again —
        /// surfacing as a confusing "already registered as a Donor" message
        /// when trying to sign in on the Volunteer screen right after.
        ///
        /// Call this from every BKP logout path so each new "Sign in with
        /// Google" click always re-prompts, matching the expectation that
        /// logging out of BKP also ends the Google session for this app.
        /// </summary>
        public static async Task SignOutAsync()
        {
            try
            {
                var dataStore = new FileDataStore(DataStoreFolderName, true);
                await dataStore.ClearAsync();
            }
            catch (Exception)
            {
                // Best-effort — if the cache file is missing or locked,
                // there's nothing meaningful to do about it here. Worst
                // case, the next sign-in silently reuses a stale token,
                // same as today; this never blocks the actual logout.
            }
        }
    }

    /// <summary>
    /// Minimal result of a successful Google sign-in — just what's needed
    /// to find-or-create a row in this app's own Users/Donor/Volunteer
    /// tables. Deliberately does not carry the access token or any other
    /// Google credential beyond this point; once we know the email, this
    /// app's own session model takes over exactly like a password login.
    /// </summary>
    public class GoogleSignInResult
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
    }
}