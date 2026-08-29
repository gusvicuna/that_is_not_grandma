# 04 — Story director (beats + the day/night loop)

**Goal:** a pure-C# `StoryDirector` fires authored story beats (`trigger → conditions → effects`) that move, show, hide and re-script NPCs and items, and a day clock that runs the day → night → day loop until the run ends. **Priority:** Must-Have (day-2 plan, Aug 26 amendment §2: "Story progression system is an explicit Must-Have"; Aug 28 amendment: the night is one check and the run loops).

**Owner: Gus.** Code and editor setup. Tooling was researched first — see `Docs/research/story-director-tooling.md`; verdict: build it, ~14 files, no new dependency.

---

## Decisions this plan encodes (Gus, Aug 28)

1. **Beats are data, never code.** A `StoryBeatSO` per beat; adding story content never touches C#. The director's Domain half only decides *which* beats fire; Presentation applies their effects.
2. **NPCs and items are pre-placed in every room where they can appear and toggled with `SetActive`.** "Move NPC" = hide it in room A, show it in room B. No `Instantiate`, no prefabs loaded at runtime, no anchors — and no WebGL risk.
3. **Beats are one-shot by default** (`_repeatable` off). This is what makes the free-roam intro (Aug 26 amendment §3) behave: each intro beat fires the first time its condition is met, in whatever order the player roams.
4. **The day clock is mixed:** a real-time countdown *plus* time charged per significant action. **It pauses while a dialogue or the exchange panel is open** — reading is free, the action costs a fixed amount when it ends. Otherwise the clock punishes players for reading the writing the team spent two nights on.
5. **Clicking a hiding spot ends the day immediately** (`_hidingEndsDayImmediately`, on by default). Waiting out a timer inside a wardrobe is dead time in an 8–12 minute run.
6. **This plan owns `CH_DayStarted`.** When it lands, plan 06's `DebugDayAdvancer` is deleted and its `PoliceCallController` points at the same asset. Nobody writes a second day counter.
7. **The night is `NightCheck.Survives(...)`, a pure function of "where did you hide" and "which rooms leaked".** No patrol, no AI (Aug 28 amendment §1).
8. **Leaked rooms are accumulated from `CH_RoomLeaked`, never reset, and never queried from `ExchangeController`.** The exchange side already raises the channel; this side only listens.
9. **`PlayDialogue` effects are queued, never played on the spot.** `CH_ClueShared` is raised **while the exchange panel is still open**, and `PoliceCallController` deliberately keeps its panel open after `SubmitEvidence` so the player can read the result. Firing a dialogue at that moment would stack a conversation on top of an open panel — and `DialogueController.OnDialogueRequested` silently drops the request when a dialogue is already running, so the beat would vanish with no error. The binder holds the dialogue until nothing modal is open.
10. **Hiding asks for confirmation.** Clicking a hiding spot opens a small yes/no prompt; the day ends only on "yes". Decision 5 (hiding ends the day immediately) stands — the prompt is what makes it legible instead of a trap.

## Post-merge amendment (Gus, Aug 29)

Janhavi's branch landed with a parallel day/night implementation. Resolution:

