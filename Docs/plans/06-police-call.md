# 06 — Police call
**Goal:** clicking the phone starts a short police dialogue and then opens a panel where the player hands one clue to the police: the real evidence wins the game, anything else costs police trust — once per day, from the second morning on. **Priority:** Must-Have (GDD §5 "Calling the police selecting evidence", §7 win/lose; day-2 plan §B MVP loop).

**Owner: Janhavi.** She writes the code *and* does the Unity editor setup for this feature. Gus reviews the PR.

> This plan is written to be followed top to bottom. Every file it asks for is listed with its full path, every asset with its menu path. If something here contradicts what you see in the editor, ask Gus before improvising — the architecture rules in `CLAUDE.md` are not negotiable, but the details here can be.

---

## Decisions this plan encodes (Gus, Aug 28)

1. **The phone is an item, not an NPC.** It lives in the house, is clickable all day, and appears from **day 2** onward (day-2 plan, Aug 28 amendment §3). One call per day.
2. **The day number arrives through a new event channel, `DayStartedEventChannelSO(int)`.** Nothing in the project raises it yet — the story/day-clock system is plan 04 (Gus, not written). **This plan defines the contract and ships a temporary debug button that raises it**, so the police call is fully testable on its own. When plan 04 lands, Gus points the `StoryDirector` at the same channel and the debug button is deleted. **Do not write your own day counter.**
3. **Right evidence = any `ClueSO` with `IsEvidence == true`.** The flag already exists on `ClueSO`. The Domain never sees a `ClueSO` — it receives a plain `bool`.
4. **The end of the game gets two new channels**, `GameWonEventChannelSO` (no payload) and `GameLostEventChannelSO(LossReason)`. You raise them and show a placeholder end screen. Gus's night check will raise the same `GameLost` channel later with a different reason.
5. **The clue-handover UI is a copy of the exchange panel, not a shared one.** `ClueSharePanelView` stays untouched; you write `PoliceCallPanelView` next to it, starting from a copy. Yes, that duplicates ~80 lines — that is deliberate: two independent panels are easier to build and to change under jam pressure than one generalized one. Merging them is a post-jam cleanup.
6. **The intro conversation reuses the dialogue system as-is.** A normal `DialogueSO` played by a normal `NpcSO` ("Police"). No new field on `DialogueSO`, no new dialogue feature.
7. **`Assets/Game/Scripts/PoliceCall/PoliceCall.cs` is the seed of this feature and gets refactored into `PhoneInteractable`**, not deleted and rewritten. The "Editor setup checklist" walks through the move.

## Jam rule reminder

**You write no player-facing text and neither does any AI.** Every string the player can read is a placeholder in brackets — `"[POLICE_GREETING_01]"` — until a human writes the real line. The police dialogue lines are Gus's to write; ask him for them once the flow works.

---

## Domain

Pure C#, **no `using UnityEngine;`**, namespace `Game.Domain`, folder `Assets/Game/Scripts/Domain/`. This is the part that gets unit-tested, so it must not know what a `ClueSO`, a phone or a panel is.

### `PoliceCallOutcome.cs`

```csharp
namespace Game.Domain
{
    public enum PoliceCallOutcome
    {
        Unavailable,    // the call could not be made at all (wrong day, already called, game over)
        Won,            // the clue was the real evidence
        WrongEvidence,  // wrong clue, trust lost, but the player is still in the game
        TrustLost       // wrong clue and that was the last of the police's patience
    }
}
```

### `LossReason.cs`

```csharp
namespace Game.Domain
{
    public enum LossReason
    {
        PoliceTrustLost,    // you raise this one
        HidInLeakedRoom,    // reserved for Gus's night check
        DayClockExpired     // reserved for Gus's night check
    }
}
```

Declare all three values now even though you only ever raise the first: the enum is the shared vocabulary of the end-of-game channel, and adding values later means re-touching Gus's code.

