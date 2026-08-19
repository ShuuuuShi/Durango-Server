using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace BestHTTP.Caching;

public static class HTTPCacheService
{
	private const int LibraryVersion = 2;

	private static bool isSupported;

	private static bool IsSupportCheckDone;

	private static Dictionary<Uri, HTTPCacheFileInfo> library;

	private static Dictionary<ulong, HTTPCacheFileInfo> UsedIndexes;

	private static bool InClearThread;

	private static bool InMaintainenceThread;

	private static ulong NextNameIDX;

	public static bool IsSupported
	{
		get
		{
			if (IsSupportCheckDone)
			{
				return isSupported;
			}
			try
			{
				File.Exists(HTTPManager.GetRootCacheFolder());
				isSupported = true;
			}
			catch
			{
				isSupported = false;
				HTTPManager.Logger.Warning("HTTPCacheService", "Cache Service Disabled!");
			}
			finally
			{
				IsSupportCheckDone = true;
			}
			return isSupported;
		}
	}

	private static Dictionary<Uri, HTTPCacheFileInfo> Library
	{
		get
		{
			LoadLibrary();
			return library;
		}
	}

	internal static string CacheFolder { get; private set; }

	private static string LibraryPath { get; set; }

	static HTTPCacheService()
	{
		UsedIndexes = new Dictionary<ulong, HTTPCacheFileInfo>();
		NextNameIDX = 1uL;
	}

	internal static void CheckSetup()
	{
		if (!IsSupported)
		{
			return;
		}
		try
		{
			SetupCacheFolder();
			LoadLibrary();
		}
		catch
		{
		}
	}

	internal static void SetupCacheFolder()
	{
		if (!IsSupported)
		{
			return;
		}
		try
		{
			if (string.IsNullOrEmpty(CacheFolder) || string.IsNullOrEmpty(LibraryPath))
			{
				CacheFolder = Path.Combine(HTTPManager.GetRootCacheFolder(), "HTTPCache");
				if (!Directory.Exists(CacheFolder))
				{
					Directory.CreateDirectory(CacheFolder);
				}
				LibraryPath = Path.Combine(HTTPManager.GetRootCacheFolder(), "Library");
			}
		}
		catch
		{
		}
	}

	internal static ulong GetNameIdx()
	{
		lock (Library)
		{
			ulong nextNameIDX = NextNameIDX;
			do
			{
				NextNameIDX = ++NextNameIDX % ulong.MaxValue;
			}
			while (UsedIndexes.ContainsKey(NextNameIDX));
			return nextNameIDX;
		}
	}

	internal static bool HasEntity(Uri uri)
	{
		if (!IsSupported)
		{
			return false;
		}
		lock (Library)
		{
			return Library.ContainsKey(uri);
		}
	}

	internal static bool DeleteEntity(Uri uri, bool removeFromLibrary = true)
	{
		if (!IsSupported)
		{
			return false;
		}
		object obj = HTTPCacheFileLock.Acquire(uri);
		lock (obj)
		{
			try
			{
				lock (Library)
				{
					HTTPCacheFileInfo value;
					bool flag = Library.TryGetValue(uri, out value);
					if (flag)
					{
						value.Delete();
					}
					if (flag && removeFromLibrary)
					{
						Library.Remove(uri);
						UsedIndexes.Remove(value.MappedNameIDX);
					}
					return true;
				}
			}
			finally
			{
			}
		}
	}

	internal static bool IsCachedEntityExpiresInTheFuture(HTTPRequest request)
	{
		if (!IsSupported)
		{
			return false;
		}
		lock (Library)
		{
			if (Library.TryGetValue(request.CurrentUri, out var value))
			{
				return value.WillExpireInTheFuture();
			}
		}
		return false;
	}

	internal static void SetHeaders(HTTPRequest request)
	{
		if (!IsSupported)
		{
			return;
		}
		lock (Library)
		{
			if (Library.TryGetValue(request.CurrentUri, out var value))
			{
				value.SetUpRevalidationHeaders(request);
			}
		}
	}

	internal static Stream GetBody(Uri uri, out int length)
	{
		length = 0;
		if (!IsSupported)
		{
			return null;
		}
		lock (Library)
		{
			if (Library.TryGetValue(uri, out var value))
			{
				return value.GetBodyStream(out length);
			}
		}
		return null;
	}

	internal static HTTPResponse GetFullResponse(HTTPRequest request)
	{
		if (!IsSupported)
		{
			return null;
		}
		lock (Library)
		{
			if (Library.TryGetValue(request.CurrentUri, out var value))
			{
				return value.ReadResponseTo(request);
			}
		}
		return null;
	}

