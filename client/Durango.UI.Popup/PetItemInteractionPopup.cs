using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Ability;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class PetItemInteractionPopup : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UISprite _portraitSprite;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private UILabel _cageSizeLabel;

	[SerializeField]
	private UIWidget _profileWidget;

	[SerializeField]
	private UILabel _noAnimalLabel;

	[SerializeField]
	private NestedPrefabLinker _domesticStatusWidget;

	[SerializeField]
	private NestedPrefabLinker _taskStatusWidget;

	[SerializeField]
	private UIWidget _ageWidget;

	[SerializeField]
	private UILabel _ageLabel;

	[SerializeField]
	private PetTagsPredictWidget _petFeedTagsPredictWidget;

	[SerializeField]
	private NestedPrefabLinker _itemListLinker;

	[SerializeField]
	private SelectableButton _okBtn;

	[SerializeField]
	private SelectableButton _cancelBtn;

	[SerializeField]
	private SelectableButton _marketBtn;

	[SerializeField]
	private RectLayout _layout;

	private DomesticRatioWidget _domesticStatus;

	private PetTaskProgressWidget _taskStatus;

	private ItemList _itemList;

	private DomesticCage? _domesticCage;

	private DomesticationInfo? _domesticationInfo;

	private KeyValuePair<Messages.Pet, TaskStatus?>? _petTask;

	private Action<ItemData> _onConfirm;

	private Action<List<ItemData>> _onListConfirm;

	private string _buttonClickSound;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_domesticStatus = _domesticStatusWidget.Object.GetComponent<DomesticRatioWidget>();
		_taskStatus = _taskStatusWidget.Object.GetComponent<PetTaskProgressWidget>();
		_itemList = _itemListLinker.Object.GetComponent<ItemList>();
		_itemList.FixedIconSize = true;
		_itemList.OnLongPress = _itemList.DefaultLongPress;
		ItemList itemList = _itemList;
		itemList.OnUpdateSelectItem = (Action)Delegate.Combine(itemList.OnUpdateSelectItem, new Action(OnUpdateSelectItem));
		_cancelBtn.Text = T._("취소");
		_noAnimalLabel.text = T._("동물을 선택하세요.");
		_cancelBtn.Clicked = Hide;
		SelectableButton okBtn = _okBtn;
		okBtn.Clicked = (Action)Delegate.Combine(okBtn.Clicked, new Action(OnConfirm));
		_marketBtn.Icon = "market_icon_search";
		SelectableButton marketBtn = _marketBtn;
		marketBtn.Clicked = (Action)Delegate.Combine(marketBtn.Clicked, new Action(OnClickMarket));
		UIEventListener uIEventListener = UIEventListener.Get(_infoLabel.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			string[] array = null;
			KeyValuePair<Messages.Pet, TaskStatus?>? petTask = _petTask;
			if (petTask.HasValue)
			{
				array = _petTask.Value.Key.Stat.EatableTags;
			}
			DomesticationInfo? domesticationInfo = _domesticationInfo;
			if (domesticationInfo.HasValue)
			{
				array = _domesticationInfo.Value.EatableTags;
			}
			if (KUtility.GetSize(array) != 0)
			{
				List<string> list = new List<string>();
				int i = 0;
				for (int size = KUtility.GetSize(array); i < size; i++)
				{
					Yaml.Tag tag = SingletonDict<string, Yaml.Tag>.Get(array[i]);
					list.Add((tag != null) ? tag.Name.ToString() : array[i]);
				}
				if (KUtility.GetSize(array) != 0)
				{
					UIWidget childSprite = UIUtility.GetChildSprite(_infoLabel, "img_loading_unknown_question1");
					WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
					widgetTooltipControl.Set(null, T._("{0:l:{}|, }", list));
					if (childSprite == null)
					{
						widgetTooltipControl.Show(60f);
					}
					else
					{
						widgetTooltipControl.AutoPosition = false;
						widgetTooltipControl.Show(60f);
						widgetTooltipControl.SetPosition(childSprite, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 20f));
					}
				}
			}
		});
		ResetArguments();
	}

	private void ResetArguments()
	{
		_domesticCage = null;
		_domesticationInfo = null;
		_petTask = null;
		_onConfirm = null;
		_onListConfirm = null;
		_buttonClickSound = null;
		_okBtn.SetClickSound(UISound.ClickType.ButtonDefault);
	}

	protected override void OnClickWidget()
	{
		base.OnClickWidget();
		ItemInfoTooltip itemInfoTooltip = UIManager.Popup.FindTooltip<ItemInfoTooltip>();
		if (itemInfoTooltip.IsVisible)
		{
			itemInfoTooltip.Hide();
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		ItemInfoTooltip itemInfoTooltip = UIManager.Popup.FindTooltip<ItemInfoTooltip>();
		itemInfoTooltip.Hide();
		ResetArguments();
	}

	private void OnUpdateSelectItem()
	{
		ShowSelectedItemInfoPopup();
		if (_domesticCage.HasValue)
		{
			OnUpdateItemByDomesticRein();
		}
		else if (_domesticationInfo.HasValue)
		{
			OnUpdateItemByDomesticationFeed();
		}
		else
		{
			KeyValuePair<Messages.Pet, TaskStatus?>? petTask = _petTask;
			if (petTask.HasValue)
			{
				OnUpdateItemByTaskFeed();
			}
		}
		_okBtn.Disabled = _itemList.SelectedList.Count == 0;
	}

	private void ShowSelectedItemInfoPopup()
	{
		ItemData lastClickedItem = _itemList.LastClickedItem;
		ItemInfoTooltip itemInfoTooltip = UIManager.Popup.Tooltip<ItemInfoTooltip>();
		if (lastClickedItem == null)
		{
			itemInfoTooltip.Hide();
			return;
		}
		itemInfoTooltip.Set(lastClickedItem);
		if (itemInfoTooltip.IsVisible)
		{
			itemInfoTooltip.Refresh();
			return;
		}
		itemInfoTooltip.AutoPosition = false;
		itemInfoTooltip.Show();
		itemInfoTooltip.HideIgnoreParent = base.transform;
		itemInfoTooltip.SetPosition(base.Widget, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-10f, 0f));
	}

	protected override void OnTryConfirmOnModal()
	{
		OnConfirm();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _okBtn;
	}

	protected override SelectableButton GetCancelButton(out bool showShortcut)
	{
		showShortcut = true;
		return _cancelBtn;
	}

	private void OnConfirm()
	{
		if (_itemList.SelectedList.Count != 0)
		{
			if (!string.IsNullOrEmpty(_buttonClickSound))
			{
				SoundManager.PlayEvent(_buttonClickSound);
			}
			if (OnPreConfirm())
			{
				Confirmed();
			}
		}
	}

	private void OnClickMarket()
	{
		KeyValuePair<Messages.Pet, TaskStatus?>? petTask = _petTask;
		if (!petTask.HasValue)
		{
			return;
		}
		Yaml.Pet pet = SingletonDict<int, Yaml.Pet>.Get(_petTask.Value.Key.EntityType);
		if (pet != null)
		{
			string text = string.Empty;
			string type = pet.Type;
			if (type.Equals("Herbivore", StringComparison.OrdinalIgnoreCase))
			{
				text = "feed_herb";
			}
			else if (type.Equals("Carnivore", StringComparison.OrdinalIgnoreCase))
			{
				text = "feed_carni";
			}
			if (!string.IsNullOrEmpty(text))
			{
				MarketGroup marketGroup = UIManager.FindScript<MarketGroup>();
				marketGroup.OpenAndSearch(null, 0, text);
				Hide();
			}
		}
	}

	private bool OnPreConfirm()
	{
		KeyValuePair<Messages.Pet, TaskStatus?>? petTask = _petTask;
		if (!petTask.HasValue)
		{
			return true;
		}
		List<ItemData> selectedList = _itemList.SelectedList;
		Messages.Pet key = _petTask.Value.Key;
		double num = 0.0;
		for (int i = 0; i < selectedList.Count; i++)
		{
			num += (double)PetUtil.GetPetFoodRejuvenatingDays(selectedList[i]);
		}
		double num2 = num * 24.0 * 60.0 * 60.0;
		PetStats stat = key.Stat;
		double num3 = stat.AgingUntil - stat.GrazedAt.GetValueOrDefault(Connections.Frontend.GetPredictedServerTime());
		double num4 = key.Statistics.DerivedAbilities.Get(Derived.LifeSpan, 0f);
		double num5 = Math.Max(0.0, num4 - num3);
		if (num5 > num2)
		{
			return true;
		}
		MessageBox messageBox = UIManager.MessageBox;
		messageBox.AddKeyValueInfo(T._("현재 선택한 영약의 총 회복량"), TimedeltaFormatter.Format(num2, 2, "min"));
		messageBox.AddKeyValueInfo(T._("실제로 적용되는 회복량"), string.Format("<alert>{0}</alert>", TimedeltaFormatter.Format(num5, 2, "min")));
		messageBox.Show(T._("선택한 영약의 회복량이 실제 적용량보다 많습니다.\n그래도 먹이시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				Confirmed();
			}
		}, T._("먹이기"));
		return false;
	}

	private void Confirmed()
	{
		if (_onConfirm != null)
		{
			_onConfirm(_itemList.LastSelectedItem);
		}
		if (_onListConfirm != null)
		{
			_onListConfirm(_itemList.SelectedList);
		}
		Hide();
	}

	public void SetAsReinSelection(DomesticCage cage, [NotNull] Action<ItemData> confirmed)
	{
		ResetArguments();
		_domesticCage = cage;
		_onConfirm = confirmed;
	}

	public void SetAsFeeding(DomesticationInfo targetRein, Action<List<ItemData>> confirmed)
	{
		ResetArguments();
		_domesticationInfo = targetRein;
		_onListConfirm = confirmed;
	}

	public void SetAsFeeding(Messages.Pet pet, TaskStatus? task, Action<List<ItemData>> confirmed)
	{
		ResetArguments();
		_petTask = new KeyValuePair<Messages.Pet, TaskStatus?>(pet, task);
		_onListConfirm = confirmed;
	}

	protected override void FillData()
	{
		if (_domesticCage.HasValue)
		{
			FillDomesticCage(_domesticCage.Value);
		}
		else if (_domesticationInfo.HasValue)
		{
			FillDomesticationInfo(_domesticationInfo.Value);
		}
		else if (_petTask.HasValue)
		{
			FillTaskInfo(_petTask.Value);
		}
		OnUpdateSelectItem();
	}

	protected override void UpdateLayout()
	{
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		base.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
	}

	private void FillDomesticCage(DomesticCage cage)
	{
		_titleLabel.text = T._("동물을 길들이시거나 보관하시겠습니까?");
		_okBtn.Text = T._("동물을 선택하세요");
		_cageSizeLabel.gameObject.SetActive(value: true);
		_domesticStatus.SetBlank();
		_domesticStatusWidget.gameObject.SetActive(value: true);
		_taskStatusWidget.gameObject.SetActive(value: false);
		_petFeedTagsPredictWidget.gameObject.SetActive(value: false);
		_ageWidget.gameObject.SetActive(value: false);
		UpdateAnimalProfile(null);
		DomesticCage c = cage;
		_itemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList, (ItemData data) => data.Reins.HasValue, delegate(ItemIconWidget icon)
		{
			icon.IconMode = ((!IsValidRein(c, icon.Item.Reins)) ? ItemIconWidget.Mode.Disabled : ItemIconWidget.Mode.Enabled);
		});
		_itemList.SelectableCount = 1;
	}

	private void FillDomesticationInfo(DomesticationInfo info)
	{
		_titleLabel.text = T._("먹이를 주시겠습니까?");
		UpdateAnimalProfile(info);
		_domesticStatus.Set(info);
		_domesticStatusWidget.gameObject.SetActive(value: true);
		_taskStatusWidget.gameObject.SetActive(value: false);
		_petFeedTagsPredictWidget.gameObject.SetActive(value: false);
		_ageWidget.gameObject.SetActive(value: false);
		_cageSizeLabel.gameObject.SetActive(value: false);
		_itemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList, PetUtil.GetDomesticationFoodFilter(info.EatableTags));
		_itemList.SelectableCount = -1;
		_okBtn.Text = T._("확인");
	}

	private void FillTaskInfo(KeyValuePair<Messages.Pet, TaskStatus?> info)
	{
		_titleLabel.text = T._("먹이를 주시겠습니까?");
		UpdateAnimalProfile(info.Key);
		_domesticStatusWidget.gameObject.SetActive(value: false);
		if (!info.Value.HasValue)
		{
			_taskStatusWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_taskStatusWidget.gameObject.SetActive(value: true);
			_taskStatus.Set(info.Key, info.Value.Value);
		}
		_ageWidget.gameObject.SetActive(value: true);
		_petFeedTagsPredictWidget.gameObject.SetActive(value: true);
		_cageSizeLabel.gameObject.SetActive(value: false);
		string[] eatableTags = info.Key.Stat.EatableTags;
		_itemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList, PetUtil.GetAnimalFoodFilter(eatableTags));
		_itemList.SelectableCount = -1;
		_okBtn.Text = T._("확인");
	}

	private void OnUpdateItemByTaskFeed()
	{
		KeyValuePair<Messages.Pet, TaskStatus?>? petTask = _petTask;
		if (petTask.HasValue)
		{
			Messages.Pet key = _petTask.Value.Key;
			TaskStatus? value = _petTask.Value.Value;
			List<ItemData> selectedList = _itemList.SelectedList;
			float num = key.Stat.Hungry.Get();
			float num2 = key.Stat.Hungry.Max();
			float num3 = 0f;
			int i = 0;
			for (int size = KUtility.GetSize(selectedList); i < size; i++)
			{
				ItemData item = selectedList[i];
				num3 += PetUtil.GetPetFoodEnergy(item);
			}
			_itemList.UpdateSelectedItems(isScrollToItem: false);
			_petFeedTagsPredictWidget.Set(selectedList);
			double num4 = 0.0;
			for (int j = 0; j < selectedList.Count; j++)
			{
				num4 += (double)PetUtil.GetPetFoodRejuvenatingDays(selectedList[j]);
			}
			_ageLabel.SetText(GetAgeString(key, num4 * 24.0 * 60.0 * 60.0));
			if (value.HasValue)
			{
				double? taskEndTime = Singleton<Constants>.Instance.Pet.GetTaskEndTime(value.Value, selectedList);
				_taskStatus.Set(key, value.Value, taskEndTime);
			}
			_infoLabel.SetText(GetPetDescription(key, num3));
		}
	}

	private void OnUpdateItemByDomesticRein()
	{
		DomesticCage? domesticCage = _domesticCage;
		if (!domesticCage.HasValue)
		{
			return;
		}
		DomesticCage value = _domesticCage.Value;
		ItemData lastClickedItem = _itemList.LastClickedItem;
		if (lastClickedItem == null || !lastClickedItem.Reins.HasValue)
		{
			UpdateAnimalProfile(null);
			_domesticStatus.SetBlank();
			_okBtn.Text = T._("확인");
			return;
		}
		UpdateAnimalProfile(lastClickedItem);
		_domesticStatus.Set(lastClickedItem.Reins.Value);
		if (lastClickedItem.Reins.Value.Size > value.RemainSize)
		{
			UIManager.SystemMsg(T._("축사에 남은 공간이 부족합니다."));
		}
		if (_itemList.LastSelectedItem == null || !_itemList.LastSelectedItem.Reins.HasValue)
		{
			_okBtn.Text = T._("확인");
			return;
		}
		if (_itemList.LastSelectedItem.Reins.Value.Domesticated)
		{
			_okBtn.Text = T._("보관하기");
			return;
		}
		_okBtn.Text = T._("길들이기");
		_buttonClickSound = "ui_button_tame";
		_okBtn.SetClickSound(UISound.ClickType.NoSound);
	}

	private void OnUpdateItemByDomesticationFeed()
	{
		DomesticationInfo? domesticationInfo = _domesticationInfo;
		if (!domesticationInfo.HasValue)
		{
			return;
		}
		DomesticationInfo value = _domesticationInfo.Value;
		double? domesticationEndTime = Singleton<Constants>.Instance.Pet.GetDomesticationEndTime(value, _itemList.SelectedList);
		_domesticStatus.Set(value, domesticationEndTime);
		_infoLabel.text = GetDomesticationDescription(value, _itemList.SelectedList);
		if (domesticationEndTime.HasValue && domesticationEndTime.HasValue && domesticationEndTime.GetValueOrDefault() >= value.DomesticateUntil)
		{
			return;
		}
		ItemData selectedItem = _itemList.LastClickedItem;
		if (selectedItem != null && selectedItem.Equals(_itemList.LastSelectedItem))
		{
			double? domesticationEndTime2 = Singleton<Constants>.Instance.Pet.GetDomesticationEndTime(value, _itemList.SelectedList.Where((ItemData elem) => elem.Id != selectedItem.Id));
			if (domesticationEndTime.HasValue && domesticationEndTime2.HasValue && Math.Abs(domesticationEndTime.Value - domesticationEndTime2.Value) < 1.0)
			{
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				widgetTooltipControl.Set(null, T._("더 이상 효과를 볼 수 없습니다."), 400);
				widgetTooltipControl.Show();
			}
		}
	}

	private void UpdateAnimalProfile(DomesticationInfo info)
	{
		_profileWidget.alpha = 1f;
		_noAnimalLabel.gameObject.SetActive(value: false);
		Animal animal = SingletonDict<int, Animal>.Get(info.EntityType);
		if (animal != null)
		{
			_nameLabel.text = animal.Name;
			_portraitSprite.spriteName = animal.Portrait;
			_infoLabel.text = GetDomesticationDescription(info);
		}
	}

	private void UpdateAnimalProfile([CanBeNull] ItemData item)
	{
		if (item == null)
		{
			_noAnimalLabel.gameObject.SetActive(value: true);
			_profileWidget.alpha = 0f;
			if (_domesticCage.HasValue)
			{
				DomesticCage value = _domesticCage.Value;
				_cageSizeLabel.text = string.Format("{0}  {1}/{2}", T._("축사 공간"), value.Size - value.RemainSize, value.Size);
			}
			return;
		}
		_noAnimalLabel.gameObject.SetActive(value: false);
		_profileWidget.alpha = 1f;
		Yaml.Pet pet = (item.Reins.HasValue ? SingletonDict<int, Yaml.Pet>.Get(item.Reins.Value.PetEntityType) : null);
		Animal animal = ((pet != null) ? SingletonDict<int, Animal>.Get(pet.VehicleEntityType) : null);
		string spriteName = ((animal == null) ? string.Empty : animal.Portrait);
		_nameLabel.text = ((!item.Pet.HasValue) ? item.Name : item.Pet.Value.GetPetName(includeRank: true));
		_portraitSprite.spriteName = spriteName;
		_infoLabel.text = T._("{0:lv:}  <bar/>  {1}", item.Level, T._("크기 {0}", item.Reins.Value.Size));
		if (_domesticCage.HasValue)
		{
			DomesticCage value2 = _domesticCage.Value;
			_cageSizeLabel.text = string.Format("{0}  <em>{1}</em>/{2}", T._("축사 공간"), value2.Size - value2.RemainSize + item.Reins.Value.Size, value2.Size);
		}
	}

	private void UpdateAnimalProfile(Messages.Pet pet)
	{
		_noAnimalLabel.gameObject.SetActive(value: false);
		_profileWidget.alpha = 1f;
		Animal animal = SingletonDict<int, Animal>.Get(pet.GetAnimalType());
		string spriteName = ((animal == null) ? string.Empty : animal.Portrait);
		_nameLabel.text = pet.GetPetName(includeRank: true);
		_portraitSprite.spriteName = spriteName;
		_infoLabel.SetText(GetPetDescription(pet));
	}

	private static bool IsValidRein(DomesticCage cage, Reins? target)
	{
		if (!target.HasValue)
		{
			return false;
		}
		if (target.Value.Size > cage.RemainSize)
		{
			return false;
		}
		return true;
	}

	private static string GetDomesticationDescription(DomesticationInfo info, List<ItemData> items = null)
	{
		string text = null;
		float num = Mathf.Min(info.DomesticateSuccessRate, info.DomesticationSuccessMaxRate);
		string arg = $"{num:P0}";
		if (KUtility.GetSize(items) > 0)
		{
			float domesticationProbability = Singleton<Constants>.Instance.Pet.GetDomesticationProbability(info.DomesticationSuccessMaxRate, info.DomesticateSuccessRate, items);
			float num2 = domesticationProbability - num;
			if (num2 > 0f)
			{
				string text2 = $"{domesticationProbability:P0}";
				text = string.Format("{0} {1} [preset=animation_arrow] {2}", T._("길들이기 성공률"), arg, text2.ToEncodedColor(PresetColor.UIYellow));
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = string.Format("{0} {1}", T._("길들이기 성공률"), arg);
		}
		Yaml.Pet pet = SingletonDict<int, Yaml.Pet>.Get(info.PetEntityType);
		return T._("{0:lv:}  <bar/>  {1} [icon=img_loading_unknown_question1]\n{2}", info.Level, (pet != null) ? PetUtil.PetTasteToString(pet.Type) : string.Empty, text);
	}

	private static SyncString GetPetDescription(Messages.Pet pet, float hungryModify = 0f)
	{
		Gauge energy = pet.Stat.Hungry;
		Yaml.Pet pet2 = SingletonDict<int, Yaml.Pet>.Get(pet.EntityType);
		string textFormat = T._("{0:lv:}  <bar/>  {1} [icon=img_loading_unknown_question1]  <bar/>  {2}", pet.Statistics.Level, (pet2 != null) ? PetUtil.PetTasteToString(pet2.Type) : string.Empty, "{0}");
		return new SyncString(delegate(out string text, out float period)
		{
			double currentTime = Gauge.CurrentTime;
			float num = ((energy != null) ? energy.Get(currentTime) : 0f);
			float num2 = ((energy != null) ? energy.Max(currentTime) : 0f);
			float num3 = Mathf.Min(num + hungryModify, num2);
			text = string.Format(arg0: (num < num3) ? $"[51A3C3][icon=pet_energy][-] <em>{num3:0}</em>/{num2:0}" : ((!(num > num3)) ? $"[51A3C3][icon=pet_energy][-] {num3:0}/{num2:0}" : $"[51A3C3][icon=pet_energy][-] <alert>{num3:0}</alert>/{num2:0}"), format: textFormat);
			double? nextChangedAt = Gauge.GetNextChangedAt((energy != null) ? energy.Determination : null, currentTime);
			if (!nextChangedAt.HasValue)
			{
				period = 0f;
			}
			else
			{
				period = (float)(nextChangedAt.Value - currentTime);
			}
		});
	}

	private SyncString GetAgeString(Messages.Pet pet, double modified = 0.0)
	{
		return new SyncString(delegate(out string text, out float period)
		{
			PetStats stat = pet.Stat;
			double valueOrDefault = stat.GrazedAt.GetValueOrDefault(Connections.Frontend.GetPredictedServerTime());
			double num = Math.Max(0.0, stat.AgingUntil - valueOrDefault);
			double num2 = pet.Statistics.DerivedAbilities.Get(Derived.LifeSpan, 0f);
			double num3 = Math.Min(num2, num + modified);
			string text2;
			if (num3 > 0.0)
			{
				text2 = TimedeltaFormatter.Format(num3);
				period = ((!stat.GrazedAt.HasValue) ? TimedeltaFormatter.NextPeriod(num3) : 0f);
			}
			else
			{
				text2 = T._("노화된");
				period = 0f;
			}
			text = string.Format("[icon=pet_time] {0}  <bar/>  {1}", T._("수명"), (!(num3 > num)) ? text2 : $"<em>{text2}</em>");
			if (!(num3 < num2))
			{
				text += " [preset=rect_box?<em>MAX</em>]";
			}
		});
	}
}
