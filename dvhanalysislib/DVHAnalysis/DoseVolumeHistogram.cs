using System;

namespace DVHAnalysis
{
    public class DoseVolumeHistogram
    {
        public DoseVolumeHistogram(double[] dose, double voxelVolume, double doseBinSize)
        {
            double[] sortedDose = GetSortedArray(dose);
            var nBins = GetNumberOfBins(sortedDose, doseBinSize);

            Curve = new DVPoint[nBins];

            double doseBinLimit = 0.0;
            double volume = dose.Length * voxelVolume;    // Start with the total volume

            int doseIndex = 0;

            for (int binIndex = 0; binIndex < nBins; binIndex++)
            {
                // Note: This while loop will not enter on the first iteration
                // (when binIndex == 0), which is OK because we want the first bin
                // to contain the point (0.0, total_volume).
                while (sortedDose[doseIndex] < doseBinLimit)
                {
                    volume -= voxelVolume;
                    doseIndex++;
                }

                Curve[binIndex] = new DVPoint(doseBinLimit, volume);

                doseBinLimit += doseBinSize;
            }
        }

        private double[] GetSortedArray(double[] array)
        {
            double[] sortedArray = new double[array.Length];
            Array.Copy(array, sortedArray, array.Length);
            Array.Sort(sortedArray);
            return sortedArray;
        }

        private int GetNumberOfBins(double[] sortedDose, double doseBinSize)
        {
            var largestDose = sortedDose[sortedDose.Length - 1];

            // When the largest dose is 0.0, there should be one bin, not zero bins
            return largestDose > 0.0 ? (int)Math.Ceiling(largestDose / doseBinSize) : 1;
        }

        public DVPoint[] Curve { get; }
    }
}
