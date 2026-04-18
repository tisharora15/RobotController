using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using robot_controller_api.Controllers;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using Xunit;

namespace robot_controller_api.Tests.Unit;

public class MapsControllerTests
{
    private readonly Mock<IMapDataAccess> _mockRepo;
    private readonly MapsController _controller;

    private static readonly List<Map> _seedMaps = new()
    {
        new Map(1, 10, 10, "Square Map",  DateTime.Now, DateTime.Now, "10x10 grid"),
        new Map(2, 5,  10, "Tall Map",    DateTime.Now, DateTime.Now, "5x10 grid"),
        new Map(3, 20, 20, "Large Square",DateTime.Now, DateTime.Now, "20x20 grid"),
    };

    public MapsControllerTests()
    {
        _mockRepo   = new Mock<IMapDataAccess>();
        _controller = new MapsController(_mockRepo.Object);
    }

    // ── GetAllMaps ─────────────────────────────────────────────────────────

    [Fact]
    public void GetAllMaps_ReturnsAllMaps()
    {
        _mockRepo.Setup(r => r.GetMaps()).Returns(_seedMaps);

        var result = _controller.GetAllMaps();

        result.Should().HaveCount(3);
    }

    // ── GetSquareMaps ──────────────────────────────────────────────────────

    [Fact]
    public void GetSquareMaps_ReturnsOnlySquareMaps()
    {
        _mockRepo.Setup(r => r.GetMaps()).Returns(_seedMaps);

        var result = _controller.GetSquareMaps();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(m => m.Columns == m.Rows);
    }

    [Fact]
    public void GetSquareMaps_ReturnsEmpty_WhenNoSquareMaps()
    {
        var nonSquare = new List<Map>
        {
            new Map(1, 5, 10, "Rect", DateTime.Now, DateTime.Now)
        };
        _mockRepo.Setup(r => r.GetMaps()).Returns(nonSquare);

        var result = _controller.GetSquareMaps();

        result.Should().BeEmpty();
    }

    // ── GetMapById ─────────────────────────────────────────────────────────

    [Fact]
    public void GetMapById_ReturnsOk_WhenFound()
    {
        _mockRepo.Setup(r => r.GetMaps()).Returns(_seedMaps);

        var result = _controller.GetMapById(1);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<Map>()
            .Which.Id.Should().Be(1);
    }

    [Fact]
    public void GetMapById_ReturnsNotFound_WhenMissing()
    {
        _mockRepo.Setup(r => r.GetMaps()).Returns(_seedMaps);

        var result = _controller.GetMapById(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── AddMap ─────────────────────────────────────────────────────────────

    [Fact]
    public void AddMap_ReturnsOk_WhenValid()
    {
        _mockRepo.Setup(r => r.AddMap(It.IsAny<Map>()));

        var newMap = new Map(0, 8, 8, "New Map", DateTime.Now, DateTime.Now, "A new map");

        var result = _controller.AddMap(newMap);

        result.Should().BeOfType<OkObjectResult>();
        _mockRepo.Verify(r => r.AddMap(It.IsAny<Map>()), Times.Once);
    }

    [Fact]
    public void AddMap_ReturnsBadRequest_WhenNull()
    {
        var result = _controller.AddMap(null!);

        result.Should().BeOfType<BadRequestResult>();
    }

    // ── DeleteMap ──────────────────────────────────────────────────────────

    [Fact]
    public void DeleteMap_ReturnsNoContent()
    {
        _mockRepo.Setup(r => r.DeleteMap(1));

        var result = _controller.DeleteMap(1);

        result.Should().BeOfType<NoContentResult>();
        _mockRepo.Verify(r => r.DeleteMap(1), Times.Once);
    }

    // ── CheckCoordinate ────────────────────────────────────────────────────

    [Theory]
    [InlineData(5,  5,  true)]   // inside 10x10
    [InlineData(9,  9,  true)]   // boundary (0-indexed)
    [InlineData(10, 5,  false)]  // x out of bounds
    [InlineData(5,  10, false)]  // y out of bounds
    [InlineData(15, 15, false)]  // both out
    public void CheckCoordinate_ReturnsCorrectResult(int x, int y, bool expected)
    {
        _mockRepo.Setup(r => r.GetMaps()).Returns(_seedMaps);

        var result = _controller.CheckCoordinate(1, x, y);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(-5, -5)]
    public void CheckCoordinate_ReturnsBadRequest_WhenNegative(int x, int y)
    {
        var result = _controller.CheckCoordinate(1, x, y);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void CheckCoordinate_ReturnsNotFound_WhenMapMissing()
    {
        _mockRepo.Setup(r => r.GetMaps()).Returns(_seedMaps);

        var result = _controller.CheckCoordinate(999, 5, 5);

        result.Should().BeOfType<NotFoundResult>();
    }
}