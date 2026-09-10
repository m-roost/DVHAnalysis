using System;

namespace UMRO.DvhAnalysis.Script
{
    public class ProgressEventArgs : EventArgs
    {
        public double Progress { get; set; }

        public ProgressEventArgs(double progress)
        {
            Progress = progress;
        }
    }
}