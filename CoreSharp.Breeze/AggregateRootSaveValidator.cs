using System;
using System.Collections.Generic;
using System.Linq;
using Breeze.NHibernate;
using CoreSharp.DataAccess;

namespace CoreSharp.Breeze
{
    public class AggregateRootSaveValidator : ModelSaveValidator
    {
        protected override void ValidateRootNodes(Type rootModelType, List<GraphNode> rootNodes)
        {
            if (rootNodes.Count == 1)
            {
                base.ValidateRootNodes(rootModelType, rootNodes);
                return;
            }

            // Try to find the root node by IAggregate
            var aggregateRoots = new HashSet<object>();
            for (var i = 0; i < rootNodes.Count;)
            {
                var entity = rootNodes[i].EntityInfo.Entity;
                var aggregateRoot = entity is IAggregate aggregate ? aggregate.GetAggregateRoot() : entity;
                if (aggregateRoot != entity)
                {
                    rootNodes.RemoveAt(i);
                }
                else
                {
                    i++;
                }

                aggregateRoots.Add(aggregateRoot);
            }

            if (aggregateRoots.Count > 1)
            {
                throw new InvalidOperationException("Multiple aggregate roots found.");
            }

            var rootNode = rootNodes.FirstOrDefault();
            if (rootNode == null)
            {
                throw new InvalidOperationException("The aggregate root is not present in the dependency graph.");
            }

            if (rootNode.EntityInfo.EntityType != rootModelType)
            {
                throw new InvalidOperationException("The found aggregate root type is not valid.");
            }
        }
    }
}
