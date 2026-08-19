using System;
using Durango.Environment;
using Durango.Logic;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class AlarmPvpIsland : MonoBehaviour, IUIInitializable
{
	[Serializable]
	private struct SoundEvent
	{
		public string WeatherId;

		public string BgmName;

		public SoundEventType Sound;
	}

	[SerializeField]
	private string _bgmSwitchGroupName;

	[SerializeField]
	private SoundEvent[] _soundEvents;

	[SerializeField]
	private SoundEvent _gameFinish;

	void IUIInitializable.Init()
	{
		if (!GameManager.Region.IsPvpIsland())
		{
			base.enabled = false;
			return;
		}
		GameSystem<PvpIslandSystem>.Instance().GameStarted += delegate
		{
			WeatherManager weatherManager = Singleton<WeatherManager>.Instance();
			weatherManager.WeatherChanged = (Action<string>)Delegate.Combine(weatherManager.WeatherChanged, (Action<string>)delegate(string weatherString)
			{
				SoundEvent[] soundEvents = _soundEvents;
				for (int i = 0; i < soundEvents.Length; i++)
				{
					SoundEvent soundEvent = soundEvents[i];
					if (!(soundEvent.WeatherId != weatherString))
					{
						PlaySounds(soundEvent);
						break;
					}
				}
			});
			GameSystem<PvpIslandSystem>.Instance().Win += delegate
			{
				PlaySounds(_gameFinish);
			};
			GameSystem<PvpIslandSystem>.Instance().PlayerDied += delegate
			{
				PlaySounds(_gameFinish);
			};
		};
	}

	private void PlaySounds(SoundEvent soundEvent)
	{
		if (!string.IsNullOrEmpty(soundEvent.BgmName))
		{
			SetBgmSwitch(soundEvent.BgmName);
		}
		SoundManager.PlayEvent(soundEvent.Sound);
	}

	private void SetBgmSwitch(string stateName)
	{
		if (!string.IsNullOrEmpty(_bgmSwitchGroupName) && !string.IsNullOrEmpty(stateName))
		{
			Singleton<BgmManager>.Instance().SetSwitch(SoundSwitch.Set(_bgmSwitchGroupName, stateName));
		}
	}
}
