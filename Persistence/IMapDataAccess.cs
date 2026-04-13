using robot_controller_api;

namespace robot_controller_api.Persistence;
using robot_controller_api.Models;

public interface IMapDataAccess
{
    List<Map> GetMaps();

    void AddMap(Map map);

    void UpdateMap(int id, Map updatedMap);

    void DeleteMap(int id);
}