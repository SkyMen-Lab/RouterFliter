using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TheP0ngServer.Models;

namespace TheP0ngServer.ProtocolManagers
{
    public class TcpManager
    {
        static HttpClient _client;
        private static string _schoolCode;
        private static string _apiDomain;

        private static LoggerService _logger;
        private static int _tcpPort;
        private static int _port;

        private static System.Timers.Timer _timer;


        public static void StartTcpServer(int port, string APIDomain, string SchoolCode)
        {
            _logger = new LoggerService();
            //Port +1 to leave this port open for UDP listening;
            _port = port;
            _tcpPort = port + 1;
            _schoolCode = SchoolCode;
            _apiDomain = APIDomain;

            try{
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                _client = new HttpClient(clientHandler);
                _client.BaseAddress = new Uri("https://127.0.0.1:5001");
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch(Exception e){
                _logger.LogError($"Unable to connect as a client to webAPI {e}");
                Restart();
            }
            TcpListener listener = new TcpListener(IPAddress.Any, _tcpPort);
            TcpClient client;
            try
            {
                listener.Start(1000);
                _logger.LogInformation($"Started TCP Listening on port {_tcpPort}");
                while (true)
                {
                    client = listener.AcceptTcpClient();
                    Task.Run(() => HandleClient(client));
                }
            }
            catch (SocketException e)
            {
                _logger.LogError($"Socket Exception: {e}");
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError($"Null Exception: {e}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception: {e}");
            }
            finally
            {
                listener.Stop();
                Restart();
            }
        }
        private static void Restart()
        {
            _timer = new System.Timers.Timer();
            _logger.LogInformation("Restarting TCP server in 5 seconds");
            _timer.Interval = 5000;
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = false;
            _timer.Enabled = true;
        }
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            StartTcpServer(_port, _apiDomain, _schoolCode);
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
                _logger.LogError($"Failed to convert user data into Json: {e}");
            }
            if (User.TeamCode == _schoolCode && User.IsJoining)
            {
                _logger.LogInformation($"Attempting to register client: {User.TeamCode} {User.GameCode}");
               
                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(User), Encoding.UTF8, "application/json");
                
                int response = await RegisterPlayer(httpContent);
                
                stream.Write(Encoding.ASCII.GetBytes(response.ToString()));

                if(response == 200)
                    _logger.LogInformation("New client registered");
                else if (response == 1)
                    _logger.LogError("Http Request failed");
                else
                    _logger.LogError($"Client failed to register. Error Code: {response}");
            }
            else if(User.TeamCode == _schoolCode && !User.IsJoining)
            {
                _logger.LogInformation($"Client attempting to leave the game: {User.TeamCode} {User.GameCode}");

                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(User), Encoding.UTF8, "application/json");

                int response = await UserLeaving(httpContent);
                stream.Write(Encoding.ASCII.GetBytes(response.ToString()));

                if (response == 200)
                    _logger.LogInformation("User has left");
                else if (response == 1)
                    _logger.LogError("Http Request failed");
                else
                    _logger.LogError($"Client failed to leave. Error Code: {response}");
            }
            else
            {
                int response = 100;
                stream.Write(Encoding.ASCII.GetBytes(response.ToString()));
                _logger.LogInformation($"Client has invalid SchoolCode: {User.TeamCode}");
            }
            stream.Close();
        }
        private static async Task<int> UserLeaving(StringContent user)
        {
            try
            {
                HttpResponseMessage response = await _client.PostAsync("http://127.0.0.1:5000/v1a/user_left ", user);
                _logger.LogInformation($"WebAPI response: {response.StatusCode}");
                return (int)response.StatusCode;
            }
            catch
            {
                _logger.LogError($"Fail to register user {user}");
                return 1;
            }

        }
        private static async Task<int> RegisterPlayer(StringContent user)
        {
            try
            {
                HttpResponseMessage response = await _client.PostAsync("http://127.0.0.1:5000/v1a/user_joined", user);
                _logger.LogInformation($"WebAPI response: {response.StatusCode}");
                return (int) response.StatusCode;
            }
            catch
            {
                _logger.LogError($"Fail to register user {user}");
                return 1;
            }
            
        }
    }
}
