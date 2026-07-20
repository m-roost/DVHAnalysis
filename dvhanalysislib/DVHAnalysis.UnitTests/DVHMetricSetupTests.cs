using NUnit.Framework;

namespace DVHAnalysis.UnitTests
{
    [TestFixture]
    public class DVHMetricSetupTests
    {
        [Test]
        public void ReturnsTheCorrectNameForTheDVHModelAndMetric()
        {
            var dvhSetup = new DVHMetricSetup();

            // Min
            dvhSetup.Metric = new MinDoseMetric();

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Min[Gy]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Min[%]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().WithBioParams(2.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Min(LQ, α/β=2.5)[EQD2Gy]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().WithBioParams(3).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Min(LQ, α/β=3)[EQD2Gy]"));

            dvhSetup.DVHModel = LQLModel().AbsoluteDose().WithBioParams(3, 1).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Min(LQL, α/β=3, DT=1)[EQD2Gy]"));

            // Max
            dvhSetup.Metric = new MaxDoseMetric();

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Max[Gy]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Max[%]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().WithBioParams(2.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Max(LQ, α/β=2.5)[EQD2Gy]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().WithBioParams(3).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Max(LQ, α/β=3)[EQD2Gy]"));

            dvhSetup.DVHModel = LQLModel().AbsoluteDose().WithBioParams(3, 1).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Max(LQL, α/β=3, DT=1)[EQD2Gy]"));

            // Mean
            dvhSetup.Metric = new MeanDoseMetric();

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Mean[Gy]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Mean[%]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().WithBioParams(2.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Mean(LQ, α/β=2.5)[EQD2Gy]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().WithBioParams(3).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Mean(LQ, α/β=3)[EQD2Gy]"));

            dvhSetup.DVHModel = LQLModel().AbsoluteDose().WithBioParams(3, 1).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Mean(LQL, α/β=3, DT=1)[EQD2Gy]"));

            // Median
            dvhSetup.Metric = new MedianDoseMetric();

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Median[Gy]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Median[%]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().WithBioParams(2.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Median(LQ, α/β=2.5)[EQD2Gy]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().WithBioParams(3).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Median(LQ, α/β=3)[EQD2Gy]"));

            dvhSetup.DVHModel = LQLModel().AbsoluteDose().WithBioParams(3, 1).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("Median(LQL, α/β=3, DT=1)[EQD2Gy]"));

            // StdDev
            dvhSetup.Metric = new StdDevDoseMetric();

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("StdDev[Gy]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("StdDev[%]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().WithBioParams(2.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("StdDev(LQ, α/β=2.5)[EQD2Gy]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().WithBioParams(3).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("StdDev(LQ, α/β=3)[EQD2Gy]"));

            dvhSetup.DVHModel = LQLModel().AbsoluteDose().WithBioParams(3, 1).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("StdDev(LQL, α/β=3, DT=1)[EQD2Gy]"));

            // Dose to Volume
            var doseToVolume = new DoseToVolumeMetric {Volume = 10};
            dvhSetup.Metric = doseToVolume;

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().AbsoluteVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("D10cc[Gy]"));

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().RelativeVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("D10%[Gy]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().RelativeVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("D10%[%]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().AbsoluteVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("D10cc[%]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().AbsoluteVolume()
                .WithBioParams(2.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("D10cc(LQ, α/β=2.5)[EQD2Gy]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().RelativeVolume()
                .WithBioParams(2.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("D10%(LQ, α/β=2.5)[EQD2Gy]"));

            dvhSetup.DVHModel = LQLModel().AbsoluteDose().AbsoluteVolume()
                .WithBioParams(4.0, 2.1).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("D10cc(LQL, α/β=4, DT=2.1)[EQD2Gy]"));

            // Volume with Dose
            var volumeWithDose = new VolumeWithDoseMetric {Dose = 0.1};
            dvhSetup.Metric = volumeWithDose;

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().AbsoluteVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("V0.1Gy[cc]"));

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().RelativeVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("V0.1Gy[%]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().RelativeVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("V0.1%[%]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().AbsoluteVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("V0.1%[cc]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().RelativeVolume()
                .WithBioParams(3.0).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("V0.1EQD2Gy(LQ, α/β=3)[%]"));

            dvhSetup.DVHModel = LQLModel().AbsoluteDose().AbsoluteVolume()
                .WithBioParams(2.0, 0.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("V0.1EQD2Gy(LQL, α/β=2, DT=0.5)[cc]"));

            // Dose Complement to Volume
            var doseComplementToVolume = new DoseComplementToVolumeMetric {Volume = 10};
            dvhSetup.Metric = doseComplementToVolume;

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().AbsoluteVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("DC10cc[Gy]"));

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().RelativeVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("DC10%[Gy]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().RelativeVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("DC10%[%]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().AbsoluteVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("DC10cc[%]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().AbsoluteVolume()
                .WithBioParams(2.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("DC10cc(LQ, α/β=2.5)[EQD2Gy]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().RelativeVolume()
                .WithBioParams(2.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("DC10%(LQ, α/β=2.5)[EQD2Gy]"));

            dvhSetup.DVHModel = LQLModel().AbsoluteDose().AbsoluteVolume()
                .WithBioParams(4.0, 2.1).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("DC10cc(LQL, α/β=4, DT=2.1)[EQD2Gy]"));

            // Cold Volume with Dose
            var coldVolumeWithDose = new ColdVolumeWithDoseMetric {Dose = 0.1};
            dvhSetup.Metric = coldVolumeWithDose;

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().AbsoluteVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("CV0.1Gy[cc]"));

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().RelativeVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("CV0.1Gy[%]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().RelativeVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("CV0.1%[%]"));

            dvhSetup.DVHModel = StandardModel().RelativeDose().AbsoluteVolume().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("CV0.1%[cc]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().RelativeVolume()
                .WithBioParams(3.0).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("CV0.1EQD2Gy(LQ, α/β=3)[%]"));

            dvhSetup.DVHModel = LQLModel().AbsoluteDose().AbsoluteVolume()
                .WithBioParams(2.0, 0.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("CV0.1EQD2Gy(LQL, α/β=2, DT=0.5)[cc]"));

            // EUD
            var eud = new EUDMetric {a = 0.1};
            dvhSetup.Metric = eud;

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("gEUD(a=0.1)[Gy]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().WithBioParams(2.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("gEUD(a=0.1, LQ, α/β=2.5)[EQD2Gy]"));

            dvhSetup.DVHModel = LQLModel().AbsoluteDose().WithBioParams(2.5, 0.1).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("gEUD(a=0.1, LQL, α/β=2.5, DT=0.1)[EQD2Gy]"));

            // NTCP
            var ntcp = new NTCPMetric {LKBn = 0.97, LKBm = 0.12, LKBD50 = 40.7};
            dvhSetup.Metric = ntcp;

            dvhSetup.DVHModel = StandardModel().AbsoluteDose().Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("NTCP(n=0.97, m=0.12, TD50=40.7)[%]"));

            dvhSetup.DVHModel = LQModel().AbsoluteDose().WithBioParams(2.5).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("NTCP(n=0.97, m=0.12, TD50=40.7, LQ, α/β=2.5)[%]"));

            dvhSetup.DVHModel = LQLModel().AbsoluteDose().WithBioParams(2.5, 1.0).Build();
            Assert.That(dvhSetup.Name, Is.EqualTo("NTCP(n=0.97, m=0.12, TD50=40.7, LQL, α/β=2.5, DT=1)[%]"));
        }

        private DVHModelBuilder StandardModel()
        {
            return new DVHModelBuilder("Standard");
        }

        private DVHModelBuilder LQModel()
        {
            return new DVHModelBuilder("LQ");
        }

        private DVHModelBuilder LQLModel()
        {
            return new DVHModelBuilder("LQL");
        }
    }
}
