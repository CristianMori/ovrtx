// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

// CUDA Driver API P/Invoke bindings and interop helpers.
// Port of examples/c/vulkan-interop/src/cuda/cuda_kernel.cpp.

using System.Runtime.InteropServices;

namespace VulkanInterop;

public enum CudaImageFormat { Half4, UInt8_4 }

/// <summary>
/// Wraps the CUDA Driver API calls needed for Vulkan-CUDA interop.
/// Imports Vulkan-exported memory as CUDA surfaces and manages synchronization.
/// </summary>
public static unsafe class CudaInterop
{
    // ========================================================================
    // CUDA Driver API P/Invoke
    // ========================================================================

    private const string CudaLib = "nvcuda";

    // Basic types
    // CUcontext, CUstream, CUevent, CUarray, CUmipmappedArray, CUexternalMemory,
    // CUexternalSemaphore, CUsurfObject are all IntPtr-sized handles.

    [DllImport(CudaLib)] static extern int cuInit(uint flags);
    [DllImport(CudaLib)] static extern int cuCtxGetCurrent(out IntPtr ctx);
    [DllImport(CudaLib)] static extern int cuDeviceGet(out int device, int ordinal);
    [DllImport(CudaLib)] static extern int cuDeviceGetCount(out int count);
    [DllImport(CudaLib)] static extern int cuDeviceGetUuid(byte* uuid, int dev);
    [DllImport(CudaLib)] static extern int cuStreamCreate(out IntPtr stream, uint flags);
    [DllImport(CudaLib)] static extern int cuStreamSynchronize(IntPtr stream);
    [DllImport(CudaLib)] static extern int cuStreamDestroy(IntPtr stream);
    [DllImport(CudaLib)] static extern int cuEventCreate(out IntPtr evt, uint flags);
    [DllImport(CudaLib)] static extern int cuEventDestroy(IntPtr evt);
    [DllImport(CudaLib)] static extern int cuEventRecord(IntPtr evt, IntPtr stream);
    [DllImport(CudaLib)] static extern int cuEventQuery(IntPtr evt);
    [DllImport(CudaLib)] static extern int cuEventElapsedTime(out float ms, IntPtr start, IntPtr end);
    [DllImport(CudaLib)] static extern int cuStreamWaitEvent(IntPtr stream, IntPtr evt, uint flags);

    // External memory
    [DllImport(CudaLib)] static extern int cuImportExternalMemory(out IntPtr extMem, void* desc);
    [DllImport(CudaLib)] static extern int cuExternalMemoryGetMappedMipmappedArray(out IntPtr mipArray, IntPtr extMem, void* desc);
    [DllImport(CudaLib)] static extern int cuMipmappedArrayGetLevel(out IntPtr array, IntPtr mipArray, uint level);
    [DllImport(CudaLib)] static extern int cuSurfObjectCreate(out ulong surfObj, void* desc);
    [DllImport(CudaLib)] static extern int cuSurfObjectDestroy(ulong surfObj);
    [DllImport(CudaLib)] static extern int cuDestroyExternalMemory(IntPtr extMem);
    [DllImport(CudaLib)] static extern int cuMipmappedArrayDestroy(IntPtr mipArray);

    // External semaphore
    [DllImport(CudaLib)] static extern int cuImportExternalSemaphore(out IntPtr extSem, void* desc);
    [DllImport(CudaLib)] static extern int cuSignalExternalSemaphoresAsync(IntPtr* extSems, void* paramsArray, uint count, IntPtr stream);
    [DllImport(CudaLib)] static extern int cuWaitExternalSemaphoresAsync(IntPtr* extSems, void* paramsArray, uint count, IntPtr stream);
    [DllImport(CudaLib)] static extern int cuDestroyExternalSemaphore(IntPtr extSem);

    // Memory copy
    [DllImport(CudaLib)] static extern int cuMemcpy2D(void* copyParams);

    // Constants
    const uint CU_EVENT_DEFAULT = 0;
    const uint CU_EVENT_DISABLE_TIMING = 2;
    const int CUDA_SUCCESS = 0;

    // External memory/semaphore handle types
    const int CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_WIN32 = 1;
    const int CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_FD = 2;
    const int CU_EXTERNAL_SEMAPHORE_HANDLE_TYPE_OPAQUE_WIN32 = 1;
    const int CU_EXTERNAL_SEMAPHORE_HANDLE_TYPE_OPAQUE_FD = 2;
    const int CU_EXTERNAL_SEMAPHORE_HANDLE_TYPE_TIMELINE_WIN32 = 5;
    const int CU_EXTERNAL_SEMAPHORE_HANDLE_TYPE_TIMELINE_FD = 6;

    // CUDA array format
    const int CU_AD_FORMAT_UNSIGNED_INT8 = 0x01;
    const int CU_AD_FORMAT_HALF = 0x10;

