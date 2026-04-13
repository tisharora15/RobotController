using Npgsql;
using robot_controller_api;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public class MapRepository :
    IMapDataAccess, IRepository
{
    private IRepository _repo => this;

    public List<Map> GetMaps()
    {
        var maps = _repo.ExecuteReader<Map>(
            "SELECT * FROM map");

        return maps;
    }

    public void AddMap(Map map)
    {
        var sql = @"INSERT INTO map
                    (columns, rows, name, description, createddate, modifieddate)
                    VALUES (@columns,@rows,@name,@description,@created,@modified)";

        var parameters = new NpgsqlParameter[]
        {
            new("columns", map.Columns),
            new("rows", map.Rows),
            new("name", map.Name),
            new("description", map.Description ?? (object)DBNull.Value),
            new("created", map.CreatedDate),
            new("modified", map.ModifiedDate)
        };

        _repo.ExecuteReader<Map>(sql, parameters);
    }

    public void UpdateMap(int id, Map updatedMap)
    {
        var sql = @"UPDATE map
                    SET columns=@columns,
                        rows=@rows,
                        name=@name,
                        description=@description,
                        modifieddate=current_timestamp
                    WHERE id=@id
                    RETURNING *;";

        var parameters = new NpgsqlParameter[]
        {
            new("id", id),
            new("columns", updatedMap.Columns),
            new("rows", updatedMap.Rows),
            new("name", updatedMap.Name),
            new("description", updatedMap.Description ?? (object)DBNull.Value)
        };

        _repo.ExecuteReader<Map>(sql, parameters);
    }

    public void DeleteMap(int id)
    {
        var parameters = new NpgsqlParameter[]
        {
            new("id", id)
        };

        _repo.ExecuteReader<Map>(
            "DELETE FROM map WHERE id=@id",
            parameters);
    }
}