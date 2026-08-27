# 02 — Dialogue system (graphs with cosmetic branching)
**Goal:** Click an NPC to play a dialogue graph — sequences of speaker lines with optional player choices that alter which lines play but never game state. **Priority:** Must-Have (GDD §5: "Dialogue system", "Dialogue trees for each NPC"; day-2 plan Aug 26 amendment: homemade, cosmetic branching, owned by Gus).

Decisions this plan encodes (day-2 plan, Aug 26 amendment): no Yarn Spinner / Ink; branching is cosmetic only; the seam to the future `StoryDirector` (plan 03) is the pair of channels below — *which* dialogue an NPC offers is not this plan's problem.

## Domain
Pure C#, no UnityEngine. Namespace `Game.Domain`. The domain models dialogue **topology only** — indices, not text. Text lives in Data; Presentation joins the two by node index.

- `DialogueNode` (immutable):
  - `int NextIndex` — node to advance to when the node has no options; `DialogueGraph.EndIndex` (`-1`) ends the dialogue.
  - `IReadOnlyList<int> OptionTargets` — empty = linear node. Each target is a node index or `EndIndex`.
- `DialogueGraph` (immutable):
  - `const int EndIndex = -1`.
  - ctor `DialogueGraph(IReadOnlyList<DialogueNode> nodes)` — throws `ArgumentException` if `nodes` is null/empty, or if any `NextIndex`/option target is neither `EndIndex` nor a valid node index. Catching bad indices at construction is the whole defense against the classic adventure-game "flag bug" — a branch that points nowhere must fail in an EditMode test, not in a WebGL build.
  - `int Count`, `DialogueNode this[int index]`.
- `DialogueRunner` (mutable traversal state, one per conversation):
  - ctor `DialogueRunner(DialogueGraph graph)` — starts at node `0`, throws `ArgumentNullException` on null graph.
  - `int CurrentIndex` — throws `InvalidOperationException` when `IsFinished`.
  - `bool IsFinished`
  - `bool CurrentHasOptions`
  - `void Advance()` — linear nodes only; moves to `NextIndex` or finishes. Throws `InvalidOperationException` on a choice node or when finished.
  - `void Choose(int optionIndex)` — choice nodes only; jumps to the option's target (or finishes on `EndIndex`). Throws `InvalidOperationException` on a linear node or when finished; `ArgumentOutOfRangeException` for a bad option index.
  - Deliberately **no side effects, no flags, no callbacks**: choosing an option only changes `CurrentIndex`. That is the "cosmetic branching" rule enforced by design.

## Events
Channel SOs per the CLAUDE.md pattern. Namespace `Game.Events`, in `Assets/Game/Scripts/Events/`.

| Channel | Payload | Raised by | Listened by |
|---|---|---|---|
| `DialogueRequestedEventChannelSO` | `DialogueSO` | `NpcInteractable` (click on an NPC) | `DialogueController` |
| `DialogueFinishedEventChannelSO` | `DialogueSO` | `DialogueController` (runner reached end) | plan 03 `StoryDirector` (beat trigger "talked to X"); later: audio |

## Data
Namespace `Game.Data`, in `Assets/Game/Scripts/Data/`. All player-facing strings are placeholders (`"[MOTHER_INTRO_01]"`, `"[OPT_ASK_ABOUT_GRANDMA]"`) — real lines are written by their assigned humans (writing split, day-2 plan §A5; jam rule: no AI text).

- `DialogueSO` — `[CreateAssetMenu(menuName = "Game/Data/Dialogue")]`
  - `string _id` — unique, e.g. `dlg_mother_intro`
  - `DialogueNodeData[] _nodes`
  - `DialogueGraph ToGraph()` — builds the Domain graph from the node array (Data may reference Domain, never the reverse). Any authoring mistake surfaces as the ctor's `ArgumentException`.
- `SpeakerType` (enum, own file): `Npc`, `Player`, `InnerMonologue`. Presentation-facing metadata — the Domain never sees it. Default (`Npc`) keeps previously-authored assets valid. *(Added Aug 26: inner monologues + distinct player text.)*
- `DialogueNodeData` (`[Serializable]`, plain class in the same file):
  - `SpeakerType _speakerType` — drives the view: `InnerMonologue` hides speaker name/portrait; `Player` and `Npc` pick different text styles.
  - `string _speakerName` — placeholder display name (`"[MOTHER]"`). Ignored by the view for `InnerMonologue` nodes. A proper `NpcSO` arrives with the NPC feature; don't build it here.
  - `[TextArea] string _text` — the line, placeholder.
  - `DialogueOptionData[] _options` — empty = linear node.
  - `int _nextIndex` — used when `_options` is empty; `-1` ends.
- `DialogueOptionData` (`[Serializable]`):
  - `[TextArea] string _text` — the choice label, placeholder.
  - `int _targetIndex` — node index or `-1`.

## Presentation
Namespace `Game.Presentation`, thin MonoBehaviours in `Presentation/Dialogue/` (new folder) and `Presentation/UI/`.

