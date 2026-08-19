using System.IO;

namespace Newtonsoft.Json.Utilities;

internal static class JavaScriptUtils
{
	public static void WriteEscapedJavaScriptString(TextWriter writer, string value, char delimiter, bool appendDelimiters)
	{
		if (appendDelimiters)
		{
			writer.Write(delimiter);
		}
		if (value != null)
		{
			int num = 0;
			int num2 = 0;
			char[] array = null;
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				string text = c switch
				{
					'\t' => "\\t", 
					'\n' => "\\n", 
					'\r' => "\\r", 
					'\f' => "\\f", 
					'\b' => "\\b", 
					'\\' => "\\\\", 
					'\u0085' => "\\u0085", 
					'\u2028' => "\\u2028", 
					'\u2029' => "\\u2029", 
					'\'' => (delimiter != '\'') ? null : "\\'", 
					'"' => (delimiter != '"') ? null : "\\\"", 
					_ => (c > '\u001f') ? null : StringUtils.ToCharAsUnicode(c), 
				};
				if (text != null)
				{
					if (array == null)
					{
						array = value.ToCharArray();
					}
					if (num2 > 0)
					{
						writer.Write(array, num, num2);
						num2 = 0;
					}
					writer.Write(text);
					num = i + 1;
				}
				else
				{
					num2++;
				}
			}
			if (num2 > 0)
			{
				if (num == 0)
				{
					writer.Write(value);
				}
				else
				{
					writer.Write(array, num, num2);
				}
			}
		}
		if (appendDelimiters)
		{
			writer.Write(delimiter);
		}
	}

	public static string ToEscapedJavaScriptString(string value)
	{
		return ToEscapedJavaScriptString(value, '"', appendDelimiters: true);
	}

	public static string ToEscapedJavaScriptString(string value, char delimiter, bool appendDelimiters)
	{
		int? length = StringUtils.GetLength(value);
		using StringWriter stringWriter = StringUtils.CreateStringWriter((!length.HasValue) ? 16 : length.Value);
		WriteEscapedJavaScriptString(stringWriter, value, delimiter, appendDelimiters);
		return stringWriter.ToString();
	}
}
