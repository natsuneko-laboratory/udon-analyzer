// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

using System.Threading.Tasks;

using NatsunekoLaboratory.UdonAnalyzer.AnalyzerSpec.Attributes;
using NatsunekoLaboratory.UdonAnalyzer.Testing;
using NatsunekoLaboratory.UdonAnalyzer.Udon;

using Xunit;

namespace Analyzers.Tests.Udon;

[Describe(typeof(FieldAccessorIsNotExposedInUdonAnalyzer), "VRC")]
public class FieldAccessorIsNotExposedInUdonAnalyzerTest : UdonSharpDiagnosticVerifier<FieldAccessorIsNotExposedInUdonAnalyzer>
{
    [Fact]
    [Example]
    public async Task TestDiagnostic_DisallowedFieldAccessorOnUdonSharpBehaviour()
    {
        AddAdditionalFile("PublicAPI.Shipped.test.txt", "");

        await VerifyAnalyzerAsync(@"
using UdonSharp;

using UnityEngine;

class TestBehaviour : UdonSharpBehaviour
{
    [SerializeField]
    private ParticleSystemForceField _field;

    public void TestMethod()
    {
        var go = [|_field.gameObject|];
    }
}
");
    }

    [Fact]
    public async Task TestNoDiagnostic_AllowedFieldAccessorOnUdonSharpBehaviour()
    {
        AddAdditionalFile("PublicAPI.Shipped.test.txt", "F:UnityEngine.Vector3.one");

        await VerifyAnalyzerAsync(@"
using UdonSharp;

using UnityEngine;

class TestBehaviour : UdonSharpBehaviour
{
    public void TestMethod()
    {
        var go = Vector3.one;
    }
}
");
    }

    [Fact]
    public async Task TestNoDiagnostic_UserDefinedFieldInOtherBehaviourOnUdonSharpBehaviour()
    {
        await VerifyAnalyzerAsync(@"
using UdonSharp;

class TestBehaviour : UdonSharpBehaviour
{
    private SomeBehaviour _behaviour;

    public void TestMethod()
    {
        var a = _behaviour.SomeStr;
    }
}

class SomeBehaviour : UdonSharpBehaviour
{
    public readonly string SomeStr = ""str"";
}
");
    }

    [Fact]
    public async Task TestNoDiagnostic_UserDefinedFieldOnUdonSharpBehaviour()
    {
        await VerifyAnalyzerAsync(@"
using UdonSharp;

class TestBehaviour : UdonSharpBehaviour
{
    private readonly string _str = ""str"";

    public void TestMethod()
    {
        var a = _str;
    }
}
");
    }
}