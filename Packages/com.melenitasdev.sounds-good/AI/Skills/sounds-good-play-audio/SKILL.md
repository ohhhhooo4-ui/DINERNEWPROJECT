---
name: sounds-good-play-audio
description: "Use this skill whenever the user wants to play, stop, fade or control audio from code in a project with Sounds Good Lite — e.g. 'play a sound when the player jumps', 'add background music', 'fade the music out when the level ends', 'stop all sounds', 'play footsteps', 'make the music loop', 'play a random hit sound'. Covers the three audio types (Sound, Music, Playlist), the Set/Change/On method convention, 3D positional audio, fades, looping, callbacks, and global control through SoundsGoodManager. Do NOT use for registering new clips (see sounds-good-register-audio) or for Inspector components instead of code (see sounds-good-inspector-components). When in doubt whether playing audio in this project involves Sounds Good, use this skill — Prerequisites shows how to confirm the asset is installed."
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

# Play Audio from Code with Sounds Good

Sounds Good plays registered audio in one line. This skill covers the three audio types, how their
methods are named, and how to control everything that is playing without keeping references to it.

## When to use this skill

- "play a sound when the player jumps", "play footsteps"
- "add background music", "make the music loop", "fade the music out"
- "stop all sounds", "pause the audio when the game pauses"
- Any runtime audio behaviour written in C#.

Not for:

- Making a new clip playable — see `sounds-good-register-audio`.
- Doing it without code — see `sounds-good-inspector-components`.
- Reverb, filters and occlusion, which are part of the full version.

## Prerequisites

- Sounds Good installed, and the tag already registered. Check with
  `SoundsGoodAuthoring.GetSoundTags()` from an editor script, or in
  `Tools > Sounds Good > Audio Collection`.
- An `AudioListener` in the scene, normally on the main camera. Without one nothing is audible and
  nothing is logged.

**Install check.** Confirm the type `MelenitasDev.SoundsGood.Sound` resolves. If it does not,
Sounds Good is not installed; use Unity's `AudioSource` instead and say so.

## Quick start

```csharp
using MelenitasDev.SoundsGood;
using UnityEngine;

public class Player : MonoBehaviour
{
    void Jump ()
    {
        new Sound(SFX.Jump).Play();
    }
}
```

Expected observable result: the sound is audible on jump, and no AudioSource has been added to the
scene by hand — Sounds Good pools them internally.

## Workflows

### Workflow: Play a sound effect

**Goal.** Play a one-shot, configured as needed.

**Steps.**

1. Construct with the tag: `new Sound(SFX.Explosion)`.
2. Chain `Set…` methods to configure. Each returns the object, so they read as one sentence.
3. Call `Play()`, optionally with a fade-in time in seconds.

```csharp
new Sound(SFX.Explosion)
    .SetVolume(0.7f)
    .SetRandomPitch()            // small variation each play, so repeats do not sound identical
    .SetSpatialSound()           // 3D: heard from where it is
    .SetFollowTarget(transform)  // moves with this object
    .SetOutput(Output.SFX)
    .Play();
```

Useful options: `SetPosition(Vector3)` for a fixed point, `SetLoop()`, `SetPlayProbability(0.3f)`
to play only sometimes, `SetRandomVolume()`, `SetHearDistance(min, max)`.

**Expected result.** The sound plays with the configuration applied. Positioned sounds get quieter
with distance; `SetSpatialSound()` is what makes position matter at all.

### Workflow: Play music and playlists

**Goal.** Background music, looping or as a sequence.

**Steps.**

1. A single track:

   ```csharp
   Music music = new Music(Track.MainTheme).SetLoop().SetVolume(0.6f);
   music.Play(fadeInTime: 2f);
   ```

2. Several tracks in order, fading between them:

   ```csharp
   new Playlist(Track.Song1, Track.Song2, Track.Song3)
       .SetLoop()
       .SetFadeIn(2f)
       .SetFadeOut(2f)
       .Play();
   ```

3. Keep the reference to control it later, or give it an id (see the global control workflow).

**Expected result.** Music plays and loops. In a playlist, each track fades into the next without a
gap, and `SetLoop()` makes the queue start again after the last one.

