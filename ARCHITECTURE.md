# Technical Architecture (ARCHITECTURE.md)

Godot 4.x + C# (.NET), mobile-targeted. This document defines how the code side is structured. For game design, see @DESIGN.md.

> **Important:** Claude does not touch scene files (.tscn). The node structures below DESCRIBE the structure the user will build manually in the editor — scripts are written to expect this structure (`[Export]` fields, `GetNode` paths). The user builds the scene and wires up the scripts.

---

## 1. Folder Structure

res://
├── scenes/            # .tscn files (user builds manually)
│   ├── screens/       # CharacterCreate, CharacterSelect, Main, Battle, Wheel, Stats, Settings
│   └── entities/      # Fighter, ProjectileVFX, etc.
├── scripts/           # all C# (.cs) — Claude works here
│   ├── core/          # game loop, state management, save/load
│   ├── combat/        # combat engine, turn resolver, damage calculation
│   ├── data/          # data models (Race, Weapon, Spell, Stats)
│   ├── progression/   # XP, level, wheel logic, tier/pool management
│   ├── ui/            # screen controller scripts
│   └── localization/  # language system
├── data/              # JSON/Resource definitions (races, 10 weapons/spells, tier tables)
├── assets/
│   ├── characters/    # per-race folder: idle/normal/thrust/heavy/hit/death
│   │   └── _refs/     # each race's CANONICAL REFERENCE image (used in asset generation)
│   ├── icons/         # weapon/spell icons
│   ├── vfx/           # fireball, sword effect, block effect
│   └── audio/         # SFX + music (filled at the very end)
└── localization/      # tr.csv / en.csv or .po files


---

## 2. Data Models (data-driven approach)

The game's balance is not hardcoded — it's all kept under `data/` as JSON/Resource so balancing can be done without changing code.

- **RaceData:** id, name, base stat distribution (STR/SPD/DUR/DEX/LUK/INT), visual identity note, reference asset path
- **StatBlock:** struct holding the values of the 6 stats
- **WeaponData:** id, name, tier (1-5), minLevel, damage values, normal/thrust/heavy parameters, heavy chance/cooldown, icon path
- **SpellData:** id, name, tier, minLevel, mana cost, cooldown, base damage (scaled by INT), icon path, vfx path
- **StatRollData:** stat bonuses that can come from the wheel (which stat, +amount, tier, level-weight)

Every weapon/spell requires a **tier** and **minLevel** field (for the pool lock).

---

## 3. Core Systems

