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
| `interop.zip` | The assemblies (`*.dll`) and BepInEx's `assembly-hash.txt`, at the top level of the zip. |

`manifest.json`:

```json
{
  "format": 1,
  "gameVersion": "13.0.8",
  "unityVersion": "6000.3.24f1",
  "metadataSha256": "<sha-256 of the game's global-metadata.dat>",
  "assemblyHash": "<contents of assembly-hash.txt>",
  "interop": { "file": "interop.zip", "size": 0, "sha256": "<sha-256 of interop.zip>", "files": 0 }
}
```

## How the loader uses it

1. It looks for `https://github.com/SBAModding/AssemblyRepository/releases/download/<game version>/manifest.json`.
2. It only uses the set when the `metadataSha256` matches the game's own `global-metadata.dat` on the device, and the
   downloaded zip matches `interop.sha256` and `interop.size`.
3. Anything else (no release, no network, a mismatch) falls back to generating the assemblies on the device.
4. If a downloaded set fails three launches in a row, the loader discards it and generates on the device instead.

## Adding a game version

Generate the set on a device that has enough memory (launch the game once with Fusion on and a clean
`BepInEx/interop` folder), then zip the `*.dll` files and `assembly-hash.txt` from that folder, write the manifest and
publish a release named after the game version. The `.db` cache files in that folder are not included; they are rebuilt
on demand.
