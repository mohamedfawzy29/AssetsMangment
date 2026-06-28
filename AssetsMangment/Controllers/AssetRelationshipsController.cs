
using Microsoft.AspNetCore.Authorization;

namespace AssetsMangment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetRelationshipsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AssetRelationshipService _service;

        public AssetRelationshipsController(ApplicationDbContext context, AssetRelationshipService service)
        {
            _context = context;
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssetRelationshipResponse>>> GetAllRelationships([FromQuery] GetRelationshipsQueryRequest queryRequest)
        {
            var relationships = _context.AssetRelationships
                .AsNoTracking()
                .Include(r => r.SourceAsset)
                .Include(r => r.TargetAsset)
                .AsQueryable();

            if (queryRequest.Type.HasValue)
            {
                relationships = relationships.Where(r => r.Type == queryRequest.Type.Value);
            }

            if (queryRequest.SourceAssetId.HasValue)
            {
                relationships = relationships.Where(r => r.SourceAssetId == queryRequest.SourceAssetId.Value);
            }

            if (queryRequest.TargetAssetId.HasValue)
            {
                relationships = relationships.Where(r => r.TargetAssetId == queryRequest.TargetAssetId.Value);
            }

            relationships = relationships
                .Skip((queryRequest.Page - 1) * queryRequest.PageSize)
                .Take(queryRequest.PageSize);

            var result = await relationships.ToListAsync();

            var response = result.Select(r => new AssetRelationshipResponse
            {
                Id = r.Id,
                Type = r.Type,
                SourceAsset = AssetMapper.ToSummary(r.SourceAsset),
                TargetAsset = AssetMapper.ToSummary(r.TargetAsset)
            }).ToList();

            return Ok(response);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CreateRelationshipResponse>> CreateRelationship(CreateAssetRelationshipRequest request)
        {
            var result = await _service.CreateRelationshipAsync(request);

            if (!result.Success)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Error",
                    Detail = result.Error
                });
            }

            return CreatedAtAction(nameof(GetRelationshipById), new { id = result.Relationship!.Id }, result.Relationship);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AssetRelationshipResponse>> GetRelationshipById(Guid id)
        {
            var relationship = await _context.AssetRelationships.AsNoTracking().Include(r => r.SourceAsset).Include(r => r.TargetAsset).FirstOrDefaultAsync(r => r.Id == id);

            if (relationship == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Relationship not found",
                    Detail = $"This relationship does not exist."
                });
            }

            var response = new AssetRelationshipResponse
            {
                Id = relationship.Id,
                Type = relationship.Type,
                SourceAsset = AssetMapper.ToSummary(relationship.SourceAsset),
                TargetAsset = AssetMapper.ToSummary(relationship.TargetAsset)
            };

            return Ok(response);
        }

        [HttpGet("asset/{assetId}")]
        public async Task<ActionResult<IEnumerable<AssetRelationshipResponse>>> GetAssetRelationships(Guid assetId)
        {
            var assetExists = await _context.Assets.AnyAsync(a => a.Id == assetId);

            if (!assetExists)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Asset not found",
                    Detail = $"This asset does not exist."
                });
            }

            var relationships = await _context.AssetRelationships.AsNoTracking()
                .Include(r => r.SourceAsset)
                .Include(r => r.TargetAsset)
                .Where(r => r.SourceAssetId == assetId || r.TargetAssetId == assetId)
                .ToListAsync();

            var response = relationships.Select(r => new AssetRelationshipResponse
            {
                Id = r.Id,
                Type = r.Type,
                SourceAsset = AssetMapper.ToSummary(r.SourceAsset),
                TargetAsset = AssetMapper.ToSummary(r.TargetAsset)
            }).ToList();

            return Ok(response);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRelationship(Guid id)
        {
            var relationship = await _context.AssetRelationships.FindAsync(id);

            if (relationship == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Relationship not found",
                    Detail = $"This relationship does not exist."
                });
            }

            _context.AssetRelationships.Remove(relationship);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpPost("bulk")]
        public async Task<ActionResult<BulkCreateRelationshipsResponse>> BulkCreateRelationships(BulkCreateRelationshipsRequest request)
        {
            int total = request.Relationships.Count;
            int imported = 0;
            int duplicates = 0;
            int invalid = 0;

            var assetIds = await _context.Assets
                .Select(a => a.Id)
                .ToHashSetAsync();

            var existingRelationships = await _context.AssetRelationships
                .Select(r => new
                {
                    r.SourceAssetId,
                    r.TargetAssetId,
                    r.Type
                })
                .ToListAsync();

            var relationshipKeys = existingRelationships
                .Select(r => (r.SourceAssetId, r.TargetAssetId, r.Type))
                .ToHashSet();

            var requestKeys = new HashSet<(Guid, Guid, RelationshipType)>();

            foreach (var relationshipRequest in request.Relationships)
            {
                if (relationshipRequest.SourceAssetId == relationshipRequest.TargetAssetId)
                {
                    invalid++;
                    continue;
                }

                if (!assetIds.Contains(relationshipRequest.SourceAssetId) ||
                    !assetIds.Contains(relationshipRequest.TargetAssetId))
                {
                    invalid++;
                    continue;
                }

                var key = (
                    relationshipRequest.SourceAssetId,
                    relationshipRequest.TargetAssetId,
                    relationshipRequest.Type
                );

                if (!requestKeys.Add(key))
                {
                    duplicates++;
                    continue;
                }

                if (relationshipKeys.Contains(key))
                {
                    duplicates++;
                    continue;
                }

                var relationship = new AssetRelationship
                {
                    Id = Guid.NewGuid(),
                    SourceAssetId = relationshipRequest.SourceAssetId,
                    TargetAssetId = relationshipRequest.TargetAssetId,
                    Type = relationshipRequest.Type
                };

                _context.AssetRelationships.Add(relationship);
                relationshipKeys.Add(key);
                imported++;
            }

            await _context.SaveChangesAsync();

            return Ok(new BulkCreateRelationshipsResponse
            {
                Total = total,
                Imported = imported,
                Duplicates = duplicates,
                Invalid = invalid
            });
        }

        //[HttpPost("generate-sample")]
        //public async Task<ActionResult<GenerateSampleRelationshipsResponse>> GenerateSampleRelationships()
        //{
        //    int created = 0;
        //    int skipped = 0;

        //    var assets = await _context.Assets
        //        .AsNoTracking()
        //        .ToListAsync();

        //    var relationshipKeys = await _context.AssetRelationships
        //        .Select(r => new
        //        {
        //            r.SourceAssetId,
        //            r.TargetAssetId,
        //            r.Type
        //        })
        //        .ToListAsync();

        //    var existingRelationships = relationshipKeys
        //        .Select(r => (r.SourceAssetId, r.TargetAssetId, r.Type))
        //        .ToHashSet();

        //    var domains = assets.Where(a => a.Type == AssetType.domain).ToList();
        //    var subdomains = assets.Where(a => a.Type == AssetType.subdomain).ToList();
        //    var ips = assets.Where(a => a.Type == AssetType.ip_address).ToList();
        //    var technologies = assets.Where(a => a.Type == AssetType.technology).ToList();
        //    var certificates = assets.Where(a => a.Type == AssetType.certificate).ToList();
        //    var services = assets.Where(a => a.Type == AssetType.service).ToList();

        //    void AddRelationship(Guid sourceId, Guid targetId, RelationshipType type)
        //    {
        //        var key = (sourceId, targetId, type);

        //        if (existingRelationships.Contains(key))
        //        {
        //            skipped++;
        //            return;
        //        }

        //        _context.AssetRelationships.Add(new AssetRelationship
        //        {
        //            Id = Guid.NewGuid(),
        //            SourceAssetId = sourceId,
        //            TargetAssetId = targetId,
        //            Type = type
        //        });

        //        existingRelationships.Add(key);
        //        created++;
        //    }

        //    // Subdomain -> Domain
        //    foreach (var subdomain in subdomains)
        //    {
        //        var parentDomain = domains.FirstOrDefault(d =>
        //            subdomain.Value.EndsWith(d.Value, StringComparison.OrdinalIgnoreCase));

        //        if (parentDomain != null)
        //        {
        //            AddRelationship(
        //                subdomain.Id,
        //                parentDomain.Id,
        //                RelationshipType.SubdomainOf);
        //        }
        //    }

        //    // Domain -> IP
        //    for (int i = 0; i < Math.Min(domains.Count, ips.Count); i++)
        //    {
        //        AddRelationship(
        //            domains[i].Id,
        //            ips[i].Id,
        //            RelationshipType.ResolvesTo);
        //    }

        //    // Domain -> Technology
        //    for (int i = 0; i < Math.Min(domains.Count, technologies.Count); i++)
        //    {
        //        AddRelationship(
        //            domains[i].Id,
        //            technologies[i].Id,
        //            RelationshipType.UsesTechnology);
        //    }

        //    // Domain -> Certificate
        //    for (int i = 0; i < Math.Min(domains.Count, certificates.Count); i++)
        //    {
        //        AddRelationship(
        //            domains[i].Id,
        //            certificates[i].Id,
        //            RelationshipType.Secures);
        //    }

        //    // Domain -> Service
        //    for (int i = 0; i < Math.Min(domains.Count, services.Count); i++)
        //    {
        //        AddRelationship(
        //            domains[i].Id,
        //            services[i].Id,
        //            RelationshipType.RunsOn);
        //    }

        //    await _context.SaveChangesAsync();

        //    return Ok(new GenerateSampleRelationshipsResponse
        //    {
        //        Created = created,
        //        Skipped = skipped
        //    });
        //}

    }
}
