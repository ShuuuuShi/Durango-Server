using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SharpRaven.Utilities;

internal class GzipUtil
{
	public static string CompressEncode(string json)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(json);
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
		{
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		return Convert.ToBase64String(memoryStream.ToArray());
	}
}
