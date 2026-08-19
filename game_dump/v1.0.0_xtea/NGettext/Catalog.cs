using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NGettext.Loaders;
using NGettext.Plural;

namespace NGettext;

public class Catalog : ICatalog
{
	public const char CONTEXT_GLUE = '\u0004';

	private IPluralRule _PluralRule;

	public CultureInfo CultureInfo { get; protected set; }

	public Dictionary<string, string[]> Translations { get; protected set; }

	public IPluralRule PluralRule
	{
		get
		{
			return _PluralRule;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_PluralRule = value;
		}
	}

	public Catalog()
		: this(CultureInfo.CurrentUICulture)
	{
	}

	public Catalog(CultureInfo cultureInfo)
	{
		CultureInfo = cultureInfo;
		Translations = new Dictionary<string, string[]>();
		PluralRule = new DefaultPluralRuleGenerator().CreateRule(cultureInfo);
	}

	public Catalog(ILoader loader)
		: this(loader, CultureInfo.CurrentUICulture)
	{
	}

	public Catalog(ILoader loader, CultureInfo cultureInfo)
		: this(cultureInfo)
	{
		try
		{
			Load(loader);
		}
		catch (FileNotFoundException)
		{
		}
	}

	public Catalog(Stream moStream)
		: this(new MoLoader(moStream))
	{
	}

	public Catalog(Stream moStream, CultureInfo cultureInfo)
		: this(new MoLoader(moStream), cultureInfo)
	{
	}

	public Catalog(string domain, string localeDir)
		: this(new MoLoader(domain, localeDir))
	{
	}

	public Catalog(string domain, string localeDir, CultureInfo cultureInfo)
		: this(new MoLoader(domain, localeDir), cultureInfo)
	{
	}

	public void Load(ILoader loader)
	{
		if (loader == null)
		{
			throw new ArgumentNullException("loader");
		}
		loader.Load(this);
	}

	public virtual string GetString(string text)
	{
		return GetStringDefault(text, text);
	}

	public virtual string GetString(string text, params object[] args)
	{
		return string.Format(CultureInfo, GetStringDefault(text, text), args);
	}

	public virtual string GetPluralString(string text, string pluralText, long n)
	{
		return GetPluralStringDefault(text, text, pluralText, n);
	}

	public virtual string GetPluralString(string text, string pluralText, long n, params object[] args)
	{
		return string.Format(CultureInfo, GetPluralStringDefault(text, text, pluralText, n), args);
	}

	public virtual string GetParticularString(string context, string text)
	{
		return GetStringDefault(context + '\u0004' + text, text);
	}

	public virtual string GetParticularString(string context, string text, params object[] args)
	{
		return string.Format(CultureInfo, GetStringDefault(context + '\u0004' + text, text), args);
	}

	public virtual string GetParticularPluralString(string context, string text, string pluralText, long n)
	{
		return GetPluralStringDefault(context + '\u0004' + text, text, pluralText, n);
	}

	public virtual string GetParticularPluralString(string context, string text, string pluralText, long n, params object[] args)
	{
		return string.Format(CultureInfo, GetPluralStringDefault(context + '\u0004' + text, text, pluralText, n), args);
	}

	public virtual string GetStringDefault(string messageId, string defaultMessage)
	{
		string[] translations = GetTranslations(messageId);
		if (translations == null || translations.Length == 0)
		{
			return defaultMessage;
		}
		return translations[0];
	}

	public virtual string GetPluralStringDefault(string messageId, string defaultMessage, string defaultPluralMessage, long n)
	{
		string[] translations = GetTranslations(messageId);
		int num = PluralRule.Evaluate(n);
		if (num < 0 || num >= PluralRule.NumPlurals)
		{
			throw new IndexOutOfRangeException($"Calculated plural form index ({num}) is out of allowed range (0~{PluralRule.NumPlurals - 1}).");
		}
		if (translations == null || translations.Length <= num)
		{
			return (n != 1) ? defaultPluralMessage : defaultMessage;
		}
		return translations[num];
	}

	public virtual string[] GetTranslations(string messageId)
	{
		if (string.IsNullOrEmpty(messageId))
		{
			return null;
		}
		if (!Translations.ContainsKey(messageId))
		{
			return null;
		}
		return Translations[messageId];
	}
}
