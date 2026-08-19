using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class DiscoverMissionNode : MonoBehaviour
{
	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISprite _progressRatio;

	[SerializeField]
	private UILabel _progressLabel;

	[SerializeField]
	private GameObject _lockedIcon;

	public void Set([CanBeNull] string regionName, int percentage, bool isLocked)
	{
		_progressLabel.gameObject.SetActive(!isLocked);
		_lockedIcon.SetActive(isLocked);
		if (isLocked)
		{
			_nameLabel.text = "???";
			_progressRatio.fillAmount = 0f;
			return;
		}
		float num = (float)percentage / 100f;
		_nameLabel.text = regionName;
		_progressRatio.fillAmount = num;
		_progressLabel.text = $"{num:P0}";
	}
}
