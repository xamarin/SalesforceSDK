using System;

using MonoTouch.Foundation;

namespace Salesforce {

	public enum SFRestMethod {
		GET = 0,
		POST,
		PUT,
		DELETE,
		HEAD,
		PATCH
	}

	[Model, BaseType (typeof (NSObject))]
	public partial interface RestDelegate {

		[Export ("request:didLoadResponse:")]
		void DidLoadResponse (RestDelegate request, NSObject dataResponse);

		[Export ("request:didFailLoadWithError:")]
		void DidFailLoadWithError (RestDelegate request, NSError error);

		[Export ("requestDidCancelLoad:")]
		void  DidCancelLoad(RestDelegate request);

		[Export ("requestDidTimeout:")]
		void  DidTimeout(RestDelegate request);
	}

	[BaseType (typeof (NSObject))]
	public partial interface RestDelegate {

		[Export ("method")]
		SFRestMethod Method { get; set; }

		[Export ("path", ArgumentSemantic.Retain)]
		string Path { get; set; }

		[Export ("queryParams", ArgumentSemantic.Retain)]
		NSDictionary QueryParams { get; set; }

		[Export ("delegate", ArgumentSemantic.Assign)]
		RestDelegate Delegate { get; set; }

		[Export ("endpoint", ArgumentSemantic.Retain)]
		string Endpoint { get; set; }

		[Export ("parseResponse")]
		bool ParseResponse { get; set; }

		[Static, Export ("requestWithMethod:path:queryParams:")]
		NSObject RequestWithMethod (SFRestMethod method, string path, NSDictionary queryParams);

		[Field ("kSFRestErrorDomain")]
		NSString SFRestErrorDomain { get; }

		[Field ("kSFRestErrorCode")]
		NSInteger SFRestErrorCode { get; }

		[Field ("kSFRestDefaultAPIVersion")]
		NSString SFRestDefaultAPIVersion { get; }

		[Field ("kSFMobileSDKNativeDesignator")]
		NSString SFMobileSDKNativeDesignator { get; }
	}

	[BaseType (typeof (NSObject))]
	public partial interface RestAPI {

		[Export ("coordinator", ArgumentSemantic.Retain)]
		SFOAuthCoordinator Coordinator { get; set; }

		[Export ("rkClient", ArgumentSemantic.Retain)]
		RKClient RkClient { get; }

		[Export ("apiVersion", ArgumentSemantic.Retain)]
		string ApiVersion { get; set; }

		[Static, Export ("sharedInstance")]
		RestAPI SharedInstance { get; }

		[Export ("send:delegate:")]
		void Send (RestDelegate request, RestDelegate callback);

		[Export ("requestForVersions")]
		RestDelegate RequestForVersions { get; }

		[Export ("requestForResources")]
		RestDelegate RequestForResources { get; }

		[Export ("requestForDescribeGlobal")]
		RestDelegate RequestForDescribeGlobal { get; }

		[Export ("requestForMetadataWithObjectType:")]
		RestDelegate RequestForMetadataWithObjectType (string objectType);

		[Export ("requestForDescribeWithObjectType:")]
		RestDelegate RequestForDescribeWithObjectType (string objectType);

		[Export ("requestForRetrieveWithObjectType:objectId:fieldList:")]
		RestDelegate RequestForRetrieveWithObjectType (string objectType, string objectId, string fieldList);

		[Export ("requestForCreateWithObjectType:fields:")]
		RestDelegate RequestForCreateWithObjectType (string objectType, NSDictionary fields);

		[Export ("requestForUpsertWithObjectType:externalIdField:externalId:fields:")]
		RestDelegate RequestForUpsertWithObjectType (string objectType, string externalIdField, string externalId, NSDictionary fields);

		[Export ("requestForUpdateWithObjectType:objectId:fields:")]
		RestDelegate RequestForUpdateWithObjectType (string objectType, string objectId, NSDictionary fields);

		[Export ("requestForDeleteWithObjectType:objectId:")]
		RestDelegate RequestForDeleteWithObjectType (string objectType, string objectId);

		[Export ("requestForQuery:")]
		RestDelegate RequestForQuery (string soql);

		[Export ("requestForSearch:")]
		RestDelegate RequestForSearch (string sosl);

		[Static, Export ("userAgentString")]
		string UserAgentString { get; }
	}

