using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.ContractsLight;
using System.Reflection;

namespace RuntimeContracts.Analyzer.Test
{
    internal static class AdditionalMetadataReferences
    {
        private static readonly Lazy<MetadataReference> LazyRuntimeContracts = new Lazy<MetadataReference>(
            () => MetadataReference.CreateFromFile(typeof(Contract).Assembly.Location));
        private static readonly Lazy<MetadataReference> LazySystemRuntime = new Lazy<MetadataReference>(
            () => MetadataReference.CreateFromFile(Assembly.Load("System.Runtime, Version=4.0.20.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location));

        public static MetadataReference RuntimeContracts => LazyRuntimeContracts.Value;
        public static MetadataReference SystemRuntime => LazySystemRuntime.Value;
    }
}
