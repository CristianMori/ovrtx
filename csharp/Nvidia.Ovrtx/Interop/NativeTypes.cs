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

namespace Nvidia.Ovrtx.Interop;

/// <summary>ovrtx_result_t (4 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeResult
{
    public int Status; // NativeApiStatus

    public readonly bool IsSuccess => Status == (int)NativeApiStatus.Success;
    public readonly bool IsError => Status == (int)NativeApiStatus.Error;
    public readonly bool IsTimeout => Status == (int)NativeApiStatus.Timeout;
}

/// <summary>ovrtx_enqueue_result_t (16 bytes: 4 + 4pad + 8).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeEnqueueResult
{
    public int Status; // NativeApiStatus
    // 4 bytes padding implicit from alignment
    public ulong OpIndex; // ovrtx_op_id_t

    public readonly bool IsSuccess => Status == (int)NativeApiStatus.Success;
    public readonly bool IsError => Status == (int)NativeApiStatus.Error;
}

/// <summary>ovrtx_timeout_t (8 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeTimeout
{
    public ulong TimeOutNs;

    public NativeTimeout(ulong ns) => TimeOutNs = ns;

    public static readonly NativeTimeout Infinite = new(ulong.MaxValue);
    public static readonly NativeTimeout Zero = new(0);

    public static NativeTimeout FromMilliseconds(ulong ms) => new(ms * 1_000_000);
    public static NativeTimeout FromSeconds(double seconds) => new((ulong)(seconds * 1_000_000_000));
}

/// <summary>ovrtx_cuda_sync_t (16 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeCudaSync
{
    public nuint Stream;    // uintptr_t: 0=no sync, 1=default stream, >1=specific stream
    public nuint WaitEvent; // uintptr_t: 0=none

    public static readonly NativeCudaSync None = default;
}

/// <summary>ovrtx_op_wait_result_t (24 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeOpWaitResult
{
    public IntPtr ErrorOpIds;         // ovrtx_op_id_t*
    public nuint NumErrorOps;         // size_t
    public ulong LowestPendingOpId;   // ovrtx_op_id_t

    public readonly unsafe ReadOnlySpan<ulong> GetErrorOpIds()
    {
        if (NumErrorOps == 0 || ErrorOpIds == IntPtr.Zero) return ReadOnlySpan<ulong>.Empty;
        return new ReadOnlySpan<ulong>((void*)ErrorOpIds, (int)NumErrorOps);
    }
}

/// <summary>ovrtx_usd_input_t (40 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeUsdInput
{
    public NativeString UsdFilePath;      // ovx_string_t (16 bytes)
    public ulong UsdStageId;              // uint64_t (8 bytes)
    public NativeString UsdLayerContent;  // ovx_string_t (16 bytes)
}

/// <summary>ovrtx_render_product_set_t (16 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeRenderProductSet
{
    public IntPtr RenderProducts;     // const ovx_string_t*
    public nuint NumRenderProducts;   // size_t
}

/// <summary>ovx_string_or_token_t (24 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeStringOrToken
{
    public ulong Token;          // ovx_token_t (uint64_t)
    public NativeString String;  // ovx_string_t (16 bytes)
}

/// <summary>ovrtx_prim_list_t (16 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativePrimList
{
    public IntPtr PrimPaths;  // const ovx_string_t*
    public nuint NumPaths;    // size_t
}

/// <summary>ovrtx_attribute_type_t (12 bytes with padding).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeAttributeType
{
    public DLDataType DType;                // 4 bytes
    public byte IsArray;                    // bool (1 byte)
    // 3 bytes padding
    public int Semantic;                    // ovrtx_attribute_semantic_t (4 bytes)
}

/// <summary>ovrtx_binding_desc_t (~72 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeBindingDesc
{
    public NativePrimList PrimList;              // 16 bytes
    public ulong PrimsListHandle;                // ovx_primpath_list_t (8 bytes)
    public NativeStringOrToken AttributeName;    // 24 bytes
    public NativeAttributeType AttributeType;    // 12 bytes
    public int PrimMode;                         // ovrtx_binding_prim_mode_t (4 bytes)
    public int Flags;                            // ovrtx_binding_flag_t (4 bytes)
}

