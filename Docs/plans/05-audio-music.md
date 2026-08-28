# 05 — Audio & music system
**Goal:** A bus-based audio stack that survives WebGL: silence until the first click, per-room ambience crossfades, music as stems that fade in and out with tension, one router that turns existing gameplay channels into SFX, and volume sliders that persist. **Priority:** Must-Have (GDD §5 "Audio system", §8 Sound & Music; day-2 plan Tue 25 "bus structure, per-room ambience, volume control, first-click unlock" + Thu 27 "music layers, the 5 critical SFX, mix pass"). Audio is one of the three categories we're betting on (day-2 §C3).

> Numbered **05**: plan 04 stays reserved for the `StoryDirector` (plan 03 + day-2 Aug 26 amendment).

Decisions this plan encodes (Gus, Thu Aug 27):
- **One plan, whole stack.** Tuesday's foundation and Thursday's layers ship together; there is no second audio plan.
- **Tension arrives through a channel, not through a dependency.** `TensionChangedEventChannelSO` is the contract the day clock and the `StoryDirector` (plan 04) will raise into. Until they exist, the audio side is driven by signals that already work today (`ClueShared`, `DialogueRequested/Finished`, `RoomChanged`), so the system is audible and testable now and nobody is blocked later.
- **Amended Fri Aug 28 for the night redesign** (day-2 plan, same date). The patrol is cut, so the Approach layer is re-pointed at the **day clock** and the night becomes a short scored beat instead of a section: hide → resolution → morning. **Nothing in the Domain moves and none of the 48 tests change** — the layer weights were never coupled to a patrol, only to a `TensionLevel`. What changes is who raises it, plus two new channel contracts and four new cues.
- **Room navigation doesn't exist yet, so this plan defines its audio-facing contract:** `RoomChangedEventChannelSO(RoomId)`. Janhavi's navigation raises it; ambience only listens. One line on her side, zero coupling.
- **Settings panel is in scope** — 3 sliders + mute, persisted. "Volume control in menu ☐" is a pre-upload checklist item in GDD §8, not a nicety.
- **Clips are placeholders / empty slots.** Every component must run with `null` clips and empty cue banks without throwing. Real audio is recorded or licensed by Gus (jam rule: no AI audio) and gets its `CREDITS.md` row the same day it lands.
- **Build it, don't buy it — because the platform says so.** `Docs/research/audio-tooling.md` compares the alternatives (mixer snapshots, Audio Random Container, Timeline, FMOD, Wwise, Sonity, JSAM). On Web the AudioMixer only changes volume, so snapshots, DSP and middleware-style mixing are unavailable or risky; what survives is exactly the thin volume-moving C# below. One adoption comes out of it: `ObjectPool<AudioSource>` in `SfxPlayer`. The Audio Random Container was the one built-in worth a spike; **dropped Aug 28 without running it** (Gus's call, two days from close) — clip variation stays in `NoRepeatPicker`, which is written and green.

## ⚠️ One design decision that needs your call

**The lie layer must not be wired to `RoomLeaked`.** It's the obvious hookup and it breaks the game: the leak fires *only* for the Uncle, so a musical sting on leak tells the player who the traitor is the first time they trade — the whole C1 mechanic, given away for free in 30 seconds.

Recommendation, mirroring `_allowsClueExchange`: **`DialogueSO` gains `bool _playsLieMotif` (off by default)**, and the lie layer is pulsed by *authored* conversations — including ones where nobody is actually lying. That's the GDD §8 hook working as written ("a motif tied to the traitor that plays *before* the betrayal") and it keeps the tell ambiguous, because you decide where it fires. `RoomLeaked` stays **silent**.

The plan below assumes that. If you'd rather drive the lie layer purely from `TensionChanged` (story director's job, plan 04) and leave `DialogueSO` untouched, say so — it's a one-field difference.

