namespace AssetsMangment.DTOs.Response
{
    public class CreateRelationshipResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public AssetRelationshipResponse? Relationship { get; set; }
    }
}
