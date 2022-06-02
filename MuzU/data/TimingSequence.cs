using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MuzU.data
{
    public class TimingSequence : XmlBase
    {
        public TimingSequence(){}
        public TimingSequence(XElement xElement) : base(xElement) { }

        public string UniqueName { get; set; } = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        public TimingTemplate TimingTemplate { get; set; } = new TimingTemplate();
        public List<TimingItem> TimingItems { get; set; } = new List<TimingItem>();

        internal override XElement ToXElement()
        {
            ThisElement = new XElement("TimingSequence", 
                            new XElement("UniqueName", UniqueName),
                            TimingTemplate.ToXElement(),
                            XmlConverter.ListToElement(TimingItems));
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element("TimingSequence");
            UniqueName = ThisElement.Element("UniqueName").Value;
            TimingTemplate = new TimingTemplate(ThisElement);
            TimingItems = XmlConverter.ElementToList<TimingItem>(ThisElement);
            base.LoadFromXElement(ThisElement);
        }
    }
}
