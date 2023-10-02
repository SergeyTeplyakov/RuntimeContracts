using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

#nullable enable

namespace RuntimeContracts.Analyzer.Core;

/// <summary>
/// Helper class that resolves all invocations to <code>System.Diagnostics.ContractsLight.Contract</code> class
/// with all utilities useful to extract invocation for different method invocation.
/// </summary>
public sealed class ContractResolver
{
    private readonly INamedTypeSymbol _standardContractTypeSymbol;
    private readonly INamedTypeSymbol _runtimeContractTypeSymbol;
    private readonly INamedTypeSymbol _fluentContractExtensionsTypeSymbol;

    /// <nodoc />
    public ContractResolver(Compilation compilation)
    {
        _standardContractTypeSymbol = compilation.GetTypeByMetadataName("System.Diagnostics.Contracts.Contract")!;
        _runtimeContractTypeSymbol = compilation.GetTypeByMetadataName(FluentContractNames.FluentContractFullName)!;
        _fluentContractExtensionsTypeSymbol = compilation.GetTypeByMetadataName(FluentContractNames.FluentExtensionsFullName)!;
    }

    /// <summary>
    /// Returns true if a given <paramref name="candidate"/> type is <code>System.Diagnostics.Contracts.Contract</code>.
    /// </summary>
    public bool IsStandardContractType(INamedTypeSymbol candidate) => candidate.SymbolEquals(_standardContractTypeSymbol);

    /// <summary>
    /// Returns true if a given <paramref name="invokedMethod"/> is a method
    /// from System.Diagnostics.ContractsLight.Contract class.
    /// </summary>
    public bool GetContractInvocation(IMethodSymbol invokedMethod, out ContractMethodNames contract)
    {
        contract = default;
        if (!invokedMethod.ContainingType.SymbolEquals(_runtimeContractTypeSymbol))
        {
            return false;
        }

        contract = ParseContractMethodName(invokedMethod.Name);
        return true;
    }

    /// <summary>
    /// Returns true if a given <paramref name="method"/> is a method from System.Diagnostics.ContractsLight.Contract class.
    /// </summary>
    public bool IsContractInvocation(
        IMethodSymbol method,
        ContractMethodNames allowedMethodNames = ContractMethodNames.All)
    {
        return IsContractInvocation(method, allowedMethodNames, _runtimeContractTypeSymbol);
    }

    /// <summary>
    /// Returns true if a given <paramref name="method"/> invokes member
    /// of a System.Diagnostics.FluentContracts.Contract class.
    /// </summary>
    public bool IsFluentContractInvocation(
        IMethodSymbol method,
        ContractMethodNames allowedMethodNames = ContractMethodNames.AllFluentContracts)
    {
        return IsContractInvocation(method, allowedMethodNames, _runtimeContractTypeSymbol);
    }

    /// <summary>
    /// Returns true if a given <paramref name="method"/> is a special extension method that forces
    /// the contract check and throws an exception if the contract is violated.
    /// </summary>
    public bool IsFluentContractCheck(IMethodSymbol method)
    {
        return 
            (method.Name == FluentContractNames.Requires || method.Name == FluentContractNames.Assert) &&
            method.ContainingType.SymbolEquals(_fluentContractExtensionsTypeSymbol);
    }

    public bool TryParseFluentContractInvocation(IOperation? operation, out ContractMethodNames contractMethod, [NotNullWhen(true)]out ArgumentSyntax? condition, [NotNullWhen(true)] out ArgumentSyntax? message)
    {
        condition = message = null;
        contractMethod = default;
        if (operation is IConditionalAccessOperation conditionalAccess && 
            conditionalAccess.WhenNotNull is IInvocationOperation contractInvocation &&
            conditionalAccess.Operation is IInvocationOperation checkInvocation)
        {
                
            contractMethod = ParseContractMethodName(contractInvocation.TargetMethod.Name);

            if (checkInvocation.TargetMethod.Name == ContractMethodNames.CheckDebug.ToString())
            {
                contractMethod = contractMethod switch
                {
                    ContractMethodNames.Requires => ContractMethodNames.RequiresDebug,
                    ContractMethodNames.Assert => ContractMethodNames.AssertDebug,
                    _ => contractMethod,
                };
            }
                
            condition = (ArgumentSyntax)checkInvocation.Arguments[0].Syntax;
                
            // 'Requires' is an extension method and the second argument is the message.
            message = (ArgumentSyntax)contractInvocation.Arguments[1].Syntax;
            return true;
        }

        return false;
    }


    /// <summary>
    /// Returns true if a given <paramref name="invocationExpression"/> invokes member
    /// of a <see cref="System.Diagnostics.Contracts.Contract"/> class.
    /// </summary>
    public bool IsStandardContractInvocation(
        IMethodSymbol invocationExpression,
        ContractMethodNames allowedMethodNames = ContractMethodNames.All)
    {
        return IsContractInvocation(invocationExpression, allowedMethodNames, _standardContractTypeSymbol);
    }

    public static ContractMethodNames ParseContractMethodName(string? methodName, bool isDebug = false)
        => ContractMethodNamesExtensions.ParseContractMethodName(methodName);

    private bool IsContractInvocation(
        IMethodSymbol? memberSymbol,
        ContractMethodNames allowedMethodNames,
        INamedTypeSymbol contractTypeSymbol)
    {
        if ((ParseContractMethodName(memberSymbol?.Name) & allowedMethodNames) == ContractMethodNames.None)
        {
            return false;
        }

        // TODO: ToMetadataFullName() on every call is probably somewhat expensive
        if (memberSymbol == null || !memberSymbol.ContainingType.SymbolEquals(contractTypeSymbol))
        {
            // This is not Contract.
            return false;
        }

        return true;
    }
}