using System;
using System.Collections.Generic;
using BestHTTP.Caching;
using BestHTTP.Extensions;
using BestHTTP.Logger;
using BestHTTP.Statistics;
using UnityEngine;

namespace BestHTTP;

public static class HTTPManager
{
	private static byte maxConnectionPerServer;

	private static HeartbeatManager heartbeats;

	private static ILogger logger;

	private static Dictionary<string, List<ConnectionBase>> Connections;

	private static List<ConnectionBase> ActiveConnections;

	private static List<ConnectionBase> FreeConnections;

	private static List<ConnectionBase> RecycledConnections;

	private static List<HTTPRequest> RequestQueue;

	private static bool IsCallingCallbacks;

	internal static object Locker;

	public static byte MaxConnectionPerServer
	{
		get
		{
			return maxConnectionPerServer;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("MaxConnectionPerServer must be greater than 0!");
			}
			maxConnectionPerServer = value;
		}
	}

	public static bool KeepAliveDefaultValue { get; set; }

	public static bool IsCachingDisabled { get; set; }

	public static TimeSpan MaxConnectionIdleTime { get; set; }

	public static uint CookieJarSize { get; set; }

	public static bool EnablePrivateBrowsing { get; set; }

	public static TimeSpan ConnectTimeout { get; set; }

	public static TimeSpan RequestTimeout { get; set; }

	public static Func<string> RootCacheFolderProvider { get; set; }

	public static HTTPProxy Proxy { get; set; }

	public static HeartbeatManager Heartbeats
	{
		get
		{
			if (heartbeats == null)
			{
				heartbeats = new HeartbeatManager();
			}
			return heartbeats;
		}
	}

	public static ILogger Logger
	{
		get
		{
			if (logger == null)
			{
				logger = new DefaultLogger();
				logger.Level = Loglevels.None;
			}
			return logger;
		}
		set
		{
			logger = value;
		}
	}

	internal static int MaxPathLength { get; set; }

	static HTTPManager()
	{
		Connections = new Dictionary<string, List<ConnectionBase>>();
		ActiveConnections = new List<ConnectionBase>();
		FreeConnections = new List<ConnectionBase>();
		RecycledConnections = new List<ConnectionBase>();
		RequestQueue = new List<HTTPRequest>();
		Locker = new object();
		MaxConnectionPerServer = 4;
		KeepAliveDefaultValue = true;
		MaxPathLength = 255;
		MaxConnectionIdleTime = TimeSpan.FromSeconds(30.0);
		CookieJarSize = 10485760u;
		EnablePrivateBrowsing = false;
		ConnectTimeout = TimeSpan.FromSeconds(20.0);
		RequestTimeout = TimeSpan.FromSeconds(60.0);
		logger = new DefaultLogger();
	}

	public static void Setup()
	{
		HTTPUpdateDelegator.CheckInstance();
		HTTPCacheService.CheckSetup();
	}

	public static HTTPRequest SendRequest(string url, OnRequestFinishedDelegate callback)
	{
		return SendRequest(new HTTPRequest(new Uri(url), HTTPMethods.Get, callback));
	}

	public static HTTPRequest SendRequest(string url, HTTPMethods methodType, OnRequestFinishedDelegate callback)
	{
		return SendRequest(new HTTPRequest(new Uri(url), methodType, callback));
	}

	public static HTTPRequest SendRequest(string url, HTTPMethods methodType, bool isKeepAlive, OnRequestFinishedDelegate callback)
	{
		return SendRequest(new HTTPRequest(new Uri(url), methodType, isKeepAlive, callback));
	}

	public static HTTPRequest SendRequest(string url, HTTPMethods methodType, bool isKeepAlive, bool disableCache, OnRequestFinishedDelegate callback)
	{
		return SendRequest(new HTTPRequest(new Uri(url), methodType, isKeepAlive, disableCache, callback));
	}

	public static HTTPRequest SendRequest(HTTPRequest request)
	{
		lock (Locker)
		{
			Setup();
			if (IsCallingCallbacks)
			{
				request.State = HTTPRequestStates.Queued;
				RequestQueue.Add(request);
			}
			else
			{
				SendRequestImpl(request);
			}
			return request;
		}
	}

	public static GeneralStatistics GetGeneralStatistics(StatisticsQueryFlags queryFlags)
	{
		GeneralStatistics result = default(GeneralStatistics);
		result.QueryFlags = queryFlags;
		if ((queryFlags & StatisticsQueryFlags.Connections) != 0)
		{
			int num = 0;
			foreach (KeyValuePair<string, List<ConnectionBase>> connection in Connections)
			{
				if (connection.Value != null)
				{
					num += connection.Value.Count;
				}
			}
			result.Connections = num;
			result.ActiveConnections = ActiveConnections.Count;
			result.FreeConnections = FreeConnections.Count;
			result.RecycledConnections = RecycledConnections.Count;
			result.RequestsInQueue = RequestQueue.Count;
		}
		if ((queryFlags & StatisticsQueryFlags.Cache) != 0)
		{
			result.CacheEntityCount = HTTPCacheService.GetCacheEntityCount();
			result.CacheSize = HTTPCacheService.GetCacheSize();
		}
		return result;
	}

	private static void SendRequestImpl(HTTPRequest request)
	{
		ConnectionBase conn = FindOrCreateFreeConnection(request);
		if (conn != null)
		{
			if (ActiveConnections.Find((ConnectionBase c) => c == conn) == null)
			{
				ActiveConnections.Add(conn);
			}
			FreeConnections.Remove(conn);
			request.State = HTTPRequestStates.Processing;
			request.Prepare();
			conn.Process(request);
		}
		else
		{
			request.State = HTTPRequestStates.Queued;
			RequestQueue.Add(request);
		}
	}

	private static string GetKeyForRequest(HTTPRequest request)
	{
		if (request.CurrentUri.IsFile)
		{
			return request.CurrentUri.ToString();
		}
		return ((request.Proxy == null) ? string.Empty : new UriBuilder(request.Proxy.Address.Scheme, request.Proxy.Address.Host, request.Proxy.Address.Port).Uri.ToString()) + new UriBuilder(request.CurrentUri.Scheme, request.CurrentUri.Host, request.CurrentUri.Port).Uri.ToString();
	}

	private static ConnectionBase CreateConnection(HTTPRequest request, string serverUrl)
	{
		if (request.CurrentUri.IsFile)
		{
			return new FileConnection(serverUrl);
		}
		return new HTTPConnection(serverUrl);
	}

	private static ConnectionBase FindOrCreateFreeConnection(HTTPRequest request)
	{
		ConnectionBase connectionBase = null;
		string keyForRequest = GetKeyForRequest(request);
		if (Connections.TryGetValue(keyForRequest, out var value))
		{
			int num = 0;
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].IsActive)
				{
					num++;
				}
			}
			if (num <= MaxConnectionPerServer)
			{
				for (int j = 0; j < value.Count; j++)
				{
					if (connectionBase != null)
					{
						break;
					}
					ConnectionBase connectionBase2 = value[j];
					if (connectionBase2 != null && connectionBase2.IsFree && (!connectionBase2.HasProxy || connectionBase2.LastProcessedUri == null || connectionBase2.LastProcessedUri.Host.Equals(request.CurrentUri.Host, StringComparison.OrdinalIgnoreCase)))
					{
						connectionBase = connectionBase2;
					}
				}
			}
		}
		else
		{
			Connections.Add(keyForRequest, value = new List<ConnectionBase>(MaxConnectionPerServer));
		}
		if (connectionBase == null)
		{
			if (value.Count >= MaxConnectionPerServer)
			{
				return null;
			}
			value.Add(connectionBase = CreateConnection(request, keyForRequest));
		}
		return connectionBase;
	}

	private static bool CanProcessFromQueue()
	{
		for (int i = 0; i < RequestQueue.Count; i++)
		{
			if (FindOrCreateFreeConnection(RequestQueue[i]) != null)
			{
				return true;
			}
		}
		return false;
	}

	private static void RecycleConnection(ConnectionBase conn)
	{
		conn.Recycle(OnConnectionRecylced);
	}

	private static void OnConnectionRecylced(ConnectionBase conn)
	{
		lock (RecycledConnections)
		{
			RecycledConnections.Add(conn);
		}
	}

	internal static ConnectionBase GetConnectionWith(HTTPRequest request)
	{
		lock (Locker)
		{
			for (int i = 0; i < ActiveConnections.Count; i++)
			{
				ConnectionBase connectionBase = ActiveConnections[i];
				if (connectionBase.CurrentRequest == request)
				{
					return connectionBase;
				}
			}
			return null;
		}
	}

	internal static bool RemoveFromQueue(HTTPRequest request)
	{
		return RequestQueue.Remove(request);
	}

	internal static string GetRootCacheFolder()
	{
		try
		{
			if (RootCacheFolderProvider != null)
			{
				return RootCacheFolderProvider();
			}
		}
		catch (Exception ex)
		{
			Logger.Exception("HTTPManager", "GetRootCacheFolder", ex);
		}
		return Application.persistentDataPath;
	}

	public static void OnUpdate()
	{
		lock (Locker)
		{
			IsCallingCallbacks = true;
			try
			{
				for (int i = 0; i < ActiveConnections.Count; i++)
				{
					ConnectionBase connectionBase = ActiveConnections[i];
					switch (connectionBase.State)
					{
					case HTTPConnectionStates.Processing:
						connectionBase.HandleProgressCallback();
						if (connectionBase.CurrentRequest.UseStreaming && connectionBase.CurrentRequest.Response != null && connectionBase.CurrentRequest.Response.HasStreamedFragments())
						{
							connectionBase.HandleCallback();
						}
						if (((!connectionBase.CurrentRequest.UseStreaming && connectionBase.CurrentRequest.UploadStream == null) || connectionBase.CurrentRequest.EnableTimoutForStreaming) && DateTime.UtcNow - connectionBase.StartTime > connectionBase.CurrentRequest.Timeout)
						{
							connectionBase.Abort(HTTPConnectionStates.TimedOut);
						}
						break;
					case HTTPConnectionStates.TimedOut:
						if (DateTime.UtcNow - connectionBase.TimedOutStart > TimeSpan.FromMilliseconds(500.0))
						{
							Logger.Information("HTTPManager", "Hard aborting connection becouse of a long waiting TimedOut state");
							connectionBase.CurrentRequest.Response = null;
							connectionBase.CurrentRequest.State = HTTPRequestStates.TimedOut;
							connectionBase.HandleCallback();
							RecycleConnection(connectionBase);
						}
						break;
					case HTTPConnectionStates.Redirected:
						SendRequest(connectionBase.CurrentRequest);
						RecycleConnection(connectionBase);
						break;
					case HTTPConnectionStates.WaitForRecycle:
						connectionBase.CurrentRequest.FinishStreaming();
						connectionBase.HandleCallback();
						RecycleConnection(connectionBase);
						break;
					case HTTPConnectionStates.Upgraded:
						connectionBase.HandleCallback();
						break;
					case HTTPConnectionStates.WaitForProtocolShutdown:
					{
						IProtocol protocol2 = connectionBase.CurrentRequest.Response as IProtocol;
						protocol2?.HandleEvents();
						if (protocol2 == null || protocol2.IsClosed)
						{
							connectionBase.HandleCallback();
							connectionBase.Dispose();
							RecycleConnection(connectionBase);
						}
						break;
					}
					case HTTPConnectionStates.AbortRequested:
						if (connectionBase.CurrentRequest.Response is IProtocol protocol)
						{
							protocol.HandleEvents();
							if (protocol.IsClosed)
							{
								connectionBase.HandleCallback();
								connectionBase.Dispose();
								RecycleConnection(connectionBase);
							}
						}
						break;
					case HTTPConnectionStates.Closed:
						connectionBase.CurrentRequest.FinishStreaming();
						connectionBase.HandleCallback();
						RecycleConnection(connectionBase);
						break;
					case HTTPConnectionStates.Free:
						RecycleConnection(connectionBase);
						break;
					}
				}
			}
			finally
			{
				IsCallingCallbacks = false;
			}
			lock (RecycledConnections)
			{
				if (RecycledConnections.Count > 0)
				{
					for (int j = 0; j < RecycledConnections.Count; j++)
					{
						ConnectionBase connectionBase2 = RecycledConnections[j];
						if (connectionBase2.IsFree)
						{
							ActiveConnections.Remove(connectionBase2);
							FreeConnections.Add(connectionBase2);
						}
					}
					RecycledConnections.Clear();
				}
			}
			if (FreeConnections.Count > 0)
			{
				for (int k = 0; k < FreeConnections.Count; k++)
				{
					ConnectionBase connectionBase3 = FreeConnections[k];
					if (connectionBase3.IsRemovable)
					{
						List<ConnectionBase> value = null;
						if (Connections.TryGetValue(connectionBase3.ServerAddress, out value))
						{
							value.Remove(connectionBase3);
						}
						connectionBase3.Dispose();
						FreeConnections.RemoveAt(k);
						k--;
					}
				}
			}
			if (CanProcessFromQueue())
			{
				if (RequestQueue.Find((HTTPRequest req) => req.Priority != 0) != null)
				{
					RequestQueue.Sort((HTTPRequest req1, HTTPRequest req2) => req1.Priority - req2.Priority);
				}
				HTTPRequest[] array = RequestQueue.ToArray();
				RequestQueue.Clear();
				for (int l = 0; l < array.Length; l++)
				{
					SendRequest(array[l]);
				}
			}
		}
		if (heartbeats != null)
		{
			heartbeats.Update();
		}
	}

	public static void OnQuit()
	{
		lock (Locker)
		{
			HTTPCacheService.SaveLibrary();
			HTTPRequest[] array = RequestQueue.ToArray();
			RequestQueue.Clear();
			HTTPRequest[] array2 = array;
			foreach (HTTPRequest hTTPRequest in array2)
			{
				hTTPRequest.Abort();
			}
			foreach (KeyValuePair<string, List<ConnectionBase>> connection in Connections)
			{
				foreach (ConnectionBase item in connection.Value)
				{
					item.Abort(HTTPConnectionStates.Closed);
					item.Dispose();
				}
				connection.Value.Clear();
			}
			Connections.Clear();
			OnUpdate();
		}
	}
}
