# 07 — Story content for `Game.unity`

**Goal:** turn `Docs/Not grandma's story line.drawio` into beat assets, content assets and scene objects, so the Game scene plays the flowchart start to finish. **Priority:** Must-Have (this *is* the vertical slice).

**Owner: Gus.** All editor work. No new C# is required for anything below — the systems from plans 01–06 already cover every node of the diagram. Rooms are left as `<room?>` on purpose: pick them while placing objects.

---

## 0. Scene audit (state on Aug 29, 17:36)

Wired and good in `Game.prefab` / `Game Manager.prefab` / `UI.prefab` / `Audio.prefab`:

- Every channel reference on `DayNightCycle`, `NightSurvivalChecker`, `StoryDirectorBehaviour`, `StorySceneBinder`, `ExchangeController`, `DialogueController`, `PoliceCallController`.
- `RoomController.roomIds` = `[0,1,2,3]` and `rooms[0..3]` overridden in the scene to the four room objects. Order inside `Game.prefab` is Kitchen / LivingRoom / BedRoom / Bathroom — **check that this matches the `RoomId` enum order**; a mismatch is silent and would leak the wrong room.
- UI: dialogue, notebook, share panel, item popup, hide confirm, night sequence, game end — all present in `UI.prefab`.
- `PoliceCallController`: trust 2, first available day 2 (= the morning after night 1, exactly as the diagram draws it).

Missing or broken, in the order it will bite:

- [ ] **All 8 `RoomNavigation` / `RoomNavigationPrevious` arrows have `roomController = None`** in the scene. Removing the `RoomController` object from the rooms prefab instance nulled every reference, and `RoomNavigation.Interact()` has no null guard, so the first arrow click throws a `NullReferenceException`. Drag `Game Manager` into all 8.
- [ ] `StoryDirectorBehaviour._beats` is **size 0** in the scene instance (the prefab holds `Beat_NotGrandmaArrives`). Nothing in the story can fire until §3 is dragged in.
- [ ] `StorySceneBinder._actors` is 9 **empty** slots; `_npcs` is empty. Fill after §4.
- [ ] The four rooms are empty shells — no NPC, item, hiding spot or phone instance in any of them. The prefabs exist (`Prefabs/Game/NPC`, `Interactable Item`, `Hiding Object`).
- [ ] `DayNightCycle._secondsPerDay = 50` — testing value. Set the shipping one before the build.
- [ ] Content: only the `Test/` clue, item and dialogue assets exist. Everything in §1 and §2 is still to create.

---

## 1. Clue and item assets

`Assets/Game/ScriptableObjects/Clues/` — **Create ▸ Game ▸ Data ▸ Clue**. Text is yours to write (jam rule); ids and flags below are configuration.

| Asset | `_id` | `_roomId` | `_isEvidence` | Role in the diagram |
|---|---|---|---|---|
| `CLUE_NG_First` | `clue_ng_first` | `<room?>` | off | "Player finds ?" — the clue that makes NotGrandma arrive |
| `CLUE_About_Mother` | `clue_about_mother` | room it is hidden in | off | Unlocks the Mother |
| `CLUE_About_Uncle` | `clue_about_uncle` | room it is hidden in | off | Unlocks the Uncle **and** is what the Cousin trades for evidence |
| `CLUE_About_Cousin` | `clue_about_cousin` | room it is hidden in | off | Unlocks the Cousin |
| `CLUE_NG_Useless` | `clue_ng_useless` | any | off | "Player receives useless NotGrandma clue" — the fallback every NPC returns |
| `CLUE_NG_Evidence` | `clue_ng_evidence` | any | **on** | The only clue that wins the police call |

- [ ] `_roomId` is not decoration: `ExchangeLog` burns **that** room when the clue reaches the Uncle. Set each character clue's room to the room it is found in.
- [ ] With 4 rooms and 3 leakable clues, three careless trades leave exactly one safe room. That is the intended squeeze — just know that nothing prevents an unwinnable run.
- [ ] Items: one `ItemSO` per searchable prop that carries a clue, plus a few red herrings (`_isInspectable` on, no clue) for the Mother's "useless items may appear" branch, plus `ITEM_Phone`.

## 2. Dialogue assets and NPC exchange tables

Dialogue assets to create (names are structure — the lines are yours):

