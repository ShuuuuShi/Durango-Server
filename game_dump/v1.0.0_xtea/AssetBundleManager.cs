using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundleManager : KSingleton<AssetBundleManager>
{
	public enum Status
	{
		None,
		LoadInfo,
		LoadPreload,
		LoadPreCachedFiles,
		Reconnect,
		Succeed,
		Failed
	}

	private class AssetBundleItemToLoad
	{
		public int RetryCount;

		public AssetBundleItem Item;

		public Action<Object> Finished;

		public Type ReqType;
	}

	private enum LoadStatus
	{
		Failed = -1,
		Wait,
		Complete
	}

	public static int PrerequsitePriority = 500;

	private static readonly WaitForSeconds InternetCheckWaitTime = new WaitForSeconds(2f);

	[SerializeField]
	private string _infoHolderName;

	[SerializeField]
	private int _maxRetryCount = 1;

	private Hash128 _preloadHash;

	private readonly Dictionary<string, AssetBundleItem> _assetBundleItemDict = new Dictionary<string, AssetBundleItem>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, AssetBundleFile> _assetBundleFileDict = new Dictionary<string, AssetBundleFile>(StringComparer.OrdinalIgnoreCase);

	private readonly List<AssetBundleItemToLoad> _itemToLoad = new List<AssetBundleItemToLoad>();

	private readonly List<AssetBundleFile> _prerequisites = new List<AssetBundleFile>();

	private bool _isBackgroundDownloading;

	private AssetBundle _preloadBundle;

	private readonly string[] _preCachedFiles = new string[1] { "particle" };

	public string AssetBundleLoadPath { get; private set; }

	public static bool UseBundle => true;

	public Status CurrentStatus { get; private set; }

	public int TotalFileCount => _assetBundleFileDict.Count;

	public int PrerequsitesCount => _prerequisites.Count;

	protected override bool CheckDontDestroyOnLoad()
	{
		return true;
	}

	public void Initialize(string infoHolderPath)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Invalid comparison between Unknown and I4
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Invalid comparison between Unknown and I4
		if (UseBundle)
		{
			switch (CurrentStatus)
			{
			case Status.LoadPreCachedFiles:
				((MonoBehaviour)this).StartCoroutine(CoLoadPreCachedFiles());
				break;
			case Status.LoadInfo:
			case Status.LoadPreload:
			case Status.Succeed:
				break;
			case Status.None:
			case Status.Reconnect:
			case Status.Failed:
			{
				bool flag = string.IsNullOrEmpty(infoHolderPath) || !string.IsNullOrEmpty(_infoHolderName);
				if (flag)
				{
					if ((int)Application.platform == 11)
					{
						AssetBundleLoadPath = "jar:file://" + Application.dataPath + "!/assets/";
					}
					else if ((int)Application.platform == 8)
					{
						AssetBundleLoadPath = "file://" + Application.dataPath + "/Raw/";
					}
					else
					{
						AssetBundleLoadPath = "file://" + Application.dataPath + "/StreamingAssets/";
					}
					infoHolderPath = AssetBundleLoadPath + _infoHolderName + ".json";
				}
				else
				{
					AssetBundleLoadPath = infoHolderPath.Substring(0, infoHolderPath.LastIndexOf("/", StringComparison.Ordinal) + 1);
				}
				((MonoBehaviour)this).StartCoroutine(CoLoadAssetBundleInfoHolder(infoHolderPath, CurrentStatus == Status.Reconnect, flag));
				break;
			}
			}
		}
		else
		{
			CurrentStatus = Status.Succeed;
		}
	}

	private void Update()
	{
		if (CurrentStatus != Status.Succeed)
		{
			return;
		}
		for (int num = _itemToLoad.Count - 1; num >= 0; num--)
		{
			AssetBundleItemToLoad assetBundleItemToLoad = _itemToLoad[num];
			AssetBundleItem item = _itemToLoad[num].Item;
			switch (TryLoadBundleFile(item.Parent))
			{
			case LoadStatus.Failed:
				assetBundleItemToLoad.RetryCount++;
				if (assetBundleItemToLoad.RetryCount > _maxRetryCount)
				{
					_itemToLoad.RemoveAt(num);
					assetBundleItemToLoad.Finished(null);
					assetBundleItemToLoad.Finished = null;
				}
				break;
			case LoadStatus.Complete:
				if ((Object)(object)item.Parent.Bundle != (Object)null)
				{
					if (item.Request == null)
					{
						item.Request = item.Parent.Bundle.LoadAssetAsync(item.Name, assetBundleItemToLoad.ReqType);
					}
					if (((AsyncOperation)item.Request).isDone)
					{
						_itemToLoad.RemoveAt(num);
						assetBundleItemToLoad.Finished(item.Request.asset);
						assetBundleItemToLoad.Finished = null;
					}
				}
				break;
			}
		}
	}

	private IEnumerator CoLoadAssetBundleInfoHolder(string infoHolderPath, bool skipIfCached, bool useLocal)
	{
		CurrentStatus = Status.LoadInfo;
		bool isCached = false;
		byte[] bytes;
		if (useLocal)
		{
			WWW www = new WWW(infoHolderPath);
			try
			{
				yield return www;
				if (www.error != null)
				{
					CurrentStatus = Status.Failed;
					Debug.LogError((object)www.error);
					yield break;
				}
				bytes = www.bytes;
			}
			finally
			{
				((IDisposable)www)?.Dispose();
			}
		}
		else
		{
			HTTPRequest request = KUtility.RequestUrl(infoHolderPath, null);
			yield return ((MonoBehaviour)this).StartCoroutine((IEnumerator)request);
			bytes = KUtility.ProcessResult(request, out isCached);
		}
		if (bytes == null)
		{
			CurrentStatus = Status.Failed;
			yield break;
		}
		if (skipIfCached && isCached)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(CoLoadPreCachedFiles());
			yield break;
		}
		AssetBundleInfoHolder holder = KUtility.ParseJson<AssetBundleInfoHolder>(bytes);
		if (holder == null)
		{
			CurrentStatus = Status.Failed;
			yield break;
		}
		_preloadHash = Hash128.Parse(holder.PreloadHash);
		LoadAssetBundeFiles(holder);
		LoadAssetBundleItems(holder);
		yield return ((MonoBehaviour)this).StartCoroutine(CoLoadPreloadFile());
	}

	private static bool IsPrerequsite(AssetBundleFile file)
	{
		return file.Priority > PrerequsitePriority;
	}

	private void LoadAssetBundeFiles(AssetBundleInfoHolder holder)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		_assetBundleFileDict.Clear();
		_prerequisites.Clear();
		for (int i = 0; i < holder.FileList.Count; i++)
		{
			AssetBundleFileInfo assetBundleFileInfo = holder.FileList[i];
			AssetBundleFile assetBundleFile = new AssetBundleFile();
			assetBundleFile.Hash = Hash128.Parse(assetBundleFileInfo.Hash);
			assetBundleFile.Priority = assetBundleFileInfo.Priority;
			assetBundleFile.Name = assetBundleFileInfo.Name;
			assetBundleFile.CurrentStatus = AssetBundleFile.Status.None;
			_assetBundleFileDict.Add(assetBundleFile.Name, assetBundleFile);
			if (IsPrerequsite(assetBundleFile))
			{
				_prerequisites.Add(assetBundleFile);
			}
		}
		LoadDependencies(holder);
	}

	private void LoadDependencies(AssetBundleInfoHolder holder)
	{
		for (int i = 0; i < holder.FileList.Count; i++)
		{
			AssetBundleFileInfo assetBundleFileInfo = holder.FileList[i];
			if (assetBundleFileInfo.Dependencies == null)
			{
				continue;
			}
			AssetBundleFile assetBundleFile = _assetBundleFileDict[assetBundleFileInfo.Name];
			assetBundleFile.Dependencies = new AssetBundleFile[assetBundleFileInfo.Dependencies.Length];
			for (int j = 0; j < assetBundleFileInfo.Dependencies.Length; j++)
			{
				string text = assetBundleFileInfo.Dependencies[j];
				if (!_assetBundleFileDict.TryGetValue(text, out assetBundleFile.Dependencies[j]))
				{
					Debug.LogError((object)("Cannot find dependency - " + text));
				}
			}
		}
	}

	private void LoadAssetBundleItems(AssetBundleInfoHolder holder)
	{
		_assetBundleItemDict.Clear();
		for (int i = 0; i < holder.ItemList.Count; i++)
		{
			AssetBundleItemInfo assetBundleItemInfo = holder.ItemList[i];
			AssetBundleItem assetBundleItem = new AssetBundleItem();
			assetBundleItem.Name = AssetBundleItemInfo.GetAssetName(assetBundleItemInfo.Name);
			_assetBundleItemDict.Add(assetBundleItemInfo.Name, assetBundleItem);
			string key = ((!assetBundleItemInfo.SavePerDirectory) ? assetBundleItemInfo.Name : AssetBundleItemInfo.GetParentName(assetBundleItemInfo.Name));
			AssetBundleFile parent = _assetBundleFileDict[key];
			assetBundleItem.Parent = parent;
		}
	}

	private IEnumerator CoLoadPreloadFile()
	{
		CurrentStatus = Status.LoadPreload;
		if ((Object)(object)_preloadBundle != (Object)null)
		{
			_preloadBundle.Unload(false);
			_preloadBundle = null;
		}
		UnityWebRequest request = UnityWebRequest.GetAssetBundle(CreateTargetUrl("preload.bundle", _preloadHash), _preloadHash, 0u);
		try
		{
			yield return request.Send();
			DownloadHandler downloadHandler = request.downloadHandler;
			DownloadHandlerAssetBundle bundle = (DownloadHandlerAssetBundle)(object)((downloadHandler is DownloadHandlerAssetBundle) ? downloadHandler : null);
			if (request.isError || bundle == null)
			{
				Debug.LogError((object)((!request.isError) ? "DownloadHandlerAssetBundle is null" : request.error));
				CurrentStatus = Status.Failed;
				yield break;
			}
			_preloadBundle = bundle.assetBundle;
		}
		finally
		{
			((IDisposable)request)?.Dispose();
		}
		CurrentStatus = Status.LoadPreCachedFiles;
		yield return ((MonoBehaviour)this).StartCoroutine(CoLoadPreCachedFiles());
	}

	private string CreateTargetUrl(string fileName, Hash128 hash)
	{
		string hashedName = AssetBundleItemInfo.GetHashedName(fileName, ((Hash128)(ref hash)).ToString());
		return $"{AssetBundleLoadPath}{hashedName}";
	}

	private IEnumerator CoLoadPreCachedFiles()
	{
		CurrentStatus = Status.LoadPreCachedFiles;
		for (int i = 0; i < _preCachedFiles.Length; i++)
		{
			AssetBundleFile file = DictionaryExtensions.Get(key: AssetBundleItemInfo.GetBundleFileName(_preCachedFiles[i]), dict: _assetBundleFileDict);
			if (file == null)
			{
				CurrentStatus = Status.Failed;
				yield break;
			}
			while (true)
			{
				switch (TryLoadBundleFile(file))
				{
				case LoadStatus.Failed:
					CurrentStatus = Status.Failed;
					yield break;
				default:
					yield return null;
					continue;
				case LoadStatus.Complete:
					break;
				}
				break;
			}
		}
		CurrentStatus = Status.Succeed;
	}

	private IEnumerator CoLoadFile(AssetBundleFile file, Action<float> progressCallback = null, bool isPreload = false)
	{
		if (file.CurrentStatus == AssetBundleFile.Status.Loading)
		{
			yield break;
		}
		file.CurrentStatus = AssetBundleFile.Status.Loading;
		string url = CreateTargetUrl(file.Name, file.Hash);
		UnityWebRequest request = UnityWebRequest.GetAssetBundle(url, file.Hash, 0u);
		try
		{
			request.Send();
			while (!request.isDone)
			{
				yield return null;
				progressCallback?.Invoke(request.downloadProgress);
			}
			DownloadHandler downloadHandler = request.downloadHandler;
			DownloadHandlerAssetBundle bundle = (DownloadHandlerAssetBundle)(object)((downloadHandler is DownloadHandlerAssetBundle) ? downloadHandler : null);
			if (request.isError || bundle == null)
			{
				Debug.LogError((object)((!request.isError) ? "DownloadHandlerAssetBundle is null" : request.error));
				file.CurrentStatus = AssetBundleFile.Status.Failed;
				file.Bundle = null;
				yield break;
			}
			if (isPreload && !file.Queued)
			{
				file.CurrentStatus = AssetBundleFile.Status.None;
				file.Bundle = null;
				yield break;
			}
			if ((Object)(object)file.Bundle != (Object)null)
			{
				file.Bundle.Unload(false);
				file.Bundle = null;
			}
			file.Bundle = bundle.assetBundle;
		}
		finally
		{
			((IDisposable)request)?.Dispose();
		}
	}

	private LoadStatus TryLoadBundleFile(AssetBundleFile file)
	{
		file.Queued = true;
		if (file.Dependencies != null)
		{
			LoadStatus loadStatus = LoadStatus.Complete;
			for (int i = 0; i < file.Dependencies.Length; i++)
			{
				AssetBundleFile file2 = file.Dependencies[i];
				LoadStatus loadStatus2 = TryLoadBundleFile(file2);
				if (loadStatus2 != LoadStatus.Complete && loadStatus != LoadStatus.Failed)
				{
					loadStatus = loadStatus2;
				}
			}
			if (loadStatus != LoadStatus.Complete)
			{
				return loadStatus;
			}
		}
		switch (file.CurrentStatus)
		{
		case AssetBundleFile.Status.None:
			((MonoBehaviour)this).StartCoroutine(CoLoadFile(file));
			break;
		default:
			file.CurrentStatus = AssetBundleFile.Status.None;
			return LoadStatus.Failed;
		case AssetBundleFile.Status.Loading:
			break;
		}
		return ((Object)(object)file.Bundle != (Object)null) ? LoadStatus.Complete : LoadStatus.Wait;
	}

	public bool RequestAsset(string assetPath, Type type, Action<Object> callback, bool immediately = false)
	{
		if (string.IsNullOrEmpty(assetPath))
		{
			callback(null);
			return false;
		}
		if (CurrentStatus != Status.Succeed)
		{
			Debug.LogError((object)"RequestAsset() when AssetBundleManager is not available yet!");
			return false;
		}
		string uniqueName = AssetBundleItemInfo.GetUniqueName(assetPath);
		AssetBundleItem assetBundleItem = _assetBundleItemDict.Get(uniqueName);
		if (assetBundleItem == null)
		{
			callback(null);
			return false;
		}
		LoadStatus loadStatus = TryLoadBundleFile(assetBundleItem.Parent);
		if ((Object)(object)assetBundleItem.Parent.Bundle != (Object)null && loadStatus == LoadStatus.Complete)
		{
			if (assetBundleItem.Request == null)
			{
				assetBundleItem.Request = assetBundleItem.Parent.Bundle.LoadAssetAsync(assetBundleItem.Name, type);
			}
			if (((AsyncOperation)assetBundleItem.Request).isDone || immediately)
			{
				callback(assetBundleItem.Request.asset);
				return true;
			}
		}
		AssetBundleItemToLoad assetBundleItemToLoad = null;
		int i = 0;
		for (int count = _itemToLoad.Count; i < count; i++)
		{
			if (_itemToLoad[i].Item == assetBundleItem && (object)_itemToLoad[i].ReqType == type)
			{
				assetBundleItemToLoad = _itemToLoad[i];
				break;
			}
		}
		if (assetBundleItemToLoad == null)
		{
			assetBundleItemToLoad = new AssetBundleItemToLoad();
			assetBundleItemToLoad.Item = assetBundleItem;
			assetBundleItemToLoad.ReqType = type;
			_itemToLoad.Add(assetBundleItemToLoad);
		}
		AssetBundleItemToLoad assetBundleItemToLoad2 = assetBundleItemToLoad;
		assetBundleItemToLoad2.Finished = (Action<Object>)Delegate.Combine(assetBundleItemToLoad2.Finished, callback);
		assetBundleItemToLoad.RetryCount = 0;
		return true;
	}

	public void ClearRequests()
	{
		for (int num = _itemToLoad.Count - 1; num >= 0; num--)
		{
			_itemToLoad[num].Finished(null);
		}
		_itemToLoad.Clear();
	}

	public void ClearAll()
	{
		_itemToLoad.Clear();
		((MonoBehaviour)this).StopAllCoroutines();
		foreach (KeyValuePair<string, AssetBundleItem> item in _assetBundleItemDict)
		{
			item.Value.Request = null;
		}
		foreach (KeyValuePair<string, AssetBundleFile> item2 in _assetBundleFileDict)
		{
			item2.Value.Queued = false;
			if ((Object)(object)item2.Value.Bundle != (Object)null)
			{
				item2.Value.Bundle.Unload(false);
				item2.Value.Bundle = null;
			}
			item2.Value.CurrentStatus = AssetBundleFile.Status.None;
		}
		CurrentStatus = ((CurrentStatus == Status.Succeed) ? Status.Reconnect : Status.None);
	}

	public void StartBackgroundDownloading(Action<int, int, string> progressCallback, Action<float> detailedProgressCallback, Action<bool> completeCallback)
	{
		if (!_isBackgroundDownloading)
		{
			_isBackgroundDownloading = true;
			List<AssetBundleFile> list = new List<AssetBundleFile>(_assetBundleFileDict.Values);
			list.Sort((AssetBundleFile a, AssetBundleFile b) => (a.Priority != b.Priority) ? ((a.Priority < b.Priority) ? 1 : (-1)) : 0);
			((MonoBehaviour)this).StartCoroutine(CoBackgroundDownload(list, allow3G: false, progressCallback, detailedProgressCallback, completeCallback));
		}
	}

	public void StopBackgroundDownloading()
	{
		_isBackgroundDownloading = false;
	}

	public void StartPrerequisiteLoading(Action<int, int, string> progressCallback, Action<float> detailedProgressCallback, Action<bool> completeCallback)
	{
		if (!_isBackgroundDownloading)
		{
			_isBackgroundDownloading = true;
			((MonoBehaviour)this).StartCoroutine(CoBackgroundDownload(_prerequisites, allow3G: true, progressCallback, detailedProgressCallback, completeCallback));
		}
	}

	public void EndPrerequisiteLoading()
	{
		_isBackgroundDownloading = false;
	}

	private IEnumerator CoBackgroundDownload(List<AssetBundleFile> list, bool allow3G, Action<int, int, string> progressCallback, Action<float> detailedProgressCallback, Action<bool> completeCallback)
	{
		bool succeed = true;
		if (!Caching.enabled || !UseBundle)
		{
			_isBackgroundDownloading = false;
			completeCallback?.Invoke(obj: true);
			yield break;
		}
		while (!Caching.ready)
		{
			yield return null;
		}
		int count = list.Count;
		int retryCount = 0;
		for (int i = 0; i < count; i++)
		{
			if (!_isBackgroundDownloading)
			{
				break;
			}
			AssetBundleFile file = list[i];
			string url = CreateTargetUrl(file.Name, file.Hash);
			if (Caching.IsVersionCached(url, file.Hash))
			{
				retryCount = 0;
				continue;
			}
			progressCallback?.Invoke(i + 1, retryCount, file.Name);
			if (allow3G || (int)Application.internetReachability == 2 || IsPrerequsite(file))
			{
				yield return ((MonoBehaviour)this).StartCoroutine(CoLoadFile(file, detailedProgressCallback, isPreload: true));
				if (!Caching.IsVersionCached(url, file.Hash))
				{
					i--;
					if (file.CurrentStatus != AssetBundleFile.Status.Loading)
					{
						retryCount++;
					}
					if (retryCount >= _maxRetryCount)
					{
						succeed = false;
						break;
					}
					yield return InternetCheckWaitTime;
				}
			}
			else
			{
				yield return InternetCheckWaitTime;
			}
		}
		_isBackgroundDownloading = false;
		completeCallback?.Invoke(succeed);
	}
}
