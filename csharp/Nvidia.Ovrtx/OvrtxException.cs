// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.
//

using Nvidia.Ovrtx.Interop;

namespace Nvidia.Ovrtx;

public class OvrtxException : Exception
{
    public ApiStatus Status { get; }

    public OvrtxException(string message, ApiStatus status = ApiStatus.Error)
        : base(message)
    {
        Status = status;
    }

    public OvrtxException(string message, Exception innerException, ApiStatus status = ApiStatus.Error)
        : base(message, innerException)
    {
        Status = status;
    }

    internal static OvrtxException FromLastError(string operation)
    {
        var nativeError = NativeMethods.ovrtx_get_last_error();
        var errorMsg = nativeError.ToManaged() ?? "Unknown error";
        return new OvrtxException($"Failed to {operation}: {errorMsg}");
    }

    internal static OvrtxException FromOpError(string operation, ulong opId)
    {
        var nativeError = NativeMethods.ovrtx_get_last_op_error(opId);
        var errorMsg = nativeError.ToManaged() ?? "Unknown error";
        return new OvrtxException($"Operation {opId} failed during {operation}: {errorMsg}");
    }

    internal static void ThrowIfFailed(NativeResult result, string operation)
    {
        if (result.IsError)
            throw FromLastError(operation);
    }

    internal static void ThrowIfFailed(NativeEnqueueResult result, string operation)
    {
        if (result.IsError)
            throw FromLastError(operation);
    }
}
