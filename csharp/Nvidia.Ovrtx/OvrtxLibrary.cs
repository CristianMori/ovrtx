// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.
//

using System.Runtime.InteropServices;
using Nvidia.Ovrtx.Interop;

namespace Nvidia.Ovrtx;

/// <summary>
/// Configures native library loading for ovrtx.
/// Call SetLibraryPath before creating any Renderer instance.
/// </summary>
public static class OvrtxLibrary
{
    private static bool _resolverSet;

    /// <summary>
    /// Set a custom directory path where the ovrtx native library can be found.
    /// Must be called before any other ovrtx API call.
    /// </summary>
    /// <param name="directoryPath">Directory containing ovrtx-dynamic.dll or libovrtx-dynamic.so.</param>
    public static void SetLibraryPath(string directoryPath)
    {
        if (_resolverSet)
            throw new InvalidOperationException("Library path resolver has already been set.");

        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, (name, assembly, searchPath) =>
        {
            if (name != "ovrtx-dynamic")
                return IntPtr.Zero;

            var libFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "ovrtx-dynamic.dll"
                : "libovrtx-dynamic.so";

            var fullPath = Path.Combine(directoryPath, libFileName);
            return NativeLibrary.Load(fullPath);
        });

        _resolverSet = true;
    }

    /// <summary>Get the ovrtx library version without creating a renderer.</summary>
    public static (uint Major, uint Minor, uint Patch) GetVersion()
    {
        NativeMethods.ovrtx_get_version(out var major, out var minor, out var patch);
        return (major, minor, patch);
    }
}
