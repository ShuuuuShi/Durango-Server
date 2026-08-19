using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class MusicKeyboardItem : UIWidget
{
	public enum State
	{
		Unpress,
		Press
	}

	[SerializeField]
	[WidgetStates.Type(typeof(State))]
	private WidgetStates _states;

	[SerializeField]
	private UILabel _midiLabel;

	private bool _isPress;

	private bool _isSelect;

	public int Midi { get; private set; }

	public event Action<MusicKeyboardItem, bool> Pressed;

	private void OnPress(bool isPress)
	{
		if (this.Pressed != null)
		{
			this.Pressed(this, isPress);
		}
	}

	public void Initialize(int midi)
	{
		Midi = midi;
		if (_midiLabel != null)
		{
			_midiLabel.text = MusicManager.GetNoteName(midi, sharps: false, showOctave: true);
		}
		ResetState();
	}

	public void Press(bool press)
	{
		if (_isPress != press)
		{
			_isPress = press;
			RefershState();
		}
	}

	public void Select(bool select)
	{
		if (_isSelect != select)
		{
			_isSelect = select;
			RefershState();
		}
	}

	public void ResetState()
	{
		if (_isPress || _isSelect)
		{
			_isPress = false;
			_isSelect = false;
			RefershState();
		}
	}

	private void RefershState()
	{
		State value = ((_isPress || _isSelect) ? State.Press : State.Unpress);
		_states.Apply((int)value);
	}
}
