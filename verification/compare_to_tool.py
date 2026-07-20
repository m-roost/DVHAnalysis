"""
Compare the DVH Analysis tool's exported metrics against the independent ground truth.

Ground truth: wide CSV from dvh_groundtruth.py (index=Structure, columns=metric names).
Tool export : long CSV with columns  Structure, Metric, Value  (metric names matching the
              ground-truth columns, e.g. "Mean[Gy]", "D95%[Gy]", "V20Gy[%]").
              (Use --tool-wide if your export is wide like the ground truth.)

Flags per (Structure, Metric):
  GROSS   - ratio ~100x or ~0.01x  => a unit / cGy canonicalization bug (the primary target)
  REVIEW  - difference beyond tolerance but not 100x => investigate
  OK      - within tolerance (expected interpolation/grid/supersampling methodology difference)
"""
import argparse, os, sys
import numpy as np
import pandas as pd


def norm_struct(s):
    return str(s).strip().lower().replace(" ", "").replace("-", "_")


def load_truth(path):
    df = pd.read_csv(path)
    id_col = df.columns[0]
    long = df.melt(id_vars=[id_col], var_name="Metric", value_name="Truth").rename(columns={id_col: "Structure"})
    long["key"] = long["Structure"].map(norm_struct) + "|" + long["Metric"].astype(str).str.strip()
    return long


def load_tool(path, wide):
    df = pd.read_csv(path)
    if wide:
        id_col = df.columns[0]
        df = df.melt(id_vars=[id_col], var_name="Metric", value_name="Value").rename(columns={id_col: "Structure"})
    cols = {c.lower(): c for c in df.columns}
    df = df.rename(columns={cols.get("structure", "Structure"): "Structure",
                            cols.get("metric", "Metric"): "Metric",
                            cols.get("value", "Value"): "Value"})
    df["key"] = df["Structure"].map(norm_struct) + "|" + df["Metric"].astype(str).str.strip()
    return df[["Structure", "Metric", "Value", "key"]]


def classify(truth, tool, abs_tol, rel_tol):
    if pd.isna(truth) or pd.isna(tool):
        return "NA"
    a, b = float(truth), float(tool)
    if abs(a) < 1e-9 and abs(b) < 1e-9:
        return "OK"
    ratio = (b / a) if abs(a) > 1e-9 else np.inf
    if 50 <= ratio <= 200 or 0.005 <= ratio <= 0.02:      # ~100x either direction
        return "GROSS"
    if abs(b - a) <= abs_tol or (abs(a) > 1e-9 and abs(b - a) / abs(a) <= rel_tol):
        return "OK"
    return "REVIEW"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--truth", required=True)
    ap.add_argument("--tool", required=True)
    ap.add_argument("--tool-wide", action="store_true")
    ap.add_argument("--abs-tol", type=float, default=0.5, help="abs tolerance in metric units (Gy or %)")
    ap.add_argument("--rel-tol", type=float, default=0.03, help="relative tolerance (fraction)")
    ap.add_argument("--out", default=None)
    args = ap.parse_args()

    truth = load_truth(args.truth)
    tool = load_tool(args.tool, args.tool_wide)

    merged = tool.merge(truth[["key", "Truth"]], on="key", how="left")
    merged["Diff"] = merged["Value"].astype(float) - merged["Truth"]
    merged["Ratio"] = merged.apply(
        lambda r: (float(r["Value"]) / r["Truth"]) if pd.notna(r["Truth"]) and abs(r["Truth"]) > 1e-9 else np.nan, axis=1)
    merged["Flag"] = merged.apply(lambda r: classify(r["Truth"], r["Value"], args.abs_tol, args.rel_tol), axis=1)

    order = {"GROSS": 0, "REVIEW": 1, "NA": 2, "OK": 3}
    merged = merged.sort_values(by=["Flag", "Structure", "Metric"], key=lambda s: s.map(order).fillna(9) if s.name == "Flag" else s)
    show = merged[["Structure", "Metric", "Truth", "Value", "Diff", "Ratio", "Flag"]].round(3)

    out = args.out or os.path.join(os.path.dirname(os.path.abspath(args.truth)), "comparison_result.csv")
    show.to_csv(out, index=False)

    counts = merged["Flag"].value_counts().to_dict()
    print(f"Compared {len(merged)} metric values: "
          f"GROSS={counts.get('GROSS',0)}  REVIEW={counts.get('REVIEW',0)}  "
          f"OK={counts.get('OK',0)}  NA(unmatched)={counts.get('NA',0)}")
    pd.set_option("display.width", 200, "display.max_rows", 100)
    flagged = show[show["Flag"].isin(["GROSS", "REVIEW"])]
    print("\n=== Flagged (GROSS first, then REVIEW) ===")
    print(flagged.to_string(index=False) if len(flagged) else "  none — all within tolerance")
    print(f"\nWrote {out}")
    if counts.get("GROSS", 0):
        print("\n*** GROSS disagreements present -- indicates a unit/cGy bug. ***")
        sys.exit(2)


if __name__ == "__main__":
    main()