	internal static bool IsCacheble(Uri uri, HTTPMethods method, HTTPResponse response)
	{
		if (!IsSupported)
		{
			return false;
		}
		if (method != 0)
		{
			return false;
		}
		if (response == null)
		{
			return false;
		}
		if (response.StatusCode == 304)
		{
			return false;
		}
		if (response.StatusCode < 200 || response.StatusCode >= 400)
		{
			return false;
		}
		List<string> headerValues = response.GetHeaderValues("cache-control");
		if (headerValues != null && headerValues.Exists(delegate(string headerValue)
		{
			string text2 = headerValue.ToLower();
			return text2.Contains("no-store") || text2.Contains("no-cache");
		}))
		{
			return false;
		}
		List<string> headerValues2 = response.GetHeaderValues("pragma");
		if (headerValues2 != null && headerValues2.Exists(delegate(string headerValue)
		{
			string text = headerValue.ToLower();
			return text.Contains("no-store") || text.Contains("no-cache");
		}))
		{
			return false;
		}
		List<string> headerValues3 = response.GetHeaderValues("content-range");
		if (headerValues3 != null)
		{
			return false;
		}
		return true;
	}

	internal static HTTPCacheFileInfo Store(Uri uri, HTTPMethods method, HTTPResponse response)
	{
		if (response == null || response.Data == null || response.Data.Length == 0)
		{
			return null;
		}
		if (!IsSupported)
		{
			return null;
		}
		HTTPCacheFileInfo value = null;
		lock (Library)
		{
			if (!Library.TryGetValue(uri, out value))
			{
				Library.Add(uri, value = new HTTPCacheFileInfo(uri));
				UsedIndexes.Add(value.MappedNameIDX, value);
			}
			try
			{
				value.Store(response);
				return value;
			}
			catch
			{
				DeleteEntity(uri);
				throw;
			}
		}
	}

	internal static Stream PrepareStreamed(Uri uri, HTTPResponse response)
	{
		if (!IsSupported)
		{
			return null;
		}
		lock (Library)
		{
			if (!Library.TryGetValue(uri, out var value))
			{
				Library.Add(uri, value = new HTTPCacheFileInfo(uri));
				UsedIndexes.Add(value.MappedNameIDX, value);
			}
			try
			{
				return value.GetSaveStream(response);
			}
			catch
			{
				DeleteEntity(uri);
				throw;
			}
		}
	}

	public static void BeginClear()
	{
		if (IsSupported && !InClearThread)
		{
			InClearThread = true;
			SetupCacheFolder();
			new Thread(ClearImpl).Start();
		}
	}

	private static void ClearImpl(object param)
	{
		if (!IsSupported)
		{
			return;
		}
		try
		{
			string[] files = Directory.GetFiles(CacheFolder);
			for (int i = 0; i < files.Length; i++)
			{
				try
				{
					File.Delete(files[i]);
				}
				catch
				{
				}
			}
		}
		finally
		{
			UsedIndexes.Clear();
			library.Clear();
			NextNameIDX = 1uL;
			SaveLibrary();
			InClearThread = false;
		}
	}

	public static void BeginMaintainence(HTTPCacheMaintananceParams maintananceParam)
	{
		if (maintananceParam == null)
		{
			throw new ArgumentNullException("maintananceParams == null");
		}
		if (!IsSupported || InMaintainenceThread)
		{
			return;
		}
		InMaintainenceThread = true;
		SetupCacheFolder();
		new Thread((ParameterizedThreadStart)delegate
		{
			try
			{
				lock (Library)
				{
					DateTime dateTime = DateTime.UtcNow - maintananceParam.DeleteOlder;
					List<HTTPCacheFileInfo> list = new List<HTTPCacheFileInfo>();
					foreach (KeyValuePair<Uri, HTTPCacheFileInfo> item in Library)
					{
						if (item.Value.LastAccess < dateTime && DeleteEntity(item.Key, removeFromLibrary: false))
						{
							list.Add(item.Value);
						}
					}
					for (int i = 0; i < list.Count; i++)
					{
						Library.Remove(list[i].Uri);
						UsedIndexes.Remove(list[i].MappedNameIDX);
					}
					list.Clear();
					ulong num = GetCacheSize();
					if (num > maintananceParam.MaxCacheSize)
					{
						List<HTTPCacheFileInfo> list2 = new List<HTTPCacheFileInfo>(library.Count);
						foreach (KeyValuePair<Uri, HTTPCacheFileInfo> item2 in library)
						{
							list2.Add(item2.Value);
						}
						list2.Sort();
						int num2 = 0;
						while (num >= maintananceParam.MaxCacheSize && num2 < list2.Count)
						{
							try
							{
								HTTPCacheFileInfo hTTPCacheFileInfo = list2[num2];
								ulong num3 = (ulong)hTTPCacheFileInfo.BodyLength;
								DeleteEntity(hTTPCacheFileInfo.Uri);
								num -= num3;
							}
							catch
							{
							}
							finally
							{
								num2++;
							}
						}
					}
				}
			}
			finally
			{
				SaveLibrary();
				InMaintainenceThread = false;
			}
		}).Start();
	}

