// SPDX-FileCopyrightText: Copyright (c) 2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: LicenseRef-NvidiaProprietary
//
// NVIDIA CORPORATION, its affiliates and licensors retain all intellectual
// property and proprietary rights in and to this material, related
// documentation and any modifications thereto. Any use, reproduction,
// disclosure or distribution of this material and related documentation
// without an express license agreement from NVIDIA CORPORATION or
// its affiliates is strictly prohibited.

using System.Runtime.InteropServices;
using Nvidia.Ovrtx.Interop;

namespace Nvidia.Ovrtx;

/// <summary>
/// Configuration for creating an ovrtx Renderer instance.
/// All properties are optional; unset properties use the library defaults.
/// </summary>
public sealed record RendererConfig
{
    /// <summary>If true, stream operations execute synchronously (enqueue blocks until complete).</summary>
    public bool? SyncMode { get; init; }

    /// <summary>Path to log file for carb logging.</summary>
    public string? LogFilePath { get; init; }

    /// <summary>Log level: "verbose", "info", "warn", "error".</summary>
    public string? LogLevel { get; init; }

    /// <summary>If true, enables internal profiling.</summary>
    public bool? EnableProfiling { get; init; }

    /// <summary>If true, uses GPU world transform propagation during rendering.</summary>
    public bool? ReadGpuTransforms { get; init; }

    /// <summary>If true, outputs partial frames for incremental sensors.</summary>
    public bool? OutputPartialFrames { get; init; }

    /// <summary>If true, keeps the renderer system alive after all instances are destroyed.</summary>
    public bool? KeepSystemAlive { get; init; }

    /// <summary>Comma-separated CUDA device indices to use (e.g., "0,1,2").</summary>
    public string? ActiveCudaGpus { get; init; }

    /// <summary>If true, uses Vulkan; if false, uses DX12 (Windows). Defaults to platform default.</summary>
    public bool? UseVulkan { get; init; }

    /// <summary>
    /// Converts this config to native format. The returned context must be disposed after
    /// the native call completes to free pinned string memory.
    /// </summary>
    internal NativeConfigContext ToNative()
    {
        return new NativeConfigContext(this);
    }
}

/// <summary>
/// Manages the lifetime of pinned native config data. Dispose after the P/Invoke call.
/// </summary>
internal sealed class NativeConfigContext : IDisposable
{
    private readonly List<NativeStringContext> _stringContexts = new();
    private GCHandle _entriesHandle;

    public NativeConfig Config { get; }

    public NativeConfigContext(RendererConfig? config)
    {
        if (config is null)
        {
            Config = new NativeConfig { Entries = IntPtr.Zero, EntryCount = 0 };
            return;
        }

        var entries = new List<NativeConfigEntry>();

        AddBool(entries, NativeConfigBoolKey.SyncMode, config.SyncMode);
        AddBool(entries, NativeConfigBoolKey.EnableProfiling, config.EnableProfiling);
        AddBool(entries, NativeConfigBoolKey.ReadGpuTransforms, config.ReadGpuTransforms);
        AddBool(entries, NativeConfigBoolKey.OutputPartialFrames, config.OutputPartialFrames);
        AddBool(entries, NativeConfigBoolKey.KeepSystemAlive, config.KeepSystemAlive);
        AddBool(entries, NativeConfigBoolKey.UseVulkan, config.UseVulkan);

        AddString(entries, NativeConfigStringKey.LogFilePath, config.LogFilePath);
        AddString(entries, NativeConfigStringKey.LogLevel, config.LogLevel);
        AddString(entries, NativeConfigStringKey.ActiveCudaGpus, config.ActiveCudaGpus);

        if (entries.Count == 0)
        {
            Config = new NativeConfig { Entries = IntPtr.Zero, EntryCount = 0 };
            return;
        }

        var array = entries.ToArray();
        _entriesHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
        Config = new NativeConfig
        {
            Entries = _entriesHandle.AddrOfPinnedObject(),
            EntryCount = (nuint)array.Length,
        };
    }

    private static void AddBool(List<NativeConfigEntry> entries, NativeConfigBoolKey key, bool? value)
    {
        if (value.HasValue)
            entries.Add(NativeConfigEntry.Bool(key, value.Value));
    }

    private void AddString(List<NativeConfigEntry> entries, NativeConfigStringKey key, string? value)
    {
        if (value is null) return;

        var ctx = new NativeStringContext(value);
        _stringContexts.Add(ctx);
        entries.Add(NativeConfigEntry.String(key, ctx.Value));
    }

    public void Dispose()
    {
        if (_entriesHandle.IsAllocated)
            _entriesHandle.Free();

        foreach (var ctx in _stringContexts)
            ctx.Dispose();
    }
}
