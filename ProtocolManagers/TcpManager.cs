using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using System.Timers;
using Serilog;
using Serilog.Core;

namespace TheP0ngServer
{
    public class TcpManager
    {
        static HttpClient _client;
        private static string _schoolCode;
        private static string _apiDomain;

        private static LoggerService logger;
        private static int _tcpPort;
        private static System.Timers.Timer _timer;


        public static void StartTcpServer(int port, string APIDomain, string SchoolCode)
        {
            logger = new LoggerService();
            //Port +1 to leave this port open for UDP listening;
            _tcpPort = port + 1;

            _schoolCode = SchoolCode;
            _apiDomain = APIDomain;

            try{
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                _client = new HttpClient(clientHandler);
                _client.BaseAddress = new Uri("https://127.0.0.1:5001");
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch(Exception e){
                logger.LogError($"Unable to connect as a client to webAPI {e}");
                Restart(port, APIDomain, SchoolCode);
            }
            TcpListener listener = new TcpListener(IPAddress.Any, _tcpPort);
            TcpClient client;
            try
            {
                listener.Start(1000);
                logger.LogInformation("Started TCP Listening");
                while (true)
                {
                    client = listener.AcceptTcpClient();
                    Task.Run(() => HandleClient(client));
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
            catch (Exception e)
            {
                logger.LogError($"Exception: {e}");
            }
            finally
            {
                listener.Stop();
                Restart(port, APIDomain, SchoolCode);
            }
        }
        private static void Restart(int port, string APIDomain, string SchoolCode){
            logger.LogInformation("TcpListener is offline");
                //Sets 10000 millisecond timer
                setTimer(10000);
                //calls the StartTcpServer again
                restartTcpListening(port, APIDomain, SchoolCode);
        }
        private static void setTimer(int time){
            logger.LogInformation($"Attempting to restart server in 10 seconds.");
            _timer = new System.Timers.Timer(time);
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }
        private static void restartTcpListening(int port, string APIDomain, string SchoolCode){
            logger.LogInformation("Restarting Listening");
            StartTcpServer(port, APIDomain, SchoolCode);
        }
        private static async void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] ReceivedBytes = new byte[64];
            stream.Read(ReceivedBytes);
            JsonConfigs User = new JsonConfigs();
            try
            {
                User = JsonConvert.DeserializeObject<JsonConfigs>(Encoding.ASCII.GetString(ReceivedBytes));
            }
            catch(Exception e)
            {
                logger.LogError($"Failed to convert user data into Json: {e}");
            }
            if (User.SchoolCode == _schoolCode)
            {
                logger.LogInformation($"Attempting to register client: {User.SchoolCode} {User.GameCode}");
               
                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(User), Encoding.UTF8, "application/json");
                
                int response = await RegisterPlayer(httpContent);
                
                stream.Write(Encoding.ASCII.GetBytes(response.ToString()));

                if(response == 200)
                    logger.LogInformation("New client registered");
                else if (response == 1)
                    logger.LogError("Http Request failed");
                else
                    logger.LogError($"Client failed to register: {response}");
            }
            else
            {
                int response = 100;
                stream.Write(Encoding.ASCII.GetBytes(response.ToString()));
                logger.LogInformation($"Client has invalid SchoolCode: {User.SchoolCode}");
            }
            stream.Close();
        }
        private static async Task<int> RegisterPlayer(StringContent user)
        {
            try
            {
                HttpResponseMessage response = await _client.PostAsync("http://127.0.0.1:5000/v1a/user_joined", user);
                logger.LogInformation($"WebAPI response: {response.StatusCode}");
                return (int) response.StatusCode;
            }
            catch
            {
                logger.LogError($"Fail to register user {user}");
                return 1;
            }
            
        }
    }
}