	public static int GetCacheEntityCount()
	{
		if (!IsSupported)
		{
			return 0;
		}
		CheckSetup();
		lock (Library)
		{
			return Library.Count;
		}
	}

	public static ulong GetCacheSize()
	{
		ulong num = 0uL;
		if (!IsSupported)
		{
			return num;
		}
		CheckSetup();
		lock (Library)
		{
			foreach (KeyValuePair<Uri, HTTPCacheFileInfo> item in Library)
			{
				if (item.Value.BodyLength > 0)
				{
					num += (ulong)item.Value.BodyLength;
				}
			}
			return num;
		}
	}

	private static void LoadLibrary()
	{
		if (library != null || !IsSupported)
		{
			return;
		}
		library = new Dictionary<Uri, HTTPCacheFileInfo>();
		if (!File.Exists(LibraryPath))
		{
			DeleteUnusedFiles();
			return;
		}
		try
		{
			int num;
			lock (library)
			{
				using FileStream input = new FileStream(LibraryPath, FileMode.Open);
				using BinaryReader binaryReader = new BinaryReader(input);
				num = binaryReader.ReadInt32();
				if (num > 1)
				{
					NextNameIDX = binaryReader.ReadUInt64();
				}
				int num2 = binaryReader.ReadInt32();
				for (int i = 0; i < num2; i++)
				{
					Uri uri = new Uri(binaryReader.ReadString());
					HTTPCacheFileInfo hTTPCacheFileInfo = new HTTPCacheFileInfo(uri, binaryReader, num);
					if (hTTPCacheFileInfo.IsExists())
					{
						library.Add(uri, hTTPCacheFileInfo);
						if (num > 1)
						{
							UsedIndexes.Add(hTTPCacheFileInfo.MappedNameIDX, hTTPCacheFileInfo);
						}
					}
				}
			}
			if (num == 1)
			{
				BeginClear();
			}
			else
			{
				DeleteUnusedFiles();
			}
		}
		catch
		{
		}
	}

	internal static void SaveLibrary()
	{
		if (library == null || !IsSupported)
		{
			return;
		}
		try
		{
			lock (Library)
			{
				using FileStream output = new FileStream(LibraryPath, FileMode.Create);
				using BinaryWriter binaryWriter = new BinaryWriter(output);
				binaryWriter.Write(2);
				binaryWriter.Write(NextNameIDX);
				binaryWriter.Write(Library.Count);
				foreach (KeyValuePair<Uri, HTTPCacheFileInfo> item in Library)
				{
					binaryWriter.Write(item.Key.ToString());
					item.Value.SaveTo(binaryWriter);
				}
			}
		}
		catch
		{
		}
	}

	internal static void SetBodyLength(Uri uri, int bodyLength)
	{
		if (!IsSupported)
		{
			return;
		}
		lock (Library)
		{
			if (Library.TryGetValue(uri, out var value))
			{
				value.BodyLength = bodyLength;
				return;
			}
			Library.Add(uri, value = new HTTPCacheFileInfo(uri, DateTime.UtcNow, bodyLength));
			UsedIndexes.Add(value.MappedNameIDX, value);
		}
	}

	private static void DeleteUnusedFiles()
	{
		if (!IsSupported)
		{
			return;
		}
		CheckSetup();
		string[] files = Directory.GetFiles(CacheFolder);
		for (int i = 0; i < files.Length; i++)
		{
			try
			{
				string fileName = Path.GetFileName(files[i]);
				ulong result = 0uL;
				bool flag = false;
				if (ulong.TryParse(fileName, NumberStyles.AllowHexSpecifier, null, out result))
				{
					lock (Library)
					{
						flag = !UsedIndexes.ContainsKey(result);
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					File.Delete(files[i]);
				}
			}
			catch
			{
			}
		}
	}
}
