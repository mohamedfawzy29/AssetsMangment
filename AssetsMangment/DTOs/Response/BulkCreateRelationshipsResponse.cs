namespace AssetsMangment.DTOs.Response
{
    public class BulkCreateRelationshipsResponse
    {
        public int Total { get; set; }
        public int Imported { get; set; }
        public int Duplicates { get; set; }
        public int Invalid { get; set; }
    }
}
