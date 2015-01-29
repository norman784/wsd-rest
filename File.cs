using System;
using System.Net.Http;

namespace WSD.Rest
{
	public class File
	{
		static public class Mime
		{
			public static string HTML 		= "text/html";
			public static string JPG 		= "image/jpg";
			public static string PNG 		= "image/png";
			public static string TextPlain 	= "text/plain";
		}

		public string Name;
		public string Type;
		public string Url;

		byte[] Data;

		public File (string name, string type, byte[] data)
		{
			Name = name;
			Type = type;
			Data = data;
		}

		public ByteArrayContent GetContent()
		{
			ByteArrayContent content = new ByteArrayContent (Data);
			content.Headers.Add ("Content-Type", Type);
			return content;
		}

		public bool HasContent()
		{
			return !(Data == null || Data.Length == 0);
		}
	}
}

