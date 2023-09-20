using System.Net.Http;
using System.Text.Json.Nodes;
using System.Text.Json;
using SortSpotify.Controllers;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using System.Net.Http.Headers;
using System.Net;

namespace SortSpotify.Helpers
{

    public class CustomHttpRequestException : HttpRequestException
    {
        public HttpStatusCode _statusCode { get; }
        public HttpResponseHeaders _responseHeaders { get; }

        public CustomHttpRequestException(HttpStatusCode statusCode, HttpResponseHeaders responseHeaders, string message) : base(message)
        {
            _statusCode = statusCode;
            _responseHeaders = responseHeaders;
        }

        public TimeSpan? GetRetryAfterTimeSpan()
        {
            if (_responseHeaders?.RetryAfter != null)
            {
                if (_responseHeaders.RetryAfter.Date.HasValue)
                    return _responseHeaders.RetryAfter.Date.Value - DateTime.UtcNow;

                if (_responseHeaders.RetryAfter.Delta.HasValue)
                    return _responseHeaders.RetryAfter.Delta.Value;

                if (!string.IsNullOrEmpty(_responseHeaders.RetryAfter.ToString()))
                {
                    if (TimeSpan.TryParse(_responseHeaders.RetryAfter.ToString(), out TimeSpan timeSpan))
                        return timeSpan;
                }
            }

            return null;
        }
    }

    public static class ApiHelper
    {
        private static HttpClient _httpClient = new HttpClient();

        //public  ApiHelper()
        //{
        //    _httpClient = ;
        //
        //}

        public static async Task<string> DoWithRetryAsync(string url, int maxRetries, HttpMethod method, HttpContent content = null,Dictionary<string, List<string>> headers = null, bool tokenAuth = false)
        {

            int retryCount = 0;

            while (retryCount < maxRetries)
            {

                try
                {
                    Console.WriteLine($"Try call url : {url}. retryCount : {retryCount}");
                    HttpRequestMessage request = new HttpRequestMessage(method, url);

                    if (content != null )
                    {
                        request.Content = content;
                    }

                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, List<string>> header in headers)
                        {
                            request.Headers.Add(header.Key, header.Value);

                        }
                    }

                    if (tokenAuth)
                    {
                        request.Headers.Add("Authorization", $"Bearer {AuthController._accessToken}");
                    }

                    HttpResponseMessage response = await _httpClient.SendAsync(request).ConfigureAwait(false);

                    string responseStr = "";

                    if (!response.IsSuccessStatusCode)
                    {

                        throw new CustomHttpRequestException(response.StatusCode, response.Headers, response.ReasonPhrase);

                    }
                    
                    //response.EnsureSuccessStatusCode(); // This will throw an exception if the response is not successful

                    Console.WriteLine($"Try call url : {url}. Call succeeded");


                    responseStr = await response.Content.ReadAsStringAsync();
                
                    return responseStr;

                }
                
                catch(CustomHttpRequestException ex)
                {
                    Console.WriteLine(ex.ToString());
                    if (ex.StatusCode == System.Net.HttpStatusCode.NoContent || ex.StatusCode == System.Net.HttpStatusCode.NotFound || ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        Console.WriteLine(ex.ToString());
                        throw ex;
                    }

                    if(ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        int sleepTime = (ex.GetRetryAfterTimeSpan() ?? new TimeSpan(0,0,60)).Seconds * 1000;
                        Thread.Sleep(sleepTime);
                        retryCount--;
                    }

                    if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine(ex.ToString());
                        SpotifyAuthHelper.RefreshToken();
                    }
                    Console.WriteLine(ex.ToString());
                    retryCount++;
                }
                catch (HttpRequestException ex)
                {

                    Console.WriteLine(ex.ToString());
                    if (ex.StatusCode == System.Net.HttpStatusCode.NoContent || ex.StatusCode == System.Net.HttpStatusCode.NotFound || ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        Console.WriteLine(ex.ToString());
                        throw ex;
                    }
                    if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine(ex.ToString());
                        SpotifyAuthHelper.RefreshToken();
                    }
                    Console.WriteLine(ex.ToString());
                    retryCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw ex;
                }

            }

            throw new Exception("Out of retries");

        }
    }
}
