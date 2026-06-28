namespace AssetsMangment.DTOs.Response
{
    public class AssetSummaryResponse
    {
        public Guid Id { get; set; }
        public AssetType Type { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