## Domain
Pure C#, no UnityEngine, no `UnityEngine.Mathf`, no IO. Namespace `Game.Domain`. Everything below is deterministic and tested without a scene; the engine never appears in a rule.

- `AudioBus` (enum): `Master, Music, Sfx, Ambience`. Also the routing tag on a cue.
- `MusicLayerId` (enum): `Bed, Approach, Lie`. `Approach` carries the GDD §C4 instrument that only Not Grandma brings with her; `Bed` is the near-silent pad under everything. **Since the Aug 28 redesign, "approach" means the *night* approaching, not her walking down the hall** — the layer is driven by the day clock. Same name, same asset, better driver: the clock actually exists and the patrol never will.
- `TensionLevel` (enum): `Calm = 0, Uneasy = 1, Alert = 2`. The day clock and the story director speak in these, never in weights. Mapping suggested to whoever owns the clock: `Calm` for the first half of the day, `Uneasy` past the halfway mark, `Alert` for the last stretch before the player must be hidden.
- `VolumeCurve` (static): the linear-slider ↔ decibel conversion, in one place, because getting it wrong is why a slider at 50% sounds like 90%.
  - `const float MinDecibels = -80f`
  - `float ToDecibels(float linear01)` — clamps to `[0,1]`; `<= 0.0001f` returns `MinDecibels`; otherwise `20 * log10(linear)`.
  - `float FromDecibels(float decibels)` — inverse, clamped to `[0,1]`.
- `VolumeSettings` (mutable, one per run): per-bus linear volume plus a global mute.
  - ctor `VolumeSettings(float master = 1f, float music = 1f, float sfx = 1f, float ambience = 1f)` — every value clamped to `[0,1]`.
  - `float Get(AudioBus bus)` / `void Set(AudioBus bus, float value)` — clamps; `Set` on an undefined enum value throws `ArgumentOutOfRangeException`.
  - `bool IsMuted`, `void SetMuted(bool muted)` — mute never destroys the stored values; unmuting restores exactly what the sliders had.
  - `float EffectiveLinear(AudioBus bus)` — `0` when muted; `Master` returns its own value; the other buses return `bus * master`. This is what gets converted to dB and pushed into the mixer.
  - `event Action Changed` — raised on any mutation that actually changes state (the presentation layer persists on it; no polling).
- `MusicLayerMixer` (mutable): current-vs-target weight per layer, with asymmetric fades — music should sneak in faster than it leaves, or it feels like a bug.
  - ctor `MusicLayerMixer(float fadeInPerSecond, float fadeOutPerSecond)` — throws `ArgumentOutOfRangeException` on non-positive rates. All layers start at `0`.
  - `void SetTarget(MusicLayerId layer, float weight01)` — clamps.
  - `void Tick(float deltaTime)` — throws `ArgumentOutOfRangeException` on negative `deltaTime`; moves each current weight toward its target at the in/out rate, **never overshooting**, snapping when within `0.001f`.
  - `float GetWeight(MusicLayerId layer)`, `void SnapToTargets()`.
- `TensionDirector` (mutable): the rules that turn game state into layer targets. The only place that knows what "Alert" sounds like.
  - ctor `TensionDirector(float lieMotifSeconds)` — throws on non-positive.
  - `TensionLevel Level`, `void SetTension(TensionLevel level)` — throws on undefined enum value.
  - `void PulseLieMotif()` — starts *or refreshes* the lie timer to full duration.
  - `void Tick(float deltaTime)` — throws on negative dt; decays the lie timer to zero, never below.
  - `bool IsLieMotifActive`
  - `float GetTarget(MusicLayerId layer)` — `Bed` → `1`; `Approach` → `0 / 0.5 / 1` for `Calm / Uneasy / Alert`; `Lie` → `1` while the pulse is active, else `0`.
