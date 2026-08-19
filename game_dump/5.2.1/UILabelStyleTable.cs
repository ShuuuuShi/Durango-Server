using System;
using System.Collections.Generic;
using System.Text;
using Durango.System;
using UnityEngine;

[ResourcePath("label_style_table")]
public class UILabelStyleTable : ResourceSingleton<UILabelStyleTable>
{
	public enum ReplaceType
	{
		None,
		Replace,
		Format
	}

	private enum TagType
	{
		Open,
		Close,
		Volatility
	}

	[Serializable]
	public struct StyleStruct
	{
		public string Tag;

		public bool Bold;

		public bool Italic;

		public bool Underline;

		public bool Strikethrough;

		public Color Color;

		public bool IgnoreTint;

		public ReplaceType ReplaceType;

		[TextArea(1, 3)]
		public string Replace;

		[TextArea(1, 3)]
		public string SpriteLabelReplace;

		public StyleStruct Override(StyleStruct style)
		{
			Bold |= style.Bold;
			Italic |= style.Italic;
			Underline |= style.Underline;
			Strikethrough |= style.Strikethrough;
			IgnoreTint |= style.IgnoreTint;
			if (style.Color != Color.clear && style.Color != Color.white)
			{
				Color = style.Color;
			}
			ReplaceType = style.ReplaceType;
			Replace = style.Replace;
			SpriteLabelReplace = style.SpriteLabelReplace;
			return this;
		}
	}

	private struct TagStruct
	{
		public int StyleIndex;

		public StyleStruct Style;

		public int TextIndex;
	}

	[SerializeField]
	private List<StyleStruct> _styles;

	private readonly List<TagStruct> _tags = new List<TagStruct>();

	private readonly StringBuilder _bulider = new StringBuilder();

	private string _text;

	private int _cursor;

