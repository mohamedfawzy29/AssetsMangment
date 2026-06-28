namespace AssetsMangment.DTOs.Response
{
    public enum RelationshipDirection
    {
        Incoming,
        Outgoing
    }
    public class AssetGraphRelationshipResponse
    {
        public RelationshipType Type { get; set; }
        public RelationshipDirection Direction { get; set; }
        public AssetSummaryResponse Asset { get; set; } = null!;
    }
}