- `Crossfader` (mutable): the two-slot ambience fade. Exists as a domain type because "player walks kitchen→bedroom→kitchen faster than the fade" is exactly the case that produces two ambiences playing forever.
  - ctor `Crossfader(float durationSeconds)` — throws on non-positive. A new crossfader is silent and idle: both track ids `null`, `IncomingWeight = 1` (silence is the incoming track), `IsFading = false`.
  - `void To(string trackId)` — `null` is legal and means silence. If `trackId` equals the incoming track, **no-op** (a re-entry mid-fade must not restart it). Otherwise the current incoming becomes outgoing, `trackId` becomes incoming, and progress restarts from `1 - currentWeight`. **The invariant that follows, and the one the tests pin:** the track that was fading in keeps its exact audible level across the swap. A reversal is therefore seamless; a third track mid-fade enters at the level the previous one had, which is the price of two slots and is inaudible at a 2 s fade.
  - `void Tick(float deltaTime)` — throws on negative dt; advances progress, clamped at `1`.
  - `string IncomingTrackId`, `string OutgoingTrackId` (null once the fade completes), `float IncomingWeight`, `float OutgoingWeight` (`1 - IncomingWeight`), `bool IsFading`.
- `NoRepeatPicker` (mutable): random clip choice that never plays the same variation twice in a row — the difference between "footsteps" and "a machine gun".
  - ctor `NoRepeatPicker(int count, Random rng)` — throws `ArgumentOutOfRangeException` if `count < 1`, `ArgumentNullException` on null rng. Randomness is injected, per CLAUDE.md.
  - `int Next()` — with `count == 1` always returns `0`; otherwise never returns the previous index.

Deliberately *not* in Domain: which clip, which mixer group, dspTime, PlayerPrefs. The Domain answers "how loud is layer X right now", never "who plays it".

## Events
Channel SOs per the CLAUDE.md pattern, namespace `Game.Events`, subscribe in `OnEnable` / unsubscribe in `OnDisable`.

| Channel | Payload | Raised by | Listened by |
|---|---|---|---|
| `VoidEventChannelSO` | — | generic payload-less channel (CLAUDE.md asks for it; first user is the unlock below) | — |
| `RoomChangedEventChannelSO` | `RoomId` | **navigation (Janhavi)** — contract defined here, one `Raise` call on her side | `AmbienceController`, `AudioCueRouter` (door cue) |
| `TensionChangedEventChannelSO` | `TensionLevel` | the **day clock** and `StoryDirector` (plan 04) — **nobody today** | `MusicDirector`, `AudioCueRouter` (alert cue on rising edge) |
| `NightStartedEventChannelSO` *(a `VoidEventChannelSO` asset)* | — | the day/night system, when the clock expires and the night resolves | `AmbienceController` (cut to the night track), `MusicDirector` (`Alert`), `AudioCueRouter` |
| `DayStartedEventChannelSO` | `int` (day number, 1-based) | the day/night system, each morning | `MusicDirector` (back to `Calm`), `AudioCueRouter` (morning cue; the phone arrives from day 2) |
| `NightResolvedEventChannelSO` | `bool survived` | the night check | `AudioCueRouter` → survived cue or caught cue. **The only place the leak is ever audible** — and only after it already killed you, which is the lesson, not a tell |
| `SfxRequestedEventChannelSO` | `AudioCueSO` | `AudioCueRouter`, UI buttons, anything that wants a one-shot | `SfxPlayer` (single listener) |
| `AudioUnlockedEventChannelSO` *(a `VoidEventChannelSO` asset)* | — | `AudioUnlocker` on the first user gesture | `AmbienceController`, `MusicLayerPlayer` |
| `ClueCollectedEventChannelSO` *(existing)* | `ClueSO` | — | `AudioCueRouter` → reward cue |
| `ItemInspectedEventChannelSO` *(existing)* | `ItemSO` | — | `AudioCueRouter` → main-action cue |
| `ClueSharedEventChannelSO` *(existing)* | `NpcSO, ClueSO` | — | `AudioCueRouter` → share cue (**identical for every NPC** — see the design call above) |
| `DialogueRequestedEventChannelSO` *(existing)* | `DialogueSO` | — | `AudioCueRouter` → lie-motif pulse when `_playsLieMotif` |
| `RoomLeakedEventChannelSO` *(existing)* | `RoomId` | — | **nothing in audio. On purpose.** |

