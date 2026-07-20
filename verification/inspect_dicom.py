import glob, os, sys
import pydicom
import numpy as np

# DICOM export folder (containing Dose\ and Structure\ subfolders):
# set the DVH_DICOM_DIR environment variable or pass the folder as the first argument.
BASE = os.environ.get("DVH_DICOM_DIR") or (sys.argv[1] if len(sys.argv) > 1 else None)
if not BASE:
    sys.exit("Set the DVH_DICOM_DIR environment variable or pass the DICOM folder as the first argument.")

def find_one(subdir):
    files = glob.glob(os.path.join(BASE, subdir, "*.dcm"))
    return files[0] if files else None

dose_f = find_one("Dose")
struct_f = find_one("Structure")

print("=== RTDOSE ===", dose_f)
d = pydicom.dcmread(dose_f)
print("Modality:", d.Modality)
print("DoseUnits:", d.get("DoseUnits"))
print("DoseType:", d.get("DoseType"))
print("DoseSummationType:", d.get("DoseSummationType"))
print("GridScaling:", d.get("DoseGridScaling"))
print("Rows,Cols,Frames:", d.Rows, d.Columns, d.get("NumberOfFrames"))
print("PixelSpacing:", d.get("PixelSpacing"), "  ImagePositionPatient:", d.get("ImagePositionPatient"))
gfov = d.get("GridFrameOffsetVector")
print("GridFrameOffsetVector len:", len(gfov) if gfov is not None else None,
      "first/last:", (gfov[0], gfov[-1]) if gfov is not None else None)
print("ImageOrientationPatient:", d.get("ImageOrientationPatient"))
arr = d.pixel_array.astype(np.float64) * float(d.DoseGridScaling)
print("Dose array shape:", arr.shape, "min/max Gy:", round(arr.min(),4), round(arr.max(),4))
print("Frame(z) spacing (mm):", np.diff(np.array(gfov, dtype=float))[:3] if gfov is not None else None)

# Referenced plan (for fractions / prescription context)
rp = d.get("ReferencedRTPlanSequence")
print("Referenced plan present:", rp is not None)

print("\n=== RTSTRUCT ===", struct_f)
s = pydicom.dcmread(struct_f)
print("Modality:", s.Modality)
rois = s.get("StructureSetROISequence", [])
print("Num ROIs:", len(rois))
# Map ROI number -> name
names = {r.ROINumber: r.ROIName for r in rois}
# Count contour points per ROI
contour_counts = {}
for c in s.get("ROIContourSequence", []):
    num = c.get("ReferencedROINumber")
    n = 0
    for cs in c.get("ContourSequence", []) or []:
        n += int(cs.NumberOfContourPoints)
    contour_counts[num] = n
for rn in sorted(names):
    print(f"  ROI {rn:>3}: {names[rn]:<30} contour_pts={contour_counts.get(rn,0)}")
