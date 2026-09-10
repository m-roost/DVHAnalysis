using System.Linq;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DVHAnalysis
{
    // Static methods to convert between Varian and internal types
    public static class DVHConverter
    {
        #region From Varian

        public static DVH FromVarian(DVHData dvhData, DVHModel dvhModel, VolumePresentation vp)
        {
            // Eclipse returns absolute dose in the system-configured unit (Gy or cGy).
            // The rest of the engine (metric parameters, EQD2 formulas, the "Gy" name
            // label) assumes Gy, so canonicalize absolute dose to Gy here. Relative (%)
            // dose and systems already configured for Gy are unaffected (toGy == 1.0).

            double toGy = FactorToGy(dvhData.CurveData[0].DoseValue.Unit);

            return new DVH
            {
                DVHModel = dvhModel,
                CurveData = ScaleDose(GetCurveData(dvhData, vp), toGy),
                DoseUnit = toGy == 1.0 ? GetDoseUnit(dvhData) : DoseUnit.Gy,
                VolumeUnit = ConvertVolumeUnit(vp),
                MinDose = dvhData.MinDose.Dose * toGy,
                MaxDose = dvhData.MaxDose.Dose * toGy,
                MeanDose = dvhData.MeanDose.Dose * toGy,
                MedianDose = dvhData.MedianDose.Dose * toGy,
                StdDevDose = dvhData.StdDev * toGy,
                TotalVolume = dvhData.Volume,
                Coverage = dvhData.Coverage
            };
        }

        public static DVPoint[] GetCurveData(DVHData dvhData, VolumePresentation vp)
        {
            return GetCurveData(dvhData.CurveData, vp);
        }

        public static DoseUnit GetDoseUnit(DVHData dvhData)
        {
            return GetDoseUnit(dvhData.CurveData[0]);
        }

        #endregion

        public static DVPoint[] GetCurveData(DVHPoint[] curveData, VolumePresentation vp)
        {
            // Sometimes there's a rounding error in DVH API,
            // so normalize relative volumes so that the first
            // point's volume is always 100%
            if (vp == VolumePresentation.Relative)
            {
                return Normalized(GetCurveData(curveData));
            }
            else
            {
                return GetCurveData(curveData);
            }
        }

        public static DVPoint[] GetCurveData(DVHPoint[] curveData)
        {
            return (from point in curveData
                    select ConvertDVHPoint(point)).ToArray();
        }

        public static DVPoint[] Normalized(DVPoint[] curveData)
        {
            double maxVolume = curveData[0].Volume;
            return (from point in curveData
                    select new DVPoint(point.Dose,
                        100.0 * point.Volume / maxVolume)).ToArray();
        }

        public static DVPoint ConvertDVHPoint(DVHPoint dvhPoint)
        {
            return new DVPoint(dvhPoint.DoseValue.Dose, dvhPoint.Volume);
        }

        public static DoseUnit GetDoseUnit(DVHPoint dvhPoint)
        {
            // Cast works because enum values are the same
            return (DoseUnit)dvhPoint.DoseValue.Unit;
        }

        public static VolumeUnit ConvertVolumeUnit(VolumePresentation vp)
        {
            switch (vp)
            {
                case VolumePresentation.AbsoluteCm3:
                    return VolumeUnit.cc;
                case VolumePresentation.Relative:
                    return VolumeUnit.Percent;
                default:
                    return VolumeUnit.Unknown;
            }
        }

        #region Dose unit canonicalization (Gy)

        // Factor that converts a value expressed in the given Varian absolute dose
        // unit to Gy. Relative (Percent) and Unknown are left unchanged (1.0).
        public static double FactorToGy(DoseValue.DoseUnit unit)
        {
            switch (unit)
            {
                case DoseValue.DoseUnit.cGy:
                    return 0.01;
                default:
                    return 1.0;
            }
        }

        public static double ToGy(double value, DoseValue.DoseUnit unit)
        {
            return value * FactorToGy(unit);
        }

        // Converts a DoseValue to Gy. Only cGy is rescaled; Gy/Percent/Unknown are
        // returned unchanged so non-cGy systems behave exactly as before.
        public static DoseValue ToGy(DoseValue dose)
        {
            return dose.Unit == DoseValue.DoseUnit.cGy
                ? new DoseValue(dose.Dose * 0.01, DoseValue.DoseUnit.Gy)
                : dose;
        }

        private static DVPoint[] ScaleDose(DVPoint[] points, double factor)
        {
            if (factor == 1.0)
            {
                return points;
            }

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new DVPoint(points[i].Dose * factor, points[i].Volume);
            }

            return points;
        }

        #endregion
    }
}
