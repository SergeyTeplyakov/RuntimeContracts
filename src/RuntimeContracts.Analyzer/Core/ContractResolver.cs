using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RuntimeContracts.Analyzer.Utilities;

namespace RuntimeContracts.Analyzer.Core
{
    [Flags]
    public enum ContractMethodNames
    {
        None,
        Assert = 1 << 0,
        Assume = 1 << 1,
        EndContractBlock = 1 << 2,
        Ensures = 1 << 3,
        EnsuresOnThrow = 1 << 4,
        Exists = 1 << 5,
        ForAll = 1 << 6,
        Invariant = 1 << 7,
        OldValue = 1 << 8,
        Requires = 1 << 9,
        Result = 1 << 10,
        ValueAtReturn = 1 << 11,
        All = (1 << 12) - 1
    }

    /// <summary>
    /// Helper class that resolves all invocations to <see cref="System.Diagnostics.ContractsLight.Contract"/> class
    /// with all utilities useful to extract invocation for different method invocation.
    /// </summary>
    public sealed class ContractResolver
    {
        private readonly SemanticModel _semanticModel;

        private readonly INamedTypeSymbol _standardContractTypeSymbol;
        private readonly INamedTypeSymbol _contractTypeSymbol;

        /// <nodoc />
        public ContractResolver(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;

            _standardContractTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Diagnostics.Contracts.Contract");
            _contractTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Diagnostics.ContractsLight.Contract");
        }

        /// <summary>
        /// Returns true if a given <paramref name="candidate"/> type is <code>System.Diagnostics.Contracts.Contract</code>.
        /// </summary>
        public bool IsStandardContractType(INamedTypeSymbol candidate) => candidate.Equals(_standardContractTypeSymbol);

        /// <summary>
        /// Returns true if a given <paramref name="invocationExpression"/> invokes member
        /// of a <see cref="System.Diagnostics.Contracts.Contract"/> class.
        /// </summary>
        //[System.Diagnostics.Contracts.Pure]
        public bool IsStandardContractInvocation(
            InvocationExpressionSyntax invocationExpression,
            ContractMethodNames allowedMethodNames = ContractMethodNames.All)
        {
            return IsContractInvocation(invocationExpression, allowedMethodNames, _standardContractTypeSymbol);
        }

        /// <summary>
        /// Returns true if a given <paramref name="invocationExpression"/> invokes member
        /// of a <see cref="System.Diagnostics.ContractsLight.Contract"/> class.
        /// </summary>
        //[System.Diagnostics.Contracts.Pure]
        public bool IsContractInvocation(
            InvocationExpressionSyntax invocationExpression,
            ContractMethodNames allowedMethodNames = ContractMethodNames.All)
        {
            return IsContractInvocation(invocationExpression, allowedMethodNames, _contractTypeSymbol);
        }
        
        private static ContractMethodNames ParseContractMethodName(string methodName)
        {
            switch (methodName)
            {
                // Names for both standard contract type and the lightweight one are the same.
                case "Assert":
                    return ContractMethodNames.Assert;
                case "Assume":
                    return ContractMethodNames.Assume;
                case "EndContractBlock":
                    return ContractMethodNames.EndContractBlock;
                case "Ensures":
                    return ContractMethodNames.Ensures;
                case "EnsuresOnThrow":
                    return ContractMethodNames.EnsuresOnThrow;
                case "Exists":
                    return ContractMethodNames.Exists;
                case "ForAll":
                    return ContractMethodNames.ForAll;
                case "Invariant":
                    return ContractMethodNames.Invariant;
                case "OldValue":
                    return ContractMethodNames.OldValue;
                case "Requires":
                    return ContractMethodNames.Requires;
                case "Result":
                    return ContractMethodNames.Result;
                case "ValueAtReturn":
                    return ContractMethodNames.ValueAtReturn;
                default:
                    return ContractMethodNames.None;
            }
        }

        private bool IsContractInvocation(
            InvocationExpressionSyntax invocationExpression,
            ContractMethodNames allowedMethodNames,
            INamedTypeSymbol contractTypeSymbol)
        {
            var memberAccess =
                invocationExpression
                .Expression.As(x => x as MemberAccessExpressionSyntax);

            if ((ParseContractMethodName(memberAccess?.Name.Identifier.Text) & allowedMethodNames) == ContractMethodNames.None)
            {
                return false;
            }

            var memberSymbol =
                memberAccess
                ?.As(x => _semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol);

            // TODO: ToMetadataFullName() on every call is probably somewhat expensive
            if (memberSymbol == null || !memberSymbol.ContainingType.Equals(contractTypeSymbol))
            {
                // This is not Contract.
                return false;
            }

            return true;
        }
    }
}
