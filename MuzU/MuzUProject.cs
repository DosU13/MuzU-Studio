using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MuzU.data;
using MuzU.util;
using Windows.Storage;

namespace MuzU
{
    public class MuzUProject
    {
        public MuzUData data;

        public MuzUProject() => data = new MuzUData();

        public MuzUProject(Stream stream) => data = LoadFromStream(stream);

        private MuzUData LoadFromStream(Stream stream) 
        {
            return new MuzUData(XDocument.Load(stream).Root);
        }

        public void Save(Stream stream)
        { 
            stream.SetLength(0);
            ToXDocument().Save(stream);
        }

        private XDocument ToXDocument()
        {
            XDocument doc = new XDocument(data.ToXElement());
            doc.Declaration = new XDeclaration("1.0", "utf-8", "true");
            return doc;
        }

        public double BPM => MuzUConverter.GetBPM(data.MicrosecondsPerQuarterNote.Value, data.TimeSignature);
        public int BeatsPerBar => MuzUConverter.GetTimeSignatureNumerator(data.TimeSignature);
    }
}
