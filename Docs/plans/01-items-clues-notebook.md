# 01 — Interactable items, clues & notebook
**Goal:** Clickable items in rooms that show text or yield a clue, collected into a notebook the player opens with Tab. **Priority:** Must-Have (GDD §5: "Clues/evidence notebook system", "Player interaction with items"; day-2 plan, Mon 24 / Janhavi).

## Domain
Pure C#, no UnityEngine. Namespace `Game.Domain`.

- `RoomId` (enum): `Kitchen, LivingRoom, Bedroom, Bathroom`. The 4 rooms are locked scope; an enum is testable and cheap. A `RoomSO` can arrive later with the navigation feature if rooms need content.
- `Notebook` (class): ordered, duplicate-free collection of clue ids (`string`).
  - `bool Collect(string clueId)` — `true` if newly added, `false` if already collected (state unchanged). Throws `ArgumentException` on null/empty id.
  - `bool Contains(string clueId)`
  - `int Count`
  - `IReadOnlyList<string> CollectedIds` — insertion order.
  - No SO references: Domain works with ids only. Data references Domain (`ClueSO` uses `RoomId`), never the reverse.

## Events
Channel SOs per the CLAUDE.md pattern (inspector-wired, subscribe `OnEnable` / unsubscribe `OnDisable`). Namespace `Game.Events`, in `Assets/Game/Scripts/Events/`.

| Channel | Payload | Raised by | Listened by |
|---|---|---|---|
| `ClueCollectedEventChannelSO` | `ClueSO` | `InteractableItem` (first click on a clue-bearing item) | `NotebookController`; later: audio feedback, exchange system |
| `ItemInspectedEventChannelSO` | `ItemSO` | `InteractableItem` (every click) | `ItemInspectPopup` |

## Data
Namespace `Game.Data`, in `Assets/Game/Scripts/Data/`. Data only, no behaviour. All player-facing strings are placeholders (`"[CLUE_KITCHEN_01]"`) — real text is written by Gus (writing split, day-2 plan §A5; jam rule: no AI text).

- `ClueSO` — `[CreateAssetMenu(menuName = "Game/Data/Clue")]`
  - `string _id` — unique, e.g. `clue_kitchen_01`
  - `[TextArea] string _text` — placeholder
  - `RoomId _roomId` — feeds the C1 leak, and from Aug 28 the night check: hide in a leaked room and you lose
  - `bool _isEvidence` — the police accusation filters on this later
- `ItemSO` — `[CreateAssetMenu(menuName = "Game/Data/Item")]`
  - `string _id`
  - `[TextArea] string _description` — placeholder
  - `ClueSO _clue` — optional (null = text-only item)

Instances to create: 2–3 `ClueSO` (at least one with `_isEvidence = true`) and 3–4 `ItemSO` (at least one text-only) — see checklist.

## Presentation
Namespace `Game.Presentation`, thin MonoBehaviours.

| Component | Folder | Responsibility | Channels |
|---|---|---|---|
| `ClickRouter` | `Presentation/` | Sole reader of input for the world. Reads the `Click` (Button) and `Point` (Value/Vector2) actions, raycasts `Physics2D` at the pointer position, calls `IInteractable.Interact()` on the hit. Camera serialized, never `Camera.main`. | — |
| `InteractableItem` | `Presentation/Clues/` | `IInteractable` + `Collider2D`. Serialized refs: `ClueSO`, `ItemSO`, and both channels. On interact: if it has an unconsumed clue → raise `ClueCollected` and mark consumed; if inspectable → raise `ItemInspected`. | raises both |
| `NotebookController` | `Presentation/Clues/` | Owns the Domain `Notebook` instance. Listens to `ClueCollected`, calls `Collect()` (defensive dedupe). Exposes collected clues to the view. | listens `ClueCollected` |
| `NotebookView` | `Presentation/UI/` | uGUI + TMP panel listing collected clue texts. Toggled by the `Toggle Notebook` (Tab) input action. Visual placeholder — Irene delivers the real notebook UI Wednesday. | — (reads controller) |
 | `ItemInspectPopup` | `Presentation/UI/` | Shows the `ItemSO` description in a simple TMP label when an item is inspected. | listens `ItemInspected` |

`IInteractable` lives in `Presentation/` (it's an engine-facing seam, not a game rule).

## Editor setup checklist
All manual work by Gus:

1. Create the `Assets/Game/` folder structure from CLAUDE.md (`Scripts/{Domain,Data,Events,Presentation/{Clues,UI}}`, `ScriptableObjects/{Clues,Channels}`, `Scenes`) and `Assets/Tests/Editor/`.
2. Open `Assets/Settings/InputSystem_Actions.inputactions` and add to the Player map: `Toggle Notebook` (Button → `<Keyboard>/tab`), `Click` (Button → `<Mouse>/leftButton`), `Point` (Value/Vector2 → `<Pointer>/position`, so mouse and touch both work on WebGL). Every component that subscribes to an action must also `Enable()` it in `OnEnable` and `Disable()` it in `OnDisable` — there is no `PlayerInput` in the scene enabling the map for you.
3. After writing the code: create channel assets in `Assets/Game/ScriptableObjects/Channels/` → `CH_ClueCollected` (menu `Game/Events/Clue Collected`), `CH_ItemInspected` (menu `Game/Events/Item Inspected`).
4. Create test data in `Assets/Game/ScriptableObjects/Clues/`: 2–3 clues (`Game/Data/Clue`) with placeholder ids/texts, one with `_isEvidence` on; 3–4 items (`Game/Data/Item`), one text-only (no clue).
5. Test scene (`Assets/Game/Scenes/ClueSandbox.unity`): placeholder sprites with `Collider2D` + `InteractableItem` (wire `ItemSO` + both channels), one `ClickRouter`, a Canvas with `NotebookView` + `ItemInspectPopup` (wire channels/controller refs, TMP texts).
6. Run Test Runner → EditMode until green.

## Tests
`Assets/Tests/Editor/NotebookTests.cs` (written by Claude, start red until Domain exists):

- `Count_EmptyNotebook_IsZero` — given a new notebook / then count is 0.
- `Collect_NewClue_ReturnsTrueAndAddsIt` — when collecting an uncollected id / returns true, Contains is true, count is 1.
- `Collect_DuplicateClue_ReturnsFalseAndCountUnchanged` — given an id already collected / when collected again / returns false, count unchanged.
- `Collect_MultipleClues_PreservesInsertionOrder` — when collecting A, B, C / CollectedIds is [A, B, C].
- `Contains_UncollectedClue_ReturnsFalse`
- `Collect_NullOrEmptyId_Throws` — null, empty and whitespace ids throw `ArgumentException`.

## Out of scope
Usable-item inventory (Nice-to-Have #5), clue combining, clue-for-clue exchange with NPCs, leak tracking (C1), police accusation, room navigation, day/night, final notebook art/UI, audio feedback on collection.
