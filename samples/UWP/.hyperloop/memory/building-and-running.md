---
summary: Building and running the UWP samples
---

# Building and running the UWP samples

Both UWP samples build the same app from `samples/UWP/ReswPlusUWPSample.Shared`. Only the project file differs: `ReswPlusUWPSample` is .NET Native, `ReswPlusNativeAotUwpSample` is modern .NET.

## Build

CI builds the whole solution, so mirror it:

```
msbuild ReswPlus.slnx /p:Configuration=Debug /p:Platform=x64
```

A UWP sample needs a developer command prompt (`VsDevCmd.bat`). Run `nuget restore ReswPlus.slnx` before the first build; do not run `msbuild /t:Restore` on the .NET Native projects, it fails with "One of your dependencies requires the .NET Framework".

Unit tests are plain SDK-style: `dotnet test tests/ReswPlusUnitTests/ReswPlusUnitTests.csproj`.

## Launching the Native AOT sample

It is a packaged app: register the built manifest, then launch by AUMID.

```
Add-AppxPackage -Register <repo>\samples\UWP\ReswPlusNativeAotUwpSample\bin\x64\<config>\net10.0-windows10.0.26100.0\AppxManifest.xml
hyperloop launch local:// --aumid <PackageFamilyName>!App --timeout 60
```

`hyperloop launch --app "<Start menu name>"` times out for this app even though the window does appear; launching by AUMID works.

## Publishing with Native AOT

`PublishAot` and `RuntimeIdentifiers` are declared in the project, and the publish profiles under `Properties/PublishProfiles` carry only the runtime identifier. Publish one architecture at a time from a developer command prompt matching it, because the AOT link step needs the MSVC linker:

```n msbuild /t:Publish /p:Configuration=Release /p:PublishProfile=win-x64
```n
All three of `win-x64`, `win-arm64` and `win-x86` publish. A real AOT publish leaves a single native `.exe` of a few megabytes and no `.dll` files beside it; if the output has ~190 DLLs and a small `.exe`, the AOT compiler was never restored.

## Symptom -> Recovery

- App exits immediately with `0xc0000409` and the crash record shows an empty `Faulting package full name` -> it is running unpackaged. UWP XAML needs package identity. The project must set `EnableMsixTooling`, and the IDE must launch the `MsixPackage` profile from `Properties/launchSettings.json`.
- App exits with `0xc0000409` and there is no `resources.pri` in the output -> `EnableMsixTooling` is missing. Without it the SDK does not include `PRIResource` items and no PRI is built, so every resource lookup fails.
- App exits with `0xc000027b` inside `Windows.UI.Xaml.dll` in Release -> `PublishAot` is set without `RuntimeIdentifiers`. Declare both in the project: `PublishAot` has to be visible at restore time for the AOT compiler to be brought in, and it needs the runtime identifiers to resolve against.
- `NETSDK1022 Duplicate PRIResource items` -> the project lists `.resw` explicitly while `EnableMsixTooling` already includes them by default. Remove the explicit items.

_Confirmed: Published win-x64, win-arm64 and win-x86 with Native AOT: each produced a single native exe (5.35/5.52/4.54 MB) with zero DLLs and a resources.pri. Confirmed the 0xc000027b crash was PublishAot without RuntimeIdentifiers, not AOT itself, by building Release both ways and launching the app._
