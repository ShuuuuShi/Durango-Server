using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NGettext.Plural;
using UnityEngine;

namespace NGettext.Loaders;

public class MoLoader : ILoader
{
	private const string LC_MESSAGES = "LC_MESSAGES";

	private const string MO_FILE_EXT = ".mo";

	private readonly Stream _MoStream;

	private readonly string _FilePath;

	private readonly string _Domain;

	private readonly string _LocaleDir;

	public IPluralRuleGenerator PluralRuleGenerator { get; private set; }

	public MoFileParser Parser { get; private set; }

	public MoLoader(string domain, string localeDir, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser)
	{
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		if (localeDir == null)
		{
			throw new ArgumentNullException("localeDir");
		}
		if (pluralRuleGenerator == null)
		{
			throw new ArgumentNullException("pluralRuleGenerator");
		}
		if (parser == null)
		{
			throw new ArgumentNullException("parser");
		}
		_Domain = domain;
		_LocaleDir = localeDir;
		PluralRuleGenerator = pluralRuleGenerator;
		Parser = parser;
	}

	public MoLoader(string filePath, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser)
	{
		if (filePath == null)
		{
			throw new ArgumentNullException("filePath");
		}
		if (pluralRuleGenerator == null)
		{
			throw new ArgumentNullException("pluralRuleGenerator");
		}
		if (parser == null)
		{
			throw new ArgumentNullException("parser");
		}
		_FilePath = filePath;
		PluralRuleGenerator = pluralRuleGenerator;
		Parser = parser;
	}

	public MoLoader(Stream moStream, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser)
	{
		if (moStream == null)
		{
			throw new ArgumentNullException("moStream");
		}
		if (pluralRuleGenerator == null)
		{
			throw new ArgumentNullException("pluralRuleGenerator");
		}
		if (parser == null)
		{
			throw new ArgumentNullException("parser");
		}
		_MoStream = moStream;
		PluralRuleGenerator = pluralRuleGenerator;
		Parser = parser;
	}

	public MoLoader(string domain, string localeDir, IPluralRuleGenerator pluralRuleGenerator)
		: this(domain, localeDir, pluralRuleGenerator, new MoFileParser())
	{
	}

	public MoLoader(string domain, string localeDir, MoFileParser parser)
		: this(domain, localeDir, new DefaultPluralRuleGenerator(), parser)
	{
	}

	public MoLoader(string domain, string localeDir)
		: this(domain, localeDir, new DefaultPluralRuleGenerator(), new MoFileParser())
	{
	}

	public MoLoader(string filePath, IPluralRuleGenerator pluralRuleGenerator)
		: this(filePath, pluralRuleGenerator, new MoFileParser())
	{
	}

	public MoLoader(string filePath, MoFileParser parser)
		: this(filePath, new DefaultPluralRuleGenerator(), parser)
	{
	}

	public MoLoader(string filePath)
		: this(filePath, new DefaultPluralRuleGenerator(), new MoFileParser())
	{
	}

	public MoLoader(Stream moStream, IPluralRuleGenerator pluralRuleGenerator)
		: this(moStream, pluralRuleGenerator, new MoFileParser())
	{
	}

	public MoLoader(Stream moStream, MoFileParser parser)
		: this(moStream, new DefaultPluralRuleGenerator(), parser)
	{
	}

	public MoLoader(Stream moStream)
		: this(moStream, new DefaultPluralRuleGenerator(), new MoFileParser())
	{
	}

	public void Load(Catalog catalog)
	{
		if (_MoStream != null)
		{
			Load(_MoStream, catalog);
		}
		else if (_FilePath != null)
		{
			Load(_FilePath, catalog);
		}
		else
		{
			Load(_Domain, _LocaleDir, catalog);
		}
	}

	protected virtual void Load(string domain, string localeDir, Catalog catalog)
	{
		string text = FindTranslationFile(catalog.CultureInfo, domain, localeDir);
		if (text == null)
		{
			throw new FileNotFoundException($"Can not find MO file name in locale directory \"{localeDir}\".");
		}
		Load(text, catalog);
	}

	protected virtual void Load(string filePath, Catalog catalog)
	{
		Object obj = Resources.Load(filePath);
		TextAsset val = (TextAsset)(object)((obj is TextAsset) ? obj : null);
		if ((Object)(object)val != (Object)null)
		{
			using (MemoryStream moStream = new MemoryStream(val.bytes))
			{
				Load(moStream, catalog);
			}
		}
	}

	protected virtual void Load(Stream moStream, Catalog catalog)
	{
		MoFile parsedMoFile = Parser.Parse(moStream);
		Load(parsedMoFile, catalog);
	}

	protected virtual void Load(MoFile parsedMoFile, Catalog catalog)
	{
		foreach (KeyValuePair<string, string[]> translation in parsedMoFile.Translations)
		{
			catalog.Translations.Add(translation.Key, translation.Value);
		}
		if (parsedMoFile.Headers.ContainsKey("Plural-Forms") && PluralRuleGenerator is IPluralRuleTextParser pluralRuleTextParser)
		{
			pluralRuleTextParser.SetPluralRuleText(parsedMoFile.Headers["Plural-Forms"]);
		}
		catalog.PluralRule = PluralRuleGenerator.CreateRule(catalog.CultureInfo);
	}

	protected virtual string FindTranslationFile(CultureInfo cultureInfo, string domain, string localeDir)
	{
		string[] array = new string[3]
		{
			GetFileName(localeDir, domain, cultureInfo.Name.Replace('-', '_')),
			GetFileName(localeDir, domain, cultureInfo.Name),
			GetFileName(localeDir, domain, cultureInfo.TwoLetterISOLanguageName)
		};
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (Resources.Load(text) != (Object)null)
			{
				return text;
			}
		}
		return null;
	}

	protected virtual string GetFileName(string localeDir, string domain, string locale)
	{
		return Path.Combine(localeDir, Path.Combine(locale, Path.Combine("LC_MESSAGES", domain + ".mo")));
	}
}
