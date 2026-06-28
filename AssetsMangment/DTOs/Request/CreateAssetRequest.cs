
namespace AssetsMangment.DTOs.Request
{
    public class CreateAssetRequest
    {
        public AssetType Type { get; set; }
        [Required]
        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;
        public AssetSource Source { get; set; }
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
