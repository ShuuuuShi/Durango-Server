using System;
using SmartFormat.Extensions;

namespace SmartFormat;

public static class Smart
{
	private static SmartFormatter _default;

	public static SmartFormatter Default
	{
		get
		{
			if (_default == null)
			{
				_default = CreateDefaultSmartFormat();
			}
			return _default;
		}
		set
		{
			_default = value;
		}
	}

	public static string Format(string format, params object[] args)
	{
		return Default.Format(format, args);
	}

	public static string Format(IFormatProvider provider, string format, params object[] args)
	{
		return Default.Format(provider, format, args);
	}

	public static string Format(string format, object arg0, object arg1, object arg2)
	{
		return Format(format, new object[3] { arg0, arg1, arg2 });
	}

	public static string Format(string format, object arg0, object arg1)
	{
		return Format(format, new object[2] { arg0, arg1 });
	}

	public static string Format(string format, object arg0)
	{
		return Format(format, new object[1] { arg0 });
	}

	public static SmartFormatter CreateDefaultSmartFormat()
	{
		SmartFormatter smartFormatter = new SmartFormatter();
		ListFormatter listFormatter = new ListFormatter(smartFormatter);
		smartFormatter.AddExtensions(listFormatter, new ReflectionSource(smartFormatter), new DictionarySource(smartFormatter), new XmlSource(smartFormatter), new DefaultSource(smartFormatter));
		smartFormatter.AddExtensions(listFormatter, new PluralLocalizationFormatter("en"), new ConditionalFormatter(), new TimeFormatter("en"), new XElementFormatter(), new ChooseFormatter(), new DefaultFormatter());
		return smartFormatter;
	}
}
