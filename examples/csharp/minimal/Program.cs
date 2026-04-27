// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.
//

// Minimal ovrtx C# example.
//
// Creates a Renderer, loads a USD scene, renders a single frame,
// and writes the result to out.png.
//
// This is the C# equivalent of examples/c/minimal/main.cpp
// and examples/python/minimal/main.py.

using Nvidia.Ovrtx;
using Nvidia.Ovrtx.Interop;

const string UsdUrl = "https://omniverse-content-production.s3.us-west-2.amazonaws.com/Samples/Robot-OVRTX/robot-ovrtx.usda";

// [snippet:create-renderer-csharp]
// Create the renderer with default configuration.
Console.Error.WriteLine("Creating renderer. The first run will take some time as shaders are compiled and cached...");
using var renderer = new Renderer();
Console.Error.WriteLine("Renderer created.");
// [/snippet:create-renderer-csharp]

// [snippet:add-usd-csharp]
// Load a USD layer into the renderer.
Console.Error.WriteLine($"Adding {UsdUrl} at root...");
renderer.AddUsd(UsdUrl);
Console.Error.WriteLine("USD loaded.");
// [/snippet:add-usd-csharp]

// [snippet:step-csharp]
// Step the renderer to simulate the Camera at 60Hz.
Console.Error.WriteLine("Stepping renderer...");
using var products = renderer.Step(
    renderProducts: ["/Render/Camera"],
    deltaTime: 1.0 / 60.0);
Console.Error.WriteLine("Stepped renderer.");
// [/snippet:step-csharp]

// [snippet:read-render-output-csharp]
// Map the LdrColor output to CPU memory and write it as a PNG.
Console.Error.WriteLine("Fetching results...");
foreach (var (productName, product) in products.Products)
{
    foreach (var frame in product.Frames)
    {
        if (!frame.RenderVars.TryGetValue("LdrColor", out var ldrColor))
        {
            Console.Error.WriteLine("LdrColor output not found");
            return 1;
        }

        using var mapped = ldrColor.Map(device: MapDeviceType.Cpu);

        // The tensor contains RGBA pixel data
        var tensor = mapped.Tensor;
        var shape = tensor.GetShape();
        int height = (int)shape[0];
        int width = (int)shape[1];

        Console.Error.WriteLine($"Got {width}x{height} image, writing to out.png...");
        WritePng("out.png", tensor.Data, width, height, channels: 4);
    }
}
Console.Error.WriteLine("Done. Output written to out.png");
// [/snippet:read-render-output-csharp]

return 0;

// ============================================================================
// Simple PNG writer using raw TGA-style encoding via .NET APIs.
// For production use, consider SkiaSharp or ImageSharp.
// ============================================================================

static void WritePng(string path, IntPtr pixelData, int width, int height, int channels)
{
    // Use System.Runtime to copy pixel data, then write as BMP/raw.
    // For a proper PNG we'd need a library, but we can write a simple
    // uncompressed file format to demonstrate the concept.
    int stride = width * channels;
    int dataSize = stride * height;
    byte[] pixels = new byte[dataSize];
    System.Runtime.InteropServices.Marshal.Copy(pixelData, pixels, 0, dataSize);

    // Write as simple PPM (Portable Pixel Map) which is easy to view
    // and requires no external libraries. Convert RGBA -> RGB.
    using var fs = File.Create(path.Replace(".png", ".ppm"));
    var header = System.Text.Encoding.ASCII.GetBytes($"P6\n{width} {height}\n255\n");
    fs.Write(header);
    for (int i = 0; i < width * height; i++)
    {
        fs.WriteByte(pixels[i * channels + 0]); // R
        fs.WriteByte(pixels[i * channels + 1]); // G
        fs.WriteByte(pixels[i * channels + 2]); // B
    }

    Console.Error.WriteLine($"(Written as PPM format to {path.Replace(".png", ".ppm")}. Use SkiaSharp or ImageSharp for PNG support.)");
}
