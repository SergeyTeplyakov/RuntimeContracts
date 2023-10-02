#nullable enable

namespace RuntimeContracts.Analyzer.Core;

public static class FluentContractNames
{
    // Can't use 'nameof' here because this project does not reference the contracts assembly.
    public static readonly string CheckMethodName = "Check";
    public static readonly string CheckDebugMethodName = "CheckDebug";
        
    public static readonly string FluentContractFullName = "System.Diagnostics.ContractsLight.Contract";

    public static readonly string ContractClassName = "Contract";

    public static readonly string FluentExtensionsFullName = "System.Diagnostics.ContractsLight.ContractFluentExtensions";

    public static readonly string Requires = "Requires";

    public static readonly string Assert = "Assert";
}