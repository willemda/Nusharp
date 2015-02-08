using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace Nusharp.SelfHost
{
	public class NusharpBootstrapper : DefaultNancyBootstrapper
	{
		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			base.ApplicationStartup(container, pipelines);

			pipelines.EnableBasicAuthentication(new BasicAuthenticationConfiguration(new UserValidator(), "Nusharp", UserPromptBehaviour.Always));
		}
	}
}
