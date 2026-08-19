using System;
using UnityEngine;

namespace Durango.UI.Control;

public class SeparatorSprite : UIWidget, ITextLinkWithValue, ITextLink
{
	[SerializeField]
	private UISprite _sprite;

	private UILabel _parentLabel;

	private bool _isInitialized;

	private void Initialize()
	{
		if (!_isInitialized)
		{
			_isInitialized = true;
			_parentLabel = UIUtility.FindComponentInParent<UILabel>(base.gameObject);
			UILabel parentLabel = _parentLabel;
			parentLabel.onChange = (Action)Delegate.Combine(parentLabel.onChange, new Action(ParentLabel_onChange));
		}
	}

	private void ParentLabel_onChange()
	{
		_sprite.width = _parentLabel.width;
		UpdateSpritePivot();
	}

	private void OnDestroy()
	{
		if (_parentLabel != null)
		{
			UILabel parentLabel = _parentLabel;
			parentLabel.onChange = (Action)Delegate.Remove(parentLabel.onChange, new Action(ParentLabel_onChange));
		}
	}

	private void UpdateSpritePivot()
	{
		NGUIText.Alignment alignment = _parentLabel.alignment;
		switch (alignment)
		{
		default:
			switch (_parentLabel.pivot)
			{
			case Pivot.TopLeft:
			case Pivot.Left:
			case Pivot.BottomLeft:
				alignment = NGUIText.Alignment.Left;
				break;
			case Pivot.TopRight:
			case Pivot.Right:
			case Pivot.BottomRight:
				alignment = NGUIText.Alignment.Right;
				break;
			default:
				alignment = NGUIText.Alignment.Center;
				break;
			}
			break;
		case NGUIText.Alignment.Left:
		case NGUIText.Alignment.Center:
		case NGUIText.Alignment.Right:
			break;
		}
		switch (alignment)
		{
		case NGUIText.Alignment.Left:
			_sprite.pivot = Pivot.Left;
			break;
		case NGUIText.Alignment.Center:
			_sprite.pivot = Pivot.Center;
			break;
		case NGUIText.Alignment.Right:
			_sprite.pivot = Pivot.Right;
			break;
		}
		_sprite.transform.localPosition = Vector3.zero;
	}

	void ITextLinkWithValue.SetPresetValue(string text)
	{
		Initialize();
		_sprite.spriteName = text;
	}

	LinkLayoutOption ITextLink.UpdateLayout(TextBuilder builder, int size)
	{
		base.height = size;
		UISpriteData atlasSprite = _sprite.GetAtlasSprite();
		int h = 2;
		if (atlasSprite != null)
		{
			h = atlasSprite.height + atlasSprite.paddingBottom + atlasSprite.paddingTop;
		}
		_sprite.SetDimensions(_parentLabel.width, h);
		UpdateSpritePivot();
		LinkLayoutOption result = default(LinkLayoutOption);
		result.IsSingle = true;
		return result;
	}
}
