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
/// A mapped render variable output providing access to rendered pixel data.
/// Dispose (or call Unmap) when done reading the data.
/// </summary>
public sealed class MappedRenderVar : IDisposable
{
    private readonly IntPtr _rendererHandle;
    private bool _unmapped;

    internal MappedRenderVar(IntPtr rendererHandle, NativeRenderedOutput output)
    {
        _rendererHandle = rendererHandle;
        MapHandle = new RenderedOutputMapHandle(output.MapHandle);
        Name = output.Name.ToManaged() ?? "";
        Tensor = output.Buffer.Dl;
        CudaSync = (output.Buffer.CudaSync.Stream, output.Buffer.CudaSync.WaitEvent);
    }

    public RenderedOutputMapHandle MapHandle { get; }
    public string Name { get; }
    public DLTensor Tensor { get; }
    /// <summary>CUDA synchronization info (stream + wait event) for the mapped output.</summary>
    public (nuint Stream, nuint WaitEvent) CudaSync { get; }

    /// <summary>Unmap with optional CUDA synchronization before the buffer is released.</summary>
    public void Unmap(nuint cudaEvent = 0, nuint cudaStream = 0)
    {
        if (_unmapped) return;
        _unmapped = true;

        var sync = new NativeCudaSync { Stream = cudaStream, WaitEvent = cudaEvent };
        var result = NativeMethods.ovrtx_unmap_rendered_output(_rendererHandle, MapHandle.Value, sync);
        OvrtxException.ThrowIfFailed(result, "unmap rendered output");
    }

    public void Dispose()
    {
        if (!_unmapped)
        {
            try { Unmap(); }
            catch { /* suppress in dispose */ }
        }
    }
}

/// <summary>Single render variable output from a frame. Call Map() to access pixel data.</summary>
public sealed class RenderVarOutput
{
    private readonly IntPtr _rendererHandle;

    internal RenderVarOutput(IntPtr rendererHandle, string name, RenderedOutputHandle handle)
    {
        _rendererHandle = rendererHandle;
        Name = name;
        Handle = handle;
    }

    public string Name { get; }
    public RenderedOutputHandle Handle { get; }

    /// <summary>
    /// Map the rendered output to access pixel data. Dispose the result when done.
    /// </summary>
    public MappedRenderVar Map(MapDeviceType device = MapDeviceType.Cpu, nuint syncStream = 0, ulong? timeoutNs = null)
    {
        var mapDesc = new NativeMapOutputDescription
        {
            DeviceType = (int)device,
            SyncStream = syncStream,
        };

        var timeout = timeoutNs.HasValue
            ? new NativeTimeout(timeoutNs.Value)
            : NativeTimeout.Infinite;

        var result = NativeMethods.ovrtx_map_rendered_output(
            _rendererHandle, Handle.Value, in mapDesc, timeout, out var renderedOutput);

        if (result.IsTimeout)
            throw new OvrtxException("Timed out waiting for rendered output", ApiStatus.Timeout);

        OvrtxException.ThrowIfFailed(result, "map rendered output");

        if (renderedOutput.Status == (int)NativeEventStatus.Failure)
        {
            var errorMsg = renderedOutput.ErrorMessage.ToManaged() ?? "Unknown error";
            throw new OvrtxException($"Rendered output failed: {errorMsg}");
        }

        return new MappedRenderVar(_rendererHandle, renderedOutput);
    }
}

/// <summary>A single frame's output from a render product.</summary>
public sealed class FrameOutput
{
    public double StartTime { get; }
    public double EndTime { get; }
    public IReadOnlyDictionary<string, RenderVarOutput> RenderVars { get; }

    internal FrameOutput(double startTime, double endTime, IReadOnlyDictionary<string, RenderVarOutput> renderVars)
    {
        StartTime = startTime;
        EndTime = endTime;
        RenderVars = renderVars;
    }
}

/// <summary>Output from a single render product for a step operation.</summary>
public sealed class ProductOutput
{
    public string Name { get; }
    public IReadOnlyList<FrameOutput> Frames { get; }

    internal ProductOutput(string name, IReadOnlyList<FrameOutput> frames)
    {
        Name = name;
        Frames = frames;
    }
}

/// <summary>
/// Complete output from a step operation. Dispose when done to release native resources.
/// Access individual products by path name.
/// </summary>
public sealed class RenderProductSetOutputs : IDisposable
{
    private readonly IntPtr _rendererHandle;
    private readonly ulong _stepResultHandle;
    private bool _destroyed;

    public IReadOnlyDictionary<string, ProductOutput> Products { get; }
    public double SimulationStartTime { get; }
    public double SimulationEndTime { get; }

