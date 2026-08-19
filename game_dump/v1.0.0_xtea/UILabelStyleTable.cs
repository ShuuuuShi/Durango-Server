using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[ResourcePath("label_style_table")]
public class UILabelStyleTable : ResourceSingleton<UILabelStyleTable>
{
	[Serializable]
	public struct StyleStruct
	{
		public string[] Tags;

		public bool Bold;

		public bool Italic;

		public bool Underline;

		public bool Strikethrough;

		public Color Color;

		public bool IgnoreTint;

		public float FontScale;

		public string Format;

		public string SpriteLabelFormat;

		public void Override(StyleStruct style)
		{
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			Bold |= style.Bold;
			Italic |= style.Italic;
			Underline |= style.Underline;
			Strikethrough |= style.Strikethrough;
			IgnoreTint |= style.IgnoreTint;
			if (style.FontScale != 1f && style.FontScale != 0f)
			{
				FontScale = style.FontScale;
			}
			if (style.Color != Color.clear && style.Color != Color.white)
			{
				Color = style.Color;
			}
			if (!string.IsNullOrEmpty(style.Format))
			{
				Format = style.Format;
			}
			if (!string.IsNullOrEmpty(style.SpriteLabelFormat))
			{
				SpriteLabelFormat = style.SpriteLabelFormat;
			}
		}
	}

	private struct TagStruct
	{
		public int StyleIndex;

		public StyleStruct Style;

		public int Start;

		public int BodyStart;

		public int TextIndex;
	}

	public static UILabel CurrentLabel;

	public static UISpriteLabel CurrentSpriteLabel;

	[SerializeField]
	private List<StyleStruct> _styles;

	private StringBuilder _stringBulider = new StringBuilder();

	private List<TagStruct> _tags = new List<TagStruct>();

	private string _text;

	private int _cursor;

	private int StyleIndex(int start, int len)
	{
		for (int i = 0; i < _styles.Count; i++)
		{
			if (IsEqual(_styles[i].Tags[0], _text, start, len))
			{
				return i;
			}
			if (_styles[i].Tags.Length > 1)
			{
				break;
			}
		}
		return -1;
	}

	private void GetStyle(TagStruct tag, IList<TagStruct> parent, out StyleStruct style)
	{
		style = default(StyleStruct);
		for (int i = 0; i < _styles.Count; i++)
		{
			bool flag = true;
			if (_styles[i].Tags.Length > parent.Count + 1)
			{
				flag = false;
			}
			else
			{
				for (int j = 0; j < _styles[i].Tags.Length; j++)
				{
					TagStruct tagStruct = ((j != 0) ? parent[parent.Count - j] : tag);
					if (!IsEqual(_styles[i].Tags[j], _text, tagStruct.Start + 1, tagStruct.BodyStart - tagStruct.Start - 2))
					{
						flag = false;
					}
				}
			}
			if (flag)
			{
				style.Override(_styles[i]);
			}
		}
	}

	private bool IsEqual(string tag, string text, int start, int len)
	{
		if (tag.Length != len)
		{
			return false;
		}
		for (int i = 0; i < len; i++)
		{
			if (tag[i] != text[start + i])
			{
				return false;
			}
		}
		return true;
	}

	private void StyleToText(StyleStruct style, bool isClose, StringBuilder str)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
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
				str.AppendFormat("[{0}]", NGUIText.EncodeColor(style.Color));
			}
		}
		if (style.FontScale != 0f && style.FontScale != 1f)
		{
			if (isClose)
			{
				str.Append("[/size]");
			}
			else
			{
				str.AppendFormat("[size={0:0}]", (float)CurrentLabel.fontSize * style.FontScale);
			}
		}
	}

	private int CheckStyle(int start, int end, out int tagStart, out int tagLength, out bool isClose)
	{
		tagStart = -1;
		tagLength = -1;
		isClose = false;
		if (start < 0 || end < 0)
		{
			return -1;
		}
		isClose = _text[start + 1] == '/';
		tagStart = start + ((!isClose) ? 1 : 2);
		tagLength = end - tagStart;
		if (tagLength <= 0)
		{
			return -1;
		}
		int num = StyleIndex(tagStart, tagLength);
		if (num == -1)
		{
			return -1;
		}
		return num;
	}

	private void CloseStyle(int style, int tagStart, int tagEnd, StringBuilder str)
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
			string text = ((!((Object)(object)CurrentSpriteLabel == (Object)null) && !string.IsNullOrEmpty(style2.SpriteLabelFormat)) ? style2.SpriteLabelFormat : style2.Format);
			if (!string.IsNullOrEmpty(text))
			{
				int textIndex = _tags[num3].TextIndex;
				int num4 = str.Length - textIndex;
				string arg = str.ToString(textIndex, num4);
				str.Remove(textIndex, num4);
				str.AppendFormat(text, arg);
			}
			StyleToText(style2, isClose: true, str);
			_tags.RemoveAt(num3);
		}
	}

	public string ReplaceStyle(string text)
	{
		_text = text;
		_cursor = 0;
		int num = -1;
		StringBuilder stringBuilder = null;
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
				int tagStart;
				int tagLength;
				bool isClose;
				int num2 = CheckStyle(num, i, out tagStart, out tagLength, out isClose);
				if (num2 != -1)
				{
					if (isClose)
					{
						CloseStyle(num2, num, i, stringBuilder);
					}
					else
					{
						if (stringBuilder == null)
						{
							stringBuilder = _stringBulider;
							stringBuilder.Remove(0, stringBuilder.Length);
						}
						if (num - _cursor > 0)
						{
							stringBuilder.Append(text, _cursor, num - _cursor);
						}
						_cursor = i + 1;
						TagStruct tagStruct = default(TagStruct);
						tagStruct.StyleIndex = num2;
						tagStruct.Start = num;
						tagStruct.BodyStart = i + 1;
						TagStruct tagStruct2 = tagStruct;
						GetStyle(tagStruct2, _tags, out var style);
						tagStruct2.Style = style;
						StyleToText(style, isClose: false, stringBuilder);
						tagStruct2.TextIndex = stringBuilder.Length;
						_tags.Add(tagStruct2);
					}
				}
				num = -1;
				break;
			}
			}
		}
		while (_tags.Count > 0)
		{
			CloseStyle(_tags[_tags.Count - 1].StyleIndex, _text.Length, _text.Length, stringBuilder);
		}
		if (_cursor > 0 && _cursor < _text.Length)
		{
			stringBuilder.Append(_text, _cursor, _text.Length - _cursor);
		}
		return (_cursor != 0) ? stringBuilder.ToString() : text;
	}
}
