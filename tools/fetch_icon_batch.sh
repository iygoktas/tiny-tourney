#!/bin/bash
set -e
DEST="/private/tmp/claude-501/-Users-iygoktas-Projects-tiny-tourney/6a4e17db-fe04-457f-9630-f8b864159096/scratchpad/icons_full"
mkdir -p "$DEST"
cd "$DEST"

fetch() {
  curl -sf "https://api.pixellab.ai/mcp/images/$2/download" -o "$1.png"
}

fetch w02_bronze_shortsword c4a8edb0-3617-4e69-9e11-617e609aaf01 &
fetch w03_steel_longsword b0b98504-ccc4-4a4a-b8ea-61f4068aa19c &
fetch w04_obsidian_axe 186f5deb-897e-4926-a3c9-1fcc5748cd0f &
fetch w05_adamantite_greatsword a6075f66-4272-4fec-8310-c35e5a976d2e &
fetch w06_sunflare_spear 492f5378-201c-4fce-a7cb-e563d9c459a3 &
fetch w07_shadowfang 39f11d61-ffe1-40d9-a8f7-e63a186d1bde &
fetch w08_void_cleaver 1ec0353f-96d7-428c-81b8-7f936f36985b &
fetch w10_worldbreaker ff232dff-93a6-4826-8362-bb9adea29f16 &
fetch sp01_magic_missile 4f8c3b50-017c-40eb-bfde-f404dd0de9ce &
fetch sp02_static_shock 1e8df2a6-4459-48e2-a067-b630d3e04a0c &
fetch sp03_ice_shard e6309d1c-6cdc-4823-a3d7-fe0ea4a9b606 &
fetch sp04_mind_blast 541afbde-540d-42b7-bf31-59e4aaa6a8d7 &
fetch sp06_blizzard 19dbc5d9-1ffd-4671-913e-fe2b6dc937cc &
fetch sp07_thunderbolt d72e75f8-bc13-42d9-8e7a-40ba32044c28 &
fetch sp08_void_storm cd73843e-6ed2-4d04-9fcb-51e4203585c7 &
fetch sp09_absolute_zero e45a8e16-88a2-47f9-91ab-bc5c9a964d71 &
fetch sp10_armageddon 388ea8ad-a1a9-4616-959a-86427d6876aa &
wait

for f in *.png; do
  case "$f" in big_*) continue;; esac
  sips -s format png --resampleHeightWidth 320 320 "$f" --out "big_$f" >/dev/null 2>&1
done

echo "done"
ls *.png | wc -l
