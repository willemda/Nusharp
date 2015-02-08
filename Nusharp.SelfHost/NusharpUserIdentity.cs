using Nancy.Security;
using System.Collections.Generic;

namespace Nusharp.SelfHost
{
	public class NusharpUserIdentity : IUserIdentity
	{
		public IEnumerable<string> Claims { get; set; }

		public string UserName { get; set; }
	}
}
