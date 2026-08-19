using System;
using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
	public const uint InvalidInstanceId = 0u;

	[SerializeField]
	private GameObject _akSoundObjectTemplate;

	[SerializeField]
	private int _maxInstanceCount = 24;

	private uint _instanceIdGenerator = 1u;

	private readonly SoundBanksLoader _soundBanksLoader = new SoundBanksLoader();

	private readonly List<SoundEventInstance> _soundInstancePool = new List<SoundEventInstance>();

	private readonly Dictionary<uint, SoundEventInstance> _soundInstanceDictionary = new Dictionary<uint, SoundEventInstance>();

	private readonly AkAudioSettings _audioSettings = new AkAudioSettings();

	public static float VolumeForSfx { get; private set; }

	public static float VolumeForAmbience { get; private set; }

	public static float VolumeForMidi { get; private set; }

	public static float VolumeForBgm { get; private set; }

	public static bool IgnorePreparedCheck { get; set; }

	public static GameObject ListenerObject { get; private set; }

	public SoundBanksLoader.State BankLoadState => _soundBanksLoader.LoadState;

	public AkAudioSettings AudioSettings => _audioSettings;

	static SoundManager()
	{
		GameManager.Reset += delegate
		{
			if (Singleton<SoundManager>.HasInstance())
			{
				Singleton<SoundManager>.Instance().ClearAll();
			}
		};
	}

	protected override bool CheckDontDestroyOnLoad()
	{
		return true;
	}

	public void Initialize()
	{
		_soundBanksLoader.Initialize();
		AkSoundEngine.SetCurrentLanguage(LocalizeSystem.VoiceLocale);
		AkSoundEngine.GetAudioSettings(_audioSettings);
	}

	private void ClearAll()
	{
		if (Singleton<MusicManager>.HasInstance())
		{
			Singleton<MusicManager>.Instance().ClearAll();
		}
		AkSoundEngine.StopAll();
		_soundBanksLoader.ClearAll();
		foreach (SoundEventInstance item in _soundInstancePool)
		{
			item.DestroySoundObject();
		}
		_soundInstancePool.Clear();
		_soundInstanceDictionary.Clear();
	}

	public static bool HasEvent(string eventName)
	{
		if (EmptyInstance() || string.IsNullOrEmpty(eventName))
		{
			return false;
		}
		return Singleton<SoundManager>.Instance().ContainsEvent(eventName);
	}

	public static void PlayEvent(string eventName)
	{
		if (!EmptyInstance() && !string.IsNullOrEmpty(eventName))
		{
			Singleton<SoundManager>.Instance().PlayWithListener(eventName);
		}
	}

	public static uint PlayEvent(string eventName, SoundPosition soundPosition, SoundSwitch soundSwitch, bool exclusive = false)
	{
		if (EmptyInstance() || string.IsNullOrEmpty(eventName))
		{
			return 0u;
		}
		return Singleton<SoundManager>.Instance().PlayNewInstance(eventName, soundPosition, soundSwitch, exclusive);
	}

	public static uint PlayEvent(string eventName, SoundPosition soundPosition, IEnumerable<SoundSwitch> soundSwitches, bool exclusive = false)
	{
		if (EmptyInstance() || string.IsNullOrEmpty(eventName))
		{
			return 0u;
		}
		return Singleton<SoundManager>.Instance().PlayNewInstance(eventName, soundPosition, soundSwitches, exclusive);
	}

	public static uint PlayEvent(string eventName, SoundPosition soundPosition, bool exclusive = false)
	{
		return PlayEvent(eventName, soundPosition, SoundSwitch.Empty, exclusive);
	}

	public static bool PlayEvent(uint id, string eventName)
	{
		if (EmptyInstance() || string.IsNullOrEmpty(eventName))
		{
			return false;
		}
		return Singleton<SoundManager>.Instance().PlayExistInstance(id, eventName);
	}

	public static void StopEvent(uint id, float transitionDuration = 0f)
	{
		if (!EmptyInstance() && id != 0)
		{
			Singleton<SoundManager>.Instance().StopInstance(id, transitionDuration);
		}
	}

	public static bool IsPlaying(uint id)
	{
		if (EmptyInstance() || id == 0)
		{
			return false;
		}
		return Singleton<SoundManager>.Instance().IsPlayingInstace(id);
	}

	public static bool IsPrepared(string eventName)
	{
		if (EmptyInstance() || string.IsNullOrEmpty(eventName))
		{
			return false;
		}
		return Singleton<SoundManager>.Instance().IsPreparedEvent(eventName);
	}

	public static void PrepareEvent(string eventName, Action callback = null)
	{
		if (!EmptyInstance() && !string.IsNullOrEmpty(eventName))
		{
			Singleton<SoundManager>.Instance().PrepareBank(eventName, callback);
		}
	}

	public static void SetPosition(uint id, SoundPosition soundPosition)
	{
		if (!EmptyInstance() && id != 0)
		{
			Singleton<SoundManager>.Instance().SetInstancePosition(id, soundPosition);
		}
	}

	public static void SetSwitch(uint id, SoundSwitch soundSwitch)
	{
		if (!EmptyInstance() && id != 0 && !soundSwitch.IsEmpty)
		{
			Singleton<SoundManager>.Instance().SetInstanceSwitch(id, soundSwitch);
		}
	}

	public static bool TryGetRTPCValue(uint id, string name, out float value)
	{
		if (EmptyInstance() || id == 0 || string.IsNullOrEmpty(name))
		{
			value = 0f;
			return false;
		}
		return Singleton<SoundManager>.Instance().TryGetInstanceRTPCValue(id, name, out value);
	}

	public static void SetState(SoundStates soundStates)
	{
		AKRESULT aKRESULT = AkSoundEngine.SetState(soundStates.Group, soundStates.State);
		if (aKRESULT == AKRESULT.AK_Success)
		{
		}
	}

	public static float GetRTPC(string name)
	{
		int io_rValueType = 1;
		float out_rValue;
		AKRESULT rTPCValue = AkSoundEngine.GetRTPCValue(name, null, 0u, out out_rValue, ref io_rValueType);
		if (rTPCValue != AKRESULT.AK_Success)
		{
		}
		return out_rValue;
	}

	public static void SetRTPC(SoundParameters parameter)
	{
		AKRESULT aKRESULT = AkSoundEngine.SetRTPCValue(parameter.Name, parameter.Value);
		if (aKRESULT == AKRESULT.AK_Success)
		{
		}
	}

	public static void SetListenerObject(GameObject listener)
	{
		if (!(listener.GetComponent<AkAudioListener>() == null))
		{
			ListenerObject = listener;
		}
	}

	public static void SetSfxVolume(float val)
	{
		VolumeForSfx = Mathf.Clamp01(val);
		SetRTPC(new SoundParameters("sfx", VolumeForSfx * 100f));
	}

	public static void SetAmbienceVolume(float val)
	{
		VolumeForAmbience = Mathf.Clamp01(val);
		SetRTPC(new SoundParameters("ambience", VolumeForAmbience * 100f));
	}

	public static void SetMidiVolume(float val)
	{
		VolumeForMidi = Mathf.Clamp01(val);
		SetRTPC(new SoundParameters("instruments", VolumeForMidi * 100f));
	}

	public static void SetBgmVolume(float val)
	{
		VolumeForBgm = Mathf.Clamp01(val);
		SetRTPC(new SoundParameters("bgm", VolumeForBgm * 100f));
	}

	private static bool EmptyInstance()
	{
		if (Singleton<SoundManager>.HasInstance())
		{
			return false;
		}
		return true;
	}

	private bool ContainsEvent(string eventName)
	{
		return _soundBanksLoader.ContainsEvent(eventName);
	}

	private void PlayWithListener(string eventName)
	{
		if (ListenerObject == null)
		{
			return;
		}
		if (IsPreparedEvent(eventName))
		{
			PostEvent(eventName, ListenerObject);
			return;
		}
		PrepareBank(eventName, delegate
		{
			PostEvent(eventName, ListenerObject);
		});
	}

	private static void PostEvent(string eventName, GameObject gameObject)
	{
		if (AkSoundEngine.PostEvent(eventName, gameObject) != 0)
		{
		}
	}

	[NotNull]
	private SoundEventInstance GetSoundInstance()
	{
		SoundEventInstance soundEventInstance = GetSoundInstanceFromPool();
		if (soundEventInstance != null)
		{
			_soundInstanceDictionary.Remove(soundEventInstance.InstanceId);
		}
		else
		{
			soundEventInstance = AddNewSoundInstaceToPool();
		}
		uint key = (soundEventInstance.InstanceId = _instanceIdGenerator++);
		_soundInstanceDictionary.Add(key, soundEventInstance);
		return soundEventInstance;
	}

	private uint PlayNewInstance(string eventName, SoundPosition soundPosition, SoundSwitch soundSwitch, bool exclusive)
	{
		SoundEventInstance soundInstance = GetSoundInstance();
		soundInstance.Play(eventName, soundPosition, soundSwitch, exclusive);
		return soundInstance.InstanceId;
	}

	private uint PlayNewInstance(string eventName, SoundPosition soundPosition, IEnumerable<SoundSwitch> soundSwitches, bool exclusive)
	{
		SoundEventInstance soundInstance = GetSoundInstance();
		soundInstance.Play(eventName, soundPosition, soundSwitches, exclusive);
		return soundInstance.InstanceId;
	}

	private bool PlayExistInstance(uint id, string eventName)
	{
		if (_soundInstanceDictionary.TryGetValue(id, out var value))
		{
			value.Play(eventName);
			return true;
		}
		return false;
	}

	private void StopInstance(uint id, float transitionDuration = 0f)
	{
		if (_soundInstanceDictionary.TryGetValue(id, out var value))
		{
			value.Stop(transitionDuration);
		}
	}

	private bool IsPlayingInstace(uint id)
	{
		if (_soundInstanceDictionary.TryGetValue(id, out var value))
		{
			return value.CurrentState != SoundEventInstance.State.Stop;
		}
		return false;
	}

	private bool IsPreparedEvent(string eventName)
	{
		return IgnorePreparedCheck || _soundBanksLoader.IsPreparedEvent(eventName);
	}

	private void PrepareBank(string eventName, Action callback)
	{
		_soundBanksLoader.LoadBankByEventName(eventName, callback);
	}

	private void SetInstancePosition(uint id, SoundPosition soundPosition)
	{
		if (_soundInstanceDictionary.TryGetValue(id, out var value))
		{
			value.SetPosition(soundPosition);
		}
	}

	public SoundEventInstance GetSoundInstance(uint id)
	{
		if (_soundInstanceDictionary.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	private void SetInstanceSwitch(uint id, SoundSwitch soundSwitch)
	{
		if (_soundInstanceDictionary.TryGetValue(id, out var value))
		{
			value.SetSwitch(soundSwitch);
		}
	}

	private bool TryGetInstanceRTPCValue(uint id, string name, out float value)
	{
		if (_soundInstanceDictionary.TryGetValue(id, out var value2))
		{
			return value2.TryGetRTPCValue(name, out value);
		}
		value = 0f;
		return false;
	}

	[NotNull]
	private SoundEventInstance AddNewSoundInstaceToPool()
	{
		SoundEventInstance soundEventInstance = new SoundEventInstance(_akSoundObjectTemplate, base.transform);
		_soundInstancePool.Add(soundEventInstance);
		return soundEventInstance;
	}

	[CanBeNull]
	private SoundEventInstance GetSoundInstanceFromPool()
	{
		bool flag = _maxInstanceCount != 0 && _maxInstanceCount <= _soundInstancePool.Count;
		SoundEventInstance soundEventInstance = null;
		for (int i = 0; i < _soundInstancePool.Count; i++)
		{
			SoundEventInstance soundEventInstance2 = _soundInstancePool[i];
			if (!soundEventInstance2.Exclusive)
			{
				if (soundEventInstance2.CurrentState == SoundEventInstance.State.Stop)
				{
					soundEventInstance = soundEventInstance2;
					break;
				}
				if (flag && (soundEventInstance == null || soundEventInstance.LastUsedTime > soundEventInstance2.LastUsedTime))
				{
					soundEventInstance = soundEventInstance2;
				}
			}
		}
		return soundEventInstance;
	}
}
