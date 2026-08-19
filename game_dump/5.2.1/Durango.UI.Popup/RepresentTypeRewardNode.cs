using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class RepresentTypeRewardNode : MonoBehaviour
{
	[SerializeField]
	private UILabel _pointLabel;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UISprite _bg1Sprite;

	[SerializeField]
	private UISprite _bg2Sprite;

	[SerializeField]
	private UISprite _activeSprite;

	private void Start()
	{
		GetComponent<RectLayoutComponent>().UpdateOnSizeChange();
	}

	public void Set(string point, string description)
	{
		_pointLabel.text = point;
		_descriptionLabel.text = description;
	}

	public void SetGaugeRatio(float ratio)
	{
		_bg1Sprite.fillAmount = Mathf.Clamp01(ratio / 0.5f);
		_bg2Sprite.fillAmount = Mathf.Clamp01((ratio - 0.5f) / 0.5f);
		if (ratio < 0.5f)
		{
			_activeSprite.color = new Color(0.137f, 0.125f, 0.09f);
			UILabel pointLabel = _pointLabel;
			Color color = new Color(1f, 1f, 1f, 0.5f);
			_descriptionLabel.color = color;
			pointLabel.color = color;
		}
		else
		{
			_activeSprite.color = _bg1Sprite.color;
			UILabel pointLabel2 = _pointLabel;
			Color uIYellow = PresetColor.UIYellow;
			_descriptionLabel.color = uIYellow;
			pointLabel2.color = uIYellow;
		}
	}
}