	private int StyleIndex(int start, int len)
	{
		bool flag = len == 4 && string.Compare(_text, start, "coin", 0, len) == 0;
		for (int i = 0; i < _styles.Count; i++)
		{
			if (flag && ((!Platform.Instance.UsePCCoin) ? "mobile_coin" : "pc_coin") == _styles[i].Tag)
			{
				return i;
			}
			if (_styles[i].Tag.Length == len && string.Compare(_text, start, _styles[i].Tag, 0, len) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	private void StyleToText(StyleStruct style, bool isClose, StringBuilder str)
	{
		if (style.IgnoreTint)
		{
			str.Append((!isClose) ? "[c]" : "[/c]");
		}
		if (style.Bold)
		{
			str.Append((!isClose) ? "[b]" : "[/b]");
		}
		if (style.Italic)
		{
			str.Append((!isClose) ? "[i]" : "[/i]");
		}
		if (style.Underline)
		{
			str.Append((!isClose) ? "[u]" : "[/u]");
		}
		if (style.Strikethrough)
		{
			str.Append((!isClose) ? "[s]" : "[/s]");
		}
		if (style.Color != Color.clear)
		{
			if (isClose)
			{
				str.Append("[-]");
			}
			else
			{
				str.AppendFormat("[{0}]", (!(style.Color.a < 1f)) ? NGUIText.EncodeColor(style.Color) : NGUIText.EncodeColor32(style.Color));
			}
		}
	}

	private int CheckStyle(int start, int end, out TagType type)
	{
		if (start < 0 || end < 0)
		{
			type = TagType.Open;
			return -1;
		}
		int num = start + 1;
		int num2 = end;
		if (_text[start + 1] == '/')
		{
			type = TagType.Close;
			num++;
		}
		else if (_text[end - 1] == '/')
		{
			type = TagType.Volatility;
			num2--;
		}
		else
		{
			type = TagType.Open;
		}
		int num3 = num2 - num;
		if (num3 <= 0)
		{
			return -1;
		}
		int num4 = StyleIndex(num, num3);
		if (num4 == -1)
		{
			return -1;
		}
		return num4;
	}

	private void OpenStyle(int style, int tagStart, int tagEnd, StringBuilder str, bool volatility, bool isSpriteLabel)
	{
		if (tagStart - _cursor > 0)
		{
			str.Append(_text, _cursor, tagStart - _cursor);
		}
		_cursor = tagEnd + 1;
		StyleStruct style2 = ((_tags.Count <= 0) ? default(StyleStruct) : _tags[_tags.Count - 1].Style);
		style2.Override(_styles[style]);
		StyleToText(style2, isClose: false, str);
		if (style2.ReplaceType == ReplaceType.Replace)
		{
			string value = ((isSpriteLabel && !string.IsNullOrEmpty(style2.SpriteLabelReplace)) ? style2.SpriteLabelReplace : style2.Replace);
			if (!string.IsNullOrEmpty(value))
			{
				str.Append(value);
			}
		}
		if (volatility)
		{
			StyleToText(style2, isClose: true, str);
			return;
		}
		_tags.Add(new TagStruct
		{
			StyleIndex = style,
			Style = style2,
			TextIndex = str.Length
		});
	}

	private void CloseStyle(int style, int tagStart, int tagEnd, StringBuilder str, bool isSpriteLabel)
	{
		int num = -1;
		for (int num2 = _tags.Count - 1; num2 >= 0; num2--)
		{
			if (_tags[num2].StyleIndex == style)
			{
				num = num2;
				break;
			}
		}
		if (num == -1)
		{
			return;
		}
		if (tagStart - _cursor > 0)
		{
			str.Append(_text, _cursor, tagStart - _cursor);
		}
		_cursor = tagEnd + 1;
		for (int num3 = _tags.Count - 1; num3 >= num; num3--)
		{
			StyleStruct style2 = _tags[num3].Style;
			if (style2.ReplaceType == ReplaceType.Format)
			{
				string text = ((isSpriteLabel && !string.IsNullOrEmpty(style2.SpriteLabelReplace)) ? style2.SpriteLabelReplace : style2.Replace);
				if (!string.IsNullOrEmpty(text))
				{
					int textIndex = _tags[num3].TextIndex;
					int num4 = str.Length - textIndex;
					string arg = str.ToString(textIndex, num4);
					str.Remove(textIndex, num4);
					str.AppendFormat(text, arg);
				}
			}
			StyleToText(style2, isClose: true, str);
			_tags.RemoveAt(num3);
		}
	}

	private string ReplacePresetColor(string text)
	{
		StringBuilder bulider = _bulider;
		bulider.Length = 0;
		int num = 0;
		int i = 0;
		for (int num2 = text.Length - 4; i < num2; i++)
		{
			if (text[i] != '[' || text[i + 1] != 'c' || text[i + 2] != '=')
			{
				continue;
			}
			int num3 = text.IndexOf(']', i + 2);
			if (num3 != -1)
			{
				bulider.Append(text, num, i - num);
				int num4 = i + 3;
				if (PresetColor.TryGet(text.Substring(num4, num3 - num4), out var color))
				{
					bulider.AppendFormat("[{0}]", (!(color.a < 1f)) ? NGUIText.EncodeColor(color) : NGUIText.EncodeColor32(color));
				}
				i = num3;
				num = i + 1;
			}
		}
		if (num > 0 && num < text.Length)
		{
			bulider.Append(text, num, text.Length - num);
		}
		if (num == 0)
		{
			return text;
		}
		return bulider.ToString();
	}

	public string ReplaceStyle(string text, bool isSpriteLabel)
	{
		_text = ReplacePresetColor(text);
		_cursor = 0;
		int num = -1;
		StringBuilder bulider = _bulider;
		bulider.Length = 0;
		int i = 0;
		for (int length = _text.Length; i < length; i++)
		{
			switch (_text[i])
			{
			case '<':
				num = i;
				break;
			case '>':
			{
				if (num == -1)
				{
					break;
				}
				TagType type;
				int num2 = CheckStyle(num, i, out type);
				if (num2 != -1)
				{
					switch (type)
					{
					case TagType.Open:
						OpenStyle(num2, num, i, bulider, volatility: false, isSpriteLabel);
						break;
					case TagType.Close:
						CloseStyle(num2, num, i, bulider, isSpriteLabel);
						break;
					case TagType.Volatility:
						OpenStyle(num2, num, i, bulider, volatility: true, isSpriteLabel);
						break;
					}
				}
				num = -1;
				break;
			}
			}
		}
		while (_tags.Count > 0)
		{
			CloseStyle(_tags[_tags.Count - 1].StyleIndex, _text.Length, _text.Length, bulider, isSpriteLabel);
		}
		if (_cursor > 0 && _cursor < _text.Length)
		{
			bulider.Append(_text, _cursor, _text.Length - _cursor);
		}
		if (_cursor == 0)
		{
			return _text;
		}
		return bulider.ToString();
	}

	public string StripStyle(string text)
	{
		_text = text;
		_cursor = 0;
		int num = -1;
		StringBuilder bulider = _bulider;
		bulider.Length = 0;
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			switch (text[i])
			{
			case '<':
				num = i;
				break;
			case '>':
			{
				if (num == -1)
				{
					break;
				}
				if (CheckStyle(num, i, out var _) != -1)
				{
					if (num - _cursor > 0)
					{
						bulider.Append(_text, _cursor, num - _cursor);
					}
					_cursor = i + 1;
				}
				num = -1;
				break;
			}
			}
		}
		if (_cursor > 0 && _cursor < _text.Length)
		{
			bulider.Append(_text, _cursor, _text.Length - _cursor);
		}
		if (_cursor == 0)
		{
			return text;
		}
		return bulider.ToString();
	}
}
