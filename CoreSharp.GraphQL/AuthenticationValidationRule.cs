using System.Collections.Generic;
using CoreSharp.GraphQL.Extensions;
using GraphQL.Language.AST;
using GraphQL.Validation;

namespace CoreSharp.GraphQL
{
    public class AuthenticationValidationRule : IValidationRule
    {
        public INodeVisitor Validate(ValidationContext context)
        {
            var userContext = context.UserContext as IDictionary<string, object>;
            var authenticated = userContext.GetValueOrDefault("IsAuthenticated") as bool? == true;
            var claims = userContext.GetValueOrDefault("Claims") as IEnumerable<string>;

            return new EnterLeaveListener(_ =>
            {
                _.Match<Operation>(op =>
                {
                    if (op.OperationType == OperationType.Mutation && !authenticated)
                    {
                        context.ReportError(new ValidationError(
                            context.OriginalQuery,
                            "auth-required",
                            $"Authorization is required to access {op.Name}.",
                            op));
                    }
                });

                // this could leak info about hidden fields in error messages
                // it would be better to implement a filter on the schema so it
                // acts as if they just don't exist vs. an auth denied error
                // - filtering the schema is not currently supported
                _.Match<Field>(fieldAst =>
                {
                    var fieldDef = context.TypeInfo.GetFieldDef();
                    if (fieldDef?.RequiresPermissions() == true &&
                        (!authenticated || (claims == null || !fieldDef.CanAccess(claims))))
                    {
                        context.ReportError(new ValidationError(
                            context.OriginalQuery,
                            "auth-required",
                            $"You are not authorized to run this query.",
                            fieldAst));
                    }
                });
            });
        }
    }
}
