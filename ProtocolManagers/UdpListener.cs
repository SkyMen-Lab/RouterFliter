using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TheP0ngServer.Models;

namespace TheP0ngServer.ProtocolManagers
{
    public class UdpListener
    {
        private static LoggerService logger;
        private static int _udpPort;
        private static System.Timers.Timer _timer;

        private static string _apiDomain;
        private static int _gameServicePort;

        public static async void StartUdpListening(int port, string APIdomain, int GameServicePort)
        {


            logger = new LoggerService();
            _udpPort = port;
            _apiDomain = APIdomain;
            _gameServicePort = GameServicePort;

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
                while (true)
                {
                    byte[] bytes = listener.Receive(ref groupEndPoint);
                    logger.LogInformation($"Received {bytes[0]}");
                    int movement = Convert.ToInt32(bytes[0]);
                    var finalMsg = movement + " " + Configs.SchoolCode;
                    //SendMessageToWebAPI(finalMsg, "127.0.0.1", GameServicePort);
                    await SendPacket(new Packet(Meta.Message, finalMsg), stream);
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
        private static void Restart()
        {
            _timer = new System.Timers.Timer();
            logger.LogInformation("Restarting UDP server in 5 seconds");
            _timer.Interval = 5000;
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = false;
            _timer.Enabled = true;
        }
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            StartUdpListening(_udpPort, _apiDomain, _gameServicePort);
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
