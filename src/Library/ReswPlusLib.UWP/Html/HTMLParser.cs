// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace ReswPlusLib.Html
{
    public static class HTMLParser
    {
        private static IEnumerable<Inline> Parse(IEnumerable<XNode> nodes, FontFamily fontFamily, Brush fontColor, FontWeight fontWeight, FontStyle fontStyle, TextDecorations textDecoration, FontVariants fontVariants)
        {
            foreach (var current in nodes)
            {
                switch (current)
                {
                    case XText xText:
                        var run = new Run() { Text = xText.Value, FontWeight = fontWeight, FontStyle = fontStyle, TextDecorations = textDecoration };
                        if (fontFamily != null)
                        {
                            run.FontFamily = fontFamily;
                        }
                        if (fontColor != null)
                        {
                            run.Foreground = fontColor;
                        }
                        Typography.SetVariants(run, fontVariants);
                        yield return run;
                        break;
                    case XElement xElement:
                        {
                            switch (xElement.Name.LocalName.ToLower())
                            {
                                case "b":
                                case "strong":
                                    {
                                        FontWeight newFontWeight = FontWeights.Bold;
                                        var weight = xElement.Attribute("weight")?.Value;
                                        if (!string.IsNullOrEmpty(weight))
                                        {
                                            switch (weight.ToLower())
                                            {
                                                case "950":
                                                case "extrablack":
                                                    newFontWeight = FontWeights.ExtraBlack;
                                                    break;
                                                case "900":
                                                case "black":
                                                    newFontWeight = FontWeights.Black;
                                                    break;
                                                case "800":
                                                case "extrabold":
                                                    newFontWeight = FontWeights.ExtraBold;
                                                    break;
                                                case "700":
                                                case "bold":
                                                    newFontWeight = FontWeights.Bold;
                                                    break;
                                                case "600":
                                                case "semibold":
                                                    newFontWeight = FontWeights.SemiBold;
                                                    break;
                                                case "500":
                                                case "medium":
                                                    newFontWeight = FontWeights.Medium;
                                                    break;
                                                case "400":
                                                case "normal":
                                                    newFontWeight = FontWeights.Normal;
                                                    break;
                                                case "350":
                                                case "semilight":
                                                    newFontWeight = FontWeights.SemiLight;
                                                    break;
                                                case "300":
                                                case "light":
                                                    newFontWeight = FontWeights.Light;
                                                    break;
                                                case "200":
                                                case "extralight":
                                                    newFontWeight = FontWeights.ExtraLight;
                                                    break;
                                                case "100":
                                                case "thin":
                                                    newFontWeight = FontWeights.Thin;
                                                    break;

                                            }
                                        }
                                        foreach (var item in Parse(xElement.Nodes(), fontFamily, fontColor, newFontWeight, fontStyle, textDecoration, fontVariants))
                                        {
                                            yield return item;
                                        }
                                        break;
                                    }
                                case "em":
                                case "i":
                                case "cite":
                                case "dfn":
                                    foreach (var item in Parse(xElement.Nodes(), fontFamily, fontColor, fontWeight, FontStyle.Italic, textDecoration, fontVariants))
                                    {
                                        yield return item;

                                    }
                                    break;
                                case "u":
                                    foreach (var item in Parse(xElement.Nodes(), fontFamily, fontColor, fontWeight, fontStyle, TextDecorations.Underline, fontVariants))
                                    {
                                        yield return item;
                                    }
                                    break;
                                case "s":
                                case "strike":
                                case "del":
                                    foreach (var item in Parse(xElement.Nodes(), fontFamily, fontColor, fontWeight, fontStyle, TextDecorations.Strikethrough, fontVariants))
                                    {
                                        yield return item;
                                    }
                                    break;
                                case "font":
                                    {
                                        Brush newFontColor = null;
                                        FontFamily newFontFamily = null;

                                        var colorStr = xElement.Attribute("color")?.Value;
                                        if (!string.IsNullOrEmpty(colorStr))
                                        {
                                            try
                                            {
                                                var color = XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), colorStr) as Color?;
                                                if (color.HasValue)
                                                {
                                                    newFontColor = new SolidColorBrush() { Color = color.Value };
                                                }
                                            }
                                            catch { }
                                        }

                                        var faceStr = xElement.Attribute("face")?.Value;
                                        if (!string.IsNullOrEmpty(faceStr))
                                        {
                                            newFontFamily = new FontFamily(faceStr);
                                        }

                                        foreach (var item in Parse(xElement.Nodes(), newFontFamily ?? fontFamily, newFontColor ?? fontColor, fontWeight, fontStyle, textDecoration, fontVariants))
                                        {
                                            yield return item;
                                        }
                                    }
                                    break;
                                case "tt":
                                    {
                                        foreach (var item in Parse(xElement.Nodes(), new FontFamily("Consolas"), fontColor, fontWeight, fontStyle, textDecoration, fontVariants))
                                        {
                                            yield return item;
                                        }
                                    }
                                    break;
                                case "sup":
                                    {
                                        foreach (var item in Parse(xElement.Nodes(), fontFamily, fontColor, fontWeight, fontStyle, textDecoration, FontVariants.Superscript))
                                        {
                                            yield return item;
                                        }
                                    }
                                    break;
                                case "sub":
                                    {
                                        foreach (var item in Parse(xElement.Nodes(), fontFamily, fontColor, fontWeight, fontStyle, textDecoration, FontVariants.Subscript))
                                        {
                                            yield return item;
                                        }
                                    }
                                    break;
                                case "br":
                                    {
                                        yield return new LineBreak();
                                    }
                                    break;
                                case "a":
                                    {
                                        var href = xElement.Attribute("href")?.Value;
                                        if (!string.IsNullOrEmpty(href))
                                        {
                                            var hyperlink = new Hyperlink()
                                            {
                                                NavigateUri = new Uri(href),
                                                UnderlineStyle = UnderlineStyle.None
                                            };
                                            foreach (var item in Parse(xElement.Nodes(), fontFamily, fontColor, fontWeight, fontStyle, textDecoration, fontVariants))
                                            {
                                                hyperlink.Inlines.Add(item);
                                            }
                                            yield return hyperlink;
                                        }
                                        else
                                        {
                                            //ignore the hyperlink
                                            foreach (var item in Parse(xElement.Nodes(), fontFamily, fontColor, fontWeight, fontStyle, textDecoration, fontVariants))
                                            {
                                                yield return item;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    //ignore the element
                                    foreach (var item in Parse(xElement.Nodes(), fontFamily, fontColor, fontWeight, fontStyle, textDecoration, fontVariants))
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
                    foreach (var item in Parse(((XElement)xDocument.FirstNode).Nodes(), null, null, FontWeights.Normal, FontStyle.Normal, TextDecorations.None, FontVariants.Normal))
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
