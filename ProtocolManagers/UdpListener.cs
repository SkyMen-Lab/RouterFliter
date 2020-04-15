using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TheP0ngServer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.CompilerServices;
using System.Timers;
using RouterFilter.Models;
using Serilog.Core;
using Serilog.Data;

namespace TheP0ngServer.ProtocolManagers
{
    public class UdpListener
    {
        private static LoggerService logger;
        private static int _udpPort;
        private static Timer _restartTimer;
        private static Timer _countingTimer;

        private static string _apiDomain;
        private static int _gameServicePort;
        private static int _countedClicks;

        public static async void StartUdpListening(int port, string APIdomain, int GameServicePort)
        {
            logger = new LoggerService();
            _udpPort = port;
            _apiDomain = APIdomain;
            _gameServicePort = GameServicePort;
            _countedClicks = 0;

            UdpClient listener = new UdpClient(_udpPort);
            IPEndPoint groupEndPoint = new IPEndPoint(IPAddress.Any, _udpPort);
            logger.LogInformation($"Started UDP Listening on {_udpPort}");
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(_apiDomain, GameServicePort);
                StreamWriter stream = new StreamWriter(client.GetStream());
                logger.LogInformation("Connected TCP with webAPI");
                await SendPacket(new Packet(Meta.Connect, "router"), stream);
                StartTimer();
                while (true)
                {
                    byte[] bytes = listener.Receive(ref groupEndPoint);
                    logger.LogInformation($"Received {bytes[0]}");
                    int movement = Convert.ToInt32(bytes[0]);
                    var finalMsg = movement + " " + Configs.SchoolCode;
                    //SendMessageToWebAPI(finalMsg, "127.0.0.1", GameServicePort);
                    await SendPacket(new Packet(Meta.Message, finalMsg), stream);
                    _countedClicks++;
                }
            }
            catch(Exception e)
            {
                logger.LogError($"Exception: {e}");
            }
            finally
            {
                listener.Close();
                Restart();
            }
        }

        public static async Task SendPacket(Packet packet, StreamWriter stream) 
        {
            await stream.WriteAsync(packet.ToJson());
            await stream.FlushAsync();
        }
        private static void StartTimer()
        {
            _countingTimer = new Timer(5000);
            _countingTimer.Elapsed += OnCountedEvent;
            _countingTimer.AutoReset = true;
            _countingTimer.Enabled = true;
        }
        
        private static void Restart()
        {
            _countingTimer.Stop();
            _restartTimer = new Timer();
            logger.LogInformation("Restarting UDP server in 5 seconds");
            _restartTimer.Interval = 5000;
            _restartTimer.Elapsed += OnTimedEvent;
            _restartTimer.AutoReset = false;
            _restartTimer.Enabled = true;
        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            StartUdpListening(_udpPort, _apiDomain, _gameServicePort);
        }
        private static void OnCountedEvent(Object source, ElapsedEventArgs e)
        {
            logger.LogInformation($"Received {_countedClicks} in the last 5 seconds");
            _countedClicks = 0;
        }
    }
}
