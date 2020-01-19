using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TheP0ngServer
{
    public class ParseXML
    {
        public int Port { get; private set; }
        public string ip { get; private set; }

        public ParseXML(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            Port = int.Parse(doc.DocumentElement.ChildNodes[0].ChildNodes[0].ChildNodes[0].InnerText);
            ip = doc.DocumentElement.ChildNodes[0].ChildNodes[1].ChildNodes[0].InnerText;
        }
    }
}
