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
            Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LoggerService logger = new LoggerService();
            //Socket for sending to web api
            try
            {
                IPEndPoint tcpEndPoint = new IPEndPoint(IPAddress.Any, port);
                IPEndPoint webEndPoint = new IPEndPoint(IPAddress.Parse(WebAPIip), port);
                senderSocket.Bind(webEndPoint);
                tcpListener.Bind(tcpEndPoint);
            }
            catch(Exception e)
            {
                logger.LogError($"Connection failed to bind sockets, or invalid Endpoints: {e}");
            }
            logger.LogInformation("Started TCP listening");
            try
            {
               
                while (true)
                {
                    //Handle each client on a new thread;
                    tcpListener.Listen(100);
                    Socket clientSocket = tcpListener.Accept();
                    Thread clientThread = new Thread(() => ClientConnection(clientSocket, senderSocket, SchoolCode, logger));
                    clientThread.Start();
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Threads failed or can't accept client: {e}");
            }
        }
        public void ClientConnection(Socket clientSocket, Socket senderSocket, string SchoolCode, LoggerService logger)
        {
            byte[] formattedData = new byte[64];
            byte[] handShakeByte = new byte[3];
            //Receive ReceivedGameCode;
            byte[] receivedBytes = new byte[64];
            clientSocket.Receive(receivedBytes);
            
            //Format ReceivedGameCode + log ReceivedGameCode;
            string receivedData = Encoding.ASCII.GetString(receivedBytes);
            JsonConfigs Receivedconfigs = JsonConvert.DeserializeObject<JsonConfigs>(receivedData);

            if (Receivedconfigs.SchoolCode == SchoolCode)
            {
                formattedData = FormatJSON(Receivedconfigs.SchoolCode, Receivedconfigs.GameCode, logger);
                Console.WriteLine(receivedData);
                logger.LogInformation($"Received Data: {receivedData}");
                //Send to WEBAPI and wait for response as a client;
                senderSocket.Send(formattedData);
                senderSocket.Receive(handShakeByte);
            }
            else
            {
                string error = "404";
                handShakeByte = Encoding.ASCII.GetBytes(error);
            }
            //Send response to client;
            clientSocket.Send(handShakeByte);
        }

        public byte[] FormatJSON(string ReceivedGameCode, string ReceivedSchoolCode, LoggerService logger)
        {
            byte[] jsonBytes = new byte[64];
            var ConfigstoSend = new JsonConfigs()
            {
                GameCode = ReceivedGameCode,
                SchoolCode = ReceivedSchoolCode
            };
            var JsonString = JsonConvert.SerializeObject(ConfigstoSend);
            logger.LogInformation($"Formatted Json: {JsonString}");
            return Encoding.ASCII.GetBytes(JsonString);
        }
    }
}
