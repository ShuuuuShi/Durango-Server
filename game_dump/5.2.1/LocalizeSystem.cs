using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Durango.System;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using MsgPack;
using UnityEngine;

public static class LocalizeSystem
{
	public enum Status
	{
		None,
		Loading,
		Ready,
		Failed
	}

	private struct LocaleItem
	{
		public readonly string Language;

		public readonly string Locale;

		public readonly string Name;

		public readonly bool Lengthy;

		public readonly bool UsingSpace;

		public LocaleItem(string language, string locale, string name, bool lengthy, bool usingSpace)
		{
			Language = language;
			Locale = locale;
			Name = name;
			Lengthy = lengthy;
			UsingSpace = usingSpace;
		}
	}

	private struct VoiceLocaleItem
	{
		public readonly string Locale;

		public readonly string Name;

		public VoiceLocaleItem(string locale, string name)
		{
			Locale = locale;
			Name = name;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct SystemLanguageComparer : IEqualityComparer<SystemLanguage>
	{
		public bool Equals(SystemLanguage x, SystemLanguage y)
		{
			return x == y;
		}

		public int GetHashCode(SystemLanguage x)
		{
			return (int)x;
		}
	}

	private static readonly LocaleItem[] Locales = new LocaleItem[10]
	{
		new LocaleItem("en", "en_US", "English", lengthy: true, usingSpace: true),
		new LocaleItem("ko", "ko_KR", "한국어", lengthy: false, usingSpace: true),
		new LocaleItem("es", "es_MX", "Español", lengthy: true, usingSpace: true),
		new LocaleItem("pt", "pt_BR", "Português", lengthy: true, usingSpace: true),
		new LocaleItem("id", "id_ID", "Bahasa Indonesia", lengthy: true, usingSpace: true),
		new LocaleItem("ru", "ru_RU", "русский", lengthy: true, usingSpace: true),
		new LocaleItem("th", "th_TH", "ภาษาไทย", lengthy: false, usingSpace: false),
		new LocaleItem("de", "de_DE", "Deutsch", lengthy: true, usingSpace: true),
		new LocaleItem("fr", "fr_FR", "Français", lengthy: true, usingSpace: true),
		new LocaleItem("zh_Hant", "zh_TW", "中文(繁體)", lengthy: false, usingSpace: false)
	};

	private static readonly VoiceLocaleItem[] VoiceLocales = new VoiceLocaleItem[2]
	{
		new VoiceLocaleItem("en_US", "English"),
		new VoiceLocaleItem("ko_KR", "한국어")
	};

	private static string[] _availableLocales;

	private static string[] _availableVoiceLocales;

	private static readonly Dictionary<SystemLanguage, string> SystemLanguageDict = new Dictionary<SystemLanguage, string>(default(SystemLanguageComparer))
	{
		{
			UnityEngine.SystemLanguage.Afrikaans,
			"af"
		},
		{
			UnityEngine.SystemLanguage.Arabic,
			"ar"
		},
		{
			UnityEngine.SystemLanguage.Basque,
			"eu"
		},
		{
			UnityEngine.SystemLanguage.Belarusian,
			"be"
		},
		{
			UnityEngine.SystemLanguage.Bulgarian,
			"bg"
		},
		{
			UnityEngine.SystemLanguage.Catalan,
			"ca"
		},
		{
			UnityEngine.SystemLanguage.Chinese,
			"zh"
		},
		{
			UnityEngine.SystemLanguage.Czech,
			"cs"
		},
		{
			UnityEngine.SystemLanguage.Danish,
			"da"
		},
		{
			UnityEngine.SystemLanguage.Dutch,
			"nl"
		},
		{
			UnityEngine.SystemLanguage.English,
			"en"
		},
		{
			UnityEngine.SystemLanguage.Estonian,
			"et"
		},
		{
			UnityEngine.SystemLanguage.Faroese,
			"fo"
		},
		{
			UnityEngine.SystemLanguage.Finnish,
			"fi"
		},
		{
			UnityEngine.SystemLanguage.French,
			"fr"
		},
		{
			UnityEngine.SystemLanguage.German,
			"de"
		},
		{
			UnityEngine.SystemLanguage.Greek,
			"el"
		},
		{
			UnityEngine.SystemLanguage.Hebrew,
			"iw"
		},
		{
			UnityEngine.SystemLanguage.Icelandic,
			"is"
		},
		{
			UnityEngine.SystemLanguage.Indonesian,
			"id"
		},
		{
			UnityEngine.SystemLanguage.Italian,
			"it"
		},
		{
			UnityEngine.SystemLanguage.Japanese,
			"ja"
		},
		{
			UnityEngine.SystemLanguage.Korean,
			"ko"
		},
		{
			UnityEngine.SystemLanguage.Latvian,
			"lv"
		},
		{
			UnityEngine.SystemLanguage.Lithuanian,
			"lt"
		},
		{
			UnityEngine.SystemLanguage.Norwegian,
			"no"
		},
		{
			UnityEngine.SystemLanguage.Polish,
			"pl"
		},
		{
			UnityEngine.SystemLanguage.Portuguese,
			"pt"
		},
		{
			UnityEngine.SystemLanguage.Romanian,
			"ro"
		},
		{
			UnityEngine.SystemLanguage.Russian,
			"ru"
		},
		{
			UnityEngine.SystemLanguage.SerboCroatian,
			"sr"
		},
		{
			UnityEngine.SystemLanguage.Slovak,
			"sk"
		},
		{
			UnityEngine.SystemLanguage.Slovenian,
			"sl"
		},
		{
			UnityEngine.SystemLanguage.Spanish,
			"es"
		},
		{
			UnityEngine.SystemLanguage.Swedish,
			"sv"
		},
		{
			UnityEngine.SystemLanguage.Thai,
			"th"
		},
		{
			UnityEngine.SystemLanguage.Turkish,
			"tr"
		},
		{
			UnityEngine.SystemLanguage.Ukrainian,
			"uk"
		},
		{
			UnityEngine.SystemLanguage.Vietnamese,
			"vi"
		},
		{
			UnityEngine.SystemLanguage.ChineseSimplified,
			"zh_Hans"
		},
		{
			UnityEngine.SystemLanguage.ChineseTraditional,
			"zh_Hant"
		},
		{
			UnityEngine.SystemLanguage.Unknown,
			string.Empty
		},
		{
			UnityEngine.SystemLanguage.Hungarian,
			"hu"
		}
	};

	private static Dictionary<string, string> _keyValueDict;

	public static string[] AvailableLocales
	{
		get
		{
			if (_availableLocales == null)
			{
				_availableLocales = Locales.Select((LocaleItem x) => x.Locale).ToArray();
			}
			return _availableLocales;
		}
	}

	public static string[] AvailableVoiceLocales
	{
		get
		{
			if (_availableVoiceLocales == null)
			{
				_availableVoiceLocales = VoiceLocales.Select((VoiceLocaleItem x) => x.Locale).ToArray();
			}
			return _availableVoiceLocales;
		}
	}

	[CanBeNull]
	public static string Locale { get; private set; }

	[CanBeNull]
	public static string VoiceLocale { get; set; }

	public static string LocaleLanguage { get; private set; }

	public static string SystemLanguage => SystemLanguageDict.Get(Application.systemLanguage, string.Empty);

	public static string GetLocaleName(string locale)
	{
		int num = Locales.IndexOf((LocaleItem x) => x.Locale == locale);
		if (num == -1)
		{
			num = 0;
		}
		return Locales[num].Name;
	}

	public static string GetLocaleLanguage(string locale)
	{
		int num = Locales.IndexOf((LocaleItem x) => x.Locale == locale);
		if (num == -1)
		{
			num = 0;
		}
		return Locales[num].Language;
	}

	public static string GetVoiceLocaleName(string locale)
	{
		int num = VoiceLocales.IndexOf((VoiceLocaleItem x) => x.Locale == locale);
		if (num == -1)
		{
			num = 0;
		}
		return VoiceLocales[num].Name;
	}

	public static bool IsLengthyLocale(string locale)
	{
		LocaleItem[] locales = Locales;
		for (int i = 0; i < locales.Length; i++)
		{
			LocaleItem localeItem = locales[i];
			if (localeItem.Locale == locale)
			{
				return localeItem.Lengthy;
			}
		}
		return Locales[0].Lengthy;
	}

	private static bool IsUsingSpace(string locale)
	{
		LocaleItem[] locales = Locales;
		for (int i = 0; i < locales.Length; i++)
		{
			LocaleItem localeItem = locales[i];
			if (localeItem.Locale == locale)
			{
				return localeItem.UsingSpace;
			}
		}
		return Locales[0].UsingSpace;
	}

	public static string Get(string key)
	{
		key = _Get(key);
		return T._(key);
	}

	private static string _Get(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return key;
		}
		if (key[0] != '#')
		{
			return key;
		}
		if (_keyValueDict.TryGetValue(key, out var value))
		{
			return _Get(value);
		}
		return key;
	}

