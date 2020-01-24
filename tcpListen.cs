﻿using Newtonsoft.Json;
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
using Serilog;
using Serilog.Core;

namespace TheP0ngServer
{
    public class tcpListen
    {
        static HttpClient _client = new HttpClient();
        private static string _schoolCode;
        private static string _apiDomain;
        private static LoggerService logger;

        public void StartTcpServer(int port, string APIDomain, string SchoolCode)
        {
            logger = new LoggerService();
            //Port +1 to leave this port open for UDP listening;
            port++;
            _schoolCode = SchoolCode;
            _apiDomain = APIDomain;
            _client.BaseAddress = new Uri($"http://localhost:{port}/");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            TcpClient client;
            listener.Start(1000);
            logger.LogInformation("Started TCP Listening");
            try
            {
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
            }
           
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
                StringContent httpContent = new StringContent(User.ToString());
                int response = await RegisterPlayer(httpContent);
                stream.Write(Encoding.ASCII.GetBytes(response.ToString()));
                if(response == 200)
                    logger.LogInformation("New client registered");
                else
                    logger.LogInformation($"Client failed to register: {response}");
            }
            else
            {
                int response = 100;
                stream.Write(Encoding.ASCII.GetBytes(response.ToString()));
                logger.LogInformation($"Client has invalid SchoolCode: {User.SchoolCode}");
            }
            stream.Close();
        }
        static async Task<int> RegisterPlayer(StringContent user)
        {
            HttpResponseMessage response = await _client.PostAsync(_apiDomain, user);
            logger.LogInformation($"WebAPI response: {response.StatusCode}");
            return (int)response.StatusCode;
        }
    }
}
