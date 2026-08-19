using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class InteractionMenuQueueWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private TweenerPlayer _tweenerPlayer;

	public int Count { get; private set; }

	public void SetCount(int count, bool isCurrentlyGathering)
	{
		Count = ((!isCurrentlyGathering) ? count : (count + 1));
		if (Count == 0)
		{
			_countLabel.text = string.Empty;
			if (base.isActiveAndEnabled)
			{
				_tweenerPlayer.Play();
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			_countLabel.text = Count.ToString();
			_tweenerPlayer.Stop();
			_widget.alpha = 1f;
			base.transform.localScale = Vector3.one;
			base.gameObject.SetActive(value: true);
		}
	}
}
