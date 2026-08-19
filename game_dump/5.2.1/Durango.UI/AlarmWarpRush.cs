using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Shared.Season2;
using UnityEngine;

namespace Durango.UI;

public class AlarmWarpRush : MonoBehaviour, IUIInitializable, AlarmRewardQueue.IMessageGroup
{
	[Flags]
	public enum Alarm
	{
		None = 0,
		DateChanged = 1,
		PhaseChanged = 2,
		DayCameAlarm = 4,
		NightCameAlarm = 8
	}

	[Serializable]
	private struct AlarmStruct
	{
		public GameObject Prefab;

		public SoundEventType Sound;
	}

	private class AlarmObject
	{
		private GameObject _gameObject;

		private TweenerPlayer _tweenerPlayer;

		private UILabel _label;

		private SoundEventType _sound;

		public bool IsPlaying => _gameObject.activeSelf;

		public AlarmObject(AlarmStruct alarmStruct, GameObject parent)
		{
			_gameObject = parent.AddChild(alarmStruct.Prefab);
			_tweenerPlayer = _gameObject.GetComponent<TweenerPlayer>();
			_label = GetComponentByName<UILabel>(_gameObject, "Label Main");
			_gameObject.SetActive(value: false);
			_sound = alarmStruct.Sound;
		}

		public void SetLabelText(string text)
		{
			if (_label != null)
			{
				_label.text = text;
			}
		}

		public void Play()
		{
			if (_tweenerPlayer != null)
			{
				_gameObject.SetActive(value: true);
				_tweenerPlayer.Play();
				SoundManager.PlayEvent(_sound);
			}
		}

		private static T GetComponentByName<T>(GameObject gameObject, string name) where T : MonoBehaviour
		{
			Transform transform = KUtility.FindTransformByName(gameObject, name);
			if (transform != null)
			{
				return transform.GetComponent<T>();
			}
			return null;
		}
	}

	[SerializeField]
	private AlarmStruct _alarmDateChanged;

	[SerializeField]
	private AlarmStruct _alarmPhaseChanged;

	[SerializeField]
	private AlarmStruct _alarmDayCame;

	[SerializeField]
	private AlarmStruct _alarmNightCame;

	[SerializeField]
	private SoundEventType _dayOrNightComingSound;

	[SerializeField]
	[EnumList(typeof(ResourceType), true, 0, -1)]
	private SoundEventType[] _gatherResourceSounds;

	[SerializeField]
	private string _bgmSwitchGroupName;

	[SerializeField]
	private string _bgmSwitchForReady;

	[SerializeField]
	private string _bgmSwitchForFinish;

	[SerializeField]
	private string _bgmSwitchForFailed;

	[SerializeField]
	private string[] _bgmSwitchesForDateChange;

	private readonly Dictionary<Alarm, AlarmObject> _alarmObjects = new Dictionary<Alarm, AlarmObject>();

	private AlarmObject _curPlayingAlarm;

	private bool _isPaused;

	private Alarm _alarms;

	void IUIInitializable.Init()
	{
		if (!GameManager.Region.IsWarpRush())
		{
			base.enabled = false;
			return;
		}
		TimeGauge.IsSunUpChanged += AddDayOrNightAlarm;
		GameSystem<WarpRushSystem>.Instance().PhaseChanged += AddPhaseChangedAlarm;
		GameSystem<WarpRushSystem>.Instance().DayChanged += AddDateChangedAlarm;
		GameSystem<WarpRushSystem>.Instance().RegionResourceGathered += WarpRush_RegionResourceGathered;
		GameSystem<WarpRushSystem>.Instance().GameStarted += WarpRushSystem_GameStarted;
		_alarmObjects[Alarm.DateChanged] = new AlarmObject(_alarmDateChanged, base.gameObject);
		_alarmObjects[Alarm.PhaseChanged] = new AlarmObject(_alarmPhaseChanged, base.gameObject);
		_alarmObjects[Alarm.DayCameAlarm] = new AlarmObject(_alarmDayCame, base.gameObject);
		_alarmObjects[Alarm.NightCameAlarm] = new AlarmObject(_alarmNightCame, base.gameObject);
		TimeGauge.RegisterTimeCallback(TimeGauge.SunriseBegin - 2, delegate
		{
			AlarmDayOrNightComing(T._("낮이 다가옵니다. 기름 안개가 옅어집니다."));
		});
		TimeGauge.RegisterTimeCallback(TimeGauge.SunsetEnd - 2, delegate
		{
			AlarmDayOrNightComing(T._("밤이 다가옵니다. 기름 안개가 짙어집니다."));
		});
	}

