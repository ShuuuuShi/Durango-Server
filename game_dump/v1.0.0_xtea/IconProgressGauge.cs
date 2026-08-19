using UnityEngine;

public class IconProgressGauge : ProgressGauge
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UISprite _upperSprite;

	[SerializeField]
	private float _hideTime;

	private string _icon;

	private Color _color;

	public void SetIcon(string icon)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		SetIcon(icon, Color.white);
	}

	public void SetIcon(string icon, Color color)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		_icon = icon;
		_color = color;
		_iconSprite.spriteName = _icon;
		_iconSprite.color = color;
	}

	protected override void InitGauge()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(_icon))
		{
			_icon = "icon_question";
		}
		_iconSprite.spriteName = _icon;
		_iconSprite.color = _color;
	}

	protected override void DrawGauge(float ratio)
	{
		_upperSprite.fillAmount = ratio;
	}

	protected override bool EndedGauge(float timer)
	{
		base.Widget.alpha = Mathf.Min(base.Widget.alpha, Mathf.Clamp01(1f - timer / _hideTime));
		return timer > _hideTime;
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		_icon = null;
	}
}
