// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using System.Reflection;
using Nvidia.Ovrtx.Interop;
using Xunit;

namespace Nvidia.Ovrtx.Tests;

/// <summary>
/// Tests for public API surface: verifies types exist, are public,
/// implement expected interfaces, and have expected members.
/// </summary>
public class PublicApiTests
{
    // ========================================================================
    // Renderer
    // ========================================================================

    [Fact]
    public void Renderer_ImplementsIDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(Renderer)));
    }

    [Fact]
    public void Renderer_HasExpectedPublicMethods()
    {
        var type = typeof(Renderer);
        Assert.NotNull(type.GetMethod("AddUsd"));
        Assert.NotNull(type.GetMethod("AddUsdAsync"));
        Assert.NotNull(type.GetMethod("AddUsdLayer"));
        Assert.NotNull(type.GetMethod("AddUsdLayerAsync"));
        Assert.NotNull(type.GetMethod("RemoveUsd"));
        Assert.NotNull(type.GetMethod("RemoveUsdAsync"));
        Assert.NotNull(type.GetMethod("CloneUsd"));
        Assert.NotNull(type.GetMethod("CloneUsdAsync"));
        Assert.NotNull(type.GetMethod("ResetStage"));
        Assert.NotNull(type.GetMethod("ResetStageAsync"));
        Assert.NotNull(type.GetMethod("UpdateStageFromUsdTime"));
        Assert.NotNull(type.GetMethod("Step"));
        Assert.NotNull(type.GetMethod("StepAsync"));
        Assert.NotNull(type.GetMethod("Reset"));
        Assert.NotNull(type.GetMethod("ResetAsync"));
        Assert.NotNull(type.GetMethod("WriteAttribute"));
        Assert.NotNull(type.GetMethod("WriteAttributeAsync"));
        Assert.NotNull(type.GetMethod("BindAttribute"));
        Assert.NotNull(type.GetMethod("BindAttributeAsync"));
        Assert.NotNull(type.GetMethod("MapAttribute"));
        Assert.NotNull(type.GetMethod("UnmapAttribute"));
        Assert.NotNull(type.GetMethod("SetLogCallback"));
        Assert.NotNull(type.GetMethod("FlushOpLog"));
        Assert.NotNull(type.GetMethod("GetLastError"));
    }

    [Fact]
    public void Renderer_HasVersionProperty()
    {
        var prop = typeof(Renderer).GetProperty("Version", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(prop);
    }

    [Fact]
    public void Renderer_HasConfigProperty()
    {
        var prop = typeof(Renderer).GetProperty("Config");
        Assert.NotNull(prop);
        Assert.Equal(typeof(RendererConfig), prop!.PropertyType);
    }

    [Fact]
    public void Renderer_IsSealed()
    {
        Assert.True(typeof(Renderer).IsSealed);
    }

    // ========================================================================
    // Output types
    // ========================================================================

    [Fact]
    public void RenderProductSetOutputs_ImplementsIDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(RenderProductSetOutputs)));
    }

    [Fact]
    public void MappedRenderVar_ImplementsIDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(MappedRenderVar)));
    }

    [Fact]
    public void RenderVarOutput_HasMapMethod()
    {
        Assert.NotNull(typeof(RenderVarOutput).GetMethod("Map"));
    }

    [Fact]
    public void FrameOutput_HasExpectedProperties()
    {
        Assert.NotNull(typeof(FrameOutput).GetProperty("StartTime"));
        Assert.NotNull(typeof(FrameOutput).GetProperty("EndTime"));
        Assert.NotNull(typeof(FrameOutput).GetProperty("RenderVars"));
    }

    [Fact]
    public void ProductOutput_HasExpectedProperties()
    {
        Assert.NotNull(typeof(ProductOutput).GetProperty("Name"));
        Assert.NotNull(typeof(ProductOutput).GetProperty("Frames"));
    }

    // ========================================================================
    // Attribute types
    // ========================================================================

    [Fact]
    public void AttributeBinding_ImplementsIDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(AttributeBinding)));
    }

    [Fact]
    public void AttributeMapping_ImplementsIDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(AttributeMapping)));
    }

    [Fact]
    public void AttributeBinding_HasExpectedMethods()
    {
        Assert.NotNull(typeof(AttributeBinding).GetMethod("Write"));
        Assert.NotNull(typeof(AttributeBinding).GetMethod("WriteAsync"));
        Assert.NotNull(typeof(AttributeBinding).GetMethod("Map"));
        Assert.NotNull(typeof(AttributeBinding).GetMethod("Unbind"));
    }

    [Fact]
    public void AttributeMapping_HasTensorProperty()
    {
        var prop = typeof(AttributeMapping).GetProperty("Tensor");
        Assert.NotNull(prop);
        Assert.Equal(typeof(DLTensor), prop!.PropertyType);
    }

    // ========================================================================
    // Operation types
    // ========================================================================

    [Fact]
    public void Operation_HasWaitMethod()
    {
        var method = typeof(Operation).GetMethod("Wait");
        Assert.NotNull(method);
        Assert.Equal(typeof(bool), method!.ReturnType);
    }

    [Fact]
    public void Operation_HasOpIdProperty()
    {
        var prop = typeof(Operation).GetProperty("OpId");
        Assert.NotNull(prop);
        Assert.Equal(typeof(OpId), prop!.PropertyType);
    }

    [Fact]
    public void OperationGeneric_HasWaitMethod()
    {
        var type = typeof(Operation<>).MakeGenericType(typeof(UsdHandle));
        Assert.NotNull(type.GetMethod("Wait"));
    }

    [Fact]
    public void OperationGeneric_HasIsCompletedProperty()
    {
        var type = typeof(Operation<>).MakeGenericType(typeof(UsdHandle));
        Assert.NotNull(type.GetProperty("IsCompleted"));
    }

    // ========================================================================
    // RendererResult
    // ========================================================================

    [Fact]
    public void RendererResult_HasWaitMethod()
    {
        Assert.NotNull(typeof(RendererResult).GetMethod("Wait"));
    }

    [Fact]
    public void RendererResult_HasStepCompleteProperty()
    {
        Assert.NotNull(typeof(RendererResult).GetProperty("StepComplete"));
    }

    // ========================================================================
    // OvrtxLibrary
    // ========================================================================

    [Fact]
    public void OvrtxLibrary_HasSetLibraryPath()
    {
        Assert.NotNull(typeof(OvrtxLibrary).GetMethod("SetLibraryPath"));
    }

    [Fact]
    public void OvrtxLibrary_HasGetVersion()
    {
        Assert.NotNull(typeof(OvrtxLibrary).GetMethod("GetVersion"));
    }

    [Fact]
    public void OvrtxLibrary_IsStatic()
    {
        Assert.True(typeof(OvrtxLibrary).IsAbstract && typeof(OvrtxLibrary).IsSealed);
    }

    // ========================================================================
    // Verify internal types are NOT public
    // ========================================================================

    [Fact]
    public void NativeMethods_IsInternal()
    {
        var type = typeof(Renderer).Assembly.GetType("Nvidia.Ovrtx.Interop.NativeMethods");
        Assert.NotNull(type);
        Assert.False(type!.IsPublic);
    }

    [Fact]
    public void NativeResult_IsInternal()
    {
        var type = typeof(Renderer).Assembly.GetType("Nvidia.Ovrtx.Interop.NativeResult");
        Assert.NotNull(type);
        Assert.False(type!.IsPublic);
    }

    [Fact]
    public void NativeConfigContext_IsInternal()
    {
        var type = typeof(Renderer).Assembly.GetType("Nvidia.Ovrtx.NativeConfigContext");
        Assert.NotNull(type);
        Assert.False(type!.IsPublic);
    }
}
