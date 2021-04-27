using System;
using System.Collections.Generic;
using CoreSharp.GraphQL.Configuration;
using GraphQL;
using GraphQL.Language.AST;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace CoreSharp.GraphQL
{
    public class AutoInputGraphType<TSourceType> : AutoGraphType<TSourceType>, IInputObjectGraphType
        where TSourceType : class
    {
        public AutoInputGraphType(ISchema schema, IGraphQLConfiguration configuration, IFieldResolver fieldResolver) : base(schema, configuration, fieldResolver)
        {
        }

        protected override string GetTypeName(Type type)
        {
            return $"{base.GetTypeName(type)}Input";
        }

        /// <summary>
        /// Converts a supplied dictionary of keys and values to an object.
        /// The default implementation uses <see cref="ObjectExtensions.ToObject"/> to convert the
        /// supplied field values into an object of type <typeparamref name="TSourceType"/>.
        /// Overriding this method allows for customizing the deserialization process of input objects,
        /// much like a field resolver does for output objects. For example, you can set some 'computed'
        /// properties for your input object which were not passed in the GraphQL request.
        /// </summary>
        public virtual object ParseDictionary(IDictionary<string, object> value)
        {
            if (value == null)
                return null;

            // for InputObjectGraphType just return the dictionary
            if (typeof(TSourceType) == typeof(object))
                return value;

            // for InputObjectGraphType<TSourceType>, convert to TSourceType via ToObject.
            return value.ToObject(typeof(TSourceType), this);
        }

        /// <inheritdoc/>
        public virtual bool IsValidDefault(object value) => value is TSourceType;

        /// <summary>
        /// Converts a value to an AST representation. This is necessary for introspection queries
        /// to return the default value for fields of this input object type. This method may throw an exception
        /// or return <see langword="null"/> for a failed conversion.
        /// <br/><br/>
        /// The default implementation always throws an exception. It is recommended that this method be
        /// overridden to support introspection of fields of this type that have default values. This method
        /// is not otherwise needed to be implemented.
        /// </summary>
        public virtual IValue ToAST(object value)
        {
            throw new System.NotImplementedException($"Please override the '{nameof(ToAST)}' method of the '{GetType().Name}' scalar to support this operation.");
        }
    }
}
