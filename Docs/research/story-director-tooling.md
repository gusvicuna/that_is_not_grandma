# Story director tooling research — build it or use a tool?

> Written Fri Aug 28, before writing `Docs/plans/04-story-director.md`. Question asked: **what already exists in Unity 6 / the Asset Store / the ecosystem that would let us skip writing the story director in plan 04?**
> Budget when this was written: **a few hours, today.** Every verdict below is a verdict *at that distance from the deadline*, not a verdict on the tool.

## What the feature actually is

A story beat is `trigger → conditions → effects`:

- **triggers** (all already exist as channels): `CH_ClueCollected`, `CH_ItemInspected`, `CH_DialogueFinished`, `CH_ClueShared` (NPC + clue), `CH_RoomChanged`, `CH_DayStarted`.
- **conditions**: flags set by earlier beats, current day, whether the beat already fired.
- **effects**: show / hide / move an NPC between the 4 rooms, add or remove an interactable item, swap which `DialogueSO` an NPC will play next, start a dialogue, raise a channel (`CH_TensionChanged`, `CH_NightStarted`).

Plus the day clock — real-time countdown *and* time charged per significant action — because the Aug 26 amendment puts it inside this system, and plan 06 already waits for this feature to raise `CH_DayStarted` so its `DebugDayAdvancer` can die.

## The finding that decides almost everything

**The graph is the cheap half; the effects are the work.**

Every tool in the table below ships the same thing: a way to author `when X and Y, do Z` without writing the plumbing. That plumbing is roughly **80 lines** of pure C# for us — a dictionary of beats, a switch on the trigger, a `HashSet` of already-fired ids. What none of them ships is the *right-hand side*: "move NPC_Uncle to the Kitchen", "make NPC_Cousin's next click play `DLG_Cousin_02`", "put the phone in the Living room". In every tool that is a **custom node / custom action class you write yourself**, in that tool's idiom, against that tool's lifecycle — Quest Machine actions, Unity Behavior nodes, Visual Scripting units, Fungus commands.

So the trade on offer is: pay integration cost + a new idiom + (sometimes) money, to skip 80 lines, and still write the other ~500 yourself with less freedom and no EditMode tests. That trade is bad on a normal week. Two days from close it isn't a trade at all.

The second finding is smaller but real: **our channels already are the trigger bus.** Any external tool would either need adapters from our channels into its own event system, or would want to own the events itself — and `CLAUDE.md`'s "no singletons, no static event buses" rules out the latter.

## Comparison