### `PoliceCase.cs`

One instance per run. Holds *all* the rules of the police call: when it is available, how many mistakes are left, and what a call does.

| Member | Contract |
|---|---|
| `PoliceCase(int startingTrust = 2, int firstAvailableDay = 2)` | Throws `ArgumentOutOfRangeException` if `startingTrust < 1` or `firstAvailableDay < 1`. |
| `int TrustRemaining` | Starts at `startingTrust`. Never goes below 0. |
| `int CurrentDay` | `0` until the first `StartDay` call. |
| `bool IsResolved` | `true` once the player has won or run out of trust. Once resolved, no further call is possible. |
| `bool IsPhoneAvailable` | `!IsResolved && CurrentDay >= firstAvailableDay`. Drives whether the phone object is visible in the room. |
| `bool CanCall` | `IsPhoneAvailable` **and** this day's call has not been used. |
| `void StartDay(int day)` | Throws `ArgumentOutOfRangeException` if `day < 1`. Sets `CurrentDay` and frees this day's call. |
| `PoliceCallOutcome Call(bool clueIsEvidence)` | If `!CanCall` → returns `Unavailable` **and changes nothing**. Otherwise marks the day's call as used, then: evidence → `IsResolved = true`, returns `Won`; not evidence → `TrustRemaining--`, and returns `TrustLost` (setting `IsResolved = true`) when trust reached 0, else `WrongEvidence`. |

Notes:

- A **wrong** call still burns the day's call. That is the whole cost of guessing.
- `startingTrust` defaults to **2** (day-2 plan §C3: "2 police lives"). The GDD §7 line reads "2 wrong accusations, the third is a loss", which would be 3 — if Gus wants that reading it is one number in the Inspector, not a code change. Do not hardcode it anywhere except the default.

## Events

Three new channel ScriptableObjects, namespace `Game.Events`, folder `Assets/Game/Scripts/Events/`. Copy the shape of `ClueCollectedEventChannelSO.cs` exactly — same `event Action<T> Raised;` + `Raise(...)` method, nothing else in the class.

| File | Payload | `[CreateAssetMenu]` menu | Asset name | Raised by | Listened by |
|---|---|---|---|---|---|
| `DayStartedEventChannelSO.cs` | `int` (day number, 1-based) | `Game/Events/Day Started` | `CH_DayStarted` | `DebugDayAdvancer` now, `StoryDirector` (plan 04) later | `PoliceCallController` |
| `GameWonEventChannelSO.cs` | none (`event Action Raised`) | `Game/Events/Game Won` | `CH_GameWon` | `PoliceCallController` | `GameEndView` |
| `GameLostEventChannelSO.cs` | `LossReason` | `Game/Events/Game Lost` | `CH_GameLost` | `PoliceCallController`, later the night check | `GameEndView` |

Existing channels you will **use but not create**: `DialogueRequestedEventChannelSO`, `DialogueFinishedEventChannelSO`.

Rules that apply to every channel, no exceptions (`CLAUDE.md`): reference them with `[SerializeField]` and wire them in the Inspector; subscribe in `OnEnable`; **always** unsubscribe in `OnDisable`; never `Resources.Load`, never a singleton or static access.

## Data

**No new ScriptableObject types.** You create *instances* of types that already exist:

- `NPC_Police` — an `NpcSO` (`Assets/Game/ScriptableObjects/Npcs/`). Id `npc_police`, display name `[POLICE]`, a colour distinct from the family, **no exchange entries, no fallback clue** (so `OffersExchange` is `false` — see the trap in the checklist), `_leaksToNotGrandma` off.
- `DLG_PoliceIntro` — a `DialogueSO`, 2–4 nodes, placeholder text, speaker `NPC_Police` on the police nodes. **`_allowsClueExchange` must stay OFF.**
- `DLG_PhoneUnavailable` — a `DialogueSO`, 1 node, placeholder text (the "you already used today's call" line). Also `_allowsClueExchange` OFF.
- At least one `ClueSO` with **`_isEvidence` ticked**, plus two without, so you can test both endings.

