using System;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WSD.Rest
{
	public class Response
	{
		public string Url;
		public HttpStatusCode StatusCode;
		public string Content;
		public string ErrorMessage;

		public string GetErrorMessage ()
		{
			if (ErrorMessage == null) {
				try {
					string error = JObject.Parse (Content).SelectToken ("error").ToString ();
					return JObject.Parse(error).SelectToken("message").ToString ();
				} catch (Exception) {
				}
			}

			return ErrorMessage;
		}

		public T Get<T>()
		{
			return JsonConvert.DeserializeObject<T> (Content);
		}

		public int GetErrorCode ()
		{

			int errorCode = 0;

			try {
				string error = JObject.Parse (Content).SelectToken ("error").ToString ();
				errorCode = int.Parse(JObject.Parse(error).SelectToken("code").ToString ());
			} catch (Exception) {
			}

			return errorCode;
		}

		public string GetString (string key)
		{
			try {
				return JObject.Parse (Content).SelectToken (key).ToString ();
			} catch (Exception) {
			}

			return null;
		}

		public bool Contains (string key)
		{
			try {
				return Content.Contains(key);
			} catch (Exception) {
			}

			return false;
		}

		public bool HasErrors ()
		{
			if (ErrorMessage == null) {
				try {
					return Content.Contains("error");
				} catch (Exception) {
				}
			}

			return ErrorMessage != null;
		}

		public void Check ()
		{
			if (Content == null) {
				throw new Exception("Unkown exception: response content was null");
			} else if (StatusCode != HttpStatusCode.OK) {
				throw new Exception(string.Format(
					"Exception: ({0}) {1}", 
					GetErrorCode(), 
					GetErrorMessage ()
				));
			}
		}
	}
}

