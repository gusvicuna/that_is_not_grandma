# 08 — UI click SFX on clue drag
**Goal:** Picking up a clue to trade makes a sound — `SFX_UIClick` when the drag starts, `SFX_Error` when the clue can't be dragged — through a reusable emitter any UI element can use. **Priority:** Must-Have (GDD §5 "Audio system", §8 Sound & Music — `SFX_UIClick` and `SFX_Error` are two of the five critical SFX; plan 05 assumed "UI buttons" would raise `SfxRequested` and the component was never written)

Decisions this plan encodes (Gus, Sep 16):
- **Only the start of the drag makes the click.** The drop is already covered by `CH_ClueShared` → `_clueSharedCue` in `AudioCueRouter`; a second sound there would double up once that slot is filled.
- **A rejected drag plays `SFX_Error`.** Clues already shared with the current NPC cancel the drag (`ClueSharePanelView` renders them at 40% alpha). Silence would be ambiguous — the player can't tell "not draggable" from "the game didn't hear me".
- **The emitter is generic.** The same component sonifies buttons later (close, pause, notebook, play again) with zero new code, per the day-2 rule that a start-to-finish build beats an elegant subsystem.

## Domain
**None.** No rule changes: this is presentation reacting to a pointer event. Clip variation and no-repeat already live in `NoRepeatPicker` (plan 05), and `SfxPlayer` is untouched.

## Events
No new channels. The existing `CH_SfxRequested` (`AudioCueEventChannelSO`, payload `AudioCueSO`) is the only one used, with `SfxPlayer` as its single listener.

| Channel | Payload | Raised by | Listened by |
|---|---|---|---|
| `CH_SfxRequested` *(existing)* | `AudioCueSO` | **`UiSfxEmitter`** *(new raiser)*, `AudioCueRouter` | `SfxPlayer` |

`UiSfxEmitter` is a second raiser on a channel `AudioCueRouter` already raises — that is the point of the channel, and `SfxPlayer` cannot tell them apart.

## Data
No new ScriptableObject types. Two existing cue assets, both already created in `Assets/Game/ScriptableObjects/Audio/Cues/`:

| Asset | State today | Needed for this plan |
|---|---|---|
| `SFX_UIClick` | 1 clip (`SFX_UIClick.wav`, Sonniss GDC 2024), `_bus = Sfx` | ready |
| `SFX_Error` | **`_clips` empty** | needs a clip; the feature ships silent-but-working until Gus licenses one |

An empty cue is silence, never an exception (`SfxPlayer.Play` returns early on `!cue.HasClips`), so the rejected-drag path can be wired and tested before the error sound exists.

## Presentation
Namespace `Game.Presentation`.

| Component | Folder | Responsibility | Channels |
|---|---|---|---|
| `UiSfxEmitter` *(new)* | `Presentation/UI/` | One cue, one channel, one job. Serialized: `AudioCueSO _cue`, `AudioCueEventChannelSO _sfxRequestedChannel`. Public `Play()` raises the channel with `_cue`; null cue or null channel = silent no-op, never a throw. `Play()` takes no arguments so it appears in the `Button.onClick` UnityEvent dropdown as a static call — that is what makes it reusable without code. | raises `SfxRequested` |
| `DraggableClueItem` *(changed)* | `Presentation/UI/` | Gains two serialized `UiSfxEmitter` references: `_dragStartSfx` and `_dragRejectedSfx`. In `OnBeginDrag`, the rejection branch (`!_isDraggable`) calls `_dragRejectedSfx.Play()` before clearing `eventData.pointerDrag`; the accepted branch calls `_dragStartSfx.Play()` before creating the ghost. No other method changes. | — (delegates) |

Two emitter components sit on the `ClueShareItem` prefab, each carrying its own cue. `DraggableClueItem` therefore holds no cue or channel field of its own: it says *when* a sound happens, the emitter says *which*, and the cue asset says what it sounds like. Same separation `AudioCueRouter` already uses.

Null-guard both references (`if (_dragStartSfx != null)`), matching the "all optional" style of `AudioCueRouter`'s cue slots: a clue item dropped into a test scene without audio must still drag.

