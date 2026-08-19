using System;
using Durango.UI.Popup;

namespace Durango.UI;

public static class UriParser
{
	public const string Prefix = "ui://";

	public static readonly char[] Separator = new char[2] { '/', '\\' };

	private static readonly char[] ArgumentSeparator = new char[1] { '&' };

	private static readonly char[] SplitSeparator = new char[1] { '?' };

	private static readonly char[] ArgumentValueSeparator = new char[1] { '=' };

	private static readonly DictionaryIgnoreCase<string> Arguments = new DictionaryIgnoreCase<string>();

	public static string GetArgument(string key, string defaultValue = null)
	{
		return Arguments.Get(key, defaultValue);
	}

	public static void OpenUri(this IUriInvokable target, string uri)
	{
		if (target == null || string.IsNullOrEmpty(uri))
		{
			return;
		}
		TooltipBase.CloseAll();
		UIManager.MessageBox.Hide();
		Arguments.Clear();
		string[] array = uri.Split(SplitSeparator, 2, StringSplitOptions.RemoveEmptyEntries);
		uri = array[0];
		if (array.Length > 1)
		{
			string[] array2 = array[1].Split(ArgumentSeparator, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array2.Length; i++)
			{
				string[] array3 = array2[i].Split(ArgumentValueSeparator, 2);
				if (array3.Length > 1)
				{
					Arguments[array3[0]] = array3[1];
				}
				else
				{
					Arguments[array3[0]] = null;
				}
			}
		}
		string[] tokens = uri.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
		target.InvokeUri(tokens, 0);
	}
}
