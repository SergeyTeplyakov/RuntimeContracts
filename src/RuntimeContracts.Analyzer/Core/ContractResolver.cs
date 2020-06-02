using Microsoft.CodeAnalysis;

#nullable enable

namespace RuntimeContracts.Analyzer.Core
{
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
            _standardContractTypeSymbol = compilation.GetTypeByMetadataName("System.Diagnostics.Contracts.Contract");
            _runtimeContractTypeSymbol = compilation.GetTypeByMetadataName(FluentContractNames.FluentContractFullName);
            _fluentContractExtensionsTypeSymbol = compilation.GetTypeByMetadataName(FluentContractNames.FluentExtensionsFullName);
        }

        /// <summary>
        /// Returns true if a given <paramref name="candidate"/> type is <code>System.Diagnostics.Contracts.Contract</code>.
        /// </summary>
        public bool IsStandardContractType(INamedTypeSymbol candidate) => candidate.Equals(_standardContractTypeSymbol);

        /// <summary>
        /// Returns true if a given <paramref name="invocationExpression"/> invokes member
        /// of a System.Diagnostics.ContractsLight.Contract class.
        /// </summary>
        public bool GetContractInvocation(IMethodSymbol invokedMethod, out ContractMethodNames contract)
        {
            contract = default;
            if (!invokedMethod.ContainingType.Equals(_runtimeContractTypeSymbol))
            {
                return false;
            }

            contract = ParseContractMethodName(invokedMethod.Name);
            return true;
        }

        /// <summary>
        /// Returns true if a given <paramref name="method"/> invokes member
        /// of a System.Diagnostics.ContractsLight.Contract class.
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
                method.ContainingType.Equals(_fluentContractExtensionsTypeSymbol);
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
            if (memberSymbol == null || !memberSymbol.ContainingType.Equals(contractTypeSymbol))
            {
                // This is not Contract.
                return false;
            }

            return true;
        }
    }
}
