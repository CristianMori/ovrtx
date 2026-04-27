// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.
//

using System.Runtime.InteropServices;

namespace Nvidia.Ovrtx.Interop;

/// <summary>
/// Callback delegate matching ovrtx_log_callback_t.
/// Called from native code on any thread (serialized per renderer).
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void NativeLogCallback(
    ulong opId,
    int severity,
    double timestamp,
    NativeString message,
    IntPtr userData);

/// <summary>
/// P/Invoke declarations for all ovrtx C API functions.
/// </summary>
internal static class NativeMethods
{
    private const string LibName = "ovrtx-dynamic";

    // ========================================================================
    // Lifecycle
    // ========================================================================

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ovrtx_get_version(out uint major, out uint minor, out uint patch);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_initialize(in NativeConfig config);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_shutdown();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_create_renderer(in NativeConfig config, out IntPtr renderer);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_destroy_renderer(IntPtr renderer);

    // ========================================================================
    // Error handling
    // ========================================================================

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeString ovrtx_get_last_error();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeString ovrtx_get_last_op_error(ulong opId);

    // ========================================================================
    // Stream / operation waiting
    // ========================================================================

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_wait_op(
        IntPtr renderer,
        ulong opId,
        NativeTimeout timeout,
        out NativeOpWaitResult waitResult);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_query_op_status(
        IntPtr renderer,
        ulong opId,
        out NativeOpStatus status);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_release_op_status(
        IntPtr renderer,
        ref NativeOpStatus status);

    // ========================================================================
    // USD stage management
    // ========================================================================

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovrtx_add_usd(
        IntPtr renderer,
        NativeUsdInput usdInput,
        NativeString pathPrefix,
        out ulong usdHandle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovrtx_remove_usd(
        IntPtr renderer,
        ulong usdHandle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovrtx_clone_usd(
        IntPtr renderer,
        NativeString sourcePath,
        IntPtr targetPaths,    // const ovx_string_t*
        nuint numTargetPaths);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovrtx_reset_stage(IntPtr renderer);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovrtx_update_stage_from_usd_time(
        IntPtr renderer,
        double usdTime);

    // ========================================================================
    // Sensor simulation / rendering
    // ========================================================================

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovrtx_step(
        IntPtr renderer,
        NativeRenderProductSet renderProducts,
        double deltaTime,
        out ulong stepResultHandle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_fetch_results(
        IntPtr renderer,
        ulong resultHandle,
        NativeTimeout timeout,
        out NativeRenderProductSetOutputs outputs);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_destroy_results(
        IntPtr renderer,
        ulong resultHandle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_map_rendered_output(
        IntPtr renderer,
        ulong outputHandle,
        in NativeMapOutputDescription mapDesc,
        NativeTimeout timeout,
        out NativeRenderedOutput renderedOutput);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_unmap_rendered_output(
        IntPtr renderer,
        ulong mapHandle,
        NativeCudaSync cudaSync);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovrtx_reset(
        IntPtr renderer,
        double time);

    // ========================================================================
    // Attribute writing
    // ========================================================================

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovrtx_write_attribute(
        IntPtr renderer,
        in NativeBindingDescOrHandle binding,
        in NativeInputBuffer dataArray,
        int dataAccess);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_map_attribute(
        IntPtr renderer,
        in NativeBindingDescOrHandle binding,
        NativeMappingDesc mappingDesc,
        out NativeAttributeMapping mapping);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovrtx_unmap_attribute(
        IntPtr renderer,
        ulong mapHandle,
        NativeCudaSync cudaSync);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovrtx_create_attribute_binding(
        IntPtr renderer,
        in NativeBindingDesc desc,
        out ulong bindingHandle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovrtx_destroy_attribute_binding(
        IntPtr renderer,
        ulong bindingHandle);

    // ========================================================================
    // Logging
    // ========================================================================

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_set_log_callback(
        IntPtr renderer,
        int minSeverity,
        IntPtr channelFilter,    // const ovx_string_t* (null for all channels)
        IntPtr callback,         // ovrtx_log_callback_t (null to disable)
        IntPtr userData);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovrtx_flush_op_log(
        IntPtr renderer,
        NativeTimeout timeout);
}
