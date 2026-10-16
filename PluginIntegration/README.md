# Milestone 2: the Dalamud plugin

The plugin lives in `src/AetherStylist.Plugin` in this repository. It opens **AetherStylist ♡** with `/aetherstylist` and displays a welcome message and a working selector with two fictional placeholder labels. Selection lasts until the plugin is unloaded; nothing is saved yet.

This milestone does not read game data, invoke the recommendation engine, analyse images or integrate with Glamourer. `AetherStylist.Core` remains unchanged and is referenced as a separate project.

## Build on Windows

You need the **.NET 10 SDK** and **Dalamud API 15** development assemblies. Following the [official SamplePlugin project](https://github.com/goatcorp/SamplePlugin/blob/master/SamplePlugin/SamplePlugin.csproj), the project uses `Dalamud.NET.Sdk/15.0.0`. The [API 15 release notes](https://dalamud.dev/versions/v15/) specify .NET 10.

Normally, install XIVLauncher, enable Dalamud and launch the game once. The SDK looks for the development assemblies at:

```text
%APPDATA%\XIVLauncher\addon\Hooks\dev
```

For another installation location, set `DALAMUD_HOME` in the PowerShell session to the directory containing `Dalamud.dll` and `Dalamud.Bindings.ImGui.dll`:

```powershell
$env:DALAMUD_HOME = 'C:\path\to\Dalamud\dev'
```

Check that these assemblies are API 15. Pinning the project SDK does not install Dalamud itself or switch your game's API version. The library path and target framework defaults are supplied by the [official SDK](https://github.com/goatcorp/Dalamud.NET.Sdk/blob/master/Dalamud.NET.Sdk/Sdk/Sdk.props).

From the repository root:

```powershell
dotnet build AetherStylist.sln -c Debug
dotnet run --project tests/AetherStylist.Specs
```

You can also build just the plugin:

```powershell
dotnet build src/AetherStylist.Plugin/AetherStylist.Plugin.csproj -c Debug -p:Platform=x64
```

The solution uses x64 for the plugin. Its output is in:

```text
src\AetherStylist.Plugin\bin\x64\Debug\
```

Keep `AetherStylist.Plugin.dll`, the generated `AetherStylist.Plugin.json` and `AetherStylist.Core.dll` together. The SDK's packager generates the manifest from the project metadata, including `InternalName`, `AssemblyVersion` and `DalamudApiLevel`; do not hand-edit the generated JSON. See [plugin metadata documentation](https://dalamud.dev/plugin-development/plugin-metadata/).

If no explicit `DALAMUD_HOME` is set and the installed Dalamud assemblies are missing, the plugin project automatically uses `.dalamud/api15` when that downloaded directory is present. This fallback works for both command-line builds and the editor's C# project loading. If none of these locations contain Dalamud, check the default directory or set `DALAMUD_HOME` to your API 15 development assemblies. The console demo and Core checks can still run without Dalamud.

### Verification in this checkout

The Debug solution build was verified on Windows with .NET SDK **10.0.401** and official Dalamud **15.0.3.6** assemblies, with zero warnings and errors. All ten existing Core checks passed, and the console demo ran successfully. The generated plugin manifest declares API 15 and the output includes `AetherStylist.Core.dll`. In-game testing is still pending.

The default Dalamud directory was absent, so the official [Dalamud distribution](https://github.com/goatcorp/dalamud-distrib) was downloaded to the ignored `.dalamud/api15` folder for compilation. The project now detects that copy automatically. To rebuild from this checkout's root:

```powershell
dotnet build AetherStylist.sln -c Debug
```

The downloaded assemblies and local NuGet cache are ignored by Git. They are build prerequisites for this checkout, not part of the plugin distribution, and do not install Dalamud into the game.

### VS Code reports missing Dalamud types

Errors such as `CS0246` for `Dalamud` or `IDalamudPlugin` mean that the C# project has not resolved the host's assembly references. The project SDK is a build helper; the actual API types live in `Dalamud.dll`. Missing `Window.Toggle()` or `IsOpen` can be follow-on errors because the editor could not resolve the base `Window` class.

Setting `$env:DALAMUD_HOME` in a PowerShell session affects that session and processes launched from it. An already running VS Code C# language server will not inherit it. The project's `.dalamud/api15` fallback avoids needing a terminal-only setting for this checkout.

After a path change, open the repository root in VS Code and run **Developer: Reload Window** from the Command Palette (`Ctrl+Shift+P`). If prompted to select a solution, choose `AetherStylist.sln`. On another checkout, install Dalamud or provide your own API 15 assemblies; the ignored downloaded folder is not included in Git.

## Load for local testing

Follow the [SamplePlugin local loading instructions](https://github.com/goatcorp/SamplePlugin#activating-in-game):

1. Launch FFXIV through XIVLauncher with Dalamud API 15 enabled.
2. Enter `/xlsettings`, open **Experimental**, and add the full path to **the plugin DLL** under **Dev Plugin Locations**. For this checkout and a Debug build:

   ```text
   C:\Users\YOUR_WINDOWS_USERNAME\Documents\AetherStylist\src\AetherStylist.Plugin\bin\x64\Debug\AetherStylist.Plugin.dll
   ```

3. Save settings. Enter `/xlplugins`, then open **Dev Tools > Installed Dev Plugins** and enable **AetherStylist**.
4. Enter `/aetherstylist`. The **AetherStylist ♡** window should open. Enter it again to close the window.

Add the DLL path rather than the whole output folder, which also contains the Core library. For a Release build, use the equivalent path under `bin\x64\Release`.

To try a code change, disable the dev plugin, rebuild, then enable it again. Dalamud also supports dev plugin hot reload; see the [development FAQ](https://dalamud.dev/faq/getting-started/#how-do-i-hot-reload-my-plugin). If loading fails, inspect `/xllog` and confirm the game's API version and the manifest's `DalamudApiLevel` match.

Manual checks in the game:

- Toggle the window with the command, close it with the title-bar button, then reopen it.
- Select each placeholder and clear the selection. Closing and reopening retains it while the plugin is loaded.
- Open the main UI through the plugin installer's button. This action always opens the window.
- Disable and re-enable the plugin, then check the command still works without duplicate callbacks. Re-enabling starts with an empty selection.

A successful build verifies compilation and packaging. These in-game checks are still needed to verify loading, drawing and the heart glyph with your installed fonts.

## Understanding the implementation

- **The project file** uses the same SDK and metadata approach as SamplePlugin. SDK 15 supplies `net10.0-windows`, C# 14, x64, the Dalamud/ImGui assembly references and packaging. The dependency lock file records the NuGet package versions. No separate ImGui.NET package is needed: this API uses `Dalamud.Bindings.ImGui`.
- **`Plugin : IDalamudPlugin`** is the entry point Dalamud discovers in the DLL. `[PluginService]` asks Dalamud to supply the plugin interface and command manager before the constructor runs. The `null!` initializer tells C#'s nullable checker that the host will supply the value; it does not construct a service. Only the two services needed by this milestone are requested. See the [entry point documentation](https://dalamud.dev/plugin-development/project-layout/#the-plugin-entrypoint).
- **The constructor** creates the registrations: the window, the slash-command callback and UI event handlers. `+=` subscribes a method to an event. The command callback calls `Window.Toggle()`, which changes `IsOpen`; the installer callback explicitly opens it. There is no asynchronous work, so the basic `IDalamudPlugin` lifecycle is sufficient.
- **`WindowSystem` and `MainWindow : Window`** separate plugin wiring from UI drawing. Dalamud manages opening/closing, sizing and the surrounding ImGui window. `Draw()` only describes its contents. `###AetherStylistMainWindow` gives ImGui a stable invisible identifier while the visible title stays **AetherStylist ♡**. The [Window API](https://dalamud.dev/api/Dalamud.Interface.Windowing/Classes/Window/) handles UI scaling for `Size` and `SizeConstraints`.
- **ImGui redraws the UI each frame.** The selected index is therefore a field on the window, not a local variable inside `Draw()`. `ref selectedItemIndex` lets `ImGui.Combo` update that field when you choose an option. The string array is created once and contains labels only, with no catalogue adapter or simulated recommendation engine.
- **`Dispose()` reverses the registrations.** `IDalamudPlugin` inherits `IDisposable`; Dalamud calls this on unload. `-=` removes the event handlers, the command handler is removed, and the windows are unregistered. The window owns no textures or other disposable resources, so it needs no empty disposal method. See the [IDalamudPlugin API](https://dalamud.dev/api/Dalamud.Plugin/Interfaces/IDalamudPlugin/).
- **`ProjectReference`** makes MSBuild build Core and copy its DLL alongside the plugin. Dependency direction remains Plugin → Core: the engine never needs to know about Dalamud. This milestone prepares the reference without calling the engine.
