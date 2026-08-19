using System.Collections.Generic;
using ItemSystem;
using UnityEngine;

public class ItemWindowProgressGauge : ProgressGauge
{
	private class IconItemPair
	{
		public UISprite Icon;

		public Transform Transform;

		public UISprite Upper;

		public GameObject Plus;

		public ItemData Item;

		public Vector3 Position;
	}

	private const int MaxItemCount = 5;

	private const float HMargin = 10f;

	private const int IconWidth = 130;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISprite _itemIcon;

	[SerializeField]
	private GameObject _cancelBtn;

	[SerializeField]
	private UIWidget _ellipsis;

	private List<IconItemPair> _itemIconList = new List<IconItemPair>();

	private void Awake()
	{
		if ((Object)(object)_cancelBtn != (Object)null)
		{
			UIEventListener.Get(_cancelBtn).onClick = delegate
			{
				base.Timer.Stop();
			};
		}
	}

	public void SetData(string name, IList<ItemData> itemList)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		_nameLabel.text = name;
		int width = Mathf.CeilToInt(_nameLabel.printedSize.x + 20f);
		if (itemList == null || itemList.Count == 0)
		{
			base.Widget.height = _nameLabel.height + 20;
			base.Widget.width = width;
			return;
		}
		int num = Mathf.Min(5, itemList.Count);
		Vector3 val = ((Component)_itemIcon).transform.localPosition + Vector3.left * (float)(num - 1) / 2f * 130f;
		((Component)_ellipsis).gameObject.SetActive(false);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			Vector3 val2 = val + Vector3.right * (float)(130 * i);
			bool flag = false;
			if (5 < itemList.Count)
			{
				if (i == num - 2)
				{
					flag = true;
					((Component)_ellipsis).gameObject.SetActive(true);
					_ellipsis.alpha = 1f;
					((Component)_ellipsis).transform.localPosition = val2;
				}
				else if (i == num - 1)
				{
					flag = true;
					IconItemPair icon = Get(num2++);
					Set(ref icon, itemList[itemList.Count - 1], val2);
				}
			}
			if (!flag)
			{
				if (num2 - 1 >= 0)
				{
					Get(num2 - 1).Plus.gameObject.SetActive(true);
				}
				IconItemPair icon2 = Get(num2++);
				Set(ref icon2, itemList[i], val2);
			}
		}
		float num3 = (float)Mathf.Max(_nameLabel.height, num * 130) + 20f;
		base.Widget.width = Mathf.CeilToInt(num3);
		base.Widget.height = _nameLabel.height + 20 + _itemIcon.height + 40;
	}

	private void Set(ref IconItemPair icon, ItemData item, Vector3 pos)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		icon.Item = item;
		icon.Position = pos;
		icon.Transform.localPosition = icon.Position;
		icon.Plus.SetActive(false);
		UIEventListener uIEventListener = UIEventListener.Get(((Component)icon.Icon).gameObject);
		uIEventListener.onPress = OnItemIconTouch;
		((Component)icon.Icon).gameObject.SetActive(true);
		icon.Icon.spriteName = ((icon.Item != null) ? icon.Item.Icon : "icon_question");
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
		iconItemPair.Icon = ((Component)this).gameObject.AddChild(((Component)_itemIcon).gameObject).GetComponent<UISprite>();
		iconItemPair.Transform = ((Component)iconItemPair.Icon).transform;
		iconItemPair.Upper = ((Component)iconItemPair.Transform.FindChild("Upper")).GetComponent<UISprite>();
		iconItemPair.Plus = ((Component)iconItemPair.Transform.FindChild("Plus")).gameObject;
		return iconItemPair;
	}

	private void OnItemIconTouch(GameObject go, bool press)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (!press)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < _itemIconList.Count; i++)
		{
			if ((Object)(object)((Component)_itemIconList[i].Icon).gameObject == (Object)(object)go)
			{
				num = i;
				break;
			}
		}
		if (num != -1 && _itemIconList[num].Item != null)
		{
			ItemInfoPopup itemInfoPopup = UIManager.Popup.Tooltip<ItemInfoPopup>();
			itemInfoPopup.Set(_itemIconList[num].Item);
			itemInfoPopup.Show((UIWidget)_itemIconList[num].Upper, Vector2.up * -20f, 5f);
		}
	}

	protected override void OnEnd()
	{
		int count = _itemIconList.Count;
		for (int i = 0; i < count; i++)
		{
			Object.Destroy((Object)(object)((Component)_itemIconList[i].Icon).gameObject);
		}
		_itemIconList.Clear();
	}

	protected override void InitGauge()
	{
		((Component)_itemIcon).gameObject.SetActive(false);
	}

	protected override void DrawGauge(float ratio)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		int count = _itemIconList.Count;
		for (int i = 0; i < count; i++)
		{
			_itemIconList[i].Upper.fillAmount = ratio;
		}
		float num = RemainTime();
		if (num < 0.5f)
		{
			float num2 = Mathf.Clamp01(1f - num / 0.4f);
			for (int j = 0; j < count && ((Component)_itemIconList[j].Icon).gameObject.activeSelf; j++)
			{
				_itemIconList[j].Plus.SetActive(false);
				_itemIconList[j].Transform.localPosition = Vector3.Lerp(_itemIconList[j].Position, Vector3.zero + Vector3.up * _itemIconList[j].Position.y, num2);
				_itemIconList[j].Icon.alpha = 1f - num2;
			}
			_ellipsis.alpha = (1f - num2) * (1f - num2);
		}
	}

	protected override bool EndedGauge(float timer)
	{
		base.Widget.alpha = Mathf.Min(base.Widget.alpha, Mathf.Clamp01(1f - timer / 0.5f));
		return timer > 0.5f;
	}
}
