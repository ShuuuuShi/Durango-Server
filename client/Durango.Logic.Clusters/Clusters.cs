using System;
using System.Collections.Generic;
using System.Linq;
using BestHTTP;
using Durango.Offline;
using Durango.System;
using Durango.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Durango.Logic.Clusters;

public class Clusters
{
	private class ClusterSet
	{
		[JsonProperty(PropertyName = "clusters")]
		public Dictionary<string, Cluster> Clusters;

		[JsonProperty(PropertyName = "maintenance")]
		public Maintenance Maintenance;

		[JsonProperty(PropertyName = "urls")]
		public Dictionary<string, Urls> Urls;

		[JsonProperty(PropertyName = "links")]
		public Dictionary<string, string[]> UrlLinks;

		[JsonProperty(PropertyName = "arena_auth_url")]
		public string ArenaAuthUrl;

		[JsonProperty(PropertyName = "engagement_url")]
		public string EngagementUrl;
	}

	public const string FallbackLocale = "en_US";

	private readonly Dictionary<string, Cluster> _clusters = new Dictionary<string, Cluster>();

	private readonly Dictionary<string, Urls> _urls = new Dictionary<string, Urls>();

	private readonly Dictionary<string, string[]> _urlLinks = new Dictionary<string, string[]>();

	private Maintenance _maintenance;

	private readonly Dictionary<string, Account> _clusterToAccount = new Dictionary<string, Account>();

	public string ArenaAuthUrl { get; private set; }

	public string EngagementUrl { get; private set; }

	public int Count => _clusters.Count;

	public static bool Offline => true;

