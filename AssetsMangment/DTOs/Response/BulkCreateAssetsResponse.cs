namespace AssetsMangment.DTOs.Response
{
    public class BulkCreateAssetsResponse
    {
        public int Total { get; set; }
        public int Imported { get; set; }
        public int Duplicates { get; set; }
    }
}
