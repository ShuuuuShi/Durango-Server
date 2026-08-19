using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat;

public class SmartFormatter
{
	private SmartSettings settings;

	public List<ISource> SourceExtensions { get; private set; }

	public List<IFormatter> FormatterExtensions { get; private set; }

	public Parser Parser { get; set; }

	public ErrorAction ErrorAction { get; set; }

	public SmartSettings Settings => settings ?? (settings = new SmartSettings());

	public SmartFormatter()
		: this(ErrorAction.Ignore)
	{
	}

	public SmartFormatter(ErrorAction errorAction)
	{
		Parser = new Parser(errorAction);
		ErrorAction = errorAction;
		SourceExtensions = new List<ISource>();
		FormatterExtensions = new List<IFormatter>();
	}

	[Obsolete("Please use the specific overloads of AddExtensions().")]
	public void AddExtensions(params object[] extensions)
	{
		foreach (object item in extensions.Reverse())
		{
			ISource source = item as ISource;
			IFormatter formatter = item as IFormatter;
			if (source == null && formatter == null)
			{
				throw new ArgumentException($"{item.GetType().FullName} does not implement ISource nor IFormatter.", "extensions");
			}
			if (source != null)
			{
				SourceExtensions.Insert(0, source);
			}
			if (formatter != null)
			{
				FormatterExtensions.Insert(0, formatter);
			}
		}
	}

	public void AddExtensions(params ISource[] sourceExtensions)
	{
		SourceExtensions.InsertRange(0, sourceExtensions);
	}

	public void AddExtensions(params IFormatter[] formatterExtensions)
	{
		FormatterExtensions.InsertRange(0, formatterExtensions);
	}

	public T GetSourceExtension<T>() where T : class, ISource
	{
		return SourceExtensions.OfType<T>().First();
	}

	public T GetFormatterExtension<T>() where T : class, IFormatter
	{
		return FormatterExtensions.OfType<T>().First();
	}

	public string Format(string format, params object[] args)
	{
		return Format(null, format, args);
	}

	public string Format(IFormatProvider provider, string format, params object[] args)
	{
		StringOutput stringOutput = new StringOutput(format.Length + args.Length * 8);
		Format format2 = Parser.ParseFormat(format);
		object current = ((args == null || args.Length <= 0) ? args : args[0]);
		FormatDetails formatDetails = new FormatDetails(this, format2, args, null, provider, stringOutput);
		Format(formatDetails, format2, current);
		return stringOutput.ToString();
	}

	public void FormatInto(IOutput output, string format, params object[] args)
	{
		Format format2 = Parser.ParseFormat(format);
		object current = ((args == null || args.Length <= 0) ? args : args[0]);
		FormatDetails formatDetails = new FormatDetails(this, format2, args, null, null, output);
		Format(formatDetails, format2, current);
	}

	public string FormatWithCache(ref FormatCache cache, string format, params object[] args)
	{
		StringOutput stringOutput = new StringOutput(format.Length + args.Length * 8);
		if (cache == null)
		{
			cache = new FormatCache(Parser.ParseFormat(format));
		}
		object current = ((args == null || args.Length <= 0) ? args : args[0]);
		FormatDetails formatDetails = new FormatDetails(this, cache.Format, args, cache, null, stringOutput);
		Format(formatDetails, cache.Format, current);
		return stringOutput.ToString();
	}

	public void FormatWithCacheInto(ref FormatCache cache, IOutput output, string format, params object[] args)
	{
		if (cache == null)
		{
			cache = new FormatCache(Parser.ParseFormat(format));
		}
		object current = ((args == null || args.Length <= 0) ? args : args[0]);
		FormatDetails formatDetails = new FormatDetails(this, cache.Format, args, cache, null, output);
		Format(formatDetails, cache.Format, current);
	}

