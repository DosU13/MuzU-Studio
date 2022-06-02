using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MuzU.data
{
    public abstract class XmlBase
    {
        protected XElement ThisElement = new XElement("empty_xmlbase");

        internal XmlBase() { }
        internal XmlBase(XElement xElement) { LoadFromXElement(xElement); }

        internal virtual XElement ToXElement() { return ThisElement; }
        internal virtual void LoadFromXElement(XElement xElement) { }
    }
}
