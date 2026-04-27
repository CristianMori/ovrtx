// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Nvidia.Ovrtx.Interop;
using Xunit;

namespace Nvidia.Ovrtx.Tests;

/// <summary>
/// Tests for NativeString, NativeStringContext, and NativeStringArrayContext
/// marshaling helpers.
/// </summary>
public class NativeStringTests
{
    [Fact]
    public void NativeStringContext_NullString_ProducesEmptyNativeString()
    {
        using var ctx = new NativeStringContext(null);
        Assert.Equal(IntPtr.Zero, ctx.Value.Ptr);
        Assert.Equal((nuint)0, ctx.Value.Length);
    }

    [Fact]
    public void NativeStringContext_EmptyString_ProducesEmptyNativeString()
    {
        using var ctx = new NativeStringContext("");
        Assert.Equal(IntPtr.Zero, ctx.Value.Ptr);
        Assert.Equal((nuint)0, ctx.Value.Length);
    }

    [Fact]
    public void NativeStringContext_AsciiString_RoundTrips()
    {
        using var ctx = new NativeStringContext("hello");
        Assert.NotEqual(IntPtr.Zero, ctx.Value.Ptr);
        Assert.Equal((nuint)5, ctx.Value.Length);
        Assert.Equal("hello", ctx.Value.ToManaged());
    }

    [Fact]
    public void NativeStringContext_UnicodeString_RoundTripsAsUtf8()
    {
        using var ctx = new NativeStringContext("héllo wörld");
        Assert.NotEqual(IntPtr.Zero, ctx.Value.Ptr);
        // UTF-8 encodes é as 2 bytes and ö as 2 bytes
        Assert.True(ctx.Value.Length > (nuint)11);
        Assert.Equal("héllo wörld", ctx.Value.ToManaged());
    }

    [Fact]
    public void NativeStringContext_LongString_Works()
    {
        string longStr = new string('x', 10000);
        using var ctx = new NativeStringContext(longStr);
        Assert.Equal((nuint)10000, ctx.Value.Length);
        Assert.Equal(longStr, ctx.Value.ToManaged());
    }

    [Fact]
    public void NativeString_Default_ToManaged_ReturnsNull()
    {
        var ns = new NativeString();
        Assert.Null(ns.ToManaged());
    }

    [Fact]
    public void NativeString_Default_ToString_ReturnsEmpty()
    {
        var ns = new NativeString();
        Assert.Equal("", ns.ToString());
    }

    [Fact]
    public void NativeStringArrayContext_EmptyArray_Works()
    {
        using var ctx = new NativeStringArrayContext(Array.Empty<string>());
        Assert.Empty(ctx.Values);
        Assert.Equal((nuint)0, ctx.Count);
    }

    [Fact]
    public void NativeStringArrayContext_MultipleStrings_AllRoundTrip()
    {
        string[] input = ["alpha", "beta", "gamma"];
        using var ctx = new NativeStringArrayContext(input);

        Assert.Equal((nuint)3, ctx.Count);
        Assert.NotEqual(IntPtr.Zero, ctx.Pointer);

        for (int i = 0; i < input.Length; i++)
        {
            Assert.Equal(input[i], ctx.Values[i].ToManaged());
        }
    }

    [Fact]
    public void NativeStringArrayContext_SingleString_Works()
    {
        using var ctx = new NativeStringArrayContext(["/Render/Camera"]);
        Assert.Equal((nuint)1, ctx.Count);
        Assert.Equal("/Render/Camera", ctx.Values[0].ToManaged());
    }

    [Fact]
    public void NativeStringContext_DisposeTwice_DoesNotThrow()
    {
        var ctx = new NativeStringContext("test");
        ctx.Dispose();
        ctx.Dispose(); // should not throw
    }

    [Fact]
    public void NativeStringArrayContext_DisposeTwice_DoesNotThrow()
    {
        var ctx = new NativeStringArrayContext(["a", "b"]);
        ctx.Dispose();
        ctx.Dispose(); // should not throw
    }
}