	private void Format(FormatDetails formatDetails, Format format, object current)
	{
		FormattingInfo formattingInfo = new FormattingInfo(formatDetails, format, current);
		Format(formattingInfo);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public void Format(FormattingInfo formattingInfo)
	{
		CheckForExtensions();
		foreach (FormatItem item in formattingInfo.Format.Items)
		{
			if (item is LiteralText literalText)
			{
				formattingInfo.Write(literalText.baseString, literalText.startIndex, literalText.endIndex - literalText.startIndex);
				continue;
			}
			Placeholder placeholder = (Placeholder)item;
			FormattingInfo formattingInfo2 = formattingInfo.CreateChild(placeholder);
			try
			{
				EvaluateSelectors(formattingInfo2);
			}
			catch (Exception innerException)
			{
				int startIndex = ((placeholder.Format == null) ? placeholder.Selectors.Last().endIndex : placeholder.Format.startIndex);
				FormatError(item, innerException, startIndex, formattingInfo2);
				continue;
			}
			try
			{
				EvaluateFormatters(formattingInfo2);
			}
			catch (Exception innerException2)
			{
				int startIndex2 = ((placeholder.Format == null) ? placeholder.Selectors.Last().endIndex : placeholder.Format.startIndex);
				FormatError(item, innerException2, startIndex2, formattingInfo2);
			}
		}
	}

	private void FormatError(FormatItem errorItem, Exception innerException, int startIndex, FormattingInfo formattingInfo)
	{
		switch (ErrorAction)
		{
		case ErrorAction.Ignore:
			break;
		case ErrorAction.ThrowError:
			throw (innerException as FormattingException) ?? new FormattingException(errorItem, innerException, startIndex);
		case ErrorAction.OutputErrorInResult:
			formattingInfo.FormatDetails.FormattingException = (innerException as FormattingException) ?? new FormattingException(errorItem, innerException, startIndex);
			formattingInfo.Write(innerException.Message);
			formattingInfo.FormatDetails.FormattingException = null;
			break;
		case ErrorAction.MaintainTokens:
			formattingInfo.Write(formattingInfo.Placeholder.RawText);
			break;
		}
	}

	private void CheckForExtensions()
	{
		if (SourceExtensions.Count == 0)
		{
			throw new InvalidOperationException("No source extensions are available.  Please add at least one source extension, such as the DefaultSource.");
		}
		if (FormatterExtensions.Count == 0)
		{
			throw new InvalidOperationException("No formatter extensions are available.  Please add at least one formatter extension, such as the DefaultFormatter.");
		}
	}

	private void EvaluateSelectors(FormattingInfo formattingInfo)
	{
		Placeholder placeholder = formattingInfo.Placeholder;
		bool flag = true;
		using List<Selector>.Enumerator enumerator = placeholder.Selectors.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Selector selector = (formattingInfo.Selector = enumerator.Current);
			formattingInfo.Result = null;
			bool flag2 = InvokeSourceExtensions(formattingInfo);
			if (flag2)
			{
				formattingInfo.CurrentValue = formattingInfo.Result;
			}
			if (flag)
			{
				flag = false;
				FormattingInfo formattingInfo2 = formattingInfo;
				while (!flag2 && formattingInfo2.Parent != null)
				{
					formattingInfo2 = formattingInfo2.Parent;
					formattingInfo2.Selector = selector;
					formattingInfo2.Result = null;
					flag2 = InvokeSourceExtensions(formattingInfo2);
					if (flag2)
					{
						formattingInfo.CurrentValue = formattingInfo2.Result;
					}
				}
			}
			if (!flag2)
			{
				throw formattingInfo.FormattingException($"Could not evaluate the selector \"{selector.RawText}\"", selector);
			}
		}
	}

	private bool InvokeSourceExtensions(FormattingInfo formattingInfo)
	{
		int count = SourceExtensions.Count;
		for (int i = 0; i < count; i++)
		{
			ISource source = SourceExtensions[i];
			if (source.TryEvaluateSelector(formattingInfo))
			{
				return true;
			}
		}
		return false;
	}

	private void EvaluateFormatters(FormattingInfo formattingInfo)
	{
		if (!InvokeFormatterExtensions(formattingInfo))
		{
			throw formattingInfo.FormattingException("No suitable Formatter could be found", formattingInfo.Format);
		}
	}

	private bool InvokeFormatterExtensions(FormattingInfo formattingInfo)
	{
		string formatterName = formattingInfo.Placeholder.FormatterName;
		int count = FormatterExtensions.Count;
		for (int i = 0; i < count; i++)
		{
			IFormatter formatter = FormatterExtensions[i];
			if (formatter.Names.Contains(formatterName) && formatter.TryEvaluateFormat(formattingInfo))
			{
				return true;
			}
		}
		return false;
	}
}
