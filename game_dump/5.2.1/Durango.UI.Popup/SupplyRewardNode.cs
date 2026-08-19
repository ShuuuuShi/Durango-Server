using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class SupplyRewardNode : MonoBehaviour
{
	[SerializeField]
	private UILabel _level;

	[SerializeField]
	private UIWidget _itemContainer;

	[SerializeField]
	private SupplyRewardItemWidget _baseItemObject;

	[SerializeField]
	private int _padding;

	private ListObjectPool<SupplyRewardItemWidget> _itemList;

	public void Init()
	{
		_itemList = new ListObjectPool<SupplyRewardItemWidget>
		{
			BaseObject = _baseItemObject,
			UseBase = true
		};
		_itemList.Init(null, _itemContainer.transform);
	}

	public void SetNode(string headerText, List<WarpRushReward> rewardList)
	{
		_level.text = headerText;
		_itemList.BeginLoad();
		foreach (WarpRushReward reward in rewardList)
		{
			_itemList.GetNext().Set(reward);
		}
		_itemList.EndLoad();
		Vector2 vector = UIUtility.WidgetsGridReposition(_itemList, null, Vector2.down, _itemContainer.localCorners[1] - new Vector3(0f, _padding), _itemContainer.width, _baseItemObject.GetComponent<UIWidget>().localSize, _padding, _padding);
		_itemContainer.height = (int)vector.y + _padding * 2;
		GetComponent<RectLayoutComponent>().UpdateLayout();
	}
}