| Component | Folder | Responsibility | Channels |
|---|---|---|---|
| `NpcInteractable` | `Presentation/Dialogue/` | `IInteractable` + `Collider2D` on the NPC placeholder. Serialized: `DialogueSO` (the dialogue this NPC currently offers — plan 03 will swap it by story state) + request channel. On interact: raise `DialogueRequested`. | raises `DialogueRequested` |
| `DialogueController` | `Presentation/Dialogue/` | Owns the `DialogueRunner` for the active conversation. Listens to `DialogueRequested`: builds the graph (`ToGraph()`), creates a runner, exposes the current node's data (speaker/text/options) to the view. Public `Advance()` / `Choose(int)` called by the view; raises `DialogueFinished` and clears state when the runner finishes. Ignores requests while a dialogue is active. `IsDialogueActive` is public — `ClickRouter` gates on it. | listens `DialogueRequested`, raises `DialogueFinished` |
| `DialogueView` | `Presentation/UI/` | uGUI + TMP panel: speaker label, optional portrait slot, line text, and a group of option buttons (choice nodes; only as many active as there are options; option-index→node-index mapping stays inside the runner). Subscribes to `DialogueController.OnNodeChanged`, re-reads the current node on each change. Per-`SpeakerType` styling (serialized color + font style for Npc / Player / InnerMonologue); `InnerMonologue` hides the speaker label and portrait. Placeholder visuals — Irene owns the dialogue UI art. | — (reads controller) |

Advance path (as implemented): **`ClickRouter` advances linear nodes on any click** while a dialogue is active (and ignores clicks when the node has options, leaving them to the buttons). There is no Continue button — do not wire one, or lines will advance twice per click.

`ClickRouter` change (one guard, Gus): skip the world raycast while `DialogueController.IsDialogueActive` (serialized reference). uGUI does **not** block `Physics2D` raycasts by itself; without the guard, clicking a dialogue button also clicks the room behind it.

WebGL note: nothing here allocates per-frame or touches threads/`System.IO`; buttons are click-driven, so no autoplay/audio implications.

## Editor setup checklist
All manual work by Gus:

1. Create folders: `Assets/Game/Scripts/Presentation/Dialogue/`, `Assets/Game/ScriptableObjects/Dialogues/`.
2. After writing the code: create channel assets in `Assets/Game/ScriptableObjects/Channels/` → `CH_DialogueRequested` (menu `Game/Events/Dialogue Requested`), `CH_DialogueFinished` (menu `Game/Events/Dialogue Finished`).
3. Create two test dialogues in `Assets/Game/ScriptableObjects/Dialogues/` (menu `Game/Data/Dialogue`), placeholder text only:
   - `DLG_Test_Linear` — 3 linear nodes, last node `_nextIndex = -1`.
   - `DLG_Test_Branching` — a choice node with 2–3 options whose branches **converge** on a shared closing node (the cosmetic-branching shape from the flowchart).
   - `DLG_Test_Monologue` — 2–3 nodes with `_speakerType = InnerMonologue` (speaker name irrelevant), mixed with at least one `Player` node to see both styles.
4. In the sandbox scene: an NPC placeholder sprite + `Collider2D` + `NpcInteractable` (wire a test `DialogueSO` + `CH_DialogueRequested`).
5. Canvas: dialogue panel (speaker TMP, line TMP, options container with 3 option buttons — **no Continue button**, `ClickRouter` advances on click) + `DialogueView` + `DialogueController` (wire both channels and the controller↔view refs). In `DialogueView`, set the three `SpeakerStyle` blocks (Npc / Player / InnerMonologue: text color + font style); the portrait slot may stay unwired until Irene's art lands. Option buttons get their listeners from code (`Awake`) — leave their inspector `OnClick` lists empty.
6. Wire the `DialogueController` reference into `ClickRouter` for the world-click guard, and verify clicking through the panel no longer hits items behind it.
7. Run Test Runner → EditMode until `DialogueGraphTests` + `DialogueRunnerTests` are green.

## Tests
`Assets/Tests/Editor/DialogueGraphTests.cs` and `Assets/Tests/Editor/DialogueRunnerTests.cs` (written by Claude, start red until Domain exists):

**DialogueGraph**
- `Ctor_NullOrEmptyNodes_Throws` — null and empty node lists throw `ArgumentException`.
- `Ctor_NextIndexOutOfRange_Throws` — a linear node pointing past the last index (or below `-1`) throws.
- `Ctor_OptionTargetOutOfRange_Throws` — an option targeting an invalid index throws.
- `Ctor_ValidGraph_ExposesNodes` — `Count` and the indexer return the given nodes.

**DialogueRunner**
- `Ctor_NewRunner_StartsAtNodeZeroUnfinished` — `CurrentIndex == 0`, not finished.
- `Advance_LinearNode_MovesToNextIndex` — given node 0 → 1 / when advanced / current is 1.
- `Advance_NodeWithEndIndex_Finishes` — advancing a node whose next is `-1` sets `IsFinished`; `CurrentIndex` then throws `InvalidOperationException`.
- `Advance_OnChoiceNode_Throws` — `InvalidOperationException`.
- `Advance_WhenFinished_Throws` — `InvalidOperationException`.
- `Choose_ValidOption_JumpsToTarget` — choosing option 1 moves to that option's target node.
- `Choose_OptionTargetingEnd_Finishes` — an option whose target is `-1` ends the dialogue.
- `Choose_OnLinearNode_Throws` — `InvalidOperationException`.
- `Choose_OutOfRangeOption_Throws` — `ArgumentOutOfRangeException` (negative and ≥ option count).
- `Choose_WhenFinished_Throws` — `InvalidOperationException`.
- `Branches_Converge_BothPathsReachSharedNode` — the cosmetic-branching shape: two options lead through different line nodes into the same closing node; both traversals end identically.

## Out of scope
Clue-for-clue exchange UI (bespoke panel, own plan), story-state-driven selection of *which* dialogue an NPC offers (plan 03 — `StoryDirector`), setting flags or any game state from dialogue (explicitly forbidden by the Aug 26 decision), `NpcSO` and portrait art/NPC movement (the view only reserves a hide-able portrait slot), typewriter or per-letter effects, dialogue audio, localization, real written lines (humans only).
