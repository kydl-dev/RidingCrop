"""
Colour variants of the riding crop (albedo only: mesh, normal map and metal/gloss map are shared with the original).
Recolours the regions of crop_Albedo.png using the atlas layout REG from crop_textures.py:
  stick  = all black leather (shaft, handle, slapper, strap, keeper, edges)
  gold   = ferrule, taper, end cap, button (stays metallic: only the colour changes)
  spiral = the thread-whipped flare ('flare' region) at the top of the shaft
Run:  python crop_variants.py "<blend folder>/textures"
"""
import numpy as np, os, sys
import crop_textures as ct

def rgb(r, g, b): return np.array([r, g, b]) / 255.0
def darker(c, f=0.7): return c * f      # f=0.7 -> 30% darker

BLACK = rgb(13, 13, 15)          # the original leather colour
TRIM_BLACK = rgb(14, 14, 16)
RED = rgb(185, 15, 22)
GREEN = darker(rgb(18, 85, 34))   # darker green, then another 30% darker
WHITE = rgb(214, 214, 214)
GREY = rgb(128, 128, 132)
DIRTY_WHITE = darker(rgb(176, 170, 158))   # dingy warm off-white, then another 30% darker
BROWN = rgb(110, 66, 35)
ORANGE = darker(rgb(170, 70, 8))  # darker orange, then another 30% darker
BROWN_DARK = darker(BROWN)       # V5 spiral

# name: (stick, gold, spiral); None = keep the original colour
VARIANTS = {
    'V1': (RED,   TRIM_BLACK, WHITE),
    'V2': (GREEN, TRIM_BLACK, TRIM_BLACK),
    'V3': (None,  RED,        RED),
    'V4': (WHITE, TRIM_BLACK, TRIM_BLACK),
    'V5': (DIRTY_WHITE, BROWN, BROWN_DARK),
    'V6': (ORANGE, TRIM_BLACK, TRIM_BLACK),
}

def region_mask(names):
    m = np.zeros((ct.TEX, ct.TEX), bool)
    for n in names:
        x0, y0, x1, y1 = ct.REG[n]
        m[y0:y1, x0:x1] = True
    return m

def recolor_leather(A, mask, target):
    """Keep the braid / grain brightness pattern of the black original, applied on top of `target`."""
    lum = A.mean(-1)
    rel = np.clip((lum - BLACK.mean()) / 0.045, -0.15, 1.1)           # 0 = flat leather, 1 = braid highlight
    c = min(0.9, 0.97 / target.max() - 1.0)                            # contrast that cannot clip the brightest channel
    out = target[None, None, :] * (1.0 + c * rel[..., None])
    A[mask] = np.clip(out[mask], 0, 1)

def recolor_metal(A, mask, target):
    """Keep the tiny wear noise of the gold, around the new colour."""
    A[mask] = np.clip(target[None, :] + (A[mask] - ct.GOLD_RGB[None, :]), 0, 1)

def make(variant, base_albedo):
    stick, gold, spiral = VARIANTS[variant]
    A = base_albedo.copy()
    gold_m = region_mask(ct.GOLD_NAMES)
    flare_m = region_mask(['flare'])
    stick_m = ~(gold_m | flare_m)
    if stick is not None: recolor_leather(A, stick_m, stick)
    recolor_metal(A, gold_m, gold)
    recolor_leather(A, flare_m, spiral)
    return A

def generate(outdir):
    H, A, M, S = ct.build_maps()
    paths = {}
    for v in VARIANTS:
        Av = make(v, A)
        rgba = np.concatenate([np.clip(Av * 255 + 0.5, 0, 255).astype(np.uint8),
                               np.full((ct.TEX, ct.TEX, 1), 255, np.uint8)], -1)
        p = os.path.join(outdir, f'crop_Albedo_{v}.png')
        ct.write_png(p, rgba)
        paths[v] = p
    return paths

if __name__ == '__main__':
    print(generate(sys.argv[1] if len(sys.argv) > 1 else 'textures'))
