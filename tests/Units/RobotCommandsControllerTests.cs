using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using robot_controller_api.Controllers;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using Xunit;

namespace robot_controller_api.Tests.Unit;

public class RobotCommandsControllerTests
{
    private readonly Mock<IRobotCommandDataAccess> _mockRepo;
    private readonly RobotCommandsController _controller;

    private static readonly List<RobotCommand> _seedCommands = new()
    {
        new RobotCommand(1, "MOVE",  true,  DateTime.Now, DateTime.Now, "Move forward"),
        new RobotCommand(2, "LEFT",  true,  DateTime.Now, DateTime.Now, "Turn left"),
        new RobotCommand(3, "RIGHT", true,  DateTime.Now, DateTime.Now, "Turn right"),
        new RobotCommand(4, "PING",  false, DateTime.Now, DateTime.Now, "Ping robot"),
    };

    public RobotCommandsControllerTests()
    {
        _mockRepo   = new Mock<IRobotCommandDataAccess>();
        _controller = new RobotCommandsController(_mockRepo.Object);
    }

    // ── GetAllRobotCommands ────────────────────────────────────────────────

    [Fact]
    public void GetAllRobotCommands_ReturnsAllCommands()
    {
        _mockRepo.Setup(r => r.GetRobotCommands()).Returns(_seedCommands);

        var result = _controller.GetAllRobotCommands();

        result.Should().HaveCount(4);
    }

    [Fact]
    public void GetAllRobotCommands_ReturnsEmptyList_WhenNoCommands()
    {
        _mockRepo.Setup(r => r.GetRobotCommands()).Returns(new List<RobotCommand>());

        var result = _controller.GetAllRobotCommands();

        result.Should().BeEmpty();
    }

    // ── GetMoveCommandsOnly ────────────────────────────────────────────────

    [Fact]
    public void GetMoveCommandsOnly_ReturnsOnlyMoveCommands()
    {
        _mockRepo.Setup(r => r.GetRobotCommands()).Returns(_seedCommands);

        var result = _controller.GetMoveCommandsOnly();

        result.Should().OnlyContain(c => c.IsMoveCommand);
        result.Should().HaveCount(3);
    }

    // ── GetRobotCommandById ────────────────────────────────────────────────

    [Fact]
    public void GetRobotCommandById_ReturnsOk_WhenFound()
    {
        _mockRepo.Setup(r => r.GetRobotCommands()).Returns(_seedCommands);

        var result = _controller.GetRobotCommandById(1);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<RobotCommand>()
            .Which.Id.Should().Be(1);
    }

    [Fact]
    public void GetRobotCommandById_ReturnsNotFound_WhenMissing()
    {
        _mockRepo.Setup(r => r.GetRobotCommands()).Returns(_seedCommands);

        var result = _controller.GetRobotCommandById(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── AddRobotCommand ────────────────────────────────────────────────────

    [Fact]
    public void AddRobotCommand_ReturnsOk_WhenValid()
    {
        _mockRepo.Setup(r => r.GetRobotCommands()).Returns(new List<RobotCommand>());
        _mockRepo.Setup(r => r.AddRobotCommand(It.IsAny<RobotCommand>()));

        var newCmd = new RobotCommand(0, "SCAN", false, DateTime.Now, DateTime.Now, "Scan area");

        var result = _controller.AddRobotCommand(newCmd);

        result.Should().BeOfType<OkObjectResult>();
        _mockRepo.Verify(r => r.AddRobotCommand(It.IsAny<RobotCommand>()), Times.Once);
    }

    [Fact]
    public void AddRobotCommand_ReturnsBadRequest_WhenNull()
    {
        var result = _controller.AddRobotCommand(null!);

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public void AddRobotCommand_ReturnsConflict_WhenDuplicateName()
    {
        _mockRepo.Setup(r => r.GetRobotCommands()).Returns(_seedCommands);

        var duplicate = new RobotCommand(0, "MOVE", true, DateTime.Now, DateTime.Now);

        var result = _controller.AddRobotCommand(duplicate);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    // ── DeleteRobotCommand ─────────────────────────────────────────────────

    [Fact]
    public void DeleteRobotCommand_ReturnsNoContent_WhenExists()
    {
        _mockRepo.Setup(r => r.GetRobotCommands()).Returns(_seedCommands);
        _mockRepo.Setup(r => r.DeleteRobotCommand(1));

        var result = _controller.DeleteRobotCommand(1);

        result.Should().BeOfType<NoContentResult>();
        _mockRepo.Verify(r => r.DeleteRobotCommand(1), Times.Once);
    }

    [Fact]
    public void DeleteRobotCommand_ReturnsNotFound_WhenMissing()
    {
        _mockRepo.Setup(r => r.GetRobotCommands()).Returns(_seedCommands);

        var result = _controller.DeleteRobotCommand(999);

        result.Should().BeOfType<NotFoundResult>();
        _mockRepo.Verify(r => r.DeleteRobotCommand(It.IsAny<int>()), Times.Never);
    }

    // ── UpdateRobotCommand ─────────────────────────────────────────────────

    [Fact]
    public void UpdateRobotCommand_ReturnsNoContent_WhenValid()
    {
        _mockRepo.Setup(r => r.GetRobotCommands()).Returns(_seedCommands);

        var updated = new RobotCommand(0, "MOVE_UPDATED", true, DateTime.Now, DateTime.Now, "Updated");

        var result = _controller.UpdateRobotCommand(1, updated);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void UpdateRobotCommand_ReturnsBadRequest_WhenNull()
    {
        var result = _controller.UpdateRobotCommand(1, null!);

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public void UpdateRobotCommand_ReturnsNotFound_WhenCommandMissing()
    {
        _mockRepo.Setup(r => r.GetRobotCommands()).Returns(_seedCommands);

        var updated = new RobotCommand(0, "GHOST", false, DateTime.Now, DateTime.Now);

        var result = _controller.UpdateRobotCommand(999, updated);

        result.Should().BeOfType<NotFoundResult>();
    }
}