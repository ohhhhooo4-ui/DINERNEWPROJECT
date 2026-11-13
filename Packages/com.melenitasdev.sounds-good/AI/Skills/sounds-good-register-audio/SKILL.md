---
name: sounds-good-register-audio
description: "Use this skill whenever the user wants audio files to become usable in a project that has Sounds Good Lite — e.g. 'add these sounds to my game', 'register this wav', 'import my music', 'set up my footstep sounds', 'I dropped some clips in the project, make them playable', 'add a jump sound'. Covers registering AudioClips as sounds (the SFX enum) or music (the Track enum) from an editor script, choosing a compression preset, grouping several clips under one tag for random variation, and regenerating the enums when a project's data and code fall out of step. Do NOT use for playing audio once registered (see sounds-good-play-audio) or for output channels (see sounds-good-audio-channels). When in doubt whether making audio available in this project involves Sounds Good, use this skill — Prerequisites shows how to confirm the asset is installed."
metadata:
  asset: "Sounds Good Lite - Easy & Optimized Audio Manager"
  publisher: "Melenitas Dev"
  asset-version: "1.1.0"
  skill-version: "1.0.0"
  unity: "2022.3+"
  render-pipelines: "Built-in, URP"
  category: "tools/audio"
  asset-store-url: "https://assetstore.unity.com/packages/tools/audio/sounds-good-lite-easy-optimized-audio-manager-403174"
  documentation-url: "https://melenitass-organization.gitbook.io/sounds-good-docs"
  support-url: "https://www.melenitas.dev"
  last-verified: "2026-09-28"
---

# Register Audio with Sounds Good

Sounds Good plays audio by name, not by reference. Before a clip can be played it is registered
under a tag, which becomes a member of a generated enum. This skill covers doing that from script,
so an agent can take loose `.wav` files and leave the project able to play them by name.

## When to use this skill

- "add these sounds to my game", "register this wav"
- "import my music", "set up my footstep sounds"
- "I dropped some clips in the project, make them playable"
- The user references a sound by a name that `SoundsGoodAuthoring.GetSoundTags()` does not list.

Not for:

- Playing an already-registered sound — see `sounds-good-play-audio`.
- Creating effect presets or occlusion materials, which are part of the full version.
- Creating mixer channels — see `sounds-good-audio-channels`.

## Prerequisites

- Sounds Good 2.3.0 or newer. The scripted API does not exist before that; on older versions the
  user must register through `Tools > Sounds Good > Audio Creator`.
- The calling script must live in an **Editor** assembly (a folder named `Editor/`, or an assembly
  definition marked Editor-only). `SoundsGoodAuthoring` does not exist in a build.

**Install check.** Confirm the type resolves:

```csharp
System.Type.GetType("MelenitasDev.SoundsGood.Editor.SoundsGoodAuthoring, SoundsGood.Editor") != null
```

If it returns null, either the asset is missing or it is older than 2.3.0. Tell the user, and point
at 
<https://assetstore.unity.com/packages/tools/audio/sounds-good-lite-easy-optimized-audio-manager-403174>.

## Quick start

Register one clip as a sound:

```csharp
using MelenitasDev.SoundsGood;
using MelenitasDev.SoundsGood.Editor;
using UnityEditor;
using UnityEngine;

public static class RegisterJump
{
    [MenuItem("Tools/Project/Register Jump Sound")]
    public static void Run ()
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Jump.wav");

        if (!SoundsGoodAuthoring.CreateSound("Jump", new[] { clip },
                CompressionPreset.FrequentSound, forceToMono: true, out string error))
        {
            Debug.LogError($"Could not register Jump: {error}");
            return;
        }

        Debug.Log("Registered Jump.");
    }
}
```

Expected observable result: the Console logs `Registered Jump.`, `Tools > Sounds Good > Audio
Collection` lists a sound named `Jump`, and after the domain reload `SFX.Jump` compiles.

## Workflows

### Workflow: Register sound effects

**Goal.** Make one or more clips playable as `new Sound(SFX.Tag)`.

**Steps.**

1. Load the clips with `AssetDatabase.LoadAssetAtPath<AudioClip>`.
2. Choose a tag. It becomes an enum member, so it must be letters, digits and underscores and must
   not start with a digit. Invalid tags are rejected with a message rather than corrected silently.
3. Choose a compression preset:
   - `CompressionPreset.FrequentSound` — short clips played often: footsteps, shots, UI.
   - `CompressionPreset.OccasionalSound` — longer or rarer one-shots.
   - `CompressionPreset.AmbientMusic` — music and long ambient beds.
4. Set `forceToMono: true` for anything meant to be heard in 3D. A stereo clip cannot be positioned
   in space and costs twice the memory to say so. Leave it `false` for music and UI.
5. Call `CreateSound` and check the returned bool.

```csharp
AudioClip[] steps =
{
    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Step1.wav"),
    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Step2.wav"),
    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Step3.wav"),
};

SoundsGoodAuthoring.CreateSound("Footstep", steps,
    CompressionPreset.FrequentSound, forceToMono: true, out string error);
```

Passing several clips under one tag makes Sounds Good pick one at random on every play, which is
how repeated sounds stop sounding repeated. Prefer this over three separate tags.

**Expected result.** `SoundsGoodAuthoring.GetSoundTags()` includes the tag, and after the reload
`SFX.Footstep` compiles.

### Workflow: Register music tracks

