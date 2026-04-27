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

internal enum NativeApiStatus : int
{
    Success = 0,
    Error = 1,
    Timeout = 2,
}

internal enum NativeEventStatus : int
{
    Pending = 0,
    Completed = 1,
    Failure = 2,
}

internal enum NativeRendererEventStatus : int
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
}

internal enum NativeConfigKeyType : int
{
    Bool = 0,
    Int64 = 1,
    UInt64 = 2,
    Double = 3,
    String = 4,
    Blob = 5,
}

internal enum NativeConfigBoolKey : int
{
    SyncMode = 0,
    EnableProfiling = 1,
    ReadGpuTransforms = 2,
    OutputPartialFrames = 3,
    KeepSystemAlive = 4,
    UseVulkan = 5,
}

internal enum NativeConfigStringKey : int
{
    BinaryPackageRootPath = 0,
    LogFilePath = 1,
    LogLevel = 2,
    ActiveCudaGpus = 3,
}

internal enum NativeAttributeSemantic : int
{
    None = 0,
    XformMat4x4 = 1,
    XformPos3dRot4fScale3f = 2,
    XformPos3dRot3x3f = 3,
    PathString = 4,
    TokenString = 5,
}

internal enum NativeBindingPrimMode : int
{
    ExistingOnly = 0,
    MustExist = 1,
    CreateNew = 2,
}

internal enum NativeBindingFlag : int
{
    None = 0,
    Optimize = 1 << 0,
}

internal enum NativeDataAccess : int
{
    Async = 0,
    Sync = 1,
}

internal enum NativeMapDeviceType : int
{
    Default = 0,
    Cpu = 1,
    Cuda = 2,
    CudaArray = 3,
}

internal enum NativeLogSeverity : int
{
    Info = 0,
    Warning = 1,
    Error = 2,
}
