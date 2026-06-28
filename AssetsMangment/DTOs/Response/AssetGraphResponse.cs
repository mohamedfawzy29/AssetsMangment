namespace AssetsMangment.DTOs.Response
{
    public class AssetGraphResponse
    {
        public AssetSummaryResponse Asset { get; set; } = null!;
        public List<AssetGraphRelationshipResponse> Relationships { get; set; } = [];
    }
}
