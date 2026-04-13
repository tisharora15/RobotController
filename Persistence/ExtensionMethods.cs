using FastMember;
using Npgsql;

namespace robot_controller_api.Persistence;

public static class ExtensionMethods
{
    public static void MapTo<T>(this NpgsqlDataReader dr, T entity)
    {
        var accessor = TypeAccessor.Create(entity.GetType());

        var members = accessor.GetMembers()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < dr.FieldCount; i++)
        {
            var name = dr.GetName(i);

            if (members.Contains(name))
            {
                accessor[entity, name] =
                    dr.IsDBNull(i) ? null : dr.GetValue(i);
            }
        }
    }
}