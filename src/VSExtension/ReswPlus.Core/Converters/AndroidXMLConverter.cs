// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlus.Core.ResourceParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace ReswPlus.Core.Converters
{
    public class AndroidXMLConverter
    {
        private static Regex _regexFixQuotes = new Regex("(?<!\\\\)\"", RegexOptions.Compiled);
        const string _reswTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<root>\r\n  <!-- \r\n    Microsoft ResX Schema \r\n    \r\n    Version 2.0\r\n    \r\n    The primary goals of this format is to allow a simple XML format \r\n    that is mostly human readable. The generation and parsing of the \r\n    various data types are done through the TypeConverter classes \r\n    associated with the data types.\r\n    \r\n    Example:\r\n    \r\n    ... ado.net/XML headers & schema ...\r\n    <resheader name=\"resmimetype\">text/microsoft-resx</resheader>\r\n    <resheader name=\"version\">2.0</resheader>\r\n    <resheader name=\"reader\">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>\r\n    <resheader name=\"writer\">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>\r\n    <data name=\"Name1\"><value>this is my long string</value><comment>this is a comment</comment></data>\r\n    <data name=\"Color1\" type=\"System.Drawing.Color, System.Drawing\">Blue</data>\r\n    <data name=\"Bitmap1\" mimetype=\"application/x-microsoft.net.object.binary.base64\">\r\n        <value>[base64 mime encoded serialized .NET Framework object]</value>\r\n    </data>\r\n    <data name=\"Icon1\" type=\"System.Drawing.Icon, System.Drawing\" mimetype=\"application/x-microsoft.net.object.bytearray.base64\">\r\n        <value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>\r\n        <comment>This is a comment</comment>\r\n    </data>\r\n                \r\n    There are any number of \"resheader\" rows that contain simple \r\n    name/value pairs.\r\n    \r\n    Each data row contains a name, and value. The row also contains a \r\n    type or mimetype. Type corresponds to a .NET class that support \r\n    text/value conversion through the TypeConverter architecture. \r\n    Classes that don't support this are serialized and stored with the \r\n    mimetype set.\r\n    \r\n    The mimetype is used for serialized objects, and tells the \r\n    ResXResourceReader how to depersist the object. This is currently not \r\n    extensible. For a given mimetype the value must be set accordingly:\r\n    \r\n    Note - application/x-microsoft.net.object.binary.base64 is the format \r\n    that the ResXResourceWriter will generate, however the reader can \r\n    read any of the formats listed below.\r\n    \r\n    mimetype: application/x-microsoft.net.object.binary.base64\r\n    value   : The object must be serialized with \r\n            : System.Runtime.Serialization.Formatters.Binary.BinaryFormatter\r\n            : and then encoded with base64 encoding.\r\n    \r\n    mimetype: application/x-microsoft.net.object.soap.base64\r\n    value   : The object must be serialized with \r\n            : System.Runtime.Serialization.Formatters.Soap.SoapFormatter\r\n            : and then encoded with base64 encoding.\r\n\r\n    mimetype: application/x-microsoft.net.object.bytearray.base64\r\n    value   : The object must be serialized into a byte array \r\n            : using a System.ComponentModel.TypeConverter\r\n            : and then encoded with base64 encoding.\r\n    -->\r\n  <xsd:schema id=\"root\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n    <xsd:import namespace=\"http://www.w3.org/XML/1998/namespace\" />\r\n    <xsd:element name=\"root\" msdata:IsDataSet=\"true\">\r\n      <xsd:complexType>\r\n        <xsd:choice maxOccurs=\"unbounded\">\r\n          <xsd:element name=\"metadata\">\r\n            <xsd:complexType>\r\n              <xsd:sequence>\r\n                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" />\r\n              </xsd:sequence>\r\n              <xsd:attribute name=\"name\" use=\"required\" type=\"xsd:string\" />\r\n              <xsd:attribute name=\"type\" type=\"xsd:string\" />\r\n              <xsd:attribute name=\"mimetype\" type=\"xsd:string\" />\r\n              <xsd:attribute ref=\"xml:space\" />\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n          <xsd:element name=\"assembly\">\r\n            <xsd:complexType>\r\n              <xsd:attribute name=\"alias\" type=\"xsd:string\" />\r\n              <xsd:attribute name=\"name\" type=\"xsd:string\" />\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n          <xsd:element name=\"data\">\r\n            <xsd:complexType>\r\n              <xsd:sequence>\r\n                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"1\" />\r\n                <xsd:element name=\"comment\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"2\" />\r\n              </xsd:sequence>\r\n              <xsd:attribute name=\"name\" type=\"xsd:string\" use=\"required\" msdata:Ordinal=\"1\" />\r\n              <xsd:attribute name=\"type\" type=\"xsd:string\" msdata:Ordinal=\"3\" />\r\n              <xsd:attribute name=\"mimetype\" type=\"xsd:string\" msdata:Ordinal=\"4\" />\r\n              <xsd:attribute ref=\"xml:space\" />\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n          <xsd:element name=\"resheader\">\r\n            <xsd:complexType>\r\n              <xsd:sequence>\r\n                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"1\" />\r\n              </xsd:sequence>\r\n              <xsd:attribute name=\"name\" type=\"xsd:string\" use=\"required\" />\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n        </xsd:choice>\r\n      </xsd:complexType>\r\n    </xsd:element>\r\n  </xsd:schema>\r\n  <resheader name=\"resmimetype\">\r\n    <value>text/microsoft-resx</value>\r\n  </resheader>\r\n  <resheader name=\"version\">\r\n    <value>2.0</value>\r\n  </resheader>\r\n  <resheader name=\"reader\">\r\n    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>\r\n  </resheader>\r\n  <resheader name=\"writer\">\r\n    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>\r\n  </resheader></root>";

        public static void ParseAndroidFile(string sourcePath, string destinationPath)
        {
            var keys = ExtractAndroidItems(sourcePath);
            SaveReswFile(destinationPath, keys);
        }

        private static IEnumerable<ReswItem> ExtractAndroidItems(string sourcePath)
        {
            var sourceAndroidXML = XDocument.Load(sourcePath);
            var xmlRoot = sourceAndroidXML.Root;
            string currentComment = null;
            foreach (var xNode in xmlRoot.Nodes())
            {
                switch (xNode)
                {
                    case XComment commentNode:
                        {
                            currentComment = RemoveAntislash(commentNode.Value.Trim());
                        }
                        break;
                    case XElement elementNode:
                        {
                            switch (elementNode.Name.LocalName.ToLower())
                            {
                                case "string":
                                    {
                                        var name = elementNode.Attribute("name")?.Value;
                                        if (name != null)
                                        {
                                            var value = elementNode.Value;
                                            yield return new ReswItem(name, RemoveAntislash(value), currentComment);
                                        }
                                        break;
                                    }
                                case "plurals":
                                    {
                                        var name = elementNode.Attribute("name").Value;
                                        foreach (var subItem in elementNode.Elements("item"))
                                        {
                                            var quantity = subItem.Attribute("quantity")?.Value;
                                            if (quantity != null)
                                            {
                                                yield return new ReswItem($"{name}_{UpperCaseFirstLetter(quantity)}", RemoveAntislash(subItem.Value), currentComment);
                                            }
                                        }
                                        break;
                                    }
                            }
                            currentComment = null;
                        }
                        break;
                }
            }
        }

        private static string UpperCaseFirstLetter(string text)
        {
            return text.ToUpper()[0] + new string(text.ToLower().Skip(1).ToArray());
        }

        private static void SaveReswFile(string destinationPath, IEnumerable<ReswItem> keys)
        {
            var reswFileXml = XDocument.Parse(_reswTemplate);
            var templateRoot = reswFileXml.Root;
            foreach (var item in keys)
            {
                var dataNode = new XElement("data");
                dataNode.SetAttributeValue("name", item.Key);
                dataNode.Add(new XElement("value") { Value = item.Value });

                var comment = item.Comment;
                if (comment != null)
                {
                    dataNode.Add(new XElement("comment") { Value = comment });
                }
                templateRoot.Add(dataNode);
            }

            reswFileXml.Save(destinationPath);
        }

        public static XmlDocument ReswToAndroidXML(
          ReswInfo reswInfo,
          bool supportPluralization = false)
        {
            Dictionary<string, List<ReswItem>> groupedItems;
            if (supportPluralization)
            {
                groupedItems = (from item in reswInfo.Items let simplifiedKey = ExtractReswKey(item.Key) group item by simplifiedKey into groupItem select groupItem).ToDictionary(k => k.Key, k => k.Select(j => j).ToList());
            }
            else
            {
                groupedItems = reswInfo.Items.ToDictionary(p => p.Key, p => new List<ReswItem>() { p });
            }

            var androidXML = new XmlDocument();
            var resourcesNode = androidXML.CreateElement("resources");
            androidXML.AppendChild(androidXML.CreateComment("smartling.instruction_comments_enabled = on"));
            androidXML.AppendChild(androidXML.CreateComment(@"smartling.placeholder_format_custom = \{\d\}"));
            androidXML.AppendChild(resourcesNode);

            foreach (var resourceItem in groupedItems.OrderBy(k => k.Key))
            {
                switch (resourceItem.Value.Count)
                {
                    case 0:
                        break;
                    case 1:
                        {
                            var valueNode = resourceItem.Value.First();
                            if (valueNode.Comment != null)
                            {
                                resourcesNode.AppendChild(androidXML.CreateComment(valueNode.Comment));
                            }

                            var stringNode = androidXML.CreateElement("string");
                            stringNode.SetAttribute("name", resourceItem.Key);
                            stringNode.AppendChild(androidXML.CreateTextNode(valueNode.Value));

                            resourcesNode.AppendChild(stringNode);
                        }
                        break;
                    default:
                        {
                            var pluralsNode = androidXML.CreateElement("plurals");
                            pluralsNode.SetAttribute("name", resourceItem.Key);
                            foreach (var reswItem in resourceItem.Value)
                            {
                                if (reswItem.Comment != null)
                                {
                                    pluralsNode.AppendChild(androidXML.CreateComment(reswItem.Comment));
                                }

                                var itemNode = androidXML.CreateElement("item");
                                var quantity = reswItem.Key.Split('_').Last().ToLower();
                                itemNode.SetAttribute("quantity", quantity);
                                itemNode.AppendChild(androidXML.CreateTextNode(reswItem.Value));
                                pluralsNode.AppendChild(itemNode);
                            }
                            resourcesNode.AppendChild(pluralsNode);
                        }
                        break;
                }
            }

            return androidXML;
        }

        private static string FixQuotes(XElement dataNode)
        {
            return _regexFixQuotes.Replace(dataNode.Value, m => "\\\"");
        }

        private static string RemoveAntislash(string text)
        {
            return text.Replace("\\\"", "\"").Replace("\\'", "'").Replace("\\n", "\n");
        }

        private static string ExtractReswKey(string key)
        {
            var pos = key.LastIndexOf('_');
            return pos < 0 ? key : key.Substring(0, pos);
        }

        public static bool AndroidXMLDirectoryToResw(string sourcePath, string destinationPath)
        {
            foreach (var directory in Directory.GetDirectories(sourcePath))
            {
                var directoryName = Path.GetFileName(directory);
                if (!directoryName.StartsWith("values-"))
                {
                    continue;
                }

                var xmlFilePath = Path.Combine(directory, "strings.xml");
                if (!File.Exists(xmlFilePath))
                {
                    continue;
                }

                var indexDash = directoryName.IndexOf('-');
                var languageId = directoryName.Substring(indexDash + 1);
                var generatedFilePath = $"{destinationPath}\\{AndroidToReswLanguageId(languageId)}\\Resources.resw";
                Directory.CreateDirectory(Path.GetDirectoryName(generatedFilePath));
                if (!AndroidXMLFileToResw(xmlFilePath, generatedFilePath))
                {
                    return false;
                }

            }
            return true;
        }

        private static string AndroidToReswLanguageId(string androidLanguageId)
        {
            if (androidLanguageId.StartsWith("b+"))
            {
                //BCP 47 tags
                var splitted = androidLanguageId.Split('+');
                var reswLanguageId = splitted[1];
                if (splitted.Length > 2)
                {
                }
                return reswLanguageId;
            }
            else
            {
                // The language is defined by a two-letter ISO 639-1 language code,
                // optionally followed by a two letter ISO 3166-1-alpha-2 region code (preceded by lowercase r).
                var splitted = androidLanguageId.Split('-');
                var reswLanguageId = splitted[0];
                if (splitted.Length > 1 && splitted[1].StartsWith("r"))
                {
                    reswLanguageId += "-" + splitted[1].Substring(1).ToUpper();
                }
                return reswLanguageId;
            }
        }

        public static bool AndroidXMLFileToResw(string sourcePath, string destinationPath)
        {
            try
            {
                ParseAndroidFile(sourcePath, destinationPath);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
