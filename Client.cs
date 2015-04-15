using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Threading;

namespace WSD.Rest
{
	public class Client
	{
		static public string ClientLibrary = "WSD-Rest-1.2.0";
		static string ClientLibraryKey = "client-library";

		static bool ApplicationSetup = false;
		static bool ApplicationInitialized = false;

		static string ApplicationIdKey;
		static string ApplicationSecretKey;
		static string ApplicationAccessTokenKey;

		static string ApplicationIdValue;
		static string ApplicationSecretValue;
		static string ApplicationAccessTokenValue;

		static string Url;
		static string Version;

		const int TotalNumberOfAttempts = 10;

		static Dictionary<string, string> Headers = new Dictionary<string, string>();

		public static void Setup (string releaseUrl, string debugUrl = null, string version = "", string applicationIdKey = "x-application-id", string applicationSecretKey = "x-application-secret", string applicationAccessTokenKey = "x-access-token")
		{
			ApplicationSetup = true;

			Version = version;
			ApplicationIdKey = applicationIdKey;
			ApplicationSecretKey = applicationSecretKey;
			ApplicationAccessTokenKey = applicationAccessTokenKey;

			Url = releaseUrl;

			#if DEBUG
			if (debugUrl != null) {
				Url = debugUrl;
			}
			#endif
		}

		public static void Init (string applicationIdValue, string applicationSecretValue)
		{
			ApplicationInitialized = true;

			ApplicationIdValue = applicationIdValue;
			ApplicationSecretValue = applicationSecretValue;

			if (ApplicationIdValue != null) {
				if (Headers.ContainsKey(ApplicationIdKey)) {
					Headers.Remove(ApplicationIdKey);
				}
				Headers.Add (ApplicationIdKey, ApplicationIdValue);
			}

			if (ApplicationSecretValue != null) {
				if (Headers.ContainsKey(ApplicationSecretKey)) {
					Headers.Remove(ApplicationSecretKey);
				}
				Headers.Add (ApplicationSecretKey, ApplicationSecretValue);
			}

			if (Headers.ContainsKey(ClientLibraryKey)) {
				Headers.Remove(ClientLibraryKey);
			}

			Headers.Add (ClientLibraryKey, ClientLibrary);
		}

		public static void SetAccessToken (string applicationAccessTokenValue)
		{
			ApplicationAccessTokenValue = applicationAccessTokenValue;

			if (Headers.ContainsKey(ApplicationAccessTokenKey)) {
				Headers.Remove(ApplicationAccessTokenKey);
			}

			if (ApplicationAccessTokenValue != null) {
				Headers.Add (ApplicationAccessTokenKey, ApplicationAccessTokenValue);
			}
		}

		public static string GetAccessToken ()
		{
			return ApplicationAccessTokenValue;
		}

		public static string GetUrl (bool version = true)
		{
			return Url + Version;
		}

		public static async Task<Response> Get (string api, Query query = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await Request (
				HttpMethod.Get,
				string.Format("{0}{1}", api, query != null ? query.ToQueryString () : ""),
				null,
				cancellationToken
			);
		}

		public static async Task<Response> Post (string api, string id = null, Dictionary<string, object> data = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			MultipartFormDataContent content = RequestContent.Parse(data);

			return await Request (
				HttpMethod.Post, 
				id != null ? string.Format ("{0}/{1}", api, id) : api, 
				content,
				cancellationToken
			);
		}

		public static async Task<Response> Put (string api, string id = null, Dictionary<string, object> data = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			MultipartFormDataContent content = RequestContent.Parse(data);

			return await Request (
				HttpMethod.Put, 
				id != null ? string.Format ("{0}/{1}", api, id) : api, 
				content,
				cancellationToken
			);
		}

		public static async Task<Response> Delete (string api, string id, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await Request (
				HttpMethod.Delete, 
				string.Format("{0}/{1}", api, id),
				null,
				cancellationToken
			);
		}

		static async Task<Response> Request (HttpMethod method, string api, MultipartContent content = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			int numberOfAttempts = 0;
			if (!ApplicationSetup) {
				throw new Exception ("You must run Client.Setup!");
			} else if (!ApplicationInitialized) {
				throw new Exception ("You must run Client.Init!");
			}

			string url = Url + Version + api;
			Response response = new Response ();
			response.Url = url;

			HttpClient httpClient = new HttpClient ();
			httpClient.MaxResponseContentBufferSize = 256000;

			foreach (KeyValuePair<string, string> header in Headers) {
				if (header.Key == null || header.Key.Equals ("")) continue;
				httpClient.DefaultRequestHeaders.Add (header.Key, header.Value);
			}

			HttpResponseMessage httpResponse = null;

//			do {
				try {
				if (method.Equals(HttpMethod.Get)) {
					httpResponse = await httpClient.GetAsync (url, cancellationToken).ConfigureAwait (false);
				} else if (method.Equals(HttpMethod.Post)) {
					httpResponse = await httpClient.PostAsync (url, content, cancellationToken).ConfigureAwait (false);
				} else if (method.Equals(HttpMethod.Put)) {
					httpResponse = await httpClient.PutAsync (url, content, cancellationToken).ConfigureAwait (false);
				} else if (method.Equals(HttpMethod.Delete)) {
					httpResponse = await httpClient.DeleteAsync (url, cancellationToken).ConfigureAwait (false);
				}

				if (httpResponse == null) {
					response.Content = null;
					response.StatusCode = 0;
				} else {
					HttpContent httpContent = httpResponse.Content;
					response.Content = await httpContent.ReadAsStringAsync ();
					response.StatusCode = httpResponse.StatusCode;
				}

					return response;
				} catch (AggregateException ex) {
					++numberOfAttempts;
					response.Content = ex.Message;
					response.StatusCode = HttpStatusCode.Gone;
				} catch (System.Exception e) {
					response.Content = e.Message;
					response.StatusCode = HttpStatusCode.Gone;
				}
//			} while (numberOfAttempts < TotalNumberOfAttempts);

			return response;
		}
	}
}