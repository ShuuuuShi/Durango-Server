using Durango.UI.Control;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class FactionTalksNode : SelectableWidget
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private GameObject _notification;

	public void Set(Talks talks)
	{
		_titleLabel.text = talks.Title;
		_notification.gameObject.SetActive(!talks.IsRead);
	}
}
