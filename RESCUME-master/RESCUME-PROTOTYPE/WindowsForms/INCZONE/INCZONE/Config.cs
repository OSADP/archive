using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace INCZONE
{
    public class Config
    {
        [XmlElement]
        public string HostIP { get; set; }
        [XmlElement]
        public int Port { get; set; }
        [XmlElement]
        public string Username { get; set; }
        [XmlElement]
        public string Password { get; set; }
    }
}