/// <summary>
/// ovrtx_binding_desc_or_handle_t.
/// NOT a union: both fields present. binding_handle wins if non-zero.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeBindingDescOrHandle
{
    public NativeBindingDesc BindingDesc;                // ~72 bytes
    public ulong BindingHandle;                          // ovrtx_attribute_binding_handle_t (8 bytes)

    public static NativeBindingDescOrHandle FromHandle(ulong handle) => new()
    {
        BindingHandle = handle,
    };

    public static NativeBindingDescOrHandle FromDesc(NativeBindingDesc desc) => new()
    {
        BindingDesc = desc,
        BindingHandle = 0,
    };
}

/// <summary>ovrtx_input_buffer_t (64 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeInputBuffer
{
    public IntPtr Tensors;               // DLTensor*
    public ulong TensorCount;            // uint64_t
    public IntPtr DirtyBits;             // uint8_t*
    public nuint DirtyBitsSize;          // size_t
    public NativeCudaSync AccessCudaSync;
    public NativeCudaSync DoneCudaSync;
}

/// <summary>ovrtx_output_buffer_t (64 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeOutputBuffer
{
    public DLTensor Dl;               // 48 bytes
    public NativeCudaSync CudaSync;   // 16 bytes
}

/// <summary>ovrtx_attribute_mapping_t (56 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeAttributeMapping
{
    public ulong MapHandle;   // ovrtx_map_handle_t (8 bytes)
    public DLTensor Dl;       // 48 bytes
}

/// <summary>ovrtx_mapping_desc_t (8 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeMappingDesc
{
    public int DeviceType;   // DLDevice device_type
    public int DeviceId;     // DLDevice device_id
}

/// <summary>ovrtx_map_output_description_t (16 bytes with padding).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeMapOutputDescription
{
    public int DeviceType;      // ovrtx_map_device_type_t (4 bytes)
    // 4 bytes padding
    public nuint SyncStream;    // uintptr_t (8 bytes)
}

/// <summary>ovrtx_render_product_render_var_output_t (24 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeRenderVarOutput
{
    public NativeString RenderVarName;    // ovx_string_t (16 bytes)
    public ulong OutputHandle;            // ovrtx_rendered_output_handle_t (8 bytes)
}

/// <summary>ovrtx_render_product_frame_output_t (32 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeFrameOutput
{
    public double FrameStartTime;
    public double FrameEndTime;
    public IntPtr OutputRenderVars;    // ovrtx_render_product_render_var_output_t*
    public nuint RenderVarCount;       // size_t
}

/// <summary>ovrtx_render_product_output_t (~40 bytes with padding).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeProductOutput
{
    public NativeString RenderProductPath;   // ovx_string_t (16 bytes)
    public float OutputFramesProduced;       // float (4 bytes)
    // 4 bytes padding before pointer
    public IntPtr OutputFrames;              // ovrtx_render_product_frame_output_t*
    public nuint OutputFrameCount;           // size_t
}

/// <summary>ovrtx_render_product_set_outputs_t (~72 bytes with padding).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeRenderProductSetOutputs
{
    public int Status;                        // ovrtx_event_status_t (4 bytes)
    // 4 bytes padding
    public NativeString ErrorMessage;         // ovx_string_t (16 bytes)
    public double SimulationStartTime;
    public double SimulationEndTime;
    public IntPtr Outputs;                    // ovrtx_render_product_output_t*
    public nuint OutputCount;                 // size_t
    public double StartTime;
    public double EndTime;
}

/// <summary>ovrtx_rendered_output_t (~112 bytes with padding).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeRenderedOutput
{
    public int Status;                        // ovrtx_event_status_t (4 bytes)
    // 4 bytes padding
    public NativeString ErrorMessage;         // ovx_string_t (16 bytes)
    public ulong MapHandle;                   // ovrtx_rendered_output_map_handle_t (8 bytes)
    public NativeString Name;                 // ovx_string_t (16 bytes)
    public NativeOutputBuffer Buffer;         // ovrtx_output_buffer_t (64 bytes)
}

/// <summary>ovrtx_op_counter_t (32 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeOpCounter
{
    public NativeString Name;    // ovx_string_t (16 bytes)
    public ulong Current;        // uint64_t
    public ulong Total;          // uint64_t
}

/// <summary>ovrtx_op_status_t (~40 bytes with padding).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeOpStatus
{
    public ulong OpId;           // ovrtx_op_id_t (8 bytes)
    public int State;            // ovrtx_event_status_t (4 bytes)
    // 4 bytes padding
    public double Progress;      // double (8 bytes)
    public IntPtr Counters;      // ovrtx_op_counter_t*
    public nuint CounterCount;   // size_t
}
