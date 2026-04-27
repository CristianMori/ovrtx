// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Nvidia.Ovrtx.Interop;
using Xunit;

namespace Nvidia.Ovrtx.Tests;

/// <summary>
/// Tests for native struct helper methods and factory functions.
/// </summary>
public class NativeTypeTests
{
    // ========================================================================
    // NativeResult
    // ========================================================================

    [Fact]
    public void NativeResult_Success_IsSuccess()
    {
        var r = new NativeResult { Status = 0 };
        Assert.True(r.IsSuccess);
        Assert.False(r.IsError);
        Assert.False(r.IsTimeout);
    }

    [Fact]
    public void NativeResult_Error_IsError()
    {
        var r = new NativeResult { Status = 1 };
        Assert.False(r.IsSuccess);
        Assert.True(r.IsError);
        Assert.False(r.IsTimeout);
    }

    [Fact]
    public void NativeResult_Timeout_IsTimeout()
    {
        var r = new NativeResult { Status = 2 };
        Assert.False(r.IsSuccess);
        Assert.False(r.IsError);
        Assert.True(r.IsTimeout);
    }

    // ========================================================================
    // NativeEnqueueResult
    // ========================================================================

    [Fact]
    public void NativeEnqueueResult_Success_WithOpIndex()
    {
        var r = new NativeEnqueueResult { Status = 0, OpIndex = 42 };
        Assert.True(r.IsSuccess);
        Assert.Equal(42UL, r.OpIndex);
    }

    // ========================================================================
    // NativeTimeout
    // ========================================================================

    [Fact]
    public void NativeTimeout_Infinite_IsMaxValue()
    {
        Assert.Equal(ulong.MaxValue, NativeTimeout.Infinite.TimeOutNs);
    }

    [Fact]
    public void NativeTimeout_Zero_IsZero()
    {
        Assert.Equal(0UL, NativeTimeout.Zero.TimeOutNs);
    }

    [Fact]
    public void NativeTimeout_FromMilliseconds_ConvertsCorrectly()
    {
        var t = NativeTimeout.FromMilliseconds(500);
        Assert.Equal(500_000_000UL, t.TimeOutNs);
    }

    [Fact]
    public void NativeTimeout_FromSeconds_ConvertsCorrectly()
    {
        var t = NativeTimeout.FromSeconds(1.5);
        Assert.Equal(1_500_000_000UL, t.TimeOutNs);
    }

    // ========================================================================
    // NativeCudaSync
    // ========================================================================

    [Fact]
    public void NativeCudaSync_None_IsAllZero()
    {
        var sync = NativeCudaSync.None;
        Assert.Equal((nuint)0, sync.Stream);
        Assert.Equal((nuint)0, sync.WaitEvent);
    }

    // ========================================================================
    // NativeBindingDescOrHandle
    // ========================================================================

    [Fact]
    public void NativeBindingDescOrHandle_FromHandle_SetsHandle()
    {
        var b = NativeBindingDescOrHandle.FromHandle(99);
        Assert.Equal(99UL, b.BindingHandle);
    }

    [Fact]
    public void NativeBindingDescOrHandle_FromDesc_HasZeroHandle()
    {
        var desc = new NativeBindingDesc();
        var b = NativeBindingDescOrHandle.FromDesc(desc);
        Assert.Equal(0UL, b.BindingHandle);
    }

    // ========================================================================
    // NativeOpWaitResult
    // ========================================================================

    [Fact]
    public void NativeOpWaitResult_Default_HasNoErrors()
    {
        var r = new NativeOpWaitResult();
        Assert.True(r.GetErrorOpIds().IsEmpty);
    }
}
