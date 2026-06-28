namespace AssetsMangment.DTOs.Response
{
    public class CreateOrUpdateAssetResponse
    {
        public Asset Asset { get; set; } = null!;
        public bool IsCreated { get; set; }
    }
}
