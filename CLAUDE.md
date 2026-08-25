# CLAUDE.md — That Is Not Grandma

Context file for Claude Code. Read this before doing anything in this repo.

## What this project is

Point-and-click mystery for **Brackeys Game Jam 2026.2** (theme: *Trust No One*). Jam runs Aug 23–30, 2026; upload deadline **Sun Aug 30, 10:00** (verify Chilean local time), build must be on itch.io ≥2h before. **WebGL is the primary target.**

Premise: the protagonist's grandma has been replaced by a doppelgänger. Gather clues by day, trade clues with family NPCs, survive the night while Not Grandma patrols, then call the police with the right evidence. One of the NPCs (the **Uncle — fixed, never randomized**) secretly leaks every clue you share with him to Not Grandma, which biases her night patrol. The player is never told who the ally is.

- **Team:** Gus (engineering, audio, final design call), Janhavi (gameplay dev), Irene (art).

### Design docs — read these before planning any feature

| Doc | What it is |
|---|---|
| `Docs/GDD.md` | Full game design document (snapshot of the team's Google Doc). Mantra, pillars, story, core loop, features table, art & audio direction, win/lose. |
| `Docs/day2-plan.md` | Scope cuts and decisions locked on Aug 24. **Authoritative where it disagrees with the GDD.** |

Locked scope: **4 rooms** (Kitchen, Living room, Bedroom, Garden), **4 characters** (Not Grandma, Mother, Uncle, Cousin), 1 day + 1 night, 2 hiding spots (under bed, closet), 2 police "lives", one 8–12 min run.

`Docs/GDD.md` is a copy — the team edits the Google Doc. If Gus mentions a design change that isn't in these files, update them as part of the feature work.

## Jam rules that constrain Claude

- **No AI-generated content**: art, audio, music, voice, **and game text**. Claude must NOT write dialogue, clue texts, item descriptions, or any player-facing prose. Placeholder strings like `"[UNCLE_HINT_01]"` are fine; real writing is done by humans.
- Pre-made assets need a license + credit in `CREDITS.md` (keep it updated from day 1).
- After the deadline: bugfixes only, no new content.

## Tech stack

- Unity **6000.5.9f1**, URP 2D (Renderer2D), **Input System** (`Assets/Settings/InputSystem_Actions.inputactions`), uGUI, TextMeshPro, **Unity Test Framework 1.7**.
- Delivery: WebGL on itch.io (Windows build only as a bonus).
- Repo is Gus's fresh 2D project (Janhavi's original repo used a different Unity version). No gameplay code yet — this file defines the structure everything new must follow.

## Division of labor — non-negotiable

**Claude Code does:** feature analysis, development plans, EditMode tests, doc updates, code review on request.
**Gus (human) does:** all production code, all Unity Editor work (scenes, prefabs, inspector wiring, sizes/positions), all asset creation.

Claude must not write production code or edit scene/prefab/asset YAML unless Gus explicitly asks for that specific thing in that message. Never modify `.unity`, `.prefab`, `.asset`, `.meta`, `ProjectSettings/` or `Packages/` files — editor work is done by hand in the editor.

## Feature workflow

Every new feature follows this loop:

