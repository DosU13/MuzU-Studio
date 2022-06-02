using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MuzU.data
{
    public class MuzUData: XmlBase
    {
        public MuzUData() { }
        public MuzUData(XElement xElement) : base(xElement) { }

        public string Name { get; set; } = "NoName";
        public string MusicDescription { get; set; }
        public string MusicPath { get; set; }
        public long MusicAllign_μs { get; set; } = 0;
        public long? MicrosecondsPerQuarterNote { get; set; } = null;
        public string TimeSignature { get; set; } = null;
        public List<TimingSequence> TimingSequences { get; set; } = new List<TimingSequence>();

        internal override XElement ToXElement()
        {
            ThisElement = new XElement("MuzU",
                            new XElement(nameof(Name), Name),
                            new XElement(nameof(MusicDescription), MusicDescription),
                            new XElement(nameof(MusicPath), MusicPath),
                            new XElement(nameof(MusicAllign_μs), MusicAllign_μs),
                            new XElement(nameof(MicrosecondsPerQuarterNote), MicrosecondsPerQuarterNote),
                            new XElement(nameof(TimeSignature), TimeSignature),
                            XmlConverter.ListToElement(TimingSequences));
            return ThisElement;
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement;
            Name = ThisElement.Element(nameof(Name)).Value;
            MusicDescription = ThisElement.Element(nameof(MusicDescription))?.Value;
            MusicPath = ThisElement.Element(nameof(MusicPath))?.Value;
            MusicAllign_μs = long.Parse(ThisElement.Element(nameof(MusicAllign_μs))?.Value??"0");
            if (!long.TryParse(ThisElement.Element(nameof(MicrosecondsPerQuarterNote))?.Value, out long _mpq))
                MicrosecondsPerQuarterNote = null;
            else MicrosecondsPerQuarterNote = _mpq;
            TimeSignature = ThisElement.Element(nameof(TimeSignature))?.Value;
            TimingSequences = XmlConverter.ElementToList<TimingSequence>(ThisElement);
            base.LoadFromXElement(ThisElement);
        }
    }
}
