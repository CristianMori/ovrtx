// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.
//

using System.Runtime.InteropServices;
using System.Text;

namespace Nvidia.Ovrtx.Interop;

/// <summary>
/// Maps to ovx_string_t { const char* ptr; size_t length; } (16 bytes on x64).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeString
{
    public IntPtr Ptr;
    public nuint Length;

    public readonly string? ToManaged()
    {
        if (Ptr == IntPtr.Zero) return null;
        unsafe
        {
            return Encoding.UTF8.GetString((byte*)Ptr, (int)Length);
        }
    }

    public readonly override string ToString() => ToManaged() ?? string.Empty;

    public static readonly NativeString Empty = default;
}

/// <summary>
/// Manages a pinned UTF-8 byte array for passing a string to native code.
/// Must be disposed after the P/Invoke call completes.
/// </summary>
internal sealed class NativeStringContext : IDisposable
{
    private GCHandle _handle;
    public NativeString Value;

    public NativeStringContext(string? managed)
    {
        if (string.IsNullOrEmpty(managed))
        {
            Value = default;
            _handle = default;
            return;
        }

        byte[] utf8 = Encoding.UTF8.GetBytes(managed);
        _handle = GCHandle.Alloc(utf8, GCHandleType.Pinned);
        Value = new NativeString
        {
            Ptr = _handle.AddrOfPinnedObject(),
            Length = (nuint)utf8.Length,
        };
    }

    public void Dispose()
    {
        if (_handle.IsAllocated)
            _handle.Free();
    }
}

/// <summary>
/// Manages an array of pinned UTF-8 strings for passing to native code.
/// Produces both the NativeString[] array and pins it for pointer access.
/// </summary>
internal sealed class NativeStringArrayContext : IDisposable
{
    private readonly NativeStringContext[] _contexts;
    private GCHandle _arrayHandle;

    public NativeString[] Values { get; }
    public IntPtr Pointer => _arrayHandle.IsAllocated ? _arrayHandle.AddrOfPinnedObject() : IntPtr.Zero;
    public nuint Count => (nuint)Values.Length;

    public NativeStringArrayContext(string[] managed)
    {
        _contexts = new NativeStringContext[managed.Length];
        Values = new NativeString[managed.Length];

        for (int i = 0; i < managed.Length; i++)
        {
            _contexts[i] = new NativeStringContext(managed[i]);
            Values[i] = _contexts[i].Value;
        }

        if (Values.Length > 0)
            _arrayHandle = GCHandle.Alloc(Values, GCHandleType.Pinned);
    }

    public void Dispose()
    {
        if (_arrayHandle.IsAllocated)
            _arrayHandle.Free();

        foreach (var ctx in _contexts)
            ctx.Dispose();
    }
}
