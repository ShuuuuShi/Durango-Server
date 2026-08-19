using System;
using System.Collections.Generic;
using Durango.UI;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.System;

public class Platform
{
	public enum StoreType
	{
		Unknown = 0,
		GooglePlayStore = 1,
		AppleAppStore = 2,
		Steam = 4,
		Arena = 5
	}

	public static Platform Instance { get; private set; }

	public virtual string NPSN => string.Empty;

	public virtual string AccountProvider => string.Empty;

	public virtual string StoreProvider => AccountProvider;

	public virtual StoreType Store => StoreType.Unknown;

	public string StoreName
	{
		get
		{
			if (Store == StoreType.Unknown)
			{
				return null;
			}
			return Store.ToString();
		}
	}

	public virtual bool IsPCStore => false;

	public virtual string AppBundleId => Application.identifier;

	public virtual string Token => string.Empty;

	public virtual string NPA => string.Empty;

	public virtual string ProductId => string.Empty;

	public virtual string ProductTicket => string.Empty;

	public virtual string ClusterListUrl => string.Empty;

	public virtual bool IsLoginTypeGuest => false;

	public virtual bool IsConnectFacebook => false;

	public virtual bool IsConnectGooglePlus => false;

	public virtual bool UseAssetBundle => true;

	public virtual bool IsAvailableOfferwall => false;

	public virtual RuntimePlatform AssetBundlePlatform => Application.platform;

	public virtual int DefaultRenderTargetSize => 512;

	public virtual string PrologueMovieUrl
	{
		get
		{
			if (Debug.isDebugBuild)
			{
				return "http://db.kyllox.pe.kr/durango/movies/standard/prologue_movie_dev.mp4";
			}
			return "http://db.kyllox.pe.kr/durango/movies/standard/prologue_movie.mp4";
		}
	}

	public virtual string LoginTypeDescription => string.Empty;

	public virtual string CountryLetterCode => Country switch
	{
		NPCountry.Korea => "KR", 
		NPCountry.Taiwan => "TW", 
		_ => "US", 
	};

	public virtual NPCountry Country => Application.systemLanguage switch
	{
		SystemLanguage.Korean => NPCountry.Korea, 
		SystemLanguage.ChineseTraditional => NPCountry.Taiwan, 
		_ => NPCountry.UnitedStatesofAmerica, 
	};

	public virtual string AdvertisingIdentifier => string.Empty;

	public virtual string MainSceneName => "Main";

	public UIPrefabMap.Type UIType
	{
		get
		{
			if (UsePCUI)
			{
				return UIPrefabMap.Type.PC;
			}
			return UIPrefabMap.Type.Mobile;
		}
	}

	public virtual bool UsePCUI => true;

	public virtual bool UsePCCoin => IsPCStore;

	public virtual int DefaultUISize => 1280;

	public virtual bool UsePCRenderer => false;

	public virtual bool SupportPortrait => true;

	static Platform()
	{
		Instance = new Platform_PC();
	}

	public virtual void Login(Action onSuccess, Action<int> onFailure)
	{
		onSuccess();
	}

	public virtual void Logout(Action<bool> onResult)
	{
		onResult(obj: true);
	}

	public virtual void Leave(Action<bool> onResult)
	{
		onResult(obj: true);
	}

	public virtual Dictionary<string, string> BuildSessionForm()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("account_provider", (!(AccountProvider == "steam")) ? AccountProvider : "arena");
		dictionary.Add("account_id", NPSN);
		dictionary.Add("token", Token);
		dictionary.Add("locale", LocalizeSystem.Locale);
		if (!string.IsNullOrEmpty(StoreName))
		{
			dictionary.Add("app_market", StoreName);
		}
		dictionary.Add("country", CountryLetterCode);
		dictionary.Add("platform", AssetBundlePlatform.ToString());
		dictionary.Add("adid", AdvertisingIdentifier);
		dictionary.Add("os_version", SystemInfo.operatingSystem);
		return dictionary;
	}

	public virtual void ShowWeb(string title, [NotNull] string url)
	{
		Application.OpenURL(url);
	}

	public virtual void ShowNotice()
	{
		Application.OpenURL("https://durango-database.vercel.app/archive/notice/");
	}

	public virtual void ShowPlate()
	{
	}

	public virtual void ShowCustomerServiece()
	{
	}

	public virtual void ShowAccountMenu()
	{
	}

	public virtual void SetLocale(string locale)
	{
	}

	public virtual void ShowOfferwall()
	{
	}

	public virtual void RequestPermission(string permission, Action<bool> callback)
	{
		callback?.Invoke(obj: true);
	}

	public virtual bool GetScreenResolution(bool isPortrait, out int width, out int height)
	{
		width = Screen.width;
		height = Screen.height;
		int uISize = UIManager.UISize;
		int num = Mathf.RoundToInt((float)uISize * UIAnchorPolicy.DefaultAspectRatio);
		if (SupportPortrait)
		{
			if (isPortrait)
			{
				if (width < height)
				{
					width = num;
					height = uISize;
					return true;
				}
			}
			else if (width > height)
			{
				width = uISize;
				height = num;
				return true;
			}
			return false;
		}
		width = uISize;
		height = num;
		return true;
	}

	public virtual void Quit()
	{
		Application.Quit();
	}
}
