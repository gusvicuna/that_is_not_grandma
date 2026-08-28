# Day 2 Review — Locked Decisions, Open Gaps & Adjusted Plan

> Companion to `Docs/GDD.md`. Written Mon Aug 24. **Where this doc and the GDD disagree, this doc wins.**
> **Reality check:** 6 working days (Mon 24 → Sat 29). Close is Sun 30, 10:00.
> **Capacity:** 3 people × ~4.5 h/day × 6 days ≈ **80 person-hours total.**

---

## A. Decisions locked

| # | Decision |
|---|---|
| 1 | **Scope: 1 day + 1 night, 3 NPCs, 4 rooms.** One 8–12 min run. |
| 2 | **Delivery: WebGL first.** Windows build only if there's spare time on day 7. |
| 3 | **Final design call: Gus.** Ties get broken same day. |
| 4 | **Audio direction: quiet house + tension layers.** Near-silent ambience (clock, pipes, distant TV); music enters as layers when Not Grandma approaches or when someone lies to you. |
| 5 | **Writing split by character.** Gus: Not Grandma + item/clue texts. Janhavi: the Uncle (the traitor). Irene: Mother + Cousin. One shared tone guide in Discord before anyone writes a line. |
| 6 | **The features table is the authority**, not the core loop. GDD §4 mixes Must-Have and Nice-to-Have steps — see the MVP loop below. |
| 7 | **The Uncle is Not Grandma's ally. Fixed, never randomized.** All three dialogue sets are written against this. |

### Cuts that follow from decision 1

- Rooms: **4** — Kitchen, Living room, Bedroom, Bathroom. (Study and Garden cut. Bedroom holds the under-bed hiding spot.)
- Characters: **4** — Not Grandma, Mother, Uncle, Cousin. (Real Grandma appears only in the win screen, static, no walk cycle.)
- Hiding spots: **2** — under the bed, closet. (The shower stays cut even though the bathroom is back in — hiding spots are locked at 2.)
- Days: **1 day + 1 night.** No multi-day progression, no NPC behaviour changing across days.

#### Amendment — Tue Aug 25 (Gus)

**The Garden is out, the Bathroom is in.** The 4th room is the Bathroom, not the Garden. The room list above and every other doc (`Docs/GDD.md`, `CLAUDE.md`, `Docs/plans/01-*.md`) reflect the swap; `RoomId` in code matches.

