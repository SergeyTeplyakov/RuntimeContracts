#nullable enable

namespace RuntimeContracts.Analyzer
{
    public class Diagnostics
    {
        public const string NoAssertionMessageId = "RA004";

        // New contract analyzers.
        // Error when the result of Contract.Requires(x>0) is not observed
        // Error when Contract.Requiers(x > 0, "message") is used,
        // Fixers: use the fluent thing.
        // Use the fluent thing and generate message.
    }
}