	public static bool Has(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		if (key[0] == '#')
		{
			return _keyValueDict.ContainsKey(key);
		}
		return false;
	}

	public static string GetRandom(string[] list)
	{
		if (list == null || list.Length == 0)
		{
			return string.Empty;
		}
		int num = UnityEngine.Random.Range(0, list.Length);
		return T._(list[num]);
	}

	public static List<string> GetSequenceKeys(string tokenBase, bool numberOnly)
	{
		List<string> list = new List<string>();
		foreach (string key in _keyValueDict.Keys)
		{
			if (!string.IsNullOrEmpty(key) && key.StartsWith(tokenBase, StringComparison.OrdinalIgnoreCase) && (!numberOnly || int.TryParse(key.Substring(key.LastIndexOf("_", StringComparison.Ordinal) + 1), out var _)))
			{
				list.Add(key);
			}
		}
		return list;
	}

	public static string SetLocale(string locale)
	{
		locale = NormalizeLocale(locale);
		LoadLegacyCatalog();
		Locale = locale;
		LocaleLanguage = GetLocaleLanguage(locale);
		Platform.Instance.SetLocale(locale);
		T.InstallCatalog(Locale);
		try
		{
			TextAsset textAsset = Resources.Load("offline/i18n/" + locale) as TextAsset;
			if (textAsset != null)
			{
				using MemoryStream moFileStream = new MemoryStream(textAsset.bytes);
				T.InstallCatalog(moFileStream, locale);
			}
		}
		catch (Exception)
		{
		}
		TextBuilder.WrapBySeperatorOnly = IsUsingSpace(locale);
		return locale;
	}

