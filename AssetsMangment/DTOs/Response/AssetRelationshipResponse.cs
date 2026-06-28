namespace AssetsMangment.DTOs.Response
{
    public class AssetRelationshipResponse
    {
        public Guid Id { get; set; }
        public AssetSummaryResponse SourceAsset { get; set; }
        public AssetSummaryResponse TargetAsset { get; set; }
        public RelationshipType Type { get; set; }

    }
}
