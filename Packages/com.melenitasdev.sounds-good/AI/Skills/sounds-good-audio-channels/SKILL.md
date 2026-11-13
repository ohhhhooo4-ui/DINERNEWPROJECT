---
name: sounds-good-audio-channels
description: "Use this skill whenever the user wants separate volume controls or mixer routing in a project with Sounds Good Lite — e.g. 'add a music volume slider', 'separate sfx and music volume', 'make an options menu with audio settings', 'mute the music', 'set up audio channels', 'the volume doesn't save between sessions', 'route this sound to the voice channel'. Covers creating output channels from script (which builds the Audio Mixer group and exposes its volume for you), routing audio to them, changing channel volume at runtime with automatic saving, and dropping a ready-made volume slider into a UI. Do NOT use for the volume of a single sound (see sounds-good-play-audio) or for registering clips (see sounds-good-register-audio). When in doubt whether an audio-settings or volume-slider request involves Sounds Good, use this skill — Prerequisites shows how to confirm the asset is installed."
metadata:
  asset: "Sounds Good Lite - Easy & Optimized Audio Manager"
  publisher: "Melenitas Dev"
  asset-version: "1.1.0"
  skill-version: "1.1.0"
  unity: "2022.3+"
  render-pipelines: "Built-in, URP"
  category: "tools/audio"
  asset-store-url: "https://assetstore.unity.com/packages/tools/audio/sounds-good-lite-easy-optimized-audio-manager-403174"
  documentation-url: "https://melenitass-organization.gitbook.io/sounds-good-docs"
  support-url: "https://www.melenitas.dev"
  last-verified: "2026-10-01"
---

# Set Up Audio Channels with Sounds Good

An output channel is an Audio Mixer group with its volume exposed and wired up. Sounds Good builds
it for you from a name, so separating music from effects, and giving the player sliders for each,
does not involve the Audio Mixer's manual steps.

## When to use this skill

- "add a music volume slider", "separate sfx and music volume"
- "make an options menu with audio settings", "mute the music"
- "the volume doesn't save between sessions"
- "route this sound to the voice channel"

Not for:

- The volume of one sound — that is `SetVolume`, see `sounds-good-play-audio`.

## Prerequisites

- Sounds Good installed. Output channels exist in both editions.
- Sounds Good 2.3.0 or newer to create them from script; otherwise use
  `Tools > Sounds Good > Output Manager`.
- The creating script must be in an Editor assembly.

**Install check.** Confirm the menu `Tools > Sounds Good > Output Manager` exists, or that
`MelenitasDev.SoundsGood.Editor.SoundsGoodAuthoring` resolves.

## Quick start

```csharp
using MelenitasDev.SoundsGood.Editor;
using UnityEngine;

foreach (string name in new[] { "Music", "SFX", "Voice" })
    if (!SoundsGoodAuthoring.CreateOutput(name, out string error))
        Debug.LogWarning($"Could not create {name}: {error}");
```

After the reload:

```csharp
new Sound(SFX.Shot).SetOutput(Output.SFX).Play();
SoundsGoodManager.ChangeOutputVolume(Output.Music, 0.5f);
```

Expected observable result: the Audio Mixer contains the groups, `Tools > Sounds Good > Output
Manager` lists them, and `Output.Music` compiles.

## Workflows

### Workflow: Create the channels a game needs

**Goal.** Separate faders for the categories the player expects.

**Steps.**

1. Decide the set. Music, SFX and Voice covers most games; add UI or Ambience when they need their
   own slider.
2. Create them with `CreateOutput`. Each call builds the mixer group under Master, exposes its
   volume under the same name, and regenerates the `Output` enum.
3. Check `GetOutputNames()` first so re-running the script does not try to duplicate one.

**Expected result.** Each name is a member of `Output`, and the groups are visible in
`Window > Audio > Audio Mixer`.

### Workflow: Route audio to a channel

**Goal.** Every sound counted under the right fader.

**Steps.**

1. From code, chain `SetOutput` before playing:

   ```csharp
   new Sound(SFX.Shot).SetOutput(Output.SFX).Play();
   new Music(Track.MainTheme).SetOutput(Output.Music).SetLoop().Play();
   ```

2. On emitters, set the **Output** field in the Inspector.
3. Audio with no output set goes to the master.

