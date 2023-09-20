using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SortSpotify.Models
{
    public class Image
    {
        [Required]
        public string url { get; set; }

        [AllowNull]
        [Required]
        public int height { get; set; }

        [AllowNull]
        [Required]
        public int width { get; set; }

    }
}