1. **One system survives: this one.** Deleted `DayNight/DayNightCycle.cs`, `Presentation/Day/DayClock.cs`, and `Presentation/Night/Rooms/{HidingSpot,NightHiding,NightSurvivalChecker,Room}.cs`. Her `Presentation/Day/DayClock.cs` was a MonoBehaviour in the **global namespace**, and it broke the build: this plan's `using` directives sit outside the `namespace` block, so at global scope a declared type beats an imported one and `DayClock` resolved to her MonoBehaviour — seven compile errors, and Unity refusing to add *any* component until they cleared.
2. **Her names win.** `DayClockController` → **`DayNightCycle`**, `NightController` → **`NightSurvivalChecker`**, `HidingSpotInteractable` → **`HidingSpot`**. `Game.Domain.DayClock` keeps the name her MonoBehaviour used to hold, now as the pure-C# clock.
3. **`NightResultUI` is kept** — it is the only piece of that group with no equivalent here, and its three loss lines are already written by a human. It now listens to `CH_GameLost` instead of being called by a hiding spot.
4. **`GameWonEventChannelSO` is deleted; `CH_GameWon` becomes a `VoidEventChannelSO`.** Her class derived from `ScriptableObject` directly, so it had no description, no listener count and no inspector Raise button.
5. **`CH_PoliceCallResolved` is now raised** at the end of `PoliceCallController.SubmitEvidence` — the dependency below is satisfied.
6. **Final folder layout** (the plan's paths below predate the merge): the story rules live in `Domain/Story/`, but `DayClock.cs` and `NightCheck.cs` sit in `Domain/` next to `PoliceCase.cs`; `StoryActor`, `StorySceneBinder` and `StoryDirectorBehaviour` are in `Presentation/Story/`, while `DayNightCycle`, `NightSurvivalChecker` and `HidingSpot` are in `Presentation/DayNight/`.

## Cleanup pass (Gus, Aug 29)

Reviewed the whole feature after wiring. Three behaviour fixes and one hardening pass; the clock floor is the only Domain change, and it came with six tests (**62/62 green**, `PoliceCaseTests` included):

1. **The clock now stops for the share panel and the item popup, not just for dialogue.** Decision 4 says reading is free, but the code only paused on `CH_DialogueRequested` — and the dialogue pause is lifted precisely when the share panel *opens*, so the most reading-heavy screen in the game was the one running the clock. Both references are optional and null-guarded.
2. **The dialogue queue now waits on the police call panel too.** That was a `// when plan 06 merges` note; plan 06 has merged, and `PoliceCallController` deliberately keeps its panel open after the verdict, which is exactly when a `PoliceCallResolved` beat wants to speak.
3. **An action never leaves less than 5 seconds on the clock** (`_minimumSecondsAfterAction`, serialized). Losing the night in the same instant a conversation ends reads as the game cheating — the player never saw the clock, so the cost is invisible and the death feels arbitrary. Only real time passing can run the day out now. The floor never adds time back: acting with 2 seconds left leaves 2, it does not top up to 5.
4. **`Wiring.Require` in every `Awake`.** Subscriptions were null-guarded but raises were not, so an unassigned channel failed later, elsewhere, as a bare `NullReferenceException`. Now the scene load says which field on which object is empty. `StoryDirectorBehaviour` disables itself if it has no binder rather than running half-built.

## Clock and night UI (Gus, Aug 29)

1. **The clock reads as an hour, not a bar.** `TimeOfDay` (Domain, tested) maps day progress onto the hours the house keeps — 8 AM at dawn, 8 PM at nightfall by default — with **minutes rounded down to 5**. A 180-second day covers 12 hours, so one displayed minute lasts a quarter of a second; without the step the digits are a blur, and exact minutes would rebuild the TMP mesh four times a second on Web. `DayClockView` only writes the text when the reading actually changes.
2. **The hour is computed in `double`, and the tests never sit on a boundary.** The first version multiplied the day's 720 minutes by a `float` progress and truncated. C# allows a float expression to be evaluated with more precision than float, so at exactly 10/12 of the day `720f * progress` lands on 599.99998 under one runtime and on 600 under another: .NET said 6:00 PM, Unity's Mono said 5:55 PM, and IL2CPP on Web is a third opinion. A domain rule that answers differently per platform is not a rule. The arithmetic is now `double`, and the 6 PM tests straddle the boundary by half a minute instead of sitting on it.
3. **An optional second label shows the day number.** Same component, its own `TMP_Text`, and it is written only when the number changes — three times in a whole run — because `string.Format` allocates and `Refresh` runs every frame. **It stays hidden until day 2** (`_firstVisibleDay`, serialized): a counter reading "Day 1" announces that there will be a day 2 before the player has survived a single night, and the run looping is something they should discover by living through it. The same comparison covers the frames before the first morning, when `CurrentDay` is still 0. The format string is serialized with `[DAY] {0}` as the placeholder.
4. **The clock changes colour when the evening arrives** — two serialized colours and its own serialized hour, deliberately separate from the tension hour so the visual warning and the musical one can be tuned apart. Like the text, the colour is only assigned when the state actually flips, and the flag is nullable so a new morning repaints back to normal with no special case. The warning colour in the file is a placeholder: it must come from the locked palette, and the GDD reserves one colour for Not Grandma that nothing else may use.
5. **6 PM pulls the tension up**, once per day, re-armed every `CH_DayStarted`. Hour and level are serialized; it raises the existing `CH_TensionChanged`, which audio already listens to (plan 05). No new channel, no new trigger.
6. **The night is a sequence, and it owns the pacing of the loop.** `NightSequenceView` fades to black, holds a night line, then either shows the loss or holds a next-morning line and fades back in. **`DayNightCycle` no longer starts the next day when the night resolves** — it waits for `CH_NightSequenceFinished`, so the player never spends daylight behind a black screen. If that channel is left unwired the old behaviour returns (morning starts immediately), which keeps a scene without the sequence playable instead of hanging after the first night.
7. **The outcome is read late, on purpose, and the loss waits a frame.** The whole night resolves synchronously inside `CH_NightStarted`, and the order in which `NightSurvivalChecker` and the view receive that event is not ours to choose. Three consequences, all found the hard way in play mode:
   - The view stores what arrives and only reads it after the first fade, when the chain is certainly over. If nothing resolved the night it logs an error and continues as survived rather than freezing the run.
   - It must **not** clear the stored outcome when the night starts: if the checker subscribed first, that answer is already in hand and clearing throws it away.
   - `CH_GameLost` can arrive *before* the night sequence has been asked to start, which made the view read it as a police loss and skip straight from the fade to the result, with no night message. So a loss with no sequence running waits **one frame** before claiming it: if a night took over in the meantime, the night owns the result.
8. **`NightResultUI` became `GameEndView`, and it ends every run.** Winning used to show nothing at all — `PoliceCallController` raised `CH_GameWon` into a channel with zero listeners. `NightSequenceView` now listens to it too and fades to black before the panel, so victory and defeat are presented the same way. The view's API is `ShowWin()` / `ShowLoss(reason)`: the old `ShowResult(bool survived, …)` took a flag whose `true` branch did nothing. Its own `CH_GameLost` field is **deleted**, not just left empty — a field nobody can wire is the only fix that survives the next person who sees an empty channel slot and fills it. **All five lines are serialized fields**, editable in the inspector without a recompile: the win is a `[WIN_TEXT]` placeholder, and the four loss lines default to exactly what Janhavi wrote. Because Unity fills a newly added field from its C# default, the components already in `UI.prefab` and `greyboxnav` pick her text up too — nothing to re-enter. The consequence to remember: from now on those lines live in the prefab, so **`UI.prefab` is a content file** and a careless prefab revert loses writing.
9. **`NightSequenceView` owns `CH_GameLost` and `CH_GameWon`; `GameEndView` only draws.** A loss with no night behind it — the police running out of patience — fades to black the same way before showing the result. The field that caused that is now gone from `GameEndView` entirely.

## Navigation merge (Gus, Aug 30)

Janhavi's room navigation landed on `dev`. What it broke and how it was resolved:

1. **`CH_RoomChanged` had been re-pointed to a second channel class** (`RoomChangedEventChannelSO`, payload `int`, deriving from `ScriptableObject` rather than `EventChannelSO`). Unity silently drops a reference whose type no longer matches, so `AmbienceController`, `AudioCueRouter`, `NightSurvivalChecker` and `StoryDirectorBehaviour` all lost theirs at once: no ambience on room change, no room SFX, leaving a room no longer left the hiding spot, and `RoomEntered` beats could never fire. The duplicate class is deleted and the asset goes back to `RoomIdEventChannelSO`.
2. **The payload is a `RoomId`, never an array index.** The navigation raised `currentRoom`, the index into its inspector array. Even with the type fixed, an index only matches the enum while somebody keeps the array in the same order — and the failure is silent and awful: the room you burned in the Kitchen would kill you in the Bathroom. `RoomController` now carries a `RoomId[]` parallel to `rooms[]` and logs an error on `Awake` if the two do not line up.
3. **The starting room announces itself.** `ShowRoom` raises the channel, so `Start` covers the first room too — otherwise its ambience never begins and a beat waiting on it can never fire.
4. **The navigation arrows go through `ClickRouter`.** They used `OnMouseDown`, a physics callback that ignores every guard the router applies. A `CanvasGroup` blocks uGUI raycasts but not that, so the player could walk between rooms mid-conversation, with the share panel open, and behind the night sequence's black screen.
5. Two dead channel classes from that branch (`DialogueRequestedEventChannelSO`, `DialogueFinishedEventChannelSO`) referenced by nothing were deleted with it.

## Dependencies on other plans

| Needs | From | If it isn't on `dev` yet |
|---|---|---|
| `LossReason` enum, `GameLostEventChannelSO`, `CH_GameLost` | plan 06 (Janhavi) | Create them **exactly** as plan 06 §Domain/§Events specifies — same file names, same values. Do not invent a second enum. |
| `CH_RoomChanged` raised on room entry | room navigation (Janhavi) | **Satisfied Aug 30** — `RoomController` raises it with a `RoomId`. See the navigation merge section above. |
| `DialogueSO.Id` is `private` with no getter | plan 02 (this repo) | One-line patch: `public string Id => _id;` |
| `NpcInteractable._npc` has no getter | plan 03 (this repo) | One-line patch: `public NpcSO Npc => _npc;` plus `public void SetDialogue(DialogueSO dialogue)`. |
| `CH_PoliceCallResolved` raised after a call | plan 06 (Janhavi) | **One line at the end of `PoliceCallController.SubmitEvidence`:** `_policeCallResolved.Raise(outcome);` plus the serialized field. Everything else — the channel class, the asset, the beats — is created here. Without it, only the exchange reaction works; nothing breaks. |

---

## Domain

Pure C#, **no `using UnityEngine;`**, namespace `Game.Domain`, folder `Assets/Game/Scripts/Domain/Story/`.

### `StoryTrigger.cs`

```csharp
public enum StoryTrigger
{
    ClueCollected,      // CH_ClueCollected
    ItemInspected,      // CH_ItemInspected
    DialogueFinished,   // CH_DialogueFinished
    ClueShared,         // CH_ClueShared  (clue + npc)
    RoomEntered,        // CH_RoomChanged
    DayStarted,         // CH_DayStarted
    PoliceCallResolved  // CH_PoliceCallResolved
}
```

### `StoryEvent.cs` — readonly struct

`StoryEvent(StoryTrigger trigger, string primaryId = null, string secondaryId = null, int number = 0)`.

How Presentation fills it, per channel:

| Channel | Trigger | `PrimaryId` | `SecondaryId` | `Number` |
|---|---|---|---|---|
| `CH_ClueCollected` (`ClueSO`) | `ClueCollected` | `clue.Id` | — | — |
| `CH_ItemInspected` (`ItemSO`) | `ItemInspected` | `item.Id` | — | — |
| `CH_DialogueFinished` (`DialogueSO`) | `DialogueFinished` | `dialogue.Id` | — | — |
| `CH_ClueShared` (`NpcSO`, `ClueSO`) | `ClueShared` | `clue.Id` | `npc.Id` | — |
| `CH_RoomChanged` (`RoomId`) | `RoomEntered` | — | — | `(int)room` |
| `CH_DayStarted` (`int`) | `DayStarted` | — | — | `day` |
| `CH_PoliceCallResolved` (`PoliceCallOutcome`) | `PoliceCallResolved` | — | — | `(int)outcome` |

### `StoryCondition.cs` — immutable

`StoryCondition(IReadOnlyList<string> requiredFlags = null, IReadOnlyList<string> forbiddenFlags = null, int minDay = 0)` — null lists are stored as empty ones.

- `IReadOnlyList<string> RequiredFlags` — all must be set.
- `IReadOnlyList<string> ForbiddenFlags` — none may be set.
- `int MinDay` — `0` = any day.
- `static readonly StoryCondition Always`.

### `StoryBeat.cs` — immutable

```csharp
public StoryBeat(
    string id,
    StoryTrigger trigger,
    string matchPrimaryId = null,
    string matchSecondaryId = null,
    int matchNumber = -1,
    StoryCondition condition = null,
    bool repeatable = false)
```

| Member | Contract |
|---|---|
| `string Id` | Unique within the director. Throws `ArgumentException` if null/empty. |
| `StoryTrigger Trigger` | — |
| `string MatchPrimaryId` / `string MatchSecondaryId` | Null or empty = **wildcard** (any clue / any NPC). |
| `int MatchNumber` | `-1` = wildcard. Used by `RoomEntered` (the `RoomId` as int), `DayStarted` (the day) and `PoliceCallResolved` (the `PoliceCallOutcome` as int). |
| `StoryCondition Condition` | Never null; defaults to `Always`. |
| `bool Repeatable` | Default `false` — the beat fires at most once per run. |

### `StoryDirector.cs`

| Member | Contract |
|---|---|
| `StoryDirector(IEnumerable<StoryBeat> beats)` | Throws `ArgumentNullException` on null, `ArgumentException` on a duplicate beat id. Order is preserved. |
| `int CurrentDay` | `0` until the first `DayStarted` event. |
| `bool HasFlag(string flag)` / `void SetFlag(string flag)` | The flag set is the director's only mutable story state. |
| `bool HasFired(string beatId)` | — |
| `IReadOnlyList<string> Notify(StoryEvent evt)` | Returns the ids of the beats that fired, **in declaration order**. A `DayStarted` event updates `CurrentDay` *before* matching, so a beat with `MinDay = 2` fires on `DayStarted(2)`. |

Matching rule — a beat fires when **all** hold: same trigger · each non-wildcard match field equals the event's · `CurrentDay >= Condition.MinDay` · every required flag set · no forbidden flag set · `Repeatable || !HasFired`.

**No cascade within one event.** Flags set by a beat's effects are applied by Presentation *after* `Notify` returns, so a beat can never trigger another beat in the same call. That is deliberate: it keeps the order of the returned list the only thing to reason about, and it makes the tests read straight down.

### `DayClock.cs`

| Member | Contract |
|---|---|
| `DayClock(float secondsPerDay, float minimumAfterSpend = 0)` | Throws `ArgumentOutOfRangeException` if `secondsPerDay <= 0`, if `minimumAfterSpend` is negative, or if it is not below the length of the day (which would make every action free). Starts full. |
| `float SecondsPerDay` / `float Remaining` / `float NormalizedRemaining` | `Remaining` never goes below `0`. `NormalizedRemaining` drives the UI bar. |
| `bool IsExpired` | `Remaining <= 0`. |
| `void Tick(float deltaSeconds)` | Throws `ArgumentOutOfRangeException` on a negative delta. Clamps at 0. |
| `void Spend(float cost)` | The per-action charge. **Never leaves less than `minimumAfterSpend` on the clock, and never adds time either**: an action taken with less than the floor left costs nothing rather than handing seconds back. So an action can never end the day — only `Tick` reaches zero. |
| `void ResetForNewDay()` | Back to `SecondsPerDay`. |

### `NightCheck.cs` — static, the whole night in one function

```csharp
public static bool Survives(RoomId? hidingRoom, IReadOnlyCollection<RoomId> leakedRooms, out LossReason reason)
```

- not hiding anywhere → `false`, `LossReason.DayClockExpired`
- hiding in a room in `leakedRooms` → `false`, `LossReason.HidInLeakedRoom`
- otherwise → `true`, and **`reason` is meaningless — callers must not read it.** C# forces the `out` to be assigned on every path, so it gets `default`, which is `LossReason.PoliceTrustLost` (value 0): a valid-looking, completely wrong answer for a night the player survived. This is `TryParse`'s contract with the polarity flipped, which is exactly the kind of thing that gets misread under deadline. Keep it honest in code, not in a comment: `NightSurvivalChecker` reads `reason` only inside the failure branch, and every other call site passes `out _`.
- a `null` `leakedRooms` is treated as empty, never as an exception: "no room has leaked yet" is the state of every run before the first trade, and a night must not throw.

## Events

**Two new assets, one new channel class.** `CH_PlayerHid` needs no class at all — it is a `RoomIdEventChannelSO`, because the class is chosen by payload and the asset by signal (`CLAUDE.md`). `CH_PoliceCallResolved` carries plan 06's `PoliceCallOutcome`, which no channel covers yet, so it gets the usual one-line subclass.

```csharp
// Events/PoliceCallOutcomeEventChannelSO.cs
[CreateAssetMenu(fileName = "CH_PoliceCallOutcome", menuName = "Game/Events/Police Call Outcome")]
public class PoliceCallOutcomeEventChannelSO : EventChannelSO<PoliceCallOutcome> { }
```

| Channel asset | Class | Raised by | Listened by |
|---|---|---|---|
| `CH_PlayerHid` *(new asset)* | `RoomIdEventChannelSO` | `HidingSpot` | `NightSurvivalChecker` |
| `CH_PoliceCallResolved` *(new asset + class)* | `PoliceCallOutcomeEventChannelSO` | `PoliceCallController` (plan 06 — one added line) | `StoryDirectorBehaviour` |
| `CH_DayStarted` *(exists)* | `IntEventChannelSO` | `DayNightCycle` | `PoliceCallController` (plan 06), `StoryDirectorBehaviour` |
| `CH_NightStarted` *(exists)* | `VoidEventChannelSO` | `DayNightCycle` | `NightSurvivalChecker`, audio |
| `CH_NightResolved` *(exists)* | `BoolEventChannelSO` (survived) | `NightSurvivalChecker` | `DayNightCycle` (starts the next morning), audio, UI |
| `CH_GameLost` *(plan 06)* | `GameLostEventChannelSO` | `NightSurvivalChecker` | `GameEndView` (plan 06) |
| `CH_RoomLeaked` *(exists)* | `RoomIdEventChannelSO` | `ExchangeController` | `NightSurvivalChecker` |
| `CH_TensionChanged` *(exists)* | `TensionLevelEventChannelSO` | `StorySceneBinder` (`SetTension` effect) | audio (plan 05) |
| `CH_DialogueRequested` *(exists)* | `DialogueEventChannelSO` | `StorySceneBinder` (`PlayDialogue` effect) | `DialogueController` |

The remaining triggers (`CH_ClueCollected`, `CH_ItemInspected`, `CH_DialogueFinished`, `CH_ClueShared`, `CH_RoomChanged`, `CH_PoliceCallResolved`) are **listened to only** by `StoryDirectorBehaviour`. This feature raises nothing on them.

The day clock does **not** get a channel — a UI bar reading it 60 times a second through a ScriptableObject is churn. `DayNightCycle` exposes `event Action OnClockChanged` and `float NormalizedRemaining`, the same shape `DialogueController.OnNodeChanged` already uses.

## Data

Namespace `Game.Data`, folder `Assets/Game/Scripts/Data/`.

### `StoryEffectKind.cs`

```csharp
public enum StoryEffectKind
{
    ShowActor,        // actor id
    HideActor,        // actor id
    MoveActor,        // actor id + room
    SetNpcDialogue,   // npc + dialogue: what the NPC plays on the next click
    PlayDialogue,     // dialogue: raise CH_DialogueRequested now
    SetTension,       // tension level: raise CH_TensionChanged
    SetFlag           // flag name: story state for later conditions
}
```

### `StoryEffectData.cs` — `[Serializable]`, not a ScriptableObject

`_kind`, `_actorId` (string), `_room` (`RoomId`), `_npc` (`NpcSO`), `_dialogue` (`DialogueSO`), `_tension` (`TensionLevel`), `_flag` (string). Only the fields its kind needs are read; the rest stay empty in the inspector.

### `StoryConditionData.cs` — `[Serializable]`

`_requiredFlags` (string[]), `_forbiddenFlags` (string[]), `_minDay` (int). `ToCondition()` builds the Domain type.

### `StoryBeatSO.cs` — `[CreateAssetMenu(menuName = "Game/Story/Beat")]`

`_id`, `_trigger`, the match fields as **asset references, not strings** — `_matchClue` (`ClueSO`), `_matchItem` (`ItemSO`), `_matchDialogue` (`DialogueSO`), `_matchNpc` (`NpcSO`), `_matchRoom` (`RoomId`) + `_matchAnyRoom` (bool), `_matchDay` (int, 0 = any), `_matchOutcome` (`PoliceCallOutcome`) + `_matchAnyOutcome` (bool) — plus `_repeatable`, `_condition`, `_effects` (`StoryEffectData[]`).

`StoryBeat ToBeat()` resolves those references into the ids the Domain matches on, and picks which field feeds `MatchNumber` from the trigger: room for `RoomEntered`, day for `DayStarted`, outcome for `PoliceCallResolved`, `-1` for the rest. Leaving a reference empty means "any". Dragging assets instead of typing ids is the whole reason a renamed clue can't silently break the story.

## Presentation

Namespace `Game.Presentation`, folder `Assets/Game/Scripts/Presentation/Story/`.

### `StoryDirectorBehaviour`

Owns the `StoryDirector`. Serializes `StoryBeatSO[] _beats` (declaration order = evaluation order), the six trigger channels, and a `StorySceneBinder`. Builds the director in `Awake` from `_beats.Select(b => b.ToBeat())`; subscribes to all six channels in `OnEnable`, unsubscribes in `OnDisable`. Each handler builds a `StoryEvent`, calls `Notify`, and hands every fired beat's `StoryBeatSO` to the binder.

### `StorySceneBinder`

The only place that turns effects into scene changes. Serializes `StoryActor[] _actors`, `NpcInteractable[] _npcs`, and the `CH_DialogueRequested` / `CH_TensionChanged` channels.

- `ShowActor` / `HideActor` — `SetActive` on every actor with that id.
- `MoveActor` — hide every actor with that id, show the one whose `Room` matches.
- `SetNpcDialogue` — `SetDialogue` on every `NpcInteractable` whose `Npc` is the target (an NPC exists once per room they can appear in).
- `PlayDialogue` — **queued, not raised now** (see below).
- `SetTension` — raise the channel.
- `SetFlag` — `_director.SetFlag(...)`.

#### The deferred dialogue queue

This is what makes "the NPC reacts after a trade" and "the police answer back after a call" work. Both moments raise their trigger channel **while a panel is still on screen**: `ExchangeController.Share()` raises `CH_ClueShared` before the player closes the share panel, and `PoliceCallController.SubmitEvidence()` keeps its panel open on purpose so the result can be read. Raising `CH_DialogueRequested` there would draw a conversation over an open panel, and if a dialogue happened to be running, `DialogueController.OnDialogueRequested` returns early and the beat's dialogue is lost with no error at all.

So the binder serializes `DialogueController` and `ExchangeController` (`ClickRouter` already sets that precedent — Presentation may reference Presentation), keeps a `Queue<DialogueSO>`, and in `Update` raises the next one only when `!IsDialogueActive && !IsExchangeActive`. Two rules keep it honest: the queue is FIFO so two beats firing on the same event stay in declaration order, and it is **not** cleared between days — a dialogue that was queued gets played.

If plan 06's panel must gate it too, add `PoliceCallController` as a third serialized reference and check its public `IsCallPanelActive`. Leave it unwired until that plan merges.

**The arrays are wired by hand in the inspector, not discovered.** Actors inside rooms that `RoomController` deactivates never run `Awake`, so self-registration would silently miss exactly the objects this feature exists to move. Inspector references to inactive objects serialize fine.

### `StoryActor`

`[SerializeField] string _id;` + `[SerializeField] RoomId _room;`, public getters, `void SetVisible(bool)` → `gameObject.SetActive(...)`. One per NPC/item *instance*; the same `_id` appears once per room the actor can be in.

### `DayNightCycle`

Owns the `DayClock` and the loop. Serialized: `_secondsPerDay` (default `180`), `_talkCost` / `_shareCost` / `_clueCost` (default `8` / `12` / `5`), `_pauseWhileDialogueOpen` (default on), `_startDayOnStart`.

- `Start` → day 1: `ResetForNewDay()`, raise `CH_DayStarted(1)`.
- `Update` → `Tick(Time.deltaTime)` unless paused; on `IsExpired`, raise `CH_NightStarted` once and stop ticking.
- Charges: `CH_DialogueFinished` → `_talkCost`, `CH_ClueShared` → `_shareCost`, `CH_ClueCollected` → `_clueCost`.
- Pauses on `CH_DialogueRequested`, resumes on `CH_DialogueFinished` (after charging).
- `CH_NightResolved(true)` → `_day++`, `ResetForNewDay()`, raise `CH_DayStarted(_day)`. `false` → stays stopped; plan 06's end screen takes over.
- `EndDayNow()` — public, called by the hiding spot.

All five numbers are balance knobs, Gus's to tune in the inspector during Saturday's pass. Nothing hardcodes them.

### `NightSurvivalChecker`

Serialized: `CH_NightStarted`, `CH_PlayerHid`, `CH_RoomChanged`, `CH_RoomLeaked`, `CH_NightResolved`, `CH_GameLost`.

Keeps `RoomId? _hidingRoom` (set by `CH_PlayerHid`, **cleared on `CH_RoomChanged`** — leaving the room means leaving the hiding spot) and a `HashSet<RoomId> _leakedRooms` fed by `CH_RoomLeaked` and never cleared. On `CH_NightStarted`: call `NightCheck.Survives`, raise `CH_NightResolved(survived)`, and on failure also `CH_GameLost(reason)`.

### `HidingSpot`

`IInteractable` on a `Collider2D`, one per room, clicked through the existing `ClickRouter`. Serializes its `RoomId`, `CH_PlayerHid`, the `DayNightCycle` and the `HideConfirmView`. On `Interact()` it **only asks**: `_hideConfirmView.Ask(this)`. Nothing else happens until the player answers.

- Confirmed → raise `CH_PlayerHid(room)` and, when `_hidingEndsDayImmediately`, call `EndDayNow()`.
- Cancelled → nothing at all. No time charged: opening a prompt is not an action.

### `HideConfirmView`

A small uGUI panel, hidden by default: one placeholder line and two buttons. `Ask(HidingSpot spot)` stores the caller and shows the panel; the buttons call `Confirm()` / `Cancel()`, which hide it and call back. `bool IsOpen` is public.

**`ClickRouter` needs one guard added**, next to the `IsExchangeActive` one it already has: while `HideConfirmView.IsOpen`, world clicks are ignored. Without it the raycast underneath keeps firing and the player can collect a clue through the prompt.

The prompt text is a **human-written placeholder** (`"[HIDE_CONFIRM_PROMPT]"`, `"[YES]"`, `"[NO]"`) — jam rule, no AI-written player-facing text. Gus writes the real line.

## Editor setup checklist

1. `Assets/Game/ScriptableObjects/Channels/` → **Create ▸ Game ▸ Events ▸ Room Id**, name it `CH_PlayerHid`, fill `_description` ("The player entered a hiding spot in this room. Raised by HidingSpot."). Then **Create ▸ Game ▸ Events ▸ Police Call Outcome** → `CH_PoliceCallResolved` ("A police call was answered. Raised by PoliceCallController once per call, whatever the outcome.").
2. New folder `Assets/Game/ScriptableObjects/Story/`. Create one beat per row via **Create ▸ Game ▸ Story ▸ Beat** (ids below are scaffolding — the real list is the `.drawio` flowchart, Gus's call):

   | Beat id | Trigger | Match | Effects |
   |---|---|---|---|
   | `beat_intro_cousin` | `RoomEntered` | Living room | `PlayDialogue` cousin intro · `SetFlag met_cousin` |
   | `beat_intro_uncle` | `DialogueFinished` | cousin intro | `ShowActor npc_uncle` (Living room) |
   | `beat_grandma_arrives` | `ClueCollected` | first kitchen clue | `MoveActor npc_notgrandma` → Kitchen · `SetTension Uneasy` |
   | `beat_phone_appears` | `DayStarted` | day 2 | `ShowActor item_phone` |
   | `beat_uncle_second_talk` | `ClueShared` | any clue + `NPC_Uncle` | `SetNpcDialogue` uncle → second dialogue |
   | `beat_uncle_reacts_to_trade` | `ClueShared` | any clue + `NPC_Uncle` | `PlayDialogue` uncle's reaction (queued until the share panel closes) |
   | `beat_police_wrong_call` | `PoliceCallResolved` | `WrongEvidence` | `PlayDialogue` police reaction · `SetTension Alert` |

3. Tag the scene: add `StoryActor` to every NPC and story item instance, one per room, `_id` shared across a character's copies (`npc_uncle`), `_room` set to the room it sits in. The phone from plan 06 gets `item_phone` and starts **inactive**.
4. Add four `HidingSpot` objects, one per room, each with a `Collider2D` and its `RoomId`.
5. Build the `HideConfirmPanel` prefab (`Assets/Game/Prefabs/UI/`) on the existing canvas: one TMP line + two buttons. **`HideConfirmView` goes on a wrapper object that stays active, and its `_panel` field points at the child panel, which starts disabled** — a component on a disabled object never runs `Awake`, so putting the view on the panel itself would leave the buttons unwired forever. Wire the two buttons into the view, and the view into all four hiding spots and `ClickRouter`'s new guard.
6. Empty GameObject `StoryDirector` in the scene holding `StoryDirectorBehaviour`, `StorySceneBinder`, `DayNightCycle`, `NightSurvivalChecker`. Wire every channel asset and drag the beat assets, the actors and the NPC interactables into their arrays — plus the `DialogueController` and `ExchangeController` references the binder's queue needs.
7. When plan 06 merges: point its `PoliceCallController` at `CH_DayStarted`, **add the `CH_PoliceCallResolved` raise** at the end of `SubmitEvidence`, and **delete `DebugDayAdvancer`** and its scene object.

## Tests

`Assets/Tests/Editor/`, EditMode, domain only. **Written and green** — Aug 29: the Domain is implemented and all 39 cases pass. They were run outside Unity (`dotnet test` over the pure-C# Domain in a scratch project) because the editor was open and holding the project lock; re-run them in Test Runner ▸ EditMode to confirm inside Unity.

**`StoryDirectorTests.cs`** — `Notify_MatchingTriggerAndId_FiresBeat` · `Notify_DifferentTrigger_DoesNotFire` · `Notify_WildcardPrimaryId_FiresForAnyPayload` · `Notify_ClueSharedWithOtherNpc_DoesNotFire` · `Notify_BeatAlreadyFired_DoesNotFireAgain` · `Notify_RepeatableBeat_FiresEveryTime` · `Notify_MissingRequiredFlag_DoesNotFire` · `Notify_ForbiddenFlagSet_DoesNotFire` · `Notify_BelowMinDay_DoesNotFire` · `Notify_DayStartedAtMinDay_FiresOnTheSameEvent` · `Notify_SeveralMatchingBeats_FiresInDeclarationOrder` · `SetFlag_ThenNotify_FiresGatedBeat` · `Ctor_DuplicateBeatIds_Throws` · `Ctor_NullBeats_Throws` · `Notify_PoliceCallResolvedWithMatchingOutcome_FiresBeat` · `Notify_PoliceCallResolvedWithOtherOutcome_DoesNotFire` · `Notify_DifferentPrimaryId_DoesNotFire` · `Notify_ClueSharedWithMatchingNpc_FiresBeat` · `Notify_DayStarted_UpdatesCurrentDay` · `Notify_RoomEnteredMatchingRoom_FiresBeat` · `Notify_RoomEnteredOtherRoom_DoesNotFire` · `Notify_NoMatchingBeat_ReturnsEmpty` · `HasFired_BeatNeverTriggered_IsFalse` **(23)**

**`DayClockTests.cs`** — `Tick_ReducesRemaining` · `Spend_ReducesRemaining` · `Tick_PastZero_ClampsToZeroAndExpires` · `ResetForNewDay_RestoresFullDay` · `Ctor_NonPositiveSecondsPerDay_Throws` · `Tick_NegativeDelta_Throws` · `NormalizedRemaining_HalfSpent_IsHalf` · `Ctor_NewClock_StartsFull` · `Spend_NegativeCost_Throws` · `Spend_PastZero_ClampsToZeroAndExpires` · `Ctor_NegativeMinimumAfterSpend_Throws` · `Ctor_MinimumAfterSpendNotBelowTheDay_Throws` · `Spend_WouldDropBelowFloor_StopsAtTheFloor` · `Spend_AlreadyBelowFloor_GivesNoTimeBack` · `Spend_WellAboveFloor_ChargesTheFullCost` · `Tick_WithAFloorSet_StillReachesZero` **(16)**

**`NightCheckTests.cs`** — `Survives_HiddenInSafeRoom_True` · `Survives_NotHidden_FalseWithDayClockExpired` · `Survives_HiddenInLeakedRoom_FalseWithHidInLeakedRoom` · `Survives_NoLeakedRooms_True` · `Survives_NullLeakedRooms_TreatedAsEmpty` · `Survives_NotHiddenAndNoLeaks_FalseWithDayClockExpired` **(6)**

**60 cases**, all runnable without a scene (77 including `PoliceCaseTests`). `TimeOfDayTests.cs` adds 15: the clock face, the 5-minute step, both sides of the 6 PM boundary, noon/midnight, and the argument guards.

**Not covered by tests, and knowingly so:** the deferred dialogue queue and the hide prompt are Presentation — they need a `DialogueController`, a panel and a canvas, and PlayMode tests are off the table for the jam (`CLAUDE.md`). They get **manual checks** instead, on the checklist: trade a clue with the Uncle and confirm his reaction plays *after* the share panel closes, not on top of it; make a wrong police call and confirm the same; click a hiding spot, cancel, and confirm the day did not end and no time was charged.

## Out of scope

- **End screens** (`GameEndView`, retry button) and the **police call** — plan 06, Janhavi. This plan only raises `CH_GameLost` / `CH_DayStarted`.
- **Raising `CH_RoomChanged`** — room navigation's job (Janhavi).
- **Hiding spot art, the clock UI widget, and the visual design of the hide prompt** — Irene / a later pass. `HidingSpot`, `NormalizedRemaining` and the `HideConfirmPanel` prefab are the seams they plug into; what ships here is a grey box with two buttons that works.
- **A hard day cap.** The 3-day ceiling comes from 2 police lives, not from a rule here. If a player who never calls the police needs to be stopped, that is one serialized `_maxDays` and a `LossReason` value plan 06 owns — Gus's call, not a silent addition.
- **Not Grandma's night patrol, NPC pathfinding, save/load, randomized traitor.** Cut or Nice-to-Have.
- **Any player-facing text.** Beat ids and flag names are configuration; every line the player reads stays a human-written placeholder.
