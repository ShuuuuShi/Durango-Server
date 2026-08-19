using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using NestedPrefab;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("PioneerPoint")]
public class PioneerGradeManageGroup : UIBase
{
	[SerializeField]
	private NestedPrefabLinker _pioneerInfoLinker;

	[SerializeField]
	private NestedPrefabLinker _itemListLinker;

	[SerializeField]
	private ItemInfoContainer _itemInfo;

	[SerializeField]
	private GameObject _buttonContainer;

	[SerializeField]
	private SelectableButton _useButton;

	[SerializeField]
	private SelectableButton _detailButton;

	[SerializeField]
	private GameObject _amplifierOn;

	[SerializeField]
	private GameObject _amplifierOff;

	[SerializeField]
	private UILabel _paidRemainTimeLabel;

	[SerializeField]
	private UILabel _rateDescription;

	private bool _isLastGrade;

	private PioneerInfoWidget _pioneerInfoWidget;

	private readonly List<ItemData> _validItems = new List<ItemData>();

	private readonly List<ItemData> _invalidItems = new List<ItemData>();

	private ItemList _itemList;

	private Artifact _artifact;

	private void Awake()
	{
		_itemList = _itemListLinker.Object.GetComponent<ItemList>();
		_itemList.SelectableCount = -1;
		ItemList itemList = _itemList;
		itemList.OnUpdateSelectItem = (Action)Delegate.Combine(itemList.OnUpdateSelectItem, new Action(OnUpdateSelectItem));
		_itemList.OnLongPress = _itemList.DefaultLongPress;
		_pioneerInfoWidget = _pioneerInfoLinker.Object.GetComponent<PioneerInfoWidget>();
		_pioneerInfoWidget.Clicked += delegate
		{
			PioneerGradeRewardsPopup pioneerGradeRewardsPopup = UIManager.Popup.FindTooltip<PioneerGradeRewardsPopup>();
			pioneerGradeRewardsPopup.Show();
		};
		_pioneerInfoWidget.RateChanged += UpdateRateDescription;
		_useButton.CanClickWhenDisabled = true;
		SelectableButton useButton = _useButton;
		useButton.Clicked = (Action)Delegate.Combine(useButton.Clicked, (Action)delegate
		{
			if (_useButton.Disabled)
			{
				UIManager.SystemMsg(T._("더 이상 전송할 수 없습니다."));
			}
			else if (_artifact == null)
			{
				UIManager.SystemMsg(T._("개인 통신소를 찾을 수 없습니다."));
			}
			else
			{
				List<ItemData> list = _itemList.SelectedList;
				if (list.Count != 0)
				{
					string mainText;
					if (list.Count == 1)
					{
						mainText = list[0].SafeLevel switch
						{
							SafeLevel.Locked => T._("<em>잠금</em> 설정된 <em>{0}</em>{0:-을} 전송하시겠습니까?", list[0].Name), 
							SafeLevel.Protected => T._("<em>임무</em> 수행에 필요한 <em>{0}</em>{0:-을} 전송하시겠습니까?", list[0].Name), 
							_ => T._("<em>{0}</em>{0:-을} 전송하시겠습니까?", list[0].Name), 
						};
					}
					else
					{
						ItemData itemData = list.MaxBy((ItemData x) => x.SafeLevel);
						mainText = (itemData?.SafeLevel ?? SafeLevel.None) switch
						{
							SafeLevel.Locked => T._("<em>잠금</em> 설정된 <em>{0}</em> 외 {1}개 물품을 전송하시겠습니까?", itemData.Name, list.Count - 1), 
							SafeLevel.Protected => T._("<em>임무</em> 수행에 필요한 <em>{0}</em> 외 {1}개 물품을 전송하시겠습니까?", itemData.Name, list.Count - 1), 
							_ => T._("<em>{0}</em> 외 {1}개 물품을 전송하시겠습니까?", list[0].Name, list.Count - 1), 
						};
					}
					UIManager.MessageBox.Show(mainText, delegate(bool ok)
					{
						if (ok)
						{
							_useButton.Disabled = true;
							EstateSystem.UseItemsForPioneerPoint(_artifact, list.Select((ItemData x) => x.Id).ToArray(), delegate(bool succeeded)
							{
								_useButton.Disabled = _isLastGrade;
								if (succeeded)
								{
									_itemList.DeselectAllItems(sendEvent: true);
									_pioneerInfoWidget.SetNextItemPoints(0f, immediately: true);
								}
							});
						}
					});
				}
			}
		});
		SelectableButton detailButton = _detailButton;
		detailButton.Clicked = (Action)Delegate.Combine(detailButton.Clicked, (Action)delegate
		{
			PioneerPointPopup pioneerPointPopup = UIManager.Popup.FindTooltip<PioneerPointPopup>();
			pioneerPointPopup.Show();
		});
		base.OnOpenSucceed += delegate
		{
			GameSystem<EstateSystem>.Instance().PioneerGradeInfoUpdated += OnPioneerGradeInfoUpdated;
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnUpdateInventory;
			OnPioneerGradeInfoUpdated(GameSystem<EstateSystem>.Instance().PioneerGradeInfo);
			OnUpdateInventory();
			OnUpdateSelectItem();
			_pioneerInfoWidget.Refresh();
		};
		base.OnCloseSucceed += delegate
		{
			_pioneerInfoWidget.SetNextItemPoints(0f, immediately: true);
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdateInventory;
		};
		TryClose();
	}

