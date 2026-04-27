<!-- Copyright (c) 2026 Cristian Mori. Licensed under the MIT License. See csharp/LICENSE for details. -->
# Nvidia.Ovrtx — C# Wrapper for NVIDIA ovrtx

A .NET 8 wrapper for the ovrtx C API, providing P/Invoke bindings and a high-level managed API for Omniverse RTX sensor simulation and rendering.

## Architecture

The wrapper follows the same two-layer design as the Python SDK:

| Layer | Purpose | Files |
|-------|---------|-------|
| **Interop** | P/Invoke declarations, native struct mappings, string marshaling | `Interop/*.cs` |
| **High-level** | Managed `Renderer` class with IDisposable, async operations, error handling | `Renderer.cs`, `Operation.cs`, `RenderOutput.cs`, etc. |

## Quick Start

```csharp
using Nvidia.Ovrtx;

// Create renderer and load a USD scene
using var renderer = new Renderer();
renderer.AddUsd("scene.usda");

// Render a frame
using var products = renderer.Step(
    renderProducts: ["/Render/Camera"],
    deltaTime: 1.0 / 60.0);

// Read the output
foreach (var (name, product) in products.Products)
foreach (var frame in product.Frames)
{
    using var mapped = frame.RenderVars["LdrColor"].Map(MapDeviceType.Cpu);
    DLTensor tensor = mapped.Tensor;
    // tensor.Data points to RGBA pixel data
    // tensor.GetShape() returns [height, width, channels]
}
```

## API Surface

### Renderer (IDisposable)

| Method | Description |
|--------|-------------|
| `AddUsd` / `AddUsdAsync` | Load USD file or layer into the stage |
| `RemoveUsd` / `RemoveUsdAsync` | Remove a previously loaded USD file |
| `CloneUsd` / `CloneUsdAsync` | Clone a USD subtree |
| `Step` / `StepAsync` | Run a simulation step and produce rendered frames |
| `Reset` / `ResetAsync` | Reset sensor simulation history |
| `ResetStage` / `ResetStageAsync` | Clear the runtime stage |
| `WriteAttribute` / `WriteAttributeAsync` | Write attribute data to prims |
| `BindAttribute` / `BindAttributeAsync` | Create persistent attribute binding |
| `MapAttribute` | Map attribute for direct buffer access |
| `SetLogCallback` | Register a log message callback |

All async methods return `Operation` or `Operation<T>` with `Wait(timeoutNs)`.

### Configuration

```csharp
var config = new RendererConfig
{
    SyncMode = true,
    LogLevel = "info",
    ActiveCudaGpus = "0",
};
using var renderer = new Renderer(config);
```

### Native Library Loading

The wrapper loads `ovrtx-dynamic.dll` (Windows) or `libovrtx-dynamic.so` (Linux) from the standard library search paths. To specify a custom path:

```csharp
OvrtxLibrary.SetLibraryPath("/path/to/ovrtx/bin");
```

## Requirements

- .NET 8.0 SDK
- NVIDIA RTX-capable GPU with compatible driver
- ovrtx native library (from [GitHub Releases](https://github.com/NVIDIA-Omniverse/ovrtx/releases))

## Building

```bash
cd csharp/Nvidia.Ovrtx
dotnet build
```

## Examples

See [examples/csharp/](../../examples/csharp/) for runnable example projects.

## Author

Cristian Mori (cristian.mori@gmail.com)