	private void LateUpdate()
	{
		if (_isPaused || IsPlaying())
		{
			return;
		}
		if (GameSystem<WarpRushSystem>.Instance().DaysPassed < 1)
		{
			_alarms = Alarm.None;
			return;
		}
		Alarm alarm = (Alarm)((int)_alarms & (0 - _alarms));
		if (alarm > Alarm.None)
		{
			if (alarm == Alarm.DateChanged)
			{
				PlayDateChangedSound();
			}
			_curPlayingAlarm = _alarmObjects.Get(alarm);
			if (_curPlayingAlarm != null)
			{
				_curPlayingAlarm.Play();
			}
			_alarms &= ~alarm;
		}
		else
		{
			_curPlayingAlarm = null;
		}
	}

	private void AddDayOrNightAlarm()
	{
		_alarms |= (Alarm)((!TimeGauge.IsSunUp) ? 8 : 4);
	}

	private void AddDateChangedAlarm()
	{
		_alarmObjects.Get(Alarm.DateChanged)?.SetLabelText(T._("생존 {0}일 차", GameSystem<WarpRushSystem>.Instance().DaysPassed));
		_alarms |= Alarm.DateChanged;
	}

	private void AddPhaseChangedAlarm()
	{
		_alarms |= Alarm.PhaseChanged;
	}

	private void AlarmDayOrNightComing(string warningText)
	{
		if (GameSystem<WarpRushSystem>.Instance().DaysPassed >= 1)
		{
			UIManager.SystemMsg("WarpRush_Warning", warningText);
			GameSystem<SocialSystem>.Instance().AddSystemChat(warningText, string.Empty);
			SoundManager.PlayEvent(_dayOrNightComingSound);
		}
	}

	private void PlayDateChangedSound()
	{
		int daysPassed = GameSystem<WarpRushSystem>.Instance().DaysPassed;
		if (0 <= daysPassed && daysPassed < _bgmSwitchesForDateChange.Length)
		{
			SetBgmSwitch(_bgmSwitchesForDateChange[daysPassed]);
		}
	}

	private void WarpRush_RegionResourceGathered(ResourceType stoneType)
	{
		if (ResourceType.AlphaStone <= stoneType && (int)stoneType < _gatherResourceSounds.Length)
		{
			SoundManager.PlayEvent(_gatherResourceSounds[(int)stoneType]);
		}
	}

	private void WarpRushSystem_GameStarted()
	{
		SetBgmSwitch(_bgmSwitchForReady);
	}

	private void SetBgmSwitch(string stateName)
	{
		if (!string.IsNullOrEmpty(_bgmSwitchGroupName) && !string.IsNullOrEmpty(stateName))
		{
			Singleton<BgmManager>.Instance().SetSwitch(SoundSwitch.Set(_bgmSwitchGroupName, stateName));
		}
	}

	[ExposedInEditor(null)]
	private void Test()
	{
		AddPhaseChangedAlarm();
		AddDayOrNightAlarm();
		AddDateChangedAlarm();
	}

	public bool IsPlaying()
	{
		if (_curPlayingAlarm != null)
		{
			return _curPlayingAlarm.IsPlaying;
		}
		return false;
	}

	public void PauseToNext()
	{
		_isPaused = true;
	}

	public void Resume()
	{
		_isPaused = false;
	}
}
