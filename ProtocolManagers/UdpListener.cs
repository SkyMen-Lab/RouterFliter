using System;
using System.Collections.Generic;
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
        private NetworkStream ns;

        public void StartUdpListening(int port, string APIdomain, int GameServicePort)
        {

            logger = new LoggerService();
            _udpPort = port;

            UdpClient Listener = new UdpClient(_udpPort);
            IPEndPoint groupEndPoint = new IPEndPoint(IPAddress.Any, _udpPort);

            try
            {
                TcpClient client = new TcpClient();
                client.Connect(APIdomain, GameServicePort);
                ns = client.GetStream();
                logger.LogInformation("Started UDP Listening");
                logger.LogInformation("Connected TCP with webAPI");
            }
            catch(Exception e)
            {
                logger.LogError($"Exception: {e}");
            }
            try
            {
                while (true)
                {
                    byte[] bytes = Listener.Receive(ref groupEndPoint);
                    logger.LogInformation($"Received {bytes[0]}");
                    int movement = Convert.ToInt32(bytes[0]);
                    var finalMsg = movement + " " + Configs.SchoolCode;
                    bytes = Encoding.ASCII.GetBytes(finalMsg);
                    ns.Write(bytes);
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
                ns.Close();
            }
        }
    }
}
