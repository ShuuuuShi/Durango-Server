using System.Text;
using Durango.Development;
using L10N;

namespace Durango.Utils;

public class ObjectReferenceText
{
	private object _parent;

	private readonly string _text;

	private string _value;

	public ObjectReferenceText(string text)
		: this(null, text)
	{
	}

	public ObjectReferenceText(object parent, string text)
	{
		_parent = parent;
		_text = text;
		_value = null;
	}

	public void SetParent(object parent)
	{
		_parent = parent;
		_value = null;
	}

	public override string ToString()
	{
		if (_parent == null || string.IsNullOrEmpty(_text))
		{
			return _text;
		}
		if (_value == null)
		{
			_value = string.Empty;
			using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
			int num = 0;
			int num2 = -1;
			StringBuilder value = reusable.Value;
			value.Length = 0;
			int i = 0;
			for (int length = _text.Length; i < length; i++)
			{
				switch (_text[i])
				{
				case '{':
					num2 = i;
					break;
				case '}':
				{
					if (num2 == -1 || i - num2 <= 1)
					{
						break;
					}
					value.Append(_text, num, num2 - num);
					int num3 = num2 + 1;
					int num4 = i - num3;
					int num5 = _text.IndexOf(':', num3, num4);
					string str;
					string text;
					if (num5 == -1)
					{
						str = _text.Substring(num3, num4);
						text = null;
					}
					else
					{
						num4 = num5 - num3;
						str = _text.Substring(num3, num4);
						text = "{0:" + _text.Substring(num5 + 1, i - (num5 + 1)) + "}";
					}
					if (WatchDocs.TryGetValue(str, _parent, out var value2))
					{
						if (value2 != null)
						{
							if (string.IsNullOrEmpty(text))
							{
								value.Append(value2);
							}
							else
							{
								value.AppendFormat(T.Culture, text, value2);
							}
						}
					}
					else
					{
						value.Append(_text, num3, i - num3);
					}
					num = i + 1;
					num2 = -1;
					break;
				}
				}
			}
			if (num > 0 && num < _text.Length)
			{
				value.Append(_text, num, _text.Length - num);
			}
			_value = ((num != 0) ? value.ToString() : _text);
		}
		return _value;
	}
}
