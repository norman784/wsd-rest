using System;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Collections;

namespace WSD.Rest
{
	public class RequestContent
	{
		public static MultipartFormDataContent Parse (Dictionary<string, object> data)
		{
			if (data == null) return null;

			MultipartFormDataContent form = new MultipartFormDataContent ();

			data = GetContentDictionary (data);

			foreach (KeyValuePair<string, object> property in data) {
				if (property.Value is File) {
					if (!((File)property.Value).HasContent ())
						continue;
					File file = (File)property.Value;
					form.Add (file.GetContent (), String.Format(property.Key, property.Key.ToLower ()), file.Name);
				} else {
					form.Add (new StringContent (GetContentString(property.Value)), property.Key.ToLower ());
				}
			}

			return form;
		}

		static Dictionary<string, object> GetContentDictionary (Dictionary<string, object> data, string key = "")
		{
			Dictionary<string, object> _data = new Dictionary<string, object> ();

			foreach (KeyValuePair<string, object> property in data) {
				if (property.Value is IDictionary) {
					Dictionary<string, object> __data = GetContentDictionary (property.Value as Dictionary<string, object>, property.Key.ToLower ());

					foreach (KeyValuePair<string, object> _property in __data) {
						_data.Add (_property.Key, _property.Value);
					}
				} else if (property.Value is JContainer) {
					Dictionary<string, object> __data = GetContentJContainer (property.Value as JContainer, property.Key.ToLower ());

					foreach (KeyValuePair<string, object> _property in __data) {
						_data.Add (_property.Key, _property.Value);
					}
				} else {
					_data.Add(
						key.Length == 0 ? property.Key : String.Format ("{0}[{1}]", key, property.Key.ToLower ()),
						property.Value
					);
				}
			}

			return _data;
		}

		static Dictionary<string, object> GetContentJContainer (JContainer data, string key = "")
		{
			Dictionary<string, object> _data = new Dictionary<string, object> ();

			foreach (JToken token in data) {
				if (token is JProperty) {
					JProperty property = token as JProperty;
					_data.Add(
						key.Length == 0 ? property.Name : String.Format ("{0}[{1}]", key, property.Name.ToLower ()),
						property.Value
					);
				} else {
					Dictionary<string, object> __data = GetContentJContainer (token.Value<JContainer> (), key);

					foreach (KeyValuePair<string, object> _property in __data) {
						_data.Add (_property.Key, _property.Value);
					}
				}
			}

			return _data;
		}	

		static string GetContentString (object data)
		{
			if (data is double) {
				return ((double)data).ToString (CultureInfo.InvariantCulture);
			} else if (data is float) {
				return ((float)data).ToString (CultureInfo.InvariantCulture);
			} else if (data is decimal) {
				return ((decimal)data).ToString (CultureInfo.InvariantCulture);
			} else if (data != null) {
				return data.ToString ();
			} else {
				return string.Empty;
			}
		}
	}
}

