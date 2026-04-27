// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.
//
// ovrtx interactive viewer — CPU-mapped path with WinForms display.
// Uses GDI+ Bitmap for display to avoid OpenGL/Vulkan driver conflicts with ovrtx.

using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using Nvidia.Ovrtx;
using Nvidia.Ovrtx.Interop;

namespace VulkanInterop;

class OvrtxViewerForm : Form
{
    const string DefaultUsdUrl =
        "https://omniverse-content-production.s3.us-west-2.amazonaws.com/Samples/Robot-OVRTX/robot-ovrtx.usda";

    readonly Renderer _renderer;
    readonly OrbitCamera _camera;
    readonly string _renderProductPath;
    readonly int _texWidth, _texHeight;

    Bitmap? _bitmap;
    bool _cameraDirty = true;
    bool _mouseDown;
    float _lastMx, _lastMy;
    bool _firstFrame = true;
    readonly System.Windows.Forms.Timer _timer;

    public OvrtxViewerForm(Renderer renderer, OrbitCamera camera, string renderProductPath, int texWidth, int texHeight)
    {
        _renderer = renderer;
        _camera = camera;
        _renderProductPath = renderProductPath;
        _texWidth = texWidth;
        _texHeight = texHeight;

        Text = "ovrtx C# Viewer";
        ClientSize = new Size(1280, 720);
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

        _bitmap = new Bitmap(_texWidth, _texHeight, PixelFormat.Format32bppArgb);

        MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) _mouseDown = true; };
        MouseUp += (_, e) => { if (e.Button == MouseButtons.Left) _mouseDown = false; };
        MouseMove += (_, e) =>
        {
            if (_mouseDown)
            {
                _camera.Update(e.X - _lastMx, e.Y - _lastMy);
                _cameraDirty = true;
            }
            _lastMx = e.X;
            _lastMy = e.Y;
        };
        MouseWheel += (_, e) =>
        {
            float d = _camera.Distance * (1.0f - e.Delta / 1200.0f);
            _camera.SetDistance(d);
            _cameraDirty = true;
        };

        // Timer drives the render loop at ~30 fps
        _timer = new System.Windows.Forms.Timer { Interval = 33 };
        _timer.Tick += (_, _) => RenderFrame();
        _timer.Start();
    }

    unsafe void RenderFrame()
    {
        if (_cameraDirty)
        {
            WriteCameraTransform();
            _cameraDirty = false;
        }

        using var products = _renderer.Step([_renderProductPath], deltaTime: _firstFrame ? 0.0 : 1.0 / 30.0);
        _firstFrame = false;

        foreach (var (_, product) in products.Products)
        foreach (var frame in product.Frames)
        {
            var varName = frame.RenderVars.ContainsKey("LdrColor") ? "LdrColor" : "HdrColor";
            if (!frame.RenderVars.TryGetValue(varName, out var rv)) continue;

            using var mapped = rv.Map(MapDeviceType.Cpu);
            var tensor = mapped.Tensor;
            int w = (int)tensor.GetShape()[1];
            int h = (int)tensor.GetShape()[0];

            if (_bitmap == null || _bitmap.Width != w || _bitmap.Height != h)
            {
                _bitmap?.Dispose();
                _bitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            }

            // Copy RGBA pixels from tensor to bitmap (RGBA -> BGRA swap)
            var bmpData = _bitmap.LockBits(
                new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            byte* src = (byte*)tensor.Data;
            byte* dst = (byte*)bmpData.Scan0;
            int srcStride = w * 4;
            int dstStride = bmpData.Stride;

            for (int y = 0; y < h; y++)
            {
                byte* srcRow = src + y * srcStride;
                byte* dstRow = dst + y * dstStride;
                for (int x = 0; x < w; x++)
                {
                    // RGBA -> BGRA
                    dstRow[x * 4 + 0] = srcRow[x * 4 + 2]; // B
                    dstRow[x * 4 + 1] = srcRow[x * 4 + 1]; // G
                    dstRow[x * 4 + 2] = srcRow[x * 4 + 0]; // R
                    dstRow[x * 4 + 3] = srcRow[x * 4 + 3]; // A
                }
            }

            _bitmap.UnlockBits(bmpData);
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (_bitmap != null)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
            e.Graphics.DrawImage(_bitmap, 0, 0, ClientSize.Width, ClientSize.Height);
        }
    }

    unsafe void WriteCameraTransform()
    {
        double[] xform = _camera.TransformMatrix();
        fixed (double* p = xform)
        {
            long shape = 1;
            var tensor = new DLTensor
            {
                Data = (IntPtr)p,
                Device = DLDevice.Cpu,
                NDim = 1,
                DType = new DLDataType(DLDataTypeCode.Float, 64, 16),
                Shape = (IntPtr)(&shape),
                Strides = IntPtr.Zero,
                ByteOffset = 0,
            };
            _renderer.WriteAttribute(
                primPaths: ["/World/Camera"],
                attributeName: "omni:xform",
                tensor: &tensor,
                semantic: Semantic.XformMat4x4,
                dataAccess: DataAccess.Sync);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            _bitmap?.Dispose();
        }
        base.Dispose(disposing);
    }
}

