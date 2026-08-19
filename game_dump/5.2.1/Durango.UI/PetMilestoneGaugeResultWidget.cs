using Durango.UI.Control;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class PetMilestoneGaugeResultWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _keyLabel;

	[SerializeField]
	private UILabel _valueLabel;

	[SerializeField]
	private UIWidget _gaugeWidget;

	[SerializeField]
	private UISprite _bgGaugeSprite;

	[SerializeField]
	private UISprite _upperGaugeSprite;

	[SerializeField]
	private UISprite[] _cursorSprites;

	public void Set(string key, string value, float ratio, Color gaugeColor)
	{
		_keyLabel.text = key;
		_valueLabel.text = value;
		_upperGaugeSprite.fillAmount = ratio;
		Vector3 localPosition = _cursorSprites[0].transform.localPosition;
		localPosition.x = _gaugeWidget.localCorners[0].x + (float)_gaugeWidget.width * ratio;
		_cursorSprites[0].transform.localPosition = localPosition;
		_bgGaugeSprite.color = gaugeColor.WithA(_bgGaugeSprite.alpha);
		_upperGaugeSprite.color = gaugeColor;
		for (int i = 0; i < _cursorSprites.Length; i++)
		{
			_cursorSprites[i].color = gaugeColor;
		}
	}

	public void PlayAnimation(float delay)
	{
		GetComponent<TweenerPlayer>().Play(delay);
	}
}
