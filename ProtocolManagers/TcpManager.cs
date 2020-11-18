using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Serilog;
using System.Threading.Tasks;
using RouterFilter.Models;

namespace RouterFilter.ProtocolManagers
{
    public class TcpManager
    {
        static HttpClient _client;
        private static string _schoolCode;
        private static string _apiDomain;

        private static int _tcpPort;
        private static int _port;

        private static System.Timers.Timer _timer;


        public static void StartTcpServer(int port, string APIDomain, string SchoolCode)
        {
            //Port +1 to leave this port open for UDP listening;
            _port = port;
            _tcpPort = port + 1;
            _schoolCode = SchoolCode;
            _apiDomain = APIDomain;

            try{
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                _client = new HttpClient(clientHandler);
                _client.BaseAddress = new Uri($"http://{_apiDomain}/game/");
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch(Exception e){
                Log.Error($"Unable to connect as a client to webAPI {e}");
                Restart();
            }
            TcpListener listener = new TcpListener(IPAddress.Any, _tcpPort);
            TcpClient client;
            try
            {
                listener.Start(1000);
                Log.Information($"Started TCP Listening on port {_tcpPort}");
                while (true)
                {
                    client = listener.AcceptTcpClient();
                    Task.Run(() => HandleClient(client));
                }
            }
            catch (SocketException e)
            {
                Log.Error($"Socket Exception: {e}");
            }
            catch (ArgumentNullException e)
            {
                Log.Error($"Null Exception: {e}");
            }
            catch (Exception e)
            {
                Log.Error($"Exception: {e}");
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
            Log.Information("Restarting TCP server in 5 seconds");
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
            byte[] receivedBytes = new byte[64];
            stream.Read(receivedBytes);
            JsonConfigs user = new JsonConfigs();
            try
            {
                 user = JsonSerializer.Deserialize<JsonConfigs>(Encoding.ASCII.GetString(receivedBytes));  
            }
            catch(Exception e)
            {
                Log.Error($"Failed to convert user data into Json: {e}");
            }
            if (user.TeamCode == _schoolCode && user.IsJoining)
            {
                Log.Information($"Attempting to register client: {user.TeamCode} {user.GameCode}");
               
                StringContent httpContent = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
                
                int response = await RegisterPlayer(httpContent);
                
                stream.Write(Encoding.ASCII.GetBytes(response.ToString()));

                if(response == 200)
                    Log.Information("New client registered");
                else if (response == 1)
                    Log.Error("Http Request failed");
                else
                    Log.Error($"Client failed to register. Error Code: {response}");
            }
            else if(user.TeamCode == _schoolCode && !user.IsJoining)
            {
                Log.Information($"Client attempting to leave the game: {user.TeamCode} {user.GameCode}");

                StringContent httpContent = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

                int response = await UserLeaving(httpContent);
                stream.Write(Encoding.ASCII.GetBytes(response.ToString()));

                switch (response)
                {
                    case 200:
                        Log.Information("User has left");
                        break;
                    case 1:
                        Log.Error("Http Request failed");
                        break;
                    default:
                        Log.Error($"Client failed to leave. Error Code: {response}");
                        break;
                }
            }
            else
            {
                int response = 100;
                stream.Write(Encoding.ASCII.GetBytes(response.ToString()));
                Log.Information($"Client has invalid SchoolCode: {user.TeamCode}");
            }
            stream.Close();
        }
        private static async Task<int> UserLeaving(StringContent user)
        {
            try
            {
                HttpResponseMessage response = await _client.PostAsync($"http://{_apiDomain}/v1a/user_left", user);
                Log.Information($"WebAPI response: {response.StatusCode}");
                return (int)response.StatusCode;
            }
            catch
            {
                Log.Error($"Fail to register user {user}");
                return 1;
            }

        }
        private static async Task<int> RegisterPlayer(StringContent user)
        {
            try
            {
                HttpResponseMessage response = await _client.PostAsync($"http://{_apiDomain}/v1a/user_joined", user);
                Log.Information($"WebAPI response: {response.StatusCode}");
                return (int) response.StatusCode;
            }
            catch
            {
                Log.Error($"Fail to register user {user}");
                return 1;
            }
            
        }
    }
}
