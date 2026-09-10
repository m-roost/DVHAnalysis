using VMS.TPS.Common.Model.Types;

namespace DVHAnalysis
{
    public class MetricNamer
    {
        public Metric Metric { get; set; }
        public DVHModel DVHModel { get; set; }

        public MetricNamer(DVHMetricSetup dvhMetricSetup)
        {
            Metric = dvhMetricSetup.Metric;
            DVHModel = dvhMetricSetup.DVHModel;
        }

        public string GetName()
        {
            if (Metric is DoseToVolumeMetric)
            {
                return FormatMetricName((DoseToVolumeMetric)Metric);
            }
            else if (Metric is VolumeWithDoseMetric)
            {
                return FormatMetricName((VolumeWithDoseMetric)Metric);
            }
            else if (Metric is ColdVolumeWithDoseMetric)
            {
                return FormatMetricName((ColdVolumeWithDoseMetric)Metric);
            }
            else if (Metric is DoseComplementToVolumeMetric)
            {
                return FormatMetricName((DoseComplementToVolumeMetric)Metric);
            }
            else if (Metric is MinDoseMetric)
            {
                return FormatMetricName((MinDoseMetric)Metric);
            }
            else if (Metric is MaxDoseMetric)
            {
                return FormatMetricName((MaxDoseMetric)Metric);
            }
            else if (Metric is MeanDoseMetric)
            {
                return FormatMetricName((MeanDoseMetric)Metric);
            }
            else if (Metric is MedianDoseMetric)
            {
                return FormatMetricName((MedianDoseMetric)Metric);
            }
            else if (Metric is StdDevDoseMetric)
            {
                return FormatMetricName((StdDevDoseMetric)Metric);
            }
            else if (Metric is NTCPMetric)
            {
                return FormatMetricName((NTCPMetric)Metric);
            }
            else if (Metric is EUDMetric)
            {
                return FormatMetricName((EUDMetric)Metric);
            }
            else if (Metric is SMPCMetric)
            {
                return FormatMetricName((SMPCMetric)Metric);
            }
            else
            {
                return "Unknown metric";
            }
        }
        private string FormatMetricName(SMPCMetric metric)
        {
            return "SMPC_AutoNamed"; // this is used only when IsAutoName == true
        }

        private string FormatMetricName(DoseToVolumeMetric metric)
        {
            return string.Format("D{0}{1}{2}[{3}]", metric.Volume,
                GetVolumeUnit(), FormatDVHModelParameters(), GetDoseUnit());
        }

        private string FormatMetricName(VolumeWithDoseMetric metric)
        {
            return string.Format("V{0}{1}{2}[{3}]", metric.Dose,
                GetDoseUnit(), FormatDVHModelParameters(), GetVolumeUnit());
        }

        private string FormatMetricName(ColdVolumeWithDoseMetric metric)
        {
            return string.Format("CV{0}{1}{2}[{3}]", metric.Dose,
                GetDoseUnit(), FormatDVHModelParameters(), GetVolumeUnit());
        }

        private string FormatMetricName(DoseComplementToVolumeMetric metric)
        {
            return string.Format("DC{0}{1}{2}[{3}]", metric.Volume,
                GetVolumeUnit(), FormatDVHModelParameters(), GetDoseUnit());
        }

        private string FormatMetricName(MinDoseMetric metric)
        {
            return string.Format("Min{0}[{1}]", FormatDVHModelParameters(), GetDoseUnit());
        }

        private string FormatMetricName(MaxDoseMetric metric)
        {
            return string.Format("Max{0}[{1}]", FormatDVHModelParameters(), GetDoseUnit());
        }

        private string FormatMetricName(MeanDoseMetric metric)
        {
            return string.Format("Mean{0}[{1}]", FormatDVHModelParameters(), GetDoseUnit());
        }

        private string FormatMetricName(MedianDoseMetric metric)
        {
            return string.Format("Median{0}[{1}]", FormatDVHModelParameters(), GetDoseUnit());
        }

        private string FormatMetricName(StdDevDoseMetric metric)
        {
            return string.Format("StdDev{0}[{1}]", FormatDVHModelParameters(), GetDoseUnit());
        }

        private string FormatMetricName(NTCPMetric metric)
        {
            var dvhParameters = GetDVHModelParameters();
            return string.Format("NTCP(n={0}, m={1}, TD50={2}{3})[%]",
                metric.LKBn, metric.LKBm, metric.LKBD50,
                dvhParameters == string.Empty ? string.Empty : ", " + dvhParameters);
        }

        private string FormatMetricName(EUDMetric metric)
        {
            var dvhParameters = GetDVHModelParameters();
            return string.Format("gEUD(a={0}{1})[{2}]",
                metric.a, dvhParameters == string.Empty ? string.Empty : ", " + dvhParameters,
                GetDoseUnit());
        }

        private string GetDoseUnit()
        {
            return DVHModel.DoseType == DoseValuePresentation.Absolute
                ? (DVHModel is BioDoseDVHModel ? "EQD2Gy" : "Gy")
                : "%";
        }

        private string GetVolumeUnit()
        {
            return DVHModel.VolumeType == VolumePresentation.AbsoluteCm3 ? "cc" : "%";
        }

        private string FormatDVHModelParameters()
        {
            var dvhModelParameters = GetDVHModelParameters();

            if (dvhModelParameters == string.Empty)
            {
                return string.Empty;
            }
            else
            {
                return string.Format("({0})", dvhModelParameters);
            }
        }

        private string GetDVHModelParameters()
        {
            if (DVHModel is LQBioDoseDVHModel)
            {
                LQBioDoseDVHModel dvhModel = DVHModel as LQBioDoseDVHModel;
                return string.Format("LQ, \u03b1/\u03b2={0}", dvhModel.AlphaBeta);
            }
            else if (DVHModel is LQLBioDoseDVHModel)
            {
                LQLBioDoseDVHModel dvhModel = DVHModel as LQLBioDoseDVHModel;
                return string.Format("LQL, \u03b1/\u03b2={0}, DT={1}", dvhModel.AlphaBeta, dvhModel.DT);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}