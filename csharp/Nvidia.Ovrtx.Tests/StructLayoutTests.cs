// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using System.Runtime.InteropServices;
using Nvidia.Ovrtx.Interop;
using Xunit;

namespace Nvidia.Ovrtx.Tests;

/// <summary>
/// Validates that all native struct layouts match the expected C struct sizes.
/// These tests catch P/Invoke marshaling bugs without requiring the native library.
/// </summary>
public class StructLayoutTests
{
    // ========================================================================
    // DLPack types
    // ========================================================================

    [Fact]
    public void DLDataType_Is4Bytes()
    {
        Assert.Equal(4, Marshal.SizeOf<DLDataType>());
    }

    [Fact]
    public void DLDevice_Is8Bytes()
    {
        Assert.Equal(8, Marshal.SizeOf<DLDevice>());
    }

    [Fact]
    public void DLTensor_Is48Bytes()
    {
        Assert.Equal(48, Marshal.SizeOf<DLTensor>());
    }

    // ========================================================================
    // Core result types
    // ========================================================================

    [Fact]
    public void NativeResult_Is4Bytes()
    {
        Assert.Equal(4, Marshal.SizeOf<NativeResult>());
    }

    [Fact]
    public void NativeEnqueueResult_Is16Bytes()
    {
        // 4 (status) + 4 (padding) + 8 (op_index) = 16
        Assert.Equal(16, Marshal.SizeOf<NativeEnqueueResult>());
    }

    [Fact]
    public void NativeTimeout_Is8Bytes()
    {
        Assert.Equal(8, Marshal.SizeOf<NativeTimeout>());
    }

    [Fact]
    public void NativeCudaSync_Is16Bytes()
    {
        Assert.Equal(16, Marshal.SizeOf<NativeCudaSync>());
    }

    // ========================================================================
    // String and collection types
    // ========================================================================

    [Fact]
    public void NativeString_Is16Bytes()
    {
        Assert.Equal(16, Marshal.SizeOf<NativeString>());
    }

    [Fact]
    public void NativeStringOrToken_Is24Bytes()
    {
        // 8 (token) + 16 (string) = 24
        Assert.Equal(24, Marshal.SizeOf<NativeStringOrToken>());
    }

    [Fact]
    public void NativePrimList_Is16Bytes()
    {
        Assert.Equal(16, Marshal.SizeOf<NativePrimList>());
    }

    // ========================================================================
    // USD and render types
    // ========================================================================

    [Fact]
    public void NativeUsdInput_Is40Bytes()
    {
        // 16 (file_path) + 8 (stage_id) + 16 (layer_content) = 40
        Assert.Equal(40, Marshal.SizeOf<NativeUsdInput>());
    }

    [Fact]
    public void NativeRenderProductSet_Is16Bytes()
    {
        Assert.Equal(16, Marshal.SizeOf<NativeRenderProductSet>());
    }

    [Fact]
    public void NativeOpWaitResult_Is24Bytes()
    {
        Assert.Equal(24, Marshal.SizeOf<NativeOpWaitResult>());
    }

    // ========================================================================
    // Buffer types
    // ========================================================================

    [Fact]
    public void NativeInputBuffer_Is64Bytes()
    {
        Assert.Equal(64, Marshal.SizeOf<NativeInputBuffer>());
    }

    [Fact]
    public void NativeOutputBuffer_Is64Bytes()
    {
        // 48 (DLTensor) + 16 (CudaSync) = 64
        Assert.Equal(64, Marshal.SizeOf<NativeOutputBuffer>());
    }

    [Fact]
    public void NativeAttributeMapping_Is56Bytes()
    {
        // 8 (map_handle) + 48 (DLTensor) = 56
        Assert.Equal(56, Marshal.SizeOf<NativeAttributeMapping>());
    }

    // ========================================================================
    // Attribute types
    // ========================================================================

    [Fact]
    public void NativeAttributeType_Is12Bytes()
    {
        // 4 (DLDataType) + 1 (bool) + 3 (padding) + 4 (semantic) = 12
        Assert.Equal(12, Marshal.SizeOf<NativeAttributeType>());
    }

    [Fact]
    public void NativeMappingDesc_Is8Bytes()
    {
        Assert.Equal(8, Marshal.SizeOf<NativeMappingDesc>());
    }

    [Fact]
    public void NativeMapOutputDescription_Is16Bytes()
    {
        // 4 (device_type) + 4 (padding) + 8 (sync_stream) = 16
        Assert.Equal(16, Marshal.SizeOf<NativeMapOutputDescription>());
    }

    // ========================================================================
    // Config types
    // ========================================================================

    [Fact]
    public void NativeConfigEntry_Is24Bytes()
    {
        Assert.Equal(24, Marshal.SizeOf<NativeConfigEntry>());
    }

    [Fact]
    public void NativeConfig_Is16Bytes()
    {
        Assert.Equal(16, Marshal.SizeOf<NativeConfig>());
    }

    // ========================================================================
    // Render output types
    // ========================================================================

    [Fact]
    public void NativeRenderVarOutput_Is24Bytes()
    {
        // 16 (render_var_name) + 8 (output_handle) = 24
        Assert.Equal(24, Marshal.SizeOf<NativeRenderVarOutput>());
    }

    [Fact]
    public void NativeFrameOutput_Is32Bytes()
    {
        // 8 (start) + 8 (end) + 8 (ptr) + 8 (count) = 32
        Assert.Equal(32, Marshal.SizeOf<NativeFrameOutput>());
    }

    [Fact]
    public void NativeOpCounter_Is32Bytes()
    {
        // 16 (name) + 8 (current) + 8 (total) = 32
        Assert.Equal(32, Marshal.SizeOf<NativeOpCounter>());
    }

    // ========================================================================
    // Field offset verification for critical structs
    // ========================================================================

    [Fact]
    public void DLTensor_FieldOffsets_AreCorrect()
    {
        Assert.Equal(0, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.Data)).ToInt32());
        Assert.Equal(8, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.Device)).ToInt32());
        Assert.Equal(16, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.NDim)).ToInt32());
        Assert.Equal(20, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.DType)).ToInt32());
        Assert.Equal(24, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.Shape)).ToInt32());
        Assert.Equal(32, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.Strides)).ToInt32());
        Assert.Equal(40, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.ByteOffset)).ToInt32());
    }

    [Fact]
    public void NativeEnqueueResult_OpIndex_AtOffset8()
    {
        Assert.Equal(8, Marshal.OffsetOf<NativeEnqueueResult>(nameof(NativeEnqueueResult.OpIndex)).ToInt32());
    }

    [Fact]
    public void NativeUsdInput_StageId_AtOffset16()
    {
        Assert.Equal(16, Marshal.OffsetOf<NativeUsdInput>(nameof(NativeUsdInput.UsdStageId)).ToInt32());
    }

    [Fact]
    public void NativeUsdInput_LayerContent_AtOffset24()
    {
        Assert.Equal(24, Marshal.OffsetOf<NativeUsdInput>(nameof(NativeUsdInput.UsdLayerContent)).ToInt32());
    }
}