	private void UpdateRateDescription()
	{
		float lastRate = _pioneerInfoWidget.LastRate;
		PioneerGradeInfo pioneerGradeInfo = GameSystem<EstateSystem>.Instance().PioneerGradeInfo;
		string text = T._("신호증폭기 보유중  <bar/>  전송 효율 <em>{0:P0}</em>", lastRate);
		string text2 = ((!(lastRate > 0f)) ? T._("전송 효율 <em>{0:P0}</em>", lastRate) : T._("전송 효율 <em>{0:P0}</em>  <bar/>  전송 효율 높이기 <help>{1}</help>", lastRate, T._("신호증폭기 구입시 추가 포인트 획득이 가능합니다<br>8</br><ref>ui://Shop/Commodity/signal_amplifier_package,구입하기</ref>")));
		_rateDescription.text = ((!pioneerGradeInfo.IsPaid()) ? text2 : text);
	}

	[ExposedInEditor(null)]
	public void Open(Artifact artifact)
	{
		_artifact = artifact;
		base.Open();
	}

	private void OnUpdateSelectItem()
	{
		float points = _itemList.SelectedList.Sum((ItemData x) => x.PioneerCost);
		_pioneerInfoWidget.SetNextItemPoints(points);
		ItemData lastClickedItem = _itemList.LastClickedItem;
		string notUsableMsg = GetNotUsableMsg(lastClickedItem);
		if (notUsableMsg != null)
		{
			UIManager.SystemMsg("PioneerPointWarning", notUsableMsg);
			return;
		}
		ItemData lastSelectedItem = _itemList.LastSelectedItem;
		if (lastSelectedItem == null)
		{
			_itemInfo.Hide();
			_buttonContainer.SetActive(value: false);
		}
		else
		{
			_itemInfo.Show(lastSelectedItem);
			_buttonContainer.SetActive(value: true);
			_useButton.Disabled = _isLastGrade;
		}
		_detailButton.Disabled = lastSelectedItem != null;
	}

	private static string GetNotUsableMsg(ItemData item)
	{
		if (item == null)
		{
			return null;
		}
		if (item.PioneerCost <= 0f)
		{
			return T._("<em>개척 재료</em> 속성이 없는 아이템은 전송할 수 없습니다.");
		}
		if (!item.Tradable)
		{
			return T._("거래할 수 없는 아이템은 전송할 수 없습니다.");
		}
		if (!item.Dumpable)
		{
			return T._("버릴 수 없는 아이템은 전송할 수 없습니다.");
		}
		if (item.IsEquipments)
		{
			return T._("착용중인 아이템은 전송할 수 없습니다.");
		}
		if (item.IsDestroyed())
		{
			return T._("내구도가 다 된 아이템은 전송할 수 없습니다.");
		}
		int currentAccessLevel = GameSystem<EstateSystem>.Instance().PioneerGradeInfo.CurrentAccessLevel;
		TagData tagData = item.GetTagData("pioneering_material");
		if (tagData == null || tagData.Level > currentAccessLevel)
		{
			return T._("개척도가 부족해 개척 재료 {0:lv:} 이하 아이템만 전송할 수 있습니다.", currentAccessLevel);
		}
		return null;
	}

	private void OnPioneerGradeInfoUpdated(PioneerGradeInfo info)
	{
		_isLastGrade = Singleton<Pioneer>.Instance.GetNextGradePoint(info.Grade) <= 0;
		if (_isLastGrade)
		{
			_useButton.Disabled = true;
		}
		_amplifierOn.SetActive(info.IsPaid());
		_amplifierOff.SetActive(!info.IsPaid());
		double? paymentEndsAt = info.PaymentEndsAt;
		if (paymentEndsAt.HasValue)
		{
			_paidRemainTimeLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				SyncString.UpdateRemainTimeMsg(info.PaymentEndsAt.Value, T._("{0} 남음"), out text, out period, string.Empty);
			}));
		}
		else
		{
			_paidRemainTimeLabel.text = string.Empty;
		}
		UpdateRateDescription();
		OnUpdateInventory();
	}

	private void OnUpdateInventory()
	{
		_validItems.Clear();
		_invalidItems.Clear();
		foreach (ItemData playerItem in GameSystem<InventorySystem>.Instance().PlayerItemList)
		{
			if (GetNotUsableMsg(playerItem) == null)
			{
				_validItems.Add(playerItem);
			}
			else
			{
				_invalidItems.Add(playerItem);
			}
		}
		_itemList.SetItemList(new ItemList.SetStruct[2]
		{
			new ItemList.SetStruct
			{
				List = _validItems
			},
			new ItemList.SetStruct
			{
				List = _invalidItems,
				OnInit = delegate(ItemIconWidget icon)
				{
					icon.IconMode = ItemIconWidget.Mode.Disabled;
				}
			}
		});
	}
}
