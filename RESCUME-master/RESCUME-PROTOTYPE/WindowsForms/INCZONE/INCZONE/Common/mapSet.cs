using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Common
{
    [Serializable()]
    public class MapSet
    {

        public MapSet()
        {
        }

        public MapSet(Guid Id, string Name, string Description)
        {
            this.Id = Id;
            this.name = Name;
            this.description = Description;
        }

        [System.Xml.Serialization.XmlElement("Id")]
        public Guid Id { get; set; }
        [System.Xml.Serialization.XmlElement("name")]
        public string name { get; set; }
        [System.Xml.Serialization.XmlElement("description")]
        public string description { get; set; }
    }
}