	[NotNull]
	private static string NormalizeLocale(string locale)
	{
		int num = Locales.IndexOf((LocaleItem x) => x.Locale == locale);
		if (num == -1)
		{
			string systemLanguage = SystemLanguage;
			num = Locales.IndexOf((LocaleItem x) => x.Language == systemLanguage);
			if (num == -1)
			{
				num = 0;
			}
		}
		return Locales[num].Locale;
	}

	public static string SetVoiceLocale(string locale)
	{
		locale = NormalizeVoiceLocale(locale);
		VoiceLocale = locale;
		return locale;
	}

	[NotNull]
	private static string NormalizeVoiceLocale(string voice)
	{
		int num = VoiceLocales.IndexOf((VoiceLocaleItem x) => x.Locale == voice);
		if (num == -1)
		{
			num = VoiceLocales.IndexOf((VoiceLocaleItem x) => x.Locale == Locale);
			if (num == -1)
			{
				num = 0;
			}
		}
		return VoiceLocales[num].Locale;
	}

	private static void LoadLegacyCatalog()
	{
		if (_keyValueDict == null)
		{
			_keyValueDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			LoadLegacyCatalog(_keyValueDict);
		}
	}

	public static void LoadLegacyCatalog(Dictionary<string, string> result)
	{
		string arg = null;
		try
		{
			TextAsset[] array = Resources.LoadAll<TextAsset>("localize");
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				TextAsset obj = array[i];
				arg = obj.name;
				Dictionary<string, string> dictionary = Json.Read<Dictionary<string, string>>(obj.text);
				if (dictionary == null)
				{
					continue;
				}
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					result[item.Key] = item.Value;
				}
			}
			RemoveInvaildData(result);
		}
		catch (Exception arg2)
		{
			Debug.LogError(string.Format("{0}/{1}: {2}", "localize", arg, arg2));
		}
	}

	private static void RemoveInvaildData(Dictionary<string, string> dict)
	{
		string[] array = new string[dict.Count];
		dict.Keys.CopyTo(array, 0);
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (string.IsNullOrEmpty(array[i]))
			{
				dict.Remove(array[i]);
			}
			else if (array[i][0] != '#')
			{
				dict.Remove(array[i]);
			}
		}
	}

	public static string UnpackGettextFromMsgPack(Unpacker unpacker)
	{
		if (unpacker.LastReadData.IsNil)
		{
			return null;
		}
		if (unpacker.IsMapHeader)
		{
			unpacker.ReadString(out var result);
			unpacker.Read();
			if (unpacker.LastReadData.IsNil)
			{
				return T.ParseMsgIdAndGetString(result);
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			int num = unpacker.LastReadData.AsInt32();
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				unpacker.ReadString(out var result2);
				unpacker.Read();
				if (result2 == null)
				{
					num2 = unpacker.LastReadData.AsInt32();
				}
				else
				{
					dictionary[result2] = UnpackGettextArgumentFromMsgPack(unpacker);
				}
			}
			string text = T.ParseMsgIdAndGetString(result, dictionary);
			for (int j = 0; j < num2; j++)
			{
				text = T.ParseMsgIdAndGetString(text);
			}
			return text;
		}
		if (unpacker.LastReadData.UnderlyingType == typeof(string))
		{
			return unpacker.LastReadData.AsString();
		}
		throw new IOException($"Gettext type expects Map or String but got {unpacker.LastReadData.UnderlyingType}");
	}

	public static object UnpackGettextArgumentFromMsgPack(Unpacker unpacker)
	{
		if (unpacker.IsMapHeader)
		{
			return UnpackGettextFromMsgPack(unpacker);
		}
		if (unpacker.IsArrayHeader)
		{
			int num = unpacker.LastReadData.AsInt32();
			object[] array = new object[num];
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				array[i] = UnpackGettextArgumentFromMsgPack(unpacker);
			}
			return array;
		}
		return unpacker.LastReadData.ToObject();
	}
}
