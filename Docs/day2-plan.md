# Day 2 Review — Locked Decisions, Open Gaps & Adjusted Plan

> Companion to `Docs/GDD.md`. Written Mon Aug 24. **Where this doc and the GDD disagree, this doc wins.**
> **Reality check:** 6 working days (Mon 24 → Sat 29). Close is Sun 30, 10:00.
> **Capacity:** 3 people × ~4.5 h/day × 6 days ≈ **80 person-hours total.**

---

## A. Decisions locked

| # | Decision |
|---|---|
| 1 | **Scope: 1 day + 1 night, 3 NPCs, 4 rooms.** One 8–12 min run. — *superseded Aug 28: the run loops day → night → day until win or loss, ceiling 3 days. Everything else stands, including the 8–12 min target.* |
| 2 | **Delivery: WebGL first.** Windows build only if there's spare time on day 7. |
| 3 | **Final design call: Gus.** Ties get broken same day. |
| 4 | **Audio direction: quiet house + tension layers.** Near-silent ambience (clock, pipes, distant TV); music enters as layers when Not Grandma approaches or when someone lies to you. |
| 5 | **Writing split by character.** Gus: Not Grandma + item/clue texts. Janhavi: the Uncle (the traitor). Irene: Mother + Cousin. One shared tone guide in Discord before anyone writes a line. |
| 6 | **The features table is the authority**, not the core loop. GDD §4 mixes Must-Have and Nice-to-Have steps — see the MVP loop below. |
| 7 | **The Uncle is Not Grandma's ally. Fixed, never randomized.** All three dialogue sets are written against this. |

### Cuts that follow from decision 1

- Rooms: **4** — Kitchen, Living room, Bedroom, Bathroom. (Study and Garden cut. Bedroom holds the under-bed hiding spot.)
- Characters: **4** — Not Grandma, Mother, Uncle, Cousin. (Real Grandma appears only in the win screen, static, no walk cycle.)
- Hiding spots: ~~**2** — under the bed, closet~~ → **4, one per room** *(revised Aug 28)*. The night check is per-room, so two spots made it a coin flip.
- Days: ~~**1 day + 1 night**~~ → **a day/night loop, ceiling 3 days** *(revised Aug 28)*. Still no NPC behaviour changing across days — the only thing that carries over is the leaked-rooms set and the police calls spent.

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

#### Amendment — Fri Aug 28 (Gus): the night becomes a single check, and the run loops

**This is a scope cut wearing a design hat, and it's the right one two days from close.** Not Grandma's room-to-room night patrol is **cut**. What replaces it costs a fraction to build and keeps the theme's consequence intact:

1. **The night is one decision and one check.** The day runs on a clock; before it expires the player must be hiding in a hiding spot. Then:
   - didn't make it in time → **lose**;
   - hid in a room whose clues **leaked** to the Uncle → **lose** (she knew where to look);
   - otherwise → **survive to the next morning**.
   No patrol, no room-to-room AI, no night navigation. The leak stops biasing a patrol and simply *decides the night*, which makes C1 sharper, not weaker: the player learns who the traitor is by dying in the room they told him about.
2. **The run loops: day → hide → night → day…** until the player wins (right accusation), loses a night, or burns the third police call. This supersedes decision A1's "1 day + 1 night".
3. **The phone is an item, from the second morning on.** It appears in the house on the morning after the first night, is clickable all day long, and opens the police-call UI. **One call per day.** With 2 police lives, the natural ceiling is a 3-day run.
4. **Leaked rooms accumulate across the whole run**, never reset. `ExchangeLog` is unchanged — every clue handed to the Uncle burns its room as a hiding place *permanently*. Safe rooms run out as the days pass; that curve is the game's difficulty.
5. **Hiding spots go from 2 to 4 — one per room.** With only two, a night where both are leaked is an unavoidable death the player couldn't see coming. With one per room the player chooses among four rooms knowing exactly which ones they compromised. Cost: two more hiding-spot art pieces (Irene); the system is identical.
6. **Consequences for the schedule:** Janhavi's Wednesday patrol task is dead — that time goes to the police-call UI and the phone item. NPC movement between rooms is no longer needed for the night either.

