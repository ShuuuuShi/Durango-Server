using System;
using System.Collections.Generic;
using Durango.Logic.Music;
using UnityEngine;

namespace Durango.UI;

public class MusicNoteSelector : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _items;

	private readonly List<Note> _notes = new List<Note>();

	private bool _isShow;

	private bool _isInit;

	public event Action<Note> NoteSelected;

	private void Start()
	{
		if (!_isShow)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		Hide();
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_items.Init(delegate(GameObject obj)
			{
				UIEventListener uIEventListener = UIEventListener.Get(obj);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickItem));
			});
		}
	}

	private void OnClickItem(GameObject obj)
	{
		int num = _items.IndexOf(obj);
		if (num != -1 && this.NoteSelected != null)
		{
			this.NoteSelected(_notes[num]);
		}
	}

	public void Clear()
	{
		_notes.Clear();
	}

	public void Add(Note note)
	{
		_notes.Add(note);
	}

	public void Show()
	{
		Init();
		_notes.Sort(Comparison);
		_items.BeginLoad();
		for (int i = 0; i < _notes.Count; i++)
		{
			GameObject next = _items.GetNext();
			Note note = _notes[i];
			UILabel componentInChildren = next.GetComponentInChildren<UILabel>();
			componentInChildren.text = MusicManager.GetNoteName(note.Midi, sharps: true, showOctave: false);
		}
		_items.EndLoad();
		UpdateLayout();
		if (!_isShow)
		{
			_isShow = true;
			base.gameObject.SetActive(value: true);
		}
	}

	private int Comparison(Note n1, Note n2)
	{
		return n2.Midi - n1.Midi;
	}

	public void Hide()
	{
		if (_isShow)
		{
			_isShow = false;
			base.gameObject.SetActive(value: false);
		}
	}

	private void UpdateLayout()
	{
		UIWidget component = GetComponent<UIWidget>();
		float num = UIUtility.WidgetsReposition(_items, component, Vector3.down);
		component.height = (int)num;
		UIUtility.UpdateAnchors(base.transform);
	}
}
