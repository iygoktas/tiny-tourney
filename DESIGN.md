# Game Design Document (DESIGN.md)

## 1. Concept

Idle auto-battler. The player creates a character (race + name), presses the "Fight" button, and combat plays out turn-based and fully automatically — the player only watches. Winning/losing earns XP; on level-up the player spins a wheel to gain a new stat/weapon/spell. No loot. The game runs forever (bosses appear periodically). Reference feel: Sword and Sandals.

---

## 2. Character Creation

At the start of the game (and whenever a new run is started), the character creation screen appears:
- **Race selection:** one of 6 races
- **Name entry:** free text
- On confirm, it is written to a new save slot and the game proceeds to the main screen

Multiple saves: **4-5 slots**. Starting a new character does NOT delete an existing one — it is written to a separate slot. When slots are full, the user can manually delete a slot (deletion requires a confirmation dialog).

---

## 3. Races

Each race has a different starting stat distribution and a distinct visual identity.

| Race | High Stat | Low Stat | Visual Identity (art prompt direction) |
|---|---|---|---|
| **Human** | Balanced (all average) | — | Classic knight/soldier, steel armor, clean lines, blue-silver palette |
| **Elf** | DEX, SPD | STR | Slender build, pointed ears, green-gold palette, light leather armor, elegant |
| **Orc** | STR | LUK | Large, brute force, tusks, dark green-brown palette, spiked/rusty armor |
| **Undead** | DUR | SPD | Skeletal/decayed, pale gray-blue palette, torn cloak, empty eye sockets |
| **Demon** | STR, INT | DUR | Horned, red-black palette, fiery details, aggressive silhouette |
| **Celestial** | DUR, LUK | STR | Winged/radiant, white-gold palette, holy armor, calm and majestic |

Demon ↔ Celestial are intentionally designed as opposites (destructive-fragile vs. protected-lucky).

---

## 4. Stats

| Stat | Abbrev. | Effect |
|---|---|---|
| Strength | STR | Increases basic attack + weapon damage |
| Speed | SPD | Turn priority + per-turn multi-hit chance (difference-based, max 5 hits/turn) |
| Durability | DUR | Determines Max HP |
| Dexterity | DEX | Increases Block/Counter/Payback chance ↑, decreases own Miss chance ↓ |
| Luck | LUK | Per-turn chance of the equipped weapon "dropping from the sky" |
| Intelligence | INT | Mana pool + spell damage |

---

## 5. Combat System

Turn-based, automatic, no player intervention. Side view, fixed arena background. The character whose turn it is walks toward the opponent, strikes, and returns; if casting a spell, they stop in place and hurl it.

### Order and hits
- **SPD** determines order (higher SPD strikes first)
- Multi-hit: based on the **SPD difference** between the two characters, a chance to strike more than once at the start of the turn. **Maximum 5 hits/turn** (approaches the cap with diminishing returns)

### Weapon attack types
- **Normal / Thrust**
- Each weapon attack also has a chance to land as a **Critical Hit** (boosted by LUK), dealing bonus multiplied damage. No cooldown — can happen on any attack.

### Starting state
- The character starts combat with **fists** (no weapon)
- Each turn, with a chance tied to LUK, the equipped weapon "drops from the sky" and starts being used

### Defense mechanics (all increase with DEX)
- **Miss:** the attack misses. (DEX lowers your own miss.) → "Miss" text, no effect
- **Block:** damage is blocked → "Blocked" text + block effect
- **Counter:** if triggered, you take NO damage and reflect the same damage to the opponent → "Counter" text
- **Payback:** if triggered, you DO take the damage but also reflect the same damage to the opponent → "Payback" text

### Spells (10 total)
- **Mana + cooldown based.** If mana is insufficient, the spell can't be cast and the character falls back to the weapon
- Damage and mana pool are affected by **INT**
- Example visual effect: hurling a fireball

### HP
- Max HP is tied to DUR, increases on level-up
- HP is **fully restored** at the start of each battle (damage does not carry over from the previous battle)

---

## 6. Enemy and Boss

- Scaled to be near the player's level, of a random race, and not too powerful
- The enemy also has a weapon/spell
- Visual: varies by race but a single shared asset set is used per race
- **Boss:** 1 boss after every 10 enemies defeated. Higher stats; can be distinguished with a special/larger sprite as credits allow

---

## 7. Progression (XP + Wheel)

### XP
- Win → **+3 XP**, lose → **+1 XP**
- Rising XP curve per level (e.g. lvl1→10, lvl2→15, ...) — exact formula in ARCHITECTURE.md

