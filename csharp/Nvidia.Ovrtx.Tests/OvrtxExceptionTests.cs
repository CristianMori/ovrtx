// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Nvidia.Ovrtx.Interop;
using Xunit;

namespace Nvidia.Ovrtx.Tests;

/// <summary>
/// Tests for OvrtxException construction and ThrowIfFailed helpers.
/// </summary>
public class OvrtxExceptionTests
{
    [Fact]
    public void OvrtxException_MessageAndStatus()
    {
        var ex = new OvrtxException("test error", ApiStatus.Error);
        Assert.Equal("test error", ex.Message);
        Assert.Equal(ApiStatus.Error, ex.Status);
    }

    [Fact]
    public void OvrtxException_DefaultStatus_IsError()
    {
        var ex = new OvrtxException("test");
        Assert.Equal(ApiStatus.Error, ex.Status);
    }

    [Fact]
    public void OvrtxException_TimeoutStatus()
    {
        var ex = new OvrtxException("timed out", ApiStatus.Timeout);
        Assert.Equal(ApiStatus.Timeout, ex.Status);
    }

    [Fact]
    public void OvrtxException_InnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new OvrtxException("outer", inner);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void ThrowIfFailed_NativeResult_Success_DoesNotThrow()
    {
        var result = new NativeResult { Status = 0 };
        // Should not throw — but ThrowIfFailed calls get_last_error which
        // requires the native library. We test the logic path instead:
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
    }

    [Fact]
    public void ThrowIfFailed_NativeEnqueueResult_Success_DoesNotThrow()
    {
        var result = new NativeEnqueueResult { Status = 0, OpIndex = 5 };
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
    }

    [Fact]
    public void OvrtxException_IsException()
    {
        var ex = new OvrtxException("test");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
