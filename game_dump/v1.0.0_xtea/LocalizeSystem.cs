using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BestHTTP;
using L10N;
using MsgPack;
using UnityEngine;

public static class LocalizeSystem
{
	public enum DownloadState
	{
		Downloading,
		Succeed,
		Failed
	}

	public const string DefaultLocale = "ko_KR";

	private static readonly Dictionary<string, string> LocaleAliases = new Dictionary<string, string>
	{
		{ "el", "el_GR" },
		{ "fr", "fr_FR" },
		{ "bg", "bg_BG" },
		{ "th", "th_TH" },
		{ "et", "et_EE" },
		{ "ko", "ko_KR" },
		{ "ca", "ca_ES" },
		{ "de", "de_DE" },
		{ "it", "it_IT" },
		{ "da", "da_DK" },
		{ "fa", "fa_IR" },
		{ "ar", "ar_SY" },
		{ "km", "km_KH" },
		{ "bs", "bs_BA" },
		{ "cs", "cs_CZ" },
		{ "fi", "fi_FI" },
		{ "gl", "gl_ES" },
		{ "id", "id_ID" },
		{ "es", "es_ES" },
		{ "he", "he_IL" },
		{ "ru", "ru_RU" },
		{ "nl", "nl_NL" },
		{ "nn", "nn_NO" },
		{ "pt", "pt_PT" },
		{ "no", "nb_NO" },
		{ "tr", "tr_TR" },
		{ "sv", "sv_SE" },
		{ "mk", "mk_MK" },
		{ "ja", "ja_JP" },
		{ "lv", "lv_LV" },
		{ "lt", "lt_LT" },
		{ "en", "en_US" },
		{ "sk", "sk_SK" },
		{ "uk", "uk_UA" },
		{ "sl", "sl_SI" },
		{ "hu", "hu_HU" },
		{ "ro", "ro_RO" },
		{ "is", "is_IS" },
		{ "pl", "pl_PL" }
	};

	public static readonly string[] AvailableLocales = new string[2] { "ko_KR", "en_US" };

	public static readonly Dictionary<string, string> LocaleNames = new Dictionary<string, string>
	{
		{ "ko_KR", "한국어" },
		{ "en_US", "English" }
	};

	private static string[] _availableLocalesWithoutDefault;

	public static Dictionary<string, string> KeyValueDict { get; private set; }

	public static string[] AvailableLocalesWithoutDefault
	{
		get
		{
			if (_availableLocalesWithoutDefault == null)
			{
				List<string> list = new List<string>();
				for (int i = 0; i < AvailableLocales.Length; i++)
				{
					string text = AvailableLocales[i];
					if (text != "ko_KR")
					{
						list.Add(text);
					}
				}
				_availableLocalesWithoutDefault = list.ToArray();
			}
			return _availableLocalesWithoutDefault;
		}
	}

	public static bool IsLocaleNotLoadedYet => string.IsNullOrEmpty(Locale);

	public static string Locale { get; private set; }

	public static bool LoadI18NFinished { get; private set; }

	public static DownloadState DownloadL10NState { get; private set; }

	public static string NormalizeLocale(string locale)
	{
		if (string.IsNullOrEmpty(locale))
		{
			return "ko_KR";
		}
		locale = locale.ToLower();
		if (LocaleAliases.TryGetValue(locale, out var value))
		{
			return value;
		}
		locale = locale.Replace('-', '_');
		if (locale.Contains("_"))
		{
			locale = locale.Substring(0, 3) + locale.Substring(3).ToUpper();
		}
		return locale;
	}

	public static IEnumerator LoadI18N()
	{
		string locale = Locale;
		DownloadL10NState = DownloadState.Downloading;
		int retryCount = 0;
		while (DownloadL10NState == DownloadState.Downloading && retryCount < 5)
		{
			string url = KSingleton<GameManager>.Instance().MakeGatewayUrl("i18n/messages.mo");
			Dictionary<string, string> requestHeader = new Dictionary<string, string> { { "Accept-Language", locale } };
			HTTPRequest req = KUtility.RequestUrl(url, null, disableCache: false, requestHeader);
			while (req.MoveNext())
			{
				yield return null;
			}
			bool isCached;
			byte[] bytes = KUtility.ProcessResult(req, out isCached);
			if (bytes != null)
			{
				using (MemoryStream ms = new MemoryStream(bytes))
				{
					T.InstallCatalog(ms, locale);
				}
				LoadI18NFinished = true;
				DownloadL10NState = DownloadState.Succeed;
				yield break;
			}
			retryCount++;
		}
		DownloadL10NState = DownloadState.Failed;
	}

	public static string Get(string key)
	{
		key = _Get(key);
		return T._(key);
	}

