using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace RuntimeContracts.Analyzer.Test
{
    public static class CodeFixTestExtensions
    {
        public static TTest WithoutGeneratedCodeVerification<TTest>(this TTest test)
            where TTest : CodeFixTest<MSTestVerifier>
        {
            test.Exclusions &= ~AnalysisExclusions.GeneratedCode;
            return test;
        }
    }
}
