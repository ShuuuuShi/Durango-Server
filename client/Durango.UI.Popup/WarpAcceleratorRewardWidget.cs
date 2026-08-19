using Durango.Logic.Item;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI.Popup;

public class WarpAcceleratorRewardWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _phaseLabel;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _infoLabel;

	private void Start()
	{
		_titleLabel.text = T._("워프 가속 단계");
	}

	public void Set(WarpAcceleratorInfo info)
	{
		_phaseLabel.text = (info.Warpaccelerator.CurrentPhase - 1).ToString();
		Pair<int, int> warpMatterAcquisition = GameSystem<WarpAcceleratorSystem>.Instance().GetWarpMatterAcquisition();
		_infoLabel.text = string.Format("[icon={0}:1.5] <weak>{1}</weak> <bar/> <weak>{2}</weak>", Durango.Logic.Item.Inventory.GetIcon(Currency.WarpMatter), T._("이번 주 획득 가능"), T._("<em>{0:N0}</em>/{1:N0} 개 남음", warpMatterAcquisition.Item1, warpMatterAcquisition.Item2));
	}
}
