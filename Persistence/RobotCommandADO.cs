using Npgsql;
using robot_controller_api;

namespace robot_controller_api.Persistence;
using robot_controller_api.Models;
public class RobotCommandADO : IRobotCommandDataAccess
{
    private const string CONNECTION_STRING =
        "Host=localhost;Username=postgres;Password=prime;Database=sit331";

    // GET all robot commands
    public List<RobotCommand> GetRobotCommands()
    {
        var commands = new List<RobotCommand>();

        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT * FROM robotcommand", conn);
        using var dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            var command = new RobotCommand(
                (int)dr["id"],
                (string)dr["Name"],
                (bool)dr["ismovecommand"],
                (DateTime)dr["createddate"],
                (DateTime)dr["modifieddate"],
                dr["description"] as string
            );

            commands.Add(command);
        }

        return commands;
    }

    // INSERT command
    public void AddRobotCommand(RobotCommand command)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        var sql = @"INSERT INTO robotcommand
                   (""Name"", description, ismovecommand, createddate, modifieddate)
                   VALUES (@name,@description,@ismove,@created,@modified)";

        using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("name", command.Name);
        cmd.Parameters.AddWithValue("description", (object?)command.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ismove", command.IsMoveCommand);
        cmd.Parameters.AddWithValue("created", command.CreatedDate);
        cmd.Parameters.AddWithValue("modified", command.ModifiedDate);

        cmd.ExecuteNonQuery();
    }

    // DELETE command
    public void DeleteRobotCommand(int id)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand(
            "DELETE FROM robotcommand WHERE id=@id", conn);

        cmd.Parameters.AddWithValue("id", id);

        cmd.ExecuteNonQuery();
    }
}