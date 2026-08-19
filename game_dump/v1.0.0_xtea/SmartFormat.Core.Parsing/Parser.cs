using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing;

public class Parser
{
	private bool alphanumericSelectors;

	private string allowedSelectorChars = string.Empty;

	private string operators = string.Empty;

	private bool alternativeEscaping;

	private char alternativeEscapeChar = '\\';

	private char openingBrace = '{';

	private char closingBrace = '}';

	public ErrorAction ErrorAction { get; set; }

	public Parser(ErrorAction errorAction)
	{
		ErrorAction = errorAction;
	}

	public void AddAlphanumericSelectors()
	{
		alphanumericSelectors = true;
	}

	public void AddAdditionalSelectorChars(string chars)
	{
		foreach (char c in chars)
		{
			if (allowedSelectorChars.IndexOf(c) == -1)
			{
				allowedSelectorChars += c;
			}
		}
	}

	public void AddOperators(string chars)
	{
		foreach (char c in chars)
		{
			if (operators.IndexOf(c) == -1)
			{
				operators += c;
			}
		}
	}

	public void UseAlternativeEscapeChar(char alternativeEscapeChar)
	{
		this.alternativeEscapeChar = alternativeEscapeChar;
		alternativeEscaping = true;
	}

	public void UseBraceEscaping()
	{
		alternativeEscaping = false;
	}

	public void UseAlternativeBraces(char opening, char closing)
	{
		openingBrace = opening;
		closingBrace = closing;
	}

	public Format ParseFormat(string format)
	{
		Format format2 = new Format(format);
		Format format3 = format2;
		Placeholder placeholder = null;
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		ParsingErrors parsingErrors = new ParsingErrors(format2);
		char c = openingBrace;
		char c2 = closingBrace;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int i = 0;
		for (int length = format.Length; i < length; i++)
		{
			char c3 = format[i];
			if (placeholder == null)
			{
				if (c3 == c)
				{
					if (i != num5)
					{
						format3.Items.Add(new LiteralText(format3, num5)
						{
							endIndex = i
						});
					}
					num5 = i + 1;
					if (!alternativeEscaping)
					{
						int num8 = num5;
						if (num8 < length && format[num8] == c)
						{
							i++;
							continue;
						}
					}
					num4++;
					placeholder = new Placeholder(format3, i, num4);
					format3.Items.Add(placeholder);
					format3.HasNested = true;
					num6 = i + 1;
					num7 = 0;
					num = -1;
				}
				else if (c3 == c2)
				{
					if (i != num5)
					{
						format3.Items.Add(new LiteralText(format3, num5)
						{
							endIndex = i
						});
					}
					num5 = i + 1;
					if (!alternativeEscaping)
					{
						int num9 = num5;
						if (num9 < length && format[num9] == c2)
						{
							i++;
							continue;
						}
					}
					if (format3.parent == null)
					{
						parsingErrors.AddIssue(format3, "Format string has too many closing braces", i, i + 1);
						continue;
					}
					num4--;
					format3.endIndex = i;
					format3.parent.endIndex = i + 1;
					format3 = format3.parent.parent;
					num = -1;
				}
				else if (alternativeEscaping && c3 == alternativeEscapeChar)
				{
					num = -1;
					int num10 = i + 1;
					if (num10 < length && (format[num10] == c || format[num10] == c2))
					{
						if (i != num5)
						{
							format3.Items.Add(new LiteralText(format3, num5)
							{
								endIndex = i
							});
						}
						num5 = i + 1;
						i++;
					}
				}
				else
				{
					if (num == -1)
					{
						continue;
					}
					switch (c3)
					{
					case '(':
						if (num == i)
						{
							num = -1;
						}
						else
						{
							num2 = i;
						}
						break;
					case ')':
					case ':':
					{
						if (c3 == ')')
						{
							bool flag = num2 != -1;
							int num11 = i + 1;
							bool flag2 = num11 < format.Length && (format[num11] == ':' || format[num11] == c2);
							if (!flag || !flag2)
							{
								num = -1;
								break;
							}
							num3 = i;
							if (format[num11] == ':')
							{
								i++;
							}
						}
						bool flag3 = num == i;
						bool flag4 = num2 != -1 && num3 == -1;
						if (flag3 || flag4)
						{
							num = -1;
							break;
						}
						num5 = i + 1;
						Placeholder parent = format3.parent;
						if (num2 == -1)
						{
							parent.FormatterName = format.Substring(num, i - num);
						}
						else
						{
							parent.FormatterName = format.Substring(num, num2 - num);
							parent.FormatterOptions = format.Substring(num2 + 1, num3 - (num2 + 1));
						}
						format3.startIndex = num5;
						num = -1;
						break;
					}
					}
				}
			}
			else if (operators.IndexOf(c3) != -1)
			{
				if (i != num5)
				{
					placeholder.Selectors.Add(new Selector(format, num5, i, num6, num7));
					num7++;
					num6 = i;
				}
				num5 = i + 1;
			}
			else if (c3 == ':')
			{
				if (i != num5)
				{
					placeholder.Selectors.Add(new Selector(format, num5, i, num6, num7));
				}
				else if (num6 != i)
				{
					parsingErrors.AddIssue(format3, "There are trailing operators in the selector", num6, i);
				}
				num5 = i + 1;
				placeholder.Format = new Format(placeholder, i + 1);
				format3 = placeholder.Format;
				placeholder = null;
				num = num5;
				num2 = -1;
				num3 = -1;
			}
			else if (c3 == c2)
			{
				if (i != num5)
				{
					placeholder.Selectors.Add(new Selector(format, num5, i, num6, num7));
				}
				else if (num6 != i)
				{
					parsingErrors.AddIssue(format3, "There are trailing operators in the selector", num6, i);
				}
				num5 = i + 1;
				num4--;
				placeholder.endIndex = i + 1;
				format3 = placeholder.parent;
				placeholder = null;
			}
			else if (('0' > c3 || c3 > '9') && (!alphanumericSelectors || (('a' > c3 || c3 > 'z') && ('A' > c3 || c3 > 'Z'))) && allowedSelectorChars.IndexOf(c3) == -1)
			{
				parsingErrors.AddIssue(format3, "Invalid character in the selector", i, i + 1);
			}
		}
		if (num5 != format.Length)
		{
			format3.Items.Add(new LiteralText(format3, num5)
			{
				endIndex = format.Length
			});
		}
		if (format3.parent != null || placeholder != null)
		{
			parsingErrors.AddIssue(format3, "Format string is missing a closing brace", format.Length, format.Length);
			format3.endIndex = format.Length;
			while (format3.parent != null)
			{
				format3 = format3.parent.parent;
				format3.endIndex = format.Length;
			}
		}
		if (parsingErrors.HasIssues && ErrorAction == ErrorAction.ThrowError)
		{
			throw parsingErrors;
		}
		return format2;
	}
}
