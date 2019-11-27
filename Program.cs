using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.IO;

namespace TheP0ngServer
{
    class Program
    {
        public const int PORT = 4455;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            int port = ParsePort("whitelist.xml");
            StartServer(port);
        }

        static void StartServer(int port)
        {
            UdpClient udp = new UdpClient(port);
            IPEndPoint groupIP = new IPEndPoint(IPAddress.Any, port);
            Console.WriteLine("Waiting for broadcast on port " + port.ToString());

            try
            {
                while (true)
                {
                    byte[] bytes = udp.Receive(ref groupIP);
                    Console.WriteLine($"Received broadcast from {groupIP} :");
                    Console.WriteLine($" {Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");
                }

            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                udp.Close();
            }
        }


        static int ParsePort(string path) {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("whitelist.xml");
            return Int32.Parse(xmlDocument.DocumentElement.ChildNodes[0].ChildNodes[0].ChildNodes[0].InnerText);
        }
    }
}