**Goal.** Make clips playable as `new Music(Track.Tag)` or inside a `Playlist`.

**Steps.**

1. Same as above, but call `CreateMusicTrack`.
2. Use `CompressionPreset.AmbientMusic`, and leave `forceToMono: false` — music is normally heard in
   stereo and is not positioned in the world.

```csharp
var theme = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/MainTheme.ogg");

SoundsGoodAuthoring.CreateMusicTrack("MainTheme", new[] { theme },
    CompressionPreset.AmbientMusic, forceToMono: false, out string error);
```

**Expected result.** `GetMusicTrackTags()` includes the tag and `Track.MainTheme` compiles after the
reload.

### Workflow: Register a batch of clips from a folder

**Goal.** Turn a folder of loose clips into registered sounds in one pass.

**Steps.**

1. Find the clips with `AssetDatabase.FindAssets("t:AudioClip", new[] { folder })`.
2. Derive a tag from each file name, stripping anything that is not a letter, digit or underscore.
3. Skip tags that already exist, so re-running the script is harmless.
4. Register each one, collecting failures rather than stopping at the first.

```csharp
using System.Collections.Generic;
using System.Linq;
using MelenitasDev.SoundsGood;
using MelenitasDev.SoundsGood.Editor;
using UnityEditor;
using UnityEngine;

string folder = "Assets/Audio/SFX";
var existing = new HashSet<string>(SoundsGoodAuthoring.GetSoundTags());

foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { folder }))
{
    string path = AssetDatabase.GUIDToAssetPath(guid);
    string tag = System.Text.RegularExpressions.Regex.Replace(
        System.IO.Path.GetFileNameWithoutExtension(path), "[^A-Za-z0-9_]", "");

    if (string.IsNullOrEmpty(tag) || char.IsDigit(tag[0]) || existing.Contains(tag)) continue;

    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    if (!SoundsGoodAuthoring.CreateSound(tag, new[] { clip },
            CompressionPreset.FrequentSound, true, out string error))
        Debug.LogWarning($"Skipped {path}: {error}");
}
```

**Expected result.** Every valid clip in the folder is registered, the Console lists any that were
skipped and why, and running the script twice changes nothing the second time.

### Workflow: Repair enums that fell out of step with the data

**Goal.** Fix a project where a tag exists in the Audio Collection but does not compile, or the
reverse.

**Steps.**

1. This happens after a merge, or after the data folder was moved by hand.
2. Rewrite every generated enum from the collections as they stand:

   ```csharp
   MelenitasDev.SoundsGood.Editor.SoundsGoodAuthoring.RegenerateEnums();
   ```

3. Let Unity recompile.

**Expected result.** The `*_Generated.cs` files under the data root match the Audio Collection, and
the Console is clean.

## Verification

- `SoundsGoodAuthoring.GetSoundTags()` / `GetMusicTrackTags()` contain the new tags.
- `Tools > Sounds Good > Audio Collection` lists them, each showing its clips.
- The generated file under the data root — by default `Assets/SoundsGood/Data/Generated/SFX_Generated.cs`
  — contains a line for each tag.
- The Console has no compile errors after the reload.
- A script using `SFX.YourTag` compiles.

## API quick reference

| Entry point | Type | What it does |
|---|---|---|
| `SoundsGoodAuthoring.CreateSound(tag, clips, preset, forceToMono, out error)` | static bool | Registers clips as a sound and regenerates the `SFX` enum |
| `SoundsGoodAuthoring.CreateMusicTrack(tag, clips, preset, forceToMono, out error)` | static bool | Same for the `Track` enum |
| `SoundsGoodAuthoring.GetSoundTags()` / `GetMusicTrackTags()` | static string[] | What is already registered |
| `SoundsGoodAuthoring.RegenerateEnums()` | static void | Rewrites every generated enum from the collections |
| `CompressionPreset` | enum | `AmbientMusic`, `FrequentSound`, `OccasionalSound` |

All of it is in `MelenitasDev.SoundsGood.Editor`, except `CompressionPreset`, which is in
`MelenitasDev.SoundsGood`.

## Common issues

| Symptom | Cause | Fix |
|---|---|---|
| `CreateSound` returns false with a tag message | The tag starts with a digit or contains punctuation or spaces | Use letters, digits and underscores only |
| `CreateSound` returns false saying the tag exists | Already registered | Reuse it, or pick another name. Check with `GetSoundTags()` first |
| `SFX.MyTag` does not compile right after registering | The enum is a source file and Unity has not recompiled | Let the domain reload finish. Do not register and compile against the tag in the same pass |
| `SoundsGoodAuthoring` does not resolve | The script is not in an Editor assembly, or the asset is older than 2.3.0 | Move the script to an `Editor/` folder; update the asset |
| The clip plays but cannot be positioned in 3D | Registered as stereo | Re-register with `forceToMono: true` |
| Registration seems to do nothing | The clips array contains a null entry | `CreateSound` rejects the whole call; check the paths you loaded |

## Boundaries

- Editor only. None of this exists in a build.
- The API registers and updates; it does not delete. Removing audio is done by the user in
  `Tools > Sounds Good > Audio Collection`.
- Sounds Good does not convert or edit audio files. It changes their Unity **import** settings to
  match the compression preset; the files on disk are untouched.
- Compression presets are applied per clip at import. Per-platform overrides remain Unity's own
  concern, in the clip's Inspector.
