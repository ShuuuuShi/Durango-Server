using Durango.Logic.Item;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class IconProgressGaugeNode : MonoBehaviour
{
	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UISprite _upperSprite;

	public void SetIcon(ItemIcon icon)
	{
		_iconTexture.SetIcon(icon);
	}

	public void DrawGauge(float ratio)
	{
		_upperSprite.fillAmount = ratio;
	}
}
