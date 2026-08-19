using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

public class IndicatorList : UIWidget
{
	private struct Item
	{
		public IndicatorWidget Wiget;

		public float Since;

		public float Position;

		public int Index;

		public bool Finished;
	}

	[SerializeField]
	private IndicatorWidget _baseWidget;

	[Tooltip("최대 개수")]
	[SerializeField]
	private int _maxIndicatorCount;

	[Tooltip("한 줄의 높이")]
	[SerializeField]
	private float _itemHeight;

	[Tooltip("알림이 떠있는 시간길이")]
	[SerializeField]
	private float _duration;

	[Tooltip("알파 페이드 인 시간")]
	[SerializeField]
	private float _fadeIn;

	[Tooltip("알파 페이드 아웃 시간")]
	[SerializeField]
	private float _fadeOut;

	[Tooltip("한 줄을 올라가는데 걸리는 시간")]
	[SerializeField]
	private float _scrollSpeed;

	[Tooltip("스크롤 방향")]
	[SerializeField]
	private bool _isUpScroll = true;

	private ListObjectPool<IndicatorWidget> _indicators;

	private readonly List<Item> _items = new List<Item>();

	private Vector2 _prefixMargin;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_indicators = new ListObjectPool<IndicatorWidget>();
			_indicators.BaseObject = _baseWidget;
			_indicators.UseBase = true;
			_indicators.Clear();
			_prefixMargin = _baseWidget.transform.localPosition;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (Application.isPlaying)
		{
			Init();
		}
	}

	public void Show(string icon, string text, Color iconColor, IndicatorWidget.Gauge? gauge)
	{
		Init();
		Item item = MakeItem(icon, text, iconColor, gauge);
		float num = 0f;
		for (int i = 0; i < _items.Count; i++)
		{
			Item value = _items[i];
			value.Index++;
			_items[i] = value;
			num = Mathf.Min(_items[i].Position, num);
		}
		item.Position = num - 1f;
		_items.Add(item);
	}

	private Item MakeItem(string icon, string text, Color iconColor, IndicatorWidget.Gauge? gauge)
	{
		Item result = default(Item);
		IndicatorWidget indicatorWidget = _indicators.Add();
		indicatorWidget.Set(icon, text, iconColor, gauge);
		result.Wiget = indicatorWidget;
		result.Since = Time.time;
		result.Position = 0f;
		result.Index = 0;
		return result;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying)
		{
			UpdatePosition();
			UpdateAlpha();
			ClearFinishedItem();
		}
	}

	private void UpdatePosition()
	{
		for (int i = 0; i < _items.Count; i++)
		{
			Item value = _items[i];
			float position = value.Position;
			int index = value.Index;
			if (position != (float)index)
			{
				if (_scrollSpeed > 0f)
				{
					float f = (float)index - position;
					float num = 1f / _scrollSpeed * Time.deltaTime;
					position = ((!(Mathf.Abs(f) < num)) ? (position + num * Mathf.Sign(f)) : ((float)index));
				}
				else
				{
					position = index;
				}
				value.Position = position;
			}
			value.Wiget.transform.localPosition = ToPosition(value.Position);
			_items[i] = value;
		}
	}

	private void UpdateAlpha()
	{
		float time = Time.time;
		for (int i = 0; i < _items.Count; i++)
		{
			Item value = _items[i];
			float num = time - value.Since;
			float num2 = _duration - num;
			float a = 1f;
			if (value.Position < 0f)
			{
				a = value.Position + 1f;
			}
			if (_fadeIn > 0f && num < _fadeIn)
			{
				a = Mathf.Min(a, num / _fadeIn);
			}
			if (_fadeOut > 0f && num2 < _fadeOut)
			{
				a = Mathf.Min(a, num2 / _fadeOut);
			}
			if (value.Position > (float)(_maxIndicatorCount - 1))
			{
				a = Mathf.Min(a, 1f - value.Position - (float)(_maxIndicatorCount - 1));
			}
			value.Wiget.alpha = a;
			value.Finished = num2 < 0f || value.Position > (float)_maxIndicatorCount;
			_items[i] = value;
		}
	}

	private void ClearFinishedItem()
	{
		for (int num = _items.Count - 1; num >= 0; num--)
		{
			Item item = _items[num];
			if (item.Finished)
			{
				_items.RemoveAt(num);
				int num2 = _indicators.IndexOf(item.Wiget);
				if (num2 != -1)
				{
					_indicators.Swap(num2, _indicators.Count - 1);
					_indicators.Set(_indicators.Count - 1);
				}
			}
		}
	}

	private Vector3 ToPosition(float value)
	{
		Vector2 prefixMargin = _prefixMargin;
		if (UIManager.IsPortraitScreen)
		{
			prefixMargin.y += 60f;
		}
		Vector2 vector = new Vector2(0f, Mathf.Max(0f, value) * _itemHeight);
		return (!_isUpScroll) ? (prefixMargin - vector) : (prefixMargin + vector);
	}
}
