
using Microsoft.AspNetCore.Authorization;

namespace AssetsMangment.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAssetService _assetService;
        public AssetsController(ApplicationDbContext context , IAssetService assetService)
        {
            _context = context;
            _assetService = assetService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CreateOrUpdateAssetResponse>> CreateAsset(CreateAssetRequest request)
        {
            var result = await _assetService.CreateOrUpdateAssetAsync(request);

            if (result.IsCreated)
            {
                return CreatedAtAction(nameof(GetAssetById), new { id = result.Asset.Id }, AssetMapper.ToResponse(result.Asset));
            }

            return Ok(AssetMapper.ToResponse(result.Asset));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AssetResponse>> GetAssetById(Guid id)
        {
            var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id);
            if (asset == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Asset not found",
                    Detail = $"This asset does not exist."
                });
            }
            return Ok(AssetMapper.ToResponse(asset));
        }
        [HttpGet]
        public async Task<ActionResult<List<AssetResponse>>> GetAssets([FromQuery] GetAssetsQueryRequest request)
        {
            var assets = await _assetService.GetAssetsAsync(request);

            return Ok(assets.Select(AssetMapper.ToResponse));
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<AssetResponse>> UpdateAsset(Guid id, UpdateAssetRequest request)
        {
            var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id);
            if (asset == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Asset not found",
                    Detail = $"This asset does not exist."
                });
            }

            asset.Status = request.Status;
            asset.Tags = request.Tags;
            asset.Metadata = request.Metadata;

            await _context.SaveChangesAsync();

            return Ok(AssetMapper.ToResponse(asset)); // 200
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(Guid id)
        {
            var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id);
            if (asset == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Asset not found",
                    Detail = $"This asset does not exist."
                });
            }
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
            return NoContent(); // 204
        }

        [Authorize]
        [HttpPost("bulk")]
        public async Task<ActionResult<BulkCreateAssetsResponse>> BulkCreateAssets(BulkCreateAssetsRequest request)
        {
            int total = request.Assets.Count;
            int imported = 0;
            int duplicates = 0;

            int failed = 0;
            foreach (var assetRequest in request.Assets)
            {
                try
                {
                    var result = await _assetService.CreateOrUpdateAssetAsync(assetRequest);

                    if (result.IsCreated)
                    {
                        imported++;
                    }
                    else
                    {
                        duplicates++;
                    }
                }
                catch
                {
                    failed++;
                }
            }

            await _context.SaveChangesAsync();

            var response = new BulkCreateAssetsResponse
            {
                Total = total,
                Imported = imported,
                Duplicates = duplicates,
                Failed = failed
            };

            return Ok(response);
        }

        [Authorize]
        [HttpPatch("{id}/stale")]
        public async Task<ActionResult<AssetResponse>> MarkAssetAsStale(Guid id)
        {
            var asset = await _context.Assets.FindAsync(id);

            if (asset == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Asset not found",
                    Detail = $"This asset does not exist."
                });
            }

            if (asset.Status == AssetStatus.stale)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = "Asset is already marked as stale."
                });
            }

            asset.Status = AssetStatus.stale;

            await _context.SaveChangesAsync();

            return Ok(AssetMapper.ToResponse(asset));
        }

        [HttpGet("{id}/graph")]
        public async Task<ActionResult<AssetGraphResponse>> GetAssetGraph(Guid id)
        {
            var asset = await _context.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asset == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Asset not found",
                    Detail = $"This asset does not exist."
                });
            }

            var relationships = await _context.AssetRelationships
                .AsNoTracking()
                .Include(r => r.SourceAsset)
                .Include(r => r.TargetAsset)
                .Where(r => r.SourceAssetId == id || r.TargetAssetId == id)
                .ToListAsync();

            var response = new AssetGraphResponse
            {
                Asset = AssetMapper.ToSummary(asset),

                Relationships = relationships.Select(r =>
                {
                    var isOutgoing = r.SourceAssetId == id;

                    return new AssetGraphRelationshipResponse
                    {
                        Type = r.Type,
                        Direction = isOutgoing? RelationshipDirection.Outgoing : RelationshipDirection.Incoming,
                        Asset = isOutgoing? AssetMapper.ToSummary(r.TargetAsset) : AssetMapper.ToSummary(r.SourceAsset)
                    };
                }).ToList()
            };

            return Ok(response);
        }

    }
}