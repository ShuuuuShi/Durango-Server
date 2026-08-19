using L10N;
using UnityEngine;

public class ClanLevelInfoNode : MonoBehaviour
{
	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private PressColorChange _pressColorChange;

	public void Set(int level, string description)
	{
		_levelLabel.text = T.Format("{0:lv:}", level);
		_commentLabel.text = description;
	}

	public void SetActiveEffect(bool isActive)
	{
		_pressColorChange.Disable(!isActive);
	}
}
