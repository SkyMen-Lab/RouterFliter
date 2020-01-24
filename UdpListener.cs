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
        public void StartUdpListening(int port, string APIdomain)
        {
            logger = new LoggerService();

            UdpClient Listener = new UdpClient(port);
            IPEndPoint groupEndPoint = new IPEndPoint(IPAddress.Any, port);

            port++;

            TcpClient client = new TcpClient(APIdomain, port);
            NetworkStream ns = client.GetStream();

            logger.LogInformation("Started UDP Listening");
            logger.LogInformation("Connected TCP with webAPI");

            try
            {
                while (true)
                {
                    byte[] bytes = Listener.Receive(ref groupEndPoint);
                    logger.LogInformation($"Received {bytes[0]}");
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
