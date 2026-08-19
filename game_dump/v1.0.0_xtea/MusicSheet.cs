using System;
using System.Collections.Generic;
using MusicData;
using UnityEngine;

public class MusicSheet : MonoBehaviour
{
	public const float TemperedScaleHeight = 7f;

	public const int StartMidiNote = 45;

	public const int EndMidiNote = 78;

	public Action<int, int> NoteTouched;

	[SerializeField]
	private UIWidget _noteArea;

	[SerializeField]
	private ListObjectPool _splitLines;

	[SerializeField]
	private ListObjectPool _notes;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private Transform _playGuideLine;

	private static readonly int[] NotePositions = new int[60]
	{
		0, 1, -1, 2, -2, 3, 4, -4, 5, -5,
		6, -6, 7, 8, -8, 9, -9, 10, 11, -11,
		12, -12, 13, -13, 14, 15, -15, 16, -16, 17,
		18, -18, 19, -19, 20, -20, 21, 22, -22, 23,
		-23, 24, 25, -25, 26, -26, 27, -27, 28, 29,
		-29, 30, -30, 31, 32, -32, 33, -33, 34, -34
	};

	private UIWidget _widget;

	private Music _music;

	private int _start;

	private int _end;

	private bool _isPress;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_noteArea).gameObject);
		uIEventListener.onPress = OnPressNoteArea;
		uIEventListener.onDrag = OnDragNoteArea;
		((Component)_playGuideLine).gameObject.SetActive(false);
	}

	private void OnPressNoteArea(GameObject go, bool press)
	{
		if (press)
		{
			NotifyCurrentTouchNote();
		}
		_isPress = press;
	}

	private void OnDragNoteArea(GameObject go, Vector2 delta)
	{
		if (_isPress)
		{
			NotifyCurrentTouchNote();
		}
	}

	private void NotifyCurrentTouchNote()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = MainCamera.ScreenPosToNGUIPos(Vector2.op_Implicit(UICamera.lastEventPosition), ((Component)_noteArea).transform);
		int arg = (int)(val.x / (float)_noteArea.width * (float)(_end - _start)) + _start;
		int arg2 = Array.IndexOf(NotePositions, Mathf.FloorToInt(val.y / 7f)) + 45;
		if (NoteTouched != null)
		{
			NoteTouched(arg, arg2);
		}
	}

	public void Refresh(Music music)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		IList<Note> notes = music.Notes;
		int num = (_end - _start) / music.Division;
		_splitLines.Set(num - 1);
		int num2 = _noteArea.width / num;
		int i = 0;
		for (int count = _splitLines.Count; i < count; i++)
		{
			_splitLines[i].transform.localPosition = Vector3.right * (float)((i + 1) * num2);
		}
		_notes.Set(0);
		int num3 = 0;
		int j = 0;
		for (int count2 = notes.Count; j < count2 && notes[j].Tick < _end; j++)
		{
			if (notes[j].Tick >= _start)
			{
				Note note = notes[j];
				if (!(Math.Abs(note.Volume) < float.Epsilon))
				{
					MusicNote musicNote = ((ListObjectPoolBase<GameObject>)_notes).Add<MusicNote>();
					musicNote.Set(note);
					Vector3 localPosition = default(Vector3);
					localPosition.x = (float)(note.Tick - _start) / (float)music.Division * (float)num2;
					localPosition.y = (float)Mathf.Abs(NotePosition(note.Midi)) * 7f;
					((Component)musicNote).transform.localPosition = localPosition;
					num3++;
				}
			}
		}
	}

	public void Set(Music music, int start, int end)
	{
		_music = music;
		_start = start;
		_end = end;
		Refresh(music);
	}

	private int NotePosition(int note)
	{
		int num = note - 45;
		if (num < 0)
		{
			num = 0;
		}
		if (num > 33)
		{
			num = 33;
		}
		return NotePositions[num];
	}

	public bool SetGuideLine(float timer)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if (timer < 0f)
		{
			((Component)_playGuideLine).gameObject.SetActive(false);
			return false;
		}
		int num = _music.TimerToTick(timer);
		bool flag = num >= _start && num <= _end;
		((Component)_playGuideLine).gameObject.SetActive(flag);
		if (flag)
		{
			float num2 = _music.TickToTimer(_start);
			float num3 = _music.TickToTimer(_end);
			float num4 = (timer - num2) / (num3 - num2);
			_playGuideLine.localPosition = (float)_noteArea.width * num4 * Vector3.right;
		}
		return flag;
	}
}
