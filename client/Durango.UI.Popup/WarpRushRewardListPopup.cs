using System.Collections.Generic;
using Durango.Logic;
using Durango.UI.Control;
using L10N;
using Shared.Season2;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class WarpRushRewardListPopup : TooltipBase
{
	[SerializeField]
	private UILabel _title;

	[SerializeField]
	private KScrollView _itemList;

	[SerializeField]
	private SelectableButton _confirmButton;

	private ResourceType _currentResourceType;

	protected override void OnAwake()
	{
		_itemList.Nodes.Init(delegate(GameObject go)
		{
			go.GetComponent<SupplyRewardNode>().Init();
		});
	}

	public void Set(ResourceType resourceType)
	{
		_title.text = T._("{0} 교환 보상품 목록", WarpRushSystem.GetBoxName(resourceType));
		_currentResourceType = resourceType;
		_confirmButton.Clicked = Hide;
	}

	protected override void FillData()
	{
		Dictionary<int, List<WarpRushReward>> dictionary = Singleton<Yaml.WarpRushRewards>.Instance.SupplyRewards.Get(_currentResourceType);
		_itemList.Nodes.BeginLoad();
		foreach (KeyValuePair<int, List<WarpRushReward>> item in dictionary)
		{
			string headerText = T._("[i]<em>{0:lv:}</em>[/i]", item.Key);
			SupplyRewardNode component = _itemList.Nodes.GetNext().GetComponent<SupplyRewardNode>();
			component.SetNode(headerText, item.Value);
		}
		_itemList.Nodes.EndLoad();
		_itemList.UpdateLayout();
	}

	protected override void OnTryConfirmOnModal()
	{
		Hide();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}
}
