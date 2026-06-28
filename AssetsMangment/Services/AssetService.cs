namespace AssetsMangment.Services
{
    public class AssetService : IAssetService
    {
        private readonly ApplicationDbContext _context;
        public AssetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CreateOrUpdateAssetResponse> CreateOrUpdateAssetAsync(CreateAssetRequest request)
        {
            var normalizedValue = AssetUtilities.NormalizeValue(request.Value);
            var existingAsset = await _context.Assets.FirstOrDefaultAsync(a => a.Type == request.Type && a.Value == normalizedValue);

            if (existingAsset != null)
            {
                existingAsset.LastSeen = DateTime.UtcNow;
                existingAsset.Status = AssetStatus.active;

                existingAsset.Tags = existingAsset.Tags
                    .Union(request.Tags)
                    .ToList();

                foreach (var metadata in request.Metadata)
                {
                    existingAsset.Metadata[metadata.Key] = metadata.Value;
                }

                await _context.SaveChangesAsync();

                return new CreateOrUpdateAssetResponse
                {
                    Asset = existingAsset,
                    IsCreated = false
                };
            }
            else
            {
                var asset = new Asset
                {
                    Id = Guid.NewGuid(),
                    Type = request.Type,
                    Value = request.Value.Trim(),
                    Source = request.Source,
                    Status = AssetStatus.active,
                    FirstSeen = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow,
                    Tags = request.Tags,
                    Metadata = request.Metadata
                };

                _context.Assets.Add(asset);

                await _context.SaveChangesAsync();

                return new CreateOrUpdateAssetResponse
                {
                    Asset = asset,
                    IsCreated = true
                };
            }
        }

        public async Task<List<Asset>> GetAssetsAsync(GetAssetsQueryRequest request)
        {
            var assets = _context.Assets.AsQueryable();
            // filteration
            if (request.Type.HasValue)
            {
                assets = assets.Where(a => a.Type == request.Type.Value);
            }
            if (request.Status.HasValue)
            {
                assets = assets.Where(a => a.Status == request.Status.Value);
            }
            if (request.Source.HasValue)
            {
                assets = assets.Where(a => a.Source == request.Source.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLowerInvariant();
                assets = assets.Where(a => a.Value.ToLower().Contains(search));
            }

            var result = await assets.ToListAsync();

            if (!string.IsNullOrWhiteSpace(request.Tag))
            {
                result = result.Where(a => a.Tags.Any(t => t.Equals(request.Tag, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                switch (request.SortBy.ToLower())
                {
                    case "value":
                        result = request.Descending ? result.OrderByDescending(a => a.Value).ToList() : result.OrderBy(a => a.Value).ToList();
                        break;

                    case "firstseen":
                        result = request.Descending ? result.OrderByDescending(a => a.FirstSeen).ToList() : result.OrderBy(a => a.FirstSeen).ToList();
                        break;

                    case "lastseen":
                        result = request.Descending ? result.OrderByDescending(a => a.LastSeen).ToList() : result.OrderBy(a => a.LastSeen).ToList();
                        break;

                    case "status":
                        result = request.Descending ? result.OrderByDescending(a => a.Status).ToList() : result.OrderBy(a => a.Status).ToList();
                        break;

                    default:
                        result = result.OrderBy(a => a.Value).ToList();
                        break;
                }
            }

            // Pagination
            result = result.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            return result;
        }
    }
}
