using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;

// ReSharper disable once CheckNamespace
namespace GraphQL.Types
{
    public static class ResolveFieldContextExtensions
    {
        public static IList<string> GetIncludes(this Field field)
        {
            return GetIncludesInternal(field.SelectionSet.Selections.OfType<Field>());
        }

        public static IList<string> GetIncludes(this ResolveFieldContext context)
        {
            return GetIncludesInternal(context.FieldAst.SelectionSet.Selections.OfType<Field>());
        }

        /// <summary>
        /// Returns Includes for Entity Framework
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="path"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        private static IList<string> GetIncludesInternal(IEnumerable<Field> fields, string path = "", IList<string> paths = null)
        {
            if (paths == null)
            {
                paths = new List<string>();
            }

            foreach (var field in fields.Where(x => x.SelectionSet?.Selections?.Any() ?? false))
            {
                paths.Add(path + field.Name.ToUpperFirstChar());
                if (field.SelectionSet?.Selections?.Any() ?? false)
                {
                    GetIncludesInternal(field.SelectionSet.Selections.OfType<Field>(), path + field.Name.ToUpperFirstChar() + ".", paths);
                }
            }

            return paths;
        }
    }
}
