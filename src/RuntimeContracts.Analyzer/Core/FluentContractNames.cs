#nullable enable

namespace RuntimeContracts.Analyzer.Core
{
    public static class FluentContractNames
    {
        // Can't use 'nameof' here because this project does not reference the contracts assembly.
        public static readonly string CheckMethodName = "IsTrue";

        public static readonly string FluentContractFullName = "System.Diagnostics.FluentContracts.Contract";

        public static readonly string FluentContractsNamespace = "System.Diagnostics.FluentContracts";

        public static readonly string OldRuntimeContractsNamespace = "System.Diagnostics.ContractsLight";

        public static readonly string ContractClassName = "Contract";

        public static readonly string FluentExtensionsFullName = "System.Diagnostics.FluentContracts.ContractFluentExtensions";

        public static readonly string Requires = "Requires";

        public static readonly string Assert = "Assert";
    }
}
