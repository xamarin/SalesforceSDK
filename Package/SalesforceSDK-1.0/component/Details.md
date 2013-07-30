## Salesforce C# SDK for Xamarin

Build native apps around your Salesforce data.

Give your users the mobile experience they expect, and increasingly demand, from their enterprise apps. Take full advantage of the raw performance and rich functionality native to each platform. Meet compliance requirements by leveraging each platform's trusted-computing features, like OS-managed credential stores. 

Do it all using C#.

#### Key Highlights

* Create, Update, and Delete [SObjects](http://www.salesforce.com/us/developer/docs/object_reference/index.htm#StartTopic=Content/sforce_api_objects_recentlyviewed.htm).
* Run queries written in [SOQL](http://www.salesforce.com/us/developer/docs/soql_sosl/index_Left.htm#CSHID=sforce_api_calls_soql.htm|StartTopic=Content%2Fsforce_api_calls_soql.htm|SkinName=webhelp).
* Full-text searching via [SOSL](http://www.salesforce.com/us/developer/docs/soql_sosl/index_Left.htm#CSHID=sforce_api_calls_sosl.htm|StartTopic=Content%2Fsforce_api_calls_sosl.htm|SkinName=webhelp).
* Use the same C# API for iOS and Android.
* Leverage async/await for keeping your UI responsive, and your code simplied.
* Simple, compact API.
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
	IEnumerable<SObject> results = 
		await client.ReadAsync ("SELECT Name, AccountNumber FROM Account");

	DoSomethingAmazingWith(results);
}
```