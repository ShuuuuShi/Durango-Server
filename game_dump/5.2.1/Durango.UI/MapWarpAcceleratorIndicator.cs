using Messages;
using Shared.Accelerator;
using UnityEngine;

namespace Durango.UI;

public class MapWarpAcceleratorIndicator : MapIndicator
{
	[SerializeField]
	private UILabel _phaseLabel;

	[SerializeField]
	private UILabel _participantsCountLabel;

	public void SetInfo(WarpAcceleratorInfo info)
	{
		SetTarget(info.Tile);
		AcceleratorStatus status = info.Warpaccelerator.Status;
		int num = ((status == AcceleratorStatus.Processing || status == AcceleratorStatus.Intermission) ? info.Warpaccelerator.CurrentPhase : 0);
		if (num > 0)
		{
			_phaseLabel.transform.parent.gameObject.SetActive(value: true);
			_phaseLabel.text = num.ToString();
		}
		else
		{
			_phaseLabel.transform.parent.gameObject.SetActive(value: false);
		}
		int size = KUtility.GetSize(info.Warpaccelerator.Participants);
		if (size > 0)
		{
			_participantsCountLabel.transform.parent.gameObject.SetActive(value: true);
			_participantsCountLabel.text = size.ToString();
		}
		else
		{
			_participantsCountLabel.transform.parent.gameObject.SetActive(value: false);
		}
	}
}
