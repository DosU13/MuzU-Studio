using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MuzU.data
{
    public static class Extensions
    {
        public static string _ToString(this ValueType type)
        {
            if (type == ValueType.Integer) return "Integer";
            else if (type == ValueType.Decimal) return "Decimal";
            else return "NoWay this is impossible";
        }

        public static ValueType ParseToValueType(this String type)
        {
            if (type == "Integer") return ValueType.Integer;
            else return ValueType.Decimal;
        }
    }
    public enum ValueType { Integer, Decimal } //TinyText}

    public class TimingTemplate : XmlBase
    {
        public TimingTemplate() { }
        public TimingTemplate(XElement xElement) : base(xElement) { }

        public bool LengthEnabled { get; set; } = false;
        public List<TimingTemplateProperty> Properties { get; set; } = new List<TimingTemplateProperty>();

        internal override XElement ToXElement()
        {
            ThisElement = new XElement("TimingTemplate",
                            new XElement("LengthEnabled", LengthEnabled),
                            XmlConverter.ListToElement(Properties));
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element("TimingTemplate");
            LengthEnabled = bool.Parse(ThisElement.Element("LengthEnabled").Value);
            Properties = XmlConverter.ElementToList<TimingTemplateProperty>(ThisElement);
            base.LoadFromXElement(ThisElement);
        }
    }

    public class TimingTemplateProperty : XmlBase
    {

        public List<ValueType> ValueTypeOptions = new List<ValueType>(new ValueType[] { ValueType.Integer, ValueType.Decimal });

        public TimingTemplateProperty() { }
        public TimingTemplateProperty(string name, ValueType valueType) 
        {
            Name = name;
            Type = valueType;
        }
        public TimingTemplateProperty(XElement xElement) : base(xElement) { }

        public string Name { get; set; } = "NoName";
        public ValueType Type { get; set; } = ValueType.Integer;

        internal override XElement ToXElement()
        {
            ThisElement = new XElement("Property",
                            new XElement("Name", Name),
                            new XElement("Type", Type._ToString()));
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element("Property");
            Name = ThisElement.Element("Name").Value;
            Type = ThisElement.Element("Type").Value.ParseToValueType();
            base.LoadFromXElement(ThisElement);
        }
    }
}