1. **Analyze** — read this file, the relevant plan docs in `Docs/plans/`, and existing code under `Assets/Game/`. Restate the feature in terms of the architecture below (domain rules → events → data → presentation).
2. **Ask** — before writing the plan, ask Gus every question needed to remove ambiguity (scope, edge cases, what's Must-Have vs Nice-to-Have, what already exists in the editor). Do not skip this step even if the feature seems obvious.
3. **Plan** — write `Docs/plans/NN-feature-name.md` using the template below.
4. **Tests + docs** — Claude writes the EditMode tests for the domain logic (they start red) and updates any doc the feature invalidates.
5. **Implement** — Gus writes the code and does the editor setup until tests are green. Claude answers questions and reviews diffs when asked.

### Plan template (`Docs/plans/NN-feature-name.md`)

```markdown
# NN — Feature name
**Goal:** one sentence. **Priority:** Must-Have / Nice-to-Have (per GDD §5)
## Domain
New/changed pure-C# types and rules (no UnityEngine).
## Events
Event channels raised/listened, with payload types.
## Data
ScriptableObject data assets needed (type + instances to create).
## Presentation
MonoBehaviours: name, responsibility, which channels they use.
## Editor setup checklist
Step-by-step manual work for Gus: assets to create (with menu path),
prefabs, scene objects, inspector references to wire.
## Tests
List of EditMode test cases (given/when/then).
## Out of scope
What this plan deliberately does not touch.
```

## Architecture

Pragmatic clean architecture. No assembly definitions — discipline by folders, namespaces and reviews.

**Dependency rule (one direction only):**

```
Presentation (MonoBehaviours, UI, audio players)
    ↓ may reference
Events (SO event channels)  +  Data (SO data assets)
    ↓ may reference
Domain (pure C#, NO UnityEngine — except attributes like [Serializable])
```

- **Domain** — game rules as plain C#: clue graph, leak tracking, police trust, patrol bias, day/night state machine. Deterministic, constructor-injected, fully testable without a scene. Randomness enters via an injected `System.Random` or interface.
- **Data** — `ScriptableObject`s as immutable-at-runtime content: `ClueSO`, `NpcSO`, `RoomSO`, dialogue references. Data, not behaviour.
- **Events** — ScriptableObject event channels (below). The only way Presentation components talk to each other.
- **Presentation** — thin MonoBehaviours that translate input/engine callbacks into domain calls, and domain results into visuals/audio. No game rules here.

### Folder layout

```
Assets/
  Game/
    Scripts/
      Domain/            # pure C#
      Data/              # ScriptableObject definitions
      Events/            # event channel SOs + listener helpers
      Presentation/      # MonoBehaviours, grouped by feature (Rooms/, Clues/, Night/, UI/, Audio/)
    ScriptableObjects/   # .asset instances (Clues/, Npcs/, Rooms/, Channels/)
    Prefabs/
    Scenes/
    Art/  Audio/
  Tests/
    Editor/              # EditMode tests (compiled into Assembly-CSharp-Editor, no asmdef needed)
  Settings/              # URP + Input (already exists — don't touch)
```

### ScriptableObject event channels

One channel asset per game signal, listed in the feature plan. Pattern:

```csharp
// Events/ClueCollectedEventChannelSO.cs
[CreateAssetMenu(menuName = "Game/Events/Clue Collected")]
public class ClueCollectedEventChannelSO : ScriptableObject
{
    public event Action<ClueSO> Raised;
    public void Raise(ClueSO clue) => Raised?.Invoke(clue);
}
```

Rules: raisers and listeners reference the channel via `[SerializeField]`, wired in the inspector — never `Resources.Load` or static access. Listeners subscribe in `OnEnable`, **always** unsubscribe in `OnDisable`. Channel assets live in `Assets/Game/ScriptableObjects/Channels/`. Use a `VoidEventChannelSO` for payload-less signals (e.g. `NightStarted`). No singletons, no static event buses, no `FindObjectOfType`.

### Code style

- C# conventions: PascalCase types/methods, `_camelCase` private fields, `[SerializeField] private` over public fields.
- SOLID applied Unity-style: small components with one responsibility, composition over inheritance, depend on interfaces/channels rather than concrete scene objects.
- No `GetComponent`/allocation in `Update`; cache in `Awake`. No `Camera.main` in hot paths.
- Presentation reads input only through the Input System actions asset.
- Keep everything WebGL-safe: no threads, no `System.IO` at runtime, audio must start after first user click (browser autoplay policy).

## Testing

- **EditMode only, domain only** — that's the deal. No PlayMode tests during the jam.
- Tests live in `Assets/Tests/Editor/`, one file per domain type: `LeakTrackerTests.cs`, etc.
- Naming: `MethodOrRule_Scenario_ExpectedResult`. Arrange/Act/Assert.
- Run: Unity → Window → General → Test Runner → EditMode, or CLI:
  `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults .\test-results.xml -quit`
- Domain code must stay runnable without a scene; if a test needs a MonoBehaviour or an `.asset` instance, the design is wrong — push the logic down into Domain.

## Git

- **Branching:** feature branches off `dev`, PRs to merge back into `dev`. Branch naming: `feature/short-description` (e.g. `feature/clue-collection`, `feature/night-patrol`). Never commit directly to `main` or `dev`.
- **PR flow:** once a feature branch is ready (tests green, editor setup done), open a PR into `dev`. Claude can help draft the PR description when asked.
- **Commit style:** small, frequent commits; message format `area: what changed` (e.g. `night: patrol bias by leaked rooms`).
- Unity `.gitignore` is set up; never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, generated `.csproj`/`.slnx`.
- Art goes through Git LFS once configured — check before committing large binaries.

## Priorities when in doubt

1. A start-to-finish playable build beats any elegant subsystem — vertical slice deadline is **Wed Aug 26**.
2. The features table in the GDD (Must-Have column) is the authority; anything else is Nice-to-Have and needs Gus's call.
3. WebGL build health is sacred: if a choice risks build size or browser audio, flag it in the plan.
