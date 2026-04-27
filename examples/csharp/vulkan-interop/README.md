# Vulkan Interop C# Example

Demonstrates ovrtx-Vulkan interoperability: rendering USD scenes with ovrtx and displaying them via CUDA-Vulkan shared images with interactive orbit camera control.

This is the C# equivalent of `examples/c/vulkan-interop`.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- NVIDIA RTX-capable GPU with compatible driver
- [CUDA Toolkit](https://developer.nvidia.com/cuda-downloads)
- [Vulkan SDK](https://vulkan.lunarg.com/sdk/home) (for shader compilation)

## Shader Compilation

The GLSL shaders from the C example need to be compiled to SPIR-V:

```bash
cd examples/c/vulkan-interop/shaders
glslc -O --target-env=vulkan1.2 --target-spv=spv1.5 fullscreen.vert -o ../../csharp/vulkan-interop/shaders/fullscreen.vert.spv
glslc -O --target-env=vulkan1.2 --target-spv=spv1.5 fullscreen.frag -o ../../csharp/vulkan-interop/shaders/fullscreen.frag.spv
```

## Build and Run

```bash
cd examples/csharp/vulkan-interop
dotnet run
```

## Options

```
--usd, -u <path>              USD file path or URL (default: robot-ovrtx sample)
--render-product, -r <path>   Render product prim path (default: /Render/Camera)
--up-axis, -a <Y|Z>           Scene up axis (default: Z)
--units <meters|centimeters>   Scene units (default: meters)
--num-frames, -n <N>          Render N frames then exit
--help, -h                    Show help
```

## Architecture

The example demonstrates the full ovrtx CUDA-Vulkan interop pipeline:

1. **ovrtx** renders the scene on GPU, producing CUDA arrays
2. **CUDA** copies the arrays into Vulkan-exported shared images
3. **Vulkan** samples the shared images and presents to the swapchain
4. **Timeline semaphores** synchronize CUDA writes with Vulkan reads
5. **Double buffering** allows CUDA and Vulkan to work in parallel

### Key files

| File | Purpose |
|------|---------|
| `Program.cs` | Main application, render loop, ovrtx integration |
| `OrbitCamera.cs` | Spherical coordinate camera with Y/Z up-axis support |
| `CudaInterop.cs` | CUDA Driver API P/Invoke, Vulkan memory import, array copy |

### C -> C# mapping

| C (cmake) | C# (.NET) |
|-----------|-----------|
| `main.cpp` | `Program.cs` |
| `orbit_camera.cpp/hpp` | `OrbitCamera.cs` |
| `cuda_kernel.cpp/hpp` | `CudaInterop.cs` |
| `vulkan_context.cpp/hpp` | Uses Silk.NET.Vulkan |
| `command_buffer.cpp/hpp` | Uses Silk.NET.Vulkan |
| volk (Vulkan loader) | Silk.NET.Vulkan |
| GLFW | Silk.NET.Windowing |
| GLM | System.Numerics |
