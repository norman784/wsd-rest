using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Globalization;

namespace WSD.Rest
{
	public class Client
	{
		public enum Method {
			GET,
			POST,
			PUT,
			DELETE
		}

		static public string UserAgent = "WSD-Rest-1.1.0";

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
		}

		public static void SetAccessToken (string applicationAccessTokenValue)
		{
			ApplicationAccessTokenValue = applicationAccessTokenValue;

			if (ApplicationAccessTokenValue != null) {
				if (Headers.ContainsKey(ApplicationAccessTokenKey)) {
					Headers.Remove(ApplicationAccessTokenKey);
				}
				Headers.Add (ApplicationAccessTokenKey, ApplicationAccessTokenValue);
			}
		}

		public static string GetAccessToken ()
		{
			return ApplicationAccessTokenValue;
		}

		public static async Task<Response> Get (string api, Query query = null)
		{
			return await Request (
				Method.GET, 
				string.Format("{0}{1}", api, query != null ? query.ToQueryString () : "")
			);
		}

		public static async Task<Response> Post (string api, string id, Dictionary<string, object> data)
		{
			MultipartFormDataContent content = GetContent(data);

			return await Request (
				Method.POST, 
				id != null ? string.Format ("{0}/{1}", api, id) : api, 
				content
			);
		}

		public static async Task<Response> Put (string api, string id, Dictionary<string, object> data)
		{
			MultipartFormDataContent content = GetContent(data);

			return await Request (
				Method.PUT, 
				id != null ? string.Format ("{0}/{1}", api, id) : api, 
				content
			);
		}

		public static async Task<Response> Delete (string api, string id)
		{
			return await Request (
				Method.DELETE, 
				string.Format("{0}/{1}", api, id)
			);
		}

		static MultipartFormDataContent GetContent (Dictionary<string, object> data)
		{
			MultipartFormDataContent form = new MultipartFormDataContent ();

			foreach (KeyValuePair<string, object> property in data) {
				if (property.Value is File) {
					if (!((File)property.Value).HasContent ())
						continue;
					File file = (File) property.Value;
					form.Add (file.GetContent (), property.Key.ToLower(), file.Name);
				} else {
					string value = "";

					if (property.Value is double) {
						value = ((double)property.Value).ToString (CultureInfo.InvariantCulture);
					} else if (property.Value is float) {
						value = ((float)property.Value).ToString (CultureInfo.InvariantCulture);
					} else if (property.Value is decimal) {
						value = ((decimal)property.Value).ToString (CultureInfo.InvariantCulture);
					} else if (property.Value != null) {
						value = property.Value.ToString ();
					}

					form.Add (new StringContent (value), property.Key.ToLower ());
				}
			}

			return form;
		}

		static async Task<Response> Request (Method method, string api, MultipartContent content = null)
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

			httpClient.DefaultRequestHeaders.UserAgent.ParseAdd (UserAgent);

			foreach (KeyValuePair<string, string> header in Headers) {
				httpClient.DefaultRequestHeaders.Add (header.Key, header.Value);
			}

			HttpResponseMessage httpResponse = null;

//			do {
				try {
					switch (method) {
					case Method.GET:
						httpResponse = await httpClient.GetAsync (url).ConfigureAwait (false);
						break;
					case Method.POST:
						httpResponse = await httpClient.PostAsync (url, content).ConfigureAwait (false);
						break;
					case Method.PUT:
						httpResponse = await httpClient.PutAsync (url, content).ConfigureAwait (false);
						break;
					case Method.DELETE:
						httpResponse = await httpClient.DeleteAsync (url).ConfigureAwait (false);
						break;
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