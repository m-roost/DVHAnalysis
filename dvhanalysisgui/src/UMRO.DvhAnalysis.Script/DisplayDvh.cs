using System.Windows.Media;
using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script
{
    public class DisplayDvh
    {
        public string Name { get; set; }
        public DVH Dvh { get; set; }
        public Color Color { get; set; }
        public DvhLineType LineType { get; set; }
    }
}