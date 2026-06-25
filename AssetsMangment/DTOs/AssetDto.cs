using AssetsMangment.Models;

namespace AssetsMangment.DTOs
{
    public class AssetDto
    {
        public Guid Id { get; set; }
        public AssetType Type { get; set; }
        public string Value { get; set; } = string.Empty;
        public AssetStatus Status { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public AssetSource Source { get; set; }
        public List<string> Tags { get; set; } = [];
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
