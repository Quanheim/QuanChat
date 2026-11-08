"""Thunderstore icon (Package/icon.png, 256 x 256): two speech bubbles, the front one in shout yellow with lines of
text, the back one in the chat window's dark grey. Drawn flat in the Quan mods' frame (dark tile, orange rim), like
QuanChunkLoader's icon. Run: python tools/render_package_icon.py"""
import os

from PIL import Image, ImageDraw, ImageFilter

SS = 4                      # supersampling: draw at 1024 px, shrink to 256
N = 256 * SS
here = os.path.dirname(os.path.abspath(__file__))
out = os.path.join(here, "..", "Package", "icon.png")

RIM = (222, 149, 50, 255)
TILE = (28, 25, 22, 255)
BACK = (92, 96, 100, 255)
BACK_LINE = (150, 154, 156, 255)
SHOUT = (255, 226, 92, 255)       # Valheim's shout colour, a touch warmer
SHOUT_SHADE = (214, 176, 52, 255)
TEXT = (52, 42, 24, 255)
GLOW = (255, 210, 80)


def s(v):
    return int(v * SS)


def bubble(draw, box, tail, fill):
    """Rounded bubble with a triangular tail (three points in icon pixels)."""
    draw.rounded_rectangle([s(v) for v in box], radius=s(22), fill=fill)
    draw.polygon([(s(x), s(y)) for x, y in tail], fill=fill)


img = Image.new("RGBA", (N, N), (0, 0, 0, 0))
d = ImageDraw.Draw(img)

# Frame: orange rim, dark tile.
d.rounded_rectangle([s(4), s(4), s(252), s(252)], radius=s(30), fill=RIM)
d.rounded_rectangle([s(13), s(13), s(243), s(243)], radius=s(22), fill=TILE)

# Back bubble, upper right, tail down-right.
d = ImageDraw.Draw(img)
bubble(d, (108, 40, 222, 122), [(186, 118), (210, 118), (214, 146)], BACK)
for i, w in enumerate((80, 58)):
    y = 66 + i * 22
    d.rounded_rectangle([s(124), s(y), s(124 + w), s(y + 9)], radius=s(4.5), fill=BACK_LINE)

# Warm glow behind the front bubble.
glow = Image.new("RGBA", (N, N), (0, 0, 0, 0))
ImageDraw.Draw(glow).rounded_rectangle([s(30), s(92), s(196), s(204)], radius=s(30), fill=GLOW + (110,))
img.alpha_composite(glow.filter(ImageFilter.GaussianBlur(s(16))))

# Front bubble, lower left, tail down-left: a shaded edge underneath, then the face.
d = ImageDraw.Draw(img)
bubble(d, (34, 102, 194, 202), [(56, 196), (88, 196), (44, 230)], SHOUT_SHADE)
bubble(d, (34, 96, 194, 196), [(56, 190), (88, 190), (46, 222)], SHOUT)
for i, w in enumerate((124, 100, 70)):
    y = 118 + i * 24
    d.rounded_rectangle([s(52), s(y), s(52 + w), s(y + 11)], radius=s(5.5), fill=TEXT)

img.resize((256, 256), Image.LANCZOS).save(out)
print(os.path.abspath(out))
