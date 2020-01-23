using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;
using Serilog.Core;

namespace TheP0ngServer
{
    public class UdpListener
    {
      
        UdpClient receivingUdpClient;
        private int Port;
        private LoggerService Logger;
        private Socket udpClientSocket;

        public void StartUdpListening(int port, string ipWebAPI)
        {
            LoggerService logger = new LoggerService();
            Port = port;
            Logger = logger;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
            receivingUdpClient = new UdpClient(endPoint);

            udpClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint webApIpEndPoint = new IPEndPoint(IPAddress.Parse(ipWebAPI), port);
            udpClientSocket.Bind(webApIpEndPoint);
            logger.LogInformation("Started UDP listening");

            while (true)
            {
                try
                {
                    byte[] received = new byte[1];
                    received = receivingUdpClient.Receive(ref endPoint);
                    Logger.LogInformation($"Received UDP packet: {received[0]}");
                    //Add collecting the data here and sending larger packets;
                    udpClientSocket.Send(received);
                }
                catch (Exception e)
                {
                    logger.LogError($"UDP connection error: {e}");
                }
            }
            
        }
    }
}
