"""
Procedural textures for the Valheim riding crop (numpy + stdlib only, so it runs inside Blender too).
Writes three 1024x1024 PNGs:
  crop_Albedo.png      sRGB colour
  crop_Normal.png      tangent-space normal, +Y up (Unity / OpenGL convention)
  crop_MetalGloss.png  Unity Standard packing: R = metallic, A = smoothness
The atlas layout (REG) must match the UV layout used by build_riding_crop.py.
"""
import numpy as np, zlib, struct, os

TEX = 1024
PXM = 4000.0          # atlas px per metre for rigid parts (design units, before the 1 m scale-up)
# name: (x0, y0, x1, y1) in pixels, origin top-left
REG = {
    'shaft': (0, 0, 142, 1000), 'handle': (146, 0, 377, 744),
    'ferrule': (381, 0, 652, 128), 'taper': (381, 132, 652, 170),
    'endcap': (381, 174, 652, 246), 'button': (381, 250, 652, 262),
    'swatch_gold': (381, 270, 453, 342), 'swatch_leather': (457, 270, 529, 342),
    'keeper': (381, 350, 652, 438), 'flare': (381, 442, 652, 566),
    'slap_face': (660, 0, 800, 820), 'slap_edge': (804, 0, 816, 820),
    'strap_face': (830, 0, 842, 900), 'strap_edge': (846, 0, 850, 900),
}
GOLD_NAMES = ['ferrule', 'taper', 'endcap', 'button', 'swatch_gold']
LEATHER_RGB = np.array([13, 13, 15]) / 255.0
GOLD_RGB = np.array([224, 182, 106]) / 255.0

def blur(a, s):
    fy = np.fft.fftfreq(a.shape[0])[:, None]; fx = np.fft.fftfreq(a.shape[1])[None, :]
    return np.real(np.fft.ifft2(np.fft.fft2(a) * np.exp(-2 * np.pi ** 2 * s ** 2 * (fx ** 2 + fy ** 2))))

def noise(rng, s):
    n = blur(rng.standard_normal((TEX, TEX)), s)
    return n / n.std()

def braid(u, vm, n, pv):
    p = 2 * np.pi * (n * u + vm / pv)
    q = 2 * np.pi * (n * u - vm / pv)
    s1 = np.clip(0.5 + 0.8 * np.cos(p), 0, 1)
    s2 = np.clip(0.5 + 0.8 * np.cos(q), 0, 1)
    par = np.cos(p / 2) * np.cos(q / 2)           # +1 / -1 at crossings: which strand lies on top
    w1 = 0.5 + 0.5 * par
    return np.maximum(s1 * (0.35 + 0.65 * w1), s2 * (0.35 + 0.65 * (1 - w1)))

def grid(name):
    x0, y0, x1, y1 = REG[name]
    ys, xs = np.mgrid[y0:y1, x0:x1]
    return (slice(y0, y1), slice(x0, x1)), (xs - x0 + 0.5) / (x1 - x0), (y1 - ys - 0.5) / PXM

def build_maps(seed=7):
    rng = np.random.default_rng(seed)
    g_fine, g_soft = noise(rng, 0.9), noise(rng, 3.0)
    H = 0.5 + 0.015 * g_fine + 0.01 * g_soft                       # height
    A = np.tile(LEATHER_RGB, (TEX, TEX, 1)) + 0.004 * g_soft[..., None]
    M = np.zeros((TEX, TEX))
    S = np.full((TEX, TEX), 0.28) + 0.03 * g_soft

    for name, n in (('shaft', 4), ('handle', 6)):                # braided leather
        sl, u, vm = grid(name)
        h = braid(u, vm, n, 0.020)
        H[sl] = 0.15 + 0.85 * h + 0.015 * g_fine[sl]
        A[sl] = LEATHER_RGB + (0.045 * h + 0.003 * g_fine[sl])[..., None]
        S[sl] = 0.22 + 0.22 * h

    sl, u, vm = grid('flare')                                     # thread whipping
    h = np.clip(0.5 + 0.5 * np.sin(2 * np.pi * vm / 0.0024), 0, 1)
    H[sl] = 0.2 + 0.8 * h
    A[sl] = LEATHER_RGB + (0.03 * h)[..., None]

    for name in ('slap_face', 'strap_face', 'keeper'):            # smooth leather with fine grain
        sl = grid(name)[0]
        H[sl] = 0.5 + 0.02 * g_fine[sl] + 0.012 * g_soft[sl]
        A[sl] = LEATHER_RGB + (0.008 * g_fine[sl] + 0.006)[..., None]
        S[sl] = 0.42 + 0.03 * g_soft[sl]
    for name in ('slap_edge', 'strap_edge', 'swatch_leather'):
        sl = grid(name)[0]
        H[sl] = 0.5
        A[sl] = (LEATHER_RGB * 1.5)

    for name in GOLD_NAMES:                                       # polished gold, very light wear
        sl, u, vm = grid(name)
        H[sl] = 0.5 + 0.003 * g_fine[sl]
        A[sl] = GOLD_RGB + (0.004 * g_soft[sl])[..., None]
        M[sl] = 1.0
        S[sl] = 0.66 + 0.015 * g_soft[sl]
    return H, A, M, S

def to_maps(H, A, M, S, k=4.0):
    dx = (np.roll(H, -1, 1) - np.roll(H, 1, 1)) / 2
    dy = (np.roll(H, -1, 0) - np.roll(H, 1, 0)) / 2             # d/d(row); rows go down, so +Y(up) = -row
    n = np.stack([-dx * k, dy * k, np.ones_like(H)], -1)
    n /= np.linalg.norm(n, axis=-1, keepdims=True)
    u8 = lambda a: np.clip(a * 255 + 0.5, 0, 255).astype(np.uint8)
    alpha = np.full((TEX, TEX, 1), 255, np.uint8)
    albedo = np.concatenate([u8(A), alpha], -1)
    normal = np.concatenate([u8(n * 0.5 + 0.5), alpha], -1)
    mg = np.zeros((TEX, TEX, 4), np.uint8)
    mg[..., 0] = u8(M); mg[..., 3] = u8(np.clip(S, 0, 1))
    return albedo, normal, mg

def write_png(path, rgba):
    h, w, _ = rgba.shape
    flat = rgba.reshape(h, w * 4)
    f = np.empty((h, 1 + w * 4), np.uint8)
    f[:, 0] = 2                                                   # PNG filter "Up"
    f[0, 1:] = flat[0]
    f[1:, 1:] = flat[1:] - flat[:-1]
    def chunk(tag, data):
        c = struct.pack('>I', len(data)) + tag + data
        return c + struct.pack('>I', zlib.crc32(tag + data) & 0xffffffff)
    with open(path, 'wb') as fh:
        fh.write(b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, 6, 0, 0, 0))
                 + chunk(b'IDAT', zlib.compress(f.tobytes(), 6)) + chunk(b'IEND', b''))

def generate_all(outdir):
    os.makedirs(outdir, exist_ok=True)
    albedo, normal, mg = to_maps(*build_maps())
    paths = {}
    for nm, img in (('crop_Albedo.png', albedo), ('crop_Normal.png', normal), ('crop_MetalGloss.png', mg)):
        paths[nm] = os.path.join(outdir, nm)
        write_png(paths[nm], img)
    return paths

if __name__ == '__main__':
    import sys
    print(generate_all(sys.argv[1] if len(sys.argv) > 1 else 'textures'))
