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
        public string Lyrics { get; set; } = null;

        internal override XElement ToXElement()
        {
            ThisElement = new XElement(nameof(TimingSequence), 
                            new XElement(nameof(UniqueName), UniqueName),
                            TimingTemplate.ToXElement(),
                            XmlConverter.ListToElement(TimingItems));
            if (Lyrics != null) ThisElement.Add(new XElement(nameof(Lyrics), Lyrics));
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element(nameof(TimingSequence));
            UniqueName = ThisElement.Element(nameof(UniqueName)).Value;
            TimingTemplate = new TimingTemplate(ThisElement);
            TimingItems = XmlConverter.ElementToList<TimingItem>(ThisElement);
            Lyrics = ThisElement.Element(nameof(Lyrics))?.Value;
            base.LoadFromXElement(ThisElement);
        }
    }
}
