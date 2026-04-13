using Npgsql;
using robot_controller_api;

namespace robot_controller_api.Persistence;
using robot_controller_api.Models;
public class MapADO : IMapDataAccess
{
    private const string CONNECTION_STRING =
        "Host=localhost;Username=postgres;Password=prime;Database=sit331";

    // GET all maps
    public List<Map> GetMaps()
    {
        var maps = new List<Map>();

        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT * FROM map", conn);
        using var dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            var map = new Map(
                (int)dr["id"],
                (int)dr["columns"],
                (int)dr["rows"],
                (string)dr["name"],
                (DateTime)dr["createddate"],
                (DateTime)dr["modifieddate"],
                dr["description"] as string
            );

            maps.Add(map);
        }

        return maps;
    }

    // INSERT map
    public void AddMap(Map map)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        var sql = @"INSERT INTO map
                   (columns, rows, name, description, createddate, modifieddate)
                   VALUES (@columns,@rows,@name,@description,@created,@modified)";

        using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("columns", map.Columns);
        cmd.Parameters.AddWithValue("rows", map.Rows);
        cmd.Parameters.AddWithValue("name", map.Name);
        cmd.Parameters.AddWithValue("description", (object?)map.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("created", map.CreatedDate);
        cmd.Parameters.AddWithValue("modified", map.ModifiedDate);

        cmd.ExecuteNonQuery();
    }

    // UPDATE map
    public void UpdateMap(int id, Map updatedMap)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        var sql = @"UPDATE map
                    SET columns = @columns,
                        rows = @rows,
                        name = @name,
                        description = @description,
                        modifieddate = @modified
                    WHERE id = @id";

        using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("columns", updatedMap.Columns);
        cmd.Parameters.AddWithValue("rows", updatedMap.Rows);
        cmd.Parameters.AddWithValue("name", updatedMap.Name);
        cmd.Parameters.AddWithValue("description", (object?)updatedMap.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("modified", DateTime.Now);

        cmd.ExecuteNonQuery();
    }

    // DELETE map
    public void DeleteMap(int id)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand(
            "DELETE FROM map WHERE id=@id", conn);

        cmd.Parameters.AddWithValue("id", id);

        cmd.ExecuteNonQuery();
    }
}