## Presentation

Namespace `Game.Presentation`. New folder `Assets/Game/Scripts/Presentation/PoliceCall/`. `Assets/Game/Scripts/PoliceCall/` disappears once `PoliceCall.cs` has moved (delete the folder from Unity's Project window, never from Windows Explorer — the `.meta` files must go with it).

| Component | Folder | Responsibility | Channels |
|---|---|---|---|
| `PoliceCallController` | `Presentation/PoliceCall/` | Owns the `PoliceCase`. The only place that decides anything. | listens `DayStarted`, `DialogueFinished`; raises `DialogueRequested`, `GameWon`, `GameLost` |
| `PhoneInteractable` *(from `PoliceCall.cs`)* | `Presentation/PoliceCall/` | The clickable phone in the room. Shows/hides itself with the controller's availability and forwards the click. No rules. | — (talks to the controller directly) |
| `PoliceCallPanelView` | `Presentation/UI/` | The hand-over-a-clue panel. Copy of `ClueSharePanelView`, adapted. | — (reads the controller) |
| `PoliceClueDropSlot` | `Presentation/UI/` | `IDropHandler` for the police panel's drop zone. Copy of `ClueDropSlot` with the panel type swapped. | — |
| `GameEndView` | `Presentation/UI/` | Placeholder win/lose screen, so the run can actually end on screen. | listens `GameWon`, `GameLost` |
| `DebugDayAdvancer` | `Presentation/Debug/` | **Temporary.** A UI button that raises `DayStarted` with 1, 2, 3… so you can test without the day clock. | raises `DayStarted` |

Reused as-is, do **not** modify: `DraggableClueItem` (and its prefab), `NotebookController`, `DialogueController`, `DialogueView`, `ClueSharePanelView`.

### `PoliceCallController` — the shape to write

```csharp
[SerializeField] private DialogueSO _policeIntroDialogue;
[SerializeField] private DialogueSO _phoneUnavailableDialogue;
[SerializeField] private DayStartedEventChannelSO _dayStarted;
[SerializeField] private DialogueRequestedEventChannelSO _dialogueRequested;
[SerializeField] private DialogueFinishedEventChannelSO _dialogueFinished;
[SerializeField] private GameWonEventChannelSO _gameWon;
[SerializeField] private GameLostEventChannelSO _gameLost;
[SerializeField] private int _startingTrust = 2;
[SerializeField] private int _firstAvailableDay = 2;

public event Action OnAvailabilityChanged;   // the phone listens to this
public event Action OnCallPanelStateChanged; // the panel listens to this
```

Public surface: `bool IsPhoneAvailable`, `bool IsCallPanelActive`, `int TrustRemaining`, `void RequestCall()`, `PoliceCallOutcome SubmitEvidence(ClueSO clue)`, `void CloseCall()`.

Behaviour:

- `Awake`: `_case = new PoliceCase(_startingTrust, _firstAvailableDay);`
- `OnEnable` / `OnDisable`: subscribe / unsubscribe `_dayStarted.Raised` and `_dialogueFinished.Raised`.
- On `DayStarted(day)`: `_case.StartDay(day)`, then `OnAvailabilityChanged?.Invoke()`.
- `RequestCall()` — called by the phone: if `_case.CanCall`, set a private `_waitingForIntro = true` and raise `DialogueRequested(_policeIntroDialogue)`; otherwise raise `DialogueRequested(_phoneUnavailableDialogue)` and return.
- On `DialogueFinished(dialogue)`: **ignore it unless `_waitingForIntro` and `dialogue == _policeIntroDialogue`** — that channel fires for *every* conversation in the game, including every NPC. Then `_waitingForIntro = false`, `_isCallPanelActive = true`, `OnCallPanelStateChanged?.Invoke()`.
- `SubmitEvidence(clue)`: `PoliceCallOutcome outcome = _case.Call(clue.IsEvidence);` → `OnAvailabilityChanged?.Invoke()` (the call is spent) → raise `_gameWon.Raise()` on `Won`, or `_gameLost.Raise(LossReason.PoliceTrustLost)` on `TrustLost` → return the outcome. **The panel stays open** so the player can read the result; the world stays click-blocked meanwhile.
- `CloseCall()`: `_isCallPanelActive = false; OnCallPanelStateChanged?.Invoke();`

### `PhoneInteractable` — what changes from `PoliceCall.cs`

Today the file uses `OnMouseDown()` and writes into a `TMP_Text` directly. Both go away:

- **`OnMouseDown` → `IInteractable`.** This project routes every world click through `ClickRouter`, which raycasts and calls `Interact()` on whatever it hit. `OnMouseDown` bypasses the Input System *and* the dialogue/panel guards, so the phone would still be clickable while a panel is open. Implement `IInteractable` and keep `[RequireComponent(typeof(Collider2D))]`.
- **No `TMP_Text` reference.** A presentation component never writes another screen's text. The controller raises `DialogueRequested` and the existing `DialogueView` draws it.
- Delete the empty `Start()` and `Update()`.

```csharp
[RequireComponent(typeof(Collider2D))]
public class PhoneInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private PoliceCallController _policeCallController;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;   // cached in Awake — never GetComponent in Update

    public void Interact() => _policeCallController.RequestCall();

    // OnEnable:  subscribe to OnAvailabilityChanged, then sync once
    // OnDisable: unsubscribe
    // Sync():    _spriteRenderer.enabled = _collider.enabled = _policeCallController.IsPhoneAvailable;
}
```

### `PoliceCallPanelView` — how to copy the exchange panel

Start from `Assets/Game/Scripts/Presentation/UI/ClueSharePanelView.cs`, save it as `PoliceCallPanelView.cs`, then:

| In `ClueSharePanelView` | In `PoliceCallPanelView` |
|---|---|
| `[SerializeField] ExchangeController _exchangeController` | `[SerializeField] PoliceCallController _policeCallController` |
| header read from `_exchangeController.CurrentNpc` | header read from a serialized `[SerializeField] NpcSO _police` (portrait, `DisplayName`, `Color`) |
| `OnExchangeStateChanged` / `IsExchangeActive` | `OnCallPanelStateChanged` / `IsCallPanelActive` |
| `clueItem.Init(clue, !HasSharedWithCurrentNpc(clue))` | `clueItem.Init(clue, true)` — every clue can be offered to the police; there is no per-NPC memory here |
| `OnClueDropped` → `Share(clue)` → show the returned clue's text | `OnClueDropped` → `SubmitEvidence(clue)` → `ClearClues()` → show the placeholder for the outcome |
| `RequestClose()` → `CloseExchange()` | `RequestClose()` → `CloseCall()` |

Keep these two details from the original — they are bug fixes someone already paid for:

- **activate the panel GameObject *before* instantiating clue items** (items created under an inactive parent never run `Awake`);
- clear the spawned-items list on hide.

Serialized placeholder strings (jam rule — no written text in code):

```csharp
[SerializeField] private string _wonPlaceholder = "[POLICE_ARREST]";
[SerializeField] private string _wrongEvidencePlaceholder = "[POLICE_WRONG_EVIDENCE]";
[SerializeField] private string _trustLostPlaceholder = "[POLICE_TRUST_LOST]";
[SerializeField] private string _noCluesPlaceholder = "[NO_CLUES_TO_SHARE]";
```

### `PoliceClueDropSlot`

Copy of `ClueDropSlot.cs` with `ClueSharePanelView` swapped for `PoliceCallPanelView`. Four lines of difference; it exists only because the two panels are separate types.

### `GameEndView`

A panel with one TMP text and nothing else. Listens to `GameWon` (show `[WIN_PLACEHOLDER]`) and `GameLost` (a placeholder per `LossReason`). No restart button needed — Gus owns the real end screens.

### `DebugDayAdvancer` — delete me later

```csharp
[SerializeField] private DayStartedEventChannelSO _dayStarted;
[SerializeField] private Button _advanceButton;
private int _day;
// Start():      _day = 1; _dayStarted.Raise(_day);
// button click: _day++;   _dayStarted.Raise(_day);
```

Put `// TODO: remove when the StoryDirector (plan 04) raises CH_DayStarted` at the top of the file, and say so in the PR description so it does not ship by accident.

### One change in a file that is not yours

`Assets/Game/Scripts/Presentation/ClickRouter.cs` already blocks world clicks while dialogue or the exchange panel is open. The police panel needs the same guard:

```csharp
if (_policeCallController.IsCallPanelActive)
{
    return;
}
```

next to the existing `_exchangeController` guard, plus the new `[SerializeField]`. **Tell Gus in the PR that you touched this file** — it is his, and it is the one place your branch can collide with his work.

## Editor setup checklist

All of this is yours. Work on **branch `feature/police-call` off `dev`** — never commit to `main` or `dev`.

1. **Folders** (Project window → right-click → Create → Folder): `Assets/Game/Scripts/Presentation/PoliceCall/` and `Assets/Game/Scripts/Presentation/Debug/`.
2. **Move** `Assets/Game/Scripts/PoliceCall/PoliceCall.cs` into `Presentation/PoliceCall/` **by dragging it inside the Unity Project window** (that keeps its `.meta` and every reference alive), rename the file to `PhoneInteractable.cs`, then rename the class to match. Delete the now-empty `Scripts/PoliceCall/` folder from the Project window.
3. **Channel assets** in `Assets/Game/ScriptableObjects/Channels/` (right-click → Create → …): `CH_DayStarted` (`Game/Events/Day Started`), `CH_GameWon` (`Game/Events/Game Won`), `CH_GameLost` (`Game/Events/Game Lost`).
4. **`NPC_Police`** in `Assets/Game/ScriptableObjects/Npcs/` (Create → `Game/Data/Npc`). Leave the exchange entries array **empty** and the fallback clue **None**.
5. **Dialogue assets** (Create → `Game/Data/Dialogue`, same folder as the existing `DLG_*`): `DLG_PoliceIntro` (2–4 nodes; `SpeakerType.Npc` + speaker `NPC_Police` on the police lines, `SpeakerType.Player` on the protagonist's) and `DLG_PhoneUnavailable` (1 node). **Both with `_allowsClueExchange` unticked.**
   > **Trap:** if you tick `_allowsClueExchange` on a police dialogue, `ExchangeController` may try to open the *trade* panel on top of yours. `NPC_Police` having no exchange entries is the second lock on that door — keep both.
6. **Clues:** make sure one `ClueSO` has `_isEvidence` ticked and two do not, and that all three are collectable in your test scene (an `InteractableItem` each).
7. **Phone object** in the scene: a sprite (a placeholder square is fine) + `Collider2D` + `PhoneInteractable`, with the controller reference wired. Put it in **one** room — Living room — not in all four.
8. **`PoliceCallController`:** an empty GameObject named `PoliceCallController` next to the other controllers. Wire the two dialogues and the five channels; leave `_startingTrust = 2` and `_firstAvailableDay = 2`.
9. **Police panel** on the Canvas — same anatomy as the share panel: root panel (starts inactive), portrait `Image` + name TMP, a **drop zone `Image` carrying `PoliceClueDropSlot`** (that Image needs *Raycast Target* ON), a vertical container for clue items, a result TMP, and a Close `Button`. Wire `PoliceCallPanelView` to the controller, the `NotebookController`, the existing **`DraggableClueItem` prefab** (reuse it, do not make a new one) and everything above.
   > The `EventSystem` in the scene must use **`InputSystemUIInputModule`**, not the legacy `StandaloneInputModule`, or no drag event ever fires.
10. **`GameEndView`:** a full-screen panel (inactive by default) with one TMP text, wired to `CH_GameWon` and `CH_GameLost`.
11. **Debug day button:** a UI Button labelled `NEXT DAY` in a corner, plus the `DebugDayAdvancer` component wired to it and to `CH_DayStarted`.
12. **`ClickRouter`:** drag the `PoliceCallController` into its new field.
13. **Smoke test**, in this order:
    - Press Play on day 1 → **the phone is invisible and unclickable.**
    - Click `NEXT DAY` (day 2) → the phone appears. Click it → the police dialogue plays → after the last line the panel opens with your notebook clues in it.
    - While the panel is open, click the room → **nothing happens.**
    - Drop a **non-evidence** clue → `[POLICE_WRONG_EVIDENCE]`, trust drops to 1. Close, then click the phone again **the same day** → the `DLG_PhoneUnavailable` line, no panel.
    - `NEXT DAY` → call again, drop another wrong clue → `[POLICE_TRUST_LOST]` and the lose screen.
    - Restart and, on day 2, drop the **evidence** clue → win screen, and the phone is gone afterwards.
14. **Test Runner** (Window → General → Test Runner → EditMode): `PoliceCaseTests` all green.
15. Commit in small steps (`police: what changed`), push, and open a **PR into `dev`**. List in the description: the `ClickRouter` change, the temporary `DebugDayAdvancer`, and the placeholder strings that still need real writing.

## Tests

`Assets/Tests/Editor/PoliceCaseTests.cs` — **already written and committed to `dev`**; the 17 tests below start **red** (the file does not compile) until `PoliceCase`, `PoliceCallOutcome` and `LossReason` exist. Getting them green is the definition of done for the Domain part — do not edit the tests to fit your code; if a test looks wrong, ask Gus. Domain only: no scene, no `ClueSO`, no MonoBehaviour. Naming: `MethodOrRule_Scenario_ExpectedResult`.

- `Ctor_Defaults_TrustIsTwoAndDayIsZero`
- `Ctor_TrustBelowOne_Throws`
- `Ctor_FirstAvailableDayBelowOne_Throws`
- `IsPhoneAvailable_BeforeFirstAvailableDay_IsFalse`
- `IsPhoneAvailable_OnFirstAvailableDay_IsTrue`
- `StartDay_InvalidDay_Throws`
- `Call_BeforeFirstAvailableDay_UnavailableAndTrustUnchanged`
- `Call_BeforeAnyDayStarted_Unavailable`
- `Call_WithEvidence_WonAndCaseResolved`
- `Call_WithWrongClue_WrongEvidenceAndTrustDecreases`
- `Call_LastTrustWithWrongClue_TrustLostAndCaseResolved`
- `Call_TwiceInTheSameDay_SecondIsUnavailable`
- `Call_AfterStartingANewDay_IsAllowedAgain`
- `Call_AfterWinning_Unavailable`
- `Call_AfterTrustLost_Unavailable`
- `Call_WithCustomStartingTrust_SurvivesThatManyWrongCalls`
- `Call_WithCustomFirstAvailableDay_FollowsThatDay`

## Out of scope

Deliberately not in this plan — do not build these, even if they look easy:

- The day clock, the day/night transition, the hiding spots and the night check (Gus, plan 04 + the night).
- Accusing an NPC other than Not Grandma (Nice-to-Have, day-2 plan §F3).
- Real win/lose screens with art and a restart button (Gus).
- Merging `PoliceCallPanelView` with `ClueSharePanelView` (post-jam).
- Any real dialogue, clue or result text (jam rule: humans only).
- Audio for the phone or the call — the SFX router is plan 05; if you want a hook, say so in the PR and Gus adds the channel.
