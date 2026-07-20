using System.Xml.Serialization;

namespace DVHAnalysis
{
    [XmlInclude(typeof(MeanDoseMetric))]
    [XmlInclude(typeof(MinDoseMetric))]
    [XmlInclude(typeof(MaxDoseMetric))]
    [XmlInclude(typeof(StdDevDoseMetric))]
    [XmlInclude(typeof(DoseToVolumeMetric))]
    [XmlInclude(typeof(VolumeWithDoseMetric))]
    [XmlInclude(typeof(ColdVolumeWithDoseMetric))]
    [XmlInclude(typeof(DoseComplementToVolumeMetric))]
    [XmlInclude(typeof(NTCPMetric))]
    [XmlInclude(typeof(EUDMetric))]
    [XmlInclude(typeof(SMPCMetric))]
    public abstract class Metric
    {
        public abstract MetricResult Calculate(DVH dvh);
    }
}
