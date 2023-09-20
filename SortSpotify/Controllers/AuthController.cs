using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Security.AccessControl;
using SortSpotify.Helpers;
using SortSpotify.Models;

namespace SortSpotify.Controllers
{
    public class AuthController : Controller
    {
        private static string _codeVerifier;
        private static string _state;
        public static string _clientId = "09f86191c869489b84914d1172051c61";
        private static string _redirectUri  = "https://localhost:7230/callback";
        public static string _accessToken { get; set; }
        public static string _refreshToken { get; set; }

        public IActionResult StartAuthorization()
        {
            Console.WriteLine("StartAuthorization");
            _codeVerifier = Helpers.SpotifyAuthHelper.GenerateRandomString(128);
            _state = Helpers.SpotifyAuthHelper.GenerateRandomString(16);
            var codeChallenge = Helpers.SpotifyAuthHelper.GenerateCodeChallenge(_codeVerifier);
            var scopes = "user-read-private user-read-email user-library-read"; // Add required scopes

            var authorizationUrl =
                $"https://accounts.spotify.com/authorize?response_type=code&client_id={_clientId}&redirect_uri={_redirectUri}&" +
                $"code_challenge_method=S256&code_challenge={codeChallenge}&scope={Uri.EscapeDataString(scopes)}&state={_state}";
            Console.WriteLine("Auth url : " + authorizationUrl);
            Console.WriteLine($"Class attributes. codeVerifier: {_codeVerifier} ; state: {_state} ; clientId: {_clientId} ; redirectUri: {_redirectUri}");

            return Redirect(authorizationUrl);
        }


        public async Task<IActionResult> Callback(string code, string state)
        {
            Console.WriteLine($"Callback. returnedState : {state} ; actualState : {_state}");
            Console.WriteLine($"Class attributes. codeVerifier: {_codeVerifier} ; state: {_state} ; clientId: {_clientId} ; redirectUri: {_redirectUri}");
            

            if (state != _state) // Validate state if you used it during Step 3
            {
                return BadRequest("Invalid state parameter.");
            }

            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", _redirectUri),
                new KeyValuePair<string, string>("code_verifier", _codeVerifier)
            };


            string tokenResponseStr = await ApiHelper.DoWithRetryAsync("https://accounts.spotify.com/api/token", 5, HttpMethod.Post, new FormUrlEncodedContent(content));

            AccessTokenResponse tokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(tokenResponseStr);

            _accessToken = tokenResponse?.access_token;
            _refreshToken = tokenResponse?.refresh_token; // Save this for refreshing the access token later

            Console.WriteLine($"Callback. AccessToken : {_accessToken} ; RefreshToken : {_refreshToken} ; TokenResponse : {tokenResponse}");


            if (string.IsNullOrEmpty(_accessToken))
            {
                return BadRequest("Failed to get access token.");
            }

            // Use the access token to access Spotify Web API resources
            // (e.g., fetch user data, playlists, etc.)

            return RedirectToAction("Index", "Home");
        }


    }
}
