using Nancy.Hosting.Self;
using System;
using Nancy;
using Nusharp.Core;
using Nancy.Responses;

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

			using (var host = new NancyHost(new Uri(Config.Uri), new NusharpBootstrapper(), hostConfigs))
			{
				host.Start();
				Console.ReadLine();
			}
		}
	}
}
