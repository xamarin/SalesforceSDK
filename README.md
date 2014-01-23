SalesforceSDK
=============

Build native apps around your Salesforce data.

Give your users the mobile experience they expect, and increasingly demand, from their enterprise apps. Take full advantage of the raw performance and rich functionality native to each platform. Meet compliance requirements by leveraging each platform's trusted-computing features, like OS-managed credential stores. 

Do it all using C#.

#### Key Highlights

* Create, Update, and Delete [SObjects](http://www.salesforce.com/us/developer/docs/object_reference/index.htm#StartTopic=Content/sforce_api_objects_recentlyviewed.htm).
* Run queries written in [SOQL](http://www.salesforce.com/us/developer/docs/soql_sosl/index_Left.htm#CSHID=sforce_api_calls_soql.htm|StartTopic=Content%2Fsforce_api_calls_soql.htm|SkinName=webhelp).
* Full-text searching via [SOSL](http://www.salesforce.com/us/developer/docs/soql_sosl/index_Left.htm#CSHID=sforce_api_calls_sosl.htm|StartTopic=Content%2Fsforce_api_calls_sosl.htm|SkinName=webhelp).
* Retrieve SObject change notifications (added in v29 of the REST API).
* Use the same C# API for iOS and Android.
* Leverage async/await for keeping your UI responsive, and your code simple.
* Simple, compact, progressive API.
* Easily create your own strongly-typed domain model classes.

### Dive In

```csharp
// Creates our connection to salesforce.
var client = new SalesforceClient (clientId, clientSecret, redirectUrl);

// Get authenticated users from the local keystore
var users = client.LoadUsers ();

if (!users.Any ())
{
        client.AuthenticationComplete += (sender, e) => OnAuthenticationCompleted (e);

        // Starts the Salesforce login process.
        var loginUI = client.GetLoginInterface (); 
        DisplayThe(loginUI);
} 
else 
{
        // We're ready to fetch some data!
        // Let's grab some sales accounts to display.
        IEnumerable<SObject> results =  await client.ReadAsync ("SELECT Name, AccountNumber FROM Account");

        DoSomethingAmazingWith(results);
}
```
For more details, see [Getting Started](https://github.com/xamarin/SalesforceSDK/blob/master/Package/SalesforceSDK-1.0/component/GettingStarted.md).


#### Are you a Xamarin user? [Get the component instead](http://components.xamarin.com/view/SalesforceSDK). ####


### Project Organization ###

**Core Libraries**
 * Salesforce.Core - Plain old .NET 4.5 library. No dependencies on Xamarin stuffs.
 * Salesforce.Android - Xamarin.Android project. Source linked from Core, but defines `PLATFORM_ANDROID` and `MOBILE`.
 * Salesforce.iOS - Xamarin.iOS project. Source linked from Core, but defines `PLATFORM_IOS` and `MOBILE`.

**Sample Apps**
 * SalesforceSample.Android - Xamarin.Android sample app using `Salesforce.Android`.
 * SalesforceSample.iOS - Xamarin.iOS sample app using `Salesforce.iOS`.

**Tests**
 * Tests.Android - Xamarin.Android unit test project (NUnit Lite).
 * Tests.iOS - Xamarin.iOS unit test project (NUnit Lite).

**Distribution**
 * Package - Source files for building the distribution packages.

**Misc**
 * Documentation - Incorrectly named folder of artwork files, SFDC reference material, and notes.
 * Xamarin.Auth - Custom fork of [Xamarin.Auth](https://github.com/xamarin/Xamarin.Auth) used to implement the SFDC OAuth2 workflow.


### Release Notes ###

*1.4.x*  
**Additions**  

 * Added `Changes` and `ChangesAsync` for easy retrieval of object change info, which was introduced in v29 of the API.

 * Supports "logout" scenario by calling `Client.CurrentUser.RequiresReauthentication = true;`.

Brings assembly version up to 0.9.5092.212xx.

*1.3*  
**Additions**  

 * Added `Describe` and `DescribeAsync` for easy retrieval of object metadata.

**Enhancements**  

 * `PlatformStrings.CredentialStoreServiceName` is now configurable.

**Breaking Changes**  

 * `Search` and `SearchAsync` now return `IEnumerable<SearchResult>`.
 * `PlatformStrings.Salesforce` was renamed to `PlatformStrings.CredentialStoreServiceName`.

Brings assembly version up to 0.9.5003.24683.

*1.2*  
**Enhancements**  

 * Allows `Update` of non-string datatypes, such as `DateTime`.
 * Updated the iOS sample to demonstrate mapping the REST API's `datetime` to `System.DateTime`.

**Additions** 

* Now throwing two new exceptions: `InvalidClientIdException` and `JsonParseException`.

Brings the assembly version up to 0.9.4987.26579.

*1.1*  
**Additions**  

 * Added `Search` and `SearchAsync` for easy SOSL queries.

**Breaking Changes**  

 * Renamed SOQL overloads of `Read` and `ReadAsync` to `Query` and `QueryAsync`.
 * Renamed extension method parameter `@object` to `sobject` to make it easier to use named-parameter syntax.

Brings assembly version up to 0.9.4975.19187.

License
=======

The first public release of the SalesforceSDK is being released under the 
terms of the MIT license (despite the fact that the history in GIT contains
a template for Apache License 2, that was unreleased until now).
