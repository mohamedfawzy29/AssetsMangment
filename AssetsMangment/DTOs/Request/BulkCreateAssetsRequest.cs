namespace AssetsMangment.DTOs.Request
{
    public class BulkCreateAssetsRequest
    {
        public List<CreateAssetRequest> Assets { get; set; } = new();
    }
}
