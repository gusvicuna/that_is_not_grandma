# Audio test pass — run this once the real clips are in

> For the WebGL build on itch, **not** the editor. Half of these can only fail in a browser.
> Companion to `Docs/plans/05-audio-music.md`. The four ☐ at the end are GDD §8's pre-upload gate.

## Before you build

- [ ] **Import settings pass on the new clips** (plan step 9). The easiest thing to forget: you added files after the last pass, and they went in as raw WAV.
- [ ] `CREDITS.md` has a row per audio asset — author, source, license. Same day it enters the repo, jam rule.
- [ ] Every cue asset has its clips assigned, and **at least one cue has 2–3 variations** so the no-repeat path is actually exercised.
- [ ] `AMB_House`: the 4 rooms mapped + the night clip.
- [ ] `MUS_Main`: the 3 layers, **same length and tempo** — that contract is what keeps them in sync.
- [ ] Note the `.data` size in `Builds/Web/Build/` before and after. Audio should stay under ~10 MB.

## In the browser, in this order

### 1. It loads and it's silent
- [ ] **F12 → Console clean.** Look here first, always.
- [ ] No sound at all before the first click. This is correct behaviour, not a bug.
- [ ] First click → audio starts. The game must be playable even if the browser never allows sound.

### 2. Ambience
- [ ] Each of the 4 rooms plays its own loop.
- [ ] Moving room to room crossfades — never two ambiences stacked.
- [ ] **Pace back and forth between two rooms faster than the fade.** Nothing should jump, restart, or double up.
- [ ] Re-entering the room you're already fading into doesn't rewind the fade.
- [ ] A room with no clip mapped fades to silence instead of leaving the previous room playing.
- [ ] **Loop seams:** let each ambience run through at least two full loops. Web re-encodes audio and alters the first samples, so a loop that is seamless in the editor can click here. This check has no editor equivalent.

### 3. Music layers
- [ ] Bed layer audible from the start.
- [ ] Tension rising brings the Approach layer in; dropping back fades it out **slowly** (asymmetric on purpose).
- [ ] **After 5+ minutes the layers are still in sync.** They start together and never stop; if they've drifted, something is stopping and restarting a source.
- [ ] A dialogue marked `PlaysLieMotif` pulses the lie layer; an unmarked one does nothing.
- [ ] Two marked conversations back to back don't cut the motif short — the second refreshes it.

### 4. Night and morning
- [ ] `NightStarted` → cuts to the night ambience and pushes tension to Alert.
- [ ] `NightResolved(true)` → survived cue · `NightResolved(false)` → caught cue.
- [ ] `DayStarted` → back to the current room's ambience, tension back to Calm.
- [ ] Across two full day/night cycles nothing accumulates: no doubled ambience, no layer stuck up.

### 5. SFX
- [ ] Each of the five criticals fires at its moment: interact · clue collected · alert · caught/reveal · error.
- [ ] A cue with variations never plays the same clip twice in a row.
- [ ] **Click interactables as fast as you can.** The pool is 8 sources — no crackle, no silence, no error.
- [ ] A cue with no clips assigned plays nothing and throws nothing.

### 6. Volume and mix
- [ ] Each slider moves **only** its own bus. Easiest check: drop one to zero and confirm what stays.
- [ ] Mute silences everything; unmute restores the same values, not zeros.
- [ ] **Reload the page (F5): the settings survived.** This is the real test that PlayerPrefs reached IndexedDB.
- [ ] Nothing bypasses the mixer: if a sound ignores its slider, that source has no Output group assigned.
- [ ] **No clipping.** The worst case is alert sting + music layers + ambience + an SFX at once. Force it and listen for distortion.

### 7. The theme (don't skip this one)
- [ ] **Sharing a clue with the Uncle sounds exactly like sharing with anyone else.** No sting, no motif, no difference. A sound here hands the player the traitor on their first trade and kills the whole mechanic.
- [ ] Nothing audible happens on `RoomLeaked`. Ever.

### 8. Cold and elsewhere
- [ ] Load the itch page in **incognito / a browser you've never used**. Cold cache, real load time, no saved PlayerPrefs.
- [ ] Try a second browser (Chrome and Firefox decode audio differently).

## GDD §8 pre-upload gate

- [ ] Volume control in menu
- [ ] No clipping
- [ ] No audible loop seams
- [ ] The game doesn't break if the browser blocks audio autoplay

## If something is silent, check in this order

1. Is the channel asset wired the right one? In play mode each channel shows its **listener count** — zero means nobody subscribed, or the component holds a different asset.
2. Does the `AudioSource` have an **Output** group? Without it, it bypasses the mixer and ignores every slider.
3. Are the mixer's exposed parameter names exactly `MasterVolume`, `MusicVolume`, `SfxVolume`, `AmbienceVolume`? A typo logs a warning and looks like a code bug.
4. Watch out for the channels the compiler can no longer tell apart: `CH_AudioUnlocked` vs `CH_NightStarted` (both Void), `CH_RoomChanged` vs `CH_RoomLeaked` (both RoomId).
