using AssetsMangment.Models;
using System.ComponentModel.DataAnnotations;

namespace AssetsMangment.DTOs
{
    public class CreateAssetDto
    {
        public AssetType Type { get; set; }
        [Required]
        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;
        public AssetSource Source { get; set; }
        public List<String> Tags { get; set; } = [];
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
