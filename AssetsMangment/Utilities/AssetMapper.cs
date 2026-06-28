namespace AssetsMangment.Utilities
{
    public class AssetMapper
    {
        public static AssetSummaryResponse ToSummary(Asset asset)
        {
            return new AssetSummaryResponse
            {
                Id = asset.Id,
                Type = asset.Type,
                Value = asset.Value
            };
        }

        public static AssetResponse ToResponse(Asset asset)
        {
            return new AssetResponse
            {
                Id = asset.Id,
                Type = asset.Type,
                Value = asset.Value,
                Status = asset.Status,
                FirstSeen = asset.FirstSeen,
                LastSeen = asset.LastSeen,
                Source = asset.Source,
                Tags = asset.Tags,
                Metadata = asset.Metadata
            };
        }
    }
}
