using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class IconMoveEffectWidget : MonoBehaviour
{
	public enum EffectType
	{
		Loot,
		PutIn
	}

	[Serializable]
	[EnumType(typeof(EffectType))]
	private class EffectOptions : EnumKeyList
	{
		[SerializeField]
		private List<EffectOption> _values;

		public EffectOption Get(EffectType type)
		{
			int num = IndexOf((int)type);
			return (num != -1) ? _values[num] : null;
		}
	}

	[Serializable]
	private class EffectOption
	{
		public AnimationCurve AlphaCurve;

		public AnimationCurve HorizontalCurve;

		public AnimationCurve VerticalCurve;

		public AnimationCurve ScaleCurve;

		public SoundEventType Audio;

		public float Duration;

		public float Delay;
	}

	private struct IconWidget
	{
		public ItemIconTex Icon;

		public float Until;

		public Vector3 Start;

		public Vector3 End;

		public EffectOption Option;
	}

	private struct IconData
	{
		public ItemIcon Icon;

		public Vector3 Start;

		public Vector3 End;

		public EffectType Type;
	}

	[SerializeField]
	private ItemIconTex _icon;

	[SerializeField]
	private EffectOptions _options;

	private readonly List<IconWidget> _iconList = new List<IconWidget>();

	private readonly Stack<ItemIconTex> _iconPool = new Stack<ItemIconTex>();

	private readonly Queue<IconData> _dataQueue = new Queue<IconData>();

	private float _nextAt;

	private UIWidget _widget;

	public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());

	private void Start()
	{
		_icon.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (_nextAt < Time.time && _dataQueue.Count > 0)
		{
			IconData data = _dataQueue.Dequeue();
			Begin(data);
		}
		for (int num = _iconList.Count - 1; num >= 0; num--)
		{
			IconWidget comp = _iconList[num];
			if (!Process(comp))
			{
				ItemIconTex icon = comp.Icon;
				_iconList.RemoveAt(num);
				icon.gameObject.SetActive(value: false);
				PushIcon(icon);
			}
		}
		if (_dataQueue.Count == 0 && _iconList.Count == 0)
		{
			base.enabled = false;
		}
	}

	public void Register(EffectType type, ItemIcon icon, Vector3 start, Vector3 end)
	{
		_dataQueue.Enqueue(new IconData
		{
			Icon = icon,
			Start = start,
			End = end,
			Type = type
		});
		base.enabled = true;
	}

	private void Begin(IconData data)
	{
		ItemIconTex itemIconTex = PopIcon();
		IconWidget iconWidget = default(IconWidget);
		iconWidget.Icon = itemIconTex;
		iconWidget.Until = Time.time;
		iconWidget.Start = data.Start;
		iconWidget.End = data.End;
		iconWidget.Option = _options.Get(data.Type);
		IconWidget item = iconWidget;
		SoundManager.PlayEvent(item.Option.Audio);
		itemIconTex.SetIcon(data.Icon);
		_iconList.Add(item);
		_nextAt = Time.time + item.Option.Delay;
	}

	private bool Process(IconWidget comp)
	{
		float num = Time.time - comp.Until;
		EffectOption option = comp.Option;
		float num2 = num / option.Duration;
		if (num2 > 1f)
		{
			return false;
		}
		float alpha = option.AlphaCurve.Evaluate(num2);
		float x = Mathf.Lerp(comp.Start.x, comp.End.x, option.HorizontalCurve.Evaluate(num2));
		float y = Mathf.Lerp(comp.Start.y, comp.End.y, option.VerticalCurve.Evaluate(num2));
		float num3 = option.ScaleCurve.Evaluate(num2);
		ItemIconTex icon = comp.Icon;
		icon.alpha = alpha;
		icon.transform.localPosition = new Vector2(x, y);
		icon.transform.localScale = Vector3.one * num3;
		return true;
	}

	private ItemIconTex PopIcon()
	{
		ItemIconTex itemIconTex = ((_iconPool.Count <= 0) ? base.gameObject.AddChild(_icon.gameObject).GetComponent<ItemIconTex>() : _iconPool.Pop());
		itemIconTex.gameObject.SetActive(value: true);
		return itemIconTex;
	}

	private void PushIcon(ItemIconTex icon)
	{
		icon.gameObject.SetActive(value: false);
		_iconPool.Push(icon);
	}

	[ExposedInEditor(null)]
	private void TestPlay(EffectType type, Vector3 start, Vector3 end)
	{
		List<ItemData> items = GameSystem<InventorySystem>.Instance().PlayerInventory.Items;
		ItemData itemData = items[UnityEngine.Random.Range(0, items.Count)];
		Register(type, itemData.Icon, start, end);
	}
}
