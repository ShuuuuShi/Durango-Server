using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class AlarmRewardQueue : MonoBehaviour, IUIInitializable
{
	private enum MotionType
	{
		None,
		Success,
		LevelUp
	}

	private struct RewardStruct
	{
		public string Key;

		public float At;

		public EffectOption EffectOption;

		public Args Args;
	}

	public struct Args
	{
		public string Main;

		public string Sub;

		public ItemIcon Icon;

		public ItemIcon[] ExtraIcons;

		public float IconScale;

		public Action Action;
	}

	[Serializable]
	private struct EffectOption
	{
		public AlarmRewardWidget Component;

		[Tooltip("우선 순위 (높을수록 먼저 나옴)")]
		public int Priority;

		[Tooltip("Effect Sound")]
		public SoundEventType Sound;

		[Tooltip("캐릭터 모션 타입")]
		public MotionType MotionType;
	}

	[Serializable]
	[EnumType(typeof(AlarmGroup.RewardEffectType))]
	private class EffectOptionList : EnumKeyList
	{
		[SerializeField]
		private List<EffectOption> _values;

		public List<EffectOption> Values => _values;

		public EffectOption Get(AlarmGroup.RewardEffectType type)
		{
			int num = IndexOf((int)type);
			if (num == -1)
			{
				return default(EffectOption);
			}
			return _values[num];
		}
	}

	public interface IMessageGroup
	{
		bool IsPlaying();

		void PauseToNext();

		void Resume();
	}

	[SerializeField]
	private EffectOptionList _effectOptionList;

	private readonly Dictionary<int, List<IMessageGroup>> _externalGroups = new Dictionary<int, List<IMessageGroup>>();

	private List<RewardStruct>[] _messageQueues;

	private AlarmRewardWidget[] _currentAlarms;

	private bool _isAlarmPause;

	private readonly HashSet<string> _alarmPauseSet = new HashSet<string>();

	private AlarmGroup _parent;

	void IUIInitializable.Init()
	{
		int num = -1;
		for (int i = 0; i < _effectOptionList.Values.Count; i++)
		{
			num = Mathf.Max(num, _effectOptionList.Values[i].Component.Group);
		}
		_messageQueues = new List<RewardStruct>[num + 1];
		_currentAlarms = new AlarmRewardWidget[num + 1];
		for (int j = 0; j < _messageQueues.Length; j++)
		{
			_messageQueues[j] = new List<RewardStruct>();
		}
		_parent = UIUtility.FindComponentInParent<AlarmGroup>(base.gameObject);
		_parent.VisibleController.Changed += delegate
		{
			UpdatePauseState();
		};
		Singleton<BlurController>.Instance().BlurStateChanged += delegate
		{
			UpdatePauseState();
		};
		UIBase.UIOpened += UpdatePauseState;
		UIBase.UIClosed += UpdatePauseState;
		TooltipBase.ModalOpened += delegate
		{
			UpdatePauseState();
		};
		TooltipBase.ModalClosed += delegate
		{
			UpdatePauseState();
		};
		UpdatePauseState();
	}

	private void Start()
	{
		int i = 0;
		for (int count = _effectOptionList.Values.Count; i < count; i++)
		{
			EffectOption effectOption = _effectOptionList.Values[i];
			if (!(effectOption.Component != null))
			{
				continue;
			}
			effectOption.Component.Disabled += OnAlarmDisable;
			bool flag = false;
			int j = 0;
			for (int size = KUtility.GetSize(_currentAlarms); j < size; j++)
			{
				if (effectOption.Component == _currentAlarms[j])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				effectOption.Component.gameObject.SetActive(value: false);
			}
		}
		bool flag2 = true;
		int k = 0;
		for (int size2 = KUtility.GetSize(_messageQueues); k < size2; k++)
		{
			if (_messageQueues[k].Count > 0)
			{
				flag2 = false;
				break;
			}
		}
		if (flag2)
		{
			base.enabled = false;
		}
	}

	private void OnDisable()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_currentAlarms); i < size; i++)
		{
			_currentAlarms[i] = null;
		}
	}

	private void Update()
	{
		if (UIManager.IsLoadingCurtain || _isAlarmPause)
		{
			return;
		}
		float time = Time.time;
		int i = 0;
		for (int size = KUtility.GetSize(_messageQueues); i < size; i++)
		{
			List<RewardStruct> list = _messageQueues[i];
			if (_currentAlarms[i] != null || list.Count == 0 || time < list[0].At)
			{
				continue;
			}
			int num = -1;
			int num2 = -1;
			int j = 0;
			for (int size2 = KUtility.GetSize(list); j < size2; j++)
			{
				RewardStruct rewardStruct = list[j];
				if (rewardStruct.At > time)
				{
					break;
				}
				if (rewardStruct.At <= time && num < rewardStruct.EffectOption.Priority)
				{
					num = rewardStruct.EffectOption.Priority;
					num2 = j;
				}
			}
			if (num2 == -1)
			{
				continue;
			}
			RewardStruct reward = list[num2];
			List<IMessageGroup> list2 = _externalGroups.Get(reward.EffectOption.Component.Group);
			bool flag = false;
			int k = 0;
			for (int size3 = KUtility.GetSize(list2); k < size3; k++)
			{
				list2[k].PauseToNext();
				if (list2[k].IsPlaying())
				{
					flag = true;
				}
			}
			if (!flag)
			{
				list.RemoveAt(num2);
				_currentAlarms[i] = Show(reward);
			}
		}
	}

	private void OnAlarmDisable(AlarmRewardWidget alarm)
	{
		int i = 0;
		for (int size = KUtility.GetSize(_currentAlarms); i < size; i++)
		{
			if (_currentAlarms[i] == alarm)
			{
				_currentAlarms[i] = null;
				break;
			}
		}
		bool flag = true;
		int j = 0;
		for (int size2 = KUtility.GetSize(_messageQueues); j < size2; j++)
		{
			if (_messageQueues[j].Count > 0 || _currentAlarms[j] != null)
			{
				flag = false;
				continue;
			}
			List<IMessageGroup> list = _externalGroups.Get(j);
			int k = 0;
			for (int size3 = KUtility.GetSize(list); k < size3; k++)
			{
				list[k].Resume();
			}
		}
		if (flag)
		{
			base.enabled = false;
		}
	}

	public void AddMessageGroup(int group, IMessageGroup comp)
	{
		List<IMessageGroup> list = _externalGroups.Get(group);
		if (list == null)
		{
			list = new List<IMessageGroup>();
			_externalGroups[group] = list;
		}
		list.Add(comp);
	}

	public void Register(string key, Args args, AlarmGroup.RewardEffectType type, float delay)
	{
		RemoveQueue(key);
		EffectOption effectOption = _effectOptionList.Get(type);
		if (!(effectOption.Component == null))
		{
			RewardStruct rewardStruct = default(RewardStruct);
			rewardStruct.Key = key;
			rewardStruct.At = Time.time + delay;
			rewardStruct.EffectOption = effectOption;
			rewardStruct.Args = args;
			RewardStruct reward = rewardStruct;
			AddToQueue(reward);
		}
	}

	public void Stop(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return;
		}
		RemoveQueue(key);
		AlarmRewardWidget[] currentAlarms = _currentAlarms;
		foreach (AlarmRewardWidget alarmRewardWidget in currentAlarms)
		{
			if (!(alarmRewardWidget == null) && alarmRewardWidget.Key == key)
			{
				alarmRewardWidget.gameObject.SetActive(value: false);
				break;
			}
		}
	}

	private void RemoveQueue(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return;
		}
		List<RewardStruct>[] messageQueues = _messageQueues;
		foreach (List<RewardStruct> list in messageQueues)
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].Key == key)
				{
					list.RemoveAt(j);
					break;
				}
			}
		}
	}

	public void Pause([NotNull] string key, bool pause)
	{
		if (pause)
		{
			_alarmPauseSet.Add(key);
		}
		else
		{
			_alarmPauseSet.Remove(key);
		}
		UpdatePauseState();
	}

	private void AddToQueue(RewardStruct reward)
	{
		List<RewardStruct> list = _messageQueues[reward.EffectOption.Component.Group];
		int num = -1;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (list[i].At > reward.At)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			list.Add(reward);
		}
		else
		{
			list.Insert(num, reward);
		}
		base.enabled = true;
	}

	private AlarmRewardWidget Show(RewardStruct reward)
	{
		AlarmRewardWidget component = reward.EffectOption.Component;
		component.Set(reward.Key, reward.Args);
		if (!string.IsNullOrEmpty(reward.EffectOption.Sound))
		{
			SoundManager.PlayEvent(reward.EffectOption.Sound, SoundPosition.Empty, GameSystem<StatisticsSystem>.Instance().GetPlayerLevelSoundSwitch());
		}
		PlayRewardMotion(reward.EffectOption.MotionType);
		if (reward.Args.Action != null)
		{
			reward.Args.Action();
		}
		return component;
	}

	private void PlayRewardMotion(MotionType motionType)
	{
		if (!GameSystem<CombatSystem>.Instance().CombatMode)
		{
			string text = null;
			switch (motionType)
			{
			case MotionType.Success:
				text = "Craft_Success";
				break;
			case MotionType.LevelUp:
				text = "Avatar_Levelup";
				break;
			}
			if (!string.IsNullOrEmpty(text))
			{
				PlayerController.MotionUpdater.Motion(text);
			}
		}
	}

	private void UpdatePauseState()
	{
		BlurController.Mask state = Singleton<BlurController>.Instance().State;
		bool pause = (UIBase.CurrentUI == null && state != 0) || !_parent.Visible || TooltipBase.HasModal() || _alarmPauseSet.Count > 0;
		AlarmPause(pause);
	}

	private void AlarmPause(bool pause)
	{
		if (_isAlarmPause == pause)
		{
			return;
		}
		_isAlarmPause = pause;
		int i = 0;
		for (int size = KUtility.GetSize(_currentAlarms); i < size; i++)
		{
			if (!(_currentAlarms[i] == null))
			{
				_currentAlarms[i].Pause(pause);
			}
		}
	}
}
