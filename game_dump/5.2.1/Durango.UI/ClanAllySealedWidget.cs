using L10N;
using UnityEngine;

namespace Durango.UI;

public class ClanAllySealedWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _levelLabel;

	public void Set(int level)
	{
		_levelLabel.text = T._("부족 {0:lv:}", level);
	}
}
