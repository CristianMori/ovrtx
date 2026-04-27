# ovrtx Examples

This directory contains example projects demonstrating various features of ovrtx.

## C Examples

<table>
  <tr>
    <td align="center" width="50%">
      <a href="c/minimal/">
        <img src="../img/example-minimal.jpg" alt="Minimal Example" width="100%">
      </a>
      <br>
      <b>Minimal</b>
      <br>
      <sub>Basic renderer initialization, rendering a single frame from an RGB camera and writing the result to disk as a PNG.</sub>
    </td>
    <td align="center" width="50%">
      <a href="c/vulkan-interop/">
        <img src="../img/example-vulkan-interop.gif" alt="Vulkan Interop Example" width="100%">
      </a>
      <br>
      <b>Vulkan Interop</b>
      <br>
      <sub>Demonstrates ovrtx-Vulkan interoperability, rendering USD scenes and displaying them in a Vulkan window with interactive orbit camera control.</sub>
    </td>
  </tr>
</table>

## C# Examples

<table>
  <tr>
    <td align="center" width="50%">
      <a href="csharp/minimal/">
        <img src="../img/example-minimal.jpg" alt="Minimal Example" width="100%">
      </a>
      <br>
      <b>Minimal</b>
      <br>
      <sub>Basic workflow: create a Renderer, load a USD layer, step the renderer, and map/display the rendered output using the C# wrapper.</sub>
    </td>
    <td align="center" width="50%">
      <a href="csharp/vulkan-interop/">
        <img src="../img/example-vulkan-interop.gif" alt="Interactive Viewer Example" width="100%">
      </a>
      <br>
      <b>Interactive Viewer</b>
      <br>
      <sub>Demonstrates ovrtx rendering with interactive orbit camera control, displaying rendered frames in a WinForms window via CPU-mapped output.</sub>
    </td>
  </tr>
</table>

## Python Examples

<table>
  <tr>
    <td align="center" width="50%">
      <a href="python/minimal/">
        <img src="../img/example-minimal.jpg" alt="Minimal Example" width="100%">
      </a>
      <br>
      <b>Minimal</b>
      <br>
      <sub>Basic workflow: create a Renderer, load a USD layer, step the renderer, and map/display the rendered output.</sub>
    </td>
    <td align="center" width="50%">
      <a href="python/planet-system/">
        <img src="../img/example-planet-system.jpg" alt="Planet System Example" width="100%">
      </a>
      <br>
      <b>Planet System</b>
      <br>
      <sub>Animated planetary system using Warp kernels for GPU-accelerated animation, demonstrating dynamic scene modification and zero-copy transform updates.</sub>
    </td>
  </tr>
</table>