class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        string usdFile = "https://omniverse-content-production.s3.us-west-2.amazonaws.com/Samples/Robot-OVRTX/robot-ovrtx.usda";
        string renderProductPath = "/Render/Camera";
        var upAxis = UpAxis.Z;
        float unitScale = 1.0f;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--usd" or "-u": usdFile = args[++i]; break;
                case "--render-product" or "-r": renderProductPath = args[++i]; break;
                case "--up-axis" or "-a":
                    upAxis = args[++i].ToUpperInvariant() == "Y" ? UpAxis.Y : UpAxis.Z; break;
                case "--units":
                    if (args[++i].StartsWith("c")) unitScale = 100.0f; break;
                case "--help" or "-h":
                    Console.Error.WriteLine("Usage: vulkan-interop [--usd <path>] [--render-product <path>] [--up-axis Y|Z] [--units meters|centimeters]");
                    return 0;
            }
        }

        Console.Error.WriteLine("Creating renderer...");
        var renderer = new Renderer();
        Console.Error.WriteLine("Renderer created.");

        Console.Error.WriteLine($"Loading {usdFile}...");
        renderer.AddUsd(usdFile);
        Console.Error.WriteLine("USD loaded.");

        // Initial step for dimensions
        int texWidth = 0, texHeight = 0;
        using (var initProducts = renderer.Step([renderProductPath], deltaTime: 0.0))
        {
            foreach (var (_, product) in initProducts.Products)
            foreach (var frame in product.Frames)
            {
                var varName = frame.RenderVars.ContainsKey("LdrColor") ? "LdrColor" : "HdrColor";
                if (frame.RenderVars.TryGetValue(varName, out var rv))
                {
                    using var mapped = rv.Map(MapDeviceType.Cpu);
                    var shape = mapped.Tensor.GetShape();
                    texHeight = (int)shape[0];
                    texWidth = (int)shape[1];
                }
            }
        }

        if (texWidth == 0) { Console.Error.WriteLine("No output"); return 1; }
        Console.Error.WriteLine($"Output: {texWidth}x{texHeight}");

        float distance = 5.0f * unitScale;
        float azimuth = 290.0f * MathF.PI / 180.0f;
        float elevation = MathF.Asin(0.5f / 5.0f);
        var camera = new OrbitCamera(distance, azimuth, elevation, new Vector3(0, 0, 1), upAxis);

        Console.Error.WriteLine("Opening window...");
        Application.EnableVisualStyles();
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        using var form = new OvrtxViewerForm(renderer, camera, renderProductPath, texWidth, texHeight);
        Application.Run(form);

        renderer.Dispose();
        Console.Error.WriteLine("Done!");
        return 0;
    }
}
