using System.Linq;
using System.Xml.Linq;

namespace gomoru.su.LightController;

internal sealed class LangVersionModifier : AssetPostprocessor
{
    public static string OnGeneratedCSProject(string path, string content)
    {
        if (!path.Contains("gomoru.su.light-controller", StringComparison.OrdinalIgnoreCase))
            return content;

        var xml = XDocument.Parse(content);
        var langVersion = xml.Root.Elements("PropertyGroup")
            .SelectMany(x => x.Elements("LangVersion"))
            .FirstOrDefault();
        if (langVersion is not null)
        {
            langVersion.Value = "10";
        }
        else
        {
            xml.Root.Element("PropertyGroup").Add(new XElement("LangVersion", "10"));
        }

        return xml.ToString();
    }
}
