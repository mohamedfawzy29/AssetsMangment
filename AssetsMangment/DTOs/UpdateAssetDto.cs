using AssetsMangment.Models;

namespace AssetsMangment.DTOs
{
    public class UpdateAssetDto
    {
        public AssetStatus Status { get; set; }
        public List<string> Tags { get; set; } = [];
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
