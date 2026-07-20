using System.Windows.Media;

namespace UMRO.DvhAnalysis.Script
{
    public class DvhColors
    {
        private readonly string[] _colorCodes =
        {
            "#d40000",
            "#0055d4",
            "#2ca02c",
            "#8800aa",
            "#ff6600",
            "#ffcc00",
            "#784421",
            "#ff80b2",
            "#999999",
            "#a02c2c",
            "#0088aa",
            "#88aa00",
            "#aa0088",
            "#c87137",
            "#806600",
            "#aa4400",
            "#d40055",
            "#4d4d4d",
            "#447821",
            "#006680"
        };

        public Color Next()
        {
            return ConvertToColor(_colorCodes[_currentColorIndex++ % _colorCodes.Length]);
        }

        private Color ConvertToColor(string colorCode)
        {
            return (Color)ColorConverter.ConvertFromString(colorCode);
        }

        private int _currentColorIndex = 0;
    }
}
