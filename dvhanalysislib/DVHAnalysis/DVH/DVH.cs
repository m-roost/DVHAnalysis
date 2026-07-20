namespace DVHAnalysis
{
    public class DVH
    {
        public DVHModel DVHModel { get; set; }
        public DVPoint[] CurveData { get; set; }
        public DoseUnit DoseUnit { get; set; }
        public VolumeUnit VolumeUnit { get; set; }
        public double MinDose { get; set; }
        public double MaxDose { get; set; }
        public double MeanDose { get; set; }
        public double MedianDose { get; set; }
        public double StdDevDose { get; set; }
        public double TotalVolume { get; set; }
        public double Coverage { get; set; }
    }
}
