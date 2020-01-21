using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;

namespace TheP0ngServer
{
    public class TCPListener
    {
        public void StartTcpServer(int port, string WebAPIip, string SchoolCode)
        {
            Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint tcpEndPoint = new IPEndPoint(IPAddress.Any, port);

            //Socket for sending to web api
            Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(WebAPIip), port);
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
                  
                    Thread clientThread = new Thread(() => ClientConnection(clientSocket, senderSocket, SchoolCode));
                    clientThread.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public bool CheckNetworkMask(string ConfigIP)
        {
            return true;
        }
        public void ClientConnection(Socket clientSocket, Socket senderSocket, string SchoolCode)
        {
            byte[] formattedData = new byte[64];
            byte[] handShakeByte = new byte[1];
            //Receive ReceivedGameCode;
            byte[] receivedBytes = new byte[64];
            clientSocket.Receive(receivedBytes);
            
            //Format ReceivedGameCode + log ReceivedGameCode;
            string receivedData = Encoding.ASCII.GetString(receivedBytes);
            JsonConfigs Receivedconfigs = JsonConvert.DeserializeObject<JsonConfigs>(receivedData);
            if (Receivedconfigs.SchoolCode == SchoolCode)
            {
                if (CheckNetworkMask(Receivedconfigs.ConfigIp))
                {
                    formattedData = FormatJSON(Receivedconfigs.SchoolCode, SchoolCode);
                    Console.WriteLine(receivedData);
                    //Send to WEBAPI and wait for response as a client;
                    senderSocket.Send(formattedData);
                    senderSocket.Receive(handShakeByte);
                }
                else
                {
                    handShakeByte[0] = 0;
                }
            }
            else
            {
                handShakeByte[0] = 0;
            }
            //Send response to client;
            clientSocket.Send(handShakeByte);
        }

        public byte[] FormatJSON(string ReceivedGameCode, string SchoolCode)
        {
            byte[] jsonBytes = new byte[64];
            string JsonFormat = "{ \"SchoolCode\": \"" + ReceivedGameCode + "\",\"GameSessionCode\": \"" + SchoolCode + "\"}";
            var JsonString = JsonConvert.SerializeObject(JsonFormat);
            Console.WriteLine(JsonString);
            return Encoding.ASCII.GetBytes(JsonString);
        }
    }
}
