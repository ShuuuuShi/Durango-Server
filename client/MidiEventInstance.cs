using Durango.Logic.Music;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public class MidiEventInstance
{
	private readonly GameObject _akSoundObjectTemplate;

	private readonly Transform _defaultParent;

	private uint _eventId;

	private GameObject _akSoundObject;

	public float FinishAt { get; private set; }

	public MidiEventInstance(GameObject akSoundObjectTemplate, Transform parent)
	{
		_akSoundObjectTemplate = akSoundObjectTemplate;
		_defaultParent = parent;
	}

	public bool Play(string midiEventName, [NotNull] AkMIDIPostArray midiPostArray, float duration, SoundPosition soundPosition)
	{
		_eventId = 0u;
		FinishAt = 0f;
		ApplyPosition(soundPosition);
		_eventId = AkSoundEngine.GetIDFromString(midiEventName);
		FinishAt = Time.time + duration;
		AKRESULT aKRESULT = AkSoundEngine.PostMIDIOnEvent(_eventId, _akSoundObject, midiPostArray, (ushort)midiPostArray.Count());
		if (aKRESULT != AKRESULT.AK_Success)
		{
			return false;
		}
		return true;
	}

	public void Stop()
	{
		if (_eventId != 0 && _akSoundObject != null)
		{
			AkSoundEngine.StopMIDIOnEvent(_eventId, _akSoundObject);
			_eventId = 0u;
			FinishAt = 0f;
		}
	}

	public void SetPosition(SoundPosition soundPosition)
	{
		ApplyPosition(soundPosition);
	}

	public void DestroySoundObject()
	{
		Stop();
		Object.Destroy(_akSoundObject);
	}

	private void ApplyPosition(SoundPosition soundPosition)
	{
		RefreshSoundObject();
		switch (soundPosition.PositionType)
		{
		case SoundPosition.Type.None:
			if (SoundManager.ListenerObject != null)
			{
				_akSoundObject.transform.parent = SoundManager.ListenerObject.transform;
				_akSoundObject.transform.localPosition = Vector3.zero;
			}
			else
			{
				_akSoundObject.transform.parent = _defaultParent;
				_akSoundObject.transform.position = Vector3.zero;
			}
			break;
		case SoundPosition.Type.Position3D:
			_akSoundObject.transform.parent = _defaultParent;
			_akSoundObject.transform.position = soundPosition.Position;
			break;
		case SoundPosition.Type.ChaseObject:
			_akSoundObject.transform.parent = soundPosition.Target.transform;
			_akSoundObject.transform.localPosition = soundPosition.Position;
			break;
		}
	}

	private void RefreshSoundObject()
	{
		if (_akSoundObject == null)
		{
			_akSoundObject = Object.Instantiate(_akSoundObjectTemplate);
		}
	}

	public static AkMIDIPostArray CreateMidiPostArray(Music music, float startAt, out float duration)
	{
		duration = 0f;
		uint num = 0u;
		if (GetStartIndexAndLength(music, startAt, out var startIndex, out var length))
		{
			length = Mathf.Min(length, 6000);
			AkMIDIPostArray akMIDIPostArray = new AkMIDIPostArray(length);
			uint uNumSamplesPerSecond = Singleton<SoundManager>.Instance().AudioSettings.uNumSamplesPerSecond;
			for (int i = 0; i < length; i++)
			{
				Note note = music.Notes[i + startIndex];
				AkMIDIPost akMIDIPost = akMIDIPostArray[i];
				uint num2 = (uint)((music.TickToTimer(note.Tick) - startAt) * (float)uNumSamplesPerSecond);
				num = num2;
				akMIDIPost.byType = ((!note.On) ? AkMIDIEventTypes.NOTE_OFF : AkMIDIEventTypes.NOTE_ON);
				akMIDIPost.byChan = (byte)note.Channel;
				akMIDIPost.byOnOffNote = (byte)note.Midi;
				akMIDIPost.byVelocity = (byte)(note.Volume * 127f);
				akMIDIPost.uOffset = num2;
			}
			duration = (float)num / (float)uNumSamplesPerSecond;
			return akMIDIPostArray;
		}
		return null;
	}

	private static bool GetStartIndexAndLength(Music music, float startAt, out int startIndex, out int length)
	{
		startIndex = -1;
		length = 0;
		for (int i = 0; i < music.Notes.Count; i++)
		{
			float num = music.TickToTimer(music.Notes[i].Tick);
			if (!(num < startAt))
			{
				startIndex = i;
				break;
			}
		}
		if (startIndex != -1)
		{
			length = music.Notes.Count - startIndex;
			return true;
		}
		return false;
	}
}
