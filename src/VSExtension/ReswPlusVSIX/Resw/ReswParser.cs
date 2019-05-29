using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ReswPlus.Resw
{
    class ReswParser
    {
        public static ReswInfo Parse(string content)
        {
            var res = new ReswInfo
            {
                Items = new List<ReswItem>()
            };

            var xml = new XmlDocument();
            xml.LoadXml(content);

            var nodes = xml.DocumentElement?.SelectNodes("//data");
            if (nodes == null)
            {
                return res;
            }

            foreach (XmlElement element in nodes)
            {
                string key = null, value = null, comment = null;
                var elementKey = element.Attributes.GetNamedItem("name");
                if (elementKey != null)
                {
                    key = elementKey.Value ?? string.Empty;
                }
                else
                {
                    continue;
                }
                var elementValue = element.SelectSingleNode("value");
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
}
