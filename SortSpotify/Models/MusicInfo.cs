namespace SortSpotify.Models
{
    public class MusicInfo
    {

        public string id { get; set; }
        public string name { get; set; }
        public List<string> artistIds { get; set; }
        public List<string> artistNames { get; set; }
        public List<string> genres { get; set; }
        public List<string> playlistNames { get; set; }

        public MusicInfo(string id, string name, List<string> artistIds, List<string> artistNames)
        {
         
            this.id = id;
            this.name = name;
            this.artistIds = artistIds;
            this.artistNames = artistNames;
            this.genres = new List<string>();
            this.playlistNames = new List<string>();

        }

    }
}
