// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using NatsunekoLaboratory.UdonAnalyzer.Attributes;
using NatsunekoLaboratory.UdonAnalyzer.Internal;

namespace NatsunekoLaboratory.UdonAnalyzer.UdonSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
[RequireUdonVersion("[2021.11.24.16.19,)")]
[RequireUdonSharpCompilerVersion("[0.20.3,)")]
public class DoesNotYetSupportLabeledStatementsAnalyzer : BaseDiagnosticAnalyzer
{
    public override DiagnosticDescriptor SupportedDiagnostic => DiagnosticDescriptors.DoesNotYetSupportLabeledStatements;

    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(w => RunAnalyzer(w, true, AnalyzeLabeledStatement), SyntaxKind.LabeledStatement);
    }

    private void AnalyzeLabeledStatement(SyntaxNodeAnalysisContext context)
    {
        var statement = (LabeledStatementSyntax)context.Node;
        DiagnosticHelper.ReportDiagnostic(context, SupportedDiagnostic, statement);
    }
}