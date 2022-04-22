using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using System;
using System.Diagnostics.ContractsLight;

namespace RuntimeContracts.Analyzer.Test;

internal static class AdditionalMetadataReferences
{
    private static readonly Lazy<MetadataReference> LazyRuntimeContracts = new Lazy<MetadataReference>(
        () => MetadataReference.CreateFromFile(typeof(Contract).Assembly.Location));

#if NETCOREAPP
    public static readonly ReferenceAssemblies DefaultReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp21;
#else
        public static readonly ReferenceAssemblies DefaultReferenceAssemblies = ReferenceAssemblies.NetFramework.Net472.Default;
#endif

    public static MetadataReference RuntimeContracts => LazyRuntimeContracts.Value;
}