#### Amendment — Fri Aug 28 (Gus), audio pass

Locked while planning `Docs/plans/05-audio-music.md` (Tuesday's audio foundation and Thursday's music layers merged into a single plan):

1. **One audio plan, whole stack:** AudioMixer buses, WebGL first-click unlock, per-room ambience crossfade, three music layers, an SFX router and a persisted volume panel ship together.
2. **Tension reaches audio through `TensionChangedEventChannelSO(TensionLevel)`**, raised later by the night patrol (Janhavi) and the `StoryDirector` (plan 04). Audio never queries the patrol.
3. **`RoomChangedEventChannelSO(RoomId)` is defined by the audio plan** as the contract room navigation must raise. Ambience only listens.
4. **The lie music layer is never triggered by `RoomLeaked`.** The leak fires only for the Uncle, so an audible leak would hand the player the traitor on the first trade. The layer is pulsed by conversations explicitly marked `DialogueSO._playsLieMotif` — authored, deliberately ambiguous, and off by default.
5. **Music is stems that never stop:** all layers start together on unlock and only their volume moves. No `PlayScheduled`, no `dspTime` — WebGL sync risk removed.
6. **The volume settings panel is a standalone prefab** built now (GDD §8 pre-upload checklist) and re-parented into the pause menu when those screens exist.
7. **No audio middleware, and the reason is the platform** (`Docs/research/audio-tooling.md`): on Web the AudioMixer only changes volume — no snapshots, no DSP — and Wwise has no WebGL support at all. FMOD is the post-jam upgrade path, not a jam-week one. The one tool we *do* adopt is `ObjectPool<AudioSource>`; Unity 6's Audio Random Container was a spike candidate, **dropped Aug 28 without running** — its WebGL support is undocumented and an ambiguous result costs more than the code it saves. Content consequence: a muffled "hiding under the bed" sound must be a recorded clip, not a lowpass filter.

#### Amendment — Fri Aug 28 (Gus), police call handed to Janhavi

Locked while planning `Docs/plans/06-police-call.md` (Janhavi implements it end to end, code **and** editor setup):

1. **The phone is a clickable item wired to `IInteractable`/`ClickRouter`**, not an NPC and not `OnMouseDown`. It shows itself only from the second morning and forwards the click to a `PoliceCallController`; every rule lives in a pure-C# `PoliceCase` (availability, one call per day, trust, resolution).
2. **The day number gets a channel before it gets a system: `DayStartedEventChannelSO(int)`.** Plan 06 defines it and ships a temporary `DebugDayAdvancer` button that raises it; when the `StoryDirector` (plan 04) lands, Gus points it at the same asset and the debug button dies. Nobody writes a second day counter.
3. **The end of the run gets two channels now:** `GameWonEventChannelSO` (void) and `GameLostEventChannelSO(LossReason)`, with `LossReason` covering `PoliceTrustLost`, `HidInLeakedRoom` and `DayClockExpired`. The night check will raise the same `GameLost` channel — the enum is the shared vocabulary.
4. **Correct evidence = any `ClueSO` with `IsEvidence` on.** The Domain only ever sees a `bool`; which clue is the evidence stays content.
5. **The police panel is a *copy* of `ClueSharePanelView`, not a generalization of it.** Duplicating ~80 lines of view code is cheaper than an abstraction two days from close, and it keeps Janhavi's branch out of Gus's exchange code. Merging them is a post-jam cleanup.
6. **The police intro reuses the dialogue system unchanged** — a `DialogueSO` spoken by an `NPC_Police` `NpcSO`, with `_allowsClueExchange` off. The controller opens the call panel on `DialogueFinished` only when the finished dialogue is its own.
7. **Police trust starts at 2**, serialized on the controller. The GDD §7 phrasing ("2 wrong accusations, the third is a loss") would be 3 — it is one number in the Inspector if Gus changes his mind.

