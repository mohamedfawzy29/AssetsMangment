namespace AssetsMangment.Services
{
    public class AssetRelationshipService : IAssetRelationshipService
    {
        private readonly ApplicationDbContext _context;

        public AssetRelationshipService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CreateRelationshipResponse> CreateRelationshipAsync(CreateAssetRelationshipRequest request)
        {
            if (request.SourceAssetId == request.TargetAssetId)
            {
                return new CreateRelationshipResponse
                {
                    Success = false,
                    Error = "Source and Target cannot be the same asset."
                };
            }

            var sourceAsset = await _context.Assets.FindAsync(request.SourceAssetId);

            if (sourceAsset == null)
            {
                return new CreateRelationshipResponse
                {
                    Success = false,
                    Error = "Source asset not found."
                };
            }

            var targetAsset = await _context.Assets.FindAsync(request.TargetAssetId);

            if (targetAsset == null)
            {
                return new CreateRelationshipResponse
                {
                    Success = false,
                    Error = "Target asset not found."
                };
            }

            var relationshipExists = await _context.AssetRelationships.AnyAsync(r =>
                r.SourceAssetId == request.SourceAssetId &&
                r.TargetAssetId == request.TargetAssetId &&
                r.Type == request.Type);

            if (relationshipExists)
            {
                return new CreateRelationshipResponse
                {
                    Success = false,
                    Error = "Relationship already exists."
                };
            }

            var relationship = new AssetRelationship
            {
                Id = Guid.NewGuid(),
                SourceAssetId = request.SourceAssetId,
                TargetAssetId = request.TargetAssetId,
                Type = request.Type
            };

            _context.AssetRelationships.Add(relationship);
            await _context.SaveChangesAsync();

            return new CreateRelationshipResponse
            {
                Success = true,
                Relationship = new AssetRelationshipResponse
                {
                    Id = relationship.Id,
                    Type = relationship.Type,
                    SourceAsset = AssetMapper.ToSummary(sourceAsset),
                    TargetAsset = AssetMapper.ToSummary(targetAsset)
                }
            };
        }
    }
}
