using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVHAnalysis
{
    public static class MetricUnitConverter
    {
        public static MetricUnit FromDoseUnit(DoseUnit doseUnit)
        {
            if (doseUnit == DoseUnit.Unknown)
            {
                return MetricUnit.Invalid;
            }
            else
            {
                // Convert DoseUnits to MetricUnit by name
                return (MetricUnit)Enum.Parse(typeof(MetricUnit), doseUnit.ToString());
            }
        }

        public static MetricUnit FromVolumeUnit(VolumeUnit volumeUnit)
        {
            if (volumeUnit == VolumeUnit.Unknown)
            {
                return MetricUnit.Invalid;
            }
            else
            {
                // Convert VolumeUnits to MetricUnit by name
                return (MetricUnit)Enum.Parse(typeof(MetricUnit), volumeUnit.ToString());
            }
        }
    }
}
