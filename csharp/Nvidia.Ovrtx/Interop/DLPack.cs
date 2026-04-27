// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.
//

using System.Runtime.InteropServices;

namespace Nvidia.Ovrtx.Interop;

/// <summary>DLPack data type code.</summary>
public enum DLDataTypeCode : byte
{
    Int = 0,
    UInt = 1,
    Float = 2,
    OpaqueHandle = 3,
    Bfloat = 4,
    Complex = 5,
    Bool = 6,
}

/// <summary>DLPack device type.</summary>
public enum DLDeviceType : int
{
    Cpu = 1,
    Cuda = 2,
    CudaHost = 3,
    OpenCL = 4,
    Vulkan = 7,
    Metal = 8,
    Vpi = 9,
    Rocm = 10,
    RocmHost = 11,
    ExtDev = 12,
    CudaManaged = 13,
    OneApi = 14,
    WebGpu = 15,
    Hexagon = 16,
}

/// <summary>Element data type descriptor (4 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
public struct DLDataType
{
    public DLDataTypeCode Code;
    public byte Bits;
    public ushort Lanes;

    public DLDataType(DLDataTypeCode code, byte bits, ushort lanes = 1)
    {
        Code = code;
        Bits = bits;
        Lanes = lanes;
    }

    public static readonly DLDataType Float32 = new(DLDataTypeCode.Float, 32);
    public static readonly DLDataType Float64 = new(DLDataTypeCode.Float, 64);
    public static readonly DLDataType Int32 = new(DLDataTypeCode.Int, 32);
    public static readonly DLDataType Int64 = new(DLDataTypeCode.Int, 64);
    public static readonly DLDataType UInt8 = new(DLDataTypeCode.UInt, 8);
    public static readonly DLDataType UInt64 = new(DLDataTypeCode.UInt, 64);
    public static readonly DLDataType Bool = new(DLDataTypeCode.Bool, 8);
}

/// <summary>Device descriptor (8 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
public struct DLDevice
{
    public DLDeviceType DeviceType;
    public int DeviceId;

    public DLDevice(DLDeviceType deviceType, int deviceId = 0)
    {
        DeviceType = deviceType;
        DeviceId = deviceId;
    }

    public static readonly DLDevice Cpu = new(DLDeviceType.Cpu);
    public static DLDevice Cuda(int deviceId = 0) => new(DLDeviceType.Cuda, deviceId);
}

/// <summary>Plain tensor descriptor (48 bytes on x64). No ownership semantics.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct DLTensor
{
    public IntPtr Data;
    public DLDevice Device;
    public int NDim;
    public DLDataType DType;
    public IntPtr Shape;    // int64_t*
    public IntPtr Strides;  // int64_t* (can be null)
    public ulong ByteOffset;

    public unsafe ReadOnlySpan<long> GetShape()
    {
        if (NDim <= 0 || Shape == IntPtr.Zero) return ReadOnlySpan<long>.Empty;
        return new ReadOnlySpan<long>((void*)Shape, NDim);
    }

    public unsafe ReadOnlySpan<long> GetStrides()
    {
        if (NDim <= 0 || Strides == IntPtr.Zero) return ReadOnlySpan<long>.Empty;
        return new ReadOnlySpan<long>((void*)Strides, NDim);
    }
}
