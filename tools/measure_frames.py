#!/usr/bin/env python3
"""
Measures the border thickness of the generated UI frame images so their
StyleBoxTexture nine-patch margins can be set from something other than a guess.

Pillow is not installed on this machine, so PNGs are decoded here with nothing
but the standard library.

Usage:  python3 tools/measure_frames.py
"""

import pathlib
import struct
import zlib

FRAMES_DIR = pathlib.Path(__file__).resolve().parent.parent / "assets" / "ui" / "frames"


def decode_png(path):
    """Returns (width, height, pixels) where pixels[y][x] is an (r, g, b, a) tuple."""
    data = path.read_bytes()
    if data[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError(f"{path.name} is not a PNG")

    pos = 8
    width = height = bit_depth = color_type = None
    idat = bytearray()
    palette = None
    trns = None

    while pos < len(data):
        (length,) = struct.unpack(">I", data[pos:pos + 4])
        ctype = data[pos + 4:pos + 8]
        body = data[pos + 8:pos + 8 + length]
        pos += 12 + length  # length + type + body + crc

        if ctype == b"IHDR":
            width, height, bit_depth, color_type = struct.unpack(">IIBB", body[:10])
        elif ctype == b"PLTE":
            palette = body
        elif ctype == b"tRNS":
            trns = body
        elif ctype == b"IDAT":
            idat += body
        elif ctype == b"IEND":
            break

    if bit_depth != 8:
        raise ValueError(f"{path.name}: only 8-bit images are handled, got {bit_depth}")

    channels = {0: 1, 2: 3, 3: 1, 4: 2, 6: 4}[color_type]
    raw = zlib.decompress(bytes(idat))
    stride = width * channels

    # Undo the per-scanline PNG filters.
    out = bytearray()
    previous = bytearray(stride)
    at = 0
    for _ in range(height):
        filter_type = raw[at]
        at += 1
        line = bytearray(raw[at:at + stride])
        at += stride

        for i in range(stride):
            left = line[i - channels] if i >= channels else 0
            up = previous[i]
            up_left = previous[i - channels] if i >= channels else 0

            if filter_type == 1:
                line[i] = (line[i] + left) & 0xFF
            elif filter_type == 2:
                line[i] = (line[i] + up) & 0xFF
            elif filter_type == 3:
                line[i] = (line[i] + (left + up) // 2) & 0xFF
            elif filter_type == 4:
                p = left + up - up_left
                pa, pb, pc = abs(p - left), abs(p - up), abs(p - up_left)
                pred = left if (pa <= pb and pa <= pc) else (up if pb <= pc else up_left)
                line[i] = (line[i] + pred) & 0xFF

        out += line
        previous = line

    pixels = []
    for y in range(height):
        row = []
        base = y * stride
        for x in range(width):
            i = base + x * channels
            if color_type == 6:
                row.append(tuple(out[i:i + 4]))
            elif color_type == 2:
                row.append((out[i], out[i + 1], out[i + 2], 255))
            elif color_type == 3:
                idx = out[i]
                r, g, b = palette[idx * 3:idx * 3 + 3]
                a = trns[idx] if trns and idx < len(trns) else 255
                row.append((r, g, b, a))
            elif color_type == 0:
                v = out[i]
                row.append((v, v, v, 255))
            else:  # 4: grey + alpha
                v = out[i]
                row.append((v, v, v, out[i + 1]))
        pixels.append(row)

    return width, height, pixels


def opaque_bounds(width, height, pixels, threshold=8):
    """Trims fully transparent margins, which the rounded corners leave behind."""
    xs = [x for y in range(height) for x in range(width) if pixels[y][x][3] > threshold]
    ys = [y for y in range(height) for x in range(width) if pixels[y][x][3] > threshold]
    if not xs:
        return 0, 0, width - 1, height - 1
    return min(xs), min(ys), max(xs), max(ys)


def run_lengths(colors, tolerance=18):
    """Groups a scanline into runs of near-identical colour: (length, colour)."""
    runs = []
    for c in colors:
        if runs and all(abs(a - b) <= tolerance for a, b in zip(runs[-1][1], c)):
            runs[-1][0] += 1
        else:
            runs.append([1, c])
    return runs


def hexcolor(c):
    return f"#{c[0]:02x}{c[1]:02x}{c[2]:02x}" + ("" if c[3] == 255 else f"a{c[3]:02x}")


def describe(path):
    width, height, pixels = decode_png(path)
    x0, y0, x1, y1 = opaque_bounds(width, height, pixels)

    print(f"\n{'=' * 66}")
    print(f"{path.name}   {width}x{height}   opaque area: x {x0}-{x1}, y {y0}-{y1}")
    print("=" * 66)

    mid_y = (y0 + y1) // 2
    mid_x = (x0 + x1) // 2

    row = [pixels[mid_y][x] for x in range(x0, x1 + 1)]
    col = [pixels[y][mid_x] for y in range(y0, y1 + 1)]

    print(f"  yatay kesit (y={mid_y}):")
    offset = x0
    for length, colour in run_lengths(row):
        print(f"     x {offset:>3}-{offset + length - 1:<3} {length:>3}px  {hexcolor(colour)}")
        offset += length

    print(f"  dikey kesit (x={mid_x}):")
    offset = y0
    for length, colour in run_lengths(col):
        print(f"     y {offset:>3}-{offset + length - 1:<3} {length:>3}px  {hexcolor(colour)}")
        offset += length


def main():
    for path in sorted(FRAMES_DIR.glob("*.png")):
        describe(path)


if __name__ == "__main__":
    main()
