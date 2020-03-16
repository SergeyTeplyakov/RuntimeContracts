#nullable enable

namespace RuntimeContracts.Analyzer
{
    public class DiagnosticIds
    {
        public const string DoNotUseStandardContractId = "RA001";

        public const string DoNotUseComputedStringId = "RA002";

        public const string UseSimplifiedNullCheckId = "RA003";

        public const string ProvideMessageId = "RA004";

        public const string FluentAssertionResultIsNotObserved = "RA005";

        public const string FluentAssertionCanBeUsed = "RA006";

        public const string UseFluentContractsId = "RA006";

        public const string RemovePostconditionId = "RA007";

        public const string UseRuntimeContractsAlias = "RA008";
    }
}
