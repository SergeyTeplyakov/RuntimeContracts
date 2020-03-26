using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Shared.Extensions
{
    public static class INamedTypeSymbolExtensions
    {
        public static IEnumerable<ITypeSymbol> GetAllTypeArguments(this INamedTypeSymbol? symbol)
        {
            var stack = GetContainmentStack(symbol);
            return stack.SelectMany(n => n.TypeArguments);
        }

        private static Stack<INamedTypeSymbol> GetContainmentStack(INamedTypeSymbol? symbol)
        {
            var stack = new Stack<INamedTypeSymbol>();
            for (var current = symbol; current != null; current = current.ContainingType)
            {
                stack.Push(current);
            }

            return stack;
        }
    }
}