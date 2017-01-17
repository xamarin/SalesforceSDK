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

### Changelog

#### v.1.4.3.5

2015-09-28

##### iOS9 Changes App Transport Security 

In iOS9 Apple has introduced ATS = App Transport Security is a feature that 
improves the security of connections between an app and web services. The 
feature consists of default connection requirements that conform to best 
practices for secure connections. ATS mandates that from iOS9 all http 
connections should be secure using minimum of TLS 1.2.

Apps can override this default behavior and turn off transport security.
Users can opt out with an entry in the info.plist.

[https://developer.apple.com/library/prerelease/ios/technotes/App-Transport-Security-Technote/](https://developer.apple.com/library/prerelease/ios/technotes/App-Transport-Security-Technote/)

Describes the exception mechanism developer can use to override the default 
behaviors of App Transport Security for web services connections.

Some Xamarin components might experience issues with iOS9 - Salesforce
component does. An update to the component is being worked on, however as a 
temporary workaround, it is possible to disable ATS and get the component 
running again by adding the following to app's Info.plist:

	<key>NSAppTransportSecurity</key>
		<dict>
			<key>NSAllowsArbitraryLoads</key>
			<true/>
	</dict>

Putting this at the end of the file just above the final </dict> tag should be 
fine, or alternatively there is possibility to use the Plist editor built into 
IDE.

##### AuthPath and TokenPath Porperties

Harcoded values for AuthPath and TokenPath changed to properties in order to
enable sandboxed testing with different urls.

