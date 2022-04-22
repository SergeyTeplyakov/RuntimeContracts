using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace RuntimeContracts.Analyzer.Test;

public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static Task RunWithFixer(string test, string fixedTest)
    {
        var t = new Test
        {
            TestState = { Sources = { test } },
            LanguageVersion = LanguageVersion.CSharp8,
            FixedState = { Sources = { fixedTest } },
        };
            
        return t.WithoutGeneratedCodeVerification().RunAsync();
    }

    public static Task RunBatchWithFixer(string test, string fixedCode, string batchFixedCode)
    {
        var t = new Test
        {
            TestState = { Sources = { test } },
            LanguageVersion = LanguageVersion.CSharp8,
            FixedCode = fixedCode,
            BatchFixedCode = batchFixedCode,
        };

        return t.WithoutGeneratedCodeVerification().RunAsync();
    }

    public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, MSTestVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = AdditionalMetadataReferences.DefaultReferenceAssemblies;

            SolutionTransforms.Add((solution, projectId) =>
            {
                var project = solution.GetProject(projectId);
                var parseOptions = (CSharpParseOptions)project.ParseOptions;
                solution = solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(LanguageVersion));

                if (IncludeContracts)
                {
                    solution = solution.AddMetadataReference(projectId, AdditionalMetadataReferences.RuntimeContracts);
                }

                return solution;
            });
        }

        public bool IncludeContracts { get; set; } = true;

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp7_1;
    }
}