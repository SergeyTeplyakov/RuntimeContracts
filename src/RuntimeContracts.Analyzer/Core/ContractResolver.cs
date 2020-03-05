using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RuntimeContracts.Analyzer.Utilities;

#nullable enable

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
        RequiresNotNull = 1 << 12,
        AssertNotNull = 1 << 13,
        RequiresNotNullOrWhiteSpace = 1 << 14,
        AssertNotNullOrWhiteSpace = 1 << 15,
        RequiresNotNullOrEmpty = 1 << 16,
        AssertNotNullOrEmpty = 1 << 17,
        All = (1 << 18) - 1,
        AllAsserts = Assert | AssertNotNull | AssertNotNullOrEmpty | AssertNotNullOrWhiteSpace,
        AllRequires = Requires | RequiresNotNull | RequiresNotNullOrEmpty | RequiresNotNullOrWhiteSpace,
    }

    /// <summary>
    /// Helper class that resolves all invocations to <code>System.Diagnostics.ContractsLight.Contract</code> class
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
        public bool IsStandardContractInvocation(
            InvocationExpressionSyntax invocationExpression,
            ContractMethodNames allowedMethodNames = ContractMethodNames.All)
        {
            return IsContractInvocation(invocationExpression, allowedMethodNames, _standardContractTypeSymbol);
        }

        /// <summary>
        /// Returns true if a given <paramref name="invocationExpression"/> invokes member
        /// of a System.Diagnostics.ContractsLight.Contract class.
        /// </summary>
        public bool IsContractInvocation(
            InvocationExpressionSyntax invocationExpression,
            ContractMethodNames allowedMethodNames = ContractMethodNames.All)
        {
            return IsContractInvocation(invocationExpression, allowedMethodNames, _contractTypeSymbol);
        }

        /// <summary>
        /// Returns true if a given <paramref name="method"/> invokes member
        /// of a System.Diagnostics.ContractsLight.Contract class.
        /// </summary>
        public bool IsContractInvocation(
            IMethodSymbol method,
            ContractMethodNames allowedMethodNames = ContractMethodNames.All)
        {
            return IsContractInvocation(method, allowedMethodNames, _contractTypeSymbol);
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

        public static ContractMethodNames ParseContractMethodName(string? methodName)
        {
            return methodName switch
            {
                // Names for both standard contract type and the lightweight one are the same.
                "Assert" => ContractMethodNames.Assert,
                "AssertNotNull" => ContractMethodNames.AssertNotNull,
                "AssertNotNullOrEmpty" => ContractMethodNames.AssertNotNullOrEmpty,
                "AssertNotNullOrWhiteSpace" => ContractMethodNames.AssertNotNullOrWhiteSpace,
                "Assume" => ContractMethodNames.Assume,
                "EndContractBlock" => ContractMethodNames.EndContractBlock,
                "Ensures" => ContractMethodNames.Ensures,
                "EnsuresOnThrow" => ContractMethodNames.EnsuresOnThrow,
                "Exists" => ContractMethodNames.Exists,
                "ForAll" => ContractMethodNames.ForAll,
                "Invariant" => ContractMethodNames.Invariant,
                "OldValue" => ContractMethodNames.OldValue,
                "Requires" => ContractMethodNames.Requires,
                "RequiresNotNull" => ContractMethodNames.RequiresNotNull,
                "RequiresNotNullOrEmpty" => ContractMethodNames.RequiresNotNullOrEmpty,
                "RequiresNotNullOrWhiteSpace" => ContractMethodNames.RequiresNotNullOrWhiteSpace,
                "Result" => ContractMethodNames.Result,
                "ValueAtReturn" => ContractMethodNames.ValueAtReturn,
                _ => ContractMethodNames.None,
            };
        }

        private bool IsContractInvocation(
            InvocationExpressionSyntax invocationExpression,
            ContractMethodNames allowedMethodNames,
            INamedTypeSymbol contractTypeSymbol)
        {
            MemberAccessExpressionSyntax? memberAccess =
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
            if (memberSymbol == null || !memberSymbol.ContainingType.Equals(contractTypeSymbol))
            {
                // This is not Contract.
                return false;
            }

            return true;
        }
    }
}
