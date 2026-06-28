using AssetsMangment.Data;
using AssetsMangment.DTOs.Request;
using AssetsMangment.Models;
using AssetsMangment.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AssetsManagement.Tests.Services
{
    public class AssetServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly AssetService _service;

        public AssetServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _service = new AssetService(_context);
        }

        [Fact] // Dedup Test (scenario 1: Duplicate asset exists)
        public async Task CreateOrUpdateAssetAsync_Should_UpdateExistingAsset_WhenDuplicateExists()
        {
            // Arrange
            var existingAsset = new Asset
            {
                Id = Guid.NewGuid(),
                Type = AssetType.domain,
                Value = "google.com",
                Source = AssetSource.import,
                Status = AssetStatus.active,
                FirstSeen = DateTime.UtcNow.AddDays(-5),
                LastSeen = DateTime.UtcNow.AddDays(-1),
                Tags = new List<string> { "search" },
                Metadata = new Dictionary<string, string>
        {
            { "Country", "US" }
        }
            };

            _context.Assets.Add(existingAsset);
            await _context.SaveChangesAsync();

            var request = new CreateAssetRequest
            {
                Type = AssetType.domain,
                Value = "Google.com",
                Source = AssetSource.import,
                Tags = new List<string> { "tech" },
                Metadata = new Dictionary<string, string>
        {
            { "ASN", "15169" }
        }
            };

            // Act
            var result = await _service.CreateOrUpdateAssetAsync(request);

            // Assert
            result.IsCreated.Should().BeFalse();

            (await _context.Assets.CountAsync()).Should().Be(1);

            var asset = await _context.Assets.FirstAsync();

            asset.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            asset.Tags.Should().Contain("search");
            asset.Tags.Should().Contain("tech");

            asset.Metadata.Should().ContainKey("Country");
            asset.Metadata.Should().ContainKey("ASN");
        }

        [Fact] // Dedup Test (scenario 2: No duplicate asset exists)
        public async Task CreateOrUpdateAssetAsync_Should_CreateNewAsset_WhenAssetDoesNotExist()
        {
            // Arrange
            var request = new CreateAssetRequest
            {
                Type = AssetType.domain,
                Value = "openai.com",
                Source = AssetSource.manual,
                Tags = new List<string> { "ai", "chatgpt" },
                Metadata = new Dictionary<string, string>
        {
            { "Country", "US" }
        }
            };

            // Act
            var result = await _service.CreateOrUpdateAssetAsync(request);

            // Assert
            result.IsCreated.Should().BeTrue();

            (await _context.Assets.CountAsync()).Should().Be(1);

            var asset = await _context.Assets.FirstAsync();

            asset.Value.Should().Be("openai.com");
            asset.Type.Should().Be(AssetType.domain);
            asset.Status.Should().Be(AssetStatus.active);

            asset.FirstSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            asset.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            asset.Tags.Should().Contain("ai");
            asset.Tags.Should().Contain("chatgpt");

            asset.Metadata.Should().ContainKey("Country");
        }

        [Fact] // filtration test by type
        public async Task GetAssetsAsync_Should_FilterByType()
        {
            // Arrange
            _context.Assets.AddRange(
                new Asset
                {
                    Id = Guid.NewGuid(),
                    Type = AssetType.domain,
                    Value = "google.com",
                    Status = AssetStatus.active,
                    Source = AssetSource.import,
                    Tags = [],
                    Metadata = []
                },
                new Asset
                {
                    Id = Guid.NewGuid(),
                    Type = AssetType.ip_address,
                    Value = "8.8.8.8",
                    Status = AssetStatus.active,
                    Source = AssetSource.scan,
                    Tags = [],
                    Metadata = []
                },
                new Asset
                {
                    Id = Guid.NewGuid(),
                    Type = AssetType.domain,
                    Value = "openai.com",
                    Status = AssetStatus.active,
                    Source = AssetSource.external,
                    Tags = [],
                    Metadata = []
                });

            await _context.SaveChangesAsync();

            var request = new GetAssetsQueryRequest
            {
                Type = AssetType.domain
            };

            // Act
            var result = await _service.GetAssetsAsync(request);

            // Assert
            result.Should().HaveCount(2);

            result.Should().OnlyContain(a => a.Type == AssetType.domain);
        }

        [Fact] // filtration test by tag
        public async Task GetAssetsAsync_Should_FilterByTag()
        {
            // Arrange
            _context.Assets.AddRange(
                new Asset
                {
                    Id = Guid.NewGuid(),
                    Type = AssetType.domain,
                    Value = "google.com",
                    Status = AssetStatus.active,
                    Source = AssetSource.manual,
                    Tags = new() { "search", "tech" },
                    Metadata = new()
                },
                new Asset
                {
                    Id = Guid.NewGuid(),
                    Type = AssetType.domain,
                    Value = "facebook.com",
                    Status = AssetStatus.active,
                    Source = AssetSource.manual,
                    Tags = new() { "social" },
                    Metadata = new()
                },
                new Asset
                {
                    Id = Guid.NewGuid(),
                    Type = AssetType.ip_address,
                    Value = "8.8.8.8",
                    Status = AssetStatus.active,
                    Source = AssetSource.scan,
                    Tags = new() { "tech", "dns" },
                    Metadata = new()
                });

            await _context.SaveChangesAsync();

            var request = new GetAssetsQueryRequest
            {
                Tag = "tech"
            };

            // Act
            var result = await _service.GetAssetsAsync(request);

            // Assert
            result.Should().HaveCount(2);

            result.Should().OnlyContain(a =>
                a.Tags.Contains("tech"));
        }

        [Fact]
        public async Task GetAssetsAsync_ShouldApplyPagination()
        {
            // Arrange
            for (int i = 1; i <= 5; i++)
            {
                _context.Assets.Add(new Asset
                {
                    Id = Guid.NewGuid(),
                    Type = AssetType.domain,
                    Value = $"site{i}.com",
                    Source = AssetSource.manual,
                    Status = AssetStatus.active,
                    FirstSeen = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow,
                    Tags = new(),
                    Metadata = new()
                });
            }

            await _context.SaveChangesAsync();

            var request = new GetAssetsQueryRequest
            {
                Page = 2,
                PageSize = 2,
                SortBy = "value",
                Descending = false
            };

            // Act
            var result = await _service.GetAssetsAsync(request);

            // Assert
            result.Should().HaveCount(2);

            result[0].Value.Should().Be("site3.com");
            result[1].Value.Should().Be("site4.com");
        }
    }
}
