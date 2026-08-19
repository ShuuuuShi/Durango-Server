using System;
using UnityEngine;

public class SoundInstance : MonoBehaviour
{
	public enum SwitchType
	{
		None,
		LevelAndRegion
	}

	[SerializeField]
	private SoundEventType _audioClip;

	[SerializeField]
	private Vector3 _offset;

	[SerializeField]
	private SwitchType _switchType;

	private uint _soundInstanceId;

	private Action _playDelegator;

	private void Awake()
	{
		SoundManager.PrepareEvent(_audioClip);
	}

	private void OnEnable()
	{
		if (_switchType == SwitchType.LevelAndRegion)
		{
			GameSystem<StatisticsSystem>.Instance().LevelChanged += StatisticsSystem_LevelChanged;
		}
		Play();
	}

	private void OnDisable()
	{
		if (_switchType == SwitchType.LevelAndRegion && GameSystem<StatisticsSystem>.HasInstance())
		{
			GameSystem<StatisticsSystem>.Instance().LevelChanged -= StatisticsSystem_LevelChanged;
		}
		Stop();
	}

	[ExposedInEditor(null)]
	private void SetTestSwitch(string group, string state)
	{
		if (_soundInstanceId != 0)
		{
			SoundManager.SetSwitch(_soundInstanceId, SoundSwitch.Set(group, state));
		}
	}

	private void Play()
	{
		if (!string.IsNullOrEmpty(_audioClip))
		{
			Stop();
			SwitchType switchType = _switchType;
			if (switchType != SwitchType.LevelAndRegion)
			{
				PlayDefault();
			}
			else if (GameSystem<StatisticsSystem>.Instance().Level != -1)
			{
				PlayWithPlayerLevel();
			}
			else
			{
				_playDelegator = PlayWithPlayerLevel;
			}
		}
	}

	private void Stop()
	{
		SoundManager.StopEvent(_soundInstanceId);
		_soundInstanceId = 0u;
	}

	private void PlayDefault()
	{
		_soundInstanceId = SoundManager.PlayEvent(_audioClip, SoundPosition.Chase(base.gameObject, _offset), exclusive: true);
	}

	private void PlayWithPlayerLevel()
	{
		SoundSwitch[] soundSwitches = new SoundSwitch[2]
		{
			GameSystem<StatisticsSystem>.Instance().GetPlayerLevelSoundSwitch(),
			SoundSwitch.Set("region_role", GameManager.Region.Role().ToString())
		};
		_soundInstanceId = SoundManager.PlayEvent(_audioClip, SoundPosition.Chase(base.gameObject, _offset), soundSwitches, exclusive: true);
	}

	private void StatisticsSystem_LevelChanged(int prev, int current)
	{
		if (_playDelegator != null)
		{
			_playDelegator();
			_playDelegator = null;
		}
	}
}
