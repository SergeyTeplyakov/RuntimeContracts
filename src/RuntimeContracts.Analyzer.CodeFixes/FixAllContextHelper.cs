﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.CodeAnalysis.CodeStyle;

/// <summary>
/// A helper that implements a custom batch mode that allows fixing more than one diagnostic at the same time.
/// </summary>
internal static class FixAllContextHelper
{
    public static async Task<ImmutableDictionary<Document, ImmutableArray<Diagnostic>>> GetDocumentDiagnosticsToFixAsync(
        FixAllContext fixAllContext)
    {
        var cancellationToken = fixAllContext.CancellationToken;

        var allDiagnostics = ImmutableArray<Diagnostic>.Empty;
        var projectsToFix = ImmutableArray<Project>.Empty;

        var document = fixAllContext.Document;
        var project = fixAllContext.Project;

        switch (fixAllContext.Scope)
        {
            case FixAllScope.Document:
                if (document != null)
                {
                    var documentDiagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(document).ConfigureAwait(false);
                    return ImmutableDictionary<Document, ImmutableArray<Diagnostic>>.Empty.SetItem(document, documentDiagnostics);
                }

                break;

            case FixAllScope.Project:
                projectsToFix = ImmutableArray.Create(project);
                allDiagnostics = await fixAllContext.GetAllDiagnosticsAsync(project).ConfigureAwait(false);
                break;

            case FixAllScope.Solution:
                projectsToFix = project.Solution.Projects
                    .Where(p => p.Language == project.Language)
                    .ToImmutableArray();

                var diagnostics = new ConcurrentDictionary<ProjectId, ImmutableArray<Diagnostic>>();
                var tasks = new Task[projectsToFix.Length];
                for (var i = 0; i < projectsToFix.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var projectToFix = projectsToFix[i];
                    tasks[i] = Task.Run(async () =>
                    {
                        var projectDiagnostics = await fixAllContext.GetAllDiagnosticsAsync(projectToFix).ConfigureAwait(false);
                        diagnostics.TryAdd(projectToFix.Id, projectDiagnostics);
                    }, cancellationToken);
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
                allDiagnostics = allDiagnostics.AddRange(diagnostics.SelectMany(i => i.Value));
                break;
        }

        if (allDiagnostics.IsEmpty)
        {
            return ImmutableDictionary<Document, ImmutableArray<Diagnostic>>.Empty;
        }

        return await GetDocumentDiagnosticsToFixAsync(
            allDiagnostics, projectsToFix, fixAllContext.CancellationToken).ConfigureAwait(false);
    }

    private static async Task<ImmutableDictionary<Document, ImmutableArray<Diagnostic>>> GetDocumentDiagnosticsToFixAsync(
        ImmutableArray<Diagnostic> diagnostics,
        ImmutableArray<Project> projects,
        CancellationToken cancellationToken)
    {
        var treeToDocumentMap = await GetTreeToDocumentMapAsync(projects, cancellationToken).ConfigureAwait(false);

        var builder = ImmutableDictionary.CreateBuilder<Document, ImmutableArray<Diagnostic>>();
        foreach (var documentAndDiagnostics in diagnostics.GroupBy(d => GetReportedDocument(d, treeToDocumentMap)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = documentAndDiagnostics.Key;
            var diagnosticsForDocument = documentAndDiagnostics.ToImmutableArray();

            if (document is not null)
            {
                builder.Add(document, diagnosticsForDocument);
            }
        }

        return builder.ToImmutable();
    }

    private static async Task<ImmutableDictionary<SyntaxTree, Document>> GetTreeToDocumentMapAsync(ImmutableArray<Project> projects, CancellationToken cancellationToken)
    {
        var builder = ImmutableDictionary.CreateBuilder<SyntaxTree, Document>();
        foreach (var project in projects)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var document in project.Documents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

                if (tree is not null)
                {
                    builder.Add(tree, document);
                }
            }
        }

        return builder.ToImmutable();
    }

    private static Document? GetReportedDocument(Diagnostic diagnostic, ImmutableDictionary<SyntaxTree, Document> treeToDocumentsMap)
    {
        var tree = diagnostic.Location.SourceTree;
        if (tree is not null)
        {
            if (treeToDocumentsMap.TryGetValue(tree, out var document))
            {
                return document;
            }
        }

        return null;
    }
}