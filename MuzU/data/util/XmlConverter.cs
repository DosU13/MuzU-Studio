using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MuzU.data
{
    public class XmlConverter
    {
        public static XElement ListToElement<T>(List<T> list) where T : XmlBase
        {
            XElement element = new XElement("list", new XAttribute("count", list.Count));
            for (int i = 0; i < list.Count; i++)
            {
                XElement xElement = new XElement("item-" + i, list[i]?.ToXElement());
                element.Add(xElement);
            }
            return element;
        }

        public static List<T> ElementToList<T>(XElement xElement) where T : XmlBase, new()
        {
            XElement element = xElement.Element("list");
            int count = int.Parse(element.Attribute("count").Value);
            List<T> list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                XElement item = element.Element("item-" + i);
                T t = new T();
                t.LoadFromXElement(item);
                list.Insert(i, t); // Use insert
            }
            return list;
        }

        public static XElement ListToElement(List<double> list)
        {
            XElement element = new XElement("list", new XAttribute("count", list.Count));
            for (int i = 0; i < list.Count; i++)
            {
                XElement xElement = new XElement("item-" + i, list[i]);
                element.Add(xElement);
            }
            return element;
        }

        public static List<double> ElementToList(XElement xElement)
        {
            XElement element = xElement.Element("list");
            int count = int.Parse(element.Attribute("count").Value);
            List<double> list = new List<double>(count);
            for (int i = 0; i < count; i++)
            {
                XElement item = element.Element("item-" + i);
                list.Insert(i, double.Parse(item.Value)); 
            }
            return list;
        }
    }
}
