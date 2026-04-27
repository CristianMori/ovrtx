// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.
//

namespace Nvidia.Ovrtx;

public enum ApiStatus
{
    Success = 0,
    Error = 1,
    Timeout = 2,
}

public enum EventStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
}

public enum Semantic
{
    None = 0,
    XformMat4x4 = 1,
    XformPosRotScale = 2,
    XformPosRot3x3 = 3,
    PathString = 4,
    TokenString = 5,
}

public enum PrimMode
{
    ExistingOnly = 0,
    MustExist = 1,
    CreateNew = 2,
}

public enum DataAccess
{
    Async = 0,
    Sync = 1,
}

public enum MapDeviceType
{
    Default = 0,
    Cpu = 1,
    Cuda = 2,
    CudaArray = 3,
}

public enum Device
{
    Cpu = 0,
    Cuda = 1,
}

[Flags]
public enum BindingFlag
{
    None = 0,
    Optimize = 1 << 0,
}

public enum LogSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
}
