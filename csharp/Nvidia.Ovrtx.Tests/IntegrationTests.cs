// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Nvidia.Ovrtx;
using Nvidia.Ovrtx.Interop;
using Xunit;

namespace Nvidia.Ovrtx.Tests;

/// <summary>
/// Integration tests that require the native ovrtx library and a GPU.
/// These tests are automatically skipped if the native library is not available.
/// Set environment variable OVRTX_TEST_INTEGRATION=1 to enable.
/// </summary>
[Collection("Integration")]
public class IntegrationTests : IDisposable
{
    private Renderer? _renderer;
    private static readonly bool _enabled =
        Environment.GetEnvironmentVariable("OVRTX_TEST_INTEGRATION") == "1";

    private const string UsdUrl =
        "https://omniverse-content-production.s3.us-west-2.amazonaws.com/Samples/Robot-OVRTX/robot-ovrtx.usda";

    private Renderer GetRenderer()
    {
        _renderer ??= new Renderer();
        return _renderer;
    }

    public void Dispose()
    {
        _renderer?.Dispose();
    }

    [Fact]
    public void Renderer_Create_Succeeds()
    {
        if (!_enabled) return;
        var r = GetRenderer();
        Assert.NotNull(r);
    }

    [Fact]
    public void Renderer_GetVersion_ReturnsValidVersion()
    {
        if (!_enabled) return;
        var (major, minor, patch) = Renderer.Version;
        Assert.True(major >= 0);
        Assert.True(minor >= 0);
    }

    [Fact]
    public void Renderer_CreateWithConfig_Succeeds()
    {
        if (!_enabled) return;
        var config = new RendererConfig { SyncMode = true };
        using var renderer = new Renderer(config);
        Assert.True(config.SyncMode);
    }

    [Fact]
    public void Renderer_AddUsd_ReturnsValidHandle()
    {
        if (!_enabled) return;
        var handle = GetRenderer().AddUsd(UsdUrl);
        Assert.True(handle.IsValid);
    }

    [Fact]
    public void Renderer_AddUsdAsync_CanWait()
    {
        if (!_enabled) return;
        var op = GetRenderer().AddUsdAsync(UsdUrl);
        var handle = op.Wait();
        Assert.NotNull(handle);
    }

    [Fact]
    public void Renderer_AddUsdLayer_ReturnsValidHandle()
    {
        if (!_enabled) return;
        string usda = "#usda 1.0\ndef Xform \"World\" {}\n";
        var handle = GetRenderer().AddUsdLayer(usda);
        Assert.True(handle.IsValid);
    }

    [Fact]
    public void Renderer_ResetStage_Succeeds()
    {
        if (!_enabled) return;
        GetRenderer().ResetStage();
    }

    [Fact]
    public void Renderer_Reset_Succeeds()
    {
        if (!_enabled) return;
        GetRenderer().Reset(0.0);
    }

    [Fact]
    public void Renderer_Step_ReturnsOutputs()
    {
        if (!_enabled) return;
        var r = GetRenderer();
        r.AddUsd(UsdUrl);
        using var products = r.Step(["/Render/Camera"], deltaTime: 0.0);
        Assert.NotNull(products.Products);
        Assert.NotEmpty(products.Products);
    }

    [Fact]
    public void Renderer_Step_MapCpu_ReturnsTensorWithData()
    {
        if (!_enabled) return;
        var r = GetRenderer();
        r.AddUsd(UsdUrl);
        using var products = r.Step(["/Render/Camera"], deltaTime: 1.0 / 60.0);

        foreach (var (_, product) in products.Products)
        foreach (var frame in product.Frames)
        {
            Assert.True(frame.RenderVars.ContainsKey("LdrColor") || frame.RenderVars.ContainsKey("HdrColor"));
            var varName = frame.RenderVars.ContainsKey("LdrColor") ? "LdrColor" : "HdrColor";
            using var mapped = frame.RenderVars[varName].Map(MapDeviceType.Cpu);
            var tensor = mapped.Tensor;
            Assert.NotEqual(IntPtr.Zero, tensor.Data);
            Assert.Equal(DLDeviceType.Cpu, tensor.Device.DeviceType);
            var shape = tensor.GetShape();
            Assert.True(shape.Length >= 2);
            Assert.True(shape[0] > 0);
            Assert.True(shape[1] > 0);
        }
    }

    [Fact]
    public void Renderer_StepAsync_ThenWait_Succeeds()
    {
        if (!_enabled) return;
        var r = GetRenderer();
        r.AddUsd(UsdUrl);
        var result = r.StepAsync(["/Render/Camera"], deltaTime: 0.0);
        using var products = result.Wait();
        Assert.NotNull(products);
        Assert.NotEmpty(products!.Products);
    }

    [Fact]
    public void Renderer_RemoveUsd_Succeeds()
    {
        if (!_enabled) return;
        var r = GetRenderer();
        var handle = r.AddUsd(UsdUrl);
        r.RemoveUsd(handle);
    }

    [Fact]
    public unsafe void Renderer_WriteAttribute_Succeeds()
    {
        if (!_enabled) return;
        var r = GetRenderer();
        r.AddUsd(UsdUrl);

        double[] xform = new double[16];
        xform[0] = 1; xform[5] = 1; xform[10] = 1; xform[15] = 1;

        fixed (double* p = xform)
        {
            long shape = 1;
            var tensor = new DLTensor
            {
                Data = (IntPtr)p,
                Device = DLDevice.Cpu,
                NDim = 1,
                DType = new DLDataType(DLDataTypeCode.Float, 64, 16),
                Shape = (IntPtr)(&shape),
                Strides = IntPtr.Zero,
                ByteOffset = 0,
            };
            r.WriteAttribute(
                primPaths: ["/World/Camera"],
                attributeName: "omni:xform",
                tensor: &tensor,
                semantic: Semantic.XformMat4x4,
                dataAccess: DataAccess.Sync);
        }
    }

    [Fact]
    public void Renderer_GetLastError_ReturnsString()
    {
        if (!_enabled) return;
        GetRenderer(); // ensure library is loaded
        var error = Renderer.GetLastError();
        Assert.NotNull(error);
    }

    [Fact]
    public void Renderer_Dispose_ThenAccessThrows()
    {
        if (!_enabled) return;
        var renderer = new Renderer();
        renderer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => renderer.ResetStage());
    }

    [Fact]
    public void Renderer_DisposeTwice_DoesNotThrow()
    {
        if (!_enabled) return;
        var renderer = new Renderer();
        renderer.Dispose();
        renderer.Dispose();
    }
}
