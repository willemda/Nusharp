using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Nancy;
using Nancy.Responses;
using Nusharp.Core.Utils;
using Nancy.Security;

namespace Nusharp.Core
{
	public class NusharpModule : NancyModule
	{ 
		public NusharpModule()
		{
			if(!string.IsNullOrWhiteSpace(Config.Username))
				this.RequiresAuthentication();

			Get["/api/v2"] = Root;
			Get["/api/v2/$metadata"] = Metadata;

			Get["/api/v2/Packages{.*}"] = SearchPackages;
			Get["/api/v2/Search{.*}"] = SearchPackages;
			Get["/api/v2/FindPackagesById{.*}"] = SearchPackages;
			Get["/api/v2/Search()/$count{.*}"] = CountPackages;

			Get["/api/v2/package/{Id}/{Version}"] = DownloadPackage;

			Put["/api/v2"] = PushPackage;
		}

		internal dynamic Root(dynamic request)
		{
			var sw = new StringWriter();
			BuildRoot().Save(sw);
			var response = (Response)sw.GetStringBuilder().ToString();
			response.ContentType = "text/xml";
			return response;
		}

		internal dynamic Metadata(dynamic request)
		{
			var response = (Response)File.ReadAllText("$metadata.xml");
			response.ContentType = "text/xml";
			return response;
		}

		internal dynamic PushPackage(dynamic request)
		{
			var tempFileName = Path.GetTempFileName();
			using(var packageStream = new FileStream(tempFileName, FileMode.Create)){
				Request.Files.First(x => x.Name == "package").Value.CopyTo(packageStream);
			}
			Package package = Package.FromNupkg(tempFileName);
			File.Copy(tempFileName, Path.Combine(Config.PackageRepositoryPath, string.Format("{0}.{1}.nupkg", package.Id, package.Version)));
			File.Delete(tempFileName);
			return new Response();
		}

		internal dynamic DownloadPackage(dynamic request)
		{
			var package = Packages().First(x => x.Id == request.Id && x.Version == new Version(request.Version));
			return new GenericFileResponse(package.Path);
		}

		internal dynamic CountPackages(dynamic request)
		{
			var response = (Response)PackagesFromRequest().Count().ToString();
			response.ContentType = "text/plain";
			return response;
		}	

		internal dynamic SearchPackages(dynamic request)
		{
			var feed = BuildFeed();
			foreach(var package in PackagesFromRequest()){
				feed.Element(Constants.Xmlns + "feed").Add(package.ToXml());
			}

			var sw = new StringWriter();
			feed.Save(sw);

			var response = (Response) sw.GetStringBuilder().ToString();
			response.ContentType = "application/atom+xml;type=feed;charset=utf-8";
			return response;
		}

		private IEnumerable<Package> Packages()
		{
			IList<Package> packages = new List<Package>();
			foreach (var packagePath in Directory.GetFiles(Config.PackageRepositoryPath, "*.nupkg").OrderByDescending(x => x))
			{
				packages.Add(Package.FromNupkg(packagePath));
			}

			foreach (var package in packages)
			{
				package.IsLatestVersion = package.IsAbsoluteLatestVersion = !packages.Any(x => x.Version > package.Version && x.Id == package.Id);
			}

			return packages;
		}

		private IEnumerable<Package> PackagesFromRequest()
		{
			var packageRequest = PackageRequest.FromRequest(Request);

			foreach (var package in Packages().Where(x =>
			{
				if (!string.IsNullOrWhiteSpace(packageRequest.SearchTerm)
					&& !(x.Title.ToLower().Contains(packageRequest.SearchTerm.ToLower()) || x.Description.ToLower().Contains(packageRequest.SearchTerm.ToLower())))
					return false;
				if (packageRequest.Filter == "IsAbsoluteLatestVersion" && !x.IsAbsoluteLatestVersion)
					return false;
				if (packageRequest.Filter == "IsLatestVersion" && !x.IsLatestVersion)
					return false;
				if (!string.IsNullOrWhiteSpace(packageRequest.Id) && x.Id != packageRequest.Id)
					return false;
				if (packageRequest.Version != null && x.Version != packageRequest.Version)
					return false;
				return true;
			}).OrderBy(x =>
			{
				switch (packageRequest.OrderBy)
				{
					case "Version":
						return x.Version.ToString();
					default:
						return x.Id;
				}
			}).Skip(packageRequest.Skip ?? 0).Take(packageRequest.Top ?? int.MaxValue))
			{
				yield return package;
			}
		}

		private XDocument BuildFeed()
		{
			return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement(Constants.Xmlns + "feed")
				.Attr(XNamespace.Xmlns + "d", Constants.D)
				.Attr(XNamespace.Xmlns + "m", Constants.M)
				.Attr(XNamespace.Xml + "base", string.Format("{0}/api/v2/", Config.Uri))
				.Start(Constants.Xmlns + "id").Val(string.Format("{0}/api/v2/Packages", Config.Uri)).End()
				.Start(Constants.Xmlns + "title").Attr("type", "text").Val("Packages").End()
				.Start(Constants.Xmlns + "updated").Val(DateTime.Now).End()
				.Start(Constants.Xmlns + "link").Attr("rel", "self").Attr("title", "Packages").Attr("href", "Packages").End());
		}

		private XDocument BuildRoot()
		{
			XNamespace xmlns = "http://www.w3.org/2007/app";
			XNamespace atom = "http://www.w3.org/2005/Atom";
			return new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
				new XElement(xmlns + "service")
					.Attr(XNamespace.Xml + "base", string.Format("{0}/api/v2", Config.Uri))
					.Attr(XNamespace.Xmlns + "atom", atom)
					.Start(xmlns + "workspace")
						.Start(atom + "title")
							.Val("Default")
						.End()
						.Start(xmlns + "collection")
							.Attr("href", "Packages")
							.Start(atom + "title")
								.Val("Packages")
							.End()
						.End()
					.End());
		}
	}
}
