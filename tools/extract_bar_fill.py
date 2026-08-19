#!/usr/bin/env python3
"""
Cuts the red fill out of bar_filled.png into its own texture.

Godot's ProgressBar draws two separate StyleBoxes — a background and a fill that
grows with the value — but the generated art has the red painted into the same
image as the trough. This lifts a narrow vertical slice from deep inside the
filled part, keeps only the red pixels, and writes it as a tall thin texture that
can be nine-patched and stretched to any width.

Pillow is not installed here, so the PNG is written with the standard library.

Usage:  python3 tools/extract_bar_fill.py
"""

import pathlib
import struct
import sys
import zlib

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
from measure_frames import decode_png  # noqa: E402

FRAMES = pathlib.Path(__file__).resolve().parent.parent / "assets" / "ui" / "frames"

# A window well inside the red run, away from both end caps and from the darker
# unfilled remainder on the right.
SLICE_X0, SLICE_X1 = 40, 60


def is_red(pixel):
    r, g, b, a = pixel
    return a > 8 and r > 70 and r > g * 2 and r > b * 2


def write_png(path, width, height, pixels):
    raw = bytearray()
    for y in range(height):
        raw.append(0)  # filter type: none
        for x in range(width):
            raw.extend(pixels[y][x])

    def chunk(tag, body):
        return (struct.pack(">I", len(body)) + tag + body
                + struct.pack(">I", zlib.crc32(tag + body) & 0xFFFFFFFF))

    png = b"\x89PNG\r\n\x1a\n"
    png += chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
    png += chunk(b"IDAT", zlib.compress(bytes(raw), 9))
    png += chunk(b"IEND", b"")
    path.write_bytes(png)


def main():
    src = FRAMES / "bar_filled.png"
    width, height, pixels = decode_png(src)

    out_w = SLICE_X1 - SLICE_X0 + 1
    out = []
    red_rows = []

    for y in range(height):
        row = []
        row_has_red = False
        for x in range(SLICE_X0, SLICE_X1 + 1):
            p = pixels[y][x]
            if is_red(p):
                row.append(bytes(p))
                row_has_red = True
            else:
                # Anything that is not the fill becomes transparent, so the fill
                # sits inside the trough drawn by the background stylebox.
                row.append(bytes((0, 0, 0, 0)))
        out.append(row)
        if row_has_red:
            red_rows.append(y)

    dest = FRAMES / "bar_fill.png"
    write_png(dest, out_w, height, out)

    print(f"yazildi: {dest.relative_to(FRAMES.parent.parent.parent)}")
    print(f"  boyut       : {out_w}x{height}")
    print(f"  kirmizi satir: y {red_rows[0]}-{red_rows[-1]}  ({len(red_rows)}px yuksek)")
    print(f"  kaynak dilim : x {SLICE_X0}-{SLICE_X1} of {src.name}")


if __name__ == "__main__":
    main()
