"""
Independent DVH ground-truth harness for validating the UMRO DVH Analysis tool.

Reads RTSTRUCT + RTDOSE straight from a DICOM plan export and computes cumulative
DVHs + the metric suite (matching resources/Metrics.xml) with NO dependence on
Eclipse/ESAPI. Dose is read in the RTDOSE native unit (Gy per DICOM standard), so
the output is Gy ground truth suitable for A/B comparison against the tool's export.

Method: for each structure contour plane, rasterize the polygon(s) onto the dose
grid's lattice at a supersampled resolution (even-odd fill handles holes), trilinearly
interpolate the dose volume at that plane's z, and accumulate (dose, volume) samples.
Metrics are computed from the resulting volume-weighted cumulative DVH.

--bio also computes biological-dose (EQD2) DVH metrics and LKB NTCP, reproducing the
formulas in dvhanalysislib (LQBioDoseDVHModel / LQLBioDoseDVHModel / EUDMetric /
NTCPMetric). EQD2 needs the number of fractions, which lives in the RTPLAN (not part of
this dose/structure export) -- pass --fractions to match the plan (HA-WBRT default 10).
"""
import argparse, glob, os, sys
import numpy as np
import pydicom
from matplotlib.path import Path
from scipy.interpolate import RegularGridInterpolator
from scipy.stats import norm
import pandas as pd

# ---- Structures to evaluate (clinically relevant for HA-WBRT). Override with --all. ----
DEFAULT_STRUCTS = [
    "Brain", "PTV_3000", "CTV_3000", "GTVm", "PTV_WBopt07", "CTV", "PTV_Dura",
    "BrainStem", "SpinalCanal", "OpticChiasm", "OpticNerve_L", "OpticNerve_R",
    "Eye_L", "Eye_R", "Lens_L", "Lens_R", "Cochlea_L", "Cochlea_R",
    "Hippocampus_L", "Hippocampus_R", "Hippocampi", "Hippocampi_PRV5", "Pituitary",
]

# Predefined LKB NTCP models from resources/Metrics.xml: (model, ab, dt, n, m, d50).
# model "LQ" ignores dt. Parameters are in EQD2 Gy.
NTCP_MODELS = {
    "Clinical NTCP Lung":          ("LQ",  2.5, None, 0.99, 0.37, 30.8),
    "Clinical NTCP Liver Primary": ("LQ",  2.5, None, 0.97, 0.12, 35.4),
    "zColon NTCP":                 ("LQL", 2.5, 5,    0.17, 0.11, 55.0),
    "zBowel NTCP":                 ("LQL", 2.5, 5,    0.15, 0.16, 55.0),
    "zCord NTCP":                  ("LQL", 2.5, 5,    0.05, 0.175, 66.5),
    "zHeart NTCP":                 ("LQL", 2.5, 5,    0.35, 0.10, 48.0),
}
# NTCP demo (structure -> predefined model). Brain-case OARs have no brain-specific model in
# Metrics.xml; SpinalCanal/BrainStem use the serial cord model to demonstrate the calculation.
NTCP_DEMO = [("SpinalCanal", "zCord NTCP"), ("BrainStem", "zCord NTCP")]


def load_dose(path):
    d = pydicom.dcmread(path)
    scaling = float(d.DoseGridScaling)
    grid = d.pixel_array.astype(np.float64) * scaling  # (frames=z, rows=y, cols=x), Gy
    ipp = np.array(d.ImagePositionPatient, dtype=float)
    ps = np.array(d.PixelSpacing, dtype=float)          # [row spacing (y), col spacing (x)]
    gfov = np.array(d.GridFrameOffsetVector, dtype=float)
    z = ipp[2] + gfov
    y = ipp[1] + np.arange(d.Rows) * ps[0]
    x = ipp[0] + np.arange(d.Columns) * ps[1]
    interp = RegularGridInterpolator((z, y, x), grid, bounds_error=False, fill_value=0.0)
    return dict(grid=grid, z=z, y=y, x=x, ps=ps, interp=interp,
                units=str(d.get("DoseUnits")), maxdose=float(grid.max()))

