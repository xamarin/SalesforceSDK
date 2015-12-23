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







Salesforce component also seems broken on iOS 9 - I cloned the repo and added 
the project to my own source, but got an SSL error, so I tried adding the 
component instead to check and got the same problem.  I used the same project 
code with iOS 8 about 2 weeks ago without issues.


