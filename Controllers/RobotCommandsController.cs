using Microsoft.AspNetCore.Mvc;
using robot_controller_api;
using robot_controller_api.Persistence;
using robot_controller_api.Models;
namespace robot_controller_api.Controllers;

[ApiController]
[Route("api/robot-commands")]
public class RobotCommandsController : ControllerBase
{
    private readonly IRobotCommandDataAccess _robotRepo;

    public RobotCommandsController(IRobotCommandDataAccess robotRepo)
    {
        _robotRepo = robotRepo;
    }

    // GET api/robot-commands
    [HttpGet]
    public IEnumerable<RobotCommand> GetAllRobotCommands()
    {
        return _robotRepo.GetRobotCommands();
    }

    // GET api/robot-commands/move
    [HttpGet("move")]
    public IEnumerable<RobotCommand> GetMoveCommandsOnly()
    {
        return _robotRepo
            .GetRobotCommands()
            .Where(c => c.IsMoveCommand);
    }

    // GET api/robot-commands/{id}
    [HttpGet("{id}", Name = "GetRobotCommand")]
    public IActionResult GetRobotCommandById(int id)
    {
        var command = _robotRepo
            .GetRobotCommands()
            .FirstOrDefault(c => c.Id == id);

        if (command == null)
            return NotFound();

        return Ok(command);
    }

    // POST api/robot-commands
    [HttpPost]
    public IActionResult AddRobotCommand(RobotCommand newCommand)
    {
        if (newCommand == null)
            return BadRequest();

        var commands = _robotRepo.GetRobotCommands();

        if (commands.Any(c => c.Name == newCommand.Name))
            return Conflict("Command already exists.");

        var command = new RobotCommand(
            0,
            newCommand.Name,
            newCommand.IsMoveCommand,
            DateTime.Now,
            DateTime.Now,
            newCommand.Description
        );

        _robotRepo.AddRobotCommand(command);

        return Ok(command);
    }
    // PUT api/robot-commands/{id}
[HttpPut("{id}")]
public IActionResult UpdateRobotCommand(int id, RobotCommand updatedCommand)
{
    if (updatedCommand == null)
        return BadRequest();

    var command = _robotRepo
        .GetRobotCommands()
        .FirstOrDefault(c => c.Id == id);

    if (command == null)
        return NotFound();

    command.Name = updatedCommand.Name;
    command.Description = updatedCommand.Description;
    command.IsMoveCommand = updatedCommand.IsMoveCommand;

    return NoContent();
}
// PATCH api/robot-commands/{id}
[HttpPatch("{id}")]
public IActionResult PatchRobotCommand(int id, RobotCommand updatedCommand)
{
    var command = _robotRepo
        .GetRobotCommands()
        .FirstOrDefault(c => c.Id == id);

    if (command == null)
        return NotFound();

    if (!string.IsNullOrEmpty(updatedCommand.Name))
        command.Name = updatedCommand.Name;

    if (!string.IsNullOrEmpty(updatedCommand.Description))
        command.Description = updatedCommand.Description;

    command.IsMoveCommand = updatedCommand.IsMoveCommand;

    return NoContent();
}

    // DELETE api/robot-commands/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteRobotCommand(int id)
    {
        var command = _robotRepo
            .GetRobotCommands()
            .FirstOrDefault(c => c.Id == id);

        if (command == null)
            return NotFound();

        _robotRepo.DeleteRobotCommand(id);

        return NoContent();
    }
}