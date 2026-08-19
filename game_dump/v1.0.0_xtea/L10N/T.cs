using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using NGettext;
using SmartFormat;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

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

	private static ICatalog _catalog;

	private static readonly SmartFormatter Formatter;

	public static CultureInfo Culture;

	static T()
	{
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
		string twoLetterISOLanguageName = culture.TwoLetterISOLanguageName;
		if (twoLetterISOLanguageName == "ko" || twoLetterISOLanguageName == "en")
		{
			culture.NumberFormat.PercentPositivePattern = 1;
		}
	}

	public static void InstallCatalog(string locale)
	{
		Culture = ParseCultureInfo(locale);
		_catalog = new Catalog("messages", "locales", Culture);
	}

	public static void InstallCatalog(Stream moFileStream, string locale)
	{
		Culture = ParseCultureInfo(locale);
		_catalog = new Catalog(moFileStream, Culture);
	}

	public static string Format(string text, params object[] args)
	{
		return Formatter.Format(Culture, text, args);
	}

	public static string _(string text)
	{
		return _catalog.GetString(text);
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

	public static bool _has(string text)
	{
		Catalog catalog = (Catalog)_catalog;
		return catalog.GetTranslations(text) != null;
	}

	public static string GetName(this Enum source)
	{
		Type type = source.GetType();
		string name = source.ToString();
		MemberInfo[] member = type.GetMember(name);
		object[] array = ((member.Length != 0) ? member[0].GetCustomAttributes(typeof(EnumNameAttribute), inherit: false) : null);
		return (array != null && array.Length != 0) ? _(((EnumNameAttribute)array[0]).Text) : LocalizeUtil.Get(source);
	}
}
