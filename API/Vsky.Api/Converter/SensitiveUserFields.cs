using System;
using System.Linq;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Identity;
using Vsky.Api.Models;

namespace Vsky.Api.Converter
{
    /// <summary>
    /// Keeps credentials out of every API response. Many entities include their CreatedBy or UpdatedBy
    /// user, so this is done once for the whole API rather than in each controller. It only affects
    /// writing JSON: request bodies, such as a new user's password, still bind as before.
    /// </summary>
    public static class SensitiveUserFields
    {
        private static readonly string[] UserFields = { nameof(IdentityUser.PasswordHash), nameof(IdentityUser.SecurityStamp) };
        private static readonly string[] UserModelFields = { nameof(UserModel.Password) };

        public static void Hide(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object)
                return;

            if (typeof(IdentityUser<string>).IsAssignableFrom(typeInfo.Type))
                HideFields(typeInfo, UserFields);
            else if (typeof(UserModel).IsAssignableFrom(typeInfo.Type))
                HideFields(typeInfo, UserModelFields);
        }

        private static void HideFields(JsonTypeInfo typeInfo, string[] fields)
        {
            foreach (var property in typeInfo.Properties)
            {
                if (fields.Any(field => field.Equals(property.Name, StringComparison.OrdinalIgnoreCase)))
                    property.ShouldSerialize = (_, _) => false;
            }
        }
    }
}
