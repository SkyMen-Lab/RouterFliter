using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TheP0ngServer
{
    public class TCPListener
    {
        public void StartTcpServer(int port, string ipWebAPI)
        {
            Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint tcpEndPoint = new IPEndPoint(IPAddress.Any, port);

            //Socket for sending to web api
            Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipWebAPI), port);
            senderSocket.Bind(endPoint);

            Console.WriteLine("TCP listening");

            try
            {
                tcpListener.Bind(tcpEndPoint);
                while (true)
                {
                    //Handle each client on a new thread;
                    tcpListener.Listen(100);
                    Socket clientSocket = tcpListener.Accept();
                    Thread clientThread = new Thread(() => ClientConnection(clientSocket, senderSocket));
                    clientThread.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public void ClientConnection(Socket clientSocket, Socket senderSocket)
        {
            //Receive data;
            byte[] receivedBytes = new byte[5];
            clientSocket.Receive(receivedBytes);

            //Format data + log data;
            string receivedData = Encoding.ASCII.GetString(receivedBytes);
            byte[] formattedData = FormatJSON(receivedData);
            Console.WriteLine(receivedData);

            //Send to WEBAPI and wait for response as a client;
            senderSocket.Send(formattedData);
            byte[] handShakeByte = new byte[1];
            senderSocket.Receive(handShakeByte);

            //Send response to client;
            clientSocket.Send(handShakeByte);
        }

        public byte[] FormatJSON(string data)
        {
            //Add JSON formating here but is any needed if we are just sending a school code?
            return Encoding.ASCII.GetBytes(data);
        }
    }
}
