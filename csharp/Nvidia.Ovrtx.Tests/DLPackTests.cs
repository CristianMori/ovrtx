// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Nvidia.Ovrtx.Interop;
using Xunit;

namespace Nvidia.Ovrtx.Tests;

/// <summary>
/// Tests for DLPack type constructors and predefined constants.
/// </summary>
public class DLPackTests
{
    [Fact]
    public void DLDataType_Float32_IsCorrect()
    {
        var dt = DLDataType.Float32;
        Assert.Equal(DLDataTypeCode.Float, dt.Code);
        Assert.Equal(32, dt.Bits);
        Assert.Equal(1, dt.Lanes);
    }

    [Fact]
    public void DLDataType_Float64_IsCorrect()
    {
        var dt = DLDataType.Float64;
        Assert.Equal(DLDataTypeCode.Float, dt.Code);
        Assert.Equal(64, dt.Bits);
        Assert.Equal(1, dt.Lanes);
    }

    [Fact]
    public void DLDataType_Int32_IsCorrect()
    {
        var dt = DLDataType.Int32;
        Assert.Equal(DLDataTypeCode.Int, dt.Code);
        Assert.Equal(32, dt.Bits);
        Assert.Equal(1, dt.Lanes);
    }

    [Fact]
    public void DLDataType_UInt8_IsCorrect()
    {
        var dt = DLDataType.UInt8;
        Assert.Equal(DLDataTypeCode.UInt, dt.Code);
        Assert.Equal(8, dt.Bits);
        Assert.Equal(1, dt.Lanes);
    }

    [Fact]
    public void DLDataType_CustomLanes_Works()
    {
        var dt = new DLDataType(DLDataTypeCode.Float, 64, 16);
        Assert.Equal(DLDataTypeCode.Float, dt.Code);
        Assert.Equal(64, dt.Bits);
        Assert.Equal(16, dt.Lanes);
    }

    [Fact]
    public void DLDevice_Cpu_IsCorrect()
    {
        var dev = DLDevice.Cpu;
        Assert.Equal(DLDeviceType.Cpu, dev.DeviceType);
        Assert.Equal(0, dev.DeviceId);
    }

    [Fact]
    public void DLDevice_Cuda_WithDeviceId()
    {
        var dev = DLDevice.Cuda(2);
        Assert.Equal(DLDeviceType.Cuda, dev.DeviceType);
        Assert.Equal(2, dev.DeviceId);
    }

    [Fact]
    public void DLDevice_Cuda_DefaultDeviceId_IsZero()
    {
        var dev = DLDevice.Cuda();
        Assert.Equal(0, dev.DeviceId);
    }

    [Fact]
    public unsafe void DLTensor_GetShape_EmptyWhenNdimZero()
    {
        var tensor = new DLTensor { NDim = 0, Shape = IntPtr.Zero };
        Assert.True(tensor.GetShape().IsEmpty);
    }

    [Fact]
    public unsafe void DLTensor_GetShape_ReturnsCorrectValues()
    {
        long[] shape = [1080, 1920, 4];
        fixed (long* shapePtr = shape)
        {
            var tensor = new DLTensor
            {
                NDim = 3,
                Shape = (IntPtr)shapePtr,
            };
            var result = tensor.GetShape();
            Assert.Equal(3, result.Length);
            Assert.Equal(1080, result[0]);
            Assert.Equal(1920, result[1]);
            Assert.Equal(4, result[2]);
        }
    }

    [Fact]
    public unsafe void DLTensor_GetStrides_EmptyWhenNull()
    {
        var tensor = new DLTensor { NDim = 3, Strides = IntPtr.Zero };
        Assert.True(tensor.GetStrides().IsEmpty);
    }

    [Fact]
    public unsafe void DLTensor_GetStrides_ReturnsCorrectValues()
    {
        long[] strides = [7680, 4, 1];
        fixed (long* stridesPtr = strides)
        {
            var tensor = new DLTensor
            {
                NDim = 3,
                Strides = (IntPtr)stridesPtr,
            };
            var result = tensor.GetStrides();
            Assert.Equal(3, result.Length);
            Assert.Equal(7680, result[0]);
        }
    }
}
