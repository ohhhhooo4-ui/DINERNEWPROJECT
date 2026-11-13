---
name: sounds-good-inspector-components
description: "Use this skill whenever the user wants audio driven by scene objects rather than by code in a project with Sounds Good Lite — e.g. 'make this torch crackle', 'add a sound to this object without scripting', 'put background music on this object', 'play music without writing code', 'add a volume slider to my menu'. Covers the Sound, Music and Playlist emitter components: which one fits the request, how to add them from the GameObject menu, and which fields to set. Do NOT use for playing audio in C# (see sounds-good-play-audio) or registering clips (see sounds-good-register-audio). Triggers, zones, random ambience and collision sounds are part of the full version. When in doubt whether a scene-driven audio request involves Sounds Good, use this skill — Prerequisites shows how to confirm the asset is installed."
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

# Drive Audio from the Inspector with Sounds Good

Sounds Good ships components that do from the scene what the API does from code. This skill covers
choosing the right one for a request and adding it correctly, so a room, a torch or a collision
makes sound without a line of script.

## When to use this skill

- "make this torch crackle", "add bird sounds to the forest"
- The user asks for audio behaviour attached to an object, or says they do not want to write code.

Not for:

- Audio triggered by game logic in C# — see `sounds-good-play-audio`.
- A clip that is not registered yet — see `sounds-good-register-audio` first.

## Prerequisites

- Sounds Good installed, and the tags the component will reference already registered.
- An `AudioListener` in the scene.

**Install check.** Confirm the menu `GameObject > Sounds Good > Emitters > Sound Emitter` exists, or
that the type `MelenitasDev.SoundsGood.SoundEmitter` resolves. If not, Sounds Good is not installed.

## Quick start

Add a sound emitter that plays on start:

1. Menu: `GameObject > Sounds Good > Emitters > Sound Emitter`. This instantiates a configured
   prefab, which is why it is preferred over adding the component by hand.
2. In the Inspector, set **Sfx** to the registered tag.
3. Tick **Play On Start**.

Expected observable result: the Hierarchy contains a `Sound Emitter` object, and entering Play Mode
plays the sound.

## Workflows

### Workflow: Choose the right component

**Goal.** Match the request to the component that already does it.

| The user wants | Component | Menu |
|---|---|---|
| An object that plays a sound / music / a queue of tracks | Sound, Music or Playlist Emitter | `GameObject > Sounds Good > Emitters > …` |
| A volume slider in an options menu | Output Volume Slider | `GameObject > Sounds Good > UI > Output Volume Slider` |

**Expected result.** One component chosen, rather than a custom script written to do the same job.

### Workflow: Add and configure an emitter

**Goal.** An object that plays a registered sound, configured from the Inspector.

**Steps.**

1. Create it from the `GameObject > Sounds Good > Emitters` menu so the prefab's defaults come with it.
2. Set the fields that matter. On `SoundEmitter` the serialized names are: `sfx`, `playOnStart`,
   `volume`, `loop`, `spatialSound`, `randomPitch`, `pitchRange`, `playProbability`,
   `hearDistanceMin`, `hearDistanceMax`, `output`.
3. From a script, set them through `SerializedObject` so the change is recorded and saved:

   ```csharp
   using UnityEditor;
   using UnityEngine;
   using MelenitasDev.SoundsGood;

   var emitter = Object.FindAnyObjectByType<SoundEmitter>();
   var so = new SerializedObject(emitter);
   so.FindProperty("playOnStart").boolValue = true;
   so.FindProperty("loop").boolValue = true;
   so.ApplyModifiedProperties();
   ```

4. Drive it at runtime through the component, which exposes `Play()`, `Stop()`, `Pause()`,
   `Resume()` and `IsPlaying`, plus `Sound` for the underlying object.

**Expected result.** The emitter plays as configured, and its Inspector shows the values you set.

## Verification

- The component is present in the Hierarchy and its Inspector shows the intended values.
- Entering Play Mode produces the expected sound at the expected moment.
- The Console shows no missing-reference or missing-script errors.

## API quick reference

| Entry point | Type | What it does |
|---|---|---|
| `SoundEmitter` / `MusicEmitter` / `PlaylistEmitter` | MonoBehaviour | The API in the Inspector; `Play/Stop/Pause/Resume`, `IsPlaying` |
| `AudioEmitter` | abstract MonoBehaviour | Base of all three, for code that treats them alike |

All in the `MelenitasDev.SoundsGood` namespace.

## Common issues

| Symptom | Cause | Fix |
|---|---|---|
| The emitter does nothing at start | **Play On Start** is off | Tick it, or call `Play()` from a trigger or script |
| The sound is heard everywhere at once | **Spatial Sound** is off on the emitter | Tick it; that is what makes position matter |
| Adding the component by hand leaves it unconfigured | `AddComponent` skips the prefab's defaults | Create it from the `GameObject > Sounds Good` menu instead |

## Boundaries

- The emitters cover common patterns. Behaviour beyond them is written in C# against the same API
— see `sounds-good-play-audio`.
