using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public class MapEF : IMapDataAccess
{
    private readonly RobotContext _context;

    public MapEF(RobotContext context)
    {
        _context = context;
    }

    public List<Map> GetMaps()
    {
        return _context.Maps.ToList();
    }

    public void AddMap(Map map)
    {
        _context.Maps.Add(map);
        _context.SaveChanges();
    }

    public void UpdateMap(int id, Map updatedMap)
    {
        var map = _context.Maps.FirstOrDefault(m => m.Id == id);

        if (map == null) return;

        map.Columns = updatedMap.Columns;
        map.Rows = updatedMap.Rows;
        map.Name = updatedMap.Name;
        map.Description = updatedMap.Description;
        map.ModifiedDate = DateTime.Now;

        _context.SaveChanges();
    }

    public void DeleteMap(int id)
    {
        var map = _context.Maps.FirstOrDefault(m => m.Id == id);

        if (map != null)
        {
            _context.Maps.Remove(map);
            _context.SaveChanges();
        }
    }
}