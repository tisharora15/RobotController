using Microsoft.AspNetCore.Mvc;
using robot_controller_api;
using robot_controller_api.Persistence;

namespace robot_controller_api.Controllers;
using robot_controller_api.Models;

[ApiController]
[Route("api/maps")]
public class MapsController : ControllerBase
{
    private readonly IMapDataAccess _mapRepo;

    public MapsController(IMapDataAccess mapRepo)
    {
        _mapRepo = mapRepo;
    }

    // GET api/maps
    [HttpGet]
    public IEnumerable<Map> GetAllMaps()
    {
        return _mapRepo.GetMaps();
    }

    // GET api/maps/square
    [HttpGet("square")]
    public IEnumerable<Map> GetSquareMaps()
    {
        return _mapRepo
            .GetMaps()
            .Where(m => m.Columns == m.Rows);
    }

    // GET api/maps/{id}
    [HttpGet("{id}", Name = "GetMap")]
    public IActionResult GetMapById(int id)
    {
        var map = _mapRepo
            .GetMaps()
            .FirstOrDefault(m => m.Id == id);

        if (map == null)
            return NotFound();

        return Ok(map);
    }

    // POST api/maps
    [HttpPost]
    public IActionResult AddMap(Map newMap)
    {
        if (newMap == null)
            return BadRequest();

        var map = new Map(
            0,
            newMap.Columns,
            newMap.Rows,
            newMap.Name,
            DateTime.Now,
            DateTime.Now,
            newMap.Description
        );

        _mapRepo.AddMap(map);

        return Ok(map);
    }

    // PUT api/maps/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateMap(int id, Map updatedMap)
    {
        if (updatedMap == null)
            return BadRequest();

        _mapRepo.UpdateMap(id, updatedMap);

        return NoContent();
    }

    // DELETE api/maps/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteMap(int id)
    {
        _mapRepo.DeleteMap(id);

        return NoContent();
    }

    // GET api/maps/{id}/{x}-{y}
    [HttpGet("{id}/{x}-{y}")]
    public IActionResult CheckCoordinate(int id, int x, int y)
    {
        if (x < 0 || y < 0)
            return BadRequest("Coordinates cannot be negative.");

        var map = _mapRepo
            .GetMaps()
            .FirstOrDefault(m => m.Id == id);

        if (map == null)
            return NotFound();

        bool isOnMap = x < map.Columns && y < map.Rows;

        return Ok(isOnMap);
    }
}
