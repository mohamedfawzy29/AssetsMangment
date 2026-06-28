using AssetsMangment.Models;

namespace AssetsMangment.DTOs.Request
{
    public class UpdateAssetRequest
    {
        public AssetStatus Status { get; set; }
        public List<string> Tags { get; set; } = [];
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
