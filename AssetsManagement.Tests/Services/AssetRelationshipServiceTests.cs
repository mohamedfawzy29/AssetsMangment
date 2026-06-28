using AssetsMangment.Data;
using AssetsMangment.DTOs.Request;
using AssetsMangment.Models;
using AssetsMangment.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AssetsManagement.Tests.Services
{
    public class AssetRelationshipServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly AssetRelationshipService _service;

        public AssetRelationshipServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _service = new AssetRelationshipService(_context);
        }

        [Fact] // create relationship test scenario 1
        public async Task CreateRelationship_ShouldReturnSuccess_WhenDataIsValid()
        {
            // Arrange
            var source = new Asset { Id = Guid.NewGuid() };
            var target = new Asset { Id = Guid.NewGuid() };

            _context.Assets.AddRange(source, target);
            await _context.SaveChangesAsync();

            var request = new CreateAssetRelationshipRequest
            {
                SourceAssetId = source.Id,
                TargetAssetId = target.Id,
                Type = RelationshipType.SubdomainOf
            };

            // Act
            var result = await _service.CreateRelationshipAsync(request);

            // Assert
            result.Success.Should().BeTrue();
            result.Relationship.Should().NotBeNull();

            result.Relationship!.SourceAsset.Id.Should().Be(source.Id);
            result.Relationship.TargetAsset.Id.Should().Be(target.Id);
        }

        [Fact] // create relationship test scenario 2
        public async Task CreateRelationship_ShouldReturnFailure_WhenRelationshipAlreadyExists()
        {
            // Arrange
            var source = new Asset { Id = Guid.NewGuid() };

            var target = new Asset { Id = Guid.NewGuid() };

            _context.Assets.AddRange(source, target);

            _context.AssetRelationships.Add(new AssetRelationship
            {
                Id = Guid.NewGuid(),
                SourceAssetId = source.Id,
                TargetAssetId = target.Id,
                Type = RelationshipType.SubdomainOf
            });

            await _context.SaveChangesAsync();

            var request = new CreateAssetRelationshipRequest
            {
                SourceAssetId = source.Id,
                TargetAssetId = target.Id,
                Type = RelationshipType.SubdomainOf
            };

            // Act
            var result = await _service.CreateRelationshipAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Error.Should().Be("Relationship already exists.");
        }

    }
}
