using GraphQL;
using GraphQL.Types;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoreSharp.GraphQL.Configuration;
using GraphQL.Resolvers;

namespace CoreSharp.GraphQL
{
    /// <summary>
    /// Allows you to automatically register the necessary fields for the specified type.
    /// Supports <see cref="DescriptionAttribute"/>, <see cref="ObsoleteAttribute"/>, <see cref="System.ComponentModel.DefaultValueAttribute"/> and <see cref="RequiredAttribute"/>.
    /// Also it can get descriptions for fields from the xml comments.
    /// </summary>
    /// <typeparam name="TSourceType"></typeparam>
    public class AutoObjectGraphType<TSourceType> : AutoGraphType<TSourceType>, IObjectGraphType
        where TSourceType : class
    {
        /// <inheritdoc/>
        public Func<object, bool> IsTypeOf { get; set; }

        /// <inheritdoc/>
        public void AddResolvedInterface(IInterfaceGraphType graphType)
        {
            if (graphType == null)
                throw new ArgumentNullException(nameof(graphType));

            _ = graphType.IsValidInterfaceFor(this, throwError: true);
            var addMethod = typeof(ResolvedInterfaces).GetMethod("Add");
            addMethod.Invoke(ResolvedInterfaces, new []{ graphType });
        }

        /// <inheritdoc/>
        public Interfaces Interfaces { get; } = new Interfaces();

        /// <inheritdoc/>
        public ResolvedInterfaces ResolvedInterfaces { get; } = new ResolvedInterfaces();

        /// <summary>
        /// Adds a GraphQL interface graph type to the list of GraphQL interfaces implemented by this graph type.
        /// </summary>
        public void Interface<TInterface>()
            where TInterface : IInterfaceGraphType
            => Interfaces.Add<TInterface>();

        /// <summary>
        /// Adds a GraphQL interface graph type to the list of GraphQL interfaces implemented by this graph type.
        /// </summary>
        public void Interface(Type type) => Interfaces.Add(type);

        protected override void AddInterface(Type type)
        {
            Interface(type);
        }

        public AutoObjectGraphType(ISchema schema, IGraphQLConfiguration configuration, IFieldResolver fieldResolver) : base(schema, configuration, fieldResolver)
        {
        }
    }
}
