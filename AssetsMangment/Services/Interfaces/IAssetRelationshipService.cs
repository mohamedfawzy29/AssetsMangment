namespace AssetsMangment.Services.Interfaces
{
    public interface IAssetRelationshipService
    {
        Task<CreateRelationshipResponse> CreateRelationshipAsync(CreateAssetRelationshipRequest request);
    }
}
