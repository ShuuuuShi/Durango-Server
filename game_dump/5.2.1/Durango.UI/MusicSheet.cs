using System;
using System.Collections.Generic;
using Durango.Logic.Music;
using Durango.Render.Camera;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class MusicSheet : MonoBehaviour
{
	private class NoteItem
	{
		public Note Note;

		public int Length;

		public bool InVisiableArea;

		public MusicNote Object;
	}

	private const int StartMargin = 30;

	private const int TemperedHeight = 20;

	public const int TermWidth = 30;

	[SerializeField]
	private MusicNote _noteBase;

	[SerializeField]
	private GameObject _sheetMain;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private Transform _noteContainer;

	[SerializeField]
	private MusicNoteEditor _noteEditor;

	[SerializeField]
	private MusicSheetBackground _background;

	[SerializeField]
	private UIWidget _playGuideLine;

	[SerializeField]
	private GameObject _playGuideLineHandle;

	[SerializeField]
	private Selectable _viewModeToggleButton;

	private Music _music;

	private float _guidelineTimer;

	private Vector2? _offset;

	private UIWidget _widget;

	private Point2 _size;

	private float _tickWidth;

	private float _limitWidth;

	private float _scrollMargin;

	private readonly Queue<MusicNote> _notePool = new Queue<MusicNote>();

	private readonly Dictionary<int, MusicNote> _makingNoteList = new Dictionary<int, MusicNote>();

	private readonly List<NoteItem> _items = new List<NoteItem>();

	private bool _isDirtyVisibleNotes;

	private bool _isFullViewer;

	private bool _isInit;

	public Action<Note, int, Note, int> NoteTickChanged
	{
		get
		{
			return _noteEditor.Changed;
		}
		set
		{
			_noteEditor.Changed = value;
		}
	}

	public event Action<int, bool> GuidelineChanged;

	public event Action<Note> NoteSelected;

	public void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_noteBase.gameObject.SetActive(value: false);
			_widget = GetComponent<UIWidget>();
			_widget.AddOnChange(OnChangeSize);
		}
	}

	private void Awake()
	{
		_background.Init(30, 30, 20, 16);
		UIEventListener uIEventListener = UIEventListener.Get(_sheetMain);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickSheet));
		UIEventListener uIEventListener2 = UIEventListener.Get(_playGuideLineHandle);
		uIEventListener2.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener2.onDrag, new UIEventListener.VectorDelegate(OnDragPlayHandle));
		UIEventListener uIEventListener3 = UIEventListener.Get(_playGuideLineHandle);
		uIEventListener3.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onPress, new UIEventListener.BoolDelegate(OnPressPlayHandle));
		Selectable viewModeToggleButton = _viewModeToggleButton;
		viewModeToggleButton.Clicked = (Action)Delegate.Combine(viewModeToggleButton.Clicked, (Action)delegate
		{
			SetViewMode(!_isFullViewer);
		});
		SetViewMode(isFull: true);
	}

	private void OnEnable()
	{
		_background.SetOffset(Vector2.zero);
	}

	private void LateUpdate()
	{
		Vector2 vector = -_scrollView.transform.localPosition;
		Vector2? offset = _offset;
		bool num = !offset.HasValue || vector != _offset.Value;
		_offset = vector;
		if (num)
		{
			SyncScroll();
		}
		if (num || _isDirtyVisibleNotes)
		{
			RefershNoteWidgets();
		}
	}

	private void SyncScroll()
	{
		Vector2 offset = -_scrollView.transform.localPosition;
		_background.SetOffset(offset);
		SetGuideLine(_guidelineTimer, scrollSync: false);
	}

	private void OnChangeSize()
	{
		Point2 point = new Point2(_widget.width, _widget.height);
		if (!(point == _size))
		{
			_size = point;
			_scrollMargin = (float)_size.x * 0.3f;
			_playGuideLine.height = _size.y - 70;
			UIUtility.UpdateAnchors(_playGuideLine.transform);
		}
	}

	private void OnClickSheet(GameObject obj)
	{
		MoveToPlayHandleToCurrentTouch(stop: true);
	}

	private void OnDragPlayHandle(GameObject obj, Vector2 delta)
	{
		MoveToPlayHandleToCurrentTouch(stop: false);
	}

	private void OnPressPlayHandle(GameObject obj, bool press)
	{
		if (press)
		{
			ClearEditNote();
		}
		else
		{
			MoveToPlayHandleToCurrentTouch(stop: true);
		}
	}

	private void SetViewMode(bool isFull)
	{
		_isFullViewer = isFull;
		_viewModeToggleButton.Selected = _isFullViewer;
		_scrollView.verticalScrollBar.value = 0.5f;
		UpdateScrollBounds();
		_isDirtyVisibleNotes = true;
	}

	private void MoveToPlayHandleToCurrentTouch(bool stop)
	{
		Vector3 vector = MainCamera.ScreenPosToNGUIPos(UICamera.lastEventPosition, _sheetMain.transform);
		int value = Mathf.RoundToInt((_offset.GetValueOrDefault().x + vector.x - 30f) / _tickWidth);
		value = Mathf.Clamp(value, 0, (_music != null) ? (_music.GetLastTick() + _music.Division) : 0);
		if (this.GuidelineChanged != null)
		{
			this.GuidelineChanged(value, stop);
		}
	}

	public void Set(Music music)
	{
		_music = music;
		_limitWidth = 0f;
		List<Note> notes = music.Notes;
		foreach (NoteItem item in _items)
		{
			if (item.InVisiableArea)
			{
				PushNoteObject(item.Object);
			}
		}
		_items.Clear();
		_tickWidth = 480f / (float)music.Division;
		int size = KUtility.GetSize(notes);
		using (Reusable<Dictionary<int, Note>> reusable = ReusableDictionary<int, Note>.Pop())
		{
			Dictionary<int, Note> value = reusable.Value;
			for (int i = 0; i < size; i++)
			{
				Note value2 = notes[i];
				bool flag = value.ContainsKey(value2.Midi);
				if (value2.On)
				{
					if (flag)
					{
						Note note = value[value2.Midi];
						_items.Add(new NoteItem
						{
							Note = note,
							Length = value2.Tick - note.Tick
						});
					}
					value[value2.Midi] = value2;
				}
				else if (flag)
				{
					Note note2 = value[value2.Midi];
					value.Remove(value2.Midi);
					_items.Add(new NoteItem
					{
						Note = note2,
						Length = value2.Tick - note2.Tick
					});
				}
			}
			foreach (KeyValuePair<int, Note> item2 in value)
			{
				Note value3 = item2.Value;
				_items.Add(new NoteItem
				{
					Note = value3,
					Length = music.Division
				});
			}
		}
		_scrollView.horizontalScrollBar.value = 0f;
		_scrollView.verticalScrollBar.value = 0.5f;
		_scrollView.UpdatePosition();
		UpdateMusicRunningTime();
		_isDirtyVisibleNotes = true;
	}

	public void UpdateMusicRunningTime()
	{
		int size = KUtility.GetSize(_music.Notes);
		float num = _widget.width;
		if (size > 0)
		{
			int num2 = Mathf.CeilToInt((float)_music.GetLastTick() / (float)_music.Division) + 1;
			num = Mathf.Max(_limitWidth, num2 * 30 * 16);
		}
		if (!(num < _limitWidth))
		{
			_limitWidth = num;
			UpdateScrollBounds();
		}
	}

	private void UpdateScrollBounds()
	{
		Vector3 size = new Vector3(_limitWidth, Mathf.Max(_widget.height, 1780));
		Bounds fixedBounds;
		float y;
		if (_isFullViewer)
		{
			fixedBounds = new Bounds(new Vector3(_limitWidth * 0.5f, 0f), size);
			y = 1f;
		}
		else
		{
			float num = _widget.height - 20;
			y = num / size.y;
			size.y = num;
			fixedBounds = new Bounds(new Vector3(_limitWidth * 0.5f, 0f), size);
		}
		Transform obj = _noteContainer.transform;
		Vector3 localScale = new Vector3(1f, y, 1f);
		_background.transform.localScale = localScale;
		obj.localScale = localScale;
		_scrollView.SetFixedBounds(fixedBounds);
		_scrollView.UpdateScrollbars(recalculateBounds: false);
	}

	public void BeginMakeNote(Note note)
	{
		int num;
		if (_makingNoteList.TryGetValue(note.Midi, out var value))
		{
			num = note.Tick - value.Note.Tick;
		}
		else
		{
			value = PopNoteObject();
			value.Set(note);
			value.SetPosition(GetNotePosition(note), 0f, 0.5f);
			num = MusicSheetEditor.TickCountPerTerm;
			_makingNoteList.Add(note.Midi, value);
		}
		value.width = (int)((float)num * _tickWidth);
		UIUtility.UpdateAnchors(value.transform);
	}

	public void FinishMakeNote(Note note)
	{
		if (_makingNoteList.TryGetValue(note.Midi, out var value))
		{
			int num = note.Tick - value.Note.Tick;
			value.width = (int)((float)num * _tickWidth);
			UIUtility.UpdateAnchors(value.transform);
			_makingNoteList.Remove(note.Midi);
			_items.Add(new NoteItem
			{
				Note = value.Note,
				Length = num,
				InVisiableArea = true,
				Object = value
			});
			_isDirtyVisibleNotes = true;
		}
	}

	public void RemoveNote(Note note)
	{
		int num = -1;
		for (int i = 0; i < _items.Count; i++)
		{
			Note note2 = _items[i].Note;
			if (note2.Midi == note.Midi && note2.Tick == note.Tick)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			NoteItem noteItem = _items[num];
			if (noteItem.InVisiableArea)
			{
				PushNoteObject(noteItem.Object);
			}
			_items.RemoveAt(num);
		}
	}

	public void AddNote(Note note, int length)
	{
		_items.Add(new NoteItem
		{
			Note = note,
			Length = length
		});
		_isDirtyVisibleNotes = true;
	}

	private Vector3 GetNotePosition(Note note)
	{
		Vector3 result = default(Vector3);
		result.x = (float)note.Tick * _tickWidth + 30f;
		result.y = -870f + ((float)(note.Midi - 21) + 0.5f) * 20f;
		return result;
	}

	public void SetScrollEnable(bool on)
	{
		_scrollView.enabled = on;
	}

	public void SetGuideLine(float timer, bool scrollSync)
	{
		_guidelineTimer = timer;
		if (timer < 0f || _music == null)
		{
			_playGuideLine.gameObject.SetActive(value: false);
			return;
		}
		_playGuideLine.gameObject.SetActive(value: true);
		float num = _music.TickToTimer(1);
		float num2 = 480f / (float)_music.Division / num;
		Vector3 vector = new Vector3(timer * num2, 0f);
		vector.x += 30f;
		float num3 = 0f - _scrollView.transform.localPosition.x;
		Vector3 localPosition = vector + Vector3.left * num3;
		if (scrollSync)
		{
			bool flag = false;
			if (localPosition.x < _scrollMargin && num3 > 0f)
			{
				flag = true;
				num3 = Mathf.Max(0f, num3 - (_scrollMargin - localPosition.x));
			}
			else if (localPosition.x > (float)_widget.width - _scrollMargin)
			{
				flag = true;
				num3 += localPosition.x - ((float)_widget.width - _scrollMargin);
				if (num3 > _limitWidth - (float)_widget.width)
				{
					_limitWidth = num3 + (float)_widget.width;
					UpdateScrollBounds();
				}
			}
			if (flag)
			{
				Vector3 localPosition2 = _scrollView.transform.localPosition;
				Vector2 clipOffset = _scrollView.panel.clipOffset;
				localPosition2.x = 0f - num3;
				clipOffset.x = num3;
				_scrollView.transform.localPosition = localPosition2;
				_scrollView.panel.clipOffset = clipOffset;
				_scrollView.UpdateScrollbars(recalculateBounds: false);
				localPosition = vector + Vector3.left * num3;
			}
		}
		foreach (MusicNote value in _makingNoteList.Values)
		{
			float num4 = _music.TickToTimer(value.Note.Tick);
			float num5 = timer - num4;
			value.width = (int)(num5 * num2);
			UIUtility.UpdateAnchors(value.transform);
		}
		_playGuideLine.transform.localPosition = localPosition;
	}

	private void RefershNoteWidgets()
	{
		_isDirtyVisibleNotes = false;
		float x = _offset.GetValueOrDefault().x;
		float num = 480f / (float)_music.Division;
		int num2 = Mathf.FloorToInt((x - 30f) / num);
		int num3 = Mathf.CeilToInt((x + _scrollView.panel.width) / num);
		Note? current = _noteEditor.Current;
		for (int i = 0; i < _items.Count; i++)
		{
			NoteItem noteItem = _items[i];
			Note note = noteItem.Note;
			bool flag = num3 >= note.Tick && note.Tick + noteItem.Length >= num2;
			if (noteItem.InVisiableArea != flag)
			{
				noteItem.InVisiableArea = flag;
				if (flag)
				{
					MusicNote musicNote = PopNoteObject();
					musicNote.Set(note);
					musicNote.SetPosition(GetNotePosition(note), 0f, 0.5f);
					musicNote.width = (int)((float)noteItem.Length * _tickWidth);
					UIUtility.UpdateAnchors(musicNote.transform);
					noteItem.Object = musicNote;
				}
				else
				{
					PushNoteObject(noteItem.Object);
				}
			}
			if (noteItem.InVisiableArea)
			{
				bool flag2 = current.HasValue && current.Value.Midi == note.Midi && current.Value.Tick == note.Tick;
				noteItem.Object.alpha = ((!flag2) ? 1f : 0f);
			}
		}
	}

	public void EditNote(Note note)
	{
		int num = -1;
		for (int i = 0; i < _items.Count; i++)
		{
			Note note2 = _items[i].Note;
			if (note2.On && note2.Midi == note.Midi && note2.Tick == note.Tick)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			_noteEditor.Show(_music, note, GetNotePosition(_items[num].Note));
			if (_isFullViewer)
			{
				_scrollView.verticalScrollBar.value = 1f - Mathf.InverseLerp(21f, 108f, note.Midi);
				_scrollView.UpdatePosition();
			}
			_isDirtyVisibleNotes = true;
		}
	}

	public void ClearEditNote()
	{
		_noteEditor.Hide();
		_isDirtyVisibleNotes = true;
	}

	private MusicNote PopNoteObject()
	{
		MusicNote musicNote;
		if (_notePool.Count > 0)
		{
			musicNote = _notePool.Dequeue();
		}
		else
		{
			musicNote = UnityEngine.Object.Instantiate(_noteBase.gameObject, _noteBase.transform.parent).GetComponent<MusicNote>();
			musicNote.Clicked += OnClickMusicNote;
		}
		musicNote.gameObject.SetActive(value: true);
		return musicNote;
	}

	private void OnClickMusicNote(MusicNote obj)
	{
		if (this.NoteSelected != null)
		{
			this.NoteSelected(obj.Note);
		}
	}

	private void PushNoteObject(MusicNote obj)
	{
		obj.gameObject.SetActive(value: false);
		_notePool.Enqueue(obj);
	}
}
