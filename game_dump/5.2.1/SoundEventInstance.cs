using System.Collections.Generic;
using UnityEngine;

public class SoundEventInstance
{
	public enum State
	{
		Stop,
		Loading,
		Playing
	}

	private readonly GameObject _akSoundObjectTemplate;

	private readonly Transform _defaultParent;

	private uint _playingId;

	private readonly Dictionary<string, SoundSwitch> _soundSwitches = new Dictionary<string, SoundSwitch>();

	private SoundPosition _soundPosition = SoundPosition.Empty;

	private GameObject _akSoundObject;

	public uint InstanceId { get; set; }

	public bool Exclusive { get; private set; }

	public State CurrentState { get; private set; }

	public float LastUsedTime { get; private set; }

	public float Duration { get; private set; }

	public SoundEventInstance(GameObject akSoundObjectTemplate, Transform parent)
	{
		_akSoundObjectTemplate = akSoundObjectTemplate;
		_defaultParent = parent;
		SetStopState();
		LastUsedTime = Time.time;
	}

	public void Play(string eventName, SoundPosition soundPosition, SoundSwitch soundSwitch, bool exclusive)
	{
		_soundSwitches.Clear();
		if (!soundSwitch.IsEmpty)
		{
			_soundSwitches[soundSwitch.Group] = soundSwitch;
		}
		_soundPosition = soundPosition;
		Exclusive = exclusive;
		Play(eventName);
	}

	public void Play(string eventName, SoundPosition soundPosition, IEnumerable<SoundSwitch> soundSwitches, bool exclusive)
	{
		_soundSwitches.Clear();
		foreach (SoundSwitch soundSwitch in soundSwitches)
		{
			if (!soundSwitch.IsEmpty)
			{
				_soundSwitches[soundSwitch.Group] = soundSwitch;
			}
		}
		_soundPosition = soundPosition;
		Exclusive = exclusive;
		Play(eventName);
	}

	public void Play(string eventName)
	{
		if (SoundManager.IsPrepared(eventName))
		{
			PostEvent(eventName);
			return;
		}
		CurrentState = State.Loading;
		SoundManager.PrepareEvent(eventName, delegate
		{
			if (CurrentState == State.Loading)
			{
				PostEvent(eventName);
			}
		});
	}

	public void Stop(float transitionDuration = 0f)
	{
		switch (CurrentState)
		{
		case State.Playing:
			AkSoundEngine.StopPlayingID(_playingId, (int)(transitionDuration * 1000f));
			break;
		case State.Loading:
			SetStopState();
			break;
		}
	}

	public void SetPosition(SoundPosition soundPosition)
	{
		_soundPosition = soundPosition;
		if (CurrentState == State.Playing)
		{
			ApplyPosition();
		}
	}

	public void SetSwitch(SoundSwitch soundSwitch)
	{
		if (!soundSwitch.IsEmpty)
		{
			_soundSwitches[soundSwitch.Group] = soundSwitch;
			if (CurrentState == State.Playing)
			{
				ApplySwitch();
			}
		}
	}

	public bool TryGetRTPCValue(string name, out float value)
	{
		AKRESULT aKRESULT = AKRESULT.AK_Fail;
		value = 0f;
		if (CurrentState == State.Playing)
		{
			int io_rValueType = 2;
			aKRESULT = AkSoundEngine.GetRTPCValue(name, _akSoundObject, _playingId, out value, ref io_rValueType);
		}
		return aKRESULT == AKRESULT.AK_Success;
	}

	public void DestroySoundObject()
	{
		Object.Destroy(_akSoundObject);
	}

	private void SetStopState()
	{
		_playingId = 0u;
		Exclusive = false;
		CurrentState = State.Stop;
	}

	private void PostEvent(string eventName)
	{
		if (!ApplyPosition())
		{
			SetStopState();
			return;
		}
		ApplySwitch();
		AkCallbackType in_uFlags = (AkCallbackType)9;
		_playingId = AkSoundEngine.PostEvent(eventName, _akSoundObject, (uint)in_uFlags, EventCallback, null);
		if (_playingId != 0)
		{
			CurrentState = State.Playing;
			LastUsedTime = Time.time;
		}
		else
		{
			SetStopState();
		}
	}

	private void ApplySwitch()
	{
		RefreshSoundObject();
		foreach (SoundSwitch value in _soundSwitches.Values)
		{
			AkSoundEngine.SetSwitch(value.Group, value.State, _akSoundObject);
		}
		_soundSwitches.Clear();
	}

	private bool ApplyPosition()
	{
		RefreshSoundObject();
		switch (_soundPosition.PositionType)
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
			_akSoundObject.transform.position = _soundPosition.Position;
			break;
		case SoundPosition.Type.ChaseObject:
			if (_soundPosition.Target == null)
			{
				return false;
			}
			_akSoundObject.transform.parent = _soundPosition.Target.transform;
			_akSoundObject.transform.localPosition = _soundPosition.Position;
			break;
		}
		return true;
	}

	private void RefreshSoundObject()
	{
		if (_akSoundObject == null)
		{
			_akSoundObject = Object.Instantiate(_akSoundObjectTemplate);
		}
	}

	private void EventCallback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
	{
		if ((in_type & AkCallbackType.AK_EndOfEvent) != 0)
		{
			SetStopState();
		}
		if ((in_type & AkCallbackType.AK_Duration) != 0)
		{
			AkDurationCallbackInfo akDurationCallbackInfo = (AkDurationCallbackInfo)in_info;
			Duration = akDurationCallbackInfo.fDuration;
		}
	}
}
