using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Animal;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PetTaskProgressWidget : MonoBehaviour, IUIInitializable
{
	private struct IconData
	{
		public string Icon;

		public KeyValuePair<string, int>? ItemIcon;

		public void SetIcon(ItemIconTex comp)
		{
			if (!string.IsNullOrEmpty(Icon))
			{
				comp.SetIcon(Icon);
			}
			if (ItemIcon.HasValue)
			{
				comp.SetIcon(ItemIcon.Value.Key, ItemIcon.Value.Value);
			}
		}
	}

	[SerializeField]
	private UIWidget _timerWidget;

	[SerializeField]
	private UILabel _timerLabel;

	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UISprite _finishedSprite;

	[SerializeField]
	private UISprite _progressSprite;

	[SerializeField]
	private UISprite _predictProgressSprite;

	private Messages.Pet _pet;

	private TaskStatus _taskStatus;

	private double? _modifiedTaskTime;

	private float _duration;

	private readonly List<IconData> _iconList = new List<IconData>();

	private int? _currentIconIndex;

	private ItemIconTex[] _iconTextures;

	private float _dirtyAt;

	void IUIInitializable.Init()
	{
		_iconTextures = new ItemIconTex[2];
		_iconTextures[0] = _iconTexture;
		_iconTextures[1] = _iconTexture.transform.parent.gameObject.AddChild(_iconTexture.gameObject).GetComponent<ItemIconTex>();
	}

	public void Set(Messages.Pet pet, TaskStatus task, double? modifiedTaskTime = null)
	{
		_dirtyAt = 0f;
		_pet = pet;
		_taskStatus = task;
		_modifiedTaskTime = modifiedTaskTime;
		if (_modifiedTaskTime.HasValue && Math.Abs(_modifiedTaskTime.Value - _taskStatus.Until) < 1.0)
		{
			_modifiedTaskTime = null;
		}
		PetTask petTask = ((!string.IsNullOrEmpty(task.TaskId)) ? SingletonDict<string, PetTask>.Get(task.TaskId) : null);
		_duration = petTask?.Duration ?? ((float)(_taskStatus.Until - _taskStatus.Since));
		_iconList.Clear();
		_currentIconIndex = null;
		if (Connections.Frontend.GetPredictedServerTime() < _taskStatus.Until)
		{
			_timerWidget.gameObject.SetActive(value: true);
			_finishedSprite.gameObject.SetActive(value: false);
			_progressSprite.gameObject.SetActive(value: true);
			GameObject obj = _predictProgressSprite.gameObject;
			double? modifiedTaskTime2 = _modifiedTaskTime;
			obj.SetActive(modifiedTaskTime2.HasValue);
			_timerLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
				double num = _taskStatus.Until - predictedServerTime;
				double? modifiedTaskTime3 = _modifiedTaskTime;
				if (!modifiedTaskTime3.HasValue)
				{
					text = ((!(num > 0.0)) ? string.Empty : TimedeltaFormatter.Format(num));
				}
				else
				{
					double seconds = Maths.Clamp(_modifiedTaskTime.Value - predictedServerTime, 0.0, _duration);
					text = T._("{0} [preset=animation_arrow] <em>{1}</em>", TimedeltaFormatter.Format(num), TimedeltaFormatter.Format(seconds));
				}
				if (num > 0.0)
				{
					period = (float)(num % (double)TimedeltaFormatter.CurrentMinUnit());
				}
				else
				{
					period = 0f;
				}
			}));
			if (petTask != null)
			{
				int level = pet.Statistics.Level;
				if (level > 0)
				{
					switch (petTask.Type)
					{
					case PetTaskType.Production:
						foreach (KeyValuePair<string, float[]> item in petTask.ProducedPrototype)
						{
							_iconList.Add(new IconData
							{
								ItemIcon = new KeyValuePair<string, int>(item.Key, level)
							});
						}
						break;
					case PetTaskType.Training:
						_iconList.Add(new IconData
						{
							Icon = "pet_move_compi_01"
						});
						_iconList.Add(new IconData
						{
							Icon = "pet_move_compi_02"
						});
						break;
					}
				}
			}
			_dirtyAt = Times.UnixTimeToUnityTime(_taskStatus.Until);
		}
		else
		{
			_timerWidget.gameObject.SetActive(value: false);
			_finishedSprite.gameObject.SetActive(value: true);
			_progressSprite.gameObject.SetActive(value: false);
			_predictProgressSprite.gameObject.SetActive(value: false);
		}
		Update();
	}

	private void Update()
	{
		if (_dirtyAt > 0f && _dirtyAt < Time.time)
		{
			Set(_pet, _taskStatus, _modifiedTaskTime);
			return;
		}
		RefreshProgress();
		RefreshIcons();
	}

	private void RefreshProgress()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		float num = (float)(_taskStatus.Until - predictedServerTime);
		float fillAmount = 1f - Mathf.Clamp01((!(_duration > 0f)) ? 0f : (num / _duration));
		_progressSprite.fillAmount = fillAmount;
		double? modifiedTaskTime = _modifiedTaskTime;
		if (modifiedTaskTime.HasValue)
		{
			num = (float)(_modifiedTaskTime.Value - predictedServerTime);
			fillAmount = 1f - Mathf.Clamp01((!(_duration > 0f)) ? 0f : (num / _duration));
			_predictProgressSprite.fillAmount = fillAmount;
		}
	}

	private void RefreshIcons()
	{
		float num = ((_iconList.Count <= 1) ? 0f : (Time.time % (float)_iconList.Count));
		int? currentIconIndex = _currentIconIndex;
		if (!currentIconIndex.HasValue || _currentIconIndex.Value != (int)num)
		{
			SetIcons((int)num);
		}
		if (_iconList.Count > 1)
		{
			float num2 = num % 1f;
			_iconTextures[0].alpha = ((!(num2 < 0.9f)) ? (1f - (num2 - 0.9f) / 0.1f) : 1f);
			_iconTextures[1].alpha = ((!(num2 < 0.9f)) ? ((num2 - 0.9f) / 0.1f) : 0f);
		}
		else
		{
			_iconTextures[0].alpha = 1f;
		}
	}

	private void SetIcons(int index)
	{
		_currentIconIndex = index;
		if (_iconList.Count == 0)
		{
			ItemIconTex[] iconTextures = _iconTextures;
			foreach (ItemIconTex itemIconTex in iconTextures)
			{
				itemIconTex.alpha = 0f;
			}
			return;
		}
		IconData iconData = _iconList[index];
		if (_iconList.Count == 1)
		{
			iconData.SetIcon(_iconTextures[0]);
			return;
		}
		IconData iconData2 = _iconList[(index + 1) % _iconList.Count];
		iconData.SetIcon(_iconTextures[0]);
		iconData2.SetIcon(_iconTextures[1]);
	}
}
