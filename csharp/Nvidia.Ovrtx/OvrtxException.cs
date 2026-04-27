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