**Expected result.** Lowering a channel's volume affects exactly the audio routed to it.

### Workflow: Change channel volume at runtime, and have it stick

**Goal.** An options menu whose settings survive restarting the game.

**Steps.**

```csharp
using MelenitasDev.SoundsGood;

// The player's own setting, from an options menu: saved, and still there next session.
SoundsGoodManager.ChangeOutputVolume(Output.Music, 0.5f);               // 0 to 1

// A change the player did not ask for: duck under a cutscene, then put it back.
SoundsGoodManager.ChangeOutputVolume(Output.Music, 0.2f, save: false);
SoundsGoodManager.RestoreSavedOutputVolume(Output.Music);
```

Choosing between those two matters. `save` defaults to true, which is right for a slider the
player dragged. Pass `save: false` for anything temporary — ducking, muting while the game is in
the background — because a saved duck becomes the player's setting for good: quit mid-cutscene
and the music stays quiet, with nothing in the options menu to explain why.

There is a string overload (`ChangeOutputVolume("Music", 0.5f)`) for when the channel is chosen
at runtime.

**Expected result.** The change is audible immediately. If it was saved, the value is still
there after a restart; if not, the next `RestoreSavedOutputVolume` brings the player's own level
back.

### Workflow: Add a working volume slider to a menu

**Goal.** A slider that controls a channel and remembers its position, without a script.

**Steps.**

1. With a Canvas selected, use `GameObject > Sounds Good > UI > Output Volume Slider`.
2. In the Inspector, set which output it controls.
3. It loads the saved value on start and writes changes back.

**Expected result.** Dragging the slider changes that channel's volume, and reopening the menu shows
the saved position.

## Verification

- `Window > Audio > Audio Mixer` shows a group per output under Master, each with an exposed
  parameter of the same name.
- `Tools > Sounds Good > Output Manager` lists them.
- `Output.YourName` compiles.
- In Play Mode, `ChangeOutputVolume` affects only audio routed to that channel.
- After restarting Play Mode, `GetLastSavedOutputVolume` returns the last value set.

## API quick reference

| Entry point | Type | What it does |
|---|---|---|
| `SoundsGoodAuthoring.CreateOutput(name, out error)` | static bool (Editor) | Builds the mixer group, exposes its volume, regenerates `Output` |
| `SoundsGoodAuthoring.GetOutputNames()` | static string[] | What already exists |
| `Sound/Music/Playlist.SetOutput(Output)` | instance | Routes audio to a channel |
| `SoundsGoodManager.ChangeOutputVolume(Output, float, bool save = true)` | static void | Sets channel volume; saves it unless `save: false` |
| `SoundsGoodManager.RestoreSavedOutputVolume(Output)` | static void | Back to the player's saved level, undoing a `save: false` change |
| `OutputVolumeSlider` | MonoBehaviour | Ready-made UI slider bound to a channel |

## Common issues

| Symptom | Cause | Fix |
|---|---|---|
| `Output.MyChannel` does not compile | The enum is a source file and Unity has not recompiled | Let the domain reload finish |
| Changing a channel's volume affects nothing | The audio was never routed to it | Add `SetOutput`, or set the emitter's Output field |
| `CreateOutput` returns false saying it exists | Already created | Check `GetOutputNames()` first |
| The volume resets every session | The value was set with `save: false`, or the mixer parameter was set directly | Use `ChangeOutputVolume` and leave `save` at its default |
| The slider does nothing | Its output is unset, or the audio is routed elsewhere | Set the field and check the routing |
| Volume changes feel wrong at the low end | Mixer volume is in decibels, not linear | Use `ChangeOutputVolume`, which takes 0–1 and converts for you |
| The player's volume setting changed on its own | A temporary duck or mute was saved over it | Pass `save: false` for changes the player did not make |

## Boundaries

- Channels are built under Master in the Sounds Good mixer. Arbitrary nested hierarchies and
  snapshots remain Unity's Audio Mixer's own concern.
- The API creates channels; renaming and deleting are done in `Tools > Sounds Good > Output Manager`.
- Channel volumes save to `PlayerPrefs` unless you pass `save: false`. For a custom settings
  backend, keep the value yourself and apply it with `save: false` so the two never disagree.