    // Memcpy types
    const int CU_MEMORYTYPE_ARRAY = 3;

    // ========================================================================
    // State
    // ========================================================================

    private static IntPtr[] _extMemory = new IntPtr[2];
    private static IntPtr[] _mipArrays = new IntPtr[2];
    private static IntPtr[] _arrays = new IntPtr[2];
    private static ulong[] _surfaces = new ulong[2];
    private static IntPtr _timelineSemaphore;
    private static byte[] _cudaUuid = new byte[16];

    // ========================================================================
    // Public API
    // ========================================================================

    /// <summary>
    /// Initialize CUDA by getting the current context (created by ovrtx).
    /// Returns the CUDA device UUID for matching with Vulkan.
    /// </summary>
    public static bool Init(out byte[] uuid)
    {
        uuid = new byte[16];
        cuInit(0);

        IntPtr ctx;
        if (cuCtxGetCurrent(out ctx) != CUDA_SUCCESS || ctx == IntPtr.Zero)
        {
            Console.Error.WriteLine("No current CUDA context (ovrtx should have created one)");
            return false;
        }

        // Get device 0's UUID (ovrtx uses device 0 by default)
        int dev;
        cuDeviceGet(out dev, 0);
        fixed (byte* p = uuid)
        {
            if (cuDeviceGetUuid(p, dev) != CUDA_SUCCESS)
                return false;
        }

        Array.Copy(uuid, _cudaUuid, 16);
        return true;
    }

    public static IntPtr CreateStream()
    {
        cuStreamCreate(out var stream, 0);
        return stream;
    }

    public static IntPtr CreateEvent(bool timing = true)
    {
        cuEventCreate(out var evt, timing ? CU_EVENT_DEFAULT : CU_EVENT_DISABLE_TIMING);
        return evt;
    }

    public static void RecordEvent(IntPtr evt, IntPtr stream) => cuEventRecord(evt, stream);
    public static bool QueryEvent(IntPtr evt) => cuEventQuery(evt) == CUDA_SUCCESS;
    public static float EventElapsed(IntPtr start, IntPtr end)
    {
        cuEventElapsedTime(out var ms, start, end);
        return ms;
    }

    public static void StreamSync(IntPtr stream) => cuStreamSynchronize(stream);
    public static void WaitEvent(IntPtr evt, IntPtr stream) => cuStreamWaitEvent(stream, evt, 0);

    public static void DestroyEvent(IntPtr evt) => cuEventDestroy(evt);
    public static void DestroyStream(IntPtr stream) => cuStreamDestroy(stream);

    /// <summary>
    /// Import a Vulkan-exported memory handle as a CUDA surface object.
    /// </summary>
    public static ulong ImportVulkanImage(int index, IntPtr handle, ulong allocSize,
        int width, int height, CudaImageFormat format)
    {
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        // CUDA_EXTERNAL_MEMORY_HANDLE_DESC (simplified - using raw bytes)
        // We need to build the struct manually since it has unions
        var descBytes = new byte[256]; // oversized, zeroed
        fixed (byte* desc = descBytes)
        {
            // type field at offset 0
            *(int*)desc = isWindows
                ? CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_WIN32
                : CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_FD;

            // handle union at offset 8 (win32 handle or fd)
            if (isWindows)
                *(IntPtr*)(desc + 8) = handle; // handle.win32.handle
            else
                *(int*)(desc + 8) = (int)(long)handle; // handle.fd

            // size at offset 32
            *(ulong*)(desc + 32) = allocSize;

            // flags at offset 40
            *(uint*)(desc + 40) = 0;

            if (cuImportExternalMemory(out _extMemory[index], desc) != CUDA_SUCCESS)
            {
                Console.Error.WriteLine($"cuImportExternalMemory failed for image {index}");
                return 0;
            }
        }

        // Map to mipmapped array
        int cudaFormat = format == CudaImageFormat.Half4 ? CU_AD_FORMAT_HALF : CU_AD_FORMAT_UNSIGNED_INT8;
        int numChannels = 4;

        var bufDescBytes = new byte[128];
        fixed (byte* bufDesc = bufDescBytes)
        {
            // offset
            *(ulong*)bufDesc = 0;
            // arrayDesc at offset 8
            // Width, Height, Depth, Format, NumChannels, Flags
            var arrayDesc = bufDesc + 8;
            *(uint*)(arrayDesc + 0) = (uint)width;       // Width
            *(uint*)(arrayDesc + 4) = (uint)height;      // Height
            *(uint*)(arrayDesc + 8) = 0;                 // Depth
            *(int*)(arrayDesc + 12) = cudaFormat;        // Format
            *(uint*)(arrayDesc + 16) = (uint)numChannels; // NumChannels
            *(uint*)(arrayDesc + 20) = 0;                // Flags
            // numLevels at offset after arrayDesc
            *(uint*)(bufDesc + 8 + 24) = 1;

            if (cuExternalMemoryGetMappedMipmappedArray(out _mipArrays[index], _extMemory[index], bufDesc) != CUDA_SUCCESS)
            {
                Console.Error.WriteLine($"cuExternalMemoryGetMappedMipmappedArray failed for image {index}");
                return 0;
            }
        }

        // Get level 0
        if (cuMipmappedArrayGetLevel(out _arrays[index], _mipArrays[index], 0) != CUDA_SUCCESS)
        {
            Console.Error.WriteLine($"cuMipmappedArrayGetLevel failed for image {index}");
            return 0;
        }

        // Create surface object
        var resDescBytes = new byte[64];
        fixed (byte* resDesc = resDescBytes)
        {
            *(int*)resDesc = 0x03; // CU_RESOURCE_TYPE_ARRAY
            *(IntPtr*)(resDesc + 8) = _arrays[index]; // res.array.hArray
        }

        fixed (byte* resDesc = resDescBytes)
        {
            if (cuSurfObjectCreate(out _surfaces[index], resDesc) != CUDA_SUCCESS)
            {
                Console.Error.WriteLine($"cuSurfObjectCreate failed for image {index}");
                return 0;
            }
        }

        return _surfaces[index];
    }

