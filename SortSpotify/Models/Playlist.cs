namespace SortSpotify.Models
{
    public class Playlist
    {

        public bool collaborative { get; set; }
        public string description { get; set; }
        public ExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public Owner owner { get; set; }
        public bool _public { get; set; }
        public string snapshot_id { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }
}
