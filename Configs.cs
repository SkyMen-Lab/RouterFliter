using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace TheP0ngServer
{
    public class Configs
    {
        public int Port { get; private set; }
        public string SchoolCode { get; private set; }
        public string Apidomain { get; private set; }

        public void ParseXML(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            Port = int.Parse(doc.DocumentElement.ChildNodes[0].ChildNodes[0].ChildNodes[0].InnerText);
            SchoolCode = doc.DocumentElement.ChildNodes[0].ChildNodes[1].ChildNodes[0].InnerText;
        }

        public void GetAPIip(string HostName)
        {
            IPAddress[] APIDomaina = Dns.GetHostAddresses(HostName);
            string[] IPs = APIDomaina.Select(ip => ip.ToString()).ToArray();
            Apidomain = IPs[1];
        }
    }
}
