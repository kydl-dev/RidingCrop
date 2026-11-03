"""
Riding crop for Valheim, v2 (Blender 5.x, tested on 5.2.2 LTS).

Before running:
  1. Save your .blend file.
  2. Generate the textures into a "textures" folder next to it:
         python crop_textures.py "<folder of your .blend>/textures"
     (plain Python with numpy works, or run crop_textures.py's code inside Blender).
Then run this script from Blender's Scripting tab (or through the Blender MCP connector).

Result: one mesh "RidingCrop", 1.0 m long, Z up, origin at the grip, one material "crop_mat"
(Albedo / Normal / MetalGloss atlas), ~392 verts / ~748 tris, UV layer "UVMap".
The UV atlas layout (REG) must stay identical to the one in crop_textures.py.
"""
import bpy, bmesh, math, os
from mathutils import Matrix, Vector

TEX, PXM, STRAP_PXM = 1024, 4000.0, 1500.0
REG = {
    'shaft': (0, 0, 142, 1000), 'handle': (146, 0, 377, 744),
    'ferrule': (381, 0, 652, 128), 'taper': (381, 132, 652, 170),
    'endcap': (381, 174, 652, 246), 'button': (381, 250, 652, 262),
    'swatch_gold': (381, 270, 453, 342), 'swatch_leather': (457, 270, 529, 342),
    'keeper': (381, 350, 652, 438), 'flare': (381, 442, 652, 566),
    'slap_face': (660, 0, 800, 820), 'slap_edge': (804, 0, 816, 820),
    'strap_face': (830, 0, 842, 900), 'strap_edge': (846, 0, 850, 900),
}
TARGET_LENGTH = 1.0   # metres
SLAPPER_TOP = 0.570   # fold centre height in design units (0.545 in v1 -> taller slapper)

def uvp(px, py):
    return (px / TEX, 1 - py / TEX)

old = bpy.data.objects.get("RidingCrop")
if old:
    m = old.data
    bpy.data.objects.remove(old, do_unlink=True)
    if m.users == 0:
        bpy.data.meshes.remove(m)
for n in ("crop_leather_black", "crop_gold", "crop_mat"):
    mt = bpy.data.materials.get(n)
    if mt:
        bpy.data.materials.remove(mt)

bm = bmesh.new()
uv = bm.loops.layers.uv.new("UVMap")

def add_cone(r1, r2, z0, z1, segs, region, matrix=None, swatch='swatch_leather'):
    m = matrix if matrix is not None else Matrix.Translation((0, 0, (z0 + z1) / 2))
    minv = m.inverted()
    ret = bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=segs,
                                radius1=r1, radius2=r2, depth=(z1 - z0), matrix=m)
    faces = {f for v in ret['verts'] for f in v.link_faces}
    rx0, ry0, rx1, ry1 = REG[region]
    sx0, sy0, sx1, sy1 = REG[swatch]
    d, R = z1 - z0, max(r1, r2)
    for f in faces:
        loc = [minv @ l.vert.co for l in f.loops]
        if len(f.verts) > 4:      # end cap: planar projection into a flat swatch
            for l, p in zip(f.loops, loc):
                l[uv].uv = uvp((sx0 + sx1) / 2 + p.x / (2 * R) * (sx1 - sx0) * 0.9,
                               (sy0 + sy1) / 2 - p.y / (2 * R) * (sy1 - sy0) * 0.9)
        else:                     # side wall: cylindrical wrap, seam handled per face corner
            ts = [round(math.atan2(p.y, p.x) / (2 * math.pi), 6) % 1.0 for p in loc]
            if max(ts) - min(ts) > 0.5:
                ts = [t + 1.0 if t < 0.5 else t for t in ts]
            for l, p, t in zip(f.loops, loc, ts):
                l[uv].uv = uvp(rx0 + t * (rx1 - rx0), ry0 + (1 - (p.z + d / 2) / d) * (ry1 - ry0))

