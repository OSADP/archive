using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace INCZONE.Common
{

    [Serializable()]
    [System.Xml.Serialization.XmlRoot("ArrayOfmapSet", Namespace = "http://schemas.datacontract.org/2004/07/MapEdit.Data.Models")]
    public class ArrayOfmapSet
    {
        [System.Xml.Serialization.XmlElementAttribute("mapSet")]
        public List<INCZONE.Common.MapSet> MapSetList { get; set; }
    }
}
