using MusicData;
using UnityEngine;

public class MusicNote : MonoBehaviour
{
	[SerializeField]
	private UILabel _label;

	public void Set(Note note)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		_label.text = MusicManager.GetNoteName(note.Midi, sharps: true, showOctave: false);
		_label.color = Music.GetChannelColor(note.Channel);
	}
}
