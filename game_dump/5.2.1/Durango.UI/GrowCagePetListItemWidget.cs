using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class GrowCagePetListItemWidget : SelectableWidget
{
	[SerializeField]
	private UIWidget _contentsWidget;

	[SerializeField]
	private UISprite _portrait;

	[SerializeField]
	private ItemGradeViewer _gradeViewer;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private UISprite _progressSprite;

	[SerializeField]
	private UISprite _taskIconSprite;

	[SerializeField]
	private UISprite _taskFinishedSprite;

	[SerializeField]
	private GameObject _taskIdleObject;

	[SerializeField]
	private UISprite _addableSprite;

	[SerializeField]
	private UILabel _oldLabel;

	[SerializeField]
	private GameObject _cheatButton;

	private float _duration;

	private float _dirtyAt;

	public Messages.Pet? Pet { get; private set; }

	public TaskStatus? Task { get; private set; }

	public event Action<Messages.Pet> SkipTaskCheat;

	private void Start()
	{
		if (!Debug.isDebugBuild)
		{
			return;
		}
		UIEventListener.Get(_cheatButton).onClick = delegate
		{
			if (Pet.HasValue && this.SkipTaskCheat != null)
			{
				this.SkipTaskCheat(Pet.Value);
			}
		};
	}

	private void Update()
	{
		if (Pet.HasValue)
		{
			if (_dirtyAt > 0f && _dirtyAt < Time.time)
			{
				Set(Pet.Value, Task);
			}
			else if (Task.HasValue)
			{
				double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
				double num = Task.Value.Until - predictedServerTime;
				float fillAmount = Mathf.Clamp01(1f - (float)(num / (double)_duration));
				_progressSprite.fillAmount = fillAmount;
			}
		}
	}

	public void Set(Messages.Pet pet, TaskStatus? task)
	{
		_dirtyAt = 0f;
		Pet = pet;
		Task = task;
		_addableSprite.gameObject.SetActive(value: false);
		_contentsWidget.gameObject.SetActive(value: true);
		SetTaskProgress(task);
		Animal animal = SingletonDict<int, Animal>.Get(pet.GetAnimalType());
		_nameLabel.text = pet.GetPetName(includeRank: true);
		_portrait.spriteName = ((animal != null) ? animal.Portrait : string.Empty);
		_infoLabel.SetText(new SyncString(delegate(out string text, out float period)
		{
			int lv = (Pet.HasValue ? Pet.Value.Statistics.Level : 0);
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			double? num = (Task.HasValue ? new double?(Task.Value.Until - predictedServerTime) : null);
			text = string.Format("{0}  <bar/>  {1}", arg1: (!num.HasValue) ? T._("대기 중") : ((!(num.Value > 0.0)) ? T._("완료") : TimedeltaFormatter.Format(num.Value)), arg0: LocalizeUtil.FormatLevel(lv));
			if (num.HasValue && num.Value > 0.0)
			{
				period = (float)(num.Value % (double)TimedeltaFormatter.CurrentMinUnit());
			}
			else
			{
				period = 0f;
			}
		}));
		_oldLabel.text = ((!pet.Stat.IsOld) ? string.Empty : string.Format("<alert>{0}</alert>", T._("노화됨")));
		_gradeViewer.SetOptions(0.5f, upward: true, 5);
		_gradeViewer.SettingBegin();
		if (pet.Stat.Tags != null)
		{
			foreach (KeyValuePair<string, int> tag in pet.Stat.Tags)
			{
				_gradeViewer.AddTagData(tag.Key, tag.Value);
			}
		}
		_gradeViewer.SettingEnd();
		_cheatButton.gameObject.SetActive(Debug.isDebugBuild && task.HasValue);
		Update();
	}

	private void SetTaskProgress(TaskStatus? task)
	{
		if (!task.HasValue)
		{
			_progressSprite.gameObject.SetActive(value: false);
			_taskIconSprite.gameObject.SetActive(value: false);
			_taskIdleObject.gameObject.SetActive(value: true);
			_taskFinishedSprite.gameObject.SetActive(value: false);
			return;
		}
		PetTask petTask = ((!string.IsNullOrEmpty(task.Value.TaskId)) ? SingletonDict<string, PetTask>.Get(task.Value.TaskId) : null);
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double num = task.Value.Until - predictedServerTime;
		_duration = petTask?.Duration ?? ((float)(task.Value.Until - task.Value.Since));
		if (num > 0.0)
		{
			_taskIconSprite.spriteName = ((petTask != null) ? petTask.Icon : string.Empty);
			_progressSprite.gameObject.SetActive(value: true);
			_taskIconSprite.gameObject.SetActive(value: true);
			_taskIdleObject.gameObject.SetActive(value: false);
			_taskFinishedSprite.gameObject.SetActive(value: false);
			_dirtyAt = Times.UnixTimeToUnityTime(task.Value.Until);
		}
		else
		{
			_progressSprite.gameObject.SetActive(value: false);
			_taskIconSprite.gameObject.SetActive(value: false);
			_taskIdleObject.gameObject.SetActive(value: false);
			_taskFinishedSprite.gameObject.SetActive(value: true);
		}
	}

	public void SetAsAddable()
	{
		Pet = null;
		Task = null;
		_addableSprite.gameObject.SetActive(value: true);
		_contentsWidget.gameObject.SetActive(value: false);
		_cheatButton.gameObject.SetActive(value: false);
		_oldLabel.text = string.Empty;
	}
}