def add_ribbon(rings, svals, hws, closed, face_reg, edge_reg, pxm, thick):
    fx0, fy0, fx1, fy1 = REG[face_reg]; ex0, ey0, ex1, ey1 = REG[edge_reg]
    cx = (fx0 + fx1) / 2
    sx0, sy0, sx1, sy1 = REG['swatch_leather']
    lat = [1, -1, -1, 1]
    vr = [[bm.verts.new(p) for p in ring] for ring in rings]
    n = len(vr)
    total = svals[-1] + (rings[0][0] - rings[-1][0]).length if closed else svals[-1]
    for i in range(n if closed else n - 1):
        i2 = (i + 1) % n
        s1, s2 = svals[i], (total if (closed and i2 == 0) else svals[i2])
        for k in range(4):
            corners = [(i, k, s1), (i, (k + 1) % 4, s1), (i2, (k + 1) % 4, s2), (i2, k, s2)]
            f = bm.faces.new((vr[i][k], vr[i][(k + 1) % 4], vr[i2][(k + 1) % 4], vr[i2][k]))
            for l, (ri, j, s) in zip(f.loops, corners):
                if k in (0, 2):
                    l[uv].uv = uvp(cx + lat[j] * hws[ri] * pxm, fy0 + s * pxm)
                else:
                    l[uv].uv = uvp(ex0 if j in (0, 1) else ex1, ey0 + s * pxm)
    if not closed:
        for ri in (0, n - 1):
            f = bm.faces.new(vr[ri])
            for l, j in zip(f.loops, range(4)):
                l[uv].uv = uvp((sx0 + sx1) / 2 + lat[j] * hws[ri] * PXM,
                               (sy0 + sy1) / 2 + (1 if j < 2 else -1) * thick / 2 * PXM)
    return total

def sstep(u):
    u = max(0.0, min(1.0, u))
    return u * u * (3 - 2 * u)

# ---- rigid parts (design units = metres before scale-up, bottom of crop at z = 0) ----
add_cone(0.0102, 0.0102, 0.000, 0.018, 10, 'endcap', swatch='swatch_gold')
add_cone(0.0090, 0.0095, 0.014, 0.200, 10, 'handle')
add_cone(0.0108, 0.0108, 0.200, 0.232, 10, 'ferrule', swatch='swatch_gold')
add_cone(0.0108, 0.0064, 0.232, 0.240, 10, 'taper', swatch='swatch_gold')
add_cone(0.0062, 0.0050, 0.238, 0.488, 8, 'shaft')
add_cone(0.0058, 0.0098, 0.462, 0.492, 8, 'flare')
add_cone(0.0098, 0.0098, 0.172, 0.194, 10, 'keeper')
add_cone(0.0032, 0.0032, -0.00125, 0.00125, 8, 'button', swatch='swatch_gold',
         matrix=Matrix.Translation((0.0105, 0, 0.183)) @ Matrix.Rotation(math.radians(90), 4, 'Y'))

# ---- wrist loop ----
xk, zt, b, c, a = 0.0107, 0.185, 0.150, 0.030, 0.012
w, t, N = 0.007, 0.0014, 32
P = []
for i in range(N):
    th = 2 * math.pi * i / N
    h = (1 - math.cos(th)) / 2
    P.append(Vector((xk + c * h + a * math.sin(th) * (0.4 + 0.6 * h), 0, zt - b * h)))
rings, svals, s = [], [], 0.0
for i in range(N):
    if i:
        s += (P[i] - P[i - 1]).length
    svals.append(s)
    T = (P[(i + 1) % N] - P[i - 1]).normalized()
    Nn, W = Vector((-T.z, 0, T.x)), Vector((0, 1, 0))
    rings.append([P[i] + W * w / 2 + Nn * t / 2, P[i] - W * w / 2 + Nn * t / 2,
                  P[i] - W * w / 2 - Nn * t / 2, P[i] + W * w / 2 - Nn * t / 2])
strap_len = add_ribbon(rings, svals, [w / 2] * N, True, 'strap_face', 'strap_edge', STRAP_PXM, t)

# ---- slapper ----
y0, zb, zf, zc = 0.0075, 0.486, 0.502, SLAPPER_TOP
pts = []
for i in range(6):
    u = i / 5; pts.append((y0 * sstep(u), zb + (zf - zb) * u))
for i in range(1, 6):
    u = i / 5; pts.append((y0, zf + (zc - zf) * u))
for i in range(1, 8):
    ph = math.pi * i / 8; pts.append((y0 * math.cos(ph), zc + y0 * math.sin(ph)))
