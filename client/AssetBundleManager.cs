using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Durango.System;
using Durango.Utils;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
	public enum Status
	{
		None,
		LoadInfo,
		LoadPreload,
		Ready,
		Failed,
		Cached
	}

	private class AssetBundleItemToLoad
	{
		public int RetryCount;

		public AssetBundleItem Item;

		public Action<UnityEngine.Object> Finished;

		public Type ReqType;
	}

	private enum LoadStatus
	{
		Failed = -1,
		Wait,
		Complete
	}

	private class RequestLimiter
	{
		private int _maxCount;

		public void Clear()
		{
			_maxCount = 0;
		}

		public bool Acquire()
		{
			if (_maxCount > 30)
			{
				return false;
			}
			_maxCount++;
			return true;
		}

		public void Release()
		{
			_maxCount--;
		}
	}

	public static int PrerequsitePriority;

	private static readonly WaitForSeconds InternetCheckWaitTime;

	[SerializeField]
	private string _infoHolderName;

	[SerializeField]
	private int _maxRetryCount = 1;

	private Hash128 _preloadHash;

	private string _preloadCrc;

	private string _prevInfoHolderPath;

	private readonly Dictionary<string, AssetBundleItem> _assetBundleItemDict = new Dictionary<string, AssetBundleItem>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, AssetBundleFile> _assetBundleFileDict = new Dictionary<string, AssetBundleFile>(StringComparer.OrdinalIgnoreCase);

	private readonly List<AssetBundleItemToLoad> _itemToLoad = new List<AssetBundleItemToLoad>();

	private readonly List<AssetBundleFile> _prerequisites = new List<AssetBundleFile>();

	private bool _isBackgroundDownloading;

	private AssetBundle _preloadBundle;

	private readonly DictionaryIgnoreCase<UnityEngine.Object> _precachedAssets = new DictionaryIgnoreCase<UnityEngine.Object>();

	private int _precachedAssetsCount;

	private readonly RequestLimiter _requestLimiter = new RequestLimiter();

	private static string[] _playerClipPath;

	public string AssetBundleLoadPath { get; private set; }

	public static bool UseBundle => Platform.Instance.UseAssetBundle;

	public Status CurrentStatus { get; private set; }

	public int TotalFileCount => _assetBundleFileDict.Count;

	public int PrerequsitesCount => _prerequisites.Count;

	public event Action BackgroundDownloadStarted;

	public event Action BackgroundDownloadCompleted;

	static AssetBundleManager()
	{
		PrerequsitePriority = 500;
		InternetCheckWaitTime = new WaitForSeconds(2f);
		_playerClipPath = new string[2] { "models/pc/male/_anim/m_reference_clips.asset", "models/pc/female/_anim/f_reference_clips.asset" };
		GameManager.Reset += delegate
		{
			Singleton<AssetBundleManager>.Instance().StopBackgroundDownloading();
			Singleton<AssetBundleManager>.Instance().ClearAll();
		};
	}

	protected override bool CheckDontDestroyOnLoad()
	{
		return true;
	}

	public void Initialize(string infoHolderPath, string urlRoot)
	{
		_infoHolderName = "Info.5.2.1";
		if (UseBundle)
		{
			if (!string.IsNullOrEmpty(_infoHolderName))
			{
				RuntimePlatform platform = Application.platform;
				if (platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.OSXEditor)
				{
					AssetBundleLoadPath = "file://" + Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/", StringComparison.Ordinal)) + "/AssetBundles/";
				}
				else
				{
					AssetBundleLoadPath = "file://" + Application.streamingAssetsPath + "/AssetBundles/";
				}
				infoHolderPath = AssetBundleLoadPath + _infoHolderName + ".json";
			}
			else
			{
				if (string.IsNullOrEmpty(infoHolderPath) || string.IsNullOrEmpty(urlRoot))
				{
					CurrentStatus = Status.Failed;
					return;
				}
				AssetBundleLoadPath = urlRoot;
			}
			bool flag = _prevInfoHolderPath == infoHolderPath;
			_prevInfoHolderPath = infoHolderPath;
			bool skipIfCached = flag && (CurrentStatus == Status.Ready || CurrentStatus == Status.Cached);
			StartCoroutine(CoLoadAssetBundleInfoHolder(infoHolderPath, skipIfCached));
		}
		else
		{
			CurrentStatus = Status.Ready;
		}
	}

	private void Update()
	{
		if (CurrentStatus != Status.Ready)
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
				if (item.Parent.Bundle != null)
				{
					if (item.Request == null)
					{
						item.Request = item.Parent.Bundle.LoadAssetAsync(item.Name, assetBundleItemToLoad.ReqType);
					}
					if (item.Request.isDone)
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

	private IEnumerator CoLoadAssetBundleInfoHolder(string infoHolderPath, bool skipIfCached)
	{
		CurrentStatus = Status.LoadInfo;
		UnityWebRequest request = UnityWebRequest.Get(infoHolderPath);
		request.SetRequestHeader("Accept-Encoding", "gzip");
		request.SetRequestHeader("Accept", "application/json");
		request.SetRequestHeader("Accept-Language", LocalizeSystem.Locale);
		request.SetRequestHeader("X-K1-System-Language", LocalizeSystem.SystemLanguage);
		request.SetRequestHeader("cache-control", "max-age=0");
		yield return request.SendWebRequest();
		byte[] bytes = request.downloadHandler.data;
		if (KUtility.GetSize(bytes) == 0)
		{
			CurrentStatus = Status.Failed;
			yield break;
		}
		AssetBundleInfoHolder holder = Json.Read<AssetBundleInfoHolder>(bytes);
		if (holder == null)
		{
			CurrentStatus = Status.Failed;
			yield break;
		}
		_preloadHash = Hash128.Parse(holder.PreloadHash);
		_preloadCrc = holder.PreloadCrc;
		LoadAssetBundeFiles(holder);
		LoadAssetBundleItems(holder);
		yield return StartCoroutine(CoLoadPreloadFile());
	}

	private static bool IsPrerequsite(AssetBundleFile file)
	{
		return file.Priority > PrerequsitePriority;
	}

	private void LoadAssetBundeFiles(AssetBundleInfoHolder holder)
	{
		UnloadPrecachedAssets();
		foreach (KeyValuePair<string, AssetBundleFile> item in _assetBundleFileDict)
		{
			if (item.Value.Bundle != null)
			{
				item.Value.Bundle.Unload(unloadAllLoadedObjects: false);
			}
		}
		_assetBundleFileDict.Clear();
		_prerequisites.Clear();
		for (int i = 0; i < holder.FileList.Count; i++)
		{
			AssetBundleFileInfo fileInfo = holder.FileList[i];
			AssetBundleFile assetBundleFile = new AssetBundleFile(fileInfo);
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
				string key = assetBundleFileInfo.Dependencies[j];
				if (_assetBundleFileDict.TryGetValue(key, out assetBundleFile.Dependencies[j]))
				{
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
			string assetName = AssetBundleItemInfo.GetAssetName(assetBundleItemInfo.Name);
			string key = ((!assetBundleItemInfo.SavePerDirectory) ? assetBundleItemInfo.Name : AssetBundleItemInfo.GetParentName(assetBundleItemInfo.Name));
			AssetBundleFile assetBundleFile = _assetBundleFileDict.Get(key);
			if (assetBundleFile != null)
			{
				AssetBundleItem value = new AssetBundleItem(assetName, assetBundleFile);
				_assetBundleItemDict.Add(assetBundleItemInfo.Name, value);
			}
		}
	}

	private IEnumerator CoLoadPreloadFile()
	{
		CurrentStatus = Status.LoadPreload;
		if (_preloadBundle != null)
		{
			_preloadBundle.Unload(unloadAllLoadedObjects: false);
			_preloadBundle = null;
		}
		using (UnityWebRequest request = UnityWebRequest.GetAssetBundle(CreateTargetUrl("preload.bundle", _preloadCrc), _preloadHash, 0u))
		{
			yield return request.SendWebRequest();
			DownloadHandlerAssetBundle bundle = request.downloadHandler as DownloadHandlerAssetBundle;
			if (request.isNetworkError || bundle == null)
			{
				CurrentStatus = Status.Failed;
				yield break;
			}
			_preloadBundle = bundle.assetBundle;
			if (_preloadBundle == null)
			{
				CurrentStatus = Status.Failed;
			}
		}
		while (!Caching.ready)
		{
			yield return null;
		}
		CurrentStatus = Status.Ready;
	}

	private string CreateTargetUrl(string fileName, string crc)
	{
		return AssetBundleItemInfo.GetCrcName(fileName, crc, AssetBundleLoadPath);
	}

	private IEnumerator CoLoadFile(AssetBundleFile file, Action<float> progressCallback = null, bool isPreload = false)
	{
		if (file.CurrentStatus == AssetBundleFile.Status.Loading)
		{
			yield break;
		}
		file.CurrentStatus = AssetBundleFile.Status.Loading;
		while (!_requestLimiter.Acquire())
		{
			yield return null;
		}
		try
		{
			string url = CreateTargetUrl(file.Name, file.Crc);
			using UnityWebRequest request = UnityWebRequest.GetAssetBundle(url, file.Hash, 0u);
			request.SendWebRequest();
			while (!request.isDone)
			{
				yield return null;
				progressCallback?.Invoke(request.downloadProgress);
			}
			DownloadHandlerAssetBundle bundle = request.downloadHandler as DownloadHandlerAssetBundle;
			if (request.isNetworkError || bundle == null)
			{
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
			if (file.Bundle != null)
			{
				file.Bundle.Unload(unloadAllLoadedObjects: false);
				file.Bundle = null;
			}
			file.Bundle = bundle.assetBundle;
			if (file.Bundle == null)
			{
				file.CurrentStatus = AssetBundleFile.Status.Failed;
			}
		}
		finally
		{
			if (file.Bundle == null && file.CurrentStatus == AssetBundleFile.Status.Loading)
			{
				file.CurrentStatus = AssetBundleFile.Status.Failed;
			}
			_requestLimiter.Release();
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
			StartCoroutine(CoLoadFile(file));
			break;
		default:
			file.CurrentStatus = AssetBundleFile.Status.None;
			return LoadStatus.Failed;
		case AssetBundleFile.Status.Loading:
			break;
		}
		return (file.Bundle != null) ? LoadStatus.Complete : LoadStatus.Wait;
	}

	public bool RequestAsset(string assetPath, Type type, Action<UnityEngine.Object> callback)
	{
		if (string.IsNullOrEmpty(assetPath))
		{
			callback(null);
			return false;
		}
		if (CurrentStatus != Status.Ready && Application.isPlaying)
		{
			callback(null);
			return false;
		}
		string uniqueName = AssetBundleItemInfo.GetUniqueName(assetPath);
		AssetBundleItem assetBundleItem = _assetBundleItemDict.Get(uniqueName);
		if (assetBundleItem == null)
		{
			callback(null);
			return false;
		}
		AssetBundleItemToLoad assetBundleItemToLoad = null;
		int i = 0;
		for (int count = _itemToLoad.Count; i < count; i++)
		{
			if (_itemToLoad[i].Item == assetBundleItem && _itemToLoad[i].ReqType == type)
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
		assetBundleItemToLoad2.Finished = (Action<UnityEngine.Object>)Delegate.Combine(assetBundleItemToLoad2.Finished, callback);
		assetBundleItemToLoad.RetryCount = 0;
		return true;
	}

	public void UnloadAsset(string assetPath)
	{
		if ((!Application.isEditor || (UseBundle && Application.isPlaying)) && CurrentStatus == Status.Ready)
		{
			string uniqueName = AssetBundleItemInfo.GetUniqueName(assetPath);
			AssetBundleItem assetBundleItem = _assetBundleItemDict.Get(uniqueName);
			if (assetBundleItem != null && assetBundleItem.Parent.Bundle != null)
			{
				assetBundleItem.Parent.Bundle.Unload(unloadAllLoadedObjects: true);
				assetBundleItem.Parent.Bundle = null;
				assetBundleItem.Parent.CurrentStatus = AssetBundleFile.Status.None;
				assetBundleItem.Request = null;
			}
		}
	}

	public bool Contains(string assetPath)
	{
		string uniqueName = AssetBundleItemInfo.GetUniqueName(assetPath);
		return _assetBundleItemDict.ContainsKey(uniqueName);
	}

	public void ClearRequests()
	{
		_requestLimiter.Clear();
		for (int num = _itemToLoad.Count - 1; num >= 0; num--)
		{
			_itemToLoad[num].Finished(null);
		}
		_itemToLoad.Clear();
	}

	public void ClearAll()
	{
		_requestLimiter.Clear();
		_itemToLoad.Clear();
		StopAllCoroutines();
		foreach (KeyValuePair<string, AssetBundleItem> item in _assetBundleItemDict)
		{
			item.Value.Request = null;
		}
		foreach (KeyValuePair<string, AssetBundleFile> item2 in _assetBundleFileDict)
		{
			item2.Value.Queued = false;
			if (item2.Value.Bundle != null)
			{
				item2.Value.Bundle.Unload(unloadAllLoadedObjects: false);
				item2.Value.Bundle = null;
			}
			item2.Value.CurrentStatus = AssetBundleFile.Status.None;
		}
		UnloadPrecachedAssets();
		CurrentStatus = ((CurrentStatus == Status.Ready) ? Status.Cached : Status.None);
		this.BackgroundDownloadStarted = null;
		this.BackgroundDownloadCompleted = null;
	}

	public void PrecacheAssets()
	{
		UnloadPrecachedAssets();
		List<string> precachedAssets = new List<string>();
		precachedAssets.AddRange(_playerClipPath);
		precachedAssets.Add("Water/River/River.prefab");
		string[] array = new string[2] { "Water/Ocean/Ocean.prefab", "Water/Lake/Lake.prefab" };
		if (Platform.Instance.UsePCRenderer)
		{
			precachedAssets.AddRange(array.Select(GetHqAssetPath));
		}
		else
		{
			precachedAssets.AddRange(array);
		}
		_precachedAssetsCount = precachedAssets.Count;
		for (int i = 0; i < precachedAssets.Count; i++)
		{
			int index = i;
			RequestAsset(precachedAssets[index], typeof(UnityEngine.Object), delegate(UnityEngine.Object asset)
			{
				if (asset != null)
				{
					_precachedAssets[precachedAssets[index]] = asset;
				}
				else
				{
					Debug.LogError("Precache Asset is invalid : " + precachedAssets[index]);
				}
			});
		}
	}

	private void UnloadPrecachedAssets()
	{
		_precachedAssetsCount = 0;
		_precachedAssets.Clear();
	}

	public bool IsPrecachedAssetsReady()
	{
		return _precachedAssetsCount == _precachedAssets.Count;
	}

	public AnimationClipResource GetPlayerClip(bool male)
	{
		return GetPrecachedAsset<AnimationClipResource>(_playerClipPath[(!male) ? 1u : 0u]);
	}

	public T GetPrecachedAsset<T>(string path) where T : UnityEngine.Object
	{
		return _precachedAssets.Get(path) as T;
	}

	public void StartBackgroundDownloading(Action<int, int, string> progressCallback, Action<float> detailedProgressCallback, Action<bool> completeCallback)
	{
		if (!_isBackgroundDownloading)
		{
			_isBackgroundDownloading = true;
			List<AssetBundleFile> list = new List<AssetBundleFile>(_assetBundleFileDict.Values);
			list.Sort((AssetBundleFile a, AssetBundleFile b) => (a.Priority != b.Priority) ? ((a.Priority < b.Priority) ? 1 : (-1)) : 0);
			StartCoroutine(CoBackgroundDownload(list, allow3G: false, progressCallback, detailedProgressCallback, completeCallback));
		}
	}

	public void StopBackgroundDownloading()
	{
		_isBackgroundDownloading = false;
	}

	public void StartPrerequisiteLoading(Action<int, int, string> progressCallback, Action<float> detailedProgressCallback, Action<bool> completeCallback, Action<int, int> filterCallback)
	{
		if (!_isBackgroundDownloading)
		{
			_isBackgroundDownloading = true;
			StartCoroutine(CoBackgroundDownload(_prerequisites, allow3G: true, progressCallback, detailedProgressCallback, completeCallback, filterCallback));
		}
	}

	private List<AssetBundleFile> FilterCached(List<AssetBundleFile> list, out int sum)
	{
		List<AssetBundleFile> list2 = new List<AssetBundleFile>();
		sum = 0;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			AssetBundleFile assetBundleFile = list[i];
			string url = CreateTargetUrl(assetBundleFile.Name, assetBundleFile.Crc);
			if (!Caching.IsVersionCached(url, assetBundleFile.Hash))
			{
				list2.Add(assetBundleFile);
				sum += assetBundleFile.Size;
			}
		}
		sum = Mathf.CeilToInt((float)sum / 1000000f);
		return list2;
	}

	private IEnumerator CoBackgroundDownload(List<AssetBundleFile> list, bool allow3G, Action<int, int, string> progressCallback, Action<float> detailedProgressCallback, Action<bool> completeCallback, Action<int, int> filterCallback = null)
	{
		if (this.BackgroundDownloadStarted != null)
		{
			this.BackgroundDownloadStarted();
		}
		if (!UseBundle)
		{
			_isBackgroundDownloading = false;
			completeCallback?.Invoke(obj: true);
			yield break;
		}
		if (filterCallback != null)
		{
			list = FilterCached(list, out var sum);
			filterCallback(sum, list.Count);
		}
		int count = list.Count;
		int retryCount = 0;
		bool succeed = true;
		for (int i = 0; i < count; i++)
		{
			if (!_isBackgroundDownloading)
			{
				break;
			}
			AssetBundleFile file = list[i];
			string url = CreateTargetUrl(file.Name, file.Crc);
			if (Caching.IsVersionCached(url, file.Hash))
			{
				retryCount = 0;
				continue;
			}
			progressCallback?.Invoke(i + 1, retryCount, file.Name);
			if (allow3G || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork || IsPrerequsite(file))
			{
				yield return StartCoroutine(CoLoadFile(file, detailedProgressCallback, isPreload: true));
				if (Caching.IsVersionCached(url, file.Hash) || file.CurrentStatus == AssetBundleFile.Status.None)
				{
					continue;
				}
				i--;
				if (file.CurrentStatus == AssetBundleFile.Status.Failed)
				{
					retryCount++;
					if (retryCount >= _maxRetryCount)
					{
						succeed = false;
						break;
					}
				}
				yield return InternetCheckWaitTime;
			}
			else
			{
				yield return InternetCheckWaitTime;
			}
		}
		_isBackgroundDownloading = false;
		completeCallback?.Invoke(succeed);
		if (this.BackgroundDownloadCompleted != null)
		{
			this.BackgroundDownloadCompleted();
		}
	}

	public static string GetHqAssetPath(string path)
	{
		string directoryName = Path.GetDirectoryName(path);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
		string extension = Path.GetExtension(path);
		return $"{directoryName}/{fileNameWithoutExtension}_HQ{extension}";
	}
}
