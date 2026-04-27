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

/// <summary>Log message callback delegate.</summary>
public delegate void LogCallback(OpId opId, LogSeverity severity, double timestamp, string message);

/// <summary>
/// Main ovrtx renderer. Creates and manages an RTX rendering instance.
/// Dispose when done to release all native resources.
/// </summary>
public sealed class Renderer : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private NativeLogCallback? _nativeLogCallback; // prevent GC
    private LogCallback? _userLogCallback;

    internal IntPtr Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _handle;
        }
    }

    public RendererConfig Config { get; }

    /// <summary>Library version as (Major, Minor, Patch).</summary>
    public static (uint Major, uint Minor, uint Patch) Version
    {
        get
        {
            NativeMethods.ovrtx_get_version(out var major, out var minor, out var patch);
            return (major, minor, patch);
        }
    }

    public Renderer(RendererConfig? config = null)
    {
        Config = config ?? new RendererConfig();

        using var nativeConfig = Config.ToNative();
        var cfg = nativeConfig.Config;
        var result = NativeMethods.ovrtx_create_renderer(in cfg, out _handle);
        OvrtxException.ThrowIfFailed(result, "create renderer");
    }

    // ========================================================================
    // USD Management
    // ========================================================================

    /// <summary>Load a USD file into the stage (synchronous).</summary>
    public UsdHandle AddUsd(string usdFilePath, string? pathPrefix = null)
    {
        var op = AddUsdAsync(usdFilePath, pathPrefix);
        return op.WaitRequired();
    }

    /// <summary>Load a USD file into the stage (asynchronous).</summary>
    public Operation<UsdHandle> AddUsdAsync(string usdFilePath, string? pathPrefix = null)
    {
        using var filePathCtx = new NativeStringContext(usdFilePath);
        using var prefixCtx = new NativeStringContext(pathPrefix);

        var usdInput = new NativeUsdInput { UsdFilePath = filePathCtx.Value };

        var result = NativeMethods.ovrtx_add_usd(Handle, usdInput, prefixCtx.Value, out var usdHandle);
        OvrtxException.ThrowIfFailed(result, "add USD");

        return new Operation<UsdHandle>(this, new OpId(result.OpIndex), new UsdHandle(usdHandle));
    }

    /// <summary>Load inline USDA layer content into the stage (synchronous).</summary>
    public UsdHandle AddUsdLayer(string layerContent, string? pathPrefix = null)
    {
        var op = AddUsdLayerAsync(layerContent, pathPrefix);
        return op.WaitRequired();
    }

    /// <summary>Load inline USDA layer content into the stage (asynchronous).</summary>
    public Operation<UsdHandle> AddUsdLayerAsync(string layerContent, string? pathPrefix = null)
    {
        using var contentCtx = new NativeStringContext(layerContent);
        using var prefixCtx = new NativeStringContext(pathPrefix);

        var usdInput = new NativeUsdInput { UsdLayerContent = contentCtx.Value };

        var result = NativeMethods.ovrtx_add_usd(Handle, usdInput, prefixCtx.Value, out var usdHandle);
        OvrtxException.ThrowIfFailed(result, "add USD layer");

        return new Operation<UsdHandle>(this, new OpId(result.OpIndex), new UsdHandle(usdHandle));
    }

    /// <summary>Remove a previously added USD file from the stage (synchronous).</summary>
    public void RemoveUsd(UsdHandle handle)
    {
        RemoveUsdAsync(handle).Wait();
    }

    /// <summary>Remove a previously added USD file from the stage (asynchronous).</summary>
    public Operation RemoveUsdAsync(UsdHandle handle)
    {
        var result = NativeMethods.ovrtx_remove_usd(Handle, handle.Value);
        OvrtxException.ThrowIfFailed(result, "remove USD");
        return new Operation(this, new OpId(result.OpIndex));
    }

    /// <summary>Clone a USD subtree to one or more target paths (synchronous).</summary>
    public void CloneUsd(string sourcePath, string[] targetPaths)
    {
        CloneUsdAsync(sourcePath, targetPaths).Wait();
    }

    /// <summary>Clone a USD subtree to one or more target paths (asynchronous).</summary>
    public Operation CloneUsdAsync(string sourcePath, string[] targetPaths)
    {
        using var sourceCtx = new NativeStringContext(sourcePath);
        using var targetsCtx = new NativeStringArrayContext(targetPaths);

        var result = NativeMethods.ovrtx_clone_usd(
            Handle, sourceCtx.Value, targetsCtx.Pointer, targetsCtx.Count);
        OvrtxException.ThrowIfFailed(result, "clone USD");

        return new Operation(this, new OpId(result.OpIndex));
    }

    /// <summary>Reset the runtime stage to empty (synchronous).</summary>
    public void ResetStage()
    {
        ResetStageAsync().Wait();
    }

    /// <summary>Reset the runtime stage to empty (asynchronous).</summary>
    public Operation ResetStageAsync()
    {
        var result = NativeMethods.ovrtx_reset_stage(Handle);
        OvrtxException.ThrowIfFailed(result, "reset stage");
        return new Operation(this, new OpId(result.OpIndex));
    }

    /// <summary>Update stage attributes from USD time samples.</summary>
    public void UpdateStageFromUsdTime(double usdTime)
    {
        var result = NativeMethods.ovrtx_update_stage_from_usd_time(Handle, usdTime);
        OvrtxException.ThrowIfFailed(result, "update stage from USD time");
        // Wait for completion
        new Operation(this, new OpId(result.OpIndex)).Wait();
    }

    // ========================================================================
    // Rendering
    // ========================================================================

    /// <summary>
    /// Run a simulation step for the given render products (synchronous).
    /// Returns output metadata. Dispose the result when done.
    /// </summary>
    public RenderProductSetOutputs Step(string[] renderProducts, double deltaTime)
    {
        var rendererResult = StepAsync(renderProducts, deltaTime);
        return rendererResult.Wait()
            ?? throw new OvrtxException("Step timed out unexpectedly");
    }

    /// <summary>
    /// Run a simulation step (asynchronous). Returns a two-stage result
    /// that allows separate timeout control for step and fetch.
    /// </summary>
    public RendererResult StepAsync(string[] renderProducts, double deltaTime)
    {
        using var productsCtx = new NativeStringArrayContext(renderProducts);

        var renderProductSet = new NativeRenderProductSet
        {
            RenderProducts = productsCtx.Pointer,
            NumRenderProducts = productsCtx.Count,
        };

        var result = NativeMethods.ovrtx_step(Handle, renderProductSet, deltaTime, out var stepResultHandle);
        OvrtxException.ThrowIfFailed(result, "step");

        return new RendererResult(this, new OpId(result.OpIndex), stepResultHandle);
    }

    /// <summary>Reset sensor simulation history (synchronous).</summary>
    public void Reset(double time = 0.0)
    {
        ResetAsync(time).Wait();
    }

    /// <summary>Reset sensor simulation history (asynchronous).</summary>
    public Operation ResetAsync(double time = 0.0)
    {
        var result = NativeMethods.ovrtx_reset(Handle, time);
        OvrtxException.ThrowIfFailed(result, "reset");
        return new Operation(this, new OpId(result.OpIndex));
    }

    // ========================================================================
    // Attribute Writing
    // ========================================================================

    /// <summary>
    /// Write attribute data to prims (synchronous).
    /// </summary>
    /// <param name="primPaths">Prim paths to write to.</param>
    /// <param name="attributeName">Attribute name (e.g., "xformOp:transform").</param>
    /// <param name="tensor">Pointer to a pinned DLTensor with the data.</param>
    /// <param name="semantic">Attribute semantic for type interpretation.</param>
    /// <param name="primMode">How to handle missing prims.</param>
    /// <param name="dataAccess">SYNC copies immediately; ASYNC requires caller to keep data alive.</param>
    /// <param name="dirtyBits">Optional dirty bit array (1 bit per prim).</param>
    /// <param name="cudaStream">Optional CUDA stream for async access.</param>
    /// <param name="cudaEvent">Optional CUDA event for async access.</param>
    public unsafe void WriteAttribute(
        string[] primPaths,
        string attributeName,
        DLTensor* tensor,
        Semantic semantic = Semantic.None,
        PrimMode primMode = PrimMode.ExistingOnly,
        DataAccess dataAccess = DataAccess.Sync,
        byte[]? dirtyBits = null,
        nuint cudaStream = 0,
        nuint cudaEvent = 0)
    {
        WriteAttributeAsync(primPaths, attributeName, tensor, semantic, primMode, dataAccess,
            dirtyBits, cudaStream, cudaEvent).Wait();
    }

    /// <summary>Write attribute data to prims (asynchronous).</summary>
    public unsafe Operation WriteAttributeAsync(
        string[] primPaths,
        string attributeName,
        DLTensor* tensor,
        Semantic semantic = Semantic.None,
        PrimMode primMode = PrimMode.ExistingOnly,
        DataAccess dataAccess = DataAccess.Sync,
        byte[]? dirtyBits = null,
        nuint cudaStream = 0,
        nuint cudaEvent = 0)
    {
        using var primsCtx = new NativeStringArrayContext(primPaths);
        using var attrNameCtx = new NativeStringContext(attributeName);

        var bindingDesc = new NativeBindingDesc
        {
            PrimList = new NativePrimList
            {
                PrimPaths = primsCtx.Pointer,
                NumPaths = primsCtx.Count,
            },
            AttributeName = new NativeStringOrToken { String = attrNameCtx.Value },
            AttributeType = new NativeAttributeType
            {
                DType = tensor->DType,
                Semantic = (int)semantic,
            },
            PrimMode = (int)primMode,
        };

        var binding = NativeBindingDescOrHandle.FromDesc(bindingDesc);

        var inputBuffer = new NativeInputBuffer
        {
            Tensors = (IntPtr)tensor,
            TensorCount = 1,
            AccessCudaSync = new NativeCudaSync { Stream = cudaStream, WaitEvent = cudaEvent },
        };

        if (dirtyBits != null)
        {
            fixed (byte* dirtyPtr = dirtyBits)
            {
                inputBuffer.DirtyBits = (IntPtr)dirtyPtr;
                inputBuffer.DirtyBitsSize = (nuint)dirtyBits.Length;

                var result = NativeMethods.ovrtx_write_attribute(
                    Handle, in binding, in inputBuffer, (int)dataAccess);
                OvrtxException.ThrowIfFailed(result, "write attribute");
                return new Operation(this, new OpId(result.OpIndex));
            }
        }
        else
        {
            var result = NativeMethods.ovrtx_write_attribute(
                Handle, in binding, in inputBuffer, (int)dataAccess);
            OvrtxException.ThrowIfFailed(result, "write attribute");
            return new Operation(this, new OpId(result.OpIndex));
        }
    }

    // ========================================================================
    // Attribute Binding (Persistent)
    // ========================================================================

    /// <summary>Create a persistent attribute binding for efficient repeated writes (synchronous).</summary>
    public AttributeBinding BindAttribute(
        string[] primPaths,
        string attributeName,
        DLDataType dtype,
        Semantic semantic = Semantic.None,
        PrimMode primMode = PrimMode.ExistingOnly,
        BindingFlag flags = BindingFlag.None)
    {
        var op = BindAttributeAsync(primPaths, attributeName, dtype, semantic, primMode, flags);
        return op.WaitRequired();
    }

    /// <summary>Create a persistent attribute binding (asynchronous).</summary>
    public Operation<AttributeBinding> BindAttributeAsync(
        string[] primPaths,
        string attributeName,
        DLDataType dtype,
        Semantic semantic = Semantic.None,
        PrimMode primMode = PrimMode.ExistingOnly,
        BindingFlag flags = BindingFlag.None)
    {
        using var primsCtx = new NativeStringArrayContext(primPaths);
        using var attrNameCtx = new NativeStringContext(attributeName);

        var desc = new NativeBindingDesc
        {
            PrimList = new NativePrimList
            {
                PrimPaths = primsCtx.Pointer,
                NumPaths = primsCtx.Count,
            },
            AttributeName = new NativeStringOrToken { String = attrNameCtx.Value },
            AttributeType = new NativeAttributeType
            {
                DType = dtype,
                Semantic = (int)semantic,
            },
            PrimMode = (int)primMode,
            Flags = (int)flags,
        };

        var result = NativeMethods.ovrtx_create_attribute_binding(Handle, in desc, out var bindingHandle);
        OvrtxException.ThrowIfFailed(result, "create attribute binding");

        var binding = new AttributeBinding(this, new AttributeBindingHandle(bindingHandle));
        return new Operation<AttributeBinding>(this, new OpId(result.OpIndex), binding);
    }

    // ========================================================================
    // Attribute Mapping (Direct Buffer Access)
    // ========================================================================

    /// <summary>Map an attribute for direct memory access.</summary>
    public AttributeMapping MapAttribute(
        string[] primPaths,
        string attributeName,
        DLDataType dtype,
        Semantic semantic = Semantic.None,
        Device device = Device.Cpu,
        int deviceId = 0,
        PrimMode primMode = PrimMode.ExistingOnly)
    {
        using var primsCtx = new NativeStringArrayContext(primPaths);
        using var attrNameCtx = new NativeStringContext(attributeName);

        var bindingDesc = new NativeBindingDesc
        {
            PrimList = new NativePrimList
            {
                PrimPaths = primsCtx.Pointer,
                NumPaths = primsCtx.Count,
            },
            AttributeName = new NativeStringOrToken { String = attrNameCtx.Value },
            AttributeType = new NativeAttributeType
            {
                DType = dtype,
                Semantic = (int)semantic,
            },
            PrimMode = (int)primMode,
        };

        var binding = NativeBindingDescOrHandle.FromDesc(bindingDesc);
        var mappingDesc = new NativeMappingDesc
        {
            DeviceType = device == Device.Cuda ? (int)DLDeviceType.Cuda : (int)DLDeviceType.Cpu,
            DeviceId = deviceId,
        };

        var result = NativeMethods.ovrtx_map_attribute(Handle, in binding, mappingDesc, out var nativeMapping);
        OvrtxException.ThrowIfFailed(result, "map attribute");

        return new AttributeMapping(Handle, nativeMapping);
    }

    /// <summary>Unmap a previously mapped attribute (synchronous).</summary>
    public void UnmapAttribute(AttributeMapping mapping, nuint cudaEvent = 0, nuint cudaStream = 0)
    {
        mapping.Unmap(cudaEvent, cudaStream);
    }

    // ========================================================================
    // Logging
    // ========================================================================

    /// <summary>
    /// Set a log callback to receive operation log messages.
    /// Pass null to disable logging.
    /// </summary>
    public void SetLogCallback(LogSeverity minSeverity, LogCallback? callback, string? channel = null)
    {
        if (callback == null)
        {
            _userLogCallback = null;
            _nativeLogCallback = null;
            var result = NativeMethods.ovrtx_set_log_callback(
                Handle, (int)minSeverity, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            OvrtxException.ThrowIfFailed(result, "set log callback");
            return;
        }

        _userLogCallback = callback;
        _nativeLogCallback = (opId, severity, timestamp, message, userData) =>
        {
            try
            {
                _userLogCallback?.Invoke(
                    new OpId(opId),
                    (LogSeverity)severity,
                    timestamp,
                    message.ToManaged() ?? "");
            }
            catch
            {
                // Never throw back into native code
            }
        };

        var callbackPtr = Marshal.GetFunctionPointerForDelegate(_nativeLogCallback);

        if (channel != null)
        {
            using var channelCtx = new NativeStringContext(channel);
            unsafe
            {
                var channelValue = channelCtx.Value;
                var channelPtr = (IntPtr)(&channelValue);
                var res = NativeMethods.ovrtx_set_log_callback(
                    Handle, (int)minSeverity, channelPtr, callbackPtr, IntPtr.Zero);
                OvrtxException.ThrowIfFailed(res, "set log callback");
            }
        }
        else
        {
            var res = NativeMethods.ovrtx_set_log_callback(
                Handle, (int)minSeverity, IntPtr.Zero, callbackPtr, IntPtr.Zero);
            OvrtxException.ThrowIfFailed(res, "set log callback");
        }
    }

    /// <summary>Flush all pending log messages through the callback.</summary>
    public void FlushOpLog(ulong? timeoutNs = null)
    {
        var timeout = timeoutNs.HasValue
            ? new NativeTimeout(timeoutNs.Value)
            : NativeTimeout.Infinite;

        var result = NativeMethods.ovrtx_flush_op_log(Handle, timeout);
        OvrtxException.ThrowIfFailed(result, "flush op log");
    }

    // ========================================================================
    // Error Handling
    // ========================================================================

    /// <summary>Get the last error message from the current thread.</summary>
    public static string GetLastError()
    {
        return NativeMethods.ovrtx_get_last_error().ToManaged() ?? "";
    }

    // ========================================================================
    // Dispose
    // ========================================================================

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _userLogCallback = null;
        _nativeLogCallback = null;

        if (_handle != IntPtr.Zero)
        {
            NativeMethods.ovrtx_destroy_renderer(_handle);
            _handle = IntPtr.Zero;
        }
    }

    ~Renderer()
    {
        Dispose();
    }
}