for i in range(5, -1, -1):
    u = i / 5; pts.append((-y0, zf + (zc - zf) * u))
for i in range(1, 6):
    u = i / 5; pts.append((-y0 * (1 - sstep(u)), zf + (zb - zf) * u))
tS = 0.0022
rings, svals, hws, s = [], [], [], 0.0
for i, (y, z) in enumerate(pts):
    if i:
        s += math.hypot(y - pts[i - 1][0], z - pts[i - 1][1])
    svals.append(s)
    p0, p1 = pts[max(i - 1, 0)], pts[min(i + 1, len(pts) - 1)]
    T = Vector((p1[0] - p0[0], p1[1] - p0[1])).normalized()
    hw = (0.012 + 0.022 * sstep((z - 0.486) / 0.060)) / 2      # tab widens from 12 mm to 34 mm
    hws.append(hw)
    C, Nv, Wv = Vector((0, y, z)), Vector((0, -T.y, T.x)), Vector((1, 0, 0))
    rings.append([C + Wv * hw + Nv * tS / 2, C - Wv * hw + Nv * tS / 2,
                  C - Wv * hw - Nv * tS / 2, C + Wv * hw - Nv * tS / 2])
slap_len = add_ribbon(rings, svals, hws, False, 'slap_face', 'slap_edge', PXM, tS)

# ---- finish: normals, uniform scale to TARGET_LENGTH, origin at the grip ----
bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
zmax = max(v.co.z for v in bm.verts)
sc = TARGET_LENGTH / zmax
grip = 0.10 * sc
for v in bm.verts:
    v.co = v.co * sc
    v.co.z -= grip

mesh = bpy.data.meshes.new("RidingCrop")
bm.to_mesh(mesh)
bm.free()
obj = bpy.data.objects.new("RidingCrop", mesh)
col = bpy.data.collections.get("RidingCrop") or bpy.data.collections.new("RidingCrop")
if col.name not in bpy.context.scene.collection.children:
    bpy.context.scene.collection.children.link(col)
col.objects.link(obj)
obj.location = (0, 0, grip)

# ---- material from the three atlas textures ----
texdir = os.path.join(os.path.dirname(bpy.data.filepath), "textures")
def load(name, cs):
    img = bpy.data.images.load(os.path.join(texdir, name), check_existing=True)
    img.reload()
    img.colorspace_settings.name = cs
    img.filepath = "//textures/" + name
    return img
mat = bpy.data.materials.new("crop_mat")
mat.use_nodes = True
nt = mat.node_tree
bsdf = nt.nodes["Principled BSDF"]
def tex_node(img, x, y):
    n = nt.nodes.new("ShaderNodeTexImage"); n.image = img; n.location = (x, y); return n
albedo = tex_node(load("crop_Albedo.png", "sRGB"), -700, 300)
mgimg = load("crop_MetalGloss.png", "Non-Color"); mgimg.alpha_mode = 'CHANNEL_PACKED'
mg = tex_node(mgimg, -700, 0)
nrm = tex_node(load("crop_Normal.png", "Non-Color"), -700, -300)
sep = nt.nodes.new("ShaderNodeSeparateColor"); sep.location = (-420, 0)
inv = nt.nodes.new("ShaderNodeMath"); inv.operation = 'SUBTRACT'; inv.inputs[0].default_value = 1.0; inv.location = (-420, -120)
nmap = nt.nodes.new("ShaderNodeNormalMap"); nmap.location = (-420, -300)
nt.links.new(albedo.outputs["Color"], bsdf.inputs["Base Color"])
nt.links.new(mg.outputs["Color"], sep.inputs["Color"])
nt.links.new(sep.outputs["Red"], bsdf.inputs["Metallic"])
nt.links.new(mg.outputs["Alpha"], inv.inputs[1])
nt.links.new(inv.outputs["Value"], bsdf.inputs["Roughness"])
nt.links.new(nrm.outputs["Color"], nmap.inputs["Color"])
nt.links.new(nmap.outputs["Normal"], bsdf.inputs["Normal"])
mesh.materials.append(mat)

bpy.context.view_layer.objects.active = obj
for o in bpy.context.view_layer.objects:
    o.select_set(False)
obj.select_set(True)
bpy.ops.object.shade_smooth_by_angle(angle=math.radians(45))
