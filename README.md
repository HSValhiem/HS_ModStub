# HS_ModStub

The template HSValhiem starts every Valheim mod from. It is a BepInEx plugin skeleton with:

- **Config with server sync**: [ServerSync](https://github.com/blaxxun-boop/ServerSync). A server admin can lock the config, and clients must run the same mod version.
- **Version check**: warns when the game version differs from the one the mod was built for (`ValheimVersion` in the .csproj). A hidden "Allow Wrong Valheim Version" setting turns the check off.
- **Localization**: [LocalizationManager](https://github.com/blaxxun-boop/LocalizationManager). It embeds `translations/<Language>.yml`.
- **Harmony**: one sample patch in `Patches.cs`.
- **Build pipeline**: game DLLs are publicized, the library DLLs are fetched and merged into one plugin DLL, and Release builds produce a Thunderstore zip.

Targets Valheim 1.0.21, BepInExPack_Valheim 5.4.2360, .NET Framework 4.8.

## Requirements

- .NET SDK 8 or newer (`dotnet --version`). Visual Studio 2022 and Rider also work.
- A Valheim install, or a folder holding `assembly_valheim.dll` and the `UnityEngine*.dll` files from `valheim_Data/Managed`.

## Setup

1. Copy `Local.props.example` to `Local.props`. Git ignores `Local.props`.
2. In `Local.props`, set `ValheimDir` to the Valheim folder (the one with `valheim.exe`).
   Or set `ValheimManagedDir` to a folder holding the game DLLs.
3. Optional: set `DeployDir` to a BepInEx `plugins` folder. Each build copies the DLL there.

Instead of `Local.props`, you can set the `VALHEIM_DIR` environment variable or pass `-p:ValheimDir=...` on the command line.
If neither is set, the build uses the default Steam path on Windows.

## Build

```sh
dotnet build HS_ModStub.csproj -c Debug
```

Name the .csproj: the folder also holds `ModStub.sln`.

The first build downloads ServerSync and LocalizationManager into `lib/` from their GitHub releases.
It checks each download against the SHA-256 pinned in the .csproj and stops on a mismatch.
The output is `bin/Debug/HS_ModStub.dll`, with the libraries merged in.

## Package for Thunderstore

```sh
dotnet build HS_ModStub.csproj -c Release
```

This writes `dist/<Name>-<Version>.zip` with `manifest.json`, `icon.png`, `README.md`, `CHANGELOG.md` and `plugins/<Name>.dll`, all taken from `ThunderStore/`.
The manifest's `version_number` is replaced with `<Version>` from the .csproj, so change the version in one place only.
The build refuses to package while `ThunderStore/manifest.json` still contains `TODO`.

## Examples

`Examples/` holds reference code that is not compiled by default:

- an ItemManager item from an embedded AssetBundle (see `Examples/assets/README.md`),
- config values shown inside translated text (localization placeholders),
- a manual Harmony patch.

Build with the examples:

```sh
dotnet build HS_ModStub.csproj -c Debug -p:IncludeExamples=true
```

This also downloads [ItemManager](https://github.com/blaxxun-boop/ItemManager). Copy what you need into your mod, then delete `Examples/`.

## Start a new mod

1. Copy this repository, or use it as a GitHub template.
2. Rename `HS_ModStub.csproj` to `<YourMod>.csproj`. The plugin name comes from the project name, and the GUID is `hs.<yourmod>` in lower case.
3. Change `namespace HS_ModStub` in `Plugin.cs` and `Patches.cs` to your mod's name. Update the project entry in `ModStub.sln`, or delete the .sln.
4. Rename the `hs_modstub_example` key in `translations/English.yml` and add your own strings.
5. In `ThunderStore/`: fill in `manifest.json` (name, description, website_url), replace `icon.png` (256x256 PNG), and rewrite `README.md` and `CHANGELOG.md`.
6. Set `<Version>` and, when you build against a new game version, `<ValheimVersion>` in the .csproj.

## Licences

This template is MIT-licensed (see `LICENSE`). The merged libraries (ServerSync, LocalizationManager, ItemManager) are by blaxxun and are MIT-0.
