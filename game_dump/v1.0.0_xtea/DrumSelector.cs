using System;
using System.Collections.Generic;
using UnityEngine;

public class DrumSelector : MonoBehaviour
{
	[SerializeField]
	private GameObject _up;

	[SerializeField]
	private GameObject _down;

	[SerializeField]
	private ListObjectPool _itemLabels;

	[SerializeField]
	private int _labelCount = 2;

	[SerializeField]
	private float _r = 50f;

	private UILabel[] _labels;

	private int _labelHeight;

	private float _currentIndex;

	private bool _isPress;

	private bool _isInit;

	private IList<string> _items;

	public int Index { get; private set; }

	public float R => _r;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_itemLabels.Set(_labelCount * 2 + 1);
			_labels = new UILabel[_itemLabels.Count];
			int i = 0;
			for (int num = _labels.Length; i < num; i++)
			{
				_labels[i] = _itemLabels[i].GetComponent<UILabel>();
			}
			_labelHeight = _itemLabels.BaseObject.GetComponent<UILabel>().height;
		}
	}

	private void OnEnable()
	{
		Index = 0;
	}

	public void Set(IList<string> items)
	{
		Init();
		_items = items;
	}

	public void Refresh()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		int num = ((_items != null) ? (_items.Count - 1) : 0);
		_currentIndex = Mathf.Clamp(_currentIndex, 0f, (float)num);
		Index = Mathf.RoundToInt(_currentIndex);
		if ((Object)(object)_up != (Object)null)
		{
			_up.gameObject.SetActive(Index != num);
		}
		if ((Object)(object)_down != (Object)null)
		{
			_down.gameObject.SetActive(Index != 0);
		}
		int labelCount = _labelCount;
		Vector3 localPosition = _itemLabels.BaseObject.transform.localPosition;
		int labelHeight = _labelHeight;
		float num2 = (float)labelHeight * 360f / ((float)Math.PI * 2f * _r);
		int i = 0;
		for (int num3 = _labels.Length; i < num3; i++)
		{
			UILabel uILabel = _labels[i];
			int num4 = i - labelCount;
			int num5 = Index + num4;
			float num6 = ((float)num4 + ((float)Index - _currentIndex)) * num2;
			float num7 = Mathf.Sin(num6 * ((float)Math.PI / 180f)) * _r;
			Vector3 localPosition2 = localPosition + Vector3.up * num7;
			float alpha = 1f - Mathf.Abs(num6) / 90f;
			((Component)uILabel).transform.localPosition = localPosition2;
			((Component)uILabel).transform.localEulerAngles = Vector3.right * num6;
			uILabel.text = ((num5 >= 0 && num5 <= num) ? _items[num5] : string.Empty);
			uILabel.alpha = alpha;
		}
	}

	public void SetIndex(int index)
	{
		_currentIndex = index;
	}

	private void Update()
	{
		if (!_isPress && _currentIndex != (float)Index)
		{
			float num = (float)Index - _currentIndex;
			float num2 = Mathf.Sign(num);
			float num3 = num2 * 4f * Time.deltaTime;
			if (Mathf.Abs(num) > Mathf.Abs(num3))
			{
				_currentIndex += num3;
			}
			else
			{
				_currentIndex = Index;
			}
			Refresh();
		}
	}

	private void OnPress(bool press)
	{
		_isPress = press;
	}

	private void OnDrag(Vector2 delta)
	{
		_currentIndex -= delta.y / (float)_labelHeight;
		Refresh();
	}

	public int ResizeWidth(int padding = 0)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		if (_items == null)
		{
			return -1;
		}
		UILabel component = _itemLabels.BaseObject.GetComponent<UILabel>();
		component.UpdateNGUIText();
		float num = 0f;
		int i = 0;
		for (int count = _items.Count; i < count; i++)
		{
			NGUIText.rectWidth = 1000000;
			NGUIText.regionWidth = 1000000;
			NGUIText.rectHeight = 1000000;
			NGUIText.regionHeight = 1000000;
			num = Mathf.Max(NGUIText.CalculatePrintedSize(_items[i]).x, num);
		}
		int num2 = (int)num + padding * 2;
		int j = 0;
		for (int num3 = _labels.Length; j < num3; j++)
		{
			_labels[j].width = num2;
			Vector3 localPosition = ((Component)_labels[j]).transform.localPosition;
			localPosition.x = 0f;
			((Component)_labels[j]).transform.localPosition = localPosition;
		}
		return num2;
	}
}
