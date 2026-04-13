using Npgsql;
using robot_controller_api;

namespace robot_controller_api.Persistence;
using robot_controller_api.Models;
public class RobotCommandRepository :
    IRobotCommandDataAccess, IRepository
{
    private IRepository _repo => this;

    public List<RobotCommand> GetRobotCommands()
    {
        var commands = _repo.ExecuteReader<RobotCommand>(
            "SELECT * FROM robotcommand");

        return commands;
    }

    public void AddRobotCommand(RobotCommand command)
    {
        var sql = @"INSERT INTO robotcommand
                    (""Name"", description, ismovecommand, createddate, modifieddate)
                    VALUES (@name,@description,@ismove,@created,@modified)";

        var parameters = new NpgsqlParameter[]
        {
            new("name", command.Name),
            new("description", command.Description ?? (object)DBNull.Value),
            new("ismove", command.IsMoveCommand),
            new("created", command.CreatedDate),
            new("modified", command.ModifiedDate)
        };

        _repo.ExecuteReader<RobotCommand>(sql, parameters);
    }

    public void DeleteRobotCommand(int id)
    {
        var parameters = new NpgsqlParameter[]
        {
            new("id", id)
        };

        _repo.ExecuteReader<RobotCommand>(
            "DELETE FROM robotcommand WHERE id=@id",
            parameters);
    }
}