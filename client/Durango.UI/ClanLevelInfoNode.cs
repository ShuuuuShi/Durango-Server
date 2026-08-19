using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ClanLevelInfoNode : SelectableWidget
{
	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private RectLayout _layout;

	private void OnEnable()
	{
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	public void Set(int level, string description)
	{
		_levelLabel.text = T._("{0:lv:}", level);
		_commentLabel.text = description;
	}
}
