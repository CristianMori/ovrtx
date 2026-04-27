// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.
//

using Nvidia.Ovrtx.Interop;

namespace Nvidia.Ovrtx;

/// <summary>
/// Represents an in-flight asynchronous ovrtx operation that produces no result value.
/// </summary>
public class Operation
{
    private readonly WeakReference<Renderer> _rendererRef;
    private bool _completed;

    internal Operation(Renderer renderer, OpId opId)
    {
        _rendererRef = new WeakReference<Renderer>(renderer);
        OpId = opId;
    }

    public OpId OpId { get; }

    /// <summary>
    /// Wait for the operation to complete.
    /// </summary>
    /// <param name="timeoutNs">Timeout in nanoseconds. null = infinite, 0 = poll.</param>
    /// <returns>True if completed, false if timed out.</returns>
    /// <exception cref="OvrtxException">If the operation failed.</exception>
    public bool Wait(ulong? timeoutNs = null)
    {
        if (_completed) return true;

        if (!_rendererRef.TryGetTarget(out var renderer))
            throw new ObjectDisposedException(nameof(Renderer));

        var timeout = timeoutNs.HasValue
            ? new NativeTimeout(timeoutNs.Value)
            : NativeTimeout.Infinite;

        var result = NativeMethods.ovrtx_wait_op(
            renderer.Handle, OpId.Value, timeout, out var waitResult);

        if (result.IsTimeout)
            return false;

        OvrtxException.ThrowIfFailed(result, "wait for operation");

        // Check for operation-level errors
        var errorOps = waitResult.GetErrorOpIds();
        if (errorOps.Length > 0)
        {
            var errors = new List<string>();
            foreach (var errorOpId in errorOps)
            {
                var opError = NativeMethods.ovrtx_get_last_op_error(errorOpId);
                errors.Add($"op {errorOpId}: {opError.ToManaged() ?? "Unknown error"}");
            }
            throw new OvrtxException($"Operation(s) failed: {string.Join("; ", errors)}");
        }

        _completed = true;
        return true;
    }
}

/// <summary>
/// Represents an in-flight asynchronous ovrtx operation that produces a result of type T.
/// </summary>
public class Operation<T>
{
    private readonly WeakReference<Renderer> _rendererRef;
    private readonly T _handle;
    private bool _completed;

    internal Operation(Renderer renderer, OpId opId, T handle)
    {
        _rendererRef = new WeakReference<Renderer>(renderer);
        OpId = opId;
        _handle = handle;
    }

    public OpId OpId { get; }

    /// <summary>
    /// Wait for the operation to complete and return the result handle.
    /// </summary>
    /// <param name="timeoutNs">Timeout in nanoseconds. null = infinite, 0 = poll.</param>
    /// <returns>The result handle, or null if timed out.</returns>
    /// <exception cref="OvrtxException">If the operation failed.</exception>
    public T? Wait(ulong? timeoutNs = null)
    {
        if (_completed) return _handle;

        if (!_rendererRef.TryGetTarget(out var renderer))
            throw new ObjectDisposedException(nameof(Renderer));

        var timeout = timeoutNs.HasValue
            ? new NativeTimeout(timeoutNs.Value)
            : NativeTimeout.Infinite;

        var result = NativeMethods.ovrtx_wait_op(
            renderer.Handle, OpId.Value, timeout, out var waitResult);

        if (result.IsTimeout)
            return default;

        OvrtxException.ThrowIfFailed(result, "wait for operation");

        var errorOps = waitResult.GetErrorOpIds();
        if (errorOps.Length > 0)
        {
            var errors = new List<string>();
            foreach (var errorOpId in errorOps)
            {
                var opError = NativeMethods.ovrtx_get_last_op_error(errorOpId);
                errors.Add($"op {errorOpId}: {opError.ToManaged() ?? "Unknown error"}");
            }
            throw new OvrtxException($"Operation(s) failed: {string.Join("; ", errors)}");
        }

        _completed = true;
        return _handle;
    }

    /// <summary>True if the operation has completed (result is cached).</summary>
    public bool IsCompleted => _completed;

    /// <summary>
    /// Wait with infinite timeout and return the result. Throws on error.
    /// Used by sync wrappers that cannot tolerate timeout.
    /// </summary>
    internal T WaitRequired()
    {
        Wait(); // infinite timeout, throws on error
        return _handle;
    }
}