| Option | What it would replace in plan 04 | Pros | Cons | Verdict |
|---|---|---|---|---|
| **Homemade `StoryDirector`** (pure C# Domain + `StoryBeatSO` assets) | — (it *is* plan 04) | Follows the Domain→Events→Presentation rule already in the repo; beat matching is EditMode-testable without a scene; effects hold direct `NpcSO`/`ClueSO`/`DialogueSO` references, no string lookups; ~610 lines, no new dependency, no WebGL risk, no `CREDITS.md` row | We write and debug it; no visual graph of the story (the flowchart stays in `.drawio`) | ✅ **Adopt** — costed below |
| **Unity Behavior** (`com.unity.behavior`, free, official, Unity 6) | The beat evaluation and its authoring | Free and first-party; graph editor with runtime debugging; event-driven nodes; subgraphs | It's a *behavior tree for an agent*, not a global rules table — "any of 6 triggers, in any order, once each" fights the paradigm; every effect is a custom node class; graph assets can't be unit-tested in EditMode; **its WebGL build was broken until 1.0.15 (Feb 2026)** by a leftover `Unity.Muse.Chat` reference — a sign the Web path is lightly travelled | ❌ **No** — wrong shape, and a new package on build day |
| **Unity Visual Scripting** (built-in) | Same | Already in the editor, nothing to install; **Web is a supported AOT platform** and the AOT pre-build has been automatic since 1.5.1 | Adds an AOT pre-build step to every WebGL build we make on the last two days; the logic lives in scene graphs → untestable and merge-hostile; explicit collision with the Domain rule in `CLAUDE.md`; dragging nodes for ~10 beats is slower than typing them into SO assets | ❌ **No** |
| **Fungus** (free, MIT, flowchart narrative) | The beat evaluation *and* the dialogue system | Purpose-built for exactly this genre; visual flowcharts; community reports Unity 6 / 6.2 working with no compile errors (Fungus CE) | Wants to own dialogue and its UI — we already shipped `DialogueRunner`, `DialogueView` and the exchange panel and would be throwing them away; known friction on Unity 6 (pink materials in samples, legacy input); flowchart state lives in the scene | ❌ **No** — this was a day-0 decision, not a day-6 one |
| **Ink** (inkle, free) | The whole story state machine | A real story VM: variables, conditions, `EXTERNAL` functions to fire our effects, variable observers to read state back | Already ruled out for dialogue on Aug 26; the story would live in `.ink` files = a second source of truth next to our SOs; the compiler ships in the build unless split into an editor-only asmdef, and **we have no asmdefs by design**; new toolchain | 🔜 **Post-jam** |
| **Yarn Spinner 3** (free) | Same | `<<commands>>` map beautifully onto our effects (`<<move_npc Uncle Kitchen>>`), `once` and detours are made for intro beats; `[YarnCommand]` wiring is minimal | Same objections as Ink, plus it was already evaluated and rejected Aug 26; WebGL support is not documented as clearly as we'd need on the last day | 🔜 **Post-jam** |
| **Quest Machine** (Pixel Crushers, $71.50) | Beat evaluation, conditions, actions, and a quest UI | Literally a trigger/condition/action machine with an editor; **explicitly tested on WebGL**; full source included | Costs money to remove ~80 lines; brings its own message system, save system and UI conventions; every effect is still a custom action class; a whole manual to read today | ❌ **No** — the best paid candidate, wrong week |
| **Adventure Creator** ($80) | Beats, navigation, items, dialogue, cursors — everything | ActionLists are *precisely* this feature, in *precisely* this genre; supports WebGL | It's an engine, not a library: adopting it means re-doing room navigation, clues, notebook, dialogue and the exchange on its terms, 2 days from close, with 5 plans already implemented | ❌ **Dead on arrival** at this date — worth a serious look before the *next* point-and-click |
| **NodeCanvas / Behavior Designer** (paid, ~$60–80) | Beat evaluation as FSM/BT | Mature, fast, good editors | Same paradigm mismatch as Unity Behavior, plus money, plus a third-party dependency in the last build | ❌ **No** |
| **Timeline + Signals** (built-in) | Ordered story sequences | Visual, no code, artist-friendly | Authored and *linear*; our beats are reactive and conditional, fired in whatever order the player roams; untestable in EditMode | ❌ Not for the director — ✅ fine later if a scripted intro cutscene appears |
| **UnityEvents wired in the scene** (no code at all) | The whole director | Zero code; Gus wires it in the inspector | Cannot express "day ≥ 2 **and** flag X **and** not fired yet"; state in the scene = no tests, painful merges with Janhavi's branch; the exact "logic in Presentation" the architecture forbids | ❌ **No** as the system — ✅ fine for one-off cosmetic triggers |
| **articy:draft X** + importer (free tier) | Authoring the story graph outside Unity | Good for a team with three writers; export → import pipeline | The story is ~10 beats; the flowchart already exists in `.drawio`; a whole authoring tool for a diagram we have already drawn | ❌ **No** |
| **A small custom inspector for `StoryBeatSO`** (ours, ~30 min) | Nothing — it makes our own assets readable | Each beat reads as one line in the Project window; catches empty references before play mode | Half an hour we may not have | 🟡 **Nice-to-Have**, only if the core lands early |

## What it costs to build ourselves

| Piece | Files | ~LOC | EditMode tests |
|---|---|---|---|
| `StoryBeat`, `StoryEvent`, `StoryTrigger`, `StoryEffectKind` (pure data) | 4 | ~120 | — |
| `StoryDirector` (matching, conditions, once-only, flags, day) | 1 | ~90 | ~8 |
| `DayClock` (real time + per-action cost + expiry) | 1 | ~60 | ~5 |
| `StoryBeatSO` + `StoryEffectData` + `StoryConditionData` | 3 | ~90 | — |
| `StoryDirectorBehaviour` (subscribes to the 6 channels, applies effects) | 1 | ~110 | — |
| `StoryActor` + registry (id → scene object, per room) | 2 | ~60 | — |
| `DayClockController` (ticks, charges actions, raises `CH_DayStarted` / `CH_NightStarted`) | 1 | ~70 | — |
| `NpcInteractable.SetDialogue` (small patch to an existing file) | 1 | ~10 | — |
| **Total** | **14** | **~610** | **~13** |

Roughly **3–5 hours** including the editor setup (beat assets + tagging the NPC/item objects). Every hour of that is spent on our rules, not on someone else's API.

**Presence model (locked with Gus, Aug 28):** NPCs and items are **pre-placed in every room where they can appear and toggled with `SetActive`**. "Move NPC" = deactivate in room A, activate in room B. No `Instantiate`, no prefab loading, no anchors — which also removes the only WebGL risk this feature had.

## Recommendation (accepted)

Write it. The comparison is not "we enjoy writing code": it is that on this schedule every tool charges integration time to remove the 80 lines we are most sure about, and leaves us writing the 500 we are least sure about anyway — in an idiom that cannot be tested in EditMode, which is the one testing rule this project actually kept.

Three things to take from the tools instead of the tools themselves:

1. **Yarn Spinner's `once`** — beats are one-shot by default, with an explicit `Repeatable` flag for the few that are not. Cheapest way to make the free-roam intro (Aug 26 amendment §3) behave.
2. **Quest Machine's split of condition vs action** — keep effects as *data* (a `StoryEffectData` array on the beat), never as code branches inside the director. New story content then never touches C#.
3. **Ink's variable observers** — the director owns a small flag set and nothing else reads it directly; anything that must react does so through an existing channel.

## Post-jam upgrade path

If this game continues past the jam, the order is: **Ink or Yarn Spinner for the story state** (they beat a hand-rolled flag set the moment the story passes ~20 beats), and **Adventure Creator evaluated before writing a single line** of the next point-and-click. Quest Machine only if the game grows actual quests. Unity Behavior only when there is a real agent with a real behaviour — if Not Grandma's night patrol ever comes back, that is where it belongs.

## Sources

- [Unity Behavior manual](https://docs.unity3d.com/Packages/com.unity.behavior@1.0/manual/index.html) · [Behavior changelog 1.0.15 — WebGL build fix](https://docs.unity3d.com/Packages/com.unity.behavior@1.0/changelog/CHANGELOG.html)
- [Visual Scripting — Building for AOT platforms](https://docs.unity3d.com/Packages/com.unity.visualscripting@1.6/manual/vs-aot.html)
- [Fungus releases](https://github.com/snozbot/fungus/releases) · [Unity 6 compatibility report (Ink-Fungus Gateway)](https://itch.io/t/4855071/update-of-the-tool-compatibility-with-unity-6)
- [ink-unity-integration](https://github.com/inkle/ink-unity-integration) · [Observing ink variables](https://videlais.github.io/learning-ink-unity/chapters/chapter16/)
- [Yarn Spinner — Commands and Functions](https://docs.yarnspinner.dev/yarn-spinner-for-unity/creating-commands-functions)
- [Quest Machine (Asset Store)](https://assetstore.unity.com/packages/tools/game-toolkits/quest-machine-39834) · [Adventure Creator (Asset Store)](https://assetstore.unity.com/packages/tools/game-toolkits/adventure-creator-11896)
