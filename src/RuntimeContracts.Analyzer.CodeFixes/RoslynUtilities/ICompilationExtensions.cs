using System;

namespace Microsoft.CodeAnalysis.Shared.Extensions;

public static class ICompilationExtensions
{
    public static INamedTypeSymbol? ExceptionType(this Compilation compilation)
        => compilation.GetTypeByMetadataName(typeof(Exception).FullName!);
}