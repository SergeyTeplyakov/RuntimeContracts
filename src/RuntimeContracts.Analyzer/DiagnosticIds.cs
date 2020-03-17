#nullable enable

namespace RuntimeContracts.Analyzer
{
    public class DiagnosticIds
    {
        /// <see cref="DoNotUseStandardContractAnalyzer" />
        public const string DoNotUseStandardContractId = "RA001";

        // No longer needed when Fluent API is used
        // public const string DoNotUseComputedStringId = "RA002";

        // No longer needed when Fluent API is used
        // public const string UseSimplifiedNullCheckId = "RA003";

        /// <see cref="ProvideMessageAnalyzer" />
        public const string ProvideMessageId = "RA004";

        /// <see cref="FluentAssertionResultIsNotObserved" />
        public const string FluentAssertionResultIsNotObserved = "RA005";

        /// <see cref="UseFluentContractsAnalyzer"/>
        public const string UseFluentContractsId = "RA006";
    }
}
