namespace AssetsMangment.DTOs.Request
{
    public class BulkCreateRelationshipsRequest
    {
        public List<CreateAssetRelationshipRequest> Relationships { get; set; } = [];
    }
}
