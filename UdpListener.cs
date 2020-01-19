using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TheP0ngServer
{
    public class UdpListener
    {
        public void StartUdpListening(int port, string ipWebAPI)
        {
            UdpClient listenerClient = new UdpClient(port);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

            Console.WriteLine("UDP listening");

            Socket udpClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint webApIpEndPoint = new IPEndPoint(IPAddress.Parse(ipWebAPI), port);

            udpClientSocket.Bind(webApIpEndPoint);
            try
            {
                while (true)
                {
                    byte[] bytes = listenerClient.Receive(ref endPoint);
                    udpClientSocket.Send(bytes);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