### Wheel (opens AUTOMATICALLY on level-up)
Two stages, produces a single result:
1. **1st spin — type:** Stat / Weapon / Spell
2. **2nd spin — specific item:** which item within the rolled type

### Tier and pool lock
- Each category (weapon/spell) is split into **5 tiers**
- The pool is locked by level: at low levels, higher tiers are **not** in the pool (e.g. Excalibur cannot appear early game)
- Stat rolls also have tiers (small/large bonuses, e.g. +2 INT or +5 STR); large bonuses have low odds early game

### Repeats and depletion
- The same weapon/spell item does **not** appear again (once obtained, it is silently removed from the pool; the player doesn't notice)
- If a category is depleted, it is removed from the wheel's 1st spin
- If both categories are depleted, the wheel only gives **Stat** (stats can come forever, which sustains infinite progression)

### Equip
- One each of weapon/spell is equipped; a new one from the wheel replaces the old
- Stats are not equipped — they are permanent and cumulative

---

## 8. UI Screens

### Character Select / Creation
- On launch: existing slots are listed ("continue" or "new character")
- New character: race selection + name entry

### Main Screen
- Character in idle pose, centered
- Stats visible
- Weapon/Spell menu: unlocked ones shown clearly; **locked ones shadowed/silhouetted** (to spark curiosity — especially the dark silhouette of nice weapons is visible)
- "Fight" button
- Statistics corner (see below)

### Battle Screen
- Fully automatic, no player intervention
- **Speed control:** 1x / 2x / 4x / Skip buttons

### Wheel Screen
- Opens automatically only on level-up, with a pleasant spinning experience

### Statistics Screen
Every counter we can include: total battles won, battles lost, bosses defeated, highest level reached, total battles played, etc.

### Settings Screen
- Sound on/off (music + SFX separately)
- Language (localization infrastructure built multi-language from the start — Turkish + English)
- Save slot management (deletion, with confirmation dialog)

---

## 9. Audio (deferred to the end, infrastructure ready)

Audio assets will be added at the very end (provided by the user), but the sound-playback infrastructure and setting toggles are built from the start. Needed: hit/miss/block/counter/payback SFX, wheel spin sound, background music.

---

## 10. Art Direction (for cool, consistent characters)

Three rules so characters don't look generic/random:

1. **Each race's distinct visual identity** (see §3 table) enters every prompt with concrete adjectives — not "elf," but "slender, pointed-eared, green-gold palette, elegant elf warrior."
2. **Silhouette test:** the character should be recognizable by race even as a flat black silhouette (horns, ears, weapon profile must be distinct).
3. **Canonical reference + lock:** for each race, a single "hero pose" is generated first and iterated until the user approves. The approved image is locked as the reference for all of that race's animations and variants (no generating from scratch randomly).

### Style
- 32-64px range, semi-pixel (not sharp pixel-art; easy on the eyes, close to clean but not fully clean)
- Animations: 4-6 frames (idle / normal attack / thrust / heavy / hit / death)
- Side view, fixed arena background

### Icons
- 20 weapon/spell icons generated with PixelLab
- Special/rare weapons like Excalibur, Shadowfang get individual, carefully crafted generations
- No separate asset is GENERATED for the locked/shadowed look — the same icon is darkened via shader/color modulation in Godot (a code task)

---

## 11. Pre-Battle Screen & Combat UI (assets required)

### Pre-battle matchup layout (Sword and Sandals style)
When the player presses "Fight" in the lobby, a pre-battle matchup screen appears:
- **Left side:** our character, with their stats displayed beside them
- **Right side:** the opposing character, with their stats displayed beside them
- This needs a well-composed, good-looking layout generated from the AI platform — a clean, professional versus/matchup arrangement, not a rough placeholder

### Health bars
- Health bars are shown during combat (for both fighters)
- The health bar visuals must look good and fit the theme — themed frame/fill art, not a default gray rectangle. This is a real asset to design, not an afterthought.

### Arenas
- Battles take place in arenas; these arena background images must be created as assets
- Multiple arena backgrounds for variety, matching the overall art style and quality bar

### Quality bar (non-negotiable priority)
Our goal is NOT for the visuals to look AI-generated — they must genuinely look like a professional game. This is our top priority. Every generated asset (characters, matchup layout, health bars, arenas, icons, VFX) is held to this standard: if something reads as generic/AI-looking, it is re-iterated until it looks hand-crafted and professional. The canonical-reference + silhouette-test discipline from §10 applies to all of these, not just characters.
