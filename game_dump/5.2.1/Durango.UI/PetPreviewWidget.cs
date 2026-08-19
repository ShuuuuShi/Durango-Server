using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PetPreviewWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _petNameLabel;

	[SerializeField]
	private UILabel _petRaceLabel;

	[SerializeField]
	private UILabel _petInfoLabel;

	[SerializeField]
	private UIModelViewer _petPreviewViewer;

	[SerializeField]
	private PetMilestoneInfoWidget _milestoneInfo;

	private Messages.Pet _pet;

	private bool _isInit;

	public event Action<Messages.Pet> Renamed;

	public event Action<Messages.Pet, int> MilestonePicked
	{
		add
		{
			_milestoneInfo.MilestonePicked += value;
		}
		remove
		{
			_milestoneInfo.MilestonePicked -= value;
		}
	}

	public event Action<Messages.Pet> ActiveSkillPicked
	{
		add
		{
			_milestoneInfo.ActiveSkillPicked += value;
		}
		remove
		{
			_milestoneInfo.ActiveSkillPicked -= value;
		}
	}

	public event Action<Messages.Pet> MilestoneHelpClicked
	{
		add
		{
			_milestoneInfo.MilestoneHelpClicked += value;
		}
		remove
		{
			_milestoneInfo.MilestoneHelpClicked -= value;
		}
	}

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		UIEventListener uIEventListener = UIEventListener.Get(_petNameLabel.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (this.Renamed != null)
			{
				this.Renamed(_pet);
			}
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(_petInfoLabel.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickAnimalType));
	}

	public void Set(Messages.Pet pet, PetsInfo petsInfo)
	{
		Init();
		_pet = pet;
		_petNameLabel.text = pet.GetPetName(includeRank: true) + " [c][FFFFFF96][icon=icon_chat_edit][-][/c]";
		_milestoneInfo.Set(pet);
		Yaml.Pet pet2 = SingletonDict<int, Yaml.Pet>.Get(pet.EntityType);
		Animal animal = ((pet2 != null) ? SingletonDict<int, Animal>.Get(pet2.VehicleEntityType) : null);
		if (animal == null)
		{
			_petRaceLabel.text = string.Empty;
			_petInfoLabel.text = string.Empty;
			_petPreviewViewer.gameObject.SetActive(value: false);
			return;
		}
		_petRaceLabel.text = animal.Name;
		_petInfoLabel.text = PetUtil.GetPetInfoString(pet);
		string prefabPath = animal.PrefabPath;
		bool flag = pet.Stat.Life == null || pet.Stat.Life.Ratio() <= 0f;
		bool isOld = pet.Stat.IsOld;
		bool flag2 = petsInfo.GrazedPets.Data != null && petsInfo.GrazedPets.Data.Any((Messages.Pet p) => p.EntityId == pet.EntityId);
		bool num = pet.CageInfo.HasValue && !string.IsNullOrEmpty(pet.CageInfo.Value.RegionId);
		Action<GameObject> a = null;
		if (!num && !flag2 && pet2.IsRidable)
		{
			a = (Action<GameObject>)Delegate.Combine(a, _petPreviewViewer.SetupSaddle());
		}
		a = (Action<GameObject>)Delegate.Combine(a, (!flag) ? _petPreviewViewer.DefaultAnimalPlay("idle", "stand", isOld) : _petPreviewViewer.DefaultDeadAnimalPlay(isOld));
		_petPreviewViewer.SetPlainModel(prefabPath, new UIModelViewer.Arguments
		{
			CameraAngle = 35f,
			Rotation = 140f,
			Loaded = a
		});
	}

	private void OnClickAnimalType(GameObject obj)
	{
		string[] eatableTags = _pet.Stat.EatableTags;
		List<string> list = new List<string>();
		int i = 0;
		for (int size = KUtility.GetSize(eatableTags); i < size; i++)
		{
			Yaml.Tag tag = SingletonDict<string, Yaml.Tag>.Get(eatableTags[i]);
			list.Add((tag != null) ? tag.Name.ToString() : eatableTags[i]);
		}
		if (KUtility.GetSize(eatableTags) != 0)
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(null, T._("{0:l:{}|, }", list));
			widgetTooltipControl.AutoPosition = false;
			widgetTooltipControl.Show(60f);
			UILabel component = obj.GetComponent<UILabel>();
			Vector3 position = component.localCorners[0];
			position.x += component.printedSize.x - (float)component.fontSize * 0.5f;
			position = component.transform.TransformPoint(position);
			position = widgetTooltipControl.transform.parent.InverseTransformPoint(position);
			widgetTooltipControl.Widget.SetPosition(position, 1f, 0f);
			widgetTooltipControl.IntoSafeArea();
			widgetTooltipControl.HideArrow();
		}
	}
}
