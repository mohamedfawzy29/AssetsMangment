using System.ComponentModel.DataAnnotations;

namespace AssetsMangment.Models
{
    public enum AssetType 
    { 
        domain, 
        subdomain, 
        ip_address, 
        service, 
        certificate, 
        technology 
    }
    public enum AssetStatus 
    { 
        active, 
        stale, 
        archived 
    }
    public enum AssetSource
    {
        import,
        scan,
        manual
    }
    public class Asset
    {
        public Guid Id { get; set; }
        public AssetType Type { get; set; }
        [Required]
        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;
        public AssetStatus Status { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public AssetSource Source { get; set; }
        public List<string> Tags { get; set; } = [];
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>(); 
        public ICollection<AssetRelationship> OutgoingRelationships { get; set; } = new List<AssetRelationship>();
        public ICollection<AssetRelationship> IncomingRelationships { get; set; } = new List<AssetRelationship>();
    }
}

