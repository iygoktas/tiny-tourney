#!/usr/bin/env python3
"""
Finds the real nine-patch margins of a frame image.

measure_frames.py sampled a single row and column, which missed decoration that
sits away from the centre line — rivets spread along an edge, an inner bevel.
Tiling a strip that still contains those repeats them across the whole control.

This walks every row and column instead, and reports how far in from each side
the image stops varying. Inside that distance a nine-patch centre is genuinely
flat and safe to tile or stretch.

Usage:  python3 tools/analyse_frame.py [name ...]     (default: every frame)
"""

import pathlib
import sys
from collections import Counter

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
from measure_frames import decode_png, opaque_bounds  # noqa: E402

FRAMES = pathlib.Path(__file__).resolve().parent.parent / "assets" / "ui" / "frames"
TOLERANCE = 20


def close(a, b, tol=TOLERANCE):
    return all(abs(x - y) <= tol for x, y in zip(a, b))


def dominant_interior(pixels, x0, y0, x1, y1):
    """The most common colour in the middle fifth — the flat interior, if there is one."""
    cx0, cx1 = x0 + (x1 - x0) * 2 // 5, x0 + (x1 - x0) * 3 // 5
    cy0, cy1 = y0 + (y1 - y0) * 2 // 5, y0 + (y1 - y0) * 3 // 5
    counter = Counter()
    for y in range(cy0, cy1 + 1):
        for x in range(cx0, cx1 + 1):
            counter[pixels[y][x]] += 1
    return counter.most_common(1)[0][0]


def analyse(path):
    w, h, px = decode_png(path)
    x0, y0, x1, y1 = opaque_bounds(w, h, px)
    interior = dominant_interior(px, x0, y0, x1, y1)

    # For every row, how far in from the left/right until the interior colour starts.
    left = right = 0
    for y in range(y0, y1 + 1):
        row = px[y]
        i = x0
        while i <= x1 and not close(row[i], interior):
            i += 1
        if i <= x1:
            left = max(left, i - 0)
        j = x1
        while j >= x0 and not close(row[j], interior):
            j -= 1
        if j >= x0:
            right = max(right, w - 1 - j)

    top = bottom = 0
    for x in range(x0, x1 + 1):
        i = y0
        while i <= y1 and not close(px[i][x], interior):
            i += 1
        if i <= y1:
            top = max(top, i - 0)
        j = y1
        while j >= y0 and not close(px[j][x], interior):
            j -= 1
        if j >= y0:
            bottom = max(bottom, h - 1 - j)

    centre_w = w - left - right
    centre_h = h - top - bottom
    fits = centre_w > 0 and centre_h > 0

    print(f"\n{path.name}  {w}x{h}   interior {interior[:3]}")
    print(f"   gercek cerceve : sol {left}  ust {top}  sag {right}  alt {bottom}")
    print(f"   kalan merkez   : {centre_w}x{centre_h}  {'OK' if fits else '<-- MERKEZ KALMIYOR'}")
    if fits and (centre_w < 4 or centre_h < 4):
        print("   UYARI: merkez cok ince, tile etmek yerine duz renk kullanmak daha iyi")
    return left, top, right, bottom, centre_w, centre_h


def main():
    names = sys.argv[1:]
    paths = [FRAMES / f"{n}.png" for n in names] if names else sorted(FRAMES.glob("*.png"))
    for p in paths:
        if p.name in {"bar_filled.png"}:
            continue
        analyse(p)


if __name__ == "__main__":
    main()
