// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Nvidia.Ovrtx.Interop;
using Xunit;

namespace Nvidia.Ovrtx.Tests;

/// <summary>
/// Validates that public and internal enum values match the C API constants.
/// </summary>
public class EnumTests
{
    // ========================================================================
    // Public enums
    // ========================================================================

    [Theory]
    [InlineData(ApiStatus.Success, 0)]
    [InlineData(ApiStatus.Error, 1)]
    [InlineData(ApiStatus.Timeout, 2)]
    public void ApiStatus_ValuesMatchC(ApiStatus value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Theory]
    [InlineData(EventStatus.Pending, 0)]
    [InlineData(EventStatus.Completed, 1)]
    [InlineData(EventStatus.Failed, 2)]
    public void EventStatus_ValuesMatchC(EventStatus value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Theory]
    [InlineData(Semantic.None, 0)]
    [InlineData(Semantic.XformMat4x4, 1)]
    [InlineData(Semantic.XformPosRotScale, 2)]
    [InlineData(Semantic.XformPosRot3x3, 3)]
    [InlineData(Semantic.PathString, 4)]
    [InlineData(Semantic.TokenString, 5)]
    public void Semantic_ValuesMatchC(Semantic value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Theory]
    [InlineData(PrimMode.ExistingOnly, 0)]
    [InlineData(PrimMode.MustExist, 1)]
    [InlineData(PrimMode.CreateNew, 2)]
    public void PrimMode_ValuesMatchC(PrimMode value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Theory]
    [InlineData(DataAccess.Async, 0)]
    [InlineData(DataAccess.Sync, 1)]
    public void DataAccess_ValuesMatchC(DataAccess value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Theory]
    [InlineData(MapDeviceType.Default, 0)]
    [InlineData(MapDeviceType.Cpu, 1)]
    [InlineData(MapDeviceType.Cuda, 2)]
    [InlineData(MapDeviceType.CudaArray, 3)]
    public void MapDeviceType_ValuesMatchC(MapDeviceType value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Theory]
    [InlineData(LogSeverity.Info, 0)]
    [InlineData(LogSeverity.Warning, 1)]
    [InlineData(LogSeverity.Error, 2)]
    public void LogSeverity_ValuesMatchC(LogSeverity value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Fact]
    public void BindingFlag_None_IsZero()
    {
        Assert.Equal(0, (int)BindingFlag.None);
    }

    [Fact]
    public void BindingFlag_Optimize_IsBit0()
    {
        Assert.Equal(1, (int)BindingFlag.Optimize);
    }

    // ========================================================================
    // Internal enums (match C header constants)
    // ========================================================================

    [Fact]
    public void NativeConfigKeyType_ValuesMatchC()
    {
        Assert.Equal(0, (int)NativeConfigKeyType.Bool);
        Assert.Equal(1, (int)NativeConfigKeyType.Int64);
        Assert.Equal(2, (int)NativeConfigKeyType.UInt64);
        Assert.Equal(3, (int)NativeConfigKeyType.Double);
        Assert.Equal(4, (int)NativeConfigKeyType.String);
        Assert.Equal(5, (int)NativeConfigKeyType.Blob);
    }

    [Fact]
    public void NativeConfigBoolKey_ValuesMatchC()
    {
        Assert.Equal(0, (int)NativeConfigBoolKey.SyncMode);
        Assert.Equal(1, (int)NativeConfigBoolKey.EnableProfiling);
        Assert.Equal(2, (int)NativeConfigBoolKey.ReadGpuTransforms);
        Assert.Equal(3, (int)NativeConfigBoolKey.OutputPartialFrames);
        Assert.Equal(4, (int)NativeConfigBoolKey.KeepSystemAlive);
        Assert.Equal(5, (int)NativeConfigBoolKey.UseVulkan);
    }

    [Fact]
    public void NativeConfigStringKey_ValuesMatchC()
    {
        Assert.Equal(0, (int)NativeConfigStringKey.BinaryPackageRootPath);
        Assert.Equal(1, (int)NativeConfigStringKey.LogFilePath);
        Assert.Equal(2, (int)NativeConfigStringKey.LogLevel);
        Assert.Equal(3, (int)NativeConfigStringKey.ActiveCudaGpus);
    }

    [Fact]
    public void NativeAttributeSemantic_ValuesMatchC()
    {
        Assert.Equal(0, (int)NativeAttributeSemantic.None);
        Assert.Equal(1, (int)NativeAttributeSemantic.XformMat4x4);
        Assert.Equal(2, (int)NativeAttributeSemantic.XformPos3dRot4fScale3f);
        Assert.Equal(3, (int)NativeAttributeSemantic.XformPos3dRot3x3f);
        Assert.Equal(4, (int)NativeAttributeSemantic.PathString);
        Assert.Equal(5, (int)NativeAttributeSemantic.TokenString);
    }

    // ========================================================================
    // DLPack enums
    // ========================================================================

    [Theory]
    [InlineData(DLDataTypeCode.Int, 0)]
    [InlineData(DLDataTypeCode.UInt, 1)]
    [InlineData(DLDataTypeCode.Float, 2)]
    [InlineData(DLDataTypeCode.OpaqueHandle, 3)]
    [InlineData(DLDataTypeCode.Bfloat, 4)]
    [InlineData(DLDataTypeCode.Complex, 5)]
    [InlineData(DLDataTypeCode.Bool, 6)]
    public void DLDataTypeCode_ValuesMatchC(DLDataTypeCode value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Theory]
    [InlineData(DLDeviceType.Cpu, 1)]
    [InlineData(DLDeviceType.Cuda, 2)]
    [InlineData(DLDeviceType.CudaHost, 3)]
    [InlineData(DLDeviceType.Vulkan, 7)]
    [InlineData(DLDeviceType.Metal, 8)]
    public void DLDeviceType_ValuesMatchC(DLDeviceType value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }
}
