# Minimal C# Example

Basic workflow: create a Renderer, load a USD layer, step the renderer, and map/display the rendered output.

This is the C# equivalent of `examples/c/minimal` and `examples/python/minimal`.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- NVIDIA RTX-capable GPU with compatible driver

## Build and Run

```bash
cd examples/csharp/minimal
dotnet run
```

The first build will automatically download the ovrtx native package from GitHub Releases (~1 GB). This is handled by `msbuild/ovrtx.targets`, which is the C# equivalent of `cmake/ovrtx.cmake`.

The first run will compile and cache shaders, which may take some time. Subsequent runs will be fast.

The rendered image will be written to `out.ppm` in the current directory and can be viewed with any image viewer.

## How It Works

The project file (`minimal.csproj`) has two key parts:

1. **`<ProjectReference>`** to `Nvidia.Ovrtx` — the C# wrapper library
2. **`<Import Project="../msbuild/ovrtx.targets" />`** — downloads the native `ovrtx-dynamic.dll` and copies it + runtime directories to the output folder

This mirrors the C example pattern where `CMakeLists.txt` calls `include(ovrtx)` + `ovrtx_fetch()` + `ovrtx_setup_runtime()`.
