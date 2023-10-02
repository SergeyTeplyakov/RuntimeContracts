using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Shared.Extensions;

public static class ISymbolExtensions
{
    public static string ToNameDisplayString(this ISymbol symbol)
    {
        return symbol.ToDisplayString(NameFormat);
    }

    /// <summary>
    /// Standard format for displaying to the user.
    /// </summary>
    /// <remarks>
    /// No return type.
    /// </remarks>
    public static readonly SymbolDisplayFormat NameFormat =
        new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters |
                             SymbolDisplayGenericsOptions.IncludeVariance,
            memberOptions: SymbolDisplayMemberOptions.IncludeParameters |
                           SymbolDisplayMemberOptions.IncludeExplicitInterface,
            parameterOptions:
            SymbolDisplayParameterOptions.IncludeParamsRefOut |
            SymbolDisplayParameterOptions.IncludeExtensionThis |
            SymbolDisplayParameterOptions.IncludeType |
            SymbolDisplayParameterOptions.IncludeName,
            miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public static ImmutableArray<ITypeSymbol> GetTypeArguments(this ISymbol? symbol)
    {
        return symbol switch
        {
            IMethodSymbol m => m.TypeArguments,
            INamedTypeSymbol nt => nt.TypeArguments,
            _ => ImmutableArray.Create<ITypeSymbol>()
        };
    }

    public static bool IsPointerType([NotNullWhen(returnValue: true)] this ISymbol? symbol)
    {
        return symbol is IPointerTypeSymbol;
    }

    public static bool IsErrorType([NotNullWhen(returnValue: true)] this ISymbol? symbol)
        => (symbol as ITypeSymbol)?.TypeKind == TypeKind.Error;

    public static bool IsModuleType([NotNullWhen(returnValue: true)] this ISymbol? symbol)
    {
        return (symbol as ITypeSymbol)?.IsModuleType() == true;
    }

    public static bool IsInterfaceType([NotNullWhen(returnValue: true)] this ISymbol? symbol)
    {
        return (symbol as ITypeSymbol)?.IsInterfaceType() == true;
    }

    public static bool IsArrayType([NotNullWhen(returnValue: true)] this ISymbol? symbol)
    {
        return symbol?.Kind == SymbolKind.ArrayType;
    }

    public static bool IsTupleType([NotNullWhen(returnValue: true)] this ISymbol? symbol)
    {
        return (symbol as ITypeSymbol)?.IsTupleType ?? false;
    }

    public static bool IsAnonymousFunction([NotNullWhen(returnValue: true)] this ISymbol? symbol)
    {
        return (symbol as IMethodSymbol)?.MethodKind == MethodKind.AnonymousFunction;
    }
}