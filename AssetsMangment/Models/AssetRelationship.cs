
namespace AssetsMangment.Models
{
    public enum RelationshipType
    {
        SubdomainOf,
        RunsOn,
        Secures,
        ResolvesTo,
        UsesTechnology
    }
    public class AssetRelationship
    {
        public Guid Id { get; set; }
        public Guid SourceAssetId { get; set; }
        public Guid TargetAssetId { get; set; }
        public RelationshipType Type { get; set; }
        public Asset SourceAsset { get; set; } 
        public Asset TargetAsset { get; set; }
    }
}
