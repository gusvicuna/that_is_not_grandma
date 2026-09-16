# Audio tooling research — build it or use a tool?

> Written Thu Aug 27, before implementing `Docs/plans/05-audio-music.md`. Question asked: **what already exists in Unity 6 / the editor / the ecosystem that would let us skip writing the audio scripts in plan 05?**

## The finding that decides almost everything

**On the Web platform, the AudioMixer only lets you change volume.** Unity's manual is explicit: *"Volume is the only property you can change on Web. Other properties and sound effects aren't supported."* Unity doesn't run FMOD in Web builds — FMOD relies on threads, which the platform doesn't have — so it uses an implementation over the browser's Web Audio API instead.

That removes the classic no-code route for adaptive audio in Unity (snapshots + `TransitionToSnapshot` + DSP effects), which is exactly what would have replaced `MusicLayerMixer`, `Crossfader` and most of plan 05. What Web leaves us is *moving volumes* — and moving volumes according to rules is precisely what those domain types do.

## Comparison

| Option | What it would replace in plan 05 | Pros | Cons | Verdict *(outcomes recorded Aug 28)* |
|---|---|---|---|---|
| **AudioMixer + exposed parameters** | Nothing — already in the plan | The only mixer supported on Web; sliders with no mixing code | Volume only on Web | **Already adopted** |
| **AudioMixer snapshots** | `MusicLayerMixer`, `Crossfader`, all fading | Declarative fades and ducking, zero code | Effects/snapshots unsupported on Web; behaviour there undocumented | ❌ **Ruled out by WebGL** |
| **Audio Random Container** (built-in, Unity 6) | `NoRepeatPicker`, `AudioCueSO`'s randomization, part of `SfxPlayer` | *Avoid Repeating Last N*, volume and pitch randomization, Sequential/Shuffle/Random playback modes, and an **Automatic trigger with Time + Randomization** — literally the clock/pipes/distant-TV one-shot ambience of GDD §8, for free | Incompatible with `PlayOneShot` (needs `AudioSource.resource` + `Play`); **WebGL support not documented anywhere**; opaque internals (no per-clip access, no `UnloadAudioData`); one hand-made asset per cue | ❌ **Spike dropped Aug 28, not run** — post-jam candidate |
| **Timeline Audio Track / Animator** | The fades and crossfades | Visual curves, no code | Authored and linear, not reactive to a changing state; untestable in EditMode; another director/animator to maintain | ❌ No |
| **FMOD Studio** | Nearly everything: parameter-driven layers, mixing, transitions | *The* right tool for adaptive music; free at our scale; sound design happens outside Unity | WebGL supported but with real friction (bank preloading, size, reported problems); a new toolchain 3 days from close; needs a `CREDITS.md` row | 🔜 **Post-jam** |
| **Wwise** | Same | — | **No WebGL support** | ❌ Dead on arrival |
| **Sonity** (Asset Store, paid) | The whole presentation stack | ScriptableObject workflow with no string lookups; works in WebGL except DSP; ~1h to learn | Costs money; third-party code to debug under deadline pressure; breaks the Domain/Events/Presentation split in `CLAUDE.md`; mandatory credit | ❌ The only credible purchase, but not 3 days out |
| **JSAM** (MIT, free, "built for jams") | `SfxPlayer` + cues | Free, installs immediately | Static-singleton based — head-on collision with "no singletons, no static event buses" in `CLAUDE.md`; hard to test | ❌ No |
| **Unity Atoms** and similar | Our SO event channels | Generic channels already written | We already have 7 channels written, green and in the repo's convention | ❌ No |
| **`UnityEngine.Pool.ObjectPool`** (built-in) | The hand-rolled pool in `SfxPlayer` | Less code of ours, standard API | None relevant | ✅ **Adopt** |
| **AudioMixer in *Edit in Play Mode*** | Thursday's "mix pass" | Mix while listening, values persist | Doesn't replace code | ✅ **Into the checklist** |

## Recommendation (accepted — folded into plan 05)

Keep plan 05 as the spine, with three changes. The conclusion isn't "hand-write everything for fun": it's that the platform we chose rules out exactly the tools that would save code, and what survives is ~200 lines of pure C# that happen to be testable.

1. ~~**Spike the Audio Random Container, timeboxed to 20 minutes — in a WebGL build, not the editor.**~~ **Dropped Aug 28 without running it** (Gus's call, two days from close). The undocumented WebGL support was the deciding factor: a 20-minute spike that comes back ambiguous costs more than the ~30 lines it would have saved. `NoRepeatPicker` stays, written and tested; `AudioCueSO` keeps its own clip list and randomization ranges. **Revisit post-jam** — the *Automatic* trigger mode is still the cheapest way to get the clock/pipes/distant-TV one-shot ambience.
2. **`ObjectPool<AudioSource>` in `SfxPlayer`** instead of the hand-rolled round-robin. Checklist note; the domain doesn't move.
3. **`MusicLayerMixer`, `TensionDirector` and `Crossfader` stay.** No built-in equivalent works on Web, and they carry ~80% of the tests.

## Platform constraints the docs confirm

These are now in plan 05's WebGL section:

- **No lowpass for "hiding under the bed".** Effects don't exist on Web. If the art direction wants a muffled sound while hiding, it has to be a separately recorded muffled clip, not a filter.
- **Audible loop seams:** AAC encoding alters the first 1024 samples on Web, so GDD §8's "no audible loop seams ☐" is verified *in the build*, never in the editor.
- **Positive pitch only** on Web — the plan's 0.5–2 range is fine, a negative pitch is not.
- **No `PlayScheduled`:** a known WebGL issue makes the scheduled clip start early, leaving two copies of the track playing over each other. This is why the music layers start together once and never stop.
- **No streaming:** `AudioClip.Create` must use `Stream = false`; long tracks are *Compressed In Memory*.

## Sources

- [Audio in Web — Unity Manual](https://docs.unity3d.com/Manual/webgl-audio.html)
- [Audio Random Container fundamentals — Unity Manual](https://docs.unity3d.com/6000.3/Documentation/Manual/AudioRandomContainer-fundamentals.html)
- [Audio Random Container reference — Unity Manual](https://docs.unity3d.com/6000.6/Documentation/Manual/AudioRandomContainer-UI.html)
- [AudioResource — Unity Scripting API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Audio.AudioResource.html)
- [AudioResource, AudioClip, AudioRandomContainer Interactions — gametorrahod](https://gametorrahod.com/audio-random-container/)
- [AudioSource.PlayScheduled causes problems in WebGL build — Unity Discussions](https://discussions.unity.com/t/audiosource-playscheduled-causes-problems-in-webgl-build/917494)
- [Sonity — Audio Middleware, Unity Asset Store](https://assetstore.unity.com/packages/tools/audio/sonity-audio-middleware-229857)
- [Simple Unity Audio Manager (JSAM) — GitHub](https://github.com/jackyyang09/Simple-Unity-Audio-Manager)
- [Wwise vs FMOD vs MetaSounds — StraySpark](https://www.strayspark.studio/blog/wwise-fmod-metasounds-audio-middleware-comparison)
