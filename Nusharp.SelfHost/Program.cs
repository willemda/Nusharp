using Nancy.Hosting.Self;
using System;
using System.Linq;
using Nancy;
using Nusharp.Core;
using Nancy.Responses;
using System.Threading;

namespace Nusharp.SelfHost
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Config.LoadFromAppSettings();
			GenericFileResponse.SafePaths.Add(Config.PackageRepositoryPath);

			HostConfiguration hostConfigs = new HostConfiguration()
			{
				UrlReservations = new UrlReservations() { CreateAutomatically = true }
			};

			using (var host = new NancyHost(new Uri(string.Format("http://localhost:{0}", Config.Port)), new NusharpBootstrapper(), hostConfigs))
			{
				host.Start();

				//Under mono if you daemonize a process a Console.ReadLine will cause an EOF 
				//so we need to block another way
				if (args.Any(s => s.Equals("-d", StringComparison.CurrentCultureIgnoreCase)))
				{
					Thread.Sleep(Timeout.Infinite);
				}
				else
				{
					Console.ReadKey();
				}

				host.Stop();
			}
		}
	}
}
