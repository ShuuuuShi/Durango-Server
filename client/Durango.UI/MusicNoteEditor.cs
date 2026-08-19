using System;
using Durango.Logic.Music;
using Durango.Render.Camera;
using UnityEngine;

namespace Durango.UI;

public class MusicNoteEditor : MonoBehaviour
{
	public Action<Note, int, Note, int> Changed;

	[SerializeField]
	private GameObject _leftButton;

	[SerializeField]
	private GameObject _rightButton;

	[SerializeField]
	private UILabel _nameLabel;

	private bool _isShow;

	private Music _music;

	private Note _begin;

	private Note _end;

	private int _changeBegin;

	private int _changeEnd;

	private Vector3 _dragTouchPos;

	private int _dragBeginTick;

	private int _dragEndTick;

	private Vector3 _leftButtonTouchPos;

	private int _leftButtonTouchedTick;

	private Vector3 _rightButtonTouchPos;

	private int _rightButtonTouchedTick;

	private bool _isDirtyMinTick;

	private int _minTick = -1;

	private bool _isDirtyMaxTick;

	private int _maxTick = -1;

	private float _tickWidth;

	private Vector3 _basePosition;

	public Note? Current
	{
		get
		{
			if (_isShow)
			{
				return _begin;
			}
			return null;
		}
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_leftButton);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragLeftButton));
		UIEventListener uIEventListener2 = UIEventListener.Get(_leftButton);
		uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, new UIEventListener.BoolDelegate(OnPressLeftButton));
		UIEventListener uIEventListener3 = UIEventListener.Get(_rightButton);
		uIEventListener3.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener3.onDrag, new UIEventListener.VectorDelegate(OnDragRightButton));
		UIEventListener uIEventListener4 = UIEventListener.Get(_rightButton);
		uIEventListener4.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener4.onPress, new UIEventListener.BoolDelegate(OnPressRightButton));
		if (!_isShow)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void Show(Music music, Note note, Vector3 pos)
	{
		if (_isShow && _begin.Midi == note.Midi && _begin.Tick == note.Tick)
		{
			return;
		}
		int num = -1;
		for (int num2 = music.Notes.Count - 1; num2 >= 0; num2--)
		{
			Note note2 = music.Notes[num2];
			if (note2.Tick <= note.Tick)
			{
				break;
			}
			if (note2.Midi == note.Midi && !note2.On)
			{
				num = num2;
			}
		}
		Note end;
		if (num == -1 || music.Notes[num].Tick > note.Tick + music.Division)
		{
			Note note3 = default(Note);
			note3.Midi = note.Midi;
			note3.Tick = note.Tick + music.Division;
			note3.Volume = 0f;
			note3.On = false;
			end = note3;
		}
		else
		{
			end = music.Notes[num];
		}
		FinishEdit();
		_isShow = true;
		_music = music;
		_begin = note;
		_end = end;
		_changeBegin = _begin.Tick;
		_changeEnd = _end.Tick;
		_isDirtyMinTick = true;
		_isDirtyMaxTick = true;
		_tickWidth = 480f / (float)_music.Division;
		_nameLabel.text = MusicManager.GetNoteName(note.Midi, sharps: true, showOctave: false);
		base.gameObject.SetActive(value: true);
		_basePosition = pos;
		UpdatePosition();
	}

	public void Hide()
	{
		if (_isShow)
		{
			FinishEdit();
			_isShow = false;
			base.gameObject.SetActive(value: false);
		}
	}

	private void FinishEdit()
	{
		if (_isShow)
		{
			int num = _changeBegin;
			int num2 = _changeEnd;
			if (_changeBegin >= _changeEnd)
			{
				num = -1;
				num2 = -1;
			}
			else if (_changeBegin + _music.Division < _changeEnd)
			{
				num2 = -1;
			}
			if ((_begin.Tick != num || _end.Tick != num2) && Changed != null)
			{
				Changed(_begin, num, _end, num2);
			}
		}
	}

	private void CheckRemovedNote()
	{
		if (_changeBegin >= _changeEnd)
		{
			Hide();
		}
	}

	private void OnPress(bool press)
	{
		if (!press)
		{
			CheckRemovedNote();
			return;
		}
		Vector3 dragTouchPos = MainCamera.ScreenPosToNGUIPos(UICamera.lastEventPosition);
		_dragTouchPos = dragTouchPos;
		_dragBeginTick = _changeBegin;
		_dragEndTick = _changeEnd;
	}

	private void OnDrag(Vector2 delta)
	{
		float num = MainCamera.ScreenPosToNGUIPos(UICamera.lastEventPosition).x - _dragTouchPos.x;
		int num2 = (int)(num / 30f) * MusicSheetEditor.TickCountPerTerm;
		ChangeBegin(_dragBeginTick + num2);
		ChangeEnd(_dragEndTick + num2);
		UpdatePosition();
	}

	private void OnPressLeftButton(GameObject obj, bool press)
	{
		if (!press)
		{
			CheckRemovedNote();
			return;
		}
		Vector3 leftButtonTouchPos = MainCamera.ScreenPosToNGUIPos(UICamera.lastEventPosition);
		_leftButtonTouchPos = leftButtonTouchPos;
		_leftButtonTouchedTick = _changeBegin;
	}

	private void OnDragLeftButton(GameObject obj, Vector2 delta)
	{
		float num = MainCamera.ScreenPosToNGUIPos(UICamera.lastEventPosition).x - _leftButtonTouchPos.x;
		int num2 = (int)(num / 30f) * MusicSheetEditor.TickCountPerTerm;
		ChangeBegin(_leftButtonTouchedTick + num2);
		UpdatePosition();
	}

	private void OnPressRightButton(GameObject obj, bool press)
	{
		if (!press)
		{
			CheckRemovedNote();
			return;
		}
		Vector3 rightButtonTouchPos = MainCamera.ScreenPosToNGUIPos(UICamera.lastEventPosition);
		_rightButtonTouchPos = rightButtonTouchPos;
		_rightButtonTouchedTick = _changeEnd;
	}

	private void OnDragRightButton(GameObject obj, Vector2 delta)
	{
		float num = MainCamera.ScreenPosToNGUIPos(UICamera.lastEventPosition).x - _rightButtonTouchPos.x;
		int num2 = (int)(num / 30f) * MusicSheetEditor.TickCountPerTerm;
		ChangeEnd(_rightButtonTouchedTick + num2);
		UpdatePosition();
	}

	private void ChangeBegin(int tick)
	{
		int minTick = GetMinTick();
		minTick = ((minTick != -1) ? Mathf.Max(minTick, _changeEnd - _music.Division) : (_changeEnd - _music.Division));
		tick = Mathf.Max(tick, minTick);
		int changeBegin = _changeBegin;
		if (changeBegin != tick)
		{
			_changeBegin = tick;
		}
	}

	private void ChangeEnd(int tick)
	{
		int maxTick = GetMaxTick();
		maxTick = ((maxTick != -1) ? Mathf.Min(maxTick, _changeBegin + _music.Division) : (_changeBegin + _music.Division));
		tick = Mathf.Min(tick, maxTick);
		int changeEnd = _changeEnd;
		if (changeEnd != tick)
		{
			_changeEnd = tick;
		}
	}

	private void UpdatePosition()
	{
		int num = _changeBegin - _begin.Tick;
		int num2 = _changeEnd - _end.Tick;
		if (_changeBegin < _changeEnd)
		{
			Vector3 localPosition = _basePosition + Vector3.right * ((float)num * _tickWidth);
			base.transform.localPosition = localPosition;
			int width = (int)((float)(_end.Tick - _begin.Tick - num + num2) * _tickWidth);
			UIWidget component = GetComponent<UIWidget>();
			component.width = width;
			component.alpha = 1f;
			UIUtility.UpdateAnchors(base.transform);
		}
		else
		{
			GetComponent<UIWidget>().alpha = 0f;
		}
	}

	private int GetMinTick()
	{
		if (_isDirtyMinTick)
		{
			Note begin = _begin;
			int num = -1;
			for (int i = 0; i < _music.Notes.Count; i++)
			{
				Note note = _music.Notes[i];
				if (note.Tick > begin.Tick)
				{
					break;
				}
				if (note.Midi == begin.Midi)
				{
					if (note.Tick == begin.Tick && note.On == begin.On)
					{
						break;
					}
					num = i;
				}
			}
			if (num == -1)
			{
				_minTick = -1;
			}
			else if (_music.Notes[num].On)
			{
				_minTick = _music.Notes[num].Tick + _music.Division;
			}
			else
			{
				_minTick = _music.Notes[num].Tick;
			}
			_isDirtyMinTick = false;
		}
		return _minTick;
	}

	private int GetMaxTick()
	{
		if (_isDirtyMaxTick)
		{
			Note end = _end;
			int num = -1;
			for (int num2 = _music.Notes.Count - 1; num2 >= 0; num2--)
			{
				Note note = _music.Notes[num2];
				if (note.Tick < end.Tick)
				{
					break;
				}
				if (note.Midi == end.Midi)
				{
					if (note.Tick == end.Tick && note.On == end.On)
					{
						break;
					}
					num = num2;
				}
			}
			if (num == -1)
			{
				_maxTick = -1;
			}
			else
			{
				_maxTick = _music.Notes[num].Tick;
			}
			_isDirtyMaxTick = false;
		}
		return _maxTick;
	}
}
