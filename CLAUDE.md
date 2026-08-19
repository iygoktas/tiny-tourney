# Project: Tiny Tourney

Idle auto-battler, C# + Godot. No loot; progression comes from cyclical combat + a level-up wheel system. Zero-budget indie project, two-person team.

For details:
- Game design (mechanics, races, stats, wheel, combat rules, UI) → @DESIGN.md
- Technical architecture (folder structure, data models, asset pipeline, node conventions) → @ARCHITECTURE.md
- Content and Mathematical construction (formulas about xp, weapon names)

## HARD RULES

1. **Do not touch scene files (.tscn).** Adding nodes, node placement, scene composition, Inspector settings — all of this is done manually by the user in the Godot editor. You prepare the script (.cs) files, data classes, and logic; leave the scene-wiring to the user.
2. **Stop after every step and wait for approval.** Do not try to finish a large task in one pass. When you complete a sub-task, summarize what you did and wait for the user to check it — do not automatically move on to the next step.
3. **Present the plan before making any PixelLab MCP call.** Explain what will be generated with which prompt first; do not generate until approval is given (it costs credits).
4. **When generating new assets, use the race's canonical reference image.** The "Art Direction" section in DESIGN.md will specify an approved reference image path for each race — use that reference for every new generation for that race (animations, weapon variants); do not generate randomly from scratch.
5. **Asset Quality & MCP Documentation:** To achieve quality and perfect AI creations that do not look like generic AI art, you MUST use PixelLab's official documentation as a reference. Always refer to `https://api.pixellab.ai/mcp/docs` to search for the right features, configuration parameters, and optimal usage strategies for the MCP before generating assets.

## Technology

- Godot 4.x, C# (.NET)
- Target platform: mobile
- Asset generation: PixelLab (Tier 1, $12/month)

## Workflow preference

Work in plan mode: first research and propose what you'll do, then implement once approved. Progress in small, verifiable steps — instead of saying "done" and moving on, state what can be tested / how it can be checked.
