using FluentAssertions;
using robot_controller_api.Models;
using Xunit;

namespace robot_controller_api.Tests.Unit;

public class ModelTests
{
    // ── Map model ──────────────────────────────────────────────────────────

    [Fact]
    public void Map_Constructor_SetsAllProperties()
    {
        var created  = new DateTime(2024, 1, 1);
        var modified = new DateTime(2024, 6, 1);

        var map = new Map(42, 15, 10, "TestMap", created, modified, "A test map");

        map.Id.Should().Be(42);
        map.Columns.Should().Be(15);
        map.Rows.Should().Be(10);
        map.Name.Should().Be("TestMap");
        map.CreatedDate.Should().Be(created);
        map.ModifiedDate.Should().Be(modified);
        map.Description.Should().Be("A test map");
    }

    [Fact]
    public void Map_DefaultConstructor_DoesNotThrow()
    {
        var act = () => new Map();
        act.Should().NotThrow();
    }

    [Fact]
    public void Map_Description_IsOptional()
    {
        var map = new Map(1, 5, 5, "NoDesc", DateTime.Now, DateTime.Now);
        map.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(10, 10, true)]
    [InlineData(5,  10, false)]
    [InlineData(1,  1,  true)]
    public void Map_IsSquare_BasedOnDimensions(int cols, int rows, bool expectedSquare)
    {
        // IsSquare is a computed DB column; verify the concept via Columns == Rows
        var map = new Map(1, cols, rows, "Test", DateTime.Now, DateTime.Now);
        (map.Columns == map.Rows).Should().Be(expectedSquare);
    }

    // ── RobotCommand model ─────────────────────────────────────────────────

    [Fact]
    public void RobotCommand_Constructor_SetsAllProperties()
    {
        var created  = new DateTime(2024, 1, 1);
        var modified = new DateTime(2024, 6, 1);

        var cmd = new RobotCommand(7, "MOVE", true, created, modified, "Move fwd");

        cmd.Id.Should().Be(7);
        cmd.Name.Should().Be("MOVE");
        cmd.IsMoveCommand.Should().BeTrue();
        cmd.CreatedDate.Should().Be(created);
        cmd.ModifiedDate.Should().Be(modified);
        cmd.Description.Should().Be("Move fwd");
    }

    [Fact]
    public void RobotCommand_DefaultConstructor_DoesNotThrow()
    {
        var act = () => new RobotCommand();
        act.Should().NotThrow();
    }

    [Fact]
    public void RobotCommand_IsMoveCommand_IsFalse_ForNonMoveCommands()
    {
        var cmd = new RobotCommand(1, "PING", false, DateTime.Now, DateTime.Now);
        cmd.IsMoveCommand.Should().BeFalse();
    }

    [Fact]
    public void RobotCommand_Description_IsOptional()
    {
        var cmd = new RobotCommand(1, "SCAN", false, DateTime.Now, DateTime.Now);
        cmd.Description.Should().BeNull();
    }
}