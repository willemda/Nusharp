using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Nusharp.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;

namespace Nusharp.Core
{
	public class Package
	{
		public string Id { get; set; }
		public Version Version { get; set; }
		public DateTime Updated { get; set; }
		public string Author { get; set; }

		public string Title { get; set; }
		public string Dependencies { get; set; }
		public string LicenceUrl { get; set; }
		public string Copyright { get; set; }
		public int DownloadCount { get; set; }
		public string ProjectUrl { get; set; }
		public bool RequireLicenseAcceptance { get; set; }
		public string GalleryDetailsUrl { get; set; }
		public string Description { get; set; }
		public string ReleaseNotes { get; set; }
		public string PackageHash { get; set; }
		public string PackageHashAlgorithm { get; set; }
		public string Language { get; set; }

		public long PackageSize { get; set; }
		public DateTime Published { get; set; }
		public string Tags { get; set; }
		public bool IsLatestVersion { get; set; }
		public int VersionDownloadCount { get; set; }
		public string Summary { get; set; }
		public bool IsAbsoluteLatestVersion { get; set; }
		public bool Listed { get; set; }
		public string IconUrl { get; set; }
		public string MinClientVersion { get; set; }


		public string Path { get; set; }

		public static Package FromNupkg(string filePath, IList<Package> packages)
		{
			var pkg = new Package();
			pkg.Path = filePath;

			pkg.PackageHash = SHAHelper.CalculateSHA512Hash(File.ReadAllText(filePath));
			pkg.PackageHashAlgorithm = "SHA512";

			var fInfo = new FileInfo(filePath);
			pkg.Updated = fInfo.LastWriteTimeUtc;
			pkg.Published = fInfo.LastWriteTimeUtc;
			pkg.PackageSize = fInfo.Length;

			using (var zipArchive = new ZipFile(File.OpenRead(filePath)))
			{
				var nuspecEntry = zipArchive.Entries().First(x => x.Name.Contains(".nuspec"));
				var reader = XmlReader.Create(zipArchive.GetInputStream(nuspecEntry));
				XElement element = XElement.Load(reader);
				XNamespace ns = element.GetDefaultNamespace();
				var nsm = new XmlNamespaceManager(reader.NameTable);
				nsm.AddNamespace("ns", ns.NamespaceName);

				pkg.Title = pkg.Id = element.XPathSelectElement("//ns:id", nsm).AsString();

				pkg.Version = new Version(element.XPathSelectElement("//ns:version", nsm).AsString());
				pkg.Author = element.XPathSelectElement("//ns:authors", nsm).AsString();
				pkg.LicenceUrl = element.XPathSelectElement("//ns:licenseUrl", nsm).AsString();
				pkg.ProjectUrl = element.XPathSelectElement("//ns:projectUrl", nsm).AsString();
				pkg.IconUrl = element.XPathSelectElement("//ns:iconUrl", nsm).AsString();
				pkg.RequireLicenseAcceptance = element.XPathSelectElement("//ns:requireLicenseAcceptance", nsm).As<bool>(false);
				pkg.Description = element.XPathSelectElement("//ns:description", nsm).AsString();
				pkg.Summary = element.XPathSelectElement("//ns:summary", nsm).AsString();
				pkg.Copyright = element.XPathSelectElement("//ns:copyright", nsm).AsString();
				pkg.Tags = element.XPathSelectElement("//ns:tags", nsm).AsString();
				pkg.Language = element.XPathSelectElement("//ns:language", nsm).AsString();

				var minClientVersionAttr = element.XPathSelectElement("//ns:metadata", nsm).Attribute("minClientVersion");

				pkg.MinClientVersion = minClientVersionAttr != null ? minClientVersionAttr.Value : null;

				var dependencyNodes = element.Descendants(ns + "dependency");
				if(dependencyNodes.Any()){
					pkg.Dependencies = dependencyNodes.Select(x => x.Attribute("id").Value + ":" + x.Attribute("version").Value).Aggregate((x, y) => x + "|" + y);
				}

				pkg.IsLatestVersion = pkg.IsAbsoluteLatestVersion = !packages.Where(x => x.Id == pkg.Id && x.Version > pkg.Version).Any();
			}
			return pkg;
		}