### GameState (autoload/singleton)
The full state of the active character: race, name, level, XP, current stats, equipped weapon/spell, list of obtained items (so they don't reappear), statistics counters. Save/load operates over this state.

### Combat Engine (`scripts/combat/`)
- Turn resolver: order by SPD, multi-hit by SPD difference (max 5, diminishing returns)
- Damage calc: STR + weapon; spell damage scaled by INT
- Defense resolution: DEX-based miss/block/counter/payback chances evaluated in sequence
- LUK check at the start of each turn (is the weapon "dropping")
- Mana tracking + spell cooldown management
- Battle result → XP → level check → trigger wheel if needed
- **Runs independent of visuals:** the combat engine produces pure logic (an event/log list), and the UI turns those events into animation. This way the speed control (1x/2x/4x/skip) only changes playback speed, not the logic.

### Progression (`scripts/progression/`)
- XP curve (rising; formula in one place, balanceable)
- Wheel: 1st spin (type) → 2nd spin (item) from the level-locked pool
- Pool management: remove obtained items; drop a category from the type list when depleted; only Stat when both are depleted
- Tier weighting: large bonuses/high tiers have low odds early game

### Save System (`scripts/core/`)
- **4-5 slots**, each slot an independent GameState
- New character = new slot (does not delete the existing slot)
- Slot deletion requires a confirmation dialog
- Saving: after every battle + after the wheel + when a setting changes
- Format JSON (easy to debug); written under `user://` on mobile

### Localization (`scripts/localization/`)
- All user-facing text is **key-based** (NO hardcoding), set up this way from the start
- Godot's built-in translation system (CSV/.po) + tr/en
- Doing this from the start makes adding languages later cost-free

---

## 4. Asset Generation Pipeline (PixelLab MCP)

1. For each race, generate a SINGLE canonical reference first → user approves → saved under `assets/characters/<race>/_refs/`
2. All subsequent generations (animations, variants) use this reference as input — prevents style drift
3. Before generating, Claude presents the prompt and waits for approval (credit control)
4. Animation sets: idle / normal / thrust / heavy / hit / death (4-6 frames)
5. Export: PNG spritesheet + JSON → the user wires it to AnimatedSprite2D/AnimationPlayer in Godot

---

## 5. Mobile Performance (Claude's responsibility)

- **Object pooling:** VFX (fireball, sword effect) and floating text (Miss/Blocked/Counter/Payback) are pooled, not constantly instantiated/freed
- **Frame limit:** allow lower FPS on non-combat screens (battery saving)
- Because the combat engine logic is decoupled from animation, the speed multiplier (2x/4x/skip) is cheap: only animation playback duration is scaled
- Spritesheets are atlased; avoid unnecessarily large textures

---

## 6. Initial Setup Order (recommended implementation steps)

Each step is a separate, verifiable piece (Claude stops after each step, the user checks):

1. ✅ Project skeleton + folder structure + data model classes (RaceData, StatBlock, Weapon/Spell)
2. ✅ Under `data/`, the 6 races + a few sample weapon/spell definitions (the full lists of 10 each filled in later) — **all 10 weapons, all 10 spells, all 6 races are fully defined**, not just samples
3. ✅ GameState singleton + save/load system (4-5 slots) — no UI yet, testable logic
4. ✅ Combat engine (pure logic, producing an event list) — test by logging to console
5. ✅ XP/level + wheel logic (including tier/pool lock) — again tested at the logic level
6. ✅ Localization infrastructure — tr/en, all UI strings key-based
7. ✅ UI controller scripts (wired as the user builds scenes): character create/select → main screen → battle → wheel → statistics → settings — **all screens built and laid out except Wheel, which is still a plain centered layout with no actual spinning wheel**
8. 🟡 Asset integration — **character sprites (6 races), all 20 weapon/spell icons, and the full wood/brass UI theme are done and wired.** Speed control (1x/2x/4x/skip) and floating-text/hit-stop combat juice are done. Arena background art is NOT done (user is sourcing this separately). The Wheel screen's actual spin interaction is NOT built yet.
9. ⬜ Not started: wiring assets into the audio infrastructure (deferred to the end per design, per CLAUDE.md)

_Status as of the UI/asset polish pass — see project memory for the detailed history and lessons if picking this back up. Statistics screen (`StatisticsController.cs`) has no `.tscn` yet — it's the one controller script still without a scene._

---

## 7. Code Conventions

- Combat/progression logic is as independent of Godot nodes as possible (pure C# classes) → testable, reusable
- No balance-affecting number is hardcoded → all in `data/`
- `[Export]` fields clearly mark references the user will wire from the Inspector
- Every user-facing string goes through a localization key

### Progression & Scaling
- **XP Required per Level:** `10 + (Level * 5) + (Level^2 * 1)`
- **Enemy Level Range:** `PlayerLevel +/- 1` (minimum level 1)
- **Boss Multiplier:** Every 10th battle is a Boss. A Boss uses standard enemy generation but all final calculated stats are multiplied by `1.5f`.

### Combat Resolution (The Engine)
- **Weapon Drop (Pity Timer):** At the start of each turn, drop chance is `(LUK * 1%) + (TurnCount * 5%)`. Once dropped, it stays equipped for the battle.
- **Multi-hit Calculation:** 
  - `SpeedDelta = Max(0, AttackerSPD - DefenderSPD)`
  - `TotalHits = 1 + Min(4, Floor(SpeedDelta / 10))`
- **Multi-hit Damage Falloff:** 1st hit does 100% damage, 2nd hit 75%, 3rd hit 50%, 4th hit 25%, 5th hit 10%.
- **Critical Hit:** Each weapon attack has a chance to crit: `WeaponCritChance + (AttackerLUK * 0.5%)`. On crit, damage is multiplied by the weapon's `CritMultiplier`. No cooldown — replaces the old Heavy attack type.
- **Defense Sequence (Calculated on Defender):**
  1. Evaluate **Miss** (Attacker's check): `Max(0, 20% - (AttackerDEX * 0.5%))`
  2. Evaluate **Counter**: `Min(15%, DefenderDEX * 0.5%)`
  3. Evaluate **Payback**: `Min(20%, DefenderDEX * 0.75%)`
  4. Evaluate **Block**: `Min(30%, DefenderDEX * 1.0%)`

### Endgame Wheel
- When both Weapon and Spell pools are completely depleted, the Wheel enters "Endgame Mode".
- The 1st spin (Type) is bypassed. The 2nd spin exclusively rolls Stat additions, but introduces ultra-rare, high-value static multiplier bonuses (e.g., +5% Max HP, +3% Flat Damage) to sustain late-game engagement.

## Data Tables (JSON/Resource Blueprints)

### Weapons (10 Items) - physical damage, scales with STR
| ID | Name | Tier | Min Lvl | DMG | Scale |
|---|---|---|---|---|---|
| W01 | Wooden Club | 1 | 1 | Low | Low |
| W02 | Bronze Shortsword | 1 | 4 | Mid | Low |
| W03 | Steel Longsword | 2 | 8 | Mid | Mid |
| W04 | Obsidian Axe | 2 | 12| High| Low |
| W05 | Adamantite Greatsword | 3 | 17| High| Low |
| W06 | Sunflare Spear | 3 | 26| Mid | Mid |
| W07 | Shadowfang | 4 | 30| High| High |
| W08 | Void Cleaver | 4 | 42| High| Mid |
| W09 | Excalibur | 5 | 47| Max | Max |
| W10 | Worldbreaker | 5 | 65| Max | Max |

### Spells (10 Items) - pure magic (Lightning/Frost/Arcane), costs Mana, scales with INT
*(Tiers dictate high base damage and high INT multipliers, designed for burst combat before falling back to physical attacks.)*
- **Tier 1 (Lvl 1-6):** Magic Missile, Static Shock
- **Tier 2 (Lvl 8-14):** Ice Shard, Mind Blast
- **Tier 3 (Lvl 17-26):** Chain Lightning, Blizzard
- **Tier 4 (Lvl 30-42):** Thunderbolt, Void Storm
- **Tier 5 (Lvl 47-65):** Absolute Zero, Armageddon