	public void LoadFromJson(string jsonString)
	{
		ClusterSet clusterSet = Json.Read<ClusterSet>(jsonString);
		if (clusterSet != null)
		{
			_clusters.Clear();
			if (clusterSet.Clusters != null)
			{
				_clusters.AddRange(clusterSet.Clusters);
			}
			_urls.Clear();
			if (clusterSet.Urls != null)
			{
				_urls.AddRange(clusterSet.Urls);
			}
			_urlLinks.Clear();
			if (clusterSet.UrlLinks != null)
			{
				_urlLinks.AddRange(clusterSet.UrlLinks);
			}
			_maintenance = clusterSet.Maintenance;
			ArenaAuthUrl = clusterSet.ArenaAuthUrl;
			EngagementUrl = clusterSet.EngagementUrl;
		}
		switch (Application.platform)
		{
		case RuntimePlatform.IPhonePlayer:
		{
			string text2 = "ios_review_" + CurrentBundleVersion.GetClientVersion();
			if (_clusters.ContainsKey(text2))
			{
				Cluster cluster2 = GetCluster(text2);
				_clusters.Clear();
				_clusters.Add(text2, cluster2);
				return;
			}
			break;
		}
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
		{
			string text = "pc_review_" + CurrentBundleVersion.GetClientVersion();
			if (_clusters.ContainsKey(text))
			{
				Cluster cluster = GetCluster(text);
				_clusters.Clear();
				_clusters.Add(text, cluster);
				return;
			}
			break;
		}
		}
		foreach (KeyValuePair<string, Cluster> cluster3 in _clusters)
		{
			if (cluster3.Key.Contains("ios_review_") || cluster3.Key.Contains("pc_review_"))
			{
				_clusters.Remove(cluster3.Key);
				break;
			}
		}
		if (GameManager.ConnectCluster != null)
		{
			string clusterKey = GameManager.ClusterKey;
			_clusters.Clear();
			_clusters.Add(clusterKey, GameManager.ConnectCluster);
			GameManager.SetCluster(clusterKey, GameManager.ConnectCluster.GatewayUrlRoot, GameManager.ConnectCluster.Mode);
		}
		else
		{
			Server[] array = Servers.GetServers(_clusters).ToArray();
			if (Offline)
			{
				_clusters.Clear();
			}
			Server[] array2 = array;
			foreach (Server server in array2)
			{
				_clusters.Add(server.Key + "_offline", server.Cluster);
			}
		}
		bool flag = false;
		string country = Platform.Instance.CountryLetterCode;
		foreach (KeyValuePair<string, Cluster> cluster4 in _clusters)
		{
			string[] countries = cluster4.Value.Countries;
			if (countries != null && countries.Any((string x) => x.Equals(country, StringComparison.OrdinalIgnoreCase)))
			{
				cluster4.Value.IsRecommendable = true;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			KeyValuePair<string, Cluster> keyValuePair = _clusters.FirstOrDefault();
			if (keyValuePair.Value != null)
			{
				keyValuePair.Value.IsRecommendable = true;
			}
		}
	}

	public void ForceSetCluster(string gateway)
	{
		_clusters.Clear();
		Cluster cluster = new Cluster();
		cluster.GatewayUrlRoot = gateway;
		cluster.Names = new Dictionary<string, string> { { "en_US", "custom" } };
		_clusters.Add("custom", cluster);
	}

	public void GetOrRequestAccounts(string targetCluster, Action<Account> callback, bool forceUpdate = false)
	{
		Account account2 = _clusterToAccount.Get(targetCluster);
		if (account2 != null && !forceUpdate)
		{
			callback(account2);
			return;
		}
		Cluster cluster = GetCluster(targetCluster);
		if (cluster.OnRequestAccount != null)
		{
			cluster.OnRequestAccount(delegate(Account account)
			{
				_clusterToAccount.Remove(targetCluster);
				if (account != null)
				{
					_clusterToAccount[targetCluster] = account;
				}
				if (callback != null)
				{
					callback(account);
				}
			});
			return;
		}
		RequestAccounts(cluster.GatewayUrlRoot, delegate(Account account)
		{
			_clusterToAccount.Remove(targetCluster);
			if (account != null)
			{
				_clusterToAccount[targetCluster] = account;
			}
			if (callback != null)
			{
				callback(account);
			}
		});
	}

	public static void RequestAccounts(string gatewayUrl, Action<Account> callback)
	{
		string capturedNpsn = Platform.Instance.NPSN;
		string text = gatewayUrl + "/accounts";
		string url = text;
		Action<byte[], HTTPResponse> callback2 = delegate(byte[] result, HTTPResponse response)
		{
			if (!(capturedNpsn != Platform.Instance.NPSN))
			{
				Account obj = Json.Read<Account>(result);
				if (callback != null)
				{
					callback(obj);
				}
			}
		};
		bool disableCache = true;
		Dictionary<string, string> fields = Platform.Instance.BuildSessionForm();
		Http.Request(url, callback2, disableCache, addSession: false, fields, HTTPMethods.Post);
	}

	public int GetPlayerCount(string clusterKey)
	{
		Account account = _clusterToAccount.Get(clusterKey);
		return (account == null || account.Players == null) ? (-1) : account.Players.Count;
	}

	[NotNull]
	public Cluster GetCluster(string clusterKey)
	{
		Cluster cluster = _clusters.Get(clusterKey);
		return (cluster == null) ? Cluster.Null : cluster;
	}

	[CanBeNull]
	public Account GetAccount(string clusterKey)
	{
		return _clusterToAccount.Get(clusterKey);
	}

	public string GetRecommendableCluster()
	{
		foreach (KeyValuePair<string, Cluster> cluster in _clusters)
		{
			if (cluster.Value.IsRecommendable)
			{
				return cluster.Key;
			}
		}
		return _clusters.FirstOrDefault().Key;
	}

	public string[] GetClusterKeys()
	{
		return _clusters.Keys.ToArray();
	}

	public IList<Urls> GetOutlinks()
	{
		List<Urls> list = new List<Urls>();
		string text = LocalizeSystem.Locale;
		if (string.IsNullOrEmpty(text))
		{
			return list;
		}
		if (!_urlLinks.ContainsKey(text))
		{
			text = "en_US";
		}
		string[] array = _urlLinks.Get(text);
		if (array == null)
		{
			return list;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (_urls.TryGetValue(array[i], out var value))
			{
				list.Add(value);
			}
		}
		return list;
	}

	public bool IsInMaintenance()
	{
		return _maintenance != null && _maintenance.IsInMaintenance();
	}

	public string GetMaintenanceText([CanBeNull] string locale)
	{
		return (_maintenance == null) ? string.Empty : _maintenance.GetMaintenanceText(locale, em: true);
	}

	public void Clear()
	{
		_clusterToAccount.Clear();
	}
}
