using System;
using System.Collections.Generic;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class UISpriteLabel : UILabel
{
	private struct TextRange
	{
		public static readonly TextRange Invalid = new TextRange
		{
			Begin = -1,
			End = -1
		};

		public int Begin;

		public int End;

		public int Length
		{
			get
			{
				if (Begin < 0)
				{
					return 0;
				}
				return End - Begin + 1;
			}
		}
	}

	private enum IconType
	{
		Sprite,
		Preset
	}

	[Serializable]
	private class Link : ITextRectLayout
	{
		public IconType Type;

		public string Key;

		public UILabel Parent;

		public UIWidget Widget;

		public float AspectRatio;

		public bool Valid;

		public void Set([NotNull] UILabel parent, [NotNull] UIWidget w)
		{
			Parent = parent;
			Widget = w;
			Vector2 localSize = Widget.localSize;
			AspectRatio = localSize.x / localSize.y;
		}

		void ITextRectLayout.Set(Vector2 pos, Vector2 size, Color color)
		{
			Valid = true;
			pos += Parent.FontOffset;
			Transform transform = Widget.transform;
			Vector2 pivotOffset = Widget.pivotOffset;
			Vector3 localPosition = new Vector3(pos.x + size.x * pivotOffset.x, pos.y + size.y * pivotOffset.y);
			transform.transform.localPosition = localPosition;
			Vector3 one = Vector3.one;
			if (Widget.width > 0)
			{
				one.x = size.x / (float)Widget.width;
			}
			if (Widget.height > 0)
			{
				one.y = size.y / (float)Widget.height;
			}
			transform.localScale = one;
			Widget.color = color;
		}
	}

	[HideInInspector]
	[SerializeField]
	private int _spriteDepthOffset = -5;

	[HideInInspector]
	[SerializeField]
	private List<Link> _links = new List<Link>();

	private readonly List<UIWidget> _invalidWidgets = new List<UIWidget>();

	private int _iconCount;

	public override Color color
	{
		get
		{
			return mColor;
		}
		set
		{
			if (mColor != value)
			{
				bool num = mColor != value;
				mColor = value;
				if (num)
				{
					MarkAsChanged();
				}
			}
		}
	}

	public override int depth
	{
		get
		{
			return base.depth;
		}
		set
		{
			base.depth = value;
			int i = 0;
			for (int count = _links.Count; i < count; i++)
			{
				if ((bool)_links[i].Widget)
				{
					_links[i].Widget.depth = value + _spriteDepthOffset;
				}
			}
		}
	}

	private void ClearUnusedIconObjects()
	{
		for (int i = _iconCount; i < _links.Count; i++)
		{
			if (_links[i].Widget != null)
			{
				_invalidWidgets.Add(_links[i].Widget);
			}
		}
		if (_iconCount < _links.Count)
		{
			_links.RemoveRange(_iconCount, _links.Count - _iconCount);
		}
		for (int j = 0; j < _invalidWidgets.Count; j++)
		{
			UIWidget uIWidget = _invalidWidgets[j];
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(uIWidget.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(uIWidget.gameObject);
			}
		}
		_invalidWidgets.Clear();
	}

	private static bool ParseIcon(string text, ref int index, out TextRange icon, out TextRange ratio, out TextRange value, out IconType tagKey)
	{
		icon = TextRange.Invalid;
		ratio = TextRange.Invalid;
		value = TextRange.Invalid;
		tagKey = IconType.Sprite;
		if (text[index] != '[')
		{
			return false;
		}
		if (index + 5 < text.Length && text[index + 1] == 'i' && text[index + 2] == 'c' && text[index + 3] == 'o' && text[index + 4] == 'n' && text[index + 5] == '=')
		{
			tagKey = IconType.Sprite;
			icon.Begin = index + 6;
		}
		else
		{
			if (index + 7 >= text.Length || text[index + 1] != 'p' || text[index + 2] != 'r' || text[index + 3] != 'e' || text[index + 4] != 's' || text[index + 5] != 'e' || text[index + 6] != 't' || text[index + 7] != '=')
			{
				return false;
			}
			tagKey = IconType.Preset;
			icon.Begin = index + 8;
		}
		int num = -1;
		int num2 = 0;
		for (int i = icon.Begin; i < text.Length; i++)
		{
			if (text[i] == '[')
			{
				num2++;
			}
			else if (text[i] == ']')
			{
				if (num2 <= 0)
				{
					num = i;
					break;
				}
				num2--;
			}
		}
		if (num == -1)
		{
			return false;
		}
		for (int j = icon.Begin; j < num - 1; j++)
		{
			if (text[j] == ':')
			{
				if (char.IsDigit(text[j + 1]))
				{
					if (icon.End == -1)
					{
						icon.End = j - 1;
					}
					if (value.Begin != -1)
					{
						value.End = j - 1;
					}
					ratio.Begin = j + 1;
				}
			}
			else if (value.Begin == -1 && text[j] == '?')
			{
				if (icon.End == -1)
				{
					icon.End = j - 1;
				}
				if (ratio.Begin != -1)
				{
					ratio.End = j - 1;
				}
				value.Begin = j + 1;
			}
		}
		if (icon.End == -1)
		{
			icon.End = num - 1;
		}
		if (ratio.Begin != -1 && ratio.End == -1)
		{
			ratio.End = num - 1;
		}
		if (value.Begin != -1 && value.End == -1)
		{
			value.End = num - 1;
		}
		index = num + 1;
		return true;
	}

	public static bool HasCharacter(string text)
	{
		string text2 = NGUIText.StripSymbols(text);
		if (string.IsNullOrEmpty(text2))
		{
			return false;
		}
		for (int i = 0; i < text2.Length; i++)
		{
			if (!ParseIcon(text2, ref i, out var _, out var _, out var _, out var _))
			{
				return true;
			}
		}
		return false;
	}

	private static bool GetPresetWidget(string key, out UIWidget preset)
	{
		return ResourceSingleton<UISpriteManager>.Instance().TryGetPreset(key, out preset);
	}

	private Link GetLink()
	{
		Link link;
		if (_iconCount < _links.Count)
		{
			link = _links[_iconCount];
		}
		else
		{
			link = new Link();
			_links.Add(link);
		}
		_iconCount++;
		return link;
	}

	private void MakeSprite(string key, string spriteName, [CanBeNull] ParamsDictionary param)
	{
		Link link = GetLink();
		bool flag = link.Widget != null;
		if (!flag || link.Type != 0)
		{
			if (flag)
			{
				_invalidWidgets.Add(link.Widget);
			}
			UISprite w = base.gameObject.AddChild<UISprite>();
			link.Type = IconType.Sprite;
			link.Set(this, w);
		}
		UISprite obj = (UISprite)link.Widget;
		obj.depth = depth + _spriteDepthOffset;
		obj.spriteName = spriteName;
		UIBasicSprite.Flip flip = UIBasicSprite.Flip.Nothing;
		UIBasicSprite.Rotate rotate = UIBasicSprite.Rotate.Nothing;
		if (param != null)
		{
			string text = param.Get("flip");
			flip = ((!string.IsNullOrEmpty(text)) ? text.ToEnum(UIBasicSprite.Flip.Nothing) : UIBasicSprite.Flip.Nothing);
			string text2 = param.Get("rotate");
			rotate = ((!string.IsNullOrEmpty(text2)) ? text2.ToEnum(UIBasicSprite.Rotate.Nothing) : UIBasicSprite.Rotate.Nothing);
		}
		obj.flip = flip;
		obj.rotate = rotate;
		link.Widget.gameObject.hideFlags = HideFlags.DontSave;
		link.Key = key;
	}

	private void MakePreset(string key, UIWidget prefab)
	{
		Link link = GetLink();
		bool flag = link.Widget != null;
		bool flag2 = false;
		if (!flag || link.Type != IconType.Preset || !(link.Key == key))
		{
			if (flag)
			{
				_invalidWidgets.Add(link.Widget);
			}
			UIWidget component = base.gameObject.AddChild(prefab.gameObject).GetComponent<UIWidget>();
			flag2 = true;
			link.Type = IconType.Preset;
			link.Set(this, component);
		}
		UIWidget widget = link.Widget;
		int num = depth + _spriteDepthOffset;
		if (flag2)
		{
			UIWidget[] componentsInChildren = widget.GetComponentsInChildren<UIWidget>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].depth += num;
			}
		}
		widget.gameObject.hideFlags = HideFlags.DontSave;
		link.Key = key;
	}

	protected override void OnTextParseStart()
	{
		base.OnTextParseStart();
		for (int i = 0; i < _links.Count; i++)
		{
			Link value = _links[i];
			_links[i] = value;
		}
		_iconCount = 0;
	}

	protected override void OnTextParseFinish()
	{
		base.OnTextParseFinish();
		ClearUnusedIconObjects();
	}

	protected override bool TryTextParse(string str, ref int index, TextBuilder builder, TextBuilder.TextTokens tokens)
	{
		if (base.TryTextParse(str, ref index, builder, tokens))
		{
			return true;
		}
		if (!ParseIcon(str, ref index, out var icon, out var ratio, out var value, out var tagKey))
		{
			return false;
		}
		if (tokens == null)
		{
			return true;
		}
		if (icon.Length <= 0)
		{
			return true;
		}
		string text = str.Substring(icon.Begin, icon.Length);
		bool flag = false;
		switch (tagKey)
		{
		case IconType.Sprite:
		{
			UISpriteData sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(text);
			if (sprite != null)
			{
				flag = true;
				ParamsDictionary param = ParamsDictionary.MakeParams((value.Length <= 0) ? null : str.Substring(value.Begin, value.Length));
				MakeSprite(text, sprite.name, param);
			}
			break;
		}
		case IconType.Preset:
		{
			if (GetPresetWidget(text, out var preset))
			{
				flag = true;
				MakePreset(text, preset);
			}
			break;
		}
		}
		int num = tokens.LastOption.FontSize;
		if (flag)
		{
			if (ratio.Length <= 0 || !float.TryParse(str.Substring(ratio.Begin, ratio.Length), out var result))
			{
				result = 1f;
			}
			Link link = _links[_iconCount - 1];
			UIWidget widget = link.Widget;
			int num2 = num;
			ITextLink component = widget.GetComponent<ITextLink>();
			LinkLayoutOption linkLayoutOption;
			if (component == null)
			{
				widget.SetDimensions((int)((float)num2 * link.AspectRatio), num2);
				linkLayoutOption = default(LinkLayoutOption);
			}
			else
			{
				if (component is ITextLinkWithValue)
				{
					((ITextLinkWithValue)component).SetPresetValue((value.Length <= 0) ? null : str.Substring(value.Begin, value.Length));
				}
				linkLayoutOption = component.UpdateLayout(builder, num2);
				num2 = widget.height;
			}
			Vector2 size = widget.GetSize();
			if (result != 1f)
			{
				linkLayoutOption.Offset = ((float)num2 - (float)num2 * result) * 0.5f + linkLayoutOption.Offset;
				size *= result;
			}
			size.x = Mathf.Min(size.x, builder.Width);
			tokens.Add(new TextBuilder.TextToken
			{
				Type = TextBuilder.TokenType.Link,
				Size = size,
				IsSingle = linkLayoutOption.IsSingle,
				Offset = linkLayoutOption.Offset,
				Link = link
			});
		}
		else
		{
			tokens.Add(new TextBuilder.TextToken
			{
				Type = TextBuilder.TokenType.Space,
				Size = new Vector2(0f, num)
			});
		}
		return true;
	}

	protected override void OnProcessedText(TextBuilder.TextTokens tokens)
	{
		base.OnProcessedText(tokens);
		foreach (Link link in _links)
		{
			link.Valid = false;
		}
		using (TextBuilder textBuilder = GetTextBuilder())
		{
			textBuilder.Build(tokens, color, base.width, null, null, null);
		}
		foreach (Link link2 in _links)
		{
			link2.Widget.visible = link2.Valid;
		}
	}

	public UIWidget GetChildWidget(int index)
	{
		if (index < _links.Count)
		{
			return _links[index].Widget;
		}
		return null;
	}

	public UIWidget GetChildWidget(string key)
	{
		int i = 0;
		for (int size = KUtility.GetSize(_links); i < size; i++)
		{
			if (_links[i].Key == key)
			{
				return _links[i].Widget;
			}
		}
		return null;
	}
}
