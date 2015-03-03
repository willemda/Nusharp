using System;
using System.Text.RegularExpressions;
using Nancy;
using System.Collections.Generic;

namespace Nusharp.Core
{
	public class PackageRequest
	{
		public string OrderBy { get; set; }
		public int? Skip { get; set; }
		public int? Top { get; set; }
		public string SearchTerm { get; set; }
		public string TargetFramework { get; set; }
		public bool IncludePreRelease { get; set; }
		public string Filter { get; set; }
		public string Id { get; set; }
		public Version Version { get; set; }

		public IDictionary<string, Version> UpdatePackages { get; set; }

		public static PackageRequest FromRequest(Request request)
		{
			var packageRequest = new PackageRequest();

			packageRequest.OrderBy = request.Query["$orderby"] != null ? request.Query["$orderby"] : null;
			packageRequest.Skip = request.Query["$skip"] != null ? request.Query["$skip"] : null;
			packageRequest.Top = request.Query["$top"] != null ? request.Query["$top"] : null;
			packageRequest.SearchTerm = request.Query["searchTerm"];
			if (!string.IsNullOrWhiteSpace(packageRequest.SearchTerm))
				packageRequest.SearchTerm = packageRequest.SearchTerm.Remove(packageRequest.SearchTerm.Length - 1, 1).Remove(0, 1).ToLower();
			packageRequest.TargetFramework = request.Query["targetFramework"];
			packageRequest.IncludePreRelease = request.Query["includePrerelease"] != null ? request.Query["includePrerelease"] : false;
			packageRequest.Filter = request.Query["$filter"];
			packageRequest.Id = request.Query["id"];
			if (!string.IsNullOrWhiteSpace(packageRequest.Id))
				packageRequest.Id = packageRequest.Id.Remove(packageRequest.Id.Length - 1, 1).Remove(0, 1);

			string query = request.Url;
			Regex regex = new Regex(@"\(Id='(?<id>.*)',Version='(?<version>.*)'\)");
			if (regex.IsMatch(query))
			{
				packageRequest.Id = regex.Match(query).Groups[1].Value;
				packageRequest.Version = new Version(regex.Match(query).Groups[2].Value);
			}

			if (!string.IsNullOrWhiteSpace(request.Query["packageIds"]) && !string.IsNullOrWhiteSpace(request.Query["versions"]))
			{
				var packageIdString = (string)request.Query["packageIds"];
				var versionString = (string)request.Query["versions"];
				string[] packageIds = packageIdString.Remove(packageIdString.Length - 1, 1).Remove(0, 1).Split(new []{"|"}, StringSplitOptions.RemoveEmptyEntries);
				string[] versions = versionString.Remove(versionString.Length - 1, 1).Remove(0, 1).Split(new []{"|"}, StringSplitOptions.RemoveEmptyEntries);
				packageRequest.UpdatePackages = new Dictionary<string, Version>();

				for(int num = 0 ; num < packageIds.Length; num ++){
					packageRequest.UpdatePackages.Add(packageIds[num], new Version(versions[num]));
				}
			}

			return packageRequest;
		}
	}


}