def load_struct(path):
    s = pydicom.dcmread(path)
    names = {int(r.ROINumber): str(r.ROIName) for r in s.StructureSetROISequence}
    contours = {}  # roi_num -> list of (z, Nx3 array)
    for c in s.ROIContourSequence:
        num = int(c.ReferencedROINumber)
        planes = []
        for cs in c.get("ContourSequence", []) or []:
            if cs.get("ContourGeometricType") not in ("CLOSED_PLANAR", "CLOSEDPLANAR", None):
                continue
            pts = np.array(cs.ContourData, dtype=float).reshape(-1, 3)
            if len(pts) >= 3:
                planes.append((float(pts[0, 2]), pts))
        if planes:
            contours[num] = planes
    return names, contours

def plane_thickness(zs):
    zs = np.unique(np.round(zs, 3))
    if len(zs) < 2:
        return 2.5  # fall back to dose slice thickness
    return float(np.median(np.diff(zs)))

def structure_samples(planes, dose, ss=2):
    """Return (doses, volumes_cc) sampled over the structure using supersampled rasterization."""
    px_x = dose["ps"][1]   # col spacing (x)
    px_y = dose["ps"][0]   # row spacing (y)
    thick = plane_thickness([z for z, _ in planes])
    cell_area = (px_x / ss) * (px_y / ss)              # mm^2 per fine cell
    cell_vol_cc = cell_area * thick / 1000.0           # mm^3 -> cc

    # group polygons by plane z (holes/islands share a z)
    byz = {}
    for z, pts in planes:
        byz.setdefault(round(z, 3), []).append(pts)

    doses, vols = [], []
    for z, polys in byz.items():
        allpts = np.vstack(polys)
        xmin, xmax = allpts[:, 0].min(), allpts[:, 0].max()
        ymin, ymax = allpts[:, 1].min(), allpts[:, 1].max()
        # fine sample lattice (cell centers) across the plane bbox
        gx = np.arange(xmin + px_x / (2 * ss), xmax, px_x / ss)
        gy = np.arange(ymin + px_y / (2 * ss), ymax, px_y / ss)
        if gx.size == 0 or gy.size == 0:
            continue
        GX, GY = np.meshgrid(gx, gy)
        flat = np.column_stack([GX.ravel(), GY.ravel()])
        inside = np.zeros(len(flat), dtype=bool)
        for poly in polys:                              # even-odd fill via XOR (handles holes)
            inside ^= Path(poly[:, :2]).contains_points(flat)
        if not inside.any():
            continue
        sel = flat[inside]
        zz = np.full(len(sel), z)
        dvals = dose["interp"](np.column_stack([zz, sel[:, 1], sel[:, 0]]))
        doses.append(dvals)
        vols.append(np.full(len(dvals), cell_vol_cc))
    if not doses:
        return np.array([]), np.array([])
    return np.concatenate(doses), np.concatenate(vols)

def cumulative_dvh(doses, vols, bin_gy=0.01):
    """Return (dose_bins, cum_vol_cc) cumulative DVH: volume receiving >= dose."""
    if doses.size == 0:
        return np.array([0.0]), np.array([0.0])
    dmax = doses.max()
    edges = np.arange(0, dmax + 2 * bin_gy, bin_gy)
    hist, _ = np.histogram(doses, bins=edges, weights=vols)
    cum = np.cumsum(hist[::-1])[::-1]                    # volume at dose >= edge
    return edges[:-1], cum

def d_at_volume(dbins, cum, vol_cc):
    """Dose (Gy) received by at least vol_cc of the structure.

    Standard DVH definition: D_v = the largest dose d for which cumulative volume(>=d) >= v.
    cum is non-increasing in dose; find the bin where cum crosses v and interpolate within it.
    (Plain np.interp fails at the flat top, e.g. D100%, returning 0.)
    """
    if cum[0] <= 0:
        return np.nan
    v = min(vol_cc, cum[0])
    below = cum < v
    if not below.any():            # curve never drops below v -> highest dose bin
        return float(dbins[-1])
    i = int(np.argmax(below))      # first bin where cum < v
    if i == 0:
        return float(dbins[0])
    d0, d1, c0, c1 = dbins[i - 1], dbins[i], cum[i - 1], cum[i]
    if c0 == c1:
        return float(d0)
    return float(d0 + (v - c0) * (d1 - d0) / (c1 - c0))

def v_at_dose(dbins, cum, dose_gy):
    """Volume (cc) receiving >= dose_gy."""
    return float(np.interp(dose_gy, dbins, cum))

