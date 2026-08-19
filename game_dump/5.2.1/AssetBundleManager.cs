using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

	[CompilerGenerated]
	private sealed class _003CCoBackgroundDownload_003Ed__68 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AssetBundleManager _003C_003E4__this;

		public Action<bool> completeCallback;

		public Action<int, int> filterCallback;

		public List<AssetBundleFile> list;

		public Action<int, int, string> progressCallback;

		public bool allow3G;

		public Action<float> detailedProgressCallback;

		private int _003Ccount_003E5__2;

		private int _003CretryCount_003E5__3;

		private bool _003Csucceed_003E5__4;

		private int _003Ci_003E5__5;

		private AssetBundleFile _003Cfile_003E5__6;

		private string _003Curl_003E5__7;

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
		public _003CCoBackgroundDownload_003Ed__68(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cfile_003E5__6 = null;
			_003Curl_003E5__7 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			AssetBundleManager assetBundleManager = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (assetBundleManager.BackgroundDownloadStarted != null)
				{
					assetBundleManager.BackgroundDownloadStarted();
				}
				if (!UseBundle)
				{
					assetBundleManager._isBackgroundDownloading = false;
					completeCallback?.Invoke(obj: true);
					return false;
				}
				if (filterCallback != null)
				{
					list = assetBundleManager.FilterCached(list, out var sum);
					filterCallback(sum, list.Count);
				}
				_003Ccount_003E5__2 = list.Count;
				_003CretryCount_003E5__3 = 0;
				_003Csucceed_003E5__4 = true;
				_003Ci_003E5__5 = 0;
				goto IL_0267;
			case 1:
				_003C_003E1__state = -1;
				if (!Caching.IsVersionCached(_003Curl_003E5__7, _003Cfile_003E5__6.Hash) && _003Cfile_003E5__6.CurrentStatus != 0)
				{
					_003Ci_003E5__5--;
					if (_003Cfile_003E5__6.CurrentStatus == AssetBundleFile.Status.Failed)
					{
						_003CretryCount_003E5__3++;
						if (_003CretryCount_003E5__3 >= assetBundleManager._maxRetryCount)
						{
							_003Csucceed_003E5__4 = false;
							break;
						}
					}
					_003C_003E2__current = InternetCheckWaitTime;
					_003C_003E1__state = 2;
					return true;
				}
				goto IL_0257;
			case 2:
				_003C_003E1__state = -1;
				goto IL_0249;
			case 3:
				{
					_003C_003E1__state = -1;
					goto IL_0249;
				}
				IL_0249:
				_003Cfile_003E5__6 = null;
				_003Curl_003E5__7 = null;
				goto IL_0257;
				IL_0267:
				if (_003Ci_003E5__5 >= _003Ccount_003E5__2 || !assetBundleManager._isBackgroundDownloading)
				{
					break;
				}
				_003Cfile_003E5__6 = list[_003Ci_003E5__5];
				_003Curl_003E5__7 = assetBundleManager.CreateTargetUrl(_003Cfile_003E5__6.Name, _003Cfile_003E5__6.Crc);
				if (Caching.IsVersionCached(_003Curl_003E5__7, _003Cfile_003E5__6.Hash))
				{
					_003CretryCount_003E5__3 = 0;
					goto IL_0257;
				}
				progressCallback?.Invoke(_003Ci_003E5__5 + 1, _003CretryCount_003E5__3, _003Cfile_003E5__6.Name);
				if (allow3G || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork || IsPrerequsite(_003Cfile_003E5__6))
				{
					_003C_003E2__current = assetBundleManager.StartCoroutine(assetBundleManager.CoLoadFile(_003Cfile_003E5__6, detailedProgressCallback, isPreload: true));
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E2__current = InternetCheckWaitTime;
				_003C_003E1__state = 3;
				return true;
				IL_0257:
				_003Ci_003E5__5++;
				goto IL_0267;
			}
			assetBundleManager._isBackgroundDownloading = false;
			completeCallback?.Invoke(_003Csucceed_003E5__4);
			if (assetBundleManager.BackgroundDownloadCompleted != null)
			{
				assetBundleManager.BackgroundDownloadCompleted();
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

	[CompilerGenerated]
	private sealed class _003CCoLoadAssetBundleInfoHolder_003Ed__45 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AssetBundleManager _003C_003E4__this;

		public string infoHolderPath;

		private UnityWebRequest _003Crequest_003E5__2;

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
		public _003CCoLoadAssetBundleInfoHolder_003Ed__45(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Crequest_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			AssetBundleManager assetBundleManager = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				assetBundleManager.CurrentStatus = Status.LoadInfo;
				_003Crequest_003E5__2 = UnityWebRequest.Get(infoHolderPath);
				_003Crequest_003E5__2.SetRequestHeader("Accept-Encoding", "gzip");
				_003Crequest_003E5__2.SetRequestHeader("Accept", "application/json");
				_003Crequest_003E5__2.SetRequestHeader("Accept-Language", LocalizeSystem.Locale);
				_003Crequest_003E5__2.SetRequestHeader("X-K1-System-Language", LocalizeSystem.SystemLanguage);
				_003Crequest_003E5__2.SetRequestHeader("cache-control", "max-age=0");
				_003C_003E2__current = _003Crequest_003E5__2.SendWebRequest();
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				byte[] data = _003Crequest_003E5__2.downloadHandler.data;
				if (KUtility.GetSize(data) == 0)
				{
					assetBundleManager.CurrentStatus = Status.Failed;
					return false;
				}
				AssetBundleInfoHolder assetBundleInfoHolder = Json.Read<AssetBundleInfoHolder>(data);
				if (assetBundleInfoHolder == null)
				{
					assetBundleManager.CurrentStatus = Status.Failed;
					return false;
				}
				assetBundleManager._preloadHash = Hash128.Parse(assetBundleInfoHolder.PreloadHash);
				assetBundleManager._preloadCrc = assetBundleInfoHolder.PreloadCrc;
				assetBundleManager.LoadAssetBundeFiles(assetBundleInfoHolder);
				assetBundleManager.LoadAssetBundleItems(assetBundleInfoHolder);
				_003C_003E2__current = assetBundleManager.StartCoroutine(assetBundleManager.CoLoadPreloadFile());
				_003C_003E1__state = 2;
				return true;
			}
			case 2:
				_003C_003E1__state = -1;
				return false;
			}
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

	[CompilerGenerated]
	private sealed class _003CCoLoadFile_003Ed__52 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AssetBundleFile file;

		public AssetBundleManager _003C_003E4__this;

		public Action<float> progressCallback;

		public bool isPreload;

		private UnityWebRequest _003Crequest_003E5__2;

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
		public _003CCoLoadFile_003Ed__52(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || num == 2)
			{
				try
				{
					if (num == -4 || num == 2)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003Crequest_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			bool result;
			try
			{
				int num = _003C_003E1__state;
				AssetBundleManager assetBundleManager = _003C_003E4__this;
				switch (num)
				{
				default:
					result = false;
					goto end_IL_0000;
				case 0:
					_003C_003E1__state = -1;
					if (file.CurrentStatus != AssetBundleFile.Status.Loading)
					{
						file.CurrentStatus = AssetBundleFile.Status.Loading;
						goto IL_006d;
					}
					result = false;
					goto end_IL_0000;
				case 1:
					_003C_003E1__state = -1;
					goto IL_006d;
				case 2:
					{
						_003C_003E1__state = -4;
						progressCallback?.Invoke(_003Crequest_003E5__2.downloadProgress);
						break;
					}
					IL_006d:
					if (assetBundleManager._requestLimiter.Acquire())
					{
						_003C_003E1__state = -3;
						string uri = assetBundleManager.CreateTargetUrl(file.Name, file.Crc);
						_003Crequest_003E5__2 = UnityWebRequest.GetAssetBundle(uri, file.Hash, 0u);
						_003C_003E1__state = -4;
						_003Crequest_003E5__2.SendWebRequest();
						break;
					}
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					result = true;
					goto end_IL_0000;
				}
				if (!_003Crequest_003E5__2.isDone)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					result = true;
				}
				else
				{
					DownloadHandlerAssetBundle downloadHandlerAssetBundle = _003Crequest_003E5__2.downloadHandler as DownloadHandlerAssetBundle;
					if (_003Crequest_003E5__2.isNetworkError || downloadHandlerAssetBundle == null)
					{
						file.CurrentStatus = AssetBundleFile.Status.Failed;
						file.Bundle = null;
						result = false;
						goto IL_01ef;
					}
					if (isPreload && !file.Queued)
					{
						file.CurrentStatus = AssetBundleFile.Status.None;
						file.Bundle = null;
						result = false;
						goto IL_01ef;
					}
					if (file.Bundle != null)
					{
						file.Bundle.Unload(unloadAllLoadedObjects: false);
						file.Bundle = null;
					}
					file.Bundle = downloadHandlerAssetBundle.assetBundle;
					if (file.Bundle == null)
					{
						file.CurrentStatus = AssetBundleFile.Status.Failed;
					}
					_003C_003Em__Finally2();
					_003Crequest_003E5__2 = null;
					_003C_003Em__Finally1();
					result = false;
				}
				goto end_IL_0000;
				IL_01ef:
				_003C_003Em__Finally2();
				_003C_003Em__Finally1();
				end_IL_0000:;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
			return result;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			AssetBundleManager assetBundleManager = _003C_003E4__this;
			if (file.Bundle == null && file.CurrentStatus == AssetBundleFile.Status.Loading)
			{
				file.CurrentStatus = AssetBundleFile.Status.Failed;
			}
			assetBundleManager._requestLimiter.Release();
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003Crequest_003E5__2 != null)
			{
				((IDisposable)_003Crequest_003E5__2).Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCoLoadPreloadFile_003Ed__50 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AssetBundleManager _003C_003E4__this;

		private UnityWebRequest _003Crequest_003E5__2;

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
		public _003CCoLoadPreloadFile_003Ed__50(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003Crequest_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			bool result;
			try
			{
				int num = _003C_003E1__state;
				AssetBundleManager assetBundleManager = _003C_003E4__this;
				switch (num)
				{
				default:
					result = false;
					goto end_IL_0000;
				case 0:
					_003C_003E1__state = -1;
					assetBundleManager.CurrentStatus = Status.LoadPreload;
					if (assetBundleManager._preloadBundle != null)
					{
						assetBundleManager._preloadBundle.Unload(unloadAllLoadedObjects: false);
						assetBundleManager._preloadBundle = null;
					}
					_003Crequest_003E5__2 = UnityWebRequest.GetAssetBundle(assetBundleManager.CreateTargetUrl("preload.bundle", assetBundleManager._preloadCrc), assetBundleManager._preloadHash, 0u);
					_003C_003E1__state = -3;
					_003C_003E2__current = _003Crequest_003E5__2.SendWebRequest();
					_003C_003E1__state = 1;
					result = true;
					goto end_IL_0000;
				case 1:
				{
					_003C_003E1__state = -3;
					DownloadHandlerAssetBundle downloadHandlerAssetBundle = _003Crequest_003E5__2.downloadHandler as DownloadHandlerAssetBundle;
					if (!_003Crequest_003E5__2.isNetworkError && downloadHandlerAssetBundle != null)
					{
						assetBundleManager._preloadBundle = downloadHandlerAssetBundle.assetBundle;
						if (assetBundleManager._preloadBundle == null)
						{
							assetBundleManager.CurrentStatus = Status.Failed;
						}
						_003C_003Em__Finally1();
						_003Crequest_003E5__2 = null;
						break;
					}
					assetBundleManager.CurrentStatus = Status.Failed;
					result = false;
					_003C_003Em__Finally1();
					goto end_IL_0000;
				}
				case 2:
					_003C_003E1__state = -1;
					break;
				}
				if (!Caching.ready)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					result = true;
				}
				else
				{
					assetBundleManager.CurrentStatus = Status.Ready;
					result = false;
				}
				end_IL_0000:;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
			return result;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003Crequest_003E5__2 != null)
			{
				((IDisposable)_003Crequest_003E5__2).Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
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
			bool num = _prevInfoHolderPath == infoHolderPath;
			_prevInfoHolderPath = infoHolderPath;
			bool skipIfCached = num && (CurrentStatus == Status.Ready || CurrentStatus == Status.Cached);
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoLoadAssetBundleInfoHolder_003Ed__45(0)
		{
			_003C_003E4__this = this,
			infoHolderPath = infoHolderPath
		};
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
			AssetBundleFile assetBundleFile = new AssetBundleFile(holder.FileList[i]);
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
			if (assetBundleFileInfo.Dependencies != null)
			{
				AssetBundleFile assetBundleFile = _assetBundleFileDict[assetBundleFileInfo.Name];
				assetBundleFile.Dependencies = new AssetBundleFile[assetBundleFileInfo.Dependencies.Length];
				for (int j = 0; j < assetBundleFileInfo.Dependencies.Length; j++)
				{
					string key = assetBundleFileInfo.Dependencies[j];
					_assetBundleFileDict.TryGetValue(key, out assetBundleFile.Dependencies[j]);
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoLoadPreloadFile_003Ed__50(0)
		{
			_003C_003E4__this = this
		};
	}

	private string CreateTargetUrl(string fileName, string crc)
	{
		return AssetBundleItemInfo.GetCrcName(fileName, crc, AssetBundleLoadPath);
	}

	private IEnumerator CoLoadFile(AssetBundleFile file, Action<float> progressCallback = null, bool isPreload = false)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoLoadFile_003Ed__52(0)
		{
			_003C_003E4__this = this,
			file = file,
			progressCallback = progressCallback,
			isPreload = isPreload
		};
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
		if (!(file.Bundle != null))
		{
			return LoadStatus.Wait;
		}
		return LoadStatus.Complete;
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
		if (_isBackgroundDownloading)
		{
			return;
		}
		_isBackgroundDownloading = true;
		List<AssetBundleFile> list = new List<AssetBundleFile>(_assetBundleFileDict.Values);
		list.Sort(delegate(AssetBundleFile a, AssetBundleFile b)
		{
			if (a.Priority == b.Priority)
			{
				return 0;
			}
			return (a.Priority < b.Priority) ? 1 : (-1);
		});
		StartCoroutine(CoBackgroundDownload(list, allow3G: false, progressCallback, detailedProgressCallback, completeCallback));
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
			if (!Caching.IsVersionCached(CreateTargetUrl(assetBundleFile.Name, assetBundleFile.Crc), assetBundleFile.Hash))
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoBackgroundDownload_003Ed__68(0)
		{
			_003C_003E4__this = this,
			list = list,
			allow3G = allow3G,
			progressCallback = progressCallback,
			detailedProgressCallback = detailedProgressCallback,
			completeCallback = completeCallback,
			filterCallback = filterCallback
		};
	}

	public static string GetHqAssetPath(string path)
	{
		string directoryName = Path.GetDirectoryName(path);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
		string extension = Path.GetExtension(path);
		return directoryName + "/" + fileNameWithoutExtension + "_HQ" + extension;
	}
}
