using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public class RobotCommandEF : IRobotCommandDataAccess
{
    private readonly RobotContext _context;

    public RobotCommandEF(RobotContext context)
    {
        _context = context;
    }

    public List<RobotCommand> GetRobotCommands()
    {
        return _context.RobotCommands.ToList();
    }

    public void AddRobotCommand(RobotCommand command)
    {
        _context.RobotCommands.Add(command);
        _context.SaveChanges();
    }

    public void DeleteRobotCommand(int id)
    {
        var command = _context.RobotCommands.FirstOrDefault(c => c.Id == id);

        if (command != null)
        {
            _context.RobotCommands.Remove(command);
            _context.SaveChanges();
        }
    }
}