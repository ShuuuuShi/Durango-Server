using System;
using ItemSystem;
using L10N;
using UnityEngine;

public class ItemCommonInfoWidget : MonoBehaviour
{
	[Serializable]
	private struct DurabilityVisibleOption
	{
		public string Icon;

		public float Ratio;
	}

	[SerializeField]
	private UILabel _itemName;

	[SerializeField]
	private ItemIconTex _iconSprite;

	[SerializeField]
	private UILabel _prototypeNameLabel;

	[SerializeField]
	private UILabel _prototypeLevelLabel;

	[SerializeField]
	private UILabel _durabilityLabel;

	[SerializeField]
	private UISprite _durabilitySprite;

	[SerializeField]
	private UILabel _modifiableLabel;

	[SerializeField]
	private UISprite _modifiableSprite;

	[SerializeField]
	private UIWidget _durabilityWidget;

	[SerializeField]
	private UIWidget _modifiableWidget;

	[SerializeField]
	private UIWidget _equipLevelWidget;

	[SerializeField]
	private UISpriteLabel _equipLevelLabel;

	[SerializeField]
	private DurabilityVisibleOption[] _durabilityVisibleOptions;

	[SerializeField]
	private string _modifiableIconName;

	[SerializeField]
	private string _unmodifiableIconName;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			Array.Sort(_durabilityVisibleOptions, (DurabilityVisibleOption o1, DurabilityVisibleOption o2) => (o1.Ratio > o2.Ratio) ? 1 : (-1));
			UIEventListener.Get(((Component)_durabilityWidget).gameObject).onClick = OnClickDurabilityPanel;
			UIEventListener.Get(((Component)_modifiableWidget).gameObject).onClick = OnClickModifiablePanel;
		}
	}

	public void Set(ItemData item)
	{
		Init();
		int contentCount = item.ContentCount;
		int num = (int)item.GetFloatAttribute("capacity");
		ItemData itemData;
		if (contentCount > 0 && num > 0)
		{
			itemData = item.GetContent(0);
			_itemName.text = $"{itemData.Name} ({item.Name})";
			_prototypeNameLabel.text = T._("{0} {1:lv:}", itemData.PrototypeName, itemData.Level);
			_prototypeLevelLabel.text = $"{contentCount}/{num}";
		}
		else
		{
			itemData = item;
			_itemName.text = itemData.Name;
			_prototypeNameLabel.text = itemData.PrototypeName;
			_prototypeLevelLabel.text = T._("{0:lv:}", itemData.Level);
		}
		_iconSprite.SetIcon(itemData);
		ShowDurability(itemData.Durability);
		ShowModifiableInfo(itemData.ModifiableCount);
		if (item.EquipLevel > 1)
		{
			((Component)_equipLevelWidget).gameObject.SetActive(true);
			_equipLevelLabel.text = string.Format("[icon=icon_make_alert]  {0}", T._("{0:lv:} 이상 장착 가능", item.EquipLevel));
		}
		else
		{
			((Component)_equipLevelWidget).gameObject.SetActive(false);
		}
	}

	private void ShowDurability(Gauge gauge)
	{
		float current = gauge.Get();
		float max = gauge.Max();
		float num = gauge.Ratio();
		int num2 = _durabilityVisibleOptions.Length - 1;
		for (int i = 0; i < _durabilityVisibleOptions.Length - 1; i++)
		{
			if (num < _durabilityVisibleOptions[i].Ratio)
			{
				num2 = i;
				break;
			}
		}
		_durabilitySprite.spriteName = _durabilityVisibleOptions[num2].Icon;
		_durabilityLabel.text = Util.LocalizedDurability(current, max);
	}

	private void ShowModifiableInfo(int modifiableCount)
	{
		_modifiableSprite.spriteName = ((modifiableCount <= 0) ? _unmodifiableIconName : _modifiableIconName);
		_modifiableLabel.text = Util.LocalizedModifiableCount(modifiableCount);
	}

	private void OnClickDurabilityPanel(GameObject go)
	{
		PopupTooltip(T._("내구도"), T._("시간이 지날 수록 점점 줄어듭니다.\n0이 되면 이 아이템은 파괴되어 사라집니다."));
	}

	private void OnClickModifiablePanel(GameObject go)
	{
		PopupTooltip(T._("가공 가능 횟수"), T._("가공이 가능한 회수가 몇 번 남았는지를 뜻합니다.\n0이 되면 이 아이템을 더 이상 가공할 수 없습니다."));
	}

	private void PopupTooltip(string title, string body)
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(title, body);
		widgetTooltipControl.Show(5f);
	}
}