def metrics_for(doses, vols, unit="Gy"):
    dbins, cum = cumulative_dvh(doses, vols)
    total = float(vols.sum())
    m = {}
    m["Volume[cc]"] = total
    if doses.size == 0 or total <= 0:
        return m, dbins, cum
    m[f"Min[{unit}]"] = float(doses.min())
    m[f"Max[{unit}]"] = float(doses.max())
    m[f"Mean[{unit}]"] = float(np.average(doses, weights=vols))
    for p in (50, 90, 95, 99, 100):
        m[f"D{p}%[{unit}]"] = d_at_volume(dbins, cum, total * p / 100.0)
    for cc in (0.03, 0.1, 2.0):
        m[f"D{cc}cc[{unit}]"] = d_at_volume(dbins, cum, cc)
    for gy in (12, 16, 20, 30):
        v = v_at_dose(dbins, cum, gy)
        m[f"V{gy}{unit}[cc]"] = v
        m[f"V{gy}{unit}[%]"] = 100.0 * v / total
    return m, dbins, cum


# ---- Biological dose (EQD2) and LKB NTCP -- mirrors dvhanalysislib exactly ----

def eqd2_lq(dose_gy, fractions, ab):
    """Per-voxel LQ EQD2 (LQBioDoseDVHModel.Eqd2): D * (D/n + ab) / (2 + ab)."""
    dpf = dose_gy / fractions
    return dose_gy * (dpf + ab) / (2.0 + ab)

def _calc_re(dpf, dt, ab_inv):
    """LQLBioDoseDVHModel.CalcRE (vectorized)."""
    return np.where(dpf < dt,
                    1.0 + dpf * ab_inv,
                    (dt + dt * dt * ab_inv + (1.0 + 2.0 * dt * ab_inv) * (dpf - dt)) / dpf)

def eqd2_lql(dose_gy, fractions, ab, dt):
    """Per-voxel LQL EQD2 (LQLBioDoseDVHModel.Eqd2Lql)."""
    ab_inv = 1.0 / ab
    dpf = dose_gy / fractions
    return dose_gy * _calc_re(dpf, dt, ab_inv) / _calc_re(np.float64(2.0), dt, ab_inv)

def geud(doses, vols, a):
    """Generalized EUD (EUDMetric): (sum over D_i>0 of (v_i/V) * D_i^a)^(1/a)."""
    V = float(np.sum(vols))
    mask = doses > 0
    if not mask.any() or V <= 0:
        return float("nan")
    s = np.sum((vols[mask] / V) * np.power(doses[mask], a))
    return float(s ** (1.0 / a))

def ntcp_lkb(eqd2_doses, vols, n, m, d50):
    """LKB NTCP (NTCPMetric): gEUD(a=1/n) on the EQD2 DVH, then normal CDF of the LKB operand.
    Returns (ntcp_fraction, gEUD)."""
    g = geud(eqd2_doses, vols, 1.0 / n)
    t = (g - d50) / (m * d50)
    if t > 4.89:                 # tool caps the operand to avoid a stats-package overflow
        t = 4.89
    ntcp = float(norm.cdf(t))    # == (1 + erf(t/sqrt2))/2, the tool's Statistics.NormSDist
    return max(0.0, ntcp), g


def compute_targets(dose, names, contours, target_names, ss):
    name_to_num = {v: k for k, v in names.items()}
    samples, rows = {}, []
    for name in target_names:
        num = name_to_num.get(name)
        if num is None or num not in contours:
            print(f"  ! {name}: no contours"); continue
        doses, vols = structure_samples(contours[num], dose, ss=ss)
        samples[name] = (doses, vols)
        m, _, _ = metrics_for(doses, vols)
        m = {"Structure": name, **{k: (round(v, 3) if isinstance(v, float) else v) for k, v in m.items()}}
        rows.append(m)
        print(f"  {name:<18} vol={m['Volume[cc]']:>8.2f}cc  "
              f"Dmean={m.get('Mean[Gy]', float('nan')):>6.2f}  Dmax={m.get('Max[Gy]', float('nan')):>6.2f}  "
              f"D95%={m.get('D95%[Gy]', float('nan')):>6.2f}  D0.03cc={m.get('D0.03cc[Gy]', float('nan')):>6.2f}")
    return samples, rows


