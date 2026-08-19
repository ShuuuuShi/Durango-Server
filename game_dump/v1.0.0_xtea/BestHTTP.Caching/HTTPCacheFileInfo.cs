using System;
using System.Collections.Generic;
using System.IO;
using BestHTTP.Extensions;

namespace BestHTTP.Caching;

internal class HTTPCacheFileInfo : IComparable<HTTPCacheFileInfo>
{
	internal Uri Uri { get; set; }

	internal DateTime LastAccess { get; set; }

	internal int BodyLength { get; set; }

	private string ETag { get; set; }

	private string LastModified { get; set; }

	private DateTime Expires { get; set; }

	private long Age { get; set; }

	private long MaxAge { get; set; }

	private DateTime Date { get; set; }

	private bool MustRevalidate { get; set; }

	private DateTime Received { get; set; }

	private string ConstructedPath { get; set; }

	internal ulong MappedNameIDX { get; set; }

	internal HTTPCacheFileInfo(Uri uri)
		: this(uri, DateTime.UtcNow, -1)
	{
	}

	internal HTTPCacheFileInfo(Uri uri, DateTime lastAcces, int bodyLength)
	{
		Uri = uri;
		LastAccess = lastAcces;
		BodyLength = bodyLength;
		MaxAge = -1L;
		MappedNameIDX = HTTPCacheService.GetNameIdx();
	}

	internal HTTPCacheFileInfo(Uri uri, BinaryReader reader, int version)
	{
		Uri = uri;
		LastAccess = DateTime.FromBinary(reader.ReadInt64());
		BodyLength = reader.ReadInt32();
		switch (version)
		{
		default:
			return;
		case 2:
			MappedNameIDX = reader.ReadUInt64();
			break;
		case 1:
			break;
		}
		ETag = reader.ReadString();
		LastModified = reader.ReadString();
		Expires = DateTime.FromBinary(reader.ReadInt64());
		Age = reader.ReadInt64();
		MaxAge = reader.ReadInt64();
		Date = DateTime.FromBinary(reader.ReadInt64());
		MustRevalidate = reader.ReadBoolean();
		Received = DateTime.FromBinary(reader.ReadInt64());
	}

	internal void SaveTo(BinaryWriter writer)
	{
		writer.Write(LastAccess.ToBinary());
		writer.Write(BodyLength);
		writer.Write(MappedNameIDX);
		writer.Write(ETag);
		writer.Write(LastModified);
		writer.Write(Expires.ToBinary());
		writer.Write(Age);
		writer.Write(MaxAge);
		writer.Write(Date.ToBinary());
		writer.Write(MustRevalidate);
		writer.Write(Received.ToBinary());
	}

	private string GetPath()
	{
		if (ConstructedPath != null)
		{
			return ConstructedPath;
		}
		return ConstructedPath = Path.Combine(HTTPCacheService.CacheFolder, MappedNameIDX.ToString("X"));
	}

	internal bool IsExists()
	{
		if (!HTTPCacheService.IsSupported)
		{
			return false;
		}
		return File.Exists(GetPath());
	}

	internal void Delete()
	{
		if (!HTTPCacheService.IsSupported)
		{
			return;
		}
		string path = GetPath();
		try
		{
			File.Delete(path);
		}
		catch
		{
		}
		finally
		{
			Reset();
		}
	}

	private void Reset()
	{
		MappedNameIDX = 0uL;
		BodyLength = -1;
		ETag = string.Empty;
		Expires = DateTime.FromBinary(0L);
		LastModified = string.Empty;
		Age = 0L;
		MaxAge = -1L;
		Date = DateTime.FromBinary(0L);
		MustRevalidate = false;
		Received = DateTime.FromBinary(0L);
	}

	private void SetUpCachingValues(HTTPResponse response)
	{
		ETag = response.GetFirstHeaderValue("ETag").ToStrOrEmpty();
		Expires = response.GetFirstHeaderValue("Expires").ToDateTime(DateTime.FromBinary(0L));
		LastModified = response.GetFirstHeaderValue("Last-Modified").ToStrOrEmpty();
		Age = response.GetFirstHeaderValue("Age").ToInt64(0L);
		Date = response.GetFirstHeaderValue("Date").ToDateTime(DateTime.FromBinary(0L));
		string firstHeaderValue = response.GetFirstHeaderValue("cache-control");
		if (!string.IsNullOrEmpty(firstHeaderValue))
		{
			string[] array = firstHeaderValue.FindOption("Max-Age");
			if (array != null && double.TryParse(array[1], out var result))
			{
				MaxAge = (int)result;
			}
			MustRevalidate = firstHeaderValue.ToLower().Contains("must-revalidate");
		}
		Received = DateTime.UtcNow;
	}

