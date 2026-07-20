using NUnit.Framework;
using VMS.TPS.Common.Model.Types;

namespace DVHAnalysis.UnitTests
{
    // Regression tests that lock in the cGy fix (canonicalization of absolute dose to Gy).
    // These exercise the conversion helpers directly and deterministically, so the cGy code
    // path is covered even when the developer's Eclipse is configured in Gy (where the cGy
    // branch of DVHConverter.FromVarian would never run end-to-end).
    //
    // Note: DVHConverter.FromVarian itself takes a Varian DVHData (in VMS.TPS.Common.Model.API,
    // which this test project does not reference and which cannot be constructed outside ESAPI),
    // so it is not directly unit-testable. Its behavior is exercised here through the public
    // helpers it delegates to (FactorToGy / ToGy / ScaleDose-equivalent arithmetic / Normalized)
    // and through the metric layer on synthetic DVHs. The bio-dose path
    // (BioDoseDVHModel.GetPointBioDoses) normalizes via the same DVHConverter.ToGy(DoseValue)
    // helper asserted below.
    [TestFixture]
    public class DVHConverterCGyTests
    {
        private const double Tol = 1e-9;

        [Test]
        public void FactorToGy_ScalesOnlyCGy()
        {
            Assert.That(DVHConverter.FactorToGy(DoseValue.DoseUnit.cGy), Is.EqualTo(0.01).Within(Tol));
            Assert.That(DVHConverter.FactorToGy(DoseValue.DoseUnit.Gy), Is.EqualTo(1.0).Within(Tol));
            Assert.That(DVHConverter.FactorToGy(DoseValue.DoseUnit.Percent), Is.EqualTo(1.0).Within(Tol));
        }

        [Test]
        public void ToGy_DoseValue_CanonicalizesCGyAndLeavesOthersUnchanged()
        {
            // 3000 cGy -> 30 Gy, relabeled to Gy
            DoseValue cgy = DVHConverter.ToGy(new DoseValue(3000.0, DoseValue.DoseUnit.cGy));
            Assert.That(cgy.Dose, Is.EqualTo(30.0).Within(1e-6));
            Assert.That(cgy.Unit, Is.EqualTo(DoseValue.DoseUnit.Gy));

            // Gy is passed through untouched (non-cGy systems behave exactly as before)
            DoseValue gy = DVHConverter.ToGy(new DoseValue(30.0, DoseValue.DoseUnit.Gy));
            Assert.That(gy.Dose, Is.EqualTo(30.0).Within(Tol));
            Assert.That(gy.Unit, Is.EqualTo(DoseValue.DoseUnit.Gy));

            // Relative (%) dose is passed through untouched
            DoseValue pct = DVHConverter.ToGy(new DoseValue(95.0, DoseValue.DoseUnit.Percent));
            Assert.That(pct.Dose, Is.EqualTo(95.0).Within(Tol));
            Assert.That(pct.Unit, Is.EqualTo(DoseValue.DoseUnit.Percent));
        }

        [Test]
        public void ToGy_Double_ScalesByUnit()
        {
            Assert.That(DVHConverter.ToGy(3000.0, DoseValue.DoseUnit.cGy), Is.EqualTo(30.0).Within(1e-6));
            Assert.That(DVHConverter.ToGy(30.0, DoseValue.DoseUnit.Gy), Is.EqualTo(30.0).Within(Tol));
        }

        [Test]
        public void Normalized_RescalesFirstPointToOneHundredPercent()
        {
            // First point < 100 due to a rounding error in the DVH API; Normalized fixes it.
            var curve = new[]
            {
                new DVPoint(0.0, 99.6),
                new DVPoint(10.0, 60.0),
                new DVPoint(20.0, 30.0),
            };

            DVPoint[] n = DVHConverter.Normalized(curve);

            Assert.That(n[0].Volume, Is.EqualTo(100.0).Within(1e-9));
            Assert.That(n[1].Volume, Is.EqualTo(100.0 * 60.0 / 99.6).Within(1e-9));
            Assert.That(n[2].Volume, Is.EqualTo(100.0 * 30.0 / 99.6).Within(1e-9));
        }

        // A cGy Eclipse system yields a curve whose absolute doses are 100x the Gy values.
        // After canonicalization (dose * FactorToGy(cGy)) the curve is identical to a native-Gy
        // curve, so metrics whose thresholds are defined in Gy (as in Metrics.xml) agree.
        // Without the fix, V20Gy on an unconverted 0-3000 cGy curve would read ~99.6% instead of 50%.
        [Test]
        public void VolumeWithDose_CanonicalizedCGyCurve_MatchesNativeGyCurve()
        {
            var gyDvh = MakeDvh(NativeGyCurve());
            var canonicalizedFromCGy = MakeDvh(CanonicalizeCGy(NativeGyCurve()));

            var v20 = new VolumeWithDoseMetric { Dose = 20.0 };

            double vGy = v20.Calculate(gyDvh).Value;
            double vCanon = v20.Calculate(canonicalizedFromCGy).Value;

            Assert.That(vCanon, Is.EqualTo(vGy).Within(1e-9));
            Assert.That(vGy, Is.EqualTo(50.0).Within(1e-9)); // 50% of volume receives >= 20 Gy
        }

        [Test]
        public void DoseToVolume_D50_OnGyCurve_IsExpected()
        {
            var d50 = new DoseToVolumeMetric { Volume = 50.0 };
            double d = d50.Calculate(MakeDvh(NativeGyCurve())).Value;
            Assert.That(d, Is.EqualTo(20.0).Within(1e-9)); // dose to the 50% volume point
        }

        // ---- helpers ----

        private static DVPoint[] NativeGyCurve()
        {
            return new[]
            {
                new DVPoint(0.0, 100.0),
                new DVPoint(10.0, 80.0),
                new DVPoint(20.0, 50.0),
                new DVPoint(30.0, 10.0),
                new DVPoint(32.0, 0.0),
            };
        }

        // Emulate FromVarian's canonicalization for a cGy system: the cGy reading is 100x the Gy
        // value; multiply by FactorToGy(cGy) to get back to Gy (must reproduce the native curve).
        private static DVPoint[] CanonicalizeCGy(DVPoint[] gyCurve)
        {
            double toGy = DVHConverter.FactorToGy(DoseValue.DoseUnit.cGy);
            var outp = new DVPoint[gyCurve.Length];
            for (int i = 0; i < gyCurve.Length; i++)
            {
                double cGyReading = gyCurve[i].Dose * 100.0;
                outp[i] = new DVPoint(cGyReading * toGy, gyCurve[i].Volume);
            }
            return outp;
        }

        private static DVH MakeDvh(DVPoint[] curve)
        {
            return new DVH
            {
                CurveData = curve,
                DoseUnit = DoseUnit.Gy,
                VolumeUnit = VolumeUnit.Percent,
                TotalVolume = 100.0,
            };
        }
    }
}
