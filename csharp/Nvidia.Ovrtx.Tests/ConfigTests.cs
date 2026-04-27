// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using System.Runtime.InteropServices;
using Nvidia.Ovrtx.Interop;
using Xunit;

namespace Nvidia.Ovrtx.Tests;

/// <summary>
/// Tests for RendererConfig, NativeConfigEntry, and NativeConfigContext.
/// </summary>
public class ConfigTests
{
    [Fact]
    public void RendererConfig_Default_AllPropertiesNull()
    {
        var config = new RendererConfig();
        Assert.Null(config.SyncMode);
        Assert.Null(config.LogFilePath);
        Assert.Null(config.LogLevel);
        Assert.Null(config.EnableProfiling);
        Assert.Null(config.ReadGpuTransforms);
        Assert.Null(config.OutputPartialFrames);
        Assert.Null(config.KeepSystemAlive);
        Assert.Null(config.ActiveCudaGpus);
        Assert.Null(config.UseVulkan);
    }

    [Fact]
    public void RendererConfig_WithInit_SetsValues()
    {
        var config = new RendererConfig
        {
            SyncMode = true,
            LogLevel = "info",
            ActiveCudaGpus = "0,1",
        };
        Assert.True(config.SyncMode);
        Assert.Equal("info", config.LogLevel);
        Assert.Equal("0,1", config.ActiveCudaGpus);
    }

    [Fact]
    public void NativeConfigContext_NullConfig_ProducesEmptyNativeConfig()
    {
        using var ctx = new NativeConfigContext(null);
        Assert.Equal(IntPtr.Zero, ctx.Config.Entries);
        Assert.Equal((nuint)0, ctx.Config.EntryCount);
    }

    [Fact]
    public void NativeConfigContext_EmptyConfig_ProducesEmptyNativeConfig()
    {
        using var ctx = new NativeConfigContext(new RendererConfig());
        Assert.Equal(IntPtr.Zero, ctx.Config.Entries);
        Assert.Equal((nuint)0, ctx.Config.EntryCount);
    }

    [Fact]
    public void NativeConfigContext_BoolOnly_ProducesOneEntry()
    {
        var config = new RendererConfig { SyncMode = true };
        using var ctx = config.ToNative();
        Assert.Equal((nuint)1, ctx.Config.EntryCount);
        Assert.NotEqual(IntPtr.Zero, ctx.Config.Entries);
    }

    [Fact]
    public void NativeConfigContext_StringOnly_ProducesOneEntry()
    {
        var config = new RendererConfig { LogLevel = "info" };
        using var ctx = config.ToNative();
        Assert.Equal((nuint)1, ctx.Config.EntryCount);
    }

    [Fact]
    public void NativeConfigContext_MultipleBools_ProducesCorrectCount()
    {
        var config = new RendererConfig
        {
            SyncMode = true,
            EnableProfiling = false,
            KeepSystemAlive = true,
        };
        using var ctx = config.ToNative();
        Assert.Equal((nuint)3, ctx.Config.EntryCount);
    }

    [Fact]
    public void NativeConfigContext_MixedBoolAndString_ProducesCorrectCount()
    {
        var config = new RendererConfig
        {
            SyncMode = true,
            LogFilePath = "/tmp/test.log",
            LogLevel = "verbose",
            EnableProfiling = false,
        };
        using var ctx = config.ToNative();
        Assert.Equal((nuint)4, ctx.Config.EntryCount);
    }

    [Fact]
    public void NativeConfigContext_AllProperties_ProducesNineEntries()
    {
        var config = new RendererConfig
        {
            SyncMode = true,
            LogFilePath = "/tmp/test.log",
            LogLevel = "info",
            EnableProfiling = true,
            ReadGpuTransforms = false,
            OutputPartialFrames = true,
            KeepSystemAlive = false,
            ActiveCudaGpus = "0",
            UseVulkan = true,
        };
        using var ctx = config.ToNative();
        Assert.Equal((nuint)9, ctx.Config.EntryCount);
    }

    [Fact]
    public void NativeConfigEntry_Bool_HasCorrectKeyType()
    {
        var entry = NativeConfigEntry.Bool(NativeConfigBoolKey.SyncMode, true);
        Assert.Equal((int)NativeConfigKeyType.Bool, entry.KeyType);
        Assert.Equal((int)NativeConfigBoolKey.SyncMode, entry.KeyValue);
        Assert.Equal((byte)1, entry.BoolValue);
    }

    [Fact]
    public void NativeConfigEntry_Bool_False_HasZeroBoolValue()
    {
        var entry = NativeConfigEntry.Bool(NativeConfigBoolKey.EnableProfiling, false);
        Assert.Equal((byte)0, entry.BoolValue);
    }

    [Fact]
    public void NativeConfigEntry_String_HasCorrectKeyType()
    {
        using var strCtx = new NativeStringContext("info");
        var entry = NativeConfigEntry.String(NativeConfigStringKey.LogLevel, strCtx.Value);
        Assert.Equal((int)NativeConfigKeyType.String, entry.KeyType);
        Assert.Equal((int)NativeConfigStringKey.LogLevel, entry.KeyValue);
        Assert.NotEqual(IntPtr.Zero, entry.StringOrBlobPtr);
    }

    [Fact]
    public void NativeConfigContext_DisposeTwice_DoesNotThrow()
    {
        var config = new RendererConfig { SyncMode = true, LogLevel = "info" };
        var ctx = config.ToNative();
        ctx.Dispose();
        ctx.Dispose(); // should not throw
    }

    [Fact]
    public void RendererConfig_IsRecord_SupportsWith()
    {
        var config = new RendererConfig { SyncMode = true };
        var modified = config with { LogLevel = "error" };
        Assert.True(modified.SyncMode);
        Assert.Equal("error", modified.LogLevel);
    }
}
