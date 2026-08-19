using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class AreaEffectIndicator : MonoBehaviour
{
	[SerializeField]
	private UISprite _effectSprite;

	private float _radius;

	private float _validRadius;

	private AnimationWidget _animWidget;

	public bool FixedScale { get; private set; }

	public MapIndicator Indicator { get; private set; }

	private AnimationWidget AnimWidget => (!(_animWidget == null)) ? _animWidget : (_animWidget = GetComponent<AnimationWidget>());

	private void Start()
	{
		AnimWidget.Widget.alpha = 0f;
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		_effectSprite.gameObject.SetActive(value: true);
		AnimWidget.Alpha = 1f;
	}

	public void Set(MapIndicator ind, float radius, float validRadius, bool fixedScale)
	{
		Indicator = ind;
		_radius = radius;
		_validRadius = validRadius;
		FixedScale = fixedScale;
		int num = (int)(_radius * 2f);
		UIWidget component = GetComponent<UIWidget>();
		component.width = num;
		component.height = num;
		UIUtility.ResizeToSquare(_effectSprite, num);
	}

	public void SetColor(Color color)
	{
		_effectSprite.color = color;
	}

	public bool Check(Vector2 center)
	{
		if (Indicator != null && Indicator.IsValid())
		{
			if (_validRadius > 0f)
			{
				float sqrMagnitude = (center - Indicator.GetTile()).sqrMagnitude;
				if (sqrMagnitude > _validRadius * _validRadius)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		_effectSprite.gameObject.SetActive(value: false);
	}
}
