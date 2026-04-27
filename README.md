

# NVIDIA ovrtx

ovrtx is a lightweight C, Python, and C# SDK for Omniverse RTX, allowing developers to integrate RTX sensor simulation and visualization easily into their applications.

Omniverse RTX provides real-time, physically accurate sensor simulation and rendering for [Physical AI](https://www.nvidia.com/en-us/glossary/generative-physical-ai/), targeting robotics learning, synthetic data generation, and industrial and design workflows.

* [Get started in Python](#getting-started-in-python)
* [Get started in C](#getting-started-in-c)
* [Get started in C#](#getting-started-in-c-1)

> [!NOTE]
> ovrtx is currently **pre-release** software.

![warehouse](img/warehouse.jpg)

## Features
* Physically accurate simulation of cameras, lidar, radar, ultrasonic and more sensors.
* Scalable simulation performance from reinforcement learning in-the-loop with tens of thousands of frames per second, through real-time, photorealistic, interactive viewport and navigation, to offline predictive rendering.
* [OpenUSD](https://aousd.org/) scene description allowing interchange with a vast ecosystem of content creation, CAD and simulation tools.
* Easy integration with Python and C# simulation and learning ecosystems.

## Getting Started in Python

During Early Access, we recommend using the [uv](https://docs.astral.sh/uv/getting-started/installation/) Python package and project manager. All the examples in this repository contain pyproject.toml files that are tested with uv.

Python 3.10–3.13 is required.

To get started, first clone this repository and run the first example with uv:

```bash
git clone https://github.com/NVIDIA-Omniverse/ovrtx.git
cd ovrtx/examples/python/minimal
uv run main.py
```

The minimal example shows how to create the renderer, load an OpenUSD scene and render a single image, copying the results back to the CPU for display.

![minimal example](img/example-minimal.jpg)


Note that the first time a program built against ovrtx is run, it will compile and cache necessary shaders, which may take some time depending on your system. Subsequent runs will use the cached shaders and will be fast.

## Getting Started in C

The C/C++ examples require CMake and a development environment. On Windows this is provided by [Visual Studio 2017 or newer](https://visualstudio.microsoft.com/). On Linux (Ubuntu):

```bash
sudo apt-get build-essential cmake
```

To get started, first clone this repository and build and run the first example using CMake:

```bash
git clone https://github.com/NVIDIA-Omniverse/ovrtx.git
cd ovrtx/examples/c/minimal
cmake -B build 
```

Then, on Windows:
```
cmake --build build --config Release
.\build\Release\minimal.exe
```

On Linux:
```
cmake --build build --config Release
./build/minimal
```

The minimal example shows how to create the renderer, load an OpenUSD scene and render a single image, copying the results back to the CPU for writing out as a PNG.

![minimal example](img/example-minimal.jpg)

The resulting image will be written to `./out.png` and can be inspected with any image viewer.

Note that the first time a program built against ovrtx is run, it will compile and cache necessary shaders, which may take some time depending on your system. Subsequent runs will use the cached shaders and will be fast.

## Getting Started in C#

The C# wrapper requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and an NVIDIA RTX-capable GPU.

To get started, first clone this repository and run the first example with `dotnet`:

```bash
git clone https://github.com/NVIDIA-Omniverse/ovrtx.git
cd ovrtx/examples/csharp/minimal
dotnet run
```

The first build will automatically download the ovrtx native package from GitHub Releases. This is handled by `examples/csharp/msbuild/ovrtx.targets`, which is the C# equivalent of the CMake `ovrtx_fetch()` macro.

The minimal example shows how to create the renderer, load an OpenUSD scene and render a single image, copying the results back to the CPU.

![minimal example](img/example-minimal.jpg)

Note that the first time a program built against ovrtx is run, it will compile and cache necessary shaders, which may take some time depending on your system. Subsequent runs will use the cached shaders and will be fast.

### Using the C# wrapper in your own project

1. Reference the `Nvidia.Ovrtx` project (or build and reference the DLL):

```xml
<ItemGroup>
  <ProjectReference Include="path/to/csharp/Nvidia.Ovrtx/Nvidia.Ovrtx.csproj" />
</ItemGroup>
```

2. Import the MSBuild targets to automatically download the native library:

```xml
<Import Project="path/to/examples/csharp/msbuild/ovrtx.targets" />
```

3. Use the API:

```csharp
using Nvidia.Ovrtx;

using var renderer = new Renderer();
renderer.AddUsd("scene.usda");

using var products = renderer.Step(
    renderProducts: ["/Render/Camera"],
    deltaTime: 1.0 / 60.0);

foreach (var (name, product) in products.Products)
foreach (var frame in product.Frames)
{
    using var mapped = frame.RenderVars["LdrColor"].Map(MapDeviceType.Cpu);
    // mapped.Tensor.Data contains RGBA pixel data
    // mapped.Tensor.GetShape() returns [height, width, channels]
}
```

See the [C# wrapper README](csharp/Nvidia.Ovrtx/README.md) for the full API reference.

## Examples

Further examples using the C, Python, and C# APIs are available in the [examples](examples/README.md) directory. See the individual examples for building and usage instructions.

## Releases

The Releases page of this repository contains binary builds for the official releases of the ovrtx C library and the corresponding Python wheels.
These binaries are provided for the supported platforms:
 * Windows x86_64
 * Linux x86_64
 * Linux aarch64

The libraries require a compatible NVIDIA RTX-capable GPU with a compatible NVIDIA driver on the system to be able to initialize correctly.
More detailed system requirements can be found at https://docs.omniverse.nvidia.com/dev-guide/latest/common/technical-requirements.html 

<!--
## Badges
TODO - show badges for
* PyPI package
* License
* GitHub Commit activity
* downloads/month
* CI pipeline results
-->

## AI Coding Agents

The [skills](skills) directory contains a series of Skills to help AI coding agents to understand how to use the API (and they're useful for humans too). Copy this directory to your project and point your agent at it.


## Documentation
<!-- TODO - Pointer to Tutorials and API Documentation -->
Documentation is published at https://nvidia-omniverse.github.io/ovrtx

### Prerequisites for Documentation Build

* [uv](https://docs.astral.sh/uv/)
* [Doxygen](https://www.doxygen.nl/)

```bash
cd docs
make html
```

Then, to view the built docs:
```bash
uv run python -m http.server 8000 -d _build/html
```

Then open http://localhost:8000/ in a browser.


## Support
https://forums.developer.nvidia.com/c/omniverse/300

## Roadmap
To be announced

## Contributing
At this time this project is not open to external contributions.

## Authors and acknowledgment
NVIDIA Corporation

## License

The software and materials are governed by the [NVIDIA Software License Agreement](https://www.nvidia.com/en-us/agreements/enterprise-software/nvidia-software-license-agreement/) and the [Product-Specific Terms for NVIDIA Omniverse](https://www.nvidia.com/en-us/agreements/enterprise-software/product-specific-terms-for-omniverse/).

This project will download and install additional third-party open source software projects. Review the license terms of these open source projects before use.