	[Category, BaseType (typeof (RestAPI))]
	public partial interface Blocks_RestAPI : RestDelegate {

		[Static, Export ("errorWithDescription:")]
		NSError ErrorWithDescription (string description);

		[Export ("sendRESTRequest:failBlock:completeBlock:")]
		void SendRESTRequest (RestDelegate request, SFRestFailBlock failBlock, NSObject completeBlock);

		[Export ("performSOQLQuery:failBlock:completeBlock:")]
		RestDelegate PerformSOQLQuery (string query, SFRestFailBlock failBlock, SFRestDictionaryResponseBlock completeBlock);

		[Export ("performSOSLSearch:failBlock:completeBlock:")]
		RestDelegate PerformSOSLSearch (string search, SFRestFailBlock failBlock, SFRestArrayResponseBlock completeBlock);

		[Export ("performDescribeGlobalWithFailBlock:completeBlock:")]
		RestDelegate PerformDescribeGlobalWithFailBlock (SFRestFailBlock failBlock, SFRestDictionaryResponseBlock completeBlock);

		[Export ("performDescribeWithObjectType:failBlock:completeBlock:")]
		RestDelegate PerformDescribeWithObjectType (string objectType, SFRestFailBlock failBlock, SFRestDictionaryResponseBlock completeBlock);

		[Export ("performMetadataWithObjectType:failBlock:completeBlock:")]
		RestDelegate PerformMetadataWithObjectType (string objectType, SFRestFailBlock failBlock, SFRestDictionaryResponseBlock completeBlock);

		[Export ("performRetrieveWithObjectType:objectId:fieldList:failBlock:completeBlock:")]
		RestDelegate PerformRetrieveWithObjectType (string objectType, string objectId, NSObject [] fieldList, SFRestFailBlock failBlock, SFRestDictionaryResponseBlock completeBlock);

		[Export ("performUpdateWithObjectType:objectId:fields:failBlock:completeBlock:")]
		RestDelegate PerformUpdateWithObjectType (string objectType, string objectId, NSDictionary fields, SFRestFailBlock failBlock, SFRestDictionaryResponseBlock completeBlock);

		[Export ("performUpsertWithObjectType:externalIdField:externalId:fields:failBlock:completeBlock:")]
		RestDelegate PerformUpsertWithObjectType (string objectType, string externalIdField, string externalId, NSDictionary fields, SFRestFailBlock failBlock, SFRestDictionaryResponseBlock completeBlock);

		[Export ("performDeleteWithObjectType:objectId:failBlock:completeBlock:")]
		RestDelegate PerformDeleteWithObjectType (string objectType, string objectId, SFRestFailBlock failBlock, SFRestDictionaryResponseBlock completeBlock);

		[Export ("performCreateWithObjectType:fields:failBlock:completeBlock:")]
		RestDelegate PerformCreateWithObjectType (string objectType, NSDictionary fields, SFRestFailBlock failBlock, SFRestDictionaryResponseBlock completeBlock);

		[Export ("performRequestForResourcesWithFailBlock:completeBlock:")]
		RestDelegate PerformRequestForResourcesWithFailBlock (SFRestFailBlock failBlock, SFRestDictionaryResponseBlock completeBlock);

		[Export ("performRequestForVersionsWithFailBlock:completeBlock:")]
		RestDelegate PerformRequestForVersionsWithFailBlock (SFRestFailBlock failBlock, SFRestDictionaryResponseBlock completeBlock);
	}

	[Model, BaseType (typeof (NSObject))]
	public partial interface RestDelegate {

		[Export ("request:didLoadResponse:")]
		void DidLoadResponse (RestDelegate request, NSObject dataResponse);

		[Export ("request:didFailLoadWithError:")]
		void DidFailLoadWithError (RestDelegate request, NSError error);

		[Export ("requestDidCancelLoad:")]
		void  DidCancelLoad(RestDelegate request);

		[Export ("requestDidTimeout:")]
		void  DidTimeout(RestDelegate request);
	}

	[BaseType (typeof (NSObject))]
	public partial interface RestAPI {

		[Export ("send:delegate:")]
		void Send (RestDelegate request, RestDelegate callback);

		[Export ("requestForMetadataWithObjectType:")]
		RestDelegate RequestForMetadataWithObjectType (string objectType);

