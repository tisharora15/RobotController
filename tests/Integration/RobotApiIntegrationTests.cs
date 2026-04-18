using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace robot_controller_api.Tests.Integration;

public class RobotApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RobotApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
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
                new Map(0, 10, 10, "Square Map", DateTime.Now, DateTime.Now, "10x10"),
                new Map(0, 5,  10, "Tall Map",   DateTime.Now, DateTime.Now, "5x10")
            );
        }
        if (!db.RobotCommands.Any())
        {
            db.RobotCommands.AddRange(
                new RobotCommand(0, "MOVE", true,  DateTime.Now, DateTime.Now, "Move fwd"),
                new RobotCommand(0, "LEFT", true,  DateTime.Now, DateTime.Now, "Turn left"),
                new RobotCommand(0, "PING", false, DateTime.Now, DateTime.Now, "Ping")
            );
        }
        db.SaveChanges();
    }

    [Fact] public async Task GET_Maps_Returns200()
    { var r = await _client.GetAsync("/api/maps"); r.StatusCode.Should().Be(HttpStatusCode.OK); }

    [Fact] public async Task GET_Maps_ReturnsJson()
    { var maps = await _client.GetFromJsonAsync<List<Map>>("/api/maps"); maps.Should().NotBeNull(); maps!.Count.Should().BeGreaterThan(0); }

    [Fact] public async Task GET_SquareMaps_ReturnsOnlySquareMaps()
    { var maps = await _client.GetFromJsonAsync<List<Map>>("/api/maps/square"); maps.Should().NotBeNull(); maps!.Should().OnlyContain(m => m.Columns == m.Rows); }

    [Fact] public async Task GET_MapById_Returns404_ForNonExistentId()
    { var r = await _client.GetAsync("/api/maps/9999"); r.StatusCode.Should().Be(HttpStatusCode.NotFound); }

    [Fact] public async Task POST_Map_Returns200_WithValidData()
    { var r = await _client.PostAsJsonAsync("/api/maps", new { Columns = 8, Rows = 8, Name = "Integration Map", Description = "Test" }); r.StatusCode.Should().Be(HttpStatusCode.OK); }

    [Fact] public async Task GET_RobotCommands_Returns200()
    { var r = await _client.GetAsync("/api/robot-commands"); r.StatusCode.Should().Be(HttpStatusCode.OK); }

    [Fact] public async Task GET_MoveCommands_ReturnsOnlyMoveCommands()
    { var cmds = await _client.GetFromJsonAsync<List<RobotCommand>>("/api/robot-commands/move"); cmds.Should().NotBeNull(); cmds!.Should().OnlyContain(c => c.IsMoveCommand); }

    [Fact] public async Task POST_RobotCommand_Returns200_WithValidData()
    { var r = await _client.PostAsJsonAsync("/api/robot-commands", new { Name = "SCAN", IsMoveCommand = false, Description = "Scan area" }); r.StatusCode.Should().Be(HttpStatusCode.OK); }

    [Fact] public async Task POST_RobotCommand_Returns409_WhenDuplicate()
    { var r = await _client.PostAsJsonAsync("/api/robot-commands", new { Name = "MOVE", IsMoveCommand = true, Description = "Duplicate" }); r.StatusCode.Should().Be(HttpStatusCode.Conflict); }

    [Fact] public async Task GET_HealthLive_Returns200()
    { var r = await _client.GetAsync("/health/live"); r.StatusCode.Should().Be(HttpStatusCode.OK); }
}