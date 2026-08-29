# 03 — NPCs & clue-for-clue exchange
**Goal:** NPC identity assets and the clue-for-clue exchange: after talking to an NPC the player may share a clue from the notebook, receives what that NPC's exchange table returns, and every clue shared with the leaker marks its room as leaked for the night patrol. **Priority:** Must-Have (GDD §5: "Information exchange using clues/evidence", "Characters"; day-2 plan C1 — the leak is the theme's Must-Have consequence).

Decisions this plan encodes (day-2 plan, Aug 27 amendment):
- **What an NPC returns is a per-NPC map** given-clue → returned-clue, with an optional fallback for unmapped clues (the flowchart's "useless NotGrandma clue"). The Uncle's-clue → Cousin → evidence beat is just a map entry.
- **Sharing keeps the clue.** Information is shared, not surrendered; the real cost is the leak. The same clue can be shared with several NPCs.
- **One share per (NPC, clue).** NPCs remember what you told them (C1); repeating a clue to the same NPC does nothing — no reward farming, no double leak.
- **Leak tracking lives here.** The exchange is the only place a leak is born. The night only *consumes* `LeakedRooms`. *(Aug 28: the consumer is no longer a patrol but the night check — hiding in a leaked room is a loss. `ExchangeLog` is unchanged, and leaks now accumulate across every day of the run.)*
- **Entry point: a share prompt when a conversation ends — but only after dialogues explicitly marked for it.** Dialogue graphs stay cosmetic; the exchange is presentation flow *after* `DialogueFinished`, never a dialogue-node effect. Most conversations (intros, story beats, Not Grandma) end with no prompt at all.
- **NPCs carry their visual identity in data** (Aug 27, second pass): a representative `Color` plus two sprites — a dialogue portrait and a world sprite. **All 4 characters get an `NpcSO`**, Not Grandma included (her unique colour is the GDD §C4 visual-deception hook); she simply has an empty exchange table. Dialogue nodes reference the speaker's `NpcSO` directly — the `_speakerName` string dies.

## Domain
Pure C#, no UnityEngine. Namespace `Game.Domain`. Ids and `RoomId` only — no SO references.

- `ExchangeTable` (immutable): what an NPC gives back.
  - ctor `ExchangeTable(IReadOnlyDictionary<string, string> pairs, string fallbackReturnClueId = null)` — throws `ArgumentNullException` on null `pairs`; throws `ArgumentException` on null/empty keys or values.
  - `bool TryGetReturn(string givenClueId, out string returnedClueId)` — mapped value, else fallback, else `false`.
- `NpcProfile` (immutable): the domain-facing identity of an NPC.
  - ctor `NpcProfile(string id, bool leaksToNotGrandma, ExchangeTable exchanges)` — throws on null/empty id or null table.
  - `string Id`, `bool LeaksToNotGrandma`, `ExchangeTable Exchanges`.
- `ShareOutcome` (enum): `Accepted`, `AlreadyShared`.
- `ShareResult` (readonly struct): `ShareOutcome Outcome`, `string ReturnedClueId` (null when nothing is returned or the share was rejected), `bool LeakedNewRoom`, `RoomId LeakedRoom` (valid only when `LeakedNewRoom`).
- `ExchangeLog` (mutable, one per run): the memory of every share plus the leaked-rooms set. This is the C1 state.
  - `ShareResult Share(NpcProfile npc, string clueId, RoomId clueRoom)` — throws `ArgumentNullException` on null npc, `ArgumentException` on null/empty/whitespace clueId. If `(npc, clue)` was already shared: `AlreadyShared`, state untouched. Otherwise records the share, resolves the return via the npc's table, and — only if `npc.LeaksToNotGrandma` — adds `clueRoom` to the leaked set (set semantics: a room leaks once; `LeakedNewRoom` reports whether this call grew the set).
  - `bool HasShared(string npcId, string clueId)`
  - `IReadOnlyCollection<RoomId> LeakedRooms`

## Events
Channel SOs per the CLAUDE.md pattern. Namespace `Game.Events`.

| Channel | Payload | Raised by | Listened by |
|---|---|---|---|
| `NpcEngagedEventChannelSO` | `NpcSO` | `NpcInteractable` (on interact, before requesting dialogue) | `ExchangeController` (remembers who the conversation is with) |
| `ClueSharedEventChannelSO` | `Action<NpcSO, ClueSO>` | `ExchangeController` (accepted share) | plan 04 `StoryDirector`; later: audio lie layer |
| `RoomLeakedEventChannelSO` | `RoomId` | `ExchangeController` (a share with the leaker leaked a new room) | the night check *(Aug 28 — not the audio: an audible leak would name the traitor)* |
| `ClueCollectedEventChannelSO` *(existing)* | `ClueSO` | `ExchangeController` (the returned clue) | `NotebookController` — the returned clue lands in the notebook through the same pipe as a found clue, dedupe included |

## Data
Namespace `Game.Data`. Data only; all strings placeholders (jam rule: no AI text).

- `NpcSO` — `[CreateAssetMenu(menuName = "Game/Data/Npc")]`
  - `string _id` — e.g. `npc_uncle`
  - `string _displayName` — placeholder (`"[UNCLE]"`)
  - `Color _color` — the character's representative colour. Used for name + line tint in dialogue, and available to any later view (notebook attribution, accusation UI). Not Grandma's is the one colour nobody else uses (GDD §C4) — actual values are art direction (Irene/Gus), placeholders until the palette lands.
  - `Sprite _portrait` — dialogue bust; optional, null until Irene's art lands
  - `Sprite _worldSprite` — in-room body; optional, independent of the portrait so either can land first
  - `bool _leaksToNotGrandma` — **on only for the Uncle**, fixed (day-2 §A7)
  - `ExchangeEntryData[] _exchangeEntries`
  - `ClueSO _fallbackReturnClue` — optional (null = unmapped shares return nothing)
  - `bool OffersExchange` — computed: any entries or a fallback. Not Grandma's is `false`, so the share prompt never opens after talking to her.
  - `NpcProfile ToProfile()` — builds the domain profile (ids only), same pattern as `DialogueSO.ToGraph()`.
  - `bool TryResolveClue(string returnedClueId, out ClueSO returnedClue)` — maps an id the Domain chose back to its asset, matching the **returned** side of each entry (and the fallback). Keeps that resolution inside the asset that produced the id; no global clue catalog to maintain.
- `ExchangeEntryData` (`[Serializable]`): `ClueSO _givenClue`, `ClueSO _returnedClue`.

## Changes to the dialogue system (plan 02)
The speaker contract moves from strings to assets. Colour and portrait have exactly one source of truth: the `NpcSO`.

- `DialogueSO`: gains `bool _allowsClueExchange` (public `AllowsClueExchange`). **Off by default** — a conversation offers a trade only when deliberately marked, so intros and story beats stay pure narration and nothing has to be un-marked later.
- `DialogueNodeData`: **`_speakerName` (string) is removed**, replaced by `NpcSO _speaker` — set on `Npc`-type nodes, left null for `Player` and `InnerMonologue`. The Domain is untouched (still pure topology); no dialogue test changes.
- `DialogueView`:
  - `Npc` nodes: speaker label shows `_speaker.DisplayName`, and **both the label and the line text are tinted `_speaker.Color`**. The serialized `_npcStyle` block stays only as the fallback for an `Npc` node whose `_speaker` was left unwired (authoring slip — render, don't throw).
  - `Player` / `InnerMonologue` nodes: unchanged — their serialized style blocks still apply, monologue still hides name and portrait.
  - The `_speakerPortrait` GameObject slot becomes an `Image _portraitImage`: shows `_speaker.Portrait` on `Npc` nodes that have one, hidden otherwise (null portrait, `Player`, `InnerMonologue`).
- **Asset migration:** `DLG_Test_Linear` and `DLG_Test_Branching` carry `_speakerName` strings; after the field swap their `Npc` nodes must be re-wired to `NpcSO` references in the inspector (checklist step 5).

## Presentation
Namespace `Game.Presentation`. New folder `Presentation/Exchange/`.

| Component | Folder | Responsibility | Channels |
|---|---|---|---|
| `NpcInteractable` *(modified)* | `Presentation/Dialogue/` | Gains `[SerializeField] NpcSO _npc` and the engaged channel. On interact: raise `NpcEngaged(_npc)` then `DialogueRequested(_dialogue)`. | raises `NpcEngaged`, `DialogueRequested` |
| `NpcVisual` | `Presentation/Dialogue/` | Tiny component next to `NpcInteractable`: in `Awake`, assigns `_npc.WorldSprite` to the cached `SpriteRenderer` when the sprite exists (placeholder sprite stays otherwise). Keeps the in-room look sourced from data instead of hand-synced per scene object. | — |
| `ExchangeController` | `Presentation/Exchange/` | Owns the `ExchangeLog`. Listens to `NpcEngaged` (stores the current `NpcSO`) and `DialogueFinished` (opens the share prompt only when **all three** hold: an NPC is current, `dialogue.AllowsClueExchange`, and `npc.OffersExchange`). `DialogueFinished` already carries the `DialogueSO`, so the dialogue gate needs no new channel. The two gates answer different questions — *"is this the kind of conversation where you'd offer a trade?"* vs *"does this character have anything to trade?"* — and Not Grandma fails the second no matter how a dialogue is marked. Public `Share(ClueSO given)`: calls `ExchangeLog.Share(npc.ToProfile(), given.Id, given.RoomId)`; on `Accepted` raises `ClueShared`, resolves and raises `ClueCollected(returned)` when there is a return, raises `RoomLeaked` when `LeakedNewRoom`. Exposes `IsExchangeActive` (for `ClickRouter`) and forwards `LeakedRooms` from the log (read-only surface the night pulls at nightfall — the leaked set is never duplicated outside the Domain). Builds the `NpcProfile` once per engagement, not per share. | listens `NpcEngaged`, `DialogueFinished`; raises `ClueShared`, `ClueCollected`, `RoomLeaked` |
| `ClueSharePanelView` | `Presentation/UI/` | Drag-and-drop panel *(revised Aug 27 — replaces the two-step button picker)*. Shows the NPC's portrait + name (tinted `NpcSO.Color`) and a **drop slot to the left of the portrait**. One `DraggableClueItem` per notebook clue (from `NotebookController.GetCollectedClues()`, instantiated on open — allocation on open, never per frame); clues already shared with this NPC render dimmed and refuse to drag. Dropping a clue on the slot calls `ExchangeController.Share(clue)`, clears the list and shows the returned clue's text (or `[NOTHING_IN_RETURN]`, serialized placeholder). Close button → `CloseExchange()`. Syncs visibility to `ExchangeController.OnExchangeStateChanged`. | — (reads controller) |
| `DraggableClueItem` | `Presentation/UI/` | Clue entry prefab component (`CanvasGroup` required): on begin-drag spawns a non-raycast-blocking ghost copy under the root canvas that follows the pointer; destroys it on end-drag. Non-draggable items cancel the drag by nulling `eventData.pointerDrag`. | — |
| `ClueDropSlot` | `Presentation/UI/` | `IDropHandler` on the drop zone: reads the `DraggableClueItem` from `eventData.pointerDrag` and forwards it to the panel. | — |

`ExchangeController` API notes (drag & drop revision): `Share` **returns the resolved returned `ClueSO`** (null when nothing came back) so the panel can show the result without re-listening to `ClueCollected`; the exchange stays active until the panel calls `CloseExchange()` — that keeps `ClickRouter`'s guard blocking world clicks while the result is on screen. `OnExchangeStateChanged` (C# event, same pattern as `DialogueController.OnNodeChanged`) drives the panel's visibility; `CurrentNpc` and `HasSharedWithCurrentNpc(clue)` feed the header and the dimming.

`ClickRouter` change (Gus): the dialogue guard extends to `_exchangeController.IsExchangeActive` — while the share panel is open, world clicks do nothing and clicks don't advance anything.

Kept out of Domain deliberately: *which* clue the Cousin's evidence beat needs, when NPCs are available, where they stand — all of that is content (`NpcSO` assets) or plan 04's problem. The Domain never learns who "the Uncle" is; it only sees a profile with a flag.

## Editor setup checklist
All manual work by Gus:

1. Create folder `Assets/Game/Scripts/Presentation/Exchange/` and `Assets/Game/ScriptableObjects/Npcs/`.
2. After writing the code: channel assets in `ScriptableObjects/Channels/` → `CH_NpcEngaged`, `CH_ClueShared`, `CH_RoomLeaked` (menus `Game/Events/...`).
3. Create the **4** NPC assets (`Game/Data/Npc`): `NPC_Mother`, `NPC_Uncle` (`_leaksToNotGrandma` **on**), `NPC_Cousin`, `NPC_NotGrandma` (empty exchange table, no fallback). Wire placeholder display names and pick a placeholder `_color` per character — distinct enough to tell apart in dialogue; real palette values come with Irene's art (Not Grandma's must be the colour nobody else uses, GDD §C4).
4. Give each family NPC 1–2 `ExchangeEntryData` rows using the existing test clues (create 2–3 more `ClueSO` if needed, one with `_isEvidence` on for the Cousin's evidence return) and a fallback clue for at least one NPC, so mapped, fallback and no-return paths are all reachable in the sandbox.
5. **Dialogue migration:** after the `DialogueNodeData` field swap, re-wire the `Npc` nodes of `DLG_Test_Linear` (Mother) and `DLG_Test_Branching` (Cousin) to their `NpcSO`s. In the dialogue panel, replace the `_speakerPortrait` slot with an `Image` and wire it to the view's `_portraitImage`. Tick `_allowsClueExchange` on **`DLG_Test_Branching` only**, leaving `DLG_Test_Linear` unmarked — that way the sandbox exercises both the prompt and the no-prompt path.
6. On the `Speaker` object (and any new NPC placeholder): assign its `NpcSO` + `CH_NpcEngaged`, and add `NpcVisual` (reuses the placeholder sprite until `_worldSprite` is set). Canvas (drag & drop revision):
   - Share panel root with: portrait `Image` + name TMP, **drop zone `Image` to the left of the portrait** with `ClueDropSlot` (wire its `_panel` ref; make sure its Image has *Raycast Target* on), a vertical container for clue items, result TMP, Close button.
   - Clue item prefab: background `Image` (*Raycast Target* on — the drag starts here) + `CanvasGroup` + `DraggableClueItem` + child TMP label wired to `_label`.
   - `ClueSharePanelView` wired to both controllers and all the above. The `EventSystem` must be using **`InputSystemUIInputModule`** (not the legacy `StandaloneInputModule`) or no drag event will ever fire.
7. Wire `ExchangeController` into `ClickRouter`'s guard. Verify: while the share panel is open, clicking the room does nothing.
8. Smoke test: talk to the Uncle placeholder, share a kitchen clue → notebook gains the returned clue, sharing the same clue again is disabled, and `LeakedRooms` (inspector debug or breakpoint) contains `Kitchen`. Share with the Cousin → no leak. Finish an **unmarked** dialogue (`DLG_Test_Linear`) → no prompt appears and the world is clickable again immediately.
9. Run Test Runner → EditMode until `ExchangeTableTests` + `ExchangeLogTests` are green.

## Tests
`Assets/Tests/Editor/ExchangeTableTests.cs` and `Assets/Tests/Editor/ExchangeLogTests.cs` (written by Claude, start red until Domain exists):

**ExchangeTable**
- `Ctor_NullPairs_Throws`
- `Ctor_NullOrEmptyKeyOrValue_Throws`
- `TryGetReturn_MappedClue_ReturnsMappedClue`
- `TryGetReturn_UnmappedClue_ReturnsFallback`
- `TryGetReturn_UnmappedClueWithoutFallback_ReturnsFalse`

**ExchangeLog**
- `LeakedRooms_NewLog_IsEmpty`
- `Share_FirstTime_AcceptedWithMappedReturn`
- `Share_UnmappedClue_AcceptedWithFallbackReturn`
- `Share_UnmappedClueWithoutFallback_AcceptedWithNoReturn`
- `Share_SameClueSameNpc_AlreadySharedAndStateUnchanged`
- `Share_SameClueDifferentNpc_Accepted`
- `Share_WithLeakerNpc_LeaksClueRoom`
- `Share_WithLoyalNpc_DoesNotLeak`
- `Share_SecondClueFromSameRoomWithLeaker_ReportsNoNewLeak`
- `Share_NullNpc_Throws`
- `Share_NullOrEmptyClueId_Throws`
- `HasShared_ReflectsShareHistory`

## Out of scope
NPC movement/placement between rooms and availability windows (plan 04 — `StoryDirector`; Janhavi's navigation), story-state selection of dialogues, the night patrol that consumes `LeakedRooms` (Janhavi — this plan only provides the read-only surface + `RoomLeaked` channel), trust system and favors (Nice-to-Have), the portrait/world/palette **art itself** (Irene — this plan only gives it slots in `NpcSO`), police accusation flow, real written text (humans only).
