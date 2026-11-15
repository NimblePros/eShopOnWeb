using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PublicApiIntegrationTests.Helpers;

/// <summary>
/// Asserts that a serialized user only carries <c>UserDto</c> fields, so Identity internals
/// such as PasswordHash or SecurityStamp can't leak through an API response.
/// </summary>
public static class UserJsonAssert
{
    private static readonly HashSet<string> s_allowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "userName", "email", "emailConfirmed", "phoneNumber", "phoneNumberConfirmed", "twoFactorEnabled", "lockoutEnd"
    };

    public static void HasOnlyUserDtoFields(JsonElement user)
    {
        var unexpected = user.EnumerateObject()
            .Select(p => p.Name)
            .Where(name => !s_allowedFields.Contains(name))
            .ToList();

        Assert.AreEqual(0, unexpected.Count, $"User JSON exposes unexpected fields: {string.Join(", ", unexpected)}");
    }
}
