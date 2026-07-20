# DVH Analysis — Independent Verification Harness

Eclipse-independent ground truth for validating the DVH Analysis tool's metric output,
computed directly from a DICOM plan export (RTSTRUCT + RTDOSE). Because the tool itself is
100% ESAPI-dependent and cannot run without a live Eclipse, this harness provides the
external reference needed to confirm metric correctness — in particular the **cGy fix**
(dose canonicalization to Gy) on the `cGy_fix` branch.

## Why this exists

DICOM RTDOSE always stores dose in **Gy** (`DoseUnits = GY`), regardless of how Eclipse is
*configured* to display dose (Gy or cGy). The tool's cGy fix canonicalizes ESAPI's dose to Gy
so metrics are correct on either system. This harness computes Gy ground truth from the DICOM,
which you compare against the tool's exported metrics for the **same plan** run on a
cGy-configured Eclipse — a direct end-to-end check that the fix produces correct Gy values.

## Method

For each structure, contour polygons are rasterized onto the dose grid's lattice at a
supersampled resolution (even-odd fill handles holes/islands), the dose volume is trilinearly
interpolated at each contour plane's z, and a volume-weighted cumulative DVH is built. Metrics
follow the standard definitions used in `dvhanalysisgui/resources/Metrics.xml`
(`D_x%`, `D_xcc`, `V_xGy`, `Mean/Min/Max`). `D_x` is the largest dose covering ≥ the target volume.

## Setup

```
cd verification
python -m venv .venv
.venv\Scripts\activate            # Windows PowerShell: .venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

## Usage

```
# Point the tools at your DICOM export (a folder with Dose\ and Structure\ subfolders).
# PowerShell:  $env:DVH_DICOM_DIR = "C:\path\to\export"   (cmd:  set DVH_DICOM_DIR=...)
# or pass --base each time (inspect_dicom.py also takes the folder as its first argument).

# Inspect a plan export (dose units, grid, ROI list)
python inspect_dicom.py

# Compute ground-truth DVH metrics (clinical subset -> reference/ CSV + console table)
python dvh_groundtruth.py --base "<path to folder containing Dose\ and Structure\>"

# All ROIs (excluding "x ..." optimization/helper structures)
python dvh_groundtruth.py --all --out reference\ground_truth_all.csv

# Higher accuracy for small structures (slower)
python dvh_groundtruth.py --ss 3

# Biological metrics: EQD2 (LQ) DVH table + LKB NTCP demo (needs the fraction count from the RTPLAN)
python dvh_groundtruth.py --bio --fractions 10 --ab 2.5
```

The DICOM folder comes from `--base` or the `DVH_DICOM_DIR` environment variable (`--base` wins if
both are set). Output columns: `Volume[cc]`, `Min/Max/Mean[Gy]`,
`D50/90/95/99/100%[Gy]`, `D0.03/0.1/2.0cc[Gy]`, `V12/16/20/30Gy[cc and %]`.

## A/B comparison against the tool (Track B3)

1. In a **cGy-configured Eclipse**, run DVH Analysis on this same plan and export the metrics.
2. Save them as a long-format CSV with columns `Structure,Metric,Value` (Metric names matching
   the ground-truth columns, e.g. `Mean[Gy]`, `D95%[Gy]`, `V20Gy[%]`).
3. Run:
   ```
   python compare_to_tool.py --tool <tool_export.csv> --truth reference\ground_truth_HA-WBRT_clinical.csv
   ```
   The comparator joins on (Structure, Metric) and flags:
   - **GROSS (≈100×)** disagreement → a unit/cGy bug (this is the primary thing to catch).
   - **Methodology** differences (a few %) → expected from interpolation/grid/supersampling
     between this harness and Eclipse's DVH engine; larger on `Max`, `Min`, and tiny-volume
     metrics (`D0.03cc`) for small structures.

## Caveats / accuracy

- Physical-dose metrics (Gy) are the primary, high-confidence comparison. `--bio` also reproduces
  the tool's biological-dose math — EQD2 (LQ via `LQBioDoseDVHModel`, LQL via `LQLBioDoseDVHModel`)
  and LKB NTCP (`EUDMetric` gEUD → `NTCPMetric` with the `NormSDist` normal CDF). EQD2/NTCP depend
  on the **number of fractions**, which lives in the RTPLAN (not part of this dose/structure
  export) — pass `--fractions` to match the plan (default 10 for this HA-WBRT case). NTCP `gEUD` is
  computed from raw voxel EQD2 (the exact limit of the tool's 0.1 Gy-binned `gEUD`).
- Eclipse supersamples structures internally; this harness supersamples the dose grid in-plane
  (`--ss`, default 2 = 1.25 mm). Expect sub-percent-to-few-percent differences on medium/large
  structures and larger scatter on very small ones (cochlea, lens, pituitary).
- Cross-check geometry via the reported `Volume[cc]` vs Eclipse's structure volume: agreement
  there confirms the contour rasterization is correct independent of dose.
