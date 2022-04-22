using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Testing;

namespace RuntimeContracts.Analyzer.Test;

public static partial class VisualBasicCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public class Test : VisualBasicCodeFixTest<TAnalyzer, TCodeFix, MSTestVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = AdditionalMetadataReferences.DefaultReferenceAssemblies;

            SolutionTransforms.Add((solution, projectId) =>
            {
                var project = solution.GetProject(projectId);
                var parseOptions = (VisualBasicParseOptions)project.ParseOptions;
                solution = solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(LanguageVersion));

                if (IncludeContracts)
                {
                    solution = solution.AddMetadataReference(projectId, AdditionalMetadataReferences.RuntimeContracts);
                }

                return solution;
            });
        }

        public bool IncludeContracts { get; set; } = true;

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.VisualBasic15_3;
    }
}