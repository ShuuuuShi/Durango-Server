using System.Collections;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine.Networking;

namespace Durango.Development;

internal class DumpedIslandUtils
{
	public class Response
	{
		public byte[] Data;
	}

	public static Dictionary<string, byte[]> DownloadDumpedDatas(string requestUrl)
	{
		Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>();
		byte[] buffer = Download(requestUrl);
		Stream baseInputStream = new MemoryStream(buffer);
		using ZipInputStream zipInputStream = new ZipInputStream(baseInputStream);
		ZipEntry nextEntry;
		while ((nextEntry = zipInputStream.GetNextEntry()) != null)
		{
			dictionary[nextEntry.Name] = LoadEntry(zipInputStream, nextEntry);
		}
		return dictionary;
	}

	public static byte[] LoadEntry(ZipInputStream stream, ZipEntry curEntry)
	{
		byte[] array = new byte[curEntry.Size];
		stream.Read(array, 0, (int)curEntry.Size);
		return array;
	}

	public static byte[] Download(string url)
	{
		Response response = new Response();
		IEnumerator enumerator = CoDownload(url, response);
		while (enumerator.MoveNext())
		{
		}
		return response.Data;
	}

	private static IEnumerator CoDownload(string url, Response response)
	{
		UnityWebRequest www = UnityWebRequest.Get(url);
		yield return www.SendWebRequest();
		while (!www.isDone)
		{
			yield return true;
		}
		if (www.isNetworkError || www.isHttpError)
		{
			response.Data = null;
		}
		else
		{
			response.Data = www.downloadHandler.data;
		}
	}
}
