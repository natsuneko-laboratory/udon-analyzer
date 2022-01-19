﻿// -----------------------------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the Microsoft Reference Source License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace NatsunekoLaboratory.UdonAnalyzer.Testing;

public abstract class DiagnosticVerifier<TAnalyzer, TProject> where TAnalyzer : DiagnosticAnalyzer, new() where TProject : StandaloneProject, new()
{
    protected virtual ImmutableArray<string> FilteredDiagnosticIds { get; } = ImmutableArray.Create<string>();

    protected virtual DiagnosticResult ExpectDiagnostic()
    {
        return new DiagnosticResult(new TAnalyzer().SupportedDiagnostics[0]);
    }

    protected async Task VerifyAnalyzerAsync(string annotatedSource)
    {
        var (source, diagnostics) = ParseAnnotatedSource(annotatedSource);
        await VerifyAnalyzerAsync(source, diagnostics);
    }

    protected async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var project = StandaloneProject.CreateProject<TProject>(new[] { source });

        await project.RunAnalyzerAsync<TAnalyzer>(expected, FilteredDiagnosticIds, CancellationToken.None);
    }

    protected (string Source, DiagnosticResult[] Diagnostics) ParseAnnotatedSource(string source)
    {
        var sb = new StringBuilder();
        var diagnostics = new List<DiagnosticResult>();

        var line = 1;
        var column = 1;
        var expectedLine = 0;
        var expectedColumn = 0;
        var isReadingAnnotation = false;

        using var sr = new StringReader(source);
        while (sr.Peek() > -1)
        {
            var c = (char)sr.Read();
            switch (c)
            {
                case '\n':
                    sb.Append(c);
                    line++;
                    column = 1;
                    break;

                case '[' when sr.Peek() == '|':
                    sr.Read();

                    expectedLine = line;
                    expectedColumn = column;
                    isReadingAnnotation = true;
                    break;

                case '|' when isReadingAnnotation && sr.Peek() == '@':
                    sr.Read();

                    var message = new StringBuilder();
                    while (sr.Peek() != ']')
                        message.Append(sr.Read());
                    sr.Read();

                    diagnostics.Add(ExpectDiagnostic().WithSpan(expectedLine, expectedColumn, line, column).WithMessage(message.ToString()));
                    isReadingAnnotation = false;
                    break;

                case '|' when isReadingAnnotation && sr.Peek() == ']':
                    sr.Read();

                    diagnostics.Add(ExpectDiagnostic().WithSpan(expectedLine, expectedColumn, line, column));
                    isReadingAnnotation = false;
                    break;

                default:
                    sb.Append(c);
                    column++;
                    break;
            }
        }

        return (Source: sb.ToString(), Diagnostics: diagnostics.ToArray());
    }
}