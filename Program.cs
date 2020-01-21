using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Serilog;

namespace TheP0ngServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");

            Configs config = new Configs();
            config.ParseXML("whitelist.xml");
            int port = config.Port;
            string SchoolCode = config.SchoolCode;
            IPAddress[] APIDomaina = Dns.GetHostAddresses("localhost");
            string[] IPs = APIDomaina.Select(ip => ip.ToString()).ToArray();
            string APIDomain = IPs[1];
            Console.WriteLine(APIDomain);

            UdpListener udpListener = new UdpListener();
            TCPListener tcpListener = new TCPListener();


            //Starts TCP on a new thread and leaves udp on the main thread and tcp branches have new threads for handling new clients;
            try
            {
                Thread udpThread = new Thread(() => udpListener.StartUdpListening(port, APIDomain));
                udpThread.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            try
            {
                Thread tcpThread = new Thread(() => tcpListener.StartTcpServer(port, APIDomain, SchoolCode));
                tcpThread.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

    }
}
