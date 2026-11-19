using System.Reflection;
using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Microsoft.eShopWeb.UnitTests.Builders;

public static class EntityIdExtensions
{
    private static readonly PropertyInfo s_idProperty = typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!;

    /// <summary>
    /// Gives an entity the Id the database would have assigned. <see cref="BaseEntity.Id"/> has a
    /// protected setter, so tests that need persisted-looking entities set it through reflection
    /// rather than substituting the entity.
    /// </summary>
    public static T WithId<T>(this T entity, int id) where T : BaseEntity
    {
        s_idProperty.SetValue(entity, id);
        return entity;
    }
}
