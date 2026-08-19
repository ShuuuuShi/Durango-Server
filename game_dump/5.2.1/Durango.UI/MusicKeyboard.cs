using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

public class MusicKeyboard : MonoBehaviour
{
	private static readonly bool[] WhiteFlag = new bool[12]
	{
		true, false, true, false, true, true, false, true, false, true,
		false, true
	};

	public Action<int, bool> KeyboardPressed;

	public Action<bool> SpacePressed;

	[SerializeField]
	private MusicKeyboardItem _white;

	[SerializeField]
	private MusicKeyboardItem _black;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private GameObject _spaceButton;

	[SerializeField]
	private GameObject _leftScrollButton;

	[SerializeField]
	private GameObject _rightScrollButton;

	private int _min;

	private readonly List<MusicKeyboardItem> _keyboards = new List<MusicKeyboardItem>();

	private ListObjectPool<MusicKeyboardItem> _whites;

	private ListObjectPool<MusicKeyboardItem> _blacks;

	private float _centerPosX;

	public bool Disable { get; set; }

	public void Init(int min, int max)
	{
		UIEventListener uIEventListener = UIEventListener.Get(_leftScrollButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			ScrollKeyboard(-1);
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(_rightScrollButton);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
		{
			ScrollKeyboard(1);
		});
		UIEventListener uIEventListener3 = UIEventListener.Get(_spaceButton);
		uIEventListener3.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onPress, (UIEventListener.BoolDelegate)delegate(GameObject go, bool press)
		{
			if (SpacePressed != null)
			{
				SpacePressed(press);
			}
		});
		_whites = new ListObjectPool<MusicKeyboardItem>();
		_whites.BaseObject = _white;
		_whites.Set(0);
		_blacks = new ListObjectPool<MusicKeyboardItem>();
		_blacks.BaseObject = _black;
		_blacks.Set(0);
		_min = min;
		_keyboards.Clear();
		int num = WhiteFlag.Length;
		for (int i = min; i <= max; i++)
		{
			int num2 = i % num;
			MusicKeyboardItem musicKeyboardItem = ((!WhiteFlag[num2]) ? _blacks : _whites).Add();
			musicKeyboardItem.Initialize(i);
			musicKeyboardItem.Pressed += OnPressKey;
			_keyboards.Add(musicKeyboardItem);
		}
		Reposition();
	}

	private void OnEnable()
	{
		_scrollView.ResetPosition();
		_centerPosX = _scrollView.transform.localPosition.x;
	}

	private void ScrollKeyboard(int value)
	{
		Vector3 localPosition = _scrollView.transform.localPosition;
		float num = (Mathf.Round((localPosition.x - _centerPosX) / (float)(_white.width * 7)) - (float)value) * (float)_white.width * 7f;
		Bounds bounds = _scrollView.bounds;
		Vector2 viewSize = _scrollView.panel.GetViewSize();
		float min = bounds.min.x + viewSize.x * 0.5f;
		float max = bounds.max.x - viewSize.x * 0.5f;
		localPosition.x = 0f - Mathf.Clamp(0f - (num + _centerPosX), min, max);
		SpringPanel.Begin(_scrollView.gameObject, localPosition, 8f);
	}

	private void Reposition()
	{
		Vector3 localPosition = _whites.BaseObject.transform.localPosition;
		int num = _whites.BaseObject.width / 2;
		int num2 = WhiteFlag.Length;
		int i = 0;
		for (int count = _keyboards.Count; i < count; i++)
		{
			_keyboards[i].transform.localPosition = localPosition;
			if (WhiteFlag[(_min + i) % num2] == WhiteFlag[(_min + i + 1) % num2])
			{
				localPosition += Vector3.right * num * 2f;
			}
			else
			{
				localPosition += Vector3.right * num;
			}
		}
		_scrollView.ResetPosition();
		_centerPosX = _scrollView.transform.localPosition.x;
	}

	private void OnPressKey(MusicKeyboardItem item, bool press)
	{
		if (!Disable)
		{
			int midi = item.Midi;
			item.Press(press);
			if (KeyboardPressed != null)
			{
				KeyboardPressed(midi, press);
			}
		}
	}

	public void SelectKey(int midi, bool select)
	{
		int num = midi - _min;
		if (num >= 0 && num < _keyboards.Count)
		{
			_keyboards[num].Select(select);
		}
	}

	public void ResetKeyboard()
	{
		for (int i = 0; i < _keyboards.Count; i++)
		{
			_keyboards[i].ResetState();
		}
	}

	public void ClearSelectedKeyboard()
	{
		for (int i = 0; i < _keyboards.Count; i++)
		{
			_keyboards[i].Select(select: false);
		}
	}
}
