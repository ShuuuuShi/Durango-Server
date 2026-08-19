using System;
using System.Collections.Generic;
using BestHTTP;
using BestHTTP.Forms;

namespace Durango.Utils;

public static class Http
{
	public static HTTPRequest Request(string url, Action<byte[], HTTPResponse> callback, bool disableCache = false, bool addSession = false, Dictionary<string, string> fields = null, HTTPMethods method = HTTPMethods.Get)
	{
		if (GameManager.IsSceneClosing)
		{
			return null;
		}
		HTTPRequest hTTPRequest = new HTTPRequest(new Uri(url), method, delegate(HTTPRequest originalRequest, HTTPResponse response)
		{
			bool isCached;
			byte[] arg = ProcessResult(originalRequest, out isCached);
			if (callback != null)
			{
				callback(arg, response);
			}
		});
		if (GameManager.ClusterMode != 0)
		{
			hTTPRequest.FormUsage = HTTPFormUsage.UrlEncoded;
		}
		hTTPRequest.AddHeader("Accept-Encoding", "gzip");
		hTTPRequest.AddHeader("Accept", "application/json");
		hTTPRequest.AddHeader("Accept-Language", LocalizeSystem.Locale);
		hTTPRequest.AddHeader("X-K1-System-Language", LocalizeSystem.SystemLanguage);
		hTTPRequest.AddHeader("cache-control", "max-age=0");
		if (addSession)
		{
			hTTPRequest.AddHeader("Authorization", GameManager.SessionToken);
		}
		if (fields != null)
		{
			foreach (KeyValuePair<string, string> field in fields)
			{
				if (field.Value != null)
				{
					hTTPRequest.AddField(field.Key, field.Value);
				}
			}
		}
		hTTPRequest.DisableCache = disableCache;
		hTTPRequest.Send();
		return hTTPRequest;
	}

	public static HTTPRequest RequestYml<T>(string url, Action<T> callback, bool disableCache = false)
	{
		return Request(url, delegate(byte[] bytes, HTTPResponse response)
		{
			if (callback != null)
			{
				callback((bytes == null) ? default(T) : Json.Read<T>(bytes));
			}
		}, disableCache);
	}

	public static byte[] ProcessResult(HTTPRequest request, out bool isCached)
	{
		switch (request.State)
		{
		case HTTPRequestStates.Finished:
			if (request.Response.IsSuccess)
			{
				isCached = request.Response.IsFromCache;
				return request.Response.Data;
			}
			break;
		}
		isCached = false;
		return null;
	}
}