- Intro: `DLG_Intro_Monologue`, `DLG_Intro_Mother`, `DLG_Intro_Cousin`, `DLG_Intro_Uncle`, `DLG_Intro_NotGrandma`, `DLG_Monologue_SomethingWrong`, `DLG_Monologue_WhatToDo`
- Suspicious pass (after she arrives): `DLG_Suspicious_Mother`, `DLG_Suspicious_Uncle`, `DLG_Suspicious_Cousin`, `DLG_Scary_NotGrandma`
- Main loop, **`AllowsClueExchange` on**: `DLG_Mother_Worried`, `DLG_Uncle_Anxious`, `DLG_Cousin_Scary`
- Reactions: `DLG_Cousin_Evidence`, `DLG_Police_Wrong` (police intro / phone-unavailable already exist)

NPC assets (`Npcs/`, already created — fill them in):

- [ ] `NPC_Uncle`: `_leaksToNotGrandma` **on**, `_fallbackReturnClue = CLUE_NG_Useless`, no exchange entries. This is the whole leak mechanic — no beat needed.
- [ ] `NPC_Mother`: leak **off**, fallback `CLUE_NG_Useless`.
- [ ] `NPC_Cousin`: leak **off**, one exchange entry `given = CLUE_About_Uncle → returned = CLUE_NG_Evidence`, fallback `CLUE_NG_Useless`.
- [ ] `NPC_NotGrandma`: no exchange entries, no fallback — `OffersExchange` must stay false, she is never a trading partner.

## 3. Beats

`Assets/Game/ScriptableObjects/Story/` — **Create ▸ Game ▸ Story ▸ Beat**, then drag them into `StoryDirectorBehaviour._beats` **in this order** (declaration order is evaluation order).

### Intro — day 1, free roam

| # | Beat id | Trigger / match | Effects |
|---|---|---|---|
| 1 | `beat_intro_wake` | `DayStarted`, day 1 | `PlayDialogue DLG_Intro_Monologue` · `SetTension Calm` |
| 2 | `beat_intro_mother` | `RoomEntered <room?>` | `PlayDialogue DLG_Intro_Mother` |
| 3 | `beat_intro_cousin` | `RoomEntered <room?>` | `PlayDialogue DLG_Intro_Cousin` |
| 4 | `beat_intro_uncle` | `RoomEntered <room?>` | `PlayDialogue DLG_Intro_Uncle` |
| 5 | `beat_ng_arrives` | `ClueCollected clue_ng_first` | `MoveActor npc_notgrandma → <room?>` · `PlayDialogue DLG_Intro_NotGrandma` · `PlayDialogue DLG_Monologue_SomethingWrong` · `SetTension Uneasy` · `SetFlag ng_arrived` |
| 6 | `beat_suspicious_pass` | `ClueCollected clue_ng_first` | `SetNpcDialogue Mother → DLG_Suspicious_Mother` · `SetNpcDialogue Uncle → DLG_Suspicious_Uncle` · `SetNpcDialogue Cousin → DLG_Suspicious_Cousin` · `SetNpcDialogue NotGrandma → DLG_Scary_NotGrandma` |
| 7 | `beat_intro_ends` | `DialogueFinished DLG_Scary_NotGrandma` | `HideActor npc_mother` · `HideActor npc_uncle` · `HideActor npc_cousin` · `PlayDialogue DLG_Monologue_WhatToDo` · `SetFlag intro_done` |

Beats 5 and 6 fire on the same event and are split only for readability — one asset carrying all the effects behaves identically. Two `PlayDialogue` effects queue FIFO, so the monologue lands after her line.

**Beat 7 is deliberately not gated on having met all three.** Gating it (`requiredFlags: met_mother, met_uncle, met_cousin`, each set by a `DialogueFinished` beat on the suspicious dialogues) is legal — those flags come from earlier events, so the no-cascade rule does not bite — but a player who clicks NotGrandma first would then never close the intro. Ungated is the safe jam choice.

### Main loop — the three unlocks

