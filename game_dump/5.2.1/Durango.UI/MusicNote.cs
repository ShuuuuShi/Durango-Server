using System;
using Durango.Logic.Music;
using UnityEngine;

namespace Durango.UI;

public class MusicNote : UIWidget
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UISprite _bgSprite;

	public Note Note { get; private set; }

	public event Action<MusicNote> Clicked;

	public void Set(Note note)
	{
		Note = note;
		_label.text = MusicManager.GetNoteName(note.Midi, sharps: true, showOctave: false);
	}

	private void OnClick()
	{
		if (this.Clicked != null)
		{
			this.Clicked(this);
		}
	}
}
