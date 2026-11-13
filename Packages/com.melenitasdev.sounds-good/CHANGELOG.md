# Changelog — Sounds Good Lite

All notable changes to the **Lite** edition are documented in this file.

This project follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) format and uses [Semantic Versioning](https://semver.org/).

> ℹ️ Lite has its own version numbers and its own release schedule, so they do not match the full version's. Each entry states which release of Sounds Good it was built from.

---

## [1.1.0] – 2026-10-03

Built from Sounds Good 2.3.0.

### Fixed
- **`ChangeOutputVolume` always wrote to `PlayerPrefs`**, so there was no way to duck or mute a channel without making it the player's saved setting. It takes `save` now, and `RestoreSavedOutputVolume` puts the player's level back.
- Pausing with a fade out while a fade in was still running threw an exception, left the audio playing and blocked every later fade on that sound, so a **Stop with a fade out did nothing**.
- Stopping with a fade out during a fade in left both fades running at once, fighting over the volume.
- A fade could outlive its sound and keep driving the volume of a pooled source already handed to the next one.
- A playlist told to stop with a fade out could still change track in the middle of the fade.

### Changed
- Lite now declares **Unity 2022.3** as its minimum, matching the full version. This is not a new restriction, only one that was never written down.

### Added
- **AI Skills**, at `AI/Skills/`. Ask your assistant to install them (the README explains how, for any tool) and it can register audio, play it, add emitters and set up audio channels for you, instead of writing its own audio code. The skills only describe what this edition can actually do; upgrading to the full version replaces them with the complete set, which adds effects, occlusion, zones and dynamic music.
- **`SoundsGoodAuthoring`**, a public editor API to register sounds and music, create output channels and regenerate the tag enums from script. This is what makes the skills above work, and it is just as usable from your own editor tooling.
- **`SoundEmitter.PlayScaled(factor)`**, to play at a fraction of the volume set in the Inspector, for any loudness you vary per play.

---

## [1.0.0] – 2026-08-29

First release of **Sounds Good Lite**. Built from Sounds Good 2.2.2.

### Added
- The complete audio API for **Sound**, **Music** and **Playlist**: volume, pitch, fades, looping, 3D sound, follow targets, callbacks and ids.
- **Sound Emitter**, **Music Emitter** and **Playlist Emitter** components, to set up and drive audio entirely from the Inspector.
- **Audio Creator** to register clips under tags, with compression presets and random clip selection.
- **Audio Collection** to browse and manage everything you have registered.
- **Output Manager** to create audio channels, with the mixer group and its volume set up for you, plus a ready-made **Output Volume Slider**.
- **Settings** for the data path, DSP buffer size and global options.
- A **2D demo scene** showing the API in use.

---