    /// <summary>
    /// Import a Vulkan timeline semaphore for CUDA signaling.
    /// </summary>
    public static void ImportTimelineSemaphore(IntPtr handle)
    {
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        var descBytes = new byte[128];
        fixed (byte* desc = descBytes)
        {
            *(int*)desc = isWindows
                ? CU_EXTERNAL_SEMAPHORE_HANDLE_TYPE_TIMELINE_WIN32
                : CU_EXTERNAL_SEMAPHORE_HANDLE_TYPE_TIMELINE_FD;

            if (isWindows)
                *(IntPtr*)(desc + 8) = handle;
            else
                *(int*)(desc + 8) = (int)(long)handle;

            *(uint*)(desc + 32) = 0; // flags

            int r = cuImportExternalSemaphore(out _timelineSemaphore, desc);
            if (r != CUDA_SUCCESS)
                Console.Error.WriteLine($"cuImportExternalSemaphore failed: {r}");
        }
    }

    /// <summary>
    /// Signal the timeline semaphore from CUDA stream.
    /// </summary>
    public static void SignalTimeline(ulong value, IntPtr stream)
    {
        var paramsBytes = new byte[128];
        fixed (byte* p = paramsBytes)
        {
            // params.value = value (at offset 8 in the struct)
            *(ulong*)(p + 0) = value; // params.params.fence.value

            var sem = _timelineSemaphore;
            cuSignalExternalSemaphoresAsync(&sem, p, 1, stream);
        }
    }

    /// <summary>
    /// Copy a CUDA array (from ovrtx) to a CUDA surface (shared Vulkan image).
    /// </summary>
    public static void CopyArrayToSurface(int bufferIdx, IntPtr srcArray,
        int width, int height, CudaImageFormat format, IntPtr stream)
    {
        int pixelSize = format == CudaImageFormat.Half4 ? 8 : 4;

        // CUDA_MEMCPY2D
        var copyParams = new byte[160]; // large enough
        fixed (byte* p = copyParams)
        {
            // srcMemoryType = CU_MEMORYTYPE_ARRAY (3)
            *(int*)(p + 0) = CU_MEMORYTYPE_ARRAY;  // srcMemoryType at offset 0
            *(IntPtr*)(p + 32) = srcArray;           // srcArray

            // dstMemoryType = CU_MEMORYTYPE_ARRAY (3)
            *(int*)(p + 48) = CU_MEMORYTYPE_ARRAY;  // dstMemoryType
            *(IntPtr*)(p + 80) = _arrays[bufferIdx]; // dstArray

            // WidthInBytes, Height
            *(ulong*)(p + 96) = (ulong)(width * pixelSize); // WidthInBytes
            *(ulong*)(p + 104) = (ulong)height;              // Height

            cuMemcpy2D(p);
        }
    }

    public static void Cleanup()
    {
        for (int i = 0; i < 2; i++)
        {
            if (_surfaces[i] != 0) { cuSurfObjectDestroy(_surfaces[i]); _surfaces[i] = 0; }
            if (_mipArrays[i] != IntPtr.Zero) { cuMipmappedArrayDestroy(_mipArrays[i]); _mipArrays[i] = IntPtr.Zero; }
            if (_extMemory[i] != IntPtr.Zero) { cuDestroyExternalMemory(_extMemory[i]); _extMemory[i] = IntPtr.Zero; }
        }
        if (_timelineSemaphore != IntPtr.Zero) { cuDestroyExternalSemaphore(_timelineSemaphore); _timelineSemaphore = IntPtr.Zero; }
    }
}
