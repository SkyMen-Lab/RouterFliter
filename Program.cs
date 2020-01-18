using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TheP0ngServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
            _ = new ParseXML("whitelist.xml");
            StartServer(ParseXML.Port);
        }

        private static void StartServer(int port)
        {
            Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint tcpEndPoint = new IPEndPoint(IPAddress.Any, port);
            try
            {
                tcpListener.Bind(tcpEndPoint);
                while (true)
                {
                    //Handle each client on a new thread;
                    tcpListener.Listen(1000);
                    Socket clientSocket = tcpListener.Accept();
                    Thread clientThread;
                    clientThread = new Thread((() => ClientConnection(clientSocket, ParseXML.IP, port)));
                    clientThread.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private static void ClientConnection(Socket clientSocket, string ipWebAPI, int port)
        {
            //Receive data;
            byte[] receivedBytes = new byte[5];
            clientSocket.Receive(receivedBytes);
            //Format data + log data;
            string receivedData = Encoding.ASCII.GetString(receivedBytes);
            byte[] formattedData = formatJSON(receivedData);
            Console.WriteLine(receivedData);
            //Send to WEBAPI and wait for response as a client;
            Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipWebAPI), port);
            senderSocket.Bind(endPoint);
            senderSocket.Send(formattedData);
            byte[] handShakeByte = new byte[1];
            senderSocket.Receive(handShakeByte);
            //Send response to client;
            clientSocket.Send(handShakeByte);
        }

        private static byte[] formatJSON(string data)
        {
            //Add JSON formating here but is any needed if we are just sending a school code?
            return Encoding.ASCII.GetBytes(data);
        }
    }
}