		[Export ("requestForDescribeWithObjectType:")]
		RestDelegate RequestForDescribeWithObjectType (string objectType);

		[Export ("requestForRetrieveWithObjectType:objectId:fieldList:")]
		RestDelegate RequestForRetrieveWithObjectType (string objectType, string objectId, string fieldList);

		[Export ("requestForCreateWithObjectType:fields:")]
		RestDelegate RequestForCreateWithObjectType (string objectType, NSDictionary fields);

		[Export ("requestForUpsertWithObjectType:externalIdField:externalId:fields:")]
		RestDelegate RequestForUpsertWithObjectType (string objectType, string externalIdField, string externalId, NSDictionary fields);

		[Export ("requestForUpdateWithObjectType:objectId:fields:")]
		RestDelegate RequestForUpdateWithObjectType (string objectType, string objectId, NSDictionary fields);

		[Export ("requestForDeleteWithObjectType:objectId:")]
		RestDelegate RequestForDeleteWithObjectType (string objectType, string objectId);

		[Export ("requestForQuery:")]
		RestDelegate RequestForQuery (string soql);

		[Export ("requestForSearch:")]
		RestDelegate RequestForSearch (string sosl);

		[Field ("kSOSLReservedCharacters")]
		NSString SOSLReservedCharacters { get; }

		[Field ("kSOSLEscapeCharacter")]
		NSString SOSLEscapeCharacter { get; }

		[Field ("kMaxSOSLSearchLimit")]
		NSInteger MaxSOSLSearchLimit { get; }
	}

	[Category, BaseType (typeof (RestAPI))]
	public partial interface QueryBuilder {

		[Static, Export ("sanitizeSOSLSearchTerm:")]
		string SanitizeSOSLSearchTerm (string searchTerm);

		[Static, Export ("SOSLSearchWithSearchTerm:objectScope:")]
		string SOSLSearchWithSearchTerm (string term, NSDictionary objectScope);

		[Static, Export ("SOSLSearchWithSearchTerm:fieldScope:objectScope:limit:")]
		string SOSLSearchWithSearchTerm (string term, string fieldScope, NSDictionary objectScope, int limit);

		[Static, Export ("SOQLQueryWithFields:sObject:where:limit:")]
		string SOQLQueryWithFields (NSObject [] fields, string sObject, string where, int limit);

		[Static, Export ("SOQLQueryWithFields:sObject:where:groupBy:having:orderBy:limit:")]
		string SOQLQueryWithFields (NSObject [] fields, string sObject, string where, NSObject [] groupBy, string having, NSObject [] orderBy, int limit);
	}

	[BaseType (typeof (NSObject))]
	public partial interface RestAPI {

		[Export ("requestForMetadataWithObjectType:")]
		RestDelegate RequestForMetadataWithObjectType (string objectType);

		[Export ("requestForDescribeWithObjectType:")]
		RestDelegate RequestForDescribeWithObjectType (string objectType);

		[Export ("requestForRetrieveWithObjectType:objectId:fieldList:")]
		RestDelegate RequestForRetrieveWithObjectType (string objectType, string objectId, string fieldList);

		[Export ("requestForCreateWithObjectType:fields:")]
		RestDelegate RequestForCreateWithObjectType (string objectType, NSDictionary fields);

		[Export ("requestForUpsertWithObjectType:externalIdField:externalId:fields:")]
		RestDelegate RequestForUpsertWithObjectType (string objectType, string externalIdField, string externalId, NSDictionary fields);

		[Export ("requestForUpdateWithObjectType:objectId:fields:")]
		RestDelegate RequestForUpdateWithObjectType (string objectType, string objectId, NSDictionary fields);

		[Export ("requestForDeleteWithObjectType:objectId:")]
		RestDelegate RequestForDeleteWithObjectType (string objectType, string objectId);

		[Export ("requestForQuery:")]
		RestDelegate RequestForQuery (string soql);

		[Export ("requestForSearch:")]
		RestDelegate RequestForSearch (string sosl);
	}

	[BaseType (typeof (NSObject))]
	public partial interface RestDelegate {
		[Static, Export ("requestWithMethod:path:queryParams:")]
		NSObject RequestWithMethod (SFRestMethod method, string path, NSDictionary queryParams);
	}
}
