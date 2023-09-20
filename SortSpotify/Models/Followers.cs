using System.Diagnostics.CodeAnalysis;

namespace SortSpotify.Models
{
    public class Followers
    {
        [AllowNull]
        public string href { get; set; }

        public int total { get; set; }
    }
}