using System;
using CombatData;
using UnityEngine;

public class ActionProgressGauge : ProgressGauge
{
	[SerializeField]
	private UISprite _upperSprite;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UIWidget _background;

	[SerializeField]
	private UIWidget _gaugeBar;

	private float _width;

	[SerializeField]
	private float _fadeTime;

	private float _enableTime;

	private bool _isDefenseAction;

	public void Set(CombatData.Action action)
	{
		if (action.IsAutoAction())
		{
			_iconSprite.spriteName = GameSystem<EquipSystem>.Instance().Weapon.Icon;
		}
		else
		{
			_iconSprite.spriteName = action.Icon;
		}
		_nameLabel.text = action.Name;
		_isDefenseAction = action.EfxType == EfxType.Defense;
		if (_isDefenseAction)
		{
			_width = _background.width;
			_gaugeBar.alpha = 1f;
			_background.alpha = 1f;
		}
		else
		{
			_gaugeBar.alpha = 0f;
			_background.alpha = 0f;
		}
	}

	protected override void InitGauge()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		base.Widget.alpha = 0f;
		((Component)this).transform.localScale = Vector3.one;
		_enableTime = Time.time;
		PositionOffset = Vector3.up * 215f;
		_width = _background.width;
		_gaugeBar.alpha = 1f;
		_background.alpha = 1f;
	}

	protected override void DrawGauge(float ratio)
	{
		float num = Time.time - _enableTime;
		float alpha = Mathf.Clamp01(num / _fadeTime);
		base.Widget.alpha = alpha;
		_upperSprite.fillAmount = 1f - ratio;
		if (_isDefenseAction)
		{
			_gaugeBar.width = (int)(_width * (1f - ratio));
		}
	}

	protected override bool EndedGauge(float timer)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (base.Timer.IsInterrupt)
		{
			if (timer <= 0.2f)
			{
				float num = Mathf.Clamp01(timer / 0.1f);
				((Component)this).transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.3f, num);
			}
			if (timer > 0.2f && timer <= 1f)
			{
				float num2 = (timer - 0.2f) / 0.8f;
				num2 *= num2;
				((Component)this).transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one * 0.5f, num2);
			}
			if (timer > 0.7f && timer <= 1f)
			{
				float num3 = (timer - 0.7f) / 0.3f;
				base.Widget.alpha = Mathf.Lerp(1f, 0f, num3);
			}
			if (timer <= 1f)
			{
				return false;
			}
			return true;
		}
		float num4 = 1f - Mathf.Clamp01(timer / _fadeTime);
		base.Widget.alpha = num4;
		if (Math.Abs(num4) < float.Epsilon)
		{
			return true;
		}
		return false;
	}
}
