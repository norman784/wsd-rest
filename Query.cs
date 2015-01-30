using System;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;

namespace WSD.Rest
{
	public class Query
	{
		static public int DefaultLimit = 25;

		public string Search { get; set; }
		public int Limit { get; set; }
		public int Offset { get; set; }
		public string Order { get; set; }

		public string ToQueryString()
		{
			String separator = "?";
			 String qs = "";

			try {
				if (int.Parse(string.Format("{0}", Limit)) < 1) {
					Limit = DefaultLimit;
				}
			} catch (Exception) {
				Limit = DefaultLimit;
			}

			Dictionary<string, object> properties = ObjectHelper.GetProperties (this);

			foreach (KeyValuePair<string, object> property in properties) {
				if (property.Value == null)
					continue;

				string value = "";

				if (property.Value is double) {
					value = ((double)property.Value).ToString (CultureInfo.InvariantCulture);
				} else if (property.Value is float) {
					value = ((float)property.Value).ToString (CultureInfo.InvariantCulture);
				} else if (property.Value is decimal) {
					value = ((decimal)property.Value).ToString (CultureInfo.InvariantCulture);
				} else {
					value = property.Value.ToString ();
				}

				qs += string.Format (
					"{0}{1}={2}", 
					separator, 
					property.Key.ToString ().ToLower (), 
					value
				);
				separator = "&";
			}

			return qs;
		}
	}
}