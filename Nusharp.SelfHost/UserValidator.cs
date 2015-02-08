using Nancy.Authentication.Basic;
using Nancy.Security;
using Nusharp.Core;

namespace Nusharp.SelfHost
{
	public class UserValidator : IUserValidator
	{
		public IUserIdentity Validate(string username, string password)
		{
			if (username == Config.Username && password == Config.Password)
				return new NusharpUserIdentity{ UserName = Config.Username};
			return null;
		}
	}
}
