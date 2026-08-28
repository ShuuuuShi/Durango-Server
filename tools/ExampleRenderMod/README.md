# ExampleRenderMod

This sample demonstrates the native client method-override API and the client rendering API.

Build it with:

```powershell
dotnet build tools\ExampleRenderMod\ExampleRenderMod.csproj -c Release
```

Install this package layout next to the game executable:

```text
mods/
  example-render-mod/
    ExampleRenderMod.dll
    assets/
      render-sample.bundle
```

Build `render-sample.bundle` for Windows with Unity 2017.4.34f1. The default prefab asset names are:

- `assets/models/custom-player.prefab`
- `assets/effects/sample-effect.prefab`

Set `DURANGO_RENDER_SAMPLE=1` before starting the game. F6 toggles the player model, F7 attaches the effect prefab, and F8 restores everything. Optional environment variables in `ExampleRenderPlugin.cs` can change the bundle path, SHA-256 and asset names.

Set `DURANGO_CLIENT_OVERRIDE_PROBE=1` to install a harmless postfix probe on `PlayerBehavior::GetCurrentPosition()`.