### Workflow: Control audio globally, without keeping references

**Goal.** Pause, stop or resume from somewhere that does not hold the object.

**Steps.**

1. Either keep the reference, or tag the audio with an id when you create it:

   ```csharp
   new Music(Track.Menu).SetId("menuMusic").SetLoop().Play();
   ```

2. Control it later from anywhere:

   ```csharp
   SoundsGoodManager.Pause("menuMusic", fadeOutTime: 0.5f);
   SoundsGoodManager.Resume("menuMusic", fadeInTime: 0.5f);
   SoundsGoodManager.Stop("menuMusic", fadeOutTime: 1f);
   ```

3. Or act on everything at once:

   ```csharp
   SoundsGoodManager.PauseAll(0.3f);
   SoundsGoodManager.ResumeAll(0.3f);
   SoundsGoodManager.StopAll(0.5f);
   ```

**Expected result.** Audio responds from anywhere in the codebase with no references wired through
the scene.

### Workflow: React to playback events

**Goal.** Run code when audio starts, finishes or loops.

**Steps.**

Chain `On…` methods before playing. They return the object, like `Set…`.

```csharp
new Music(Track.Cutscene)
    .OnPlay(() => Debug.Log("started"))
    .OnComplete(() => LoadNextScene())
    .Play();
```

`Sound` and `Music` also offer `OnLoopCycleComplete`, `OnPause`, `OnPauseComplete` and `OnResume`.

**Expected result.** The callbacks fire at the right moments without polling.

## Verification

- The expected sound is audible in Play Mode.
- The Console has no `NullReferenceException` and no "tag not found" style error.
- The scene Hierarchy gains pooled audio source objects while something plays, and they are reused
  rather than accumulating without bound.
- For positioned audio, moving the listener changes the volume.
- For a playlist, the second and third tracks are audible, not only the first.

## API quick reference

| Entry point | Type | What it does |
|---|---|---|
| `Sound(SFX)` / `Sound(string)` | class | One-shot or looping effect |
| `Music(Track)` / `Music(string)` | class | A single track |
| `Playlist(params Track[])` | class | Tracks one after another, with fades |
| `SoundsGoodManager.PauseAll/StopAll/ResumeAll(fade)` | static void | Everything at once |
| `SoundsGoodManager.Pause/Stop/Resume(id, fade)` | static void | One audio, by id |
| `SoundsGoodManager.ChangeOutputVolume(Output, float)` | static void | Channel volume, saved to PlayerPrefs |

The naming convention, which lets you guess the rest: `Set…` configures **before** playing and
returns the object; `Change…` mutates **while** playing and takes an optional lerp time;
`On…` subscribes to an event; `Play` / `Pause` / `Resume` / `Stop` act immediately.

Everything is in the `MelenitasDev.SoundsGood` namespace.

## Common issues

| Symptom | Cause | Fix |
|---|---|---|
| Nothing is audible, Console clean | No `AudioListener` in the scene | Add one to the main camera |
| `SFX.MyTag` does not compile | The tag is not registered, or Unity has not recompiled since | See `sounds-good-register-audio` |
| A 3D sound is heard at full volume everywhere | `SetSpatialSound()` was not called | Add it; without it the sound is 2D by design |
| The sound plays at the wrong place | `SetPosition` set once on a moving object | Use `SetFollowTarget(transform)` instead |
| Music restarts when it should continue | A new `Music` object is created each time | Keep the reference, or use an id and check before playing again |
| Audio keeps playing at normal speed when the game is paused | Lite always ignores `Time.timeScale`, like a plain AudioSource | That is by design here. Audio that follows the time scale is part of the full version |
| Volume changes have no effect during a fade | A fade owns the volume while it runs | Wait for it, or use `Change…` with a lerp time instead of `Set…` |

## Boundaries

- Sounds Good plays registered audio. To play an arbitrary `AudioClip` reference with no tag, use
  Unity's `AudioSource` directly.
- `SetEffect`, `SetOcclusion`, `SetTimeMode` and `DynamicMusic` are part of the full version and do
not exist here. If the user asks for them, say so rather than writing a substitute.
- Pooled sources are managed internally; do not add, move or destroy them by hand.
