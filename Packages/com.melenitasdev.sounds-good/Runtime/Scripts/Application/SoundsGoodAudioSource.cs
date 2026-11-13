/*
 * All rights to the Sounds Good plugin, © Created by Melenitas Dev, are reserved.
 * Distribution of the standalone asset is strictly prohibited.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using MelenitasDev.SoundsGood.Domain;
using UnityEngine;
using UnityEngine.Audio;
using MelenitasDev.SoundsGood.SystemUtilities;

namespace MelenitasDev.SoundsGood
{
    internal partial class SoundsGoodAudioSource // Fields
    {
        private AudioSource source;
        private Transform followTarget = null;
        private float volume = 1;
        private float volumeBeforePause = 1;
        // The configured level, captured before any fade starts moving `volume` around. LerpVolume
        // drives that field itself, so mid-fade it holds a point on a ramp rather than the level
        // the sound is meant to sit at: a fade-out leaves it at 0, and a fade-in has not reached
        // its target yet. Anything that needs to know where the volume belongs - the next track,
        // a pause that has to be resumed later - reads it from here instead.
        private float volumeBeforeFade = 1;
        private float basePitch = 1;
        private float fadeOutTime = 0;
        private float fadeInTime = 0;
        private bool loop = false;
        private bool stopping = false;
        private bool changingTrack = false;
        private bool isPlaylist;
        private Queue<AudioClip> playlist = new Queue<AudioClip>();
        private int tracksPlayedThisCycle;
        private float occlusionVolumeMultiplier = 1f;
        private AudioLowPassFilter lowPassFilter;

        // A global/output effect (pause, etc.) claimed this frame. Wins over zones and, unlike them,
        // also reaches 2D sounds.
        // True while any override (global or zone) is applied instead of the source's own effect.
        // The override actually applied, which trails the claims so it can fade out rather than be
        // dropped. It outlives the last claim by the length of that fade. See UpdateOverride.

        // Seconds left before a silenced reverb/echo has drained and can be disabled. 0 when there's
        // nothing to wait for. See DrainTailFilters.

        // Bounds for that fade out. The floor keeps an effect with no tail at all from snapping off
        // at the frame rate; the ceiling stops a long preset (a hangar rings for ten seconds) from
        // feeling like the reverb followed the listener out of the room.
        private const float MIN_EFFECT_RELEASE_TIME = 0.25f;
        private const float MAX_EFFECT_RELEASE_TIME = 2f;

        // The low pass filter and the volume are shared between the occlusion and the effects,
        // so each one keeps its own contribution and they get combined before being applied.
        private float effectVolumeMultiplier = 1f;
        // The effect's pitch shift is a multiplier on the source's own pitch (basePitch), so both
        // are combined in RefreshPitch instead of one overwriting the other.
        private float effectPitchMultiplier = 1f;


        private Coroutine lerpVolumeCor;
        private Coroutine fadeInOnChangeTrackCor;
        private Coroutine lerpPitchCor;
        private Coroutine tailReleaseCor;

        // True while the clip has finished but the source is kept reserved so an effect tail
        // (reverb/echo) can ring out before it returns to the pool.
        private bool tailing;

        // A tiny silent, looping clip fed to the source during the tail so its DSP keeps running
        // and the reverb/echo renders its decay. Shared by every pooled source.
        private static AudioClip silenceClip;
        private static AudioClip SilenceClip
        {
            get
            {
                if (silenceClip == null)
                    silenceClip = AudioClip.Create("SG_TailSilence", 4096, 1, AudioSettings.outputSampleRate, false);
                return silenceClip;
            }
        }

        private SourceState currentState = SourceState.Stopped;

        private enum SourceState
        {
            Playing,
            Paused,
            Pausing,
            FadingIn,
            ChangingTrack,
            Stopping,
            Stopped
        }
    }

    internal partial class SoundsGoodAudioSource // Fields (Callbacks)
    {
        private Action onPlay;
        private Action onComplete;
        private Action onLoopCycleComplete;
        private Action onNextTrackStart;
        private Action onPause;
        private Action onPauseComplete;
        private Action onResume;
    }

    internal partial class SoundsGoodAudioSource // Properties
    {
        internal bool Using { get; private set; }
        internal bool Playing => source.isPlaying;
        internal float Volume => source.volume;
        internal float Pitch => source.pitch;
        internal bool Paused { get; private set; }
        internal string Id { get; private set; }
        internal float PlayingTime { get; private set; }
        internal float CurrentLoopCycleTime => source.time;
        internal int CompletedLoopCycles { get; private set; }
        internal int ReproducedTracks { get; private set; }
        internal float CurrentClipDuration => source.clip.length;
        internal AudioClip CurrentClip => source.clip;
        internal AudioClip NextPlaylistClip => playlist.Peek();
    }

    internal partial class SoundsGoodAudioSource : MonoBehaviour
    {






        private void RefreshVolume ()
        {
            if (source == null) return;
            source.volume = volume;

        }



        /// <summary>
        /// Drives the source pitch from the sound's own pitch, the effect's pitch multiplier and —
        /// when following scaled time — the current <c>Time.timeScale</c>, so an effect that shifts
        /// pitch stacks on top of the configured pitch and the whole thing tracks game speed. It's
        /// recomputed every frame while scaled (see <see cref="Update"/>) since the time scale is live.
        /// </summary>
        private void RefreshPitch ()
        {
            if (source == null) return;
            source.pitch = basePitch * effectPitchMultiplier;
        }

        /// <summary> Per-source delta, scaled with game time or unscaled depending on the mode. </summary>
        private float DeltaTime => Time.unscaledDeltaTime;

        void Update ()
        {
            if (!Using) return;


            // While ringing out an effect tail there's no clip to manage, but the source must
            // still follow its target so the tail moves with it.
            if (tailing)
            {
                if (followTarget != null) transform.position = followTarget.position;
                return;
            }

            if (!loop)
            {
                if (!isPlaylist) HandleSoundStop();
                else HandlePlaylistStop();
            }

            if (!isPlaylist) HandleSoundPlaying();
            else HandlePlaylistPlaying();

            // Per-frame occlusion tracking: the whole thing is a paid feature.

            if (followTarget == null) return;

            transform.position = followTarget.position;
        }
    }

    internal partial class SoundsGoodAudioSource // Internal Methods
    {
        internal SoundsGoodAudioSource Init (AudioSource source)
        {
            this.source = source;
            return this;
        }

        internal SoundsGoodAudioSource MarkAsPlaylist ()
        {
            isPlaylist = true;
            return this;
        }

        internal SoundsGoodAudioSource SetVolume (float volume, float lerpTime = 0)
        {
            this.volume = volume;

            if (currentState == SourceState.Paused || currentState == SourceState.Stopping ||
                currentState == SourceState.ChangingTrack)
            {
                // The fade-out in flight rewrites `volume` on every frame, so the value just
                // assigned would be gone within the frame. Kept where the fade back in reads it
                // from, which is what the warning below tells the caller will happen.
                volumeBeforePause = volume;
                volumeBeforeFade = volume;

                Debug.LogWarning($"There's a volume fade out taking place at this moment, " +
                                 "so volume won't change right now, but on the next fade in it will go " +
                                 $"up until the new volume of {volume}");
                return this;
            }

            if (lerpTime <= 0)
            {
                RefreshVolume();
            }
            else
            {
                lerpVolumeCor = StartCoroutine(LerpVolume(volume, lerpTime));
            }

            return this;
        }

        internal SoundsGoodAudioSource SetHearDistance (float minHearDistance, float maxHearDistance)
        {
            source.minDistance = minHearDistance;
            source.maxDistance = maxHearDistance;
            return this;
        }

        internal SoundsGoodAudioSource SetVolumeRolloffCurve (AudioRolloffMode audioRolloffMode,
            AnimationCurve customVolumeCurve)
        {
            source.rolloffMode = audioRolloffMode;
            if (audioRolloffMode == AudioRolloffMode.Custom)
            {
                source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customVolumeCurve);
            }

            return this;
        }

        internal SoundsGoodAudioSource SetPitch (float pitch, float lerpTime = 0)
        {
            if (lerpTime <= 0)
            {
                basePitch = pitch;
                RefreshPitch();
            }
            else
            {
                if (lerpPitchCor != null)
                {
                    StopCoroutine(lerpPitchCor);
                    lerpPitchCor = null;
                }

                lerpPitchCor = StartCoroutine(LerpPitch(pitch, lerpTime));
            }

            return this;
        }

        internal SoundsGoodAudioSource SetDopplerLevel (float dopplerLevel)
        {
            source.dopplerLevel = dopplerLevel;
            return this;
        }

        /// <summary>
        /// Sets whether this source follows <c>Time.timeScale</c> (true) or ignores it (false). While
        /// scaled, its playback speed and every per-source timer track game time.
        /// </summary>

        internal SoundsGoodAudioSource SetClip (AudioClip audioClip)
        {
            source.clip = audioClip;
            return this;
        }

        internal SoundsGoodAudioSource SetPlaylist (Queue<AudioClip> playlist)
        {
            this.playlist = new Queue<AudioClip>(playlist);
            tracksPlayedThisCycle = 0;
            return this;
        }

        internal void AddToPlaylist (AudioClip addedClip)
        {
            playlist.Enqueue(addedClip);
        }

        internal SoundsGoodAudioSource SetId (string id)
        {
            Id = id;
            return this;
        }

        internal SoundsGoodAudioSource SetSpatialSound (bool activate)
        {
            source.spatialBlend = activate ? 1 : 0;
            return this;
        }








        internal AudioMixerGroup Output => source != null ? source.outputAudioMixerGroup : null;



        internal SoundsGoodAudioSource SetPosition (Vector3 position)
        {
            transform.position = position;
            return this;
        }

        internal SoundsGoodAudioSource SetFollowTarget (Transform followTarget)
        {
            this.followTarget = followTarget;
            return this;
        }

        internal SoundsGoodAudioSource SetFadeIn (float fadeInTime)
        {
            this.fadeInTime = fadeInTime;
            return this;
        }

        internal SoundsGoodAudioSource SetFadeOut (float fadeOutTime)
        {
            this.fadeOutTime = fadeOutTime;
            return this;
        }

        internal SoundsGoodAudioSource SetLoop (bool loop)
        {
            this.loop = loop;
            source.loop = loop;
            return this;
        }

        internal SoundsGoodAudioSource SetOutput (AudioMixerGroup output)
        {
            source.outputAudioMixerGroup = output;
            return this;
        }

        internal SoundsGoodAudioSource OnPlay (Action onPlay)
        {
            this.onPlay = onPlay;
            return this;
        }

        internal SoundsGoodAudioSource OnComplete (Action onComplete)
        {
            this.onComplete = onComplete;
            return this;
        }

        internal SoundsGoodAudioSource OnLoopCycleComplete (Action onLoopCycleComplete)
        {
            this.onLoopCycleComplete = onLoopCycleComplete;
            return this;
        }

        internal SoundsGoodAudioSource OnNextTrackStart (Action onNextTrackStart)
        {
            this.onNextTrackStart = onNextTrackStart;
            return this;
        }

        internal SoundsGoodAudioSource OnPause (Action onPause)
        {
            this.onPause = onPause;
            return this;
        }

        internal SoundsGoodAudioSource OnPauseComplete (Action onPauseComplete)
        {
            this.onPauseComplete = onPauseComplete;
            return this;
        }

        internal SoundsGoodAudioSource OnResume (Action onResume)
        {
            this.onResume = onResume;
            return this;
        }

        internal void Play (float fadeInTime = 0)
        {
            if (source.clip == null)
            {
                Debug.LogError(
                    "No audio clip found, make sure you have initialized it in a method (not in the declaration)");
                return;
            }

            Using = true;
            Paused = false;
            PlayingTime = 0;
            CompletedLoopCycles = 0;

            onPlay?.Invoke();

            // Occlusion at its correct level from the very first sample (before any audio is heard).


            source.Play();
            ChangeState(SourceState.Playing);
            enabled = true;

            if (fadeInTime > 0)
            {
                ChangeState(SourceState.FadingIn);
                // Ramp from silence: LerpVolume lerps from the base 'volume' field, so it must
                // start at 0 (setting only source.volume would be overwritten to full on frame 1).
                volumeBeforeFade = volume;
                volume = 0;
                source.volume = 0;
                lerpVolumeCor = StartCoroutine(LerpVolume(volumeBeforeFade, fadeInTime,
                    (() => ChangeState(SourceState.Playing))));
            }
        }

        internal void PlayPlaylist (float fadeInTime)
        {
            bool validClips = true;
            for (int i = 0; i < playlist.Count; i++)
            {
                if (playlist.ToArray()[i] != null) continue;
                validClips = false;
            }

            if (!validClips)
            {
                Debug.LogError("There are invalid audio clips in playlist, make sure you have initialized it.");
                return;
            }

            Using = true;
            Paused = false;
            PlayingTime = 0;
            ReproducedTracks = 0;
            CompletedLoopCycles = 0;
            tracksPlayedThisCycle = 0;
            if (loop) source.loop = false;
            changingTrack = false;

            // Occlusion at its correct level from the very first sample (before any audio is heard).


            // PlayNextSong fades the first track in on its own. Starting a second fade here raced
            // it: this one read `volume` after the first had already zeroed it, so it lerped
            // towards silence and won, and a playlist with a fade-in never made a sound.
            volumeBeforeFade = volume;
            PlayNextSong();
            enabled = true;
        }

        internal void Pause (float fadeOutTime = 0)
        {
            if (!Using) return;
            if (Paused) return;

            Paused = true;

            // Capture the intended volume before any fade-out drives it to 0, so Resume can restore
            // it (a pause fade mutates the volume field). When a fade is already in flight `volume`
            // is a point on its ramp rather than a level anyone chose - pausing halfway through a
            // fade-in used to make the sound come back at whatever fraction it had reached - so the
            // value to return to is the one captured before that fade started.
            bool fading = currentState == SourceState.FadingIn || currentState == SourceState.ChangingTrack;
            volumeBeforePause = fading ? volumeBeforeFade : volume;

            onPause?.Invoke();

            void CompletePause ()
            {
                onPauseComplete?.Invoke();
                source.Pause();
                ChangeState(SourceState.Paused);
            }

            if (changingTrack)
            {
                CompletePause();
                return;
            }

            if (fadeOutTime > 0)
            {
                StopEveryVolumeFade();
                ChangeState(SourceState.Pausing);
                lerpVolumeCor = StartCoroutine(LerpVolume(0, fadeOutTime, CompletePause, true));
                return;
            }

            CompletePause();
        }

        internal void Resume (float fadeInTime = 0)
        {
            if (!Paused) return;

            onResume?.Invoke();

            Paused = false;
            source.UnPause();
            ChangeState(SourceState.Playing);

            if (changingTrack) return;

            // Kill any in-flight fade (e.g. an unfinished pause fade-out) before resuming.
            StopLerpCoroutine();

            if (fadeInTime > 0)
            {
                ChangeState(SourceState.FadingIn);
                volumeBeforeFade = volumeBeforePause;
                volume = 0;
                source.volume = 0;
                lerpVolumeCor = StartCoroutine(LerpVolume(volumeBeforeFade, fadeInTime,
                    (() => ChangeState(SourceState.Playing))));
            }
            else
            {
                // Restore the volume a pause fade-out may have driven to 0.
                volume = volumeBeforePause;
                RefreshVolume();
            }
        }

        internal void Stop (float fadeOutTime = 0, Action onStop = null)
        {
            if (fadeOutTime > 0)
            {
                stopping = true;
                // Whatever fade is in flight has to go first: left running it keeps writing `volume`
                // every frame, and which of the two the listener hears comes down to the order Unity
                // happens to resume them in. The fade-out starts from the level the fade-in reached,
                // which is what `volume` already holds.
                StopEveryVolumeFade();
                ChangeState(SourceState.Stopping);
                // ignorePause: a stop asked for while paused still has to finish and free the source.
                // Without it the fade waits on `Paused` forever and the source never returns to the pool.
                lerpVolumeCor = StartCoroutine(LerpVolume(0, fadeOutTime, () => Stop(0, onStop), true));
                return;
            }

            onComplete?.Invoke();
            onStop?.Invoke();

            // Manual stop: release right away, cutting any effect tail.
            ReleaseSource();
        }

        /// <summary>
        /// Called when the clip ends on its own. If the sound is being processed by an effect with
        /// a tail (reverb/echo), the source is kept reserved with its filters enabled while the tail
        /// rings out, and only then returned to the pool. This stops a reused source from cutting
        /// the previous sound's tail (e.g. footsteps in a reverberant cave).
        /// </summary>
        private void Complete ()
        {
            onComplete?.Invoke();
            onComplete = null; // already fired; a manual Stop during the tail must not fire it again

            // No effects means no tail to ring out: the source goes straight back to the pool.
            ReleaseSource();
            return;

            // Unity stops processing a source's reverb/echo the moment it isn't playing, freezing
            // the tail. Keep the voice alive playing silence so the filters render their decay,
            // while the source stays reserved so the pool can't reuse it and cut the tail. By the
            // time it's freed the buffer is empty, so a reused source won't bleed the old tail.
        }



        private void ReleaseSource ()
        {
            // A fade still running here would keep writing volume on a source that is back in the
            // pool, and stomp whatever borrows it next.
            StopEveryVolumeFade();

            StopTailCoroutine();


            source.Stop();
            ChangeState(SourceState.Stopped);
            source.clip = null;
            source.loop = false;

            playlist.Clear();
            isPlaylist = false;

            followTarget = null;

            onComplete = null;
            onLoopCycleComplete = null;
            onPause = null;
            onPauseComplete = null;
            onResume = null;

            Id = null;

            // Reset occlusion
            occlusionVolumeMultiplier = 1f;
            if (lowPassFilter != null)
            {
                lowPassFilter.enabled = false;
                lowPassFilter.cutoffFrequency = AssetLocator.SoundsGoodSettings.MaxCutoff;
                lowPassFilter.lowpassResonanceQ = 1f;
            }

            // Reset effects
            effectVolumeMultiplier = 1f;
            effectPitchMultiplier = 1f;
            basePitch = 1f;
            if (source != null) source.pitch = 1f;
            AudioEffectApplier.Apply(gameObject, null);
            // The tail hold before this already gave the reverb/echo time to ring out, so they can
            // be taken out of the chain here — flushed, so the next sound to reuse this pooled
            // source doesn't inherit whatever was left in their delay lines.
            AudioEffectApplier.DisableTailFilters(gameObject);

            changingTrack = false;
            stopping = false;
            tailing = false;
            Using = false;
            enabled = false;
        }

        private void StopTailCoroutine ()
        {
            if (tailReleaseCor == null) return;
            StopCoroutine(tailReleaseCor);
            tailReleaseCor = null;
            tailing = false;
        }
    }

    internal partial class SoundsGoodAudioSource // Private Methods
    {
        /// <summary>
        /// Seconds of real time left before the current clip ends, which is not the same as the
        /// clip time left: pitch is the playback speed, so at pitch 2 a 4-second clip is over in 2
        /// seconds. Recomputed from the live playback position every frame rather than predicted
        /// when the clip starts, so changing the pitch mid-track corrects itself.
        /// </summary>
        private float RemainingRealTime
        {
            get
            {
                if (source.clip == null) return 0f;

                float speed = Mathf.Abs(source.pitch);
                // Frozen (pitch 0, or scaled time while the game is paused): it never gets there.
                if (speed < 0.01f) return float.PositiveInfinity;

                // Played backwards, the playhead runs towards 0 instead of towards the end.
                float remainingClipTime = source.pitch < 0f
                    ? source.time
                    : source.clip.length - source.time;

                return remainingClipTime / speed;
            }
        }

        private void HandleSoundPlaying ()
        {
            PlayingTime += DeltaTime;

            if (!loop) return;

            // Fire once per completed pass: wait until playback has crossed the end of the cycle
            // we're currently in.
            if (PlayingTime < CurrentClipDuration * (CompletedLoopCycles + 1)) return;

            CompletedLoopCycles++;

            onLoopCycleComplete?.Invoke();
        }

        private void HandlePlaylistPlaying ()
        {
            // Already stopping: a track change now would restart the audio under the fade-out.
            if (stopping) return;

            PlayingTime += DeltaTime;

            // A clip that already stopped has to be caught too: at fadeOutTime 0 the remaining
            // time never quite reaches zero before Unity ends the clip.
            bool clipEnded = !Paused && !source.isPlaying;
            if (!changingTrack && (clipEnded || RemainingRealTime <= Mathf.Max(fadeOutTime, DeltaTime)))
            {
                changingTrack = true;
                // Read before any fade-out starts, because that fade is what destroys it.
                volumeBeforeFade = volume;
                ChangeState(SourceState.ChangingTrack);
                if (fadeOutTime > 0)
                    lerpVolumeCor = StartCoroutine(LerpVolume(0, fadeOutTime, () => PlayNextSong()));
                else PlayNextSong();
            }
        }

        private void HandleSoundStop ()
        {
            if (stopping) return;

            if (fadeOutTime > 0)
            {
                if (RemainingRealTime <= fadeOutTime + 0.05f)
                {
                    Stop(fadeOutTime);
                }
            }

            if (!Playing && !Paused)
            {
                Complete();
            }
        }

        private void HandlePlaylistStop ()
        {
            if (playlist.Count > 0) return;
            if (stopping) return;

            if (fadeOutTime > 0)
            {
                if (RemainingRealTime <= fadeOutTime + 0.05f)
                {
                    Stop(fadeOutTime);
                }
            }

            if (!Playing && !Paused)
            {
                Complete();
            }
        }

        private void PlayNextSong (bool firstTrack = false)
        {
            if (playlist.Count == 0) return;

            source.clip = playlist.Dequeue();
            if (loop) playlist.Enqueue(source.clip);
            if (!firstTrack)
            {
                ReproducedTracks++;
                onNextTrackStart?.Invoke();

                // A playlist's loop cycle completes when it wraps back to its first track. While
                // looping the queue always holds every track (each one is re-enqueued as it plays),
                // so its count is the length of a full cycle.
                if (loop && ++tracksPlayedThisCycle >= playlist.Count)
                {
                    tracksPlayedThisCycle = 0;
                    CompletedLoopCycles++;
                    onLoopCycleComplete?.Invoke();
                }
            }

            changingTrack = false;

            if (Paused) return;

            source.Play();
            if (fadeInTime > 0)
            {
                ChangeState(SourceState.FadingIn);
                volume = 0;
                source.volume = 0;
                fadeInOnChangeTrackCor = StartCoroutine(LerpVolume(volumeBeforeFade, fadeInTime,
                    () => ChangeState(SourceState.Playing)));
            }
            else
            {
                ChangeState(SourceState.Playing);
                // Restored explicitly: with a fade-out and no fade-in, `volume` is still the 0 the
                // fade left behind, and refreshing from it would start the new track silent.
                volume = volumeBeforeFade;
                RefreshVolume();
            }
        }

        private void StopLerpCoroutine ()
        {
            if (lerpVolumeCor == null) return;
            StopCoroutine(lerpVolumeCor);
            lerpVolumeCor = null;
        }

        /// <summary>
        /// Kills the fade-in a track change started. Only PlayNextSong sets this one, so outside a
        /// playlist it is null while the state still says FadingIn - a Play() fade-in lives in
        /// lerpVolumeCor instead. Both have to be stoppable without knowing which is in flight.
        /// </summary>
        private void StopFadeInOnChangeTrackCoroutine ()
        {
            if (fadeInOnChangeTrackCor == null) return;
            StopCoroutine(fadeInOnChangeTrackCor);
            fadeInOnChangeTrackCor = null;
        }

        /// <summary>Stops whichever fade is in flight, so a new one is the only thing writing volume.</summary>
        private void StopEveryVolumeFade ()
        {
            StopFadeInOnChangeTrackCoroutine();
            StopLerpCoroutine();
        }

        private void ChangeState (SourceState newState) { currentState = newState; }
    }

    internal partial class SoundsGoodAudioSource // Private Methods (Coroutines)
    {
        private IEnumerator LerpVolume (float newVolume, float lerpTime, Action onFinishLerp = null,
            bool ignorePause = false)
        {
            float initialBaseVolume = volume;
            float targetBaseVolume = newVolume;

            for (float t = 0.0f; t < lerpTime; t += DeltaTime)
            {
                if (!ignorePause)
                {
                    while (Paused)
                    {
                        yield return null;
                    }
                }

                float lerpedBase = Mathf.Lerp(initialBaseVolume, targetBaseVolume, t / lerpTime);
                volume = lerpedBase;
                source.volume = lerpedBase * occlusionVolumeMultiplier * effectVolumeMultiplier;
                yield return null;
            }

            volume = targetBaseVolume;
            RefreshVolume();

            onFinishLerp?.Invoke();

            lerpVolumeCor = null;
        }

        private IEnumerator LerpPitch (float newPitch, float lerpTime)
        {
            float initialPitch = basePitch;

            for (float t = 0.0f; t < lerpTime; t += DeltaTime)
            {
                while (Paused)
                {
                    yield return null;
                }

                basePitch = Mathf.Lerp(initialPitch, newPitch, t / lerpTime);
                RefreshPitch();
                yield return null;
            }

            basePitch = newPitch;
            RefreshPitch();
            lerpPitchCor = null;
        }
    }
}