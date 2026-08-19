using Durango.Logic.Notification;
using UnityEngine;

namespace Durango.UI;

public class LockedMenuScrollNotification : UIWidget
{
	[SerializeField]
	private UISprite _background;

	private bool _on;

	private Type _type;

	private bool _resetFlag;

	private float _ratio;

	private float _prevAlpha;

	private float _alpha;

	private Color _prevColor;

	private Color _color;

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			_resetFlag = false;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying && _ratio < 1f)
		{
			float num = Time.deltaTime / 0.2f;
			_ratio = Mathf.Clamp01(_ratio + num);
			alpha = Mathf.Lerp(_prevAlpha, _alpha, _ratio);
			_background.color = Color.Lerp(_prevColor, _color, _ratio);
		}
	}

	public void Set(bool on, Type type)
	{
		bool flag = !_resetFlag;
		_resetFlag = true;
		float alphaValue = ((!on) ? 0f : 1f);
		Color typeColor = Notification.GetTypeColor(type);
		if (flag)
		{
			_on = on;
			_type = type;
			Set(alphaValue, typeColor, instant: true);
		}
		else if (_on != on || _type != type)
		{
			_on = on;
			_type = type;
			Set(alphaValue, typeColor, instant: false);
		}
	}

	private void Set(float alphaValue, Color colorValue, bool instant)
	{
		_alpha = alphaValue;
		_color = colorValue;
		if (instant)
		{
			alpha = _alpha;
			_background.color = _color;
			_ratio = 1f;
		}
		else
		{
			_prevAlpha = alpha;
			_prevColor = _background.color;
			_ratio = 0f;
		}
	}
}
