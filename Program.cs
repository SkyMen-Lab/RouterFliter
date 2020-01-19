using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
namespace TheP0ngServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");

            ParseXML config = new ParseXML("whitelist.xml");

            int port = config.Port;
            string ip = config.ip;

            UdpListener udpListener = new UdpListener();
            TCPListener tcpListener = new TCPListener();

            //Starts TCP on a new thread and leaves udp on the main thread and tcp branches have new threads for handling new clients;
            try
            {
                udpListener.StartUdpListening(port, ip);
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception);
            }
            try
            {
                Thread tcpThread = new Thread(() => tcpListener.StartTcpServer(port, ip));
                tcpThread.Start();
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
       
    }
}
