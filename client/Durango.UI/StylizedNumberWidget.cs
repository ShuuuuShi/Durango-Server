using System.Collections.Generic;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class StylizedNumberWidget : UIWidget
{
	[SerializeField]
	private UISprite _sprite;

	[SerializeField]
	private float _margin;

	private int _initialDepth;

	private string[] _iconKeys = new string[10] { "num_big_0", "num_big_1", "num_big_2", "num_big_3", "num_big_4", "num_big_5", "num_big_6", "num_big_7", "num_big_8", "num_big_9" };

	private ListObjectPool<UISprite> _list = new ListObjectPool<UISprite>();

	protected override void Awake()
	{
		base.Awake();
		_list = new ListObjectPool<UISprite>();
		_list.BaseObject = _sprite;
		_list.UseBase = true;
		_list.Clear();
		_initialDepth = _list.BaseObject.depth;
	}

	public void Set(int value)
	{
		_list.BeginLoad();
		List<UITweener> list = new List<UITweener>();
		string text = string.Format(T.Culture, "{0:N0}", value);
		for (int i = 0; i < text.Length; i++)
		{
			UISprite next = _list.GetNext();
			char c = text[text.Length - i - 1];
			if (c == '.')
			{
				next.spriteName = "num_big_dot";
			}
			else
			{
				next.spriteName = (char.IsNumber(c) ? _iconKeys[c - 48] : "num_big_comma");
			}
			UISpriteData atlasSprite = next.GetAtlasSprite();
			next.depth = _initialDepth + i;
			next.width = atlasSprite.width + atlasSprite.paddingLeft + atlasSprite.paddingRight;
			next.height = atlasSprite.height + atlasSprite.paddingTop + atlasSprite.paddingBottom;
			UITweener component = next.GetComponent<UITweener>();
			if (component != null)
			{
				list.Add(component);
			}
		}
		_list.EndLoad();
		UIUtility.WidgetsReposition(_list, Vector3.left, Vector2.zero, _margin, 0.5f);
		foreach (UITweener item in list)
		{
			item.ResetToBeginning();
			item.PlayForward();
		}
	}
}
