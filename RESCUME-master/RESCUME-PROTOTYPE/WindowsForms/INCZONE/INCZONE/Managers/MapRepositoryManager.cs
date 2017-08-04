using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using INCZONE.Common;
using log4net;

namespace INCZONE.Managers
{
    public class MapRepositoryManager
    {
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);

        internal static XmlDocument GetXmlDocument(string xml)
        {
            XmlDocument MapSetXml = new XmlDocument();

            try
            {    
                MapSetXml.LoadXml(xml);
            }
            catch (XmlException e)
            {
                log.Error("XmlException ", e);
                throw e;
            }
            catch (Exception e)
            {
                log.Error("Exception ", e);
                throw e;
            } 

            return MapSetXml;
        }

        internal static ArrayOfmapSet MapSetDigester(XmlDocument MapSetXmlDocument)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ArrayOfmapSet));
            XmlReader xmlReader = new XmlNodeReader(MapSetXmlDocument);
            ArrayOfmapSet MapSetsList = null;

            try
            {
                if (serializer.CanDeserialize(xmlReader))
                {
                    MapSetsList = (ArrayOfmapSet)serializer.Deserialize(xmlReader);
                }
            }
            catch (InvalidOperationException e)
            {
                log.Error("InvalidOperationException ", e);
                throw e;
            }
            catch (Exception e)
            {
                log.Error("Exception ", e);
                throw e;
            } 

            return MapSetsList;
        }

        internal static MapNode MapNodeDigester(XmlDocument MapNodeXmlDocument)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MapNode));
            XmlReader xmlReader = new XmlNodeReader(MapNodeXmlDocument);
            MapNode mapNode = null;

            try
            {
                if (serializer.CanDeserialize(xmlReader))
                {
                    mapNode = (MapNode)serializer.Deserialize(xmlReader);
                }
            }
            catch (InvalidOperationException e)
            {
                log.Error("InvalidOperationException ", e);
                throw e;
            }
            catch (Exception e)
            {
                log.Error("Exception ", e);
                throw e;
            }

            return mapNode;
        }
    }
}
