// SPDX-FileCopyrightText: Copyright (c) 2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: LicenseRef-NvidiaProprietary

using System.Numerics;

namespace VulkanInterop;

public enum UpAxis { Y, Z }

/// <summary>
/// Spherical-coordinate orbit camera with configurable up axis.
/// Port of examples/c/vulkan-interop/src/camera/orbit_camera.cpp.
/// </summary>
public class OrbitCamera
{
    private float _distance;
    private float _azimuth;   // radians
    private float _elevation; // radians
    private Vector3 _target;
    private readonly UpAxis _upAxis;

    private const float Sensitivity = 0.005f;
    private const float MaxElevation = MathF.PI / 2.0f - 0.01f;

    public OrbitCamera(float distance, float azimuth, float elevation, Vector3 target, UpAxis upAxis)
    {
        _distance = distance;
        _azimuth = azimuth;
        _elevation = elevation;
        _target = target;
        _upAxis = upAxis;
    }

    public float Distance => _distance;
    public void SetDistance(float d) => _distance = MathF.Max(d, 0.1f);

    public Vector3 Position
    {
        get
        {
            float cosEl = MathF.Cos(_elevation);
            float x = _distance * cosEl * MathF.Cos(_azimuth);
            float y = _distance * cosEl * MathF.Sin(_azimuth);
            float z = _distance * MathF.Sin(_elevation);

            return _upAxis == UpAxis.Z
                ? _target + new Vector3(x, y, z)
                : _target + new Vector3(x, z, -y); // Y-up remapping
        }
    }

    public void Update(float deltaX, float deltaY)
    {
        _azimuth -= deltaX * Sensitivity;
        _elevation = Math.Clamp(_elevation + deltaY * Sensitivity, -MaxElevation, MaxElevation);
    }

    /// <summary>
    /// Returns a 4x4 camera-to-world transform matrix (row-major doubles for ovrtx).
    /// </summary>
    public double[] TransformMatrix()
    {
        var eye = Position;
        var forward = Vector3.Normalize(_target - eye);
        var worldUp = _upAxis == UpAxis.Z ? Vector3.UnitZ : Vector3.UnitY;
        var right = Vector3.Normalize(Vector3.Cross(forward, worldUp));
        var up = Vector3.Cross(right, forward);

        // Row-major 4x4 matrix: [right | up | -forward | eye]
        // ovrtx expects camera-to-world (columns = right, up, -forward, position)
        return new double[]
        {
            right.X,    right.Y,    right.Z,    0,
            up.X,       up.Y,       up.Z,       0,
            -forward.X, -forward.Y, -forward.Z, 0,
            eye.X,      eye.Y,      eye.Z,      1,
        };
    }
}
