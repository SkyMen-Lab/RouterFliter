using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace TheP0ngServer
{
    public static class Configs
    {
        public static int Port { get; private set; }
        public static string SchoolCode { get; private set; }

        public static int GameServicePort { get; private set; }
        public static string GameServiceIP { get; private set; }


        static Configs() {
            ParseXML("config.xml");
        }

        private static void ParseXML(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            Port = int.Parse(doc.DocumentElement.ChildNodes[0].ChildNodes[0].ChildNodes[0].InnerText);
            SchoolCode = doc.DocumentElement.ChildNodes[0].ChildNodes[1].ChildNodes[0].InnerText;
            GameServicePort = int.Parse(doc.DocumentElement.ChildNodes[0].ChildNodes[2].ChildNodes[0].InnerText);
            GameServiceIP = doc.DocumentElement.ChildNodes[0].ChildNodes[3].ChildNodes[0].InnerText;
        }

    }
}