    internal RenderProductSetOutputs(
        IntPtr rendererHandle,
        ulong stepResultHandle,
        NativeRenderProductSetOutputs nativeOutputs)
    {
        _rendererHandle = rendererHandle;
        _stepResultHandle = stepResultHandle;
        SimulationStartTime = nativeOutputs.SimulationStartTime;
        SimulationEndTime = nativeOutputs.SimulationEndTime;

        // Parse native output structures into managed types
        var products = new Dictionary<string, ProductOutput>();
        var outputCount = (int)nativeOutputs.OutputCount;

        for (int i = 0; i < outputCount; i++)
        {
            var nativeProduct = Marshal.PtrToStructure<NativeProductOutput>(
                nativeOutputs.Outputs + i * Marshal.SizeOf<NativeProductOutput>());

            var productPath = nativeProduct.RenderProductPath.ToManaged() ?? "";
            var frameCount = (int)nativeProduct.OutputFrameCount;
            var frames = new List<FrameOutput>(frameCount);

            for (int f = 0; f < frameCount; f++)
            {
                var nativeFrame = Marshal.PtrToStructure<NativeFrameOutput>(
                    nativeProduct.OutputFrames + f * Marshal.SizeOf<NativeFrameOutput>());

                var varCount = (int)nativeFrame.RenderVarCount;
                var renderVars = new Dictionary<string, RenderVarOutput>(varCount);

                for (int v = 0; v < varCount; v++)
                {
                    var nativeVar = Marshal.PtrToStructure<NativeRenderVarOutput>(
                        nativeFrame.OutputRenderVars + v * Marshal.SizeOf<NativeRenderVarOutput>());

                    var varName = nativeVar.RenderVarName.ToManaged() ?? "";
                    renderVars[varName] = new RenderVarOutput(
                        rendererHandle, varName, new RenderedOutputHandle(nativeVar.OutputHandle));
                }

                frames.Add(new FrameOutput(nativeFrame.FrameStartTime, nativeFrame.FrameEndTime, renderVars));
            }

            products[productPath] = new ProductOutput(productPath, frames);
        }

        Products = products;
    }

    public ProductOutput this[string renderProductPath] => Products[renderProductPath];

    public void Dispose()
    {
        if (_destroyed) return;
        _destroyed = true;

        NativeMethods.ovrtx_destroy_results(_rendererHandle, _stepResultHandle);
    }
}

/// <summary>
/// Two-stage async result from StepAsync. Allows separate timeout control for
/// the step operation and the fetch operation.
/// </summary>
public sealed class RendererResult
{
    private readonly WeakReference<Renderer> _rendererRef;
    private readonly OpId _opId;
    private readonly ulong _stepResultHandle;
    private RenderProductSetOutputs? _cachedOutputs;
    private bool _stepComplete;

    internal RendererResult(Renderer renderer, OpId opId, ulong stepResultHandle)
    {
        _rendererRef = new WeakReference<Renderer>(renderer);
        _opId = opId;
        _stepResultHandle = stepResultHandle;
    }

    /// <summary>True if the step operation has completed (fetch may still be pending).</summary>
    public bool StepComplete => _stepComplete;

    /// <summary>
    /// Wait for both the step and fetch to complete.
    /// </summary>
    /// <returns>The outputs, or null if either stage timed out.</returns>
    public RenderProductSetOutputs? Wait(ulong? stepTimeoutNs = null, ulong? fetchTimeoutNs = null)
    {
        if (_cachedOutputs != null) return _cachedOutputs;

        if (!_rendererRef.TryGetTarget(out var renderer))
            throw new ObjectDisposedException(nameof(Renderer));

        // Stage 1: Wait for step to complete
        if (!_stepComplete)
        {
            var stepTimeout = stepTimeoutNs.HasValue
                ? new NativeTimeout(stepTimeoutNs.Value)
                : NativeTimeout.Infinite;

            var waitResult = NativeMethods.ovrtx_wait_op(
                renderer.Handle, _opId.Value, stepTimeout, out var opWaitResult);

            if (waitResult.IsTimeout) return null;
            OvrtxException.ThrowIfFailed(waitResult, "wait for step");

            var errorOps = opWaitResult.GetErrorOpIds();
            if (errorOps.Length > 0)
            {
                var error = NativeMethods.ovrtx_get_last_op_error(errorOps[0]);
                throw new OvrtxException($"Step failed: {error.ToManaged() ?? "Unknown error"}");
            }

            _stepComplete = true;
        }

        // Stage 2: Fetch results
        var fetchTimeout = fetchTimeoutNs.HasValue
            ? new NativeTimeout(fetchTimeoutNs.Value)
            : NativeTimeout.Infinite;

        var fetchResult = NativeMethods.ovrtx_fetch_results(
            renderer.Handle, _stepResultHandle, fetchTimeout, out var nativeOutputs);

        if (fetchResult.IsTimeout) return null;
        OvrtxException.ThrowIfFailed(fetchResult, "fetch step results");

        if (nativeOutputs.Status == (int)NativeEventStatus.Failure)
        {
            var errorMsg = nativeOutputs.ErrorMessage.ToManaged() ?? "Unknown error";
            throw new OvrtxException($"Step results failed: {errorMsg}");
        }

        _cachedOutputs = new RenderProductSetOutputs(renderer.Handle, _stepResultHandle, nativeOutputs);
        return _cachedOutputs;
    }
}
