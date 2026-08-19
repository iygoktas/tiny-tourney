#!/usr/bin/env python3
"""
Makes the stretchable zones of the UI frames uniform along their stretch axis.

The generated frames are drawn with grain and speckle everywhere. A nine-patch
stretches its edge strips and middle, and stretching noise smears every fleck
into a long streak — the scattered dashes seen on wide buttons and the vertical
lines on tall panels.

Corners are never touched. For the rest, uniform-along-the-stretch-axis is the
one property that survives stretching perfectly:

- interior           stretches both ways   -> every row becomes one colour
                                              (per-row, so vertical shading survives)
- left/right strips  stretch vertically    -> every column becomes one colour
- top/bottom strips  stretch horizontally  -> every row becomes one colour

Only opaque pixels are rewritten, so rounded silhouettes keep their shape.
The PNGs are overwritten in place; the drawn originals live in git history.

Usage:  python3 tools/flatten_stretch_zones.py
"""

import pathlib
import sys
from collections import Counter

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
from measure_frames import decode_png  # noqa: E402
from extract_bar_fill import write_png  # noqa: E402

FRAMES = pathlib.Path(__file__).resolve().parent.parent / "assets" / "ui" / "frames"
ALPHA_MIN = 40

# The same texture margins the theme uses. If those change, change these.
MARGINS = {
    "panel_wood.png": (12, 11, 12, 11),
    "panel_slate.png": (22, 24, 22, 23),
    "slot_icon.png": (20, 18, 20, 18),
    "bar_empty.png": (10, 6, 10, 6),
    "bar_fill.png": (3, 6, 3, 7),
}


def dominant(colours):
    return Counter(colours).most_common(1)[0][0]


def unify_rows(px, x_range, y_range):
    """Every row in the zone becomes its own single colour."""
    changed = 0
    for y in y_range:
        opaque = [px[y][x] for x in x_range if px[y][x][3] >= ALPHA_MIN]
        if not opaque:
            continue
        colour = dominant(opaque)
        for x in x_range:
            if px[y][x][3] >= ALPHA_MIN and px[y][x] != colour:
                px[y][x] = colour
                changed += 1
    return changed


def unify_cols(px, x_range, y_range):
    """Every column in the zone becomes its own single colour."""
    changed = 0
    for x in x_range:
        opaque = [px[y][x] for y in y_range if px[y][x][3] >= ALPHA_MIN]
        if not opaque:
            continue
        colour = dominant(opaque)
        for y in y_range:
            if px[y][x][3] >= ALPHA_MIN and px[y][x] != colour:
                px[y][x] = colour
                changed += 1
    return changed


def flatten(name, margins):
    path = FRAMES / name
    w, h, px = decode_png(path)
    left, top, right, bottom = margins
    mid_x = range(left, w - right)
    mid_y = range(top, h - bottom)

    changed = 0
    changed += unify_rows(px, mid_x, mid_y)                    # interior
    changed += unify_cols(px, range(0, left), mid_y)           # left strip
    changed += unify_cols(px, range(w - right, w), mid_y)      # right strip
    changed += unify_rows(px, mid_x, range(0, top))            # top strip
    changed += unify_rows(px, mid_x, range(h - bottom, h))     # bottom strip

    write_png(path, w, h, [[bytes(p) for p in row] for row in px])
    print(f"  {name:<18} {w}x{h}   {changed} piksel duzlestirildi")


def main():
    print("uzayan bolgeler duzlestiriliyor (koseler dokunulmadan):")
    for name, margins in MARGINS.items():
        flatten(name, margins)


if __name__ == "__main__":
    main()