	internal bool WillExpireInTheFuture()
	{
		if (!IsExists())
		{
			return false;
		}
		if (MustRevalidate)
		{
			return false;
		}
		if (MaxAge != -1)
		{
			long val = Math.Max(0L, (long)(Received - Date).TotalSeconds);
			long num = Math.Max(val, Age);
			long num2 = (long)(DateTime.UtcNow - Date).TotalSeconds;
			long num3 = num + num2;
			return num3 < MaxAge;
		}
		return Expires > DateTime.UtcNow;
	}

	internal void SetUpRevalidationHeaders(HTTPRequest request)
	{
		if (IsExists())
		{
			if (!string.IsNullOrEmpty(ETag))
			{
				request.AddHeader("If-None-Match", ETag);
			}
			if (!string.IsNullOrEmpty(LastModified))
			{
				request.AddHeader("If-Modified-Since", LastModified);
			}
		}
	}

	internal Stream GetBodyStream(out int length)
	{
		if (!IsExists())
		{
			length = 0;
			return null;
		}
		length = BodyLength;
		LastAccess = DateTime.UtcNow;
		FileStream fileStream = new FileStream(GetPath(), FileMode.Open);
		fileStream.Seek(-length, SeekOrigin.End);
		return fileStream;
	}

	internal HTTPResponse ReadResponseTo(HTTPRequest request)
	{
		if (!IsExists())
		{
			return null;
		}
		LastAccess = DateTime.UtcNow;
		using FileStream stream = new FileStream(GetPath(), FileMode.Open);
		HTTPResponse hTTPResponse = new HTTPResponse(request, stream, request.UseStreaming, isFromCache: true);
		hTTPResponse.Receive(BodyLength);
		return hTTPResponse;
	}

	internal void Store(HTTPResponse response)
	{
		if (!HTTPCacheService.IsSupported)
		{
			return;
		}
		string path = GetPath();
		if (path.Length > HTTPManager.MaxPathLength)
		{
			return;
		}
		if (File.Exists(path))
		{
			Delete();
		}
		using (FileStream fileStream = new FileStream(path, FileMode.Create))
		{
			fileStream.WriteLine("HTTP/1.1 {0} {1}", response.StatusCode, response.Message);
			foreach (KeyValuePair<string, List<string>> header in response.Headers)
			{
				for (int i = 0; i < header.Value.Count; i++)
				{
					fileStream.WriteLine("{0}: {1}", header.Key, header.Value[i]);
				}
			}
			fileStream.WriteLine();
			fileStream.Write(response.Data, 0, response.Data.Length);
		}
		BodyLength = response.Data.Length;
		LastAccess = DateTime.UtcNow;
		SetUpCachingValues(response);
	}

	internal Stream GetSaveStream(HTTPResponse response)
	{
		if (!HTTPCacheService.IsSupported)
		{
			return null;
		}
		LastAccess = DateTime.UtcNow;
		string path = GetPath();
		if (File.Exists(path))
		{
			Delete();
		}
		if (path.Length > HTTPManager.MaxPathLength)
		{
			return null;
		}
		using (FileStream fs = new FileStream(path, FileMode.Create))
		{
			fs.WriteLine("HTTP/1.1 {0} {1}", response.StatusCode, response.Message);
			foreach (KeyValuePair<string, List<string>> header in response.Headers)
			{
				for (int i = 0; i < header.Value.Count; i++)
				{
					fs.WriteLine("{0}: {1}", header.Key, header.Value[i]);
				}
			}
			fs.WriteLine();
		}
		if (response.IsFromCache && !response.Headers.ContainsKey("content-length"))
		{
			response.Headers.Add("content-length", new List<string> { BodyLength.ToString() });
		}
		SetUpCachingValues(response);
		return new FileStream(GetPath(), FileMode.Append);
	}

	public int CompareTo(HTTPCacheFileInfo other)
	{
		return LastAccess.CompareTo(other.LastAccess);
	}
}
