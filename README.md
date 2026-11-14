# AssemblyRepository

Pre-generated IL2CPP interop assemblies for [SBAModLoader](https://github.com/SBAModding/SBAModLoader).

The first time the loader starts Fusion (the part that runs plugin mods) for a game version, BepInEx builds a set of
interop assemblies from the game's metadata. That takes a minute or two and a lot of memory, more than many phones
have. Because the result only depends on the game build, it can be built once and downloaded instead.

## How it is published

One **release** per game version. The tag is the game's version name, exactly as Android reports it (for example
`13.0.8`). Each release has two assets:

| Asset | What it is |
|---|---|
| `manifest.json` | Describes the set and how to check it. |
| `interop.zip` | The assemblies (`*.dll`) and `assembly-hash.txt`, at the top level of the zip. |

`manifest.json`:

```json
{
  "format": 1,
  "gameVersion": "13.0.8",
  "unityVersion": "6000.3.24f1",
  "metadataSha256": "<sha-256 of the game's global-metadata.dat>",
  "gameAssemblySha256": "<sha-256 of the patched libil2cpp.so the hash below was computed from (informational)>",
  "assemblyHash": "<contents of assembly-hash.txt>",
  "generators": { "il2cppInterop": "1.5.2.0", "cpp2il": "2022.1.0.0" },
  "interop": { "file": "interop.zip", "size": 0, "sha256": "<sha-256 of interop.zip>", "files": 0 }
}
```

### The hash in `assembly-hash.txt`

BepInEx only keeps an existing set when `assembly-hash.txt` equals an MD5 it computes itself: the patched
`libil2cpp.so`, then (if the folder exists) every `*.dll` in `unity-libs` in directory order, then the versions of
Il2CppInterop's generator and Cpp2IL. A device that has not built a set yet has no `unity-libs` folder, and the
directory order of that folder differs between filesystems, so the hash stored in a published set is the one for **no
`unity-libs` folder**, which is the same on every device. `tools/ComputeHash` computes it:

```
dotnet run --project tools/ComputeHash -- <patched libil2cpp.so> <Il2CppInterop.Generator.dll> <Cpp2IL.Core.dll>
```

The loader removes any existing `unity-libs` folder when it installs a set, so the hash BepInEx computes matches.
If anything differs (another patch, other BepInEx versions), BepInEx simply regenerates the set.

## How the loader uses it

1. It looks for `https://github.com/SBAModding/AssemblyRepository/releases/download/<game version>/manifest.json`.
2. It only uses the set when the `metadataSha256` matches the game's own `global-metadata.dat` on the device, and the
   downloaded zip matches `interop.sha256` and `interop.size`.
3. Anything else (no release, no network, a mismatch) falls back to generating the assemblies on the device.
4. If a downloaded set fails three launches in a row, the loader discards it and generates on the device instead.

## Adding a game version

1. On a device with enough memory, launch the game once with Fusion on and nothing in `BepInEx/interop`, so BepInEx
   generates the set.
2. Zip the `*.dll` files from `BepInEx/interop` (not the `.db` caches, which are rebuilt on demand).
3. Compute the hash with `tools/ComputeHash` (above) from the device's patched `libil2cpp.so`
   (`app_sba_runtime/<game package>/libil2cpp.so` in the loader's data) and the two BepInEx dlls, and add it to the zip
   as `assembly-hash.txt`.
4. Write `manifest.json` (the sha-256 of the game's `global-metadata.dat` is `metadataSha256`) and publish a release
   named after the game version with `manifest.json` and `interop.zip`.
