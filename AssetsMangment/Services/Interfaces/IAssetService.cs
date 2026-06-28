namespace AssetsMangment.Services.Interfaces
{
    public interface IAssetService
    {
        Task<CreateOrUpdateAssetResponse> CreateOrUpdateAssetAsync(CreateAssetRequest request);
        Task<List<Asset>> GetAssetsAsync(GetAssetsQueryRequest request);
    }
}