## Data
Namespace `Game.Data`. Assets live in `Assets/Game/ScriptableObjects/Audio/`; the clips themselves in `Assets/Game/Audio/{SFX,Ambience,Music}/`.

- `AudioCueSO` — `[CreateAssetMenu(menuName = "Game/Audio/Cue")]`
  - `AudioClip[] _clips` — one entry = no variation; several = random without repeats. **May be empty**: an empty cue is silence, never an exception.
  - `AudioBus _bus` — routing target (`Sfx` for all five criticals).
  - `float _volumeMin = 1f, _volumeMax = 1f`, `float _pitchMin = 1f, _pitchMax = 1f` — humanization, `[Range(0f, 1f)]` / `[Range(0.5f, 2f)]`.
  - `bool HasClips`, `int ClipCount`, `AudioClip GetClip(int index)`.
- `AmbienceEntryData` (`[Serializable]`): `RoomId _room`, `AudioClip _clip`, `[Range(0f,1f)] float _volume`.
- `AmbienceBankSO` — `[CreateAssetMenu(menuName = "Game/Audio/Ambience Bank")]`
  - `AmbienceEntryData[] _entries`; `bool TryGet(RoomId room, out AudioClip clip, out float volume)` — `false` for an unmapped room, which the controller reads as "fade to silence".
  - `AudioClip _nightAmbience`, `[Range(0f,1f)] float _nightVolume` *(Aug 28)* — the night isn't a room, so it isn't in the room map. One clip covers the whole beat.
- `MusicLayerEntryData` (`[Serializable]`): `MusicLayerId _layer`, `AudioClip _clip`.
- `MusicLayerSetSO` — `[CreateAssetMenu(menuName = "Game/Audio/Music Layer Set")]`
  - `MusicLayerEntryData[] _layers`; `bool TryGetClip(MusicLayerId layer, out AudioClip clip)`.
  - **Authoring contract, worth a tooltip on the asset:** every layer is the *same length and tempo*, mixed to be heard together. They are started once, in the same call, and never stopped — only their volume moves. That's the "stems that toggle" of GDD §8, and it dodges WebGL scheduling entirely (see below).
- `DialogueSO` *(modified)*: `bool _playsLieMotif` (public `PlaysLieMotif`), **off by default** — same gating pattern as `_allowsClueExchange`. No other dialogue change; the dialogue Domain and its tests are untouched.

## Presentation
Namespace `Game.Presentation`, new folder `Assets/Game/Scripts/Presentation/Audio/`. Thin components: they own `AudioSource`s and translate domain weights into volumes. No rules.

