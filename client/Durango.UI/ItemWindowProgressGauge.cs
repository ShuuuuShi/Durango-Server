using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using UnityEngine;

namespace Durango.UI;

public class ItemWindowProgressGauge : ProgressGauge
{
	private class IconItemPair
	{
		public ItemIconTex Icon;

		public Transform Transform;

		public UISprite Upper;

		public UISprite Plus;

		public ItemData Item;

		public Vector3 Position;
	}

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private ItemIconTex _itemIcon;

	[SerializeField]
	private UIWidget _ellipsis;

	private const int MaxItemCount = 5;

	private const float HMargin = 10f;

	private const int IconWidth = 130;

	private List<IconItemPair> _itemIconList = new List<IconItemPair>();

	private readonly List<ItemData> _items = new List<ItemData>();

	public void ClearItems()
	{
		_items.Clear();
	}

	public void AddItem(ItemData item)
	{
		_items.Add(item);
	}

	public void AddItems(IList<ItemData> items)
	{
		_items.AddRange(items);
	}

	public void Set(string title)
	{
		if (KUtility.GetSize(_items) == 0)
		{
			return;
		}
		_nameLabel.text = title;
		int num = Mathf.Min(5, _items.Count);
		Vector3 vector = _itemIcon.transform.localPosition + Vector3.left * (num - 1) / 2f * 130f;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			Vector3 vector2 = vector + Vector3.right * (130 * i);
			bool flag = false;
			if (5 < _items.Count)
			{
				if (i == num - 2)
				{
					flag = true;
					_ellipsis.gameObject.SetActive(value: true);
					_ellipsis.alpha = 1f;
					_ellipsis.transform.localPosition = vector2;
				}
				else if (i == num - 1)
				{
					flag = true;
					IconItemPair icon = Get(num2++);
					Set(ref icon, _items[_items.Count - 1], vector2);
				}
			}
			if (!flag)
			{
				if (num2 - 1 >= 0)
				{
					Get(num2 - 1).Plus.gameObject.SetActive(value: true);
				}
				IconItemPair icon2 = Get(num2++);
				Set(ref icon2, _items[i], vector2);
			}
		}
		float f = (float)Mathf.Max(_nameLabel.height, num * 130) + 20f;
		base.Widget.width = Mathf.CeilToInt(f);
		base.Widget.height = _nameLabel.height + 20 + _itemIcon.height + 40;
	}

	private void Set(ref IconItemPair icon, ItemData item, Vector3 pos)
	{
		icon.Item = item;
		icon.Position = pos;
		icon.Transform.localPosition = icon.Position;
		icon.Plus.gameObject.SetActive(value: false);
		UIEventListener uIEventListener = UIEventListener.Get(icon.Icon.gameObject);
		uIEventListener.onPress = OnItemIconTouch;
		icon.Icon.gameObject.SetActive(value: true);
		if (icon.Item == null)
		{
			icon.Icon.SetIcon("icon_question");
		}
		else
		{
			icon.Icon.SetIcon(icon.Item);
		}
	}

	private IconItemPair Get(int index = -1)
	{
		IconItemPair iconItemPair;
		if (index < 0 || _itemIconList.Count <= index)
		{
			iconItemPair = Make();
			_itemIconList.Add(iconItemPair);
		}
		else
		{
			iconItemPair = _itemIconList[index];
		}
		return iconItemPair;
	}

	private IconItemPair Make()
	{
		IconItemPair iconItemPair = new IconItemPair();
		iconItemPair.Icon = base.gameObject.AddChild(_itemIcon.gameObject).GetComponent<ItemIconTex>();
		iconItemPair.Transform = iconItemPair.Icon.transform;
		iconItemPair.Upper = iconItemPair.Transform.Find("Upper").GetComponent<UISprite>();
		iconItemPair.Plus = iconItemPair.Transform.Find("Plus").GetComponent<UISprite>();
		return iconItemPair;
	}

	private void OnItemIconTouch(GameObject go, bool press)
	{
		if (!press)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < _itemIconList.Count; i++)
		{
			if (_itemIconList[i].Icon.gameObject == go)
			{
				num = i;
				break;
			}
		}
		if (num != -1 && _itemIconList[num].Item != null)
		{
			ItemInfoTooltip itemInfoTooltip = UIManager.Popup.Tooltip<ItemInfoTooltip>();
			itemInfoTooltip.Set(_itemIconList[num].Item);
			itemInfoTooltip.Show(_itemIconList[num].Upper, Vector2.up * -20f, 5f);
		}
	}

	protected override void OnEnd()
	{
		int count = _itemIconList.Count;
		for (int i = 0; i < count; i++)
		{
			Object.Destroy(_itemIconList[i].Icon.gameObject);
		}
		_itemIconList.Clear();
	}

	protected override void InitGauge()
	{
		_itemIcon.gameObject.SetActive(value: false);
		_ellipsis.gameObject.SetActive(value: false);
	}

	protected override void DrawGauge(float ratio)
	{
		int count = _itemIconList.Count;
		for (int i = 0; i < count; i++)
		{
			_itemIconList[i].Upper.fillAmount = ratio;
		}
		float num = RemainTime();
		if (num < 0.5f)
		{
			float num2 = Mathf.Clamp01(1f - num / 0.4f);
			for (int j = 0; j < count; j++)
			{
				if (_itemIconList[j].Icon.gameObject.activeSelf)
				{
					_itemIconList[j].Plus.alpha = 0f;
					_itemIconList[j].Transform.localPosition = Vector3.Lerp(_itemIconList[j].Position, Vector3.zero + Vector3.up * _itemIconList[j].Position.y, num2);
					_itemIconList[j].Icon.alpha = 1f - num2;
				}
			}
			_ellipsis.alpha = (1f - num2) * (1f - num2);
			return;
		}
		for (int k = 0; k < count; k++)
		{
			if (_itemIconList[k].Icon.gameObject.activeSelf)
			{
				_itemIconList[k].Plus.alpha = 1f;
				_itemIconList[k].Transform.localPosition = _itemIconList[k].Position;
				_itemIconList[k].Icon.alpha = 1f;
			}
		}
		_ellipsis.alpha = 1f;
	}

	protected override bool EndedGauge(float timer)
	{
		base.Widget.alpha = Mathf.Min(base.Widget.alpha, Mathf.Clamp01(1f - timer / 0.5f));
		return timer > 0.5f;
	}
}
