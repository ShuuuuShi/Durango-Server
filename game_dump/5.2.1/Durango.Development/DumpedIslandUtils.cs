using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine.Networking;

namespace Durango.Development;

internal class DumpedIslandUtils
{
	public class Response
	{
		public byte[] Data;
	}

	[CompilerGenerated]
	private sealed class _003CCoDownload_003Ed__4 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string url;

		public Response response;

		private UnityWebRequest _003Cwww_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoDownload_003Ed__4(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cwww_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cwww_003E5__2 = UnityWebRequest.Get(url);
				_003C_003E2__current = _003Cwww_003E5__2.SendWebRequest();
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				break;
			case 2:
				_003C_003E1__state = -1;
				break;
			}
			if (!_003Cwww_003E5__2.isDone)
			{
				_003C_003E2__current = true;
				_003C_003E1__state = 2;
				return true;
			}
			if (_003Cwww_003E5__2.isNetworkError || _003Cwww_003E5__2.isHttpError)
			{
				response.Data = null;
			}
			else
			{
				response.Data = _003Cwww_003E5__2.downloadHandler.data;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	public static Dictionary<string, byte[]> DownloadDumpedDatas(string requestUrl)
	{
		Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>();
		using ZipInputStream zipInputStream = new ZipInputStream(new MemoryStream(Download(requestUrl)));
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoDownload_003Ed__4(0)
		{
			url = url,
			response = response
		};
	}
}
