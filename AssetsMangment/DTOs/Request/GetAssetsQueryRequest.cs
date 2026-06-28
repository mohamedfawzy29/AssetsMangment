namespace AssetsMangment.DTOs.Request
{
    
    public class GetAssetsQueryRequest
    {
        public AssetType? Type { get; set; }
        public AssetStatus? Status { get; set; }
        public AssetSource? Source { get; set; }
        public string? Search { get; set; }
        public string? Tag { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
