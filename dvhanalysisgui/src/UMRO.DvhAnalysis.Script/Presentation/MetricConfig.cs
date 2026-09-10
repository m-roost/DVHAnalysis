using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation
{
    public class MetricConfig
    {
        public static string ConfigPath = AssemblySettings.MetricsPath;

        public static IEnumerable<DVHMetricSetup> Load()
        {
            XmlSerializer xs = new XmlSerializer(typeof(List<DVHMetricSetup>));

            if (File.Exists(ConfigPath))
            {
                using (Stream stream = new FileStream(ConfigPath, FileMode.Open))
                {
                    var rv = (List<DVHMetricSetup>)xs.Deserialize(stream);

                    return rv;
                }
            }

            return Enumerable.Empty<DVHMetricSetup>();
        }

        public static void Save(IEnumerable<DVHMetricSetup> metrics)
        {
            XmlSerializer xs = new XmlSerializer(typeof(List<DVHMetricSetup>));

            using (Stream stream = new FileStream(ConfigPath, FileMode.Create))
            {
                xs.Serialize(stream, metrics.ToList());
            }
        }
    }
}
