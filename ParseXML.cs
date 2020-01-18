using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TheP0ngServer
{
    public class ParseXML
    {
        public static int Port;
        public static string IP;

        public ParseXML(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            Port = int.Parse(doc.DocumentElement.ChildNodes[0].ChildNodes[0].ChildNodes[0].InnerText);
            Console.WriteLine(Port.ToString());
            IP = doc.DocumentElement.ChildNodes[0].ChildNodes[1].ChildNodes[0].InnerText;
            Console.WriteLine(IP);
        }
    }
}
