using System.Threading.Tasks;
using CoreSharp.NHibernate.Generator.Models;
using Microsoft.CodeAnalysis;

namespace CoreSharp.NHibernate.Generator
{
    internal interface IVisitor
    {
        Task<SyntaxNode> Visit(SyntaxNode node, Microsoft.CodeAnalysis.Document document, ProjectMetadata projectMetadata);
    }
}
