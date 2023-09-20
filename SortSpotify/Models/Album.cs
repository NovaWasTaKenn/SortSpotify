using System.ComponentModel.DataAnnotations;

namespace SortSpotify.Models
{
    public class Album
    {

        //[Required]
        //public string album_type { get; set; }

        //[Required]
        //public int total_tracks { get; set; }

        //[Required]
        //public List<string> available_markets { get; set; }

        [Required]
        public ExternalUrls external_urls { get; set; }

        [Required]
        public string href { get; set; }

        [Required]
        public string id { get; set; }

        //[Required]
        //public List<Image> images { get; set; }

        [Required]
        public string name { get; set; }

        //[Required]
        //public string release_date { get; set; }
        //
        //[Required]
        //public string release_date_precision { get; set; }

        //public Restrictions restrictions { get; set; }

        //[Required]
        //public string type { get; set; }

        [Required]
        public string uri { get; set; }
        //public List<Copyright> copyrights { get; set; }
        public ExternalIds external_ids { get; set; }
        public List<string> genres { get; set; }
        //public string label { get; set; }
        public int popularity { get; set; }
        //public string album_group { get; set; }

        //[Required]
        //public List<SimplifiedArtist> artists { get; set; }
    }
}
