using UnityEngine;

namespace Durango.UI;

public class WarpRushResourceWidget : UIWidget
{
	[SerializeField]
	private UILabel _bonusLabel;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private UISprite _resourceIcon;

	public void Set(int bonus, int count, string spriteName)
	{
		_bonusLabel.text = bonus.ToString("+#;-#;0");
		_bonusLabel.gameObject.SetActive(bonus > 0);
		_countLabel.text = count.ToString();
		_resourceIcon.spriteName = spriteName;
	}
}
