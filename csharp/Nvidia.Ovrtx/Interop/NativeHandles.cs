// SPDX-FileCopyrightText: Copyright (c) 2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: LicenseRef-NvidiaProprietary
//
// NVIDIA CORPORATION, its affiliates and licensors retain all intellectual
// property and proprietary rights in and to this material, related
// documentation and any modifications thereto. Any use, reproduction,
// disclosure or distribution of this material and related documentation
// without an express license agreement from NVIDIA CORPORATION or
// its affiliates is strictly prohibited.

namespace Nvidia.Ovrtx.Interop;

public readonly record struct UsdHandle(ulong Value)
{
    public bool IsValid => Value != 0;
    public static readonly UsdHandle Invalid = new(0);
}

public readonly record struct StepResultHandle(ulong Value)
{
    public bool IsValid => Value != 0;
    public static readonly StepResultHandle Invalid = new(0);
}

public readonly record struct OpId(ulong Value)
{
    public bool IsValid => Value != 0;
    public static readonly OpId Invalid = new(0);
}

public readonly record struct AttributeBindingHandle(ulong Value)
{
    public bool IsValid => Value != 0;
    public static readonly AttributeBindingHandle Invalid = new(0);
}

public readonly record struct MapHandle(ulong Value)
{
    public bool IsValid => Value != 0;
    public static readonly MapHandle Invalid = new(0);
}

public readonly record struct RenderedOutputHandle(ulong Value)
{
    public bool IsValid => Value != 0;
    public static readonly RenderedOutputHandle Invalid = new(0);
}

public readonly record struct RenderedOutputMapHandle(ulong Value)
{
    public bool IsValid => Value != 0;
    public static readonly RenderedOutputMapHandle Invalid = new(0);
}
