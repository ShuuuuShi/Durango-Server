using System;
using FatigueData;
using Shared.Survival;
using UnityEngine;
using Yaml;

public class HudFatigueMomentum : MonoBehaviour
{
	public Action<HudFatigueMomentum> Disabled;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private Color _goodColor;

	[SerializeField]
	private Color _badColor;

	private AnimationWidget _animWidget;

	public Color GoodColor => _goodColor;

	public Color BadColor => _badColor;

	public AnimationWidget AnimWidget
	{
		get
		{
			if ((Object)(object)_animWidget == (Object)null)
			{
				_animWidget = ((Component)this).GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	public Shared.Survival.FatigueCategory Key { get; set; }

	public bool Valid { get; set; }

	public int Index { get; set; }

	public void Set(FatigueVelocity velocity)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Key = velocity.Category;
		if (velocity.CategoryData != null)
		{
			Yaml.FatigueCategory categoryData = velocity.CategoryData;
			_icon.spriteName = categoryData.icon;
			_icon.color = categoryData.GetColor();
			float num = velocity.Value * 60f;
			_label.color = ((!(num > 0f)) ? _goodColor : _badColor);
			_label.text = num.ToString("+0.#;-0.#;0");
		}
	}

	private void OnDisable()
	{
		if (Disabled != null)
		{
			Disabled(this);
		}
	}
}
