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
    public class Program
    {
        static void Main(string[] args)
        {
            Configs config = new Configs();

            config.ParseXML("whitelist.xml");
            int port = config.Port;
            string SchoolCode = config.SchoolCode;
            string APIDomain = ("https://my-json-server.typicode.com/nnugget/TravelRecord/db");

            LoggerService _logger = new LoggerService(); 
            _logger.LogInformation($"Started Server on Port: {port}, Connecting to: {APIDomain}, where SchoolCode: {SchoolCode}");

            UdpListener udpListener = new UdpListener();
            tcpListen tcpListener = new tcpListen();


            //Starts TCP on a new thread and a new thread for UDP and tcp branches have new threads for handling new clients;
            try
            {
                Thread udpThread = new Thread(() => udpListener.StartUdpListening(port, APIDomain));
                udpThread.Start();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error with udpThread: {exception}");
            }
            try
            {
                Thread tcpThread = new Thread(() => tcpListener.StartTcpServer(port, APIDomain, SchoolCode));
                tcpThread.Start();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error with tcpThread: {exception}");
            }

            finally
            {
                _logger.CloseLogger();
            }
        }

    }
}
