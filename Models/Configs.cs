using System;
using System.IO;
using System.Xml;
using Serilog;

namespace RouterFilter.Models
{
    public static class Configs
    {
        public static int Port { get; private set; }
        public static string SchoolCode { get; private set; }

        public static int GameServicePort { get; private set; }
        public static string GameServiceIP { get; private set; }

        static Configs()
        {
            ParseEnvVars();
        }
        static void ParseXML(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            Port = int.Parse(doc.DocumentElement.ChildNodes[0].ChildNodes[0].ChildNodes[0].InnerText);
            SchoolCode = doc.DocumentElement.ChildNodes[0].ChildNodes[1].ChildNodes[0].InnerText;
            GameServicePort = int.Parse(doc.DocumentElement.ChildNodes[0].ChildNodes[2].ChildNodes[0].InnerText);
            GameServiceIP = doc.DocumentElement.ChildNodes[0].ChildNodes[3].ChildNodes[0].InnerText;
        }

        static void ParseEnvVars() 
        {
            try {
                Log.Information("Parsing env.vars");
                Port = int.Parse(Environment.GetEnvironmentVariable("Port"));
                SchoolCode = Environment.GetEnvironmentVariable("SchoolCode");
                GameServicePort = int.Parse(Environment.GetEnvironmentVariable("GameServicePort"));
                GameServiceIP = Environment.GetEnvironmentVariable("GameServiceIP");
            } catch
            {
                Log.Error("Error parsing env. variables! Parsing config instead.");
                ParseXML("config.xml");
            }
        }
    }
}
