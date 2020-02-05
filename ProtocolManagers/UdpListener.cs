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
        private static int _udpPort;
        private static System.Timers.Timer _timer;



        public static void StartUdpListening(int port, string APIdomain, int GameServicePort)
        {

            logger = new LoggerService();
            _udpPort = port;

            UdpClient Listener = new UdpClient(_udpPort);
            IPEndPoint groupEndPoint = new IPEndPoint(IPAddress.Any, _udpPort);
            logger.LogInformation("Started UDP Listening");
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
            catch(Exception e)
            {
                logger.LogError($"Exception: {e}");
            }
            finally
            {
                Listener.Close();
                Restart(port, APIdomain, GameServicePort);
            }
        }
        private static void Restart(int port, string APIdomain, int GameServicePort){
            logger.LogInformation("Restarting UdpListening and connection with GameService");
            SetTimer(10000);
            StartUdpListening(port, APIdomain, GameServicePort);
        }
        private static void SetTimer(int time){
            _timer = new System.Timers.Timer(time);
            _timer.AutoReset = true;
            _timer.Enabled = true;
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
