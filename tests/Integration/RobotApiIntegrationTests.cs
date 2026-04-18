using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace robot_controller_api.Tests.Integration;

/// <summary>
/// Integration tests using an in-memory database — no real Postgres needed.
/// </summary>
public class RobotApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RobotApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real DB context registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RobotContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Replace with in-memory database
                services.AddDbContext<RobotContext>(options =>
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));

                // Replace EF data access with the real EF classes pointing at in-memory DB
                var mapDesc = services.SingleOrDefault(d => d.ServiceType == typeof(IMapDataAccess));
                if (mapDesc != null) services.Remove(mapDesc);
                services.AddScoped<IMapDataAccess, MapEF>();

                var cmdDesc = services.SingleOrDefault(d => d.ServiceType == typeof(IRobotCommandDataAccess));
                if (cmdDesc != null) services.Remove(cmdDesc);
                services.AddScoped<IRobotCommandDataAccess, RobotCommandEF>();

                // Seed the database
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RobotContext>();
                db.Database.EnsureCreated();
                SeedDatabase(db);
            });
        }).CreateClient();
    }

    private static void SeedDatabase(RobotContext db)
    {
        if (!db.Maps.Any())
        {
            db.Maps.AddRange(
                new Map(0, 10, 10, "Square Map",  DateTime.Now, DateTime.Now, "10x10"),
                new Map(0, 5,  10, "Tall Map",    DateTime.Now, DateTime.Now, "5x10")
            );
        }

        if (!db.RobotCommands.Any())
        {
            db.RobotCommands.AddRange(
                new RobotCommand(0, "MOVE",  true,  DateTime.Now, DateTime.Now, "Move fwd"),
                new RobotCommand(0, "LEFT",  true,  DateTime.Now, DateTime.Now, "Turn left"),
                new RobotCommand(0, "PING",  false, DateTime.Now, DateTime.Now, "Ping")
            );
        }

        db.SaveChanges();
    }

    // ── Maps endpoints ─────────────────────────────────────────────────────

    [Fact]
    public async Task GET_Maps_Returns200()
    {
        var response = await _client.GetAsync("/api/maps");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_Maps_ReturnsJson()
    {
        var maps = await _client.GetFromJsonAsync<List<Map>>("/api/maps");
        maps.Should().NotBeNull();
        maps!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GET_SquareMaps_ReturnsOnlySquareMaps()
    {
        var maps = await _client.GetFromJsonAsync<List<Map>>("/api/maps/square");
        maps.Should().NotBeNull();
        maps!.Should().OnlyContain(m => m.Columns == m.Rows);
    }

    [Fact]
    public async Task GET_MapById_Returns404_ForNonExistentId()
    {
        var response = await _client.GetAsync("/api/maps/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_Map_Returns200_WithValidData()
    {
        var newMap = new { Columns = 8, Rows = 8, Name = "Integration Map", Description = "Test" };
        var response = await _client.PostAsJsonAsync("/api/maps", newMap);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── RobotCommands endpoints ────────────────────────────────────────────

    [Fact]
    public async Task GET_RobotCommands_Returns200()
    {
        var response = await _client.GetAsync("/api/robot-commands");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_MoveCommands_ReturnsOnlyMoveCommands()
    {
        var commands = await _client.GetFromJsonAsync<List<RobotCommand>>("/api/robot-commands/move");
        commands.Should().NotBeNull();
        commands!.Should().OnlyContain(c => c.IsMoveCommand);
    }

    [Fact]
    public async Task POST_RobotCommand_Returns200_WithValidData()
    {
        var newCmd = new { Name = "SCAN", IsMoveCommand = false, Description = "Scan area" };
        var response = await _client.PostAsJsonAsync("/api/robot-commands", newCmd);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_RobotCommand_Returns409_WhenDuplicate()
    {
        var cmd = new { Name = "MOVE", IsMoveCommand = true, Description = "Duplicate" };
        var response = await _client.PostAsJsonAsync("/api/robot-commands", cmd);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ── Health endpoints ───────────────────────────────────────────────────

    [Fact]
    public async Task GET_HealthLive_Returns200()
    {
        var response = await _client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}