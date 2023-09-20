using SortSpotify.Controllers;
using SortSpotify.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SortSpotify.Helpers
{
    public static class SpotifyAuthHelper
    {

        public static string GenerateRandomString(int lenght)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, lenght)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Base64UrlEncode(challengeBytes);
            }
        }

        public static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        public static async void RefreshToken()
        {

            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", AuthController._clientId),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("refresh_token", AuthController._refreshToken),
            };

            string tokenResponseStr = await ApiHelper.DoWithRetryAsync("https://accounts.spotify.com/api/token", 5, HttpMethod.Post, new FormUrlEncodedContent(content));

            AccessTokenResponse tokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(tokenResponseStr);

            AuthController._accessToken = tokenResponse.access_token;
            AuthController._refreshToken = tokenResponse.refresh_token;

        }

    }
}
