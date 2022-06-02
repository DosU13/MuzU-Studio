//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;

//namespace MuzU.data
//{
//    internal class XmlTimingsConverter
//    {
//        public static XElement TimingsToElement(List<TimingItem> list, TimingTemplate template)
//        {
//            XElement element = new XElement("list", new XAttribute("count", list.Count));
//            for (int i = 0; i < list.Count; i++)
//            {
//                XElement timing = list[i].ToXElement();
//                timing.Add(ObjectsToElement(list[i].Values, template));
//                XElement xElement = new XElement("item-" + i, timing);
//                element.Add(xElement);
//            }
//            return element;
//        }

//        private static object ObjectsToElement(List<double> values, TimingTemplate template)
//        {
//            XElement element = new XElement("list", new XAttribute("count", values.Count));
//            for (int i = 0; i < values.Count; i++)
//            {
//                XElement xElement = new XElement("item-" + i, values[i]);
//                element.Add(xElement);
//            }
//            return element;
//        }

//        public static List<TimingItem> ElementToTimings(XElement xElement, TimingTemplate template)
//        {
//            XElement element = xElement.Element("list");
//            int count = int.Parse(element.Attribute("count").Value);
//            List<TimingItem> list = new List<TimingItem>(count);
//            for (int i = 0; i < count; i++)
//            {
//                XElement item = element.Element("item-" + i);
//                TimingItem timing = new TimingItem(item);
//                timing.Values = ElementToObjects(item.Element("TimingItem"), template);
//                list.Insert(i, timing); // Use insert
//            }
//            return list;
//        }

//        private static List<object> ElementToObjects(XElement xElement, TimingTemplate template)
//        {
//            XElement element = xElement.Element("list");
//            int count = int.Parse(element.Attribute("count").Value);
//            List<object> list = new List<object>(count);
//            for (int i = 0; i < count; i++)
//            {
//                string item = element.Element("item-" + i).Value;
//                ValueType type = template.Properties[i].Type;
//                if (type == ValueType.Integer) list.Insert(i, long.Parse(item));
//                else if(type == ValueType.Decimal) list.Insert(i, double.Parse(item));
//                else throw new NotImplementedException();
//            }
//            return list;
//        }
//    }
//}
