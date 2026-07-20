using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation
{
    public class TemplateConfig
    {
        public static string ConfigPath = AssemblySettings.TemplatesPath;

        public static IEnumerable<Template> Load()
        {
            XmlSerializer xs = new XmlSerializer(typeof(List<Template>));

            if (File.Exists(ConfigPath))
            {
                using (Stream stream = new FileStream(ConfigPath, FileMode.Open))
                {
                    return (List<Template>)xs.Deserialize(stream);
                }
            }

            return Enumerable.Empty<Template>();
        }

        public static void Save(IEnumerable<Template> templates)
        {
            XmlSerializer xs = new XmlSerializer(typeof(List<Template>));

            using (Stream stream = new FileStream(ConfigPath, FileMode.Create))
            {
                xs.Serialize(stream, templates.ToList());
            }
        }
    }
}
