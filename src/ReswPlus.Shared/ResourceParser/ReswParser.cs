using System.IO;
using System.Xml;

namespace ReswPlus.Core.ResourceParser;

public sealed class ReswParser
{
    /// <summary>
    /// Reads the resources out of the content of a <c>.resw</c> file.
    /// </summary>
    /// <param name="content">The content of the file.</param>
    /// <returns>The resources it declares.</returns>
    /// <exception cref="XmlException">The content is not well formed, or declares a document type.</exception>
    /// <remarks>
    /// A resource file is written by hand and read on every keystroke, so it is read with document types
    /// refused and nothing resolved from outside it. A document type is what lets a few hundred bytes expand
    /// into gigabytes through nested entities, and an external reference is what lets a file reach for
    /// something that is not part of the project at all. Neither has any business in a resource file.
    /// </remarks>
    public static ReswInfo Parse(string content)
    {
        var res = new ReswInfo();

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };

        var xml = new XmlDocument { XmlResolver = null };

        using (var text = new StringReader(content))
        using (var reader = XmlReader.Create(text, settings))
        {
            xml.Load(reader);
        }

        var nodes = xml.DocumentElement?.SelectNodes("//data");
        if (nodes is null)
        {
            return res;
        }

        foreach (XmlElement element in nodes)
        {
            string? comment = null;
            var elementKey = element.Attributes.GetNamedItem("name");
            string key;
            if (elementKey != null)
            {
                key = elementKey.Value ?? string.Empty;
            }
            else
            {
                continue;
            }
            var elementValue = element.SelectSingleNode("value");
            string value;
            if (elementValue != null)
            {
                value = elementValue.InnerText;
            }
            else
            {
                continue;
            }

            var elementComment = element.SelectSingleNode("comment");
            if (elementComment != null)
            {
                comment = elementComment.InnerText;
            }

            res.Items.Add(new ReswItem(key, value, comment));
        }
        return res;
    }

}