| Component | Responsibility | Channels |
|---|---|---|
| `AudioUnlocker` | WebGL autoplay policy. In `Awake`, sets `AudioListener.pause = true`. Listens to the Input System click action (same actions asset as `ClickRouter`); on the **first** gesture: `AudioListener.pause = false`, raise `AudioUnlocked`, unsubscribe, never run again. Exposes `bool IsUnlocked`. Nothing plays before this. | raises `AudioUnlocked` |
| `VolumeController` | Owns `VolumeSettings`. `Awake`: loads from `PlayerPrefs` (defaults 1/1/1/1, unmuted) and pushes every bus into the `AudioMixer`. `SetVolume(AudioBus, float)` / `GetVolume(AudioBus)` / `SetMuted(bool)`; on `VolumeSettings.Changed`, converts with `VolumeCurve.ToDecibels(settings.EffectiveLinear(bus))`, calls `AudioMixer.SetFloat(exposedParam, dB)`, writes `PlayerPrefs` and `PlayerPrefs.Save()`. Serialized: `AudioMixer _mixer` + the four exposed param names. Keys: `audio.master`, `audio.music`, `audio.sfx`, `audio.ambience`, `audio.muted`. | — |
| `VolumeSettingsPanelView` | 3 `Slider`s (Master / Music / SFX) + a mute `Toggle`. `OnEnable`: reads current values into the widgets **without** re-firing callbacks; slider change → `VolumeController.SetVolume`. Ambience gets no slider (it rides the master — one fewer thing to explain). Built as a **standalone prefab** so it drops into the pause menu unchanged when those screens exist. | — |
| `SfxPlayer` | A pool of `AudioSource`s on the SFX mixer group via **`UnityEngine.Pool.ObjectPool<AudioSource>`** (built-in since 2021 — no hand-rolled round-robin), pre-warmed in `Awake` to a serialized size (default 8) with `maxSize` set so nothing is allocated per shot. Listens `SfxRequested(AudioCueSO)`: skips silently when the cue is null or empty, picks the variation with a per-cue `NoRepeatPicker` (`Dictionary<AudioCueSO, NoRepeatPicker>`, filled lazily), randomizes volume/pitch inside the cue's ranges, `PlayOneShot`, returns the source to the pool when the clip's length has elapsed. Also `Play(AudioCueSO)` for direct calls. | listens `SfxRequested` |
| `AmbienceController` | Two looping `AudioSource`s (A/B) on the Ambience group + a `Crossfader`. Listens `RoomChanged` → resolves the clip through `AmbienceBankSO` and calls `Crossfader.To(trackId-or-null)`; on the swap, assigns the incoming clip to the free source and plays it (only once unlocked). `Update` ticks the crossfader and writes `IncomingWeight`/`OutgoingWeight` into the two source volumes, stopping the outgoing source when the fade completes. Unmapped room or null clip = fade to silence. Listens `AudioUnlocked` to start the pending track. *(Aug 28)* Also listens `NightStarted` → `To("night")` and `DayStarted` → back to the current room's track. | listens `RoomChanged`, `NightStarted`, `DayStarted`, `AudioUnlocked` |
| `MusicLayerPlayer` | One looping `AudioSource` per `MusicLayerId` on the Music group, clips from `MusicLayerSetSO`, all at volume `0`. On `AudioUnlocked`, calls `Play()` on all of them **in the same frame**, and never stops them again. `ApplyWeights(MusicLayerMixer)` writes each layer's weight into its source volume. Layers with no clip are skipped and never touched. | listens `AudioUnlocked` |
| `MusicDirector` | Owns `TensionDirector` + `MusicLayerMixer` (fade rates and lie duration serialized). Listens `TensionChanged` → `SetTension`. `Update`: ticks the director, pushes `GetTarget` into `SetTarget` per layer, ticks the mixer, calls `MusicLayerPlayer.ApplyWeights`. Public `PulseLieMotif()` for the router. Exposes `TensionLevel CurrentTension` for debugging. *(Aug 28)* `NightStarted` → `SetTension(Alert)`; `DayStarted` → `SetTension(Calm)`, so every morning starts from silence without the clock having to re-announce it. | listens `TensionChanged`, `NightStarted`, `DayStarted` |
| `AudioCueRouter` | **The only component that knows gameplay events make sound.** Serialized cue slots, one per signal, each optional. `ItemInspected` → main-action cue · `ClueCollected` → reward cue · `ClueShared` → share cue (same for every NPC) · `RoomChanged` → door/step cue · `TensionChanged` → alert cue **on a rising edge only** (it must not retrigger while already Alert) · `DialogueRequested` → `MusicDirector.PulseLieMotif()` when `dialogue.PlaysLieMotif`. *(Aug 28)* `NightStarted` → hide/night-falls cue · `NightResolved(true)` → survived cue, `NightResolved(false)` → caught cue (the §8 "betrayal/reveal" sound, and the one place Not Grandma's instrument plays alone) · `DayStarted` → morning cue. Raises `SfxRequested`; never touches an `AudioSource`. | listens the nine above; raises `SfxRequested` |

**Why the router instead of a `PlayCue()` inside each feature:** the notebook, the exchange and the dialogue systems are done and green. This keeps them that way, keeps the whole mix in one inspector, and means the Aug 30 bugfix window can mute one noisy cue without touching gameplay code.

### WebGL notes (build health is sacred)
Every constraint below is documented, not folklore — sources in `Docs/research/audio-tooling.md`.

- **Silence until the first click** is structural here, not a workaround: `AudioListener.pause = true` in `Awake`, everything else waits on `AudioUnlocked`. Browsers refuse audio until a click, touch or key press. If the browser blocks audio the game still plays — exactly what GDD §8's checklist demands.
- **The mixer can only change volume on Web.** Effects and DSP aren't supported, and that has one content consequence: **the muffled "hiding under the bed" sound cannot be a lowpass filter.** If we want it, it's a separately recorded muffled clip. Same for reverb per room — bake it into the ambience, don't filter it.
- **No `PlayScheduled`, no `dspTime`.** There's a known WebGL issue where the scheduled clip starts early and two copies of the track play over each other. Layers start together and run for the whole session; only volumes move.
- **Loop seams are a build-only bug.** AAC encoding on Web alters the first 1024 samples, so a loop that is seamless in the editor can click in the browser. GDD §8's "no audible loop seams ☐" is checked **in the uploaded build**; if a loop clicks, pad the clip rather than fight the encoder.
- **Positive pitch only** on Web. The cue's `[Range(0.5f, 2f)]` is fine; a negative pitch is not an option for reversed effects.
- **Import settings** (checklist step 9): WebGL does **not** support streaming. Long ambience/music → *Compressed In Memory*, Vorbis, quality ≈ 60, **Force To Mono** on; short SFX → *Decompress On Load*, Vorbis, mono. Disable *Preload Audio Data* on the music layers so the first frame isn't waiting on them.
- **Budget:** keep total audio under ~10 MB compressed and check the build report after the next re-upload. Three 90 s mono layers at Vorbis 60 is roughly 1 MB — the danger is an uncompressed WAV sneaking in, not the layer count.
- `PlayerPrefs` on WebGL lands in IndexedDB — fine, but call `PlayerPrefs.Save()` on every change; a tab closed without it loses the setting.

## Editor setup checklist
All manual work by Gus. Steps 1–3 can happen before the code exists.

1. Create folders: `Assets/Game/Scripts/Presentation/Audio/`, `Assets/Game/ScriptableObjects/Audio/`, and `Assets/Game/Audio/{SFX,Ambience,Music}/`.
2. **AudioMixer** `MX_Game` in `Assets/Game/Audio/` (Create → Audio Mixer). Groups: `Master` → children `Music`, `SFX`, `Ambience`. On each group, right-click its Volume → *Expose to script*, then rename in the Exposed Parameters dropdown to exactly `MasterVolume`, `MusicVolume`, `SfxVolume`, `AmbienceVolume`. A typo here is a silent slider that looks like a code bug.
3. Import placeholder clips (yours — anything, even a hum) into the three audio folders so the wiring can be verified before the real recordings exist. Empty slots are legal; a placeholder is faster to debug.
4. After the code compiles: channel assets in `ScriptableObjects/Channels/` → `CH_RoomChanged`, `CH_TensionChanged`, `CH_SfxRequested`, `CH_DayStarted`, `CH_NightResolved`, and two `VoidEventChannelSO`s, `CH_AudioUnlocked` and `CH_NightStarted`.
5. Cue assets in `ScriptableObjects/Audio/Cues/` (`Game/Audio/Cue`) — the five criticals from GDD §8 plus UI and the night beat: `SFX_Interact` (main action), `SFX_ClueCollected` (reward), `SFX_Alert` (suspicion), `SFX_Reveal` (betrayal — reused as *caught*), `SFX_Error` (loss), `SFX_UIClick`, `SFX_RoomChange`, `SFX_Hide`, `SFX_NightSurvived`, `SFX_Morning`, `SFX_Phone`. Set `_bus = Sfx`; give at least one cue 2–3 clip variations so the no-repeat path is actually exercised.
6. `AMB_House` (`Game/Audio/Ambience Bank`) with the 4 rooms **plus the night clip**. Map only 3 rooms at first — the unmapped one verifies the fade-to-silence path.
7. `MUS_Main` (`Game/Audio/Music Layer Set`) with `Bed`, `Approach`, `Lie`, same length and tempo (authoring contract above).
8. Scene wiring in `ClueSandbox`: an `Audio` prefab with children `AudioUnlocker`, `VolumeController`, `SfxPlayer`, `AmbienceController`, `MusicLayerPlayer`, `MusicDirector`, `AudioCueRouter`. Assign **every** `AudioSource`'s *Output* to its mixer group — a source with no output bypasses the mixer and ignores the sliders, the single most common wiring bug here. Wire the mixer + the 4 exposed names into `VolumeController`, and every channel into its listener.
9. Audio import settings pass, per the WebGL table above. Do this **before** the next build, not after.
10. Settings panel prefab: 3 sliders + mute toggle + `VolumeSettingsPanelView`, wired to `VolumeController`. Drop it in the sandbox canvas for now; it moves into the pause menu untouched later.
11. **Temporary drivers** (navigation and the day/night loop don't exist yet): a small debug component of yours with 4 buttons raising `CH_RoomChanged`, 3 raising `CH_TensionChanged`, and 3 more for `CH_NightStarted` / `CH_NightResolved(true|false)` / `CH_DayStarted`. Sandbox scene only; delete it when the real systems land.
12. Tick `_playsLieMotif` on **`DLG_Test_Branching` only**, so both the pulse and the no-pulse path are reachable.
13. Smoke test **in a WebGL build, not the editor**: no sound before the first click · sliders move the right buses and survive a page reload · switching rooms crossfades without stacking · Calm→Alert brings the approach layer in and back out · night cuts to the night ambience and morning returns to the room's · an empty cue plays nothing and throws nothing.
14. **Mix pass with the mixer open in *Edit in Play Mode*** (Thursday's task in the day-2 plan): play the sandbox, arm the button on the AudioMixer window, balance the buses and the layer volumes by ear — the values stick when you exit play mode. This is the tool that replaces a tuning script; don't hardcode a mix in a component.
15. Run Test Runner → EditMode until the six new test files are green.
16. `CREDITS.md`: one row per audio asset, the day it enters the repo — licensed packs included, license column filled.

## Tests
`Assets/Tests/Editor/`, one file per domain type, `MethodOrRule_Scenario_ExpectedResult`. **Written — 55 cases across 6 files**, against the Domain as committed in `a7137bb`.

**`VolumeCurveTests.cs`** (6)
- `ToDecibels_FullVolume_IsZeroDecibels`
- `ToDecibels_Zero_IsMinDecibels`
- `ToDecibels_Half_IsAboutMinusSixDecibels`
- `ToDecibels_OutOfRange_IsClamped`
- `FromDecibels_RoundTrips_ToOriginalLinear`
- `FromDecibels_MinDecibels_IsSilence`

**`VolumeSettingsTests.cs`** (9)
- `Ctor_Default_IsFullVolumeAndUnmuted`
- `Ctor_OutOfRangeValues_AreClamped`
- `Set_ClampsToUnitRange`
- `Set_UndefinedBus_Throws` — covers `Get` too
- `EffectiveLinear_BusIsScaledByMaster`
- `EffectiveLinear_Master_IsNotScaledByItself`
- `EffectiveLinear_WhenMuted_IsZeroForEveryBus`
- `SetMuted_DoesNotChangeStoredVolumes`
- `Changed_RaisedOnlyWhenValueActuallyChanges`

**`MusicLayerMixerTests.cs`** (10)
- `Ctor_NonPositiveRate_Throws`
- `GetWeight_NewMixer_IsZeroForEveryLayer`
- `Tick_FadesTowardTargetAtFadeInRate`
- `Tick_NeverOvershootsTarget`
- `Tick_FadesOutAtFadeOutRate`
- `Tick_DoesNotMoveLayersAlreadyAtTarget`
- `Tick_NegativeDeltaTime_Throws`
- `SetTarget_ClampsToUnitRange`
- `SnapToTargets_AppliesTargetsImmediately`
- `Tick_ManySmallSteps_ReachesTargetExactly` — the snap-to-target epsilon, at frame-sized steps

**`TensionDirectorTests.cs`** (12)
- `Ctor_NonPositiveLieDuration_Throws`
- `Level_NewDirector_IsCalm`
- `GetTarget_Bed_IsAlwaysFull`
- `GetTarget_Approach_ScalesWithTensionLevel`
- `SetTension_UndefinedLevel_Throws`
- `GetTarget_UndefinedLayer_Throws`
- `PulseLieMotif_ActivatesLieLayer`
- `Tick_LieMotifExpiresAfterDuration`
- `Tick_LieMotifStaysActiveBeforeDuration`
- `PulseLieMotif_WhileActive_RefreshesFullDuration`
- `Tick_AfterExpiry_KeepsLieLayerSilent`
- `Tick_NegativeDeltaTime_Throws`

**`CrossfaderTests.cs`** (11)
- `Ctor_NonPositiveDuration_Throws`
- `NewCrossfader_IsSilentAndIdle`
- `To_FirstTrack_FadesItIn`
- `Weights_AlwaysSumToOne`
- `To_SameTrackMidFade_DoesNotRestartFade`
- `To_SameTrackAfterFadeCompleted_DoesNothing`
- `To_NewTrackMidFade_ResumesFromCurrentWeight`
- `Tick_CompletesFadeAndClearsOutgoingTrack`
- `Tick_PastDuration_DoesNotOvershoot`
- `To_Null_FadesToSilence`
- `Tick_NegativeDeltaTime_Throws`

**`NoRepeatPickerTests.cs`** (7)
- `Ctor_CountBelowOne_Throws`
- `Ctor_NullRandom_Throws`
- `Next_SingleClip_AlwaysReturnsZero` — the one-clip cue must not hang in the retry loop
- `Next_NeverRepeatsPreviousIndex`
- `Next_AlwaysReturnsIndexInRange`
- `Next_SameSeed_ProducesSameSequence` — injected randomness means a variation order is reproducible in a bug report
- `Next_EventuallyReturnsEveryIndex` — no-repeat must not collapse into an A-B-A-B rut

## Out of scope
The day clock that decides *when* tension rises and the night check itself (this plan only consumes `TensionChanged`, `NightStarted`, `NightResolved` and `DayStarted`); the phone item and the police-call UI (this plan gives them cues, nothing else); story beats raising tension (plan 04); room navigation itself (Janhavi — this plan defines `RoomChanged` and nothing else); the recorded or licensed audio content (Gus, by hand, jam rule); 3D/spatial audio and per-object footsteps (2D, non-positional, `spatialBlend = 0`); mixer snapshots and effects for the night section (unsupported on Web anyway — `Docs/research/audio-tooling.md`); music that changes with day/night beyond the three layers; audio middleware of any kind (FMOD is the post-jam upgrade path, Wwise has no WebGL support at all); and the pause-menu screens the settings panel will eventually live in.
