using System;

namespace robot_controller_api.Models;

public partial class Map
{
    public int Id { get; set; }

    public int Columns { get; set; }

    public int Rows { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool? IsSquare { get; set; }

    // Required for ORM (EF + FastMember)
    public Map()
    {
    }

    public Map(
        int id,
        int columns,
        int rows,
        string name,
        DateTime createdDate,
        DateTime modifiedDate,
        string? description = null)
    {
        Id = id;
        Columns = columns;
        Rows = rows;
        Name = name;
        CreatedDate = createdDate;
        ModifiedDate = modifiedDate;
        Description = description;
    }
}