	public static string _Get(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return key;
		}
		if (key[0] != '#')
		{
			return key;
		}
		string value;
		return (!KeyValueDict.TryGetValue(key, out value)) ? key : _Get(value);
	}

	public static bool Has(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		return key[0] == '#' && KeyValueDict.ContainsKey(key);
	}

	public static string Format(string format, params string[] args)
	{
		object[] array = new object[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			array[i] = Get(args[i]);
		}
		string text = Get(format);
		return T.Format(text, array);
	}

	public static List<string> GetSequences(string tokenBase)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < 10; i++)
		{
			string key = tokenBase + "_" + i;
			if (Has(key))
			{
				list.Add(Get(key));
			}
		}
		return list;
	}

	public static string GetRandom(string[] list)
	{
		if (list == null || list.Length == 0)
		{
			return string.Empty;
		}
		int num = Random.Range(0, list.Length);
		return T._(list[num]);
	}

	public static List<string> GetSequenceKeys(string tokenBase, bool numberOnly)
	{
		List<string> list = new List<string>();
		foreach (string key in KeyValueDict.Keys)
		{
			if (string.IsNullOrEmpty(key) || !key.StartsWith(tokenBase, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			if (numberOnly)
			{
				string s = key.Substring(key.LastIndexOf("_", StringComparison.Ordinal) + 1);
				if (!int.TryParse(s, out var _))
				{
					continue;
				}
			}
			list.Add(key);
		}
		return list;
	}

	public static void SetLocale(string locale)
	{
		LoadLegacyCatalog();
		Locale = NormalizeLocale(locale);
		T.InstallCatalog(Locale);
		LoadI18NFinished = false;
	}

	private static void LoadLegacyCatalog()
	{
		if (KeyValueDict != null)
		{
			return;
		}
		KeyValueDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		string arg = null;
		try
		{
			TextAsset[] array = Resources.LoadAll<TextAsset>("localize/ko");
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				TextAsset val = array[i];
				arg = ((Object)val).name;
				Dictionary<string, string> dictionary = KUtility.ParseJson<Dictionary<string, string>>(val.text);
				if (dictionary == null)
				{
					continue;
				}
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					KeyValueDict[item.Key] = item.Value;
				}
			}
			RemoveInvaildData();
		}
		catch (Exception arg2)
		{
			Debug.LogError((object)string.Format("{0}/{1}: {2}", "localize/ko", arg, arg2));
		}
	}

	private static void RemoveInvaildData()
	{
		string[] array = new string[KeyValueDict.Count];
		KeyValueDict.Keys.CopyTo(array, 0);
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (string.IsNullOrEmpty(array[i]))
			{
				KeyValueDict.Remove(array[i]);
			}
			else if (array[i][0] != '#')
			{
				KeyValueDict.Remove(array[i]);
			}
		}
	}

	public static string UnpackGettextFromMsgPack(Unpacker unpacker)
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (unpacker.IsMapHeader)
		{
			string text = default(string);
			unpacker.ReadString(ref text);
			unpacker.Read();
			MessagePackObject lastReadData = unpacker.LastReadData;
			if (((MessagePackObject)(ref lastReadData)).IsNil)
			{
				return T._(text);
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
			int num2 = 0;
			string text2 = default(string);
			for (int i = 0; i < num; i++)
			{
				unpacker.ReadString(ref text2);
				unpacker.Read();
				if (text2 == null)
				{
					MessagePackObject lastReadData3 = unpacker.LastReadData;
					num2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
				}
				else
				{
					dictionary[text2] = UnpackGettextArgumentFromMsgPack(unpacker);
				}
			}
			string text3 = ((dictionary.Count != 0) ? T._(text, dictionary) : T._(text));
			for (int j = 0; j < num2; j++)
			{
				text3 = T._(text3);
			}
			return text3;
		}
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if ((object)((MessagePackObject)(ref lastReadData4)).UnderlyingType == typeof(string))
		{
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			return ((MessagePackObject)(ref lastReadData5)).AsString();
		}
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		throw new IOException($"Gettext type expects Map or String but got {((MessagePackObject)(ref lastReadData6)).UnderlyingType}");
	}

	public static object UnpackGettextArgumentFromMsgPack(Unpacker unpacker)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (unpacker.IsMapHeader)
		{
			return UnpackGettextFromMsgPack(unpacker);
		}
		if (unpacker.IsArrayHeader)
		{
			MessagePackObject lastReadData = unpacker.LastReadData;
			int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
			object[] array = new object[num];
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				array[i] = UnpackGettextArgumentFromMsgPack(unpacker);
			}
			return array;
		}
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		return ((MessagePackObject)(ref lastReadData2)).ToObject();
	}
}
