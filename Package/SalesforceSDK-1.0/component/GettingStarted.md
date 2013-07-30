## Register for a Developer Edition Instance on Force.com

On the Salesforce Platform, you can get a Developer Edition licensed instance .  

Go [get a Developer Edition instance](http://developer.force.com/join), if you don't have one already. This "org", short for organization, is completely free and never expires. This means that you can build your applications at no cost.


## Creating a Connected App Definition on Force.com
For your app to communicate with Salesforce securely, it must configured as a "Connected App".  This will give
your app the keys required to identify itself and allow a Salesforce org administrator to manage it:

1. Log into your Developer Edition Org.
2. Open the Setup menu by clicking [Your Name] > Setup.
3. Create a new Connected App by going to App Setup > Create > Apps.
4. Click the ‘New’ button in the Connected Apps list.
5. Fill out all required fields and click ‘Save’:
6. Connected App: [Your App Name]
7. Developer Name: [Your Name]
8. Contact Email: [Your email]
9. Callback URL: In the samples, we use 'com.sample.salesforce:/oauth2Callback'

You will need to hand the resulting Consumer Key, Secret, and Callback URL to the Salesforce client when you initialize it.

## Start Coding

