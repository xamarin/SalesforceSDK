using Xamarin.Auth;

namespace Salesforce
{
	public interface IAuthenticatedRequest : IRestRequest
	{
		OAuth2Request ToOAuth2Request (ISalesforceUser user);
	}
}

