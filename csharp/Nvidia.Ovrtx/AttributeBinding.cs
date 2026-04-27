// SPDX-FileCopyrightText: Copyright (c) 2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: LicenseRef-NvidiaProprietary
//
// NVIDIA CORPORATION, its affiliates and licensors retain all intellectual
// property and proprietary rights in and to this material, related
// documentation and any modifications thereto. Any use, reproduction,
// disclosure or distribution of this material and related documentation
// without an express license agreement from NVIDIA CORPORATION or
// its affiliates is strictly prohibited.

using Nvidia.Ovrtx.Interop;

namespace Nvidia.Ovrtx;

/// <summary>
/// Mapped attribute buffer for direct memory writes.
/// Changes are applied to the stage when Unmap() is called or on Dispose.
/// </summary>
public sealed class AttributeMapping : IDisposable
{
    private readonly IntPtr _rendererHandle;
    private bool _unmapped;

    internal AttributeMapping(IntPtr rendererHandle, NativeAttributeMapping nativeMapping)
    {
        _rendererHandle = rendererHandle;
        MapHandle = new MapHandle(nativeMapping.MapHandle);
        Tensor = nativeMapping.Dl;
    }

    public MapHandle MapHandle { get; }

    /// <summary>The mapped DLTensor providing direct access to the attribute buffer.</summary>
    public DLTensor Tensor { get; }

    /// <summary>
    /// Unmap the attribute buffer and apply changes to the stage.
    /// </summary>
    /// <param name="cudaEvent">Optional CUDA event to wait for before accessing the data.</param>
    /// <param name="cudaStream">Optional CUDA stream for synchronization.</param>
    public void Unmap(nuint cudaEvent = 0, nuint cudaStream = 0)
    {
        if (_unmapped) return;
        _unmapped = true;

        var sync = new NativeCudaSync { Stream = cudaStream, WaitEvent = cudaEvent };
        var result = NativeMethods.ovrtx_unmap_attribute(_rendererHandle, MapHandle.Value, sync);
        OvrtxException.ThrowIfFailed(result, "unmap attribute");
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

/// <summary>
/// A persistent attribute binding for efficient repeated writes to the same attribute on the same prims.
/// Dispose when no longer needed.
/// </summary>
public sealed class AttributeBinding : IDisposable
{
    private readonly WeakReference<Renderer> _rendererRef;
    private bool _disposed;

    internal AttributeBinding(Renderer renderer, AttributeBindingHandle handle)
    {
        _rendererRef = new WeakReference<Renderer>(renderer);
        Handle = handle;
    }

    public AttributeBindingHandle Handle { get; }

    /// <summary>
    /// Write data through this binding (synchronous).
    /// </summary>
    /// <param name="tensor">Pointer to a pinned DLTensor describing the data.</param>
    /// <param name="dataAccess">SYNC copies immediately; ASYNC requires caller to keep data alive.</param>
    /// <param name="dirtyBits">Optional dirty bit array (1 bit per prim).</param>
    /// <param name="cudaStream">Optional CUDA stream for async access synchronization.</param>
    /// <param name="cudaEvent">Optional CUDA event for async access synchronization.</param>
    public unsafe void Write(
        DLTensor* tensor,
        DataAccess dataAccess = DataAccess.Sync,
        byte[]? dirtyBits = null,
        nuint cudaStream = 0,
        nuint cudaEvent = 0)
    {
        var op = WriteAsync(tensor, dataAccess, dirtyBits, cudaStream, cudaEvent);
        op.Wait();
    }

    /// <summary>
    /// Write data through this binding (asynchronous).
    /// </summary>
    public unsafe Operation WriteAsync(
        DLTensor* tensor,
        DataAccess dataAccess = DataAccess.Sync,
        byte[]? dirtyBits = null,
        nuint cudaStream = 0,
        nuint cudaEvent = 0)
    {
        if (!_rendererRef.TryGetTarget(out var renderer))
            throw new ObjectDisposedException(nameof(Renderer));

        var binding = NativeBindingDescOrHandle.FromHandle(Handle.Value);
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
                    renderer.Handle, in binding, in inputBuffer, (int)dataAccess);
                OvrtxException.ThrowIfFailed(result, "write attribute via binding");
                return new Operation(renderer, new OpId(result.OpIndex));
            }
        }
        else
        {
            var result = NativeMethods.ovrtx_write_attribute(
                renderer.Handle, in binding, in inputBuffer, (int)dataAccess);
            OvrtxException.ThrowIfFailed(result, "write attribute via binding");
            return new Operation(renderer, new OpId(result.OpIndex));
        }
    }

    /// <summary>
    /// Map this binding's attribute for direct memory access.
    /// </summary>
    public AttributeMapping Map(Device device = Device.Cpu, int deviceId = 0)
    {
        if (!_rendererRef.TryGetTarget(out var renderer))
            throw new ObjectDisposedException(nameof(Renderer));

        var binding = NativeBindingDescOrHandle.FromHandle(Handle.Value);
        var mappingDesc = new NativeMappingDesc
        {
            DeviceType = device == Device.Cuda ? (int)DLDeviceType.Cuda : (int)DLDeviceType.Cpu,
            DeviceId = deviceId,
        };

        var result = NativeMethods.ovrtx_map_attribute(
            renderer.Handle, in binding, mappingDesc, out var nativeMapping);
        OvrtxException.ThrowIfFailed(result, "map attribute via binding");

        return new AttributeMapping(renderer.Handle, nativeMapping);
    }

    /// <summary>Destroy the persistent binding.</summary>
    public void Unbind()
    {
        if (_disposed) return;
        _disposed = true;

        if (!_rendererRef.TryGetTarget(out var renderer)) return;

        var result = NativeMethods.ovrtx_destroy_attribute_binding(renderer.Handle, Handle.Value);
        OvrtxException.ThrowIfFailed(result, "destroy attribute binding");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            try { Unbind(); }
            catch { /* suppress in dispose */ }
        }
    }
}