Knock-on fix: the GDD §3 opening beat moved from the garden sink to the **kitchen** sink (Gus's call).

#### Amendment — Wed Aug 26 (Gus)

Decisions made while designing the story/dialogue systems against the story flowchart (`Docs/Not grandma's story line.drawio`):

1. **Task swap.** Gus takes the **dialogue system + story progression system**; Janhavi takes **room navigation + the police call flow**. This supersedes the Tue/Wed task tables below where they say otherwise.
2. **Story progression system is an explicit Must-Have.** The flowchart is implemented as game-state flags plus a list of story *beats* (trigger → conditions → effects) evaluated by a pure-C# `StoryDirector` in Domain. It was previously implicit in "movement of NPCs" and "time system"; without it there is no game sequence.
3. **The intro is free-roam with triggers**, not a forced sequence. Each intro beat (Mother/Cousin/Uncle intros, first clue, Not Grandma's arrival) fires the first time its condition is met while the player roams.
4. **Day clock is mixed:** a real-time base timer plus a time cost charged by significant actions (searching a hiding place, talking, trading a clue).
5. **Dialogue is homemade — no Yarn Spinner / Ink.** Evaluated both (Aug 26): integration + WebGL risk isn't worth it 3 days out, and the clue-for-clue exchange is bespoke gameplay UI either way. The system is minimal dialogue graphs in ScriptableObjects with **cosmetic branching**: dialogue options change which lines you hear, never game state. This promotes the "Dialogue options" Nice-to-Have in that limited form only. The seam stays clean enough to plug Ink in post-jam.

#### Amendment — Thu Aug 27 (Gus)

Exchange + NPC decisions, locked while planning `Docs/plans/03-npcs-clue-exchange.md` (the story director moves to plan 04):

1. **What an NPC returns is a per-NPC map** given-clue → returned-clue, plus an optional fallback for unmapped clues (the flowchart's "useless NotGrandma clue"). The Uncle's-clue → Cousin → final-evidence beat is a map entry, not code.
2. **Sharing keeps the clue in the notebook.** Information is shared, not spent; the cost is the leak. The same clue can go to several NPCs, but **only once per NPC** — NPCs remember (C1), so no reward farming and no double leak.
3. **Leak tracking (C1) lives in the exchange domain** (`ExchangeLog`): a clue shared with the leaker marks its room as leaked. The night patrol consumes `LeakedRooms` / the `RoomLeaked` channel — Janhavi's side never computes leaks.
4. **NPCs get identity assets** (`NpcSO`: id, display name, portrait slot, exchange table, leak flag). The flag is on for the Uncle only, fixed.
5. **The exchange opens as a share prompt when a conversation ends, and only after dialogues explicitly marked for it** (`DialogueSO._allowsClueExchange`, off by default). Dialogue graphs stay cosmetic; sharing is never a dialogue-node effect. Intros and story beats end with no prompt — which also keeps the trade from feeling available at moments the writing doesn't support.
6. **NPC visual identity lives in `NpcSO`** (second pass, same day): a representative colour per character (tints their dialogue name + lines; Not Grandma's is the §C4 one-colour-nobody-else-uses) and two sprite slots — dialogue portrait and in-room world sprite. **All 4 characters get an `NpcSO`**; Not Grandma's exchange table is empty, so she never offers a trade. Dialogue nodes now reference the speaker's `NpcSO` — the per-node speaker string is removed.

#### Amendment — Thu Aug 27 (Gus), audio pass

Locked while planning `Docs/plans/05-audio-music.md` (Tuesday's audio foundation and Thursday's music layers merged into a single plan):

1. **One audio plan, whole stack:** AudioMixer buses, WebGL first-click unlock, per-room ambience crossfade, three music layers, an SFX router and a persisted volume panel ship together.
2. **Tension reaches audio through `TensionChangedEventChannelSO(TensionLevel)`**, raised later by the night patrol (Janhavi) and the `StoryDirector` (plan 04). Audio never queries the patrol.
3. **`RoomChangedEventChannelSO(RoomId)` is defined by the audio plan** as the contract room navigation must raise. Ambience only listens.
4. **The lie music layer is never triggered by `RoomLeaked`.** The leak fires only for the Uncle, so an audible leak would hand the player the traitor on the first trade. The layer is pulsed by conversations explicitly marked `DialogueSO._playsLieMotif` — authored, deliberately ambiguous, and off by default.
5. **Music is stems that never stop:** all layers start together on unlock and only their volume moves. No `PlayScheduled`, no `dspTime` — WebGL sync risk removed.
6. **The volume settings panel is a standalone prefab** built now (GDD §8 pre-upload checklist) and re-parented into the pause menu when those screens exist.
7. **No audio middleware, and the reason is the platform** (`Docs/research/audio-tooling.md`): on Web the AudioMixer only changes volume — no snapshots, no DSP — and Wwise has no WebGL support at all. FMOD is the post-jam upgrade path, not a jam-week one. Two tools we *do* adopt: `ObjectPool<AudioSource>` and a timeboxed spike of Unity 6's Audio Random Container. Content consequence: a muffled "hiding under the bed" sound must be a recorded clip, not a lowpass filter.

---

## B. The MVP loop (Must-Have only — must exist by Wednesday)

```
Navigate room (click) → Interact with objects → Collect clue into notebook
   → Talk to an NPC, trade a clue for a clue
   → NIGHT: Not Grandma patrols, hide until morning
   → Accuse Not Grandma to the police with selected evidence
   → Right evidence = win · Wrong = lose police trust · Caught at night = lose
```

Everything in GDD §4 not on that line — clue combining, inventory items, favors, locking rooms, asking NPCs for help at night, accusing anyone other than Not Grandma — is **Nice-to-Have**.

---

## C. Open gaps

### C1. The theme needs a Must-Have consequence ⚠️ highest priority

The features table has information exchange, but **nothing that makes sharing with the wrong person hurt.** Without it the game is a point-and-click mystery with a traitor in the backstory — and Theme is one of the three categories we're betting on.

Cheapest version that makes the theme real, needing no trust system:

> One of the three NPCs is Not Grandma's ally. Every clue you give an NPC gets **remembered**. At nightfall, Not Grandma patrols with knowledge of the rooms whose clues reached the ally — she checks those rooms first and lingers there. The player is never told who the ally is; they infer it from where she hunts.

One boolean per NPC and one list of "leaked rooms" feeding the night patrol. **Promoted to Must-Have.**

### C2. Who is the ally — ✅ RESOLVED: the Uncle, fixed

- **The Cousin is loyal.** She warns you in the intro and stays honest — the player's only reliable read on the house, which makes the Mother the real question mark.
- **The Uncle needs a motive on screen** — a document about the house, a debt, an argument about the inheritance. One clue is enough; it's what makes the accusation feel earned instead of guessed.
- **He also needs to look helpful.** If the Uncle is obviously sinister, nobody trades clues with him and the theme never fires. He should be the most *useful* NPC to talk to: best clues, and he leaks every one you hand him.
- Randomizing the traitor stays Nice-to-Have.

### C3. Answers to previously-blank GDD fields

| Field | Answer |
|---|---|
| Voting bets | **Theme + Visuals + Audio** |
| Session length | 8–12 min |
| Police "lives" | 2 wrong accusations, third is a loss |
| Visual language of deception | See C4 |

### C4. Visual language of deception

Every character is rendered with the locked palette, but **Not Grandma uses one colour nobody else in the house uses** — subtle, never explained. The player registers it before they can articulate it. Same trick in audio: one instrument only she brings with her.

### C5. What winning looks like

**One static illustration of the real Grandma + 3 lines of text** is enough, and it's one asset. Losing needs its own screen too.

### C6. Verify the deadline in Chilean time ⚠️

The jam page shows Aug 30, 10:00 — confirm what that is in Chile, pin it in Discord. Aim to be finished **12 hours before**.

---

## D. Adjusted plan — 6 days

### Mon 24 — Navigable skeleton + first WebGL build uploaded

| Who | Task |
|---|---|
| **Gus** | Click-to-navigate between the 4 rooms with placeholder backgrounds, git hygiene (Unity `.gitignore`, LFS for art), **draft itch page + WebGL build uploaded tonight, however ugly** |
| **Janhavi** | Clickable items → clue data structure (ScriptableObject), notebook listing collected clues |
| **Irene** | Photo shoot at the grandparents' house: the 4 locked rooms + one under-bed and one closet angle. Confirm target resolution with Gus **before shooting**. |

> Uploading a WebGL build on day 2 is not optional. Unity WebGL fails in specific ways — build size, compression settings itch doesn't serve, audio blocked until first click — each a calm 30-minute fix today and a catastrophe on Saturday.

### Tue 25 — Dialogue and the exchange

| Who | Task |
|---|---|
| **Gus** | Audio system: bus structure, per-room ambience, volume control in menu, first-click audio unlock for WebGL. Rebuild and re-upload. |
| **Janhavi** | Dialogue system + dialogue trees + clue-for-clue exchange + the leak flag from C1 |
| **Irene** | Mother, Uncle and Cousin base poses. Everyone writes their assigned dialogue tonight. |

### Wed 26 — 🔴 Vertical slice: start → play → night → accusation → end

| Who | Task |
|---|---|
| **Gus** | Day→night transition, Start / Pause / Win / Lose screens, police accusation flow with evidence selection and 2 lives |
| **Janhavi** | Not Grandma's night patrol (room-to-room, biased by leaked rooms), 2 hiding spots, catch = lose |
| **Irene** | Notebook UI, dialogue UI, custom cursor states |

**Scope cut happens tonight.** If it isn't completable end to end, something on the Must-Have list gets deleted — not postponed. Deleting one room is the cheapest cut available.

### Thu 27 — Content and balance

| Who | Task |
|---|---|
| **Gus** | Music layers (approach layer, lie layer), the 5 critical SFX, mix pass |
| **Janhavi** | The real clue chain leading to the final evidence, night difficulty balance, wire in all written dialogue |
| **Irene** | Final backgrounds integrated, character polish |

### Fri 28 — Polish, external playtest, good build

| Who | Task |
|---|---|
| **All** | **Playtest with 2–3 people outside the team.** Watch, don't explain. Whatever they don't understand is Saturday's work. |
| **Gus** | Audio mix, juice, bug triage |
| **Janhavi** | Bugfixes from the playtest |
| **Irene** | Itch page: header art, 3 screenshots, animated GIF |

### Sat 29 — Final build, uploaded Saturday

Fix only what the playtest exposed. Upload the final build **Saturday night**. `CREDITS.md` complete.

### Sun 30 — Verify, then stop

Load the itch page in a browser you've never used. Play it start to finish. Touch nothing.

---

## E. Risks

| Risk | Owner | Mitigation | Deadline |
|---|---|---|---|
| Unity WebGL build fails or audio doesn't play in browser | Gus | Build and upload day 2 | Mon 24 |
| Dialogue volume balloons — 3 NPCs × trees is the biggest hidden cost | Janhavi | Cap at ~12 exchanges per NPC, written Tue night | Tue 25 |
| Photo backgrounds blow up the WebGL build size | Gus | Agree export resolution before the shoot, downscale to target, no source photos in the Unity project | Mon 24 |
| Reshoot needed because a room is missing or badly framed | Irene | Shoot all 4 rooms + both hiding spots in one trip | Mon 24 |
| Art doesn't land in time and the game ships grey-boxed | Irene | Placeholders Monday, edited photos integrated Thursday, never later | Thu 27 |
| Theme reads as decoration only | Gus | Ship C1 (leaked clues change the night) as Must-Have | Wed 26 |
| Fewer than 20 ratings → not eligible | All | WebGL build + each person rates 10 games during voting week | Aug 31 |
| Deadline misread across time zones | Gus | Confirm Chilean local time, pin it in Discord | Mon 24 |

---

## F. Nice-to-Have priority order (only if Wednesday goes clean)

1. **Binary trust** — NPCs remember whether you fed them or the ally, and change dialogue accordingly
2. **NPC favors** — activate something in exchange for a clue
3. **Accuse any NPC**, with the consequences written in GDD §4.5
4. **Clue combining**
5. Inventory items
6. Locking rooms at night
7. Randomized traitor between runs

Anything below line 3 is realistically a post-jam version — new content isn't allowed in the 48h bugfix window.