		public XElement ToXml()
		{
			return new XElement(Constants.Xmlns + "entry")
				.Start(Constants.Xmlns + "id")
					.Val(string.Format("{0}/api/v2/Packages(Id='{1}',Version='{2}')", Config.Uri, Id, Version))
				.End()
				.Start(Constants.Xmlns + "category")
					.Attr("term", "NuGetGallery.V2FeedPackage")
					.Attr("scheme", "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme")
				.End()
				.Start(Constants.Xmlns + "link")
					.Attr("rel", "edit")
					.Attr("title", "V2FeedPackage")
					.Attr("href", string.Format("Packages(Id='{0}', Version='{1}'", Id, Version))
				.End()
				.Start(Constants.Xmlns + "title")
					.Attr("type", "text")
					.Val(Id)
				.End()
				.Start(Constants.Xmlns + "summary")
					.Attr("type", "text")
					.Val(Id)
				.End()
				.Start(Constants.Xmlns + "updated")
					.Val(Updated)
				.End()
				.Start(Constants.Xmlns + "author")
					.Start(Constants.Xmlns + "name")
						.Val(Author)
					.End()
				.End()
				.Start(Constants.Xmlns + "link")
					.Attr("rel", "edit-media")
					.Attr("title", "V2FeedPackage")
					.Attr("href", string.Format("Packages(Id='{0}',Version='{1}')/$value", Id, Version))
				.End()
				.Start(Constants.Xmlns + "content")
					.Attr("type", "application/zip")
					.Attr("src", string.Format("{0}/api/v2/package/{1}/{2}", Config.Uri, Id, Version))
				.End()
				.Start(Constants.M + "properties")
					.Start(Constants.D + "Version").Val(Version).End()
					.Start(Constants.D + "Title").Val(Title).End()
					.Start(Constants.D + "Dependencies").Val(Dependencies).End()
					.Start(Constants.D + "LicenceUrl").Val(LicenceUrl).End()
					.Start(Constants.D + "Copyright").Val(Copyright).End()
					.Start(Constants.D + "DownloadCount").Val(DownloadCount).End()
					.Start(Constants.D + "ProjectUrl").Val(ProjectUrl).End()
					.Start(Constants.D + "RequireLicenseAcceptance").Val(RequireLicenseAcceptance).End()
					.Start(Constants.D + "GalleryDetailsUrl").Val(GalleryDetailsUrl).End()
					.Start(Constants.D + "Description").Val(Description).End()
					.Start(Constants.D + "ReleaseNotes").Val(ReleaseNotes).End()
					.Start(Constants.D + "PackageHash").Val(PackageHash).End()
					.Start(Constants.D + "PackageHashAlgorithm").Val(PackageHashAlgorithm).End()
					.Start(Constants.D + "PackageSize").Val(PackageSize).End()
					.Start(Constants.D + "Published").Val(Published).End()
					.Start(Constants.D + "Tags").Val(Tags).End()
					.Start(Constants.D + "IsLatestVersion").Val(IsLatestVersion).End()
					.Start(Constants.D + "VersionDownloadCount").Val(VersionDownloadCount).End()
					.Start(Constants.D + "Summary").Val(Summary).End()
					.Start(Constants.D + "IsAbsoluteLatestVersion").Val(IsAbsoluteLatestVersion).End()
					.Start(Constants.D + "Listed").Val(Listed).End()
					.Start(Constants.D + "IconUrl").Val(IconUrl).End()
					.Start(Constants.D + "Language").Val(Language).End()
					.Start(Constants.D + "MinClientVersion").Val(MinClientVersion).End()
				.End();
		}
	}
}