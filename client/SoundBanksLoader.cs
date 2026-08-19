using System;
using System.Collections.Generic;
using System.IO;
using Durango.System;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public class SoundBanksLoader
{
	public enum State
	{
		Clear,
		Initializing,
		Loaded,
		LoadFailed
	}

	public const string SoundBankFolderName = "SoundBanks";

	public const string SoundBanksInfoFileName = "SoundbanksInfo.json";

	public const string BinaryExtension = ".bytes";

	private string _soundBankPath;

	private readonly SoundBanksInfo _soundBanksInfo = new SoundBanksInfo();

	private readonly Dictionary<string, BankLoader> _bankLoaders = new Dictionary<string, BankLoader>(StringComparer.OrdinalIgnoreCase);

	private int _version;

	public State LoadState { get; private set; }

	public void Initialize()
	{
		ClearAll();
		LoadState = State.Initializing;
		_soundBankPath = Path.Combine("SoundBanks", GetTargetFolder());
		_soundBanksInfo.Initialize(Path.Combine(_soundBankPath, "SoundbanksInfo.json"), delegate(bool result)
		{
			if (result)
			{
				LoadEventIncludedBanks(new Stack<string>(_soundBanksInfo.EventIncludedBankPaths));
			}
			else
			{
				LoadState = State.LoadFailed;
			}
		});
	}

	public void ClearAll()
	{
		_version++;
		LoadState = State.Clear;
		foreach (KeyValuePair<string, BankLoader> bankLoader in _bankLoaders)
		{
			bankLoader.Value.Unload();
		}
		_bankLoaders.Clear();
		_soundBanksInfo.Clear();
	}

	public bool ContainsEvent(string eventName)
	{
		return _soundBanksInfo.ContainsEvent(eventName);
	}

	public bool IsPreparedEvent(string eventName)
	{
		if (LoadState == State.Loaded)
		{
			return GetBankLoader(eventName)?.IsLoaded ?? false;
		}
		return false;
	}

	public void LoadBankByEventName(string eventName, Action callback = null)
	{
		if (LoadState == State.Loaded)
		{
			GetBankLoader(eventName, createIfNotFound: true)?.AddCallback(callback);
		}
	}

	private void LoadEventIncludedBanks(Stack<string> bankPathSet)
	{
		if (bankPathSet.Count > 0)
		{
			RequestAssetBundle(CreateBankLoader(bankPathSet.Pop()), delegate(bool result)
			{
				if (result)
				{
					LoadEventIncludedBanks(bankPathSet);
				}
				else
				{
					LoadState = State.LoadFailed;
				}
			});
		}
		else
		{
			LoadState = State.Loaded;
		}
	}

	[NotNull]
	private BankLoader CreateBankLoader([NotNull] string bankPath)
	{
		string assetPath = Path.Combine(_soundBankPath, bankPath + ".bytes");
		BankLoader bankLoader = ((!Singleton<AssetBundleManager>.Instance().Contains(assetPath)) ? ((BankLoader)new PreloadedBankLoader(bankPath)) : ((BankLoader)new AssetBundleBankLoader(bankPath)));
		_bankLoaders.Add(bankPath, bankLoader);
		return bankLoader;
	}

	[CanBeNull]
	private BankLoader GetBankLoader(string eventName, bool createIfNotFound = false)
	{
		string mediaBankPathByEventName = _soundBanksInfo.GetMediaBankPathByEventName(eventName);
		if (mediaBankPathByEventName != null)
		{
			BankLoader bankLoader = _bankLoaders.Get(mediaBankPathByEventName);
			if (bankLoader != null)
			{
				return bankLoader;
			}
			if (createIfNotFound)
			{
				bankLoader = CreateBankLoader(mediaBankPathByEventName);
				RequestAssetBundle(bankLoader);
				return bankLoader;
			}
		}
		else if (!createIfNotFound)
		{
		}
		return null;
	}

	private void RequestAssetBundle([NotNull] BankLoader loader, Action<bool> onReply = null)
	{
		if (loader is AssetBundleBankLoader)
		{
			int requested = _version;
			string assetPath = Path.Combine(_soundBankPath, loader.BankPath + ".bytes");
			Singleton<AssetBundleManager>.Instance().RequestAsset(assetPath, typeof(TextAsset), delegate(UnityEngine.Object asset)
			{
				if (requested == _version)
				{
					TextAsset textAsset = asset as TextAsset;
					if (textAsset != null && loader.Load(textAsset.bytes))
					{
						Singleton<AssetBundleManager>.Instance().UnloadAsset(assetPath);
						if (onReply != null)
						{
							onReply(obj: true);
						}
					}
					else
					{
						_bankLoaders.Remove(loader.BankPath);
						if (onReply != null)
						{
							onReply(obj: false);
						}
					}
				}
			});
		}
		else if (onReply != null)
		{
			onReply(obj: true);
		}
	}

	private static string GetTargetFolder()
	{
		return Platform.Instance.AssetBundlePlatform switch
		{
			RuntimePlatform.IPhonePlayer => "iOS", 
			RuntimePlatform.Android => "Android", 
			_ => "Windows", 
		};
	}
}
