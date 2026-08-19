using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using NGettext;
using SmartFormat;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using UnityEngine;

namespace L10N;

public static class T
{
	public class EnumNameAttribute : Attribute
	{
		public string Text { get; private set; }

		public EnumNameAttribute(string text)
		{
			Text = text;
		}
	}

	public class EnumParticularNameAttribute : Attribute
	{
		public string Text { get; private set; }

		public string Context { get; private set; }

		public EnumParticularNameAttribute(string context, string text)
		{
			Context = context;
			Text = text;
		}
	}

	private static readonly char[] GETTEXT_CONTEXT_SEPARATOR;

	private static ICatalog _catalog;

	private static readonly SmartFormatter Formatter;

	public static CultureInfo Culture;

	private static readonly Dictionary<Type, Dictionary<Enum, string>> EnumNames;

	static T()
	{
		GETTEXT_CONTEXT_SEPARATOR = new char[1] { '\u0004' };
		_catalog = new Catalog();
		EnumNames = new Dictionary<Type, Dictionary<Enum, string>>();
		Formatter = new SmartFormatter();
		Formatter.ErrorAction = ErrorAction.Ignore;
		Formatter.Parser.ErrorAction = ErrorAction.Ignore;
		ListFormatter listFormatter = new ListFormatter(Formatter);
		Formatter.AddExtensions(new DefaultSource(Formatter), new DictionarySource(Formatter), listFormatter);
		Formatter.AddExtensions(new KoreanFormatter(Formatter), new MarkupFormatter(Formatter), new TimedeltaFormatter(), listFormatter, new PluralLocalizationFormatter("en"), new ChooseFormatter(), new DefaultFormatter());
	}

	public static CultureInfo ParseCultureInfo(string locale)
	{
		CultureInfo cultureInfo = new CultureInfo(locale.Replace('_', '-'));
		ReformCultureInfo(cultureInfo);
		return cultureInfo;
	}

	public static void ReformCultureInfo(CultureInfo culture)
	{
		switch (culture.TwoLetterISOLanguageName)
		{
		case "ko":
		case "en":
			culture.NumberFormat.PercentPositivePattern = 1;
			break;
		case "es":
			culture.DateTimeFormat.ShortTimePattern = "hh:mm tt";
			break;
		}
	}

	public static void InstallCatalog(string locale)
	{
		string obj = ((Culture == null) ? null : Culture.Name);
		Culture = ParseCultureInfo(locale);
		_catalog = new Catalog("messages", "locales", Culture);
		if (obj != Culture.Name)
		{
			EnumNames.Clear();
		}
	}

	public static void InstallCatalog(Stream moFileStream, string locale)
	{
		string obj = ((Culture == null) ? null : Culture.Name);
		Culture = ParseCultureInfo(locale);
		_catalog = new Catalog(moFileStream, Culture);
		if (obj != Culture.Name)
		{
			EnumNames.Clear();
		}
	}

	private static string Format(string text, params object[] args)
	{
		return Formatter.Format(Culture, text, args);
	}

	public static string _(string text)
	{
		string @string = _catalog.GetString(text);
		if (Debug.isDebugBuild && Application.isPlaying && !string.IsNullOrEmpty(@string) && LocalizeSystem.Locale != "ko_KR")
		{
			string text2 = @string;
			foreach (char c in text2)
			{
				if (c >= '가' && c <= '힣')
				{
					return "☞" + @string;
				}
			}
		}
		return @string;
	}

	public static string _(string text, params object[] args)
	{
		return Format(_(text), args);
	}

	public static string N_(string text)
	{
		return text;
	}

	public static string GetParticularString(string context, string text)
	{
		return _catalog.GetParticularString(context, text);
	}

	public static string GetParticularString(string context, string text, params object[] args)
	{
		return Format(GetParticularString(context, text), args);
	}

	public static string ParseMsgIdAndGetString(string msgid)
	{
		if (msgid.IndexOf(GETTEXT_CONTEXT_SEPARATOR[0]) == -1)
		{
			return _(msgid);
		}
		string[] array = msgid.Split(GETTEXT_CONTEXT_SEPARATOR, 2);
		string context = array[0];
		string text = array[1];
		return GetParticularString(context, text);
	}

	public static string ParseMsgIdAndGetString(string msgid, params object[] args)
	{
		if (msgid.IndexOf(GETTEXT_CONTEXT_SEPARATOR[0]) == -1)
		{
			return _(msgid, args);
		}
		string[] array = msgid.Split(GETTEXT_CONTEXT_SEPARATOR, 2);
		string context = array[0];
		string text = array[1];
		return GetParticularString(context, text, args);
	}

	public static bool _has(string text)
	{
		return ((Catalog)_catalog).GetTranslations(text) != null;
	}

	public static string GetName(this Enum source)
	{
		Type type = source.GetType();
		if (!EnumNames.TryGetValue(type, out var value))
		{
			MemberInfo[] members = type.GetMembers();
			foreach (MemberInfo memberInfo in members)
			{
				Enum @enum;
				try
				{
					@enum = (Enum)Enum.Parse(type, memberInfo.Name);
				}
				catch
				{
					continue;
				}
				if (value == null)
				{
					value = new Dictionary<Enum, string>();
				}
				object[] customAttributes = memberInfo.GetCustomAttributes(typeof(EnumNameAttribute), inherit: false);
				if (customAttributes.Length != 0)
				{
					EnumNameAttribute enumNameAttribute = ((customAttributes.Length != 0) ? ((EnumNameAttribute)customAttributes[0]) : null);
					if (enumNameAttribute != null)
					{
						value[@enum] = _(enumNameAttribute.Text);
						continue;
					}
				}
				customAttributes = memberInfo.GetCustomAttributes(typeof(EnumParticularNameAttribute), inherit: false);
				if (customAttributes.Length != 0)
				{
					EnumParticularNameAttribute enumParticularNameAttribute = ((customAttributes.Length != 0) ? ((EnumParticularNameAttribute)customAttributes[0]) : null);
					if (enumParticularNameAttribute != null)
					{
						value[@enum] = GetParticularString(enumParticularNameAttribute.Context, enumParticularNameAttribute.Text);
						continue;
					}
				}
				value[@enum] = LocalizeUtil.Get(@enum);
			}
			EnumNames[type] = value;
		}
		if (value == null || !value.TryGetValue(source, out var value2))
		{
			return source.ToString();
		}
		return value2;
	}
}
