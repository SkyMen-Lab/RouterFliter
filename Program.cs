using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Serilog;
using TheP0ngServer.Models;
using TheP0ngServer.ProtocolManagers;

namespace TheP0ngServer
{
	public class Program
	{
		static void Main(string[] args)
		{
			
			int port = Configs.Port;
			int gameServicePort = Configs.GameServicePort;
			string schoolCode = Configs.SchoolCode;
			string apiDomain = Configs.GameServiceIP;
			

			LoggerService logger = new LoggerService(); 
			logger.LogInformation($"Started Server on Port: {port}, Connecting to: {apiDomain}, where SchoolCode: {schoolCode}");


			//Starts TCP on a new thread and a new thread for UDP and tcp branches have new threads for handling new clients;
			try
			{
				Thread udpThread = new Thread(() => UdpListener.StartUdpListening(port, apiDomain, gameServicePort));
				udpThread.Start();
			}
			catch (Exception exception)
			{
				logger.LogError($"Error with udpThread: {exception}");
			}
			try
			{
				Thread tcpThread = new Thread(() => TcpManager.StartTcpServer(port, apiDomain, schoolCode));
				tcpThread.Start();
			}
			catch (Exception exception)
			{
				logger.LogError($"Error with tcpThread: {exception}");
			}

			finally
			{
				logger.CloseLogger();
			}
		}

	}
}