### Why not put the cue directly on `DraggableClueItem`
It would be two fewer components in the prefab, but every button that wants a click would then need its own code change. `UiSfxEmitter` is ~15 lines and covers all of them from the inspector.

### Ghost copies
`CreateGhost()` instantiates the whole clue item and destroys only the `DraggableClueItem` component, so the ghost keeps its `UiSfxEmitter`s. Harmless — nothing calls `Play()` on a ghost, and the ghost is destroyed on `OnEndDrag`. Do **not** add `Play()` calls to anything that runs on the ghost.

## Editor setup checklist
All manual work by Gus, after the code compiles:

1. **`Assets/Game/Prefabs/UI/Modals/Clue Share/ClueShareItem.prefab`** — open it and add **two** `UiSfxEmitter` components to the root object:
   - first: `_cue` = `SFX_UIClick`, `_sfxRequestedChannel` = `CH_SfxRequested`
   - second: `_cue` = `SFX_Error`, `_sfxRequestedChannel` = `CH_SfxRequested`
2. On the same object, in `DraggableClueItem`: drag the first component into `_dragStartSfx` and the second into `_dragRejectedSfx`. The inspector shows both as `UiSfxEmitter` — check the order in the component list, or temporarily set a cue to `None` to tell them apart.
3. **Save Project** (`File → Save Project`), not just the scene — prefab and SO changes need it.
4. When the error sound exists: drop it in `Assets/Game/Audio/Clips/SFX/`, assign it to `SFX_Error._clips`, add its row to `CREDITS.md` the same day, and re-save.
5. *(Optional, same component, no code)* Sonify existing buttons by adding a `UiSfxEmitter` with `SFX_UIClick` and pointing their `onClick` at `UiSfxEmitter.Play`: close button on the Clue Share panel, pause, notebook toggle, play again, settings sliders' close.

**Import settings reminder (WebGL):** `SFX_UIClick.wav` is 66 KB, but the room-change and alert clips are ~1 MB each. Set **Load Type: Compressed In Memory** + **Compression Format: Vorbis** on everything under `Assets/Game/Audio/Clips/` before the build.

## Tests
**No EditMode tests.** Per CLAUDE.md the deal is EditMode-only, Domain-only, and this plan adds no Domain code: `UiSfxEmitter` is a `MonoBehaviour` whose whole body is a null check plus a `Raise`, and a test for it would need a scene. The logic it depends on is already covered — `NoRepeatPickerTests` (variation) and `VolumeCurveTests` / `VolumeSettingsTests` (bus levels).

Manual verification instead, in `Game.unity`:

| Given | When | Then |
|---|---|---|
| An exchange is open with at least one unshared clue | The player starts dragging that clue | `SFX_UIClick` plays once, at the start, not per frame of the drag |
| The same clue was already shared with this NPC (40% alpha) | The player tries to drag it | `SFX_Error` plays (silence until the clip exists), the drag does not begin |
| The panel lists several clues | The player drags several in a row | The click plays each time — `NoRepeatPicker` varies clips only when the cue has more than one |
| Audio has not been unlocked yet (first click of the session) | The player's first gesture is the drag | The click is not swallowed: `AudioUnlocker` unpauses the listener on that same gesture |

That last row is the one to watch on WebGL: if the first drag is silent but the second works, the unlock ordering in `AudioUnlocker` is the suspect, not this plan.

## Out of scope
- Any sound on drop or on a completed exchange — that is `_clueSharedCue` on `AudioCueRouter`, a slot that is still empty (plan 05).
- Hover/selection sounds, and UI sound for the notebook, dialogue or police-call panels beyond the optional button pass in step 5.
- Filling the remaining empty cue slots on `AudioCueRouter` (`_clueSharedCue`, `_hideCue`, `_nightSurvivedCue`, `_caughtCue`, `_morningCue`) — separate editor work, no code.
- Sourcing the error sound itself (jam rule: licensed or human-made, credited the same day).
- Audio import-setting pass for WebGL, noted above as a reminder but tracked outside this plan.
