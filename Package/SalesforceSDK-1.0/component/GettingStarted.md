## Register for a Developer Edition Instance on Force.com

On the Salesforce Platform, you can get a Developer Edition licensed instance .  

Go [get a Developer Edition instance](http://developer.force.com/join), if you don't have one already. This "org", short for organization, is completely free and never expires. This means that you can build your applications at no cost.


## Creating a Connected App Definition on Force.com
For your app to communicate with Salesforce securely, it must configured as a "Connected App".  This will give
your app the keys required to identify itself and allow a Salesforce org administrator to manage it:

1. Log into your Developer Edition Org.
2. Open the Setup menu by clicking [Your Name] > Setup.
3. Create a new Connected App by going to `App Setup` > `Create` > `Apps`.
4. Click the `New` button in the `Connected Apps` list.
5. Fill out all required fields and click `Save`.
6. Connected App: `[Your App Name]`
7. Developer Name: `[Your Name]`
8. Contact Email: `[Your email]`
9. Callback URL: In the samples, we use `com.sample.salesforce:/oauth2Callback`

You will need to hand the resulting `Client Key`, `Client Secret`, and `Callback URL` to the Salesforce client when you initialize it.

## Start Coding

Now we're ready to code. We start by creating a new `SalesforceClient`. It handles all of the chores of communicating with the Salesforce REST API.

First step: you'll need the values from your setup in the previous examples.
```csharp
// Creates our connection to salesforce.
var client = new SalesforceClient (clientId, clientSecret, redirectUrl);
```

You'll want to check to see if we have already saved user credentials. If we haven't, we need to display the Salesforce login user interface.
If we do have users available, then we can skip right to making a request.

```csharp
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

As you can see, we make it easy to get access to your data with just a few lines of code.

For more sophisticated queries, like using SOSL for full-text searching, we use a generalized pattern. Here's an example:

```csharp
var request = new ReadRequest {
	Resource = new Search { QueryText = "FIND {John}" }
};

var results = await Client.ProcessAsync<ReadRequest> (request);
```