| # | Beat id | Trigger / match | Effects |
|---|---|---|---|
| 8 | `beat_find_mother` | `ClueCollected clue_about_mother` | `MoveActor npc_mother → <different room?>` · `SetNpcDialogue Mother → DLG_Mother_Worried` · `SetFlag mother_found` |
| 9 | `beat_find_uncle` | `ClueCollected clue_about_uncle` | `MoveActor npc_uncle → <different room?>` · `SetNpcDialogue Uncle → DLG_Uncle_Anxious` · `SetFlag uncle_found` |
| 10 | `beat_find_cousin` | `ClueCollected clue_about_cousin` | `MoveActor npc_cousin → <different room?>` · `SetNpcDialogue Cousin → DLG_Cousin_Scary` · `SetFlag cousin_found` |

`MoveActor` shows the copy whose `StoryActor._room` matches and hides every other copy of that id — so each of the three needs **at least two instances**: the intro room and the room they reappear in.

### Main loop — trades and the police

| # | Beat id | Trigger / match | Effects |
|---|---|---|---|
| 11 | `beat_cousin_evidence` | `ClueShared`, clue `clue_about_uncle` + `NPC_Cousin` | `PlayDialogue DLG_Cousin_Evidence` · `SetTension High` · `SetFlag has_evidence` |
| 12 | `beat_phone_appears` | `DayStarted`, day 2 | `ShowActor item_phone` |
| 13 | `beat_police_wrong` | `PoliceCallResolved = WrongEvidence` | `PlayDialogue DLG_Police_Wrong` · `SetTension Alert` |
| 14 | `beat_day3_tension` *(optional)* | `DayStarted`, day 3 | `SetTension Alert` |

The evidence clue itself is handed over by the Cousin's exchange table, not by beat 11 — the beat only reacts. Both beat 11 and beat 13 fire while a panel is still open; the binder's queue holds their dialogue until it closes, so **verify these two by playing**, not by reading the assets.

## 4. Scene placement

Per room, inside the matching room object of the `Game` prefab instance:

- [ ] **NPCs** — `Prefabs/Game/NPC`. `StoryActor._id` ∈ `npc_mother`, `npc_uncle`, `npc_cousin`, `npc_notgrandma`; `_room` = the room that copy sits in; `NpcInteractable._npc` = the matching `NpcSO`. One copy in the intro room (active), one in the reveal room (inactive). NotGrandma gets a copy in every room she can appear in, all inactive except where she first arrives.
- [ ] **Clue items** — `Prefabs/Game/Interactable Item`, one per clue plus the red herrings, each with `StoryActor._id` (`item_<name>`) and its `ClueSO` / `ItemSO`.
- [ ] **Phone** — one item copy per room, `StoryActor._id = item_phone`, `PhoneInteractable` pointed at `PoliceCallController`, **all starting inactive** (beat 12 turns every copy on at once).
- [ ] **Hiding spots** — `Prefabs/Game/Hiding Object`, one per room, `RoomId` set to that room. The diagram says "hiding place" for both the searchable stash and the night hiding spot; keep them as separate objects unless you deliberately want them to be the same prop.
- [ ] Drag **every** `StoryActor` into `StorySceneBinder._actors` and **every** `NpcInteractable` into `_npcs`. Inactive objects never self-register, so a missing entry is a beat that silently does nothing.
- [ ] Drag the beats from §3 into `_beats`, in order.
- [ ] Fix the 8 `roomController` references from §0.

## 5. Playthrough check (once per branch of the diagram)

- [ ] Arrows move between all four rooms; the starting room announces itself (ambience plays on load).
- [ ] Each intro dialogue fires once, in whatever order you roam, and never repeats.
- [ ] First clue → NotGrandma appears, her line then the monologue, in that order.
- [ ] Clicking her ends the intro and the other three vanish.
- [ ] Each character clue makes its owner reappear in the other room with the exchange dialogue.
- [ ] Trading with the Uncle burns exactly the room of the clue you gave him — hide there that night and lose with `HidInLeakedRoom`.
- [ ] Trading `clue_about_uncle` to the Cousin returns the evidence clue, and her reaction plays **after** the share panel closes.
- [ ] Let the clock run out without hiding → lose with `DayClockExpired`.
- [ ] Survive night 1 → phone appears on day 2; one call per day; wrong evidence twice → lose on trust; right evidence → win.
- [ ] Set `_secondsPerDay` to the shipping value and run the whole thing once at that speed.

## Out of scope

- Any player-facing text — every line referenced above is a human-written asset.
- Art, animation and the visual pass on the story props.
- Randomising which relative leaks: the Uncle is fixed (GDD).
