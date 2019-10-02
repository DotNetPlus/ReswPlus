using System.Collections.Generic;
using System.Xml.Linq;
using Windows.UI.Text;
using Windows.UI.Xaml.Documents;

namespace ReswPlusLib.Html
{
    public static class HTMLParser
    {
        private static IEnumerable<Inline> Parse(IEnumerable<XNode> nodes, FontWeight fontWeight, FontStyle fontStyle, TextDecorations textDecoration)
        {
            foreach (var current in nodes)
            {
                switch (current)
                {
                    case XText xText:
                        yield return new Run() { Text = xText.Value, FontWeight = fontWeight, FontStyle = fontStyle, TextDecorations = textDecoration };
                        break;
                    case XElement xElement:
                        {
                            switch (xElement.Name.LocalName.ToLower())
                            {
                                case "b":
                                    foreach (var item in Parse(xElement.Nodes(), FontWeights.Bold, fontStyle, textDecoration))
                                    {
                                        yield return item;
                                    }
                                    break;
                                case "em":
                                    foreach (var item in Parse(xElement.Nodes(), FontWeights.SemiBold, fontStyle, textDecoration))
                                    {
                                        yield return item;
                                    }
                                    break;
                                case "i":
                                case "cite":
                                case "dfn":
                                    foreach (var item in Parse(xElement.Nodes(), fontWeight, FontStyle.Italic, textDecoration))
                                    {
                                        yield return item;

                                    }
                                    break;
                                case "u":
                                    foreach (var item in Parse(xElement.Nodes(), fontWeight, fontStyle, TextDecorations.Underline))
                                    {
                                        yield return item;
                                    }
                                    break;
                                case "s":
                                case "strike":
                                case "del":
                                    foreach (var item in Parse(xElement.Nodes(), fontWeight, fontStyle, TextDecorations.Strikethrough))
                                    {
                                        yield return item;
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        public static IEnumerable<Inline> Parse(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                XDocument xDocument = null;
                try
                {
                    xDocument = XDocument.Parse($"<str>{source}</str>", LoadOptions.PreserveWhitespace);
                }
                catch
                {
                }
                if (xDocument == null)
                {
                    yield return new Run() { Text = source };
                }
                else
                {
                    foreach (var item in Parse(((XElement)xDocument.FirstNode).Nodes(), FontWeights.Normal, FontStyle.Normal, TextDecorations.None))
                    {
                        yield return item;
                    }
                }
            }
            else
            {
            }
        }
    }
}
