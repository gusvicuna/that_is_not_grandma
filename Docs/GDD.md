# GDD — Brackeys Game Jam 2026.2: "Trust No One"

> **Snapshot of the team's shared GDD** (Google Docs, owned by Gus), copied into the repo so Claude Code has it offline.
> **⚠️ Where this doc and `Docs/day2-plan.md` disagree, the day-2 plan wins** — it contains the scope cuts made on Aug 24.
> **Jam:** Aug 23, 10:00 → Aug 30, 10:00 · [itch.io/jam/brackeys-16](https://itch.io/jam/brackeys-16)
> **Team:** Gus (engineering + sound & music direction), Janhavi (development), Irene (art direction)

---

## 0. Jam Requirements (non-negotiable)

**Official theme: `Trust No One`**

One sentence for how the player *experiences* distrust: *"Sharing information with the wrong person can make your survival and progress harder."*

**Brackeys hard rules:**

- **No AI-generated content** — art, audio, music, voice, **text**. Every asset needs a documented human origin.
- No NSFW or offensive content.
- Pre-made assets allowed with valid license + explicit credit → keep `CREDITS.md` from day 1.
- The game must be mostly original.
- No external download links — everything hosted on itch.io.
- Itch page created and build uploaded **at least 2 hours before** the deadline.
- After the deadline: 48h window for **bugfixes and extra platform builds only**, no new content.
- **Minimum 20 ratings** required to be eligible (see §13).

**Engine/stack:** Unity · **Delivery:** WebGL primary, Windows as bonus.

⚠️ A download-only build drastically cuts how many people try the game. WebGL is the priority, the .exe is the bonus.

## 0.b Voting Criteria

Brackeys scores across 6 categories: Enjoyment, Gameplay, Innovation, Theme, Visuals, Audio.

**Our bets: Theme + Visuals + Audio** (per day-2 plan — Irene on art, Gus on audio).

---

## 1. Mantra

> *"A point-and-click game where the player's grandma has been replaced by a doppelgänger who has allies in their family, and the player must find evidence to call the police — without knowing who is her ally and who is on the player's side."*

## 2. Design Pillars

1. Information is crucial, but the player **can't truly trust anyone**.
2. The player should feel **unease** — something completely strange and dangerous is in their safe space.
3. The player should feel they have **sharp eyes** for finding clues and a **sharp mind** for combining information to solve a mystery.

## 3. Story Summary

Your **Grandma** went out with her friends from the elderly club in town, but when the van dropped her off, she looked… different. No one else in your family seemed to notice, but you did. It's almost as if they're too scared to admit it.

The next day you're watching your "Grandma" do the dishes in the kitchen sink; she still looks off. You hear a gasp behind you and turn to see your **cousin** run away behind the corner. You follow her, and she tells you that you were right: that thing is not Grandma.

Sneak around the house, avoid the **Doppelgänger**, and find out where they took Grandma and why. Did someone in your family plan this? Your **mother**, **uncle** and **cousin** are all roaming the house — choose who to trust very carefully.

---

## 4. Core Loop

1. **Observe** — gather clues in the house.
   - Gather clues · combine clues into better clues · gather items · use items to access new places · activate things.
2. **Talk and exchange** information or favors with family members — **← theme test**
   - Free information by just talking.
   - Better clues by: selecting gathered information to trade · gathering specific items · activating something specific.
   - Increase their trust in you.
3. **Observe the reaction** of the exchange.
   - New items may appear in the house · Not Grandma may change behaviour · members may move rooms · members may change attitude toward you.
4. **Hide from Not Grandma until the night ends.**
   - Rooms the player can lock · places to hide · other members with enough trust to help you hide.
5. **Optionally call the police** to accuse a family member, presenting evidence.
   - Not enough evidence → lose 1 police trust. No trust left → lose the game.
   - Enough evidence → that member is taken from the house:
     - An **ally of yours** → game gets harder, Not Grandma's behaviour escalates.
     - A **hostage** → Not Grandma's behaviour gets slightly harder.
     - A **helper of Not Grandma** → new clues/evidence appear for accusing Not Grandma.
     - **Not Grandma** → the player wins.
6. Start a new day → back to 1.

**Theme test:** the player feels they can't trust *when they give information to a family member and that member acts against them.*

## 4.b How We Generate Distrust

**Chosen:** a **traitor NPC hidden among many**, where you must choose who to trust and it sometimes costs you dearly.

**Connection to the loop:** the player needs help from NPCs to get new clues or to have something done, but must give information or favors in exchange — which makes the game harder if that NPC is an enemy.

---

## 5. Features

| Must-Have (no game without it) | Nice-to-Have (if time allows) |
|---|---|
| Clues/evidence notebook system | Gather/use items (inventory system) |
| House system: room connections with items, characters and Not Grandma | Clue combining |
| Characters | Trust system |
| Dialogue system | NPC favors (gather items or activate something in exchange for information) |
| Dialogue trees for each NPC | Night — locking rooms with items |
| Story progression system: flags + triggered beats driving the story flowchart (`Docs/Not grandma's story line.drawio`) — *added Aug 26* | Night — ask for help from trusted NPCs |
| Information exchange using clues/evidence (per-NPC clue→clue map; sharing keeps the clue, once per NPC; leaks born here — *Aug 27*) | |
| Player navigation in the house (Input System click) | Calling the police accusing *any* NPC, with evidence and consequences |
| Player interaction with items (Input System click): activate text or gather clues | Cutscenes |
| Movement of NPCs, Not Grandma and items around the house | Dialogue options — *promoted Aug 26 as cosmetic branching only: options change which lines play, never game state* |
| Calling the police selecting evidence (**Not Grandma as the only suspect for now**) | Traitor changes between runs |
| Police trust (like lives) | |
| Time system for day/night (mixed: real-time base + per-action time costs — *Aug 26*) | |
| Night — hideable places (Input System click) | |
| Night — Not Grandma behaviour | |
| Night — losing | |
| Audio system | |

**Jam rule:** Must-Haves must be playable start to finish by day 4.

## 6. Interface / Controls

- **Input:** keyboard + mouse.
- **Core controls:** click for action (item, navigation, NPCs), `Esc` to pause, `Tab` for notebook.
- **Minimum screens:** Start, Pause, Game Over/Win.
- **Teaching the rules:** first items glow; custom arrow cursor for navigation and items.

## 7. Art — Irene

- **Style:** mixed media —
  - *Backgrounds*: low-res / pixelated photographs with a gradient map (locked palette).
  - *Characters*: low-resolution illustration, light rendering (locked palette).
  - *UI/GUI*: low-resolution illustration, simple animations (locked palette).
- **Original scope (pre-cut):** 5 characters, 7 backgrounds, 3 hiding spots — Grandma ✅, Not Grandma ✅, Uncle, Cousin, Mother · Kitchen, Living room, Bedroom, Study, Bathroom, Garden · Door (multi-frame open/close) · hiding under bed / in closet / in shower · cursor (regular, click, turn left, turn right).
  - **Cut (day 2, revised Aug 25):** Study and **Garden** rooms, shower hiding spot. The 4 shipping rooms are Kitchen, Living room, Bedroom, **Bathroom**. Real Grandma appears only in the win screen (static, no walk cycle).
- **Visual language of deception (proposal, day-2 plan §C4):** every character uses the locked palette, but **Not Grandma uses one colour nobody else in the house uses** — subtle, never explained.

## 8. Sound & Music — Gus

- **Direction (locked day 2):** quiet house + tension layers. Near-silent ambience (clock, pipes, distant TV); music enters as layers when Not Grandma approaches or when someone lies to you.
- **Audio as a vector for the theme:** footsteps that belong to nobody, a music layer that appears when something is lying, silence as a signal, a motif tied to the traitor that plays *before* the betrayal. Audio mirror of the art hook: one instrument only Not Grandma brings with her.
- **Critical SFX for the core loop:** main action · reward/correct call · suspicion/alert · betrayal/reveal · error or loss.
- **Technical note:** keep audio modular in layers (stems that toggle) instead of long single tracks, so it reacts to the suspicion state without re-recording.
- **Rules:** no AI-generated music or voices. Licensed assets credited in `CREDITS.md`.
- **Pre-upload checklist:** volume control in menu ☐ · no clipping ☐ · no audible loop seams ☐ · game doesn't break if the browser blocks audio autoplay ☐

## 9. Win / Lose Conditions

- **Win:** the player calls the police with enough evidence to arrest Not Grandma.
- **Lose:** Not Grandma catches you at night, **or** the police lose all trust in your calls.
- **Session length:** 8–12 min (day-2 decision).
- **Police lives:** 2 wrong accusations, the third is a loss (day-2 decision).
- **Replayability:** randomized traitor is Nice-to-Have only.

## 10. Platform & Audience

**Platform:** PC, mouse and keyboard (WebGL) · **Audience:** PG-13 · **Target aspect ratio:** ~~16:9~~ → **4:3** *(corrected Aug 28)*. Build resolution **960×720**; the itch embed and the Web player canvas use the same.

## 11. Team Roles & Ownership

| Area | Owner |
|---|---|
| Engineering / architecture / build pipeline (+ audio implementation) | Gus |
| Gameplay programming | Janhavi |
| Art direction & assets | Irene |
| Sound design & music | Gus |
| Game design (final call) | **Gus** (locked day 2) |
| Itch page, screenshots, GIF, description | Janhavi |

**Writing split (locked day 2):** Gus → Not Grandma + item/clue texts · Janhavi → the Uncle (the traitor) · Irene → Mother + Cousin. One shared tone guide in Discord before anyone writes a line.

**Communication rule:** every day we post on Discord what we worked on yesterday, what we're working on now, and what's next. All assets go to the team's Drive.

## 12. 7-Day Plan

| Day | Date | Goal |
|---|---|---|
| 1 | Sun 23 | Concept locked, mantra, core loop on paper, repo + project created, palette defined |
| 2 | Mon 24 | **Playable loop with placeholders** |
| 3 | Tue 25 | The distrust mechanism works and reads clearly |
| 4 | Wed 26 | **Vertical slice: start → play → end.** Scope cut happens today |
| 5 | Thu 27 | Content: levels, balance, first final art integrated |
| 6 | Fri 28 | **Polish, juice, first build uploaded.** Playtest with outsiders |
| 7 | Sat 29 | Final build + early upload. Emergency buffer |
| Close | Sun 30, 10:00 | **Upload with ≥2h to spare.** Touch nothing after that |

**Schedule rules:** no start-to-finish game by day 4 → cut scope, don't add time. Day 7 is buffer, not a work day. Upload a build on day 6 even if it's ugly.

## 13. Post-Submission: Getting Ratings

- Every team member rates and comments on ≥10 jam games during the first week of voting.
- Post in the jam channel / Brackeys Discord with a short, eye-catching GIF.
- Itch page: animated GIF at the top, 3 screenshots, short description, visible controls, full credits.
- Reply to comments.

## 14. Risks & Open Decisions

See `Docs/day2-plan.md` §E for the filled risk table.

## 15. Retrospective

To fill in after submitting.
