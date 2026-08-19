using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class SelectPetTaskPopup : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UISprite _petPortraitSprite;

	[SerializeField]
	private UILabel _petNameLabel;

	[SerializeField]
	private UILabel _petInfoLabel;

	[SerializeField]
	private KScrollView _taskList;

	private string _titleText;

	private Messages.Pet? _pet;

	private Artifact _cage;

	private IEnumerable<string> _tasks;

	private Predicate<PetTask> _filter;

	private Func<string, bool> _onSelect;

	private bool _reset = true;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		base.OnAwake();
		_taskList.Nodes.Init(delegate(GameObject obj)
		{
			obj.GetComponent<SelectPetTaskItemWidget>().Clicked += OnSelectTaskItem;
		});
		ResetArguments();
	}

	private void ResetArguments()
	{
		_titleText = null;
		_cage = null;
		_pet = null;
		_tasks = null;
		_filter = null;
		_onSelect = null;
		_reset = true;
	}

	protected override void OnHide()
	{
		base.OnHide();
		ResetArguments();
	}

	public SelectPetTaskPopup SetTitle(string text)
	{
		_titleText = text;
		return this;
	}

	public SelectPetTaskPopup SetCage(Artifact cage)
	{
		_cage = cage;
		return this;
	}

	public SelectPetTaskPopup SetFilter(Predicate<PetTask> filter)
	{
		_filter = filter;
		return this;
	}

	public SelectPetTaskPopup SetPet(Messages.Pet pet)
	{
		_pet = pet;
		return this;
	}

	public SelectPetTaskPopup SetOnSelected(Func<string, bool> onSelect)
	{
		_onSelect = onSelect;
		return this;
	}

	protected override void FillData()
	{
		_titleLabel.text = _titleText;
		FillPetData();
		_taskList.Nodes.BeginLoad();
		if (_tasks == null)
		{
			Messages.Pet? pet = _pet;
			if (pet.HasValue && _cage != null)
			{
				PetManager.GetAvailableTask(_pet.Value.EntityId, _cage.GetPropKey(), delegate(AvailableTask? result)
				{
					if (result.HasValue)
					{
						_tasks = result.Value.Tasks;
						MarkAsChanged();
					}
				});
			}
		}
		else if (_pet.HasValue)
		{
			foreach (string task in _tasks)
			{
				PetTask petTask = SingletonDict<string, PetTask>.Get(task);
				if (petTask != null && (_filter == null || _filter(petTask)))
				{
					_taskList.Nodes.GetNext().GetComponent<SelectPetTaskItemWidget>().Set(_pet.Value, task);
				}
			}
		}
		_taskList.Nodes.EndLoad();
	}

	private void FillPetData()
	{
		Messages.Pet? pet = _pet;
		if (pet.HasValue)
		{
			Messages.Pet value = _pet.Value;
			Animal animal = SingletonDict<int, Animal>.Get(value.GetAnimalType());
			_petNameLabel.text = value.GetPetName(includeRank: true);
			_petPortraitSprite.spriteName = ((animal != null) ? animal.Portrait : string.Empty);
			_petInfoLabel.SetText(GetPetDescription(value));
		}
	}

	protected override void UpdateLayout()
	{
		GetComponent<RectLayoutComponent>().UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		_taskList.Reposition(_reset, !_reset);
		_reset = false;
	}

	private void OnSelectTaskItem(string taskId)
	{
		if (_onSelect == null || _onSelect(taskId))
		{
			Hide();
		}
	}

	private static SyncString GetPetDescription(Messages.Pet pet)
	{
		Gauge energy = pet.Stat.Hungry;
		return new SyncString(delegate(out string text, out float period)
		{
			double currentTime = Gauge.CurrentTime;
			float num = ((energy != null) ? energy.Get(currentTime) : 0f);
			float num2 = ((energy != null) ? energy.Max(currentTime) : 0f);
			int exp = pet.Statistics.Exp;
			int requiredExp = pet.Statistics.RequiredExp;
			text = $"{LocalizeUtil.FormatLevel(pet.Statistics.Level)} <weak>({exp}/{requiredExp})</weak>  <bar/>  [51A3C3][icon=pet_energy][-] {num:0}/{num2:0}";
			double? nextChangedAt = Gauge.GetNextChangedAt((energy != null) ? energy.Determination : null, currentTime);
			if (!nextChangedAt.HasValue)
			{
				period = 0f;
			}
			else
			{
				period = (float)(nextChangedAt.Value - currentTime);
			}
		});
	}
}