#### Amendment — Fri Aug 28 (Gus), the story director

Locked while planning `Docs/plans/04-story-director.md`, after the tooling comparison in `Docs/research/story-director-tooling.md`:

1. **We build it. No Ink, no Yarn Spinner, no Fungus, no Unity Behavior, no Visual Scripting, no paid toolkit.** The reason is structural, not stubborn: every tool ships the trigger/condition plumbing (~80 lines for us) and none ships the effects (~500), which stay custom classes in the tool's idiom — untestable in EditMode, which is the one testing rule this project kept. Post-jam order: Ink or Yarn Spinner for story state, Adventure Creator evaluated before the *next* point-and-click.
2. **A story beat is `trigger → conditions → effects`, authored as one `StoryBeatSO` per beat.** Effects are data, so new story content never touches C#. Beats are **one-shot by default**, which is what makes the free-roam intro (Aug 26 amendment §3) behave.
3. **NPCs and items are pre-placed in every room where they can appear and toggled with `SetActive`.** "Move NPC" = hide here, show there. No runtime `Instantiate`, no prefab loading — and no WebGL risk in this feature.
4. **The day clock pauses while a dialogue or the exchange panel is open**, and charges a fixed cost when the action ends. This keeps the Aug 26 "mixed clock" decision while making reading the writing free.
5. **Clicking a hiding spot ends the day immediately.** Waiting out a timer inside a wardrobe is dead time in an 8–12 minute run.
6. **NPCs react after a trade, and the police answer back after a call.** A beat can trigger on "clue X shared with NPC Y" or on a resolved police call and play a dialogue in response — the second needs one new channel, `CH_PoliceCallResolved(PoliceCallOutcome)`, raised by one added line in Janhavi's `PoliceCallController`. Those dialogues are **queued and played once the panel closes**, never stacked on top of it: both moments raise their trigger while their panel is still on screen, and a dialogue requested while one is running is silently dropped.
7. **Hiding asks for confirmation** — a yes/no prompt before the day ends, so nobody loses a day without knowing why. Cancelling costs no time.
8. **Plan 04 owns `CH_DayStarted`**, so plan 06's `DebugDayAdvancer` is deleted when it merges. The night check `NightCheck.Survives(hidingRoom, leakedRooms)` lives here and raises plan 06's `CH_GameLost`. There is no hard day cap: the 3-day ceiling comes from the 2 police lives.

---

## B. The MVP loop (Must-Have only — must exist by Wednesday)

```
Navigate room (click) → Interact with objects → Collect clue into notebook
   → Talk to an NPC, trade a clue for a clue (the trade leaks that clue's room)
   → Beat the day clock into a hiding spot
   → NIGHT resolves: too late = lose · leaked room = lose · else survive
   → NEXT MORNING: the phone is available, one police call per day
   → Right evidence = win · Wrong = lose police trust · 3rd wrong = lose
```
*(revised Aug 28 — the patrol is gone; the night is a single check.)*

Everything in GDD §4 not on that line — clue combining, inventory items, favors, locking rooms, asking NPCs for help at night, accusing anyone other than Not Grandma — is **Nice-to-Have**.

---

## C. Open gaps

### C1. The theme needs a Must-Have consequence ⚠️ highest priority

The features table has information exchange, but **nothing that makes sharing with the wrong person hurt.** Without it the game is a point-and-click mystery with a traitor in the backstory — and Theme is one of the three categories we're betting on.

Cheapest version that makes the theme real, needing no trust system:

> One of the three NPCs is Not Grandma's ally. Every clue you give an NPC gets **remembered**. At nightfall, Not Grandma knows the rooms whose clues reached the ally — hide in one of them and she finds you. The player is never told who the ally is; they infer it from where they died.

One boolean per NPC and one list of "leaked rooms" resolving the night. **Promoted to Must-Have.**

*(revised Aug 28: the leak no longer biases a patrol, it decides the night outright. Same data, less code, sharper lesson — and it accumulates across days, so the safe rooms run out.)*

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
