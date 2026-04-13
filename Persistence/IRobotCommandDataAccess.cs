using robot_controller_api;

namespace robot_controller_api.Persistence;
using robot_controller_api.Models;

public interface IRobotCommandDataAccess
{
    List<RobotCommand> GetRobotCommands();

    void AddRobotCommand(RobotCommand command);

    void DeleteRobotCommand(int id);
}