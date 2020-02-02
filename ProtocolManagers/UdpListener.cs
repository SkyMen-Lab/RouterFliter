using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.CompilerServices;
using Serilog.Core;
using Serilog.Data;

namespace TheP0ngServer
{
    public class UdpListener
    {
        private static LoggerService logger;
        private int _udpPort;


        public void StartUdpListening(int port, string APIdomain, int GameServicePort)
        {

            logger = new LoggerService();
            _udpPort = port;

            UdpClient Listener = new UdpClient(_udpPort);
            IPEndPoint groupEndPoint = new IPEndPoint(IPAddress.Any, _udpPort);

            try
            {
                logger.LogInformation("Started UDP Listening");
            }
            catch(Exception e)
            {
                logger.LogError($"Exception: {e}");
            }
            try
            {
                TcpClient client = new TcpClient();
                client.Connect("127.0.0.1", GameServicePort);
                StreamWriter stream = new StreamWriter(client.GetStream());
                stream.AutoFlush = true;
                logger.LogInformation("Connected TCP with webAPI");
                while (true)
                {
                    byte[] bytes = Listener.Receive(ref groupEndPoint);
                    logger.LogInformation($"Received {bytes[0]}");
                    int movement = Convert.ToInt32(bytes[0]);
                    var finalMsg = movement + " " + Configs.SchoolCode;
                    if (!string.IsNullOrEmpty(finalMsg)) {
                        //SendMessageToWebAPI(finalMsg, "127.0.0.1", GameServicePort);
                        stream.Write(finalMsg);
                    }
                }
            }
            catch (SocketException e)
            {
                logger.LogError($"Socket Exception: {e}");
            }
            catch (ArgumentNullException e)
            {
                logger.LogError($"Null Exception: {e}");
            }
            catch(Exception e)
            {
                logger.LogError($"Exception: {e}");
            }
            finally
            {
                Listener.Close();
            }
        }

        private void SendMessageToWebAPI(string message, string ip, int port) {
            //MAYBE used in future
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(ip, port);
                logger.LogInformation("Connected TCP with webAPI");
                var stream = client.GetStream();
                byte[] bytes = Encoding.ASCII.GetBytes(message);
                stream.Write(bytes, 0, bytes.Length);

                stream.Close();
                client.Close();
            }
            catch (SocketException e)
            {
                logger.LogError($"SocketException: {e}"); 
            }

        }
    }
}
