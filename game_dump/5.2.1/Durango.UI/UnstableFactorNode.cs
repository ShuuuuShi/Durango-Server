using System;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class UnstableFactorNode : MonoBehaviour
{
	[Serializable]
	public struct Shape
	{
		public int FontSize;

		public int BgIconSize;

		public Color BgIconColor;

		public Shape Lerp(float value, Shape other)
		{
			int fontSize = (int)Mathf.Lerp(FontSize, other.FontSize, value);
			int bgIconSize = (int)Mathf.Lerp(BgIconSize, other.BgIconSize, value);
			Color bgIconColor = Color.Lerp(BgIconColor, other.BgIconColor, value);
			Shape result = default(Shape);
			result.FontSize = fontSize;
			result.BgIconSize = bgIconSize;
			result.BgIconColor = bgIconColor;
			return result;
		}
	}

	[SerializeField]
	private TweenerPlayer _tweener;

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private GameObject _unstableIcon;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private GameObject _lockIcon;

	[SerializeField]
	private GameObject _missionIcon;

	public void Set([CanBeNull] string unstableFactor)
	{
		bool flag = string.IsNullOrEmpty(unstableFactor);
		_lockIcon.SetActive(flag);
		_label.gameObject.SetActive(!flag);
		_label.text = unstableFactor;
	}

	public void SetEffect(bool value)
	{
		_unstableIcon.SetActive(value);
		if (value)
		{
			_tweener.Play();
			return;
		}
		_tweener.ResetToFirst();
		_tweener.Stop();
	}

	public void SetShape(Shape shape)
	{
		_label.fontSize = shape.FontSize;
		_background.width = shape.BgIconSize;
		_background.height = shape.BgIconSize;
		_background.color = shape.BgIconColor;
	}

	public void SetMission(bool value)
	{
		_missionIcon.SetActive(value);
	}
}
