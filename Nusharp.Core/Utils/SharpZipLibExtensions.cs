using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;

namespace Nusharp.Core.Utils
{
	public static class SharpZipLibExtensions
	{
		public static IEnumerable<ZipEntry> Entries(this ZipFile zipFile)
		{
			foreach (ZipEntry zipEntry in zipFile)
			{
				yield return zipEntry;
			}
		}
	}
}
