namespace AssetsMangment.DTOs.Request
{
    public class GetRelationshipsQueryRequest
    {
        public RelationshipType? Type { get; set; }
        public Guid? SourceAssetId { get; set; }
        public Guid? TargetAssetId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
