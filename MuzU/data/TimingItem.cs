using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MuzU.data
{
    public class TimingItem : XmlBase
    {
        public TimingItem() { }
        public TimingItem(XElement xElement):base(xElement) { }

        public long Time { get; set; } = 0;
        public long? Length { get; set; } = null;
        public List<double> Values { get; set; } = new List<double>();

        internal override XElement ToXElement()
        {
            ThisElement = new XElement("TimingItem",
                            new XElement("Time_μs", Time),
                            new XElement("Length_μs", Length),
                            XmlConverter.ListToElement(Values));
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element("TimingItem");
            Time = long.Parse(ThisElement.Element("Time_μs").Value);
            if (!long.TryParse(ThisElement.Element("Length_μs")?.Value, out long _length))
                Length = null;
            else Length = _length;
            Values = XmlConverter.ElementToList(ThisElement);
            base.LoadFromXElement(ThisElement);
        }
    }
}
