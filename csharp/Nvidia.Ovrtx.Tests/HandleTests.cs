// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Nvidia.Ovrtx.Interop;
using Xunit;

namespace Nvidia.Ovrtx.Tests;

/// <summary>
/// Tests for type-safe handle wrappers.
/// </summary>
public class HandleTests
{
    [Fact]
    public void UsdHandle_Invalid_HasValueZero()
    {
        Assert.Equal(0UL, UsdHandle.Invalid.Value);
        Assert.False(UsdHandle.Invalid.IsValid);
    }

    [Fact]
    public void UsdHandle_NonZero_IsValid()
    {
        var handle = new UsdHandle(42);
        Assert.True(handle.IsValid);
        Assert.Equal(42UL, handle.Value);
    }

    [Fact]
    public void StepResultHandle_Invalid_HasValueZero()
    {
        Assert.False(StepResultHandle.Invalid.IsValid);
    }

    [Fact]
    public void OpId_Invalid_HasValueZero()
    {
        Assert.False(OpId.Invalid.IsValid);
    }

    [Fact]
    public void OpId_NonZero_IsValid()
    {
        var id = new OpId(1);
        Assert.True(id.IsValid);
    }

    [Fact]
    public void AttributeBindingHandle_Invalid_HasValueZero()
    {
        Assert.False(AttributeBindingHandle.Invalid.IsValid);
    }

    [Fact]
    public void MapHandle_Invalid_HasValueZero()
    {
        Assert.False(MapHandle.Invalid.IsValid);
    }

    [Fact]
    public void RenderedOutputHandle_Invalid_HasValueZero()
    {
        Assert.False(RenderedOutputHandle.Invalid.IsValid);
    }

    [Fact]
    public void RenderedOutputMapHandle_Invalid_HasValueZero()
    {
        Assert.False(RenderedOutputMapHandle.Invalid.IsValid);
    }

    [Fact]
    public void UsdHandle_Equality_Works()
    {
        var a = new UsdHandle(10);
        var b = new UsdHandle(10);
        var c = new UsdHandle(20);
        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void OpId_Equality_Works()
    {
        var a = new OpId(5);
        var b = new OpId(5);
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Handle_DefaultConstructor_IsInvalid()
    {
        Assert.False(default(UsdHandle).IsValid);
        Assert.False(default(StepResultHandle).IsValid);
        Assert.False(default(OpId).IsValid);
        Assert.False(default(AttributeBindingHandle).IsValid);
        Assert.False(default(MapHandle).IsValid);
        Assert.False(default(RenderedOutputHandle).IsValid);
        Assert.False(default(RenderedOutputMapHandle).IsValid);
    }
}
