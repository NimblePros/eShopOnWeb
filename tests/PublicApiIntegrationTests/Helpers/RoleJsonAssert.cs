using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PublicApiIntegrationTests.Helpers;

/// <summary>
/// Asserts that a serialized role only carries <c>RoleDto</c> fields, so Identity internals
/// such as NormalizedName or ConcurrencyStamp can't leak through an API response.
/// </summary>
public static class RoleJsonAssert
{
    private static readonly HashSet<string> s_allowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "name"
    };

    public static void HasOnlyRoleDtoFields(JsonElement role)
    {
        var unexpected = role.EnumerateObject()
            .Select(p => p.Name)
            .Where(name => !s_allowedFields.Contains(name))
            .ToList();

        Assert.AreEqual(0, unexpected.Count, $"Role JSON exposes unexpected fields: {string.Join(", ", unexpected)}");
        Assert.IsTrue(role.TryGetProperty("id", out _), "Role JSON is missing id");
        Assert.IsTrue(role.TryGetProperty("name", out _), "Role JSON is missing name");
    }
}
