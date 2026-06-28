namespace AssetsMangment.DTOs.Request
{
    public class CreateAssetRelationshipRequest
    {
        public Guid SourceAssetId { get; set; }
        public Guid TargetAssetId { get; set; }
        public RelationshipType Type { get; set; }
    }
}
