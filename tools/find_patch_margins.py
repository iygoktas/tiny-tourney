#!/usr/bin/env python3
"""
Works out nine-patch margins for the generated UI frames.

A nine-patch keeps its corners at a fixed size and stretches the four edge strips
and the middle. That only looks right if the corner decoration — the brass blobs
and rivets — sits entirely inside the corner squares. Cut through it and the
decoration gets stretched or repeated along the whole edge.

Comparing raw pixels does not work here: the wood and slate are drawn with noise,
so no two rows are ever identical. Instead each pixel is reduced to what it
structurally is (transparent, brass, outline, body) and rows are compared on that.
Where the structure stops changing is where the corner ends.

Usage:  python3 tools/find_patch_margins.py
"""

import pathlib
import sys

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
from measure_frames import decode_png  # noqa: E402

FRAMES = pathlib.Path(__file__).resolve().parent.parent / "assets" / "ui" / "frames"


def classify(pixel):
    r, g, b, a = pixel
    if a < 40:
        return " "
    if r > 120 and g > 90 and r > b + 45:
        return "B"          # bright brass
    if r > 85 and g > 60 and r > b + 30:
        return "b"          # dim brass
    if r + g + b < 95:
        return "."          # outline / deep shadow
    return "-"              # body


def structure(path):
    w, h, px = decode_png(path)
    grid = [[classify(px[y][x]) for x in range(w)] for y in range(h)]
    return w, h, grid


def stable_span(lines):
    """The longest run of consecutive identical lines: (start, end_exclusive)."""
    best_start, best_end = 0, 0
    start = 0
    for i in range(1, len(lines) + 1):
        if i == len(lines) or lines[i] != lines[start]:
            if i - start > best_end - best_start:
                best_start, best_end = start, i
            start = i
    return best_start, best_end


def margins_for(path):
    w, h, grid = structure(path)
    zone_x = max(4, w // 4)
    zone_y = max(4, h // 4)

    # Rows judged only on their left and right border zones: where does the
    # vertical edge settle into one repeating profile?
    rows = ["".join(grid[y][:zone_x]) + "|" + "".join(grid[y][w - zone_x:]) for y in range(h)]
    top, bottom_end = stable_span(rows)
    bottom = h - bottom_end

    cols = ["".join(grid[y][x] for y in range(zone_y))
            + "|" + "".join(grid[y][x] for y in range(h - zone_y, h)) for x in range(w)]
    left, right_end = stable_span(cols)
    right = w - right_end

    return w, h, left, top, right, bottom


def main():
    print(f"{'dosya':<18} {'boyut':<10} {'sol':>4} {'ust':>4} {'sag':>4} {'alt':>4}   merkez")
    print("-" * 68)
    for path in sorted(FRAMES.glob("*.png")):
        if path.name == "bar_filled.png":
            continue
        w, h, l, t, r, b = margins_for(path)
        cw, ch = w - l - r, h - t - b
        note = "" if cw > 0 and ch > 0 else "   <-- MERKEZ KALMIYOR"
        print(f"{path.name:<18} {f'{w}x{h}':<10} {l:>4} {t:>4} {r:>4} {b:>4}   {cw}x{ch}{note}")


if __name__ == "__main__":
    main()
