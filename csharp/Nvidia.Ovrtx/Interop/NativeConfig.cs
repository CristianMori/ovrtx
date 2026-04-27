// SPDX-FileCopyrightText: Copyright (c) 2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: LicenseRef-NvidiaProprietary
//
// NVIDIA CORPORATION, its affiliates and licensors retain all intellectual
// property and proprietary rights in and to this material, related
// documentation and any modifications thereto. Any use, reproduction,
// disclosure or distribution of this material and related documentation
// without an express license agreement from NVIDIA CORPORATION or
// its affiliates is strictly prohibited.

using System.Runtime.InteropServices;

namespace Nvidia.Ovrtx.Interop;

/// <summary>
/// ovrtx_config_entry_t (24 bytes).
/// Contains a key_type discriminator, a key union (4 bytes), and a value union (16 bytes).
/// Uses explicit layout to handle the overlapping union fields.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 24)]
internal struct NativeConfigEntry
{
    // Discriminator
    [FieldOffset(0)] public int KeyType;

    // Key union (all key enums are int) at offset 4
    [FieldOffset(4)] public int KeyValue;

    // Value union at offset 8
    [FieldOffset(8)] public byte BoolValue;
    [FieldOffset(8)] public long IntValue;
    [FieldOffset(8)] public ulong UintValue;
    [FieldOffset(8)] public double DoubleValue;
    // For string and blob: ptr at offset 8, size/length at offset 16
    [FieldOffset(8)] public IntPtr StringOrBlobPtr;
    [FieldOffset(16)] public nuint StringOrBlobSize;

    public static NativeConfigEntry Bool(NativeConfigBoolKey key, bool value) => new()
    {
        KeyType = (int)NativeConfigKeyType.Bool,
        KeyValue = (int)key,
        BoolValue = value ? (byte)1 : (byte)0,
    };

    public static NativeConfigEntry String(NativeConfigStringKey key, NativeString value) => new()
    {
        KeyType = (int)NativeConfigKeyType.String,
        KeyValue = (int)key,
        StringOrBlobPtr = value.Ptr,
        StringOrBlobSize = value.Length,
    };
}

/// <summary>ovrtx_config_t (16 bytes).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeConfig
{
    public IntPtr Entries;      // const ovrtx_config_entry_t*
    public nuint EntryCount;    // size_t
}