def run_bio(samples, fractions, ab, phys_out):
    print(f"\n[BIO] EQD2 with fractions={fractions}, alpha/beta={ab} Gy  "
          f"(RTPLAN not in export -- pass --fractions to match the plan)")

    # EQD2 (LQ) DVH metrics per structure
    brows = []
    for name, (doses, vols) in samples.items():
        if doses.size == 0:
            continue
        eqd2 = eqd2_lq(doses, fractions, ab)
        m, _, _ = metrics_for(eqd2, vols, unit="EQD2Gy")
        brows.append({"Structure": name,
                      "Volume[cc]": round(m["Volume[cc]"], 3),
                      "Mean[EQD2Gy]": round(m.get("Mean[EQD2Gy]", float('nan')), 3),
                      "Max[EQD2Gy]": round(m.get("Max[EQD2Gy]", float('nan')), 3),
                      "D95%[EQD2Gy]": round(m.get("D95%[EQD2Gy]", float('nan')), 3),
                      "D0.03cc[EQD2Gy]": round(m.get("D0.03cc[EQD2Gy]", float('nan')), 3)})
    bdf = pd.DataFrame(brows).set_index("Structure")
    bout = phys_out.replace(".csv", f"_EQD2_LQ_ab{ab}.csv")
    bdf.to_csv(bout)
    print(f"\n=== EQD2 (LQ, alpha/beta={ab}) DVH metrics [EQD2Gy] ===")
    print(bdf.to_string())
    print(f"Wrote {bout}")

    # LKB NTCP demo
    nrows = []
    for struct_name, model_name in NTCP_DEMO:
        if struct_name not in samples:
            continue
        doses, vols = samples[struct_name]
        if doses.size == 0:
            continue
        model, m_ab, m_dt, lkb_n, lkb_m, d50 = NTCP_MODELS[model_name]
        eqd2 = eqd2_lq(doses, fractions, m_ab) if model == "LQ" else eqd2_lql(doses, fractions, m_ab, m_dt)
        ntcp, g = ntcp_lkb(eqd2, vols, lkb_n, lkb_m, d50)
        nrows.append({"Structure": struct_name, "Model": model_name,
                      "gEUD[EQD2Gy]": round(g, 3), "NTCP[%]": round(100 * ntcp, 4)})
    if nrows:
        print(f"\n=== LKB NTCP demo (fractions={fractions}) ===")
        print(pd.DataFrame(nrows).to_string(index=False))
        print("Note: predefined NTCP models are not brain-specific; SpinalCanal/BrainStem use the")
        print("serial cord model here to demonstrate the calculation for validation against the tool.")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--base", default=os.environ.get("DVH_DICOM_DIR"),
                    help="folder containing Dose\\ and Structure\\ subfolders "
                         "(or set the DVH_DICOM_DIR environment variable)")
    ap.add_argument("--ss", type=int, default=2, help="in-plane supersample factor")
    ap.add_argument("--all", action="store_true", help="all ROIs except 'x ...' helpers")
    ap.add_argument("--bio", action="store_true", help="also compute EQD2 (LQ) metrics + LKB NTCP demo")
    ap.add_argument("--fractions", type=int, default=10,
                    help="fractions for EQD2 (RTPLAN not in export; HA-WBRT default 10)")
    ap.add_argument("--ab", type=float, default=2.5, help="alpha/beta for the EQD2 (LQ) bio table")
    ap.add_argument("--out", default=None)
    args = ap.parse_args()
    if not args.base:
        ap.error("provide --base <dicom folder> or set the DVH_DICOM_DIR environment variable")

    dose_f = glob.glob(os.path.join(args.base, "Dose", "*.dcm"))[0]
    struct_f = glob.glob(os.path.join(args.base, "Structure", "*.dcm"))[0]
    dose = load_dose(dose_f)
    names, contours = load_struct(struct_f)
    print(f"Dose units={dose['units']}  max={dose['maxdose']:.3f} Gy  supersample={args.ss}x")

    if args.all:
        targets = [n for n in names.values() if not n.lower().startswith("x ")]
    else:
        name_set = set(names.values())
        targets = [n for n in DEFAULT_STRUCTS if n in name_set]

    samples, rows = compute_targets(dose, names, contours, targets, args.ss)

    df = pd.DataFrame(rows).set_index("Structure")
    out = args.out or os.path.join(os.path.dirname(__file__), "reference", "dvh_groundtruth.csv")
    df.to_csv(out)
    print(f"\nWrote {out}")
    pd.set_option("display.width", 220, "display.max_columns", 40)
    print("\n=== Ground-truth DVH metrics (physical dose, Gy / cc) ===")
    print(df.to_string())

    if args.bio:
        run_bio(samples, args.fractions, args.ab, out)


if __name__ == "__main__":
    main()
