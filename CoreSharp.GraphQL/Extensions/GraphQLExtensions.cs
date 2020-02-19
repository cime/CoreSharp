using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;

namespace CoreSharp.GraphQL
{
    public static class GraphQLExtensions
    {
        public static readonly string PermissionsKey = "Permissions";

        public static bool RequiresPermissions(this IProvideMetadata type)
        {
            var fieldPermissions = type.GetMetadata<IEnumerable<string>>(PermissionsKey);

            return fieldPermissions != null && fieldPermissions.Any();
        }

        public static bool CanAccess(this IProvideMetadata type, params string[] permissions)
        {
            if (permissions == null || !permissions.Any())
            {
                return false;
            }

            var fieldPermissions = type.GetMetadata<IEnumerable<string>>(PermissionsKey)?.ToList();

            return fieldPermissions == null || !fieldPermissions.Any() || fieldPermissions.Any(x => permissions.Contains(x));
        }

        public static void RequirePermission(this IProvideMetadata type, string permission)
        {
            var permissions = type.GetMetadata<List<string>>(PermissionsKey);

            if (permissions == null)
            {
                permissions = new List<string>();
                type.Metadata[PermissionsKey] = permissions;
            }

            permissions.Add(permission);
        }

        public static FieldBuilder<TSourceType, TReturnType> RequirePermission<TSourceType, TReturnType>(
            this FieldBuilder<TSourceType, TReturnType> builder, string permission)
        {
            builder.FieldType.RequirePermission(permission);
            return builder;
        }
    }
}
