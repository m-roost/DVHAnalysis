using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVHAnalysis
{
    // Values must match those of Varian's DoseUnits
    public enum DoseUnit
    {
        Unknown = 0,
        Gy = 1,
        cGy = 2,
        Percent = 3
    }
}
