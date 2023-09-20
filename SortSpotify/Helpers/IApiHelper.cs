using System.Text.Json.Nodes;

namespace SortSpotify.Helpers
{
    public interface IApiHelper
    {
        Task<JsonObject> DoWithRetryAsync(string apiEndpoint, int maxRetries, HttpMethod method, IEnumerable<KeyValuePair<string,string>> body = null);
    }
}