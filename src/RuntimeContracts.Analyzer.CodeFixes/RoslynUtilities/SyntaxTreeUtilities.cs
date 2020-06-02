using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RuntimeContracts.Analyzer.Utilities
{
    public static class SyntaxTreeUtilities
    {
        public static SyntaxNode AddOrReplaceContractNamespaceUsings(SyntaxNode root)
        {
            return ReplaceNamespaceUsings(root,
                originalNamespace: "System.Diagnostics.Contracts",
                newNamespace: "System.Diagnostics.ContractsLight");
        }

        public static SyntaxNode AddNamespaceUsingsIfNeeded(SyntaxNode root, string namespaceName)
        {
            var newUsing =
                SyntaxFactory.UsingDirective(
                        SyntaxFactory.ParseName(namespaceName))
                    .NormalizeWhitespace()
                    .WithTrailingTrivia(SyntaxTriviaList.Create(SyntaxFactory.CarriageReturnLineFeed));

            // Or at the top level
            foreach (var rootNamespace in root.DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>().ToList())
            {
                if (FindIndex(rootNamespace.Usings, namespaceName, out _))
                {
                    return root;
                }
            }

            // Or at the top level
            var compilation = root.FindNode<CompilationUnitSyntax>();

            var newUsings = compilation.Usings.Add(newUsing);

            return root.ReplaceNode(compilation, compilation.WithUsings(newUsings));
        }
        
        public static SyntaxNode ReplaceNamespaceUsings(SyntaxNode root, string originalNamespace, string newNamespace)
        {
            // The method removes 'using System.Diagnostics.Contracts;' and renames it to 'using System.Diagnostics.ContractsLight;'.
            var newUsing =
                SyntaxFactory.UsingDirective(
                        SyntaxFactory.ParseName(newNamespace))
                    .NormalizeWhitespace()
                    .WithTrailingTrivia(SyntaxTriviaList.Create(SyntaxFactory.CarriageReturnLineFeed));

            var replacement = new Dictionary<SyntaxNode, SyntaxNode>();
            
            // TODO: super naive approach
            foreach (var rootNamespace in root.DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>().ToList())
            {
                if (ReplaceInsideNamespace(rootNamespace, newUsing, originalNamespace, out var newNamespaceNode))
                {
                    replacement[rootNamespace] = newNamespaceNode;
                }
            }

            if (replacement.Count != 0)
            {
                return root.ReplaceNodes(replacement.Keys, (node, syntaxNode) => replacement[node]);
            }

            // Or at the top level
            var compilation = root.FindNode<CompilationUnitSyntax>();
            if (ReplaceForTopLevel(compilation, out var newCompilation))
            {
                return root.ReplaceNode(compilation, newCompilation);
            }

            var newUsings = compilation.Usings.Add(newUsing);

            return root.ReplaceNode(compilation, compilation.WithUsings(newUsings));

            // Local functions
            bool ReplaceForTopLevel(CompilationUnitSyntax compilationRoot, out CompilationUnitSyntax? newCompilationRoot)
            {
                var namespaceUsings = compilationRoot.Usings;
                if (FindIndex(compilationRoot.Usings, originalNamespace, out int namespaceIndex))
                {
                    newCompilationRoot = compilationRoot.WithUsings(namespaceUsings.Replace(namespaceUsings[namespaceIndex], newUsing));
                    return true;
                }

                newCompilationRoot = null;
                return false;
            }
        }

        private static bool ReplaceInsideNamespace(
            NamespaceDeclarationSyntax namespaceRoot, 
            UsingDirectiveSyntax newUsing,
            string existingContractNamespace,
            [NotNullWhen(true)] out NamespaceDeclarationSyntax? newNamespaceRoot)
        {
            var namespaceUsings = namespaceRoot.Usings;
            if (FindIndex(namespaceRoot.Usings, existingContractNamespace, out int namespaceIndex))
            {
                newNamespaceRoot = namespaceRoot.WithUsings(
                    namespaceUsings.Replace(namespaceUsings[namespaceIndex], newUsing));
                return true;
            }

            newNamespaceRoot = null;
            return false;
        }

        private static bool FindIndex(IEnumerable<UsingDirectiveSyntax> localUsings, string existingContractNamespace, out int localIndex)
        {
            return localUsings
                .FindIndex(p => p.Name.GetText().ToString() == existingContractNamespace, out localIndex);
        }

        public static TNode FindNode<TNode>(this SyntaxNode node)
        {
            return node.DescendantNodesAndSelf().OfType<TNode>().First();
        }

        public static bool FindIndex<T>(this IEnumerable<T> sequence, Func<T, bool> predicate, out int index)
        {
            int i = 0;
            foreach(var item in sequence)
            {
                if (predicate(item))
                {
                    index = i;
                    return true;
                }

                i++;
            }

            index = -1;
            return false;
        }
    }
}