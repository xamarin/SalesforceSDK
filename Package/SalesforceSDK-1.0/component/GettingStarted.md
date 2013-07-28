## Register for a Developer Edition Instance on Force.com

On the Salesforce Platform, you can get a Developer Edition licensed instance (which is sometimes called an org, short for organization).  This instance is completely free and never expires, meaning that you can build your applications without a risk of cost.

In order to get a Developer Edition instance, go here:

[http://developer.force.com/join ](http://developer.force.com/join)


## Creating a Connected App Definition on Force.com
For a third party application to communicate with Salesforce API’s
securely, they must have a Connected App definition.  This will give
the app the keys required to identify itself  and allow a Salesforce
admin to control it:

1. Log into your Developer Edition Org.
2. Open the Setup menu by clicking [Your Name] > Setup.
3. Create a new Connected App by going to App Setup > Create > Apps.
4. Click the ‘New’ button in the Connected Apps list.
5. Fill out all required fields and click ‘Save’:
6. Connected App: YourAppName
7. Developer Name: YourAppName
8. Contact Email: Your email
9. Callback URL: com.sample.salesforce:/oauth2Callback

You will need to hand the resulting Consumer Key and Callback URL to the Salesforce client when you initialize it.# Getting Started with SalesforceSDK-1.0