using System;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class GrowCagePetListWidget : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _capacityLabel;

	[SerializeField]
	private NodesScrollView _petList;

	private bool _resetFlag;

	public event Action<Pet> Selected;

	public event Action PetAdded;

	public event Action<Pet> SkipTaskCheat;

	void IUIInitializable.Init()
	{
		_titleLabel.text = string.Format("{0} [icon=img_loading_unknown_question1]", T._("축사 동물"));
		UIEventListener uIEventListener = UIEventListener.Get(_titleWidget.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Set(null, T._("귀속한 동물을 축사에 넣을 수 있으며 동행하지 않고도 별도의 훈련, 생산을 진행 할 수 있습니다."), 400);
			UIWidget childSprite = UIUtility.GetChildSprite(_titleLabel, "img_loading_unknown_question1");
			if (childSprite == null)
			{
				widgetTooltipControl.Show(10f);
			}
			else
			{
				widgetTooltipControl.Show(childSprite, Vector2.zero, 10f);
			}
		});
		_petList.Nodes.Init(delegate(GameObject obj)
		{
			GrowCagePetListItemWidget component = obj.GetComponent<GrowCagePetListItemWidget>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickPetItem));
			component.SkipTaskCheat += this.SkipTaskCheat;
		});
	}

	private void OnDisable()
	{
		_resetFlag = false;
	}

	public void Set(Artifact artifact)
	{
		GrowCage? growCage = PetUtil.GetGrowCage(artifact);
		if (growCage.HasValue)
		{
			_capacityLabel.text = (growCage.Value.Size - growCage.Value.RemainSize).ToString().ToEncodedColor(PresetColor.UIYellow) + " / " + growCage.Value.Size.ToString().ToEncodedColor(PresetColor.UIMoreLightGray);
			_petList.Nodes.BeginLoad();
			if (growCage.Value.RemainSize > 0)
			{
				GrowCagePetListItemWidget component = _petList.Nodes.GetNext().GetComponent<GrowCagePetListItemWidget>();
				component.SetAsAddable();
				component.Disabled = false;
			}
			Pet[] data = growCage.Value.Pets.Data;
			int i = 0;
			for (int size = KUtility.GetSize(data); i < size; i++)
			{
				Pet pet = data[i];
				TaskStatus? taskStatus = growCage.Value.GetTaskStatus(pet.EntityId);
				GrowCagePetListItemWidget component2 = _petList.Nodes.GetNext().GetComponent<GrowCagePetListItemWidget>();
				component2.Set(pet, taskStatus);
				component2.Disabled = !string.IsNullOrEmpty(pet.TamerEntityId) && pet.TamerEntityId != GameManager.PlayerId;
			}
			_petList.Nodes.EndLoad();
			_petList.Reposition(!_resetFlag, _resetFlag);
			_resetFlag = true;
		}
	}

	public void Select(string id)
	{
		for (int i = 0; i < _petList.Nodes.Count; i++)
		{
			GrowCagePetListItemWidget component = _petList.Nodes[i].GetComponent<GrowCagePetListItemWidget>();
			component.Selected = component.Pet.HasValue && component.Pet.Value.EntityId == id;
		}
	}

	private void OnClickPetItem()
	{
		GrowCagePetListItemWidget growCagePetListItemWidget = Selectable.Current as GrowCagePetListItemWidget;
		if (growCagePetListItemWidget == null)
		{
			return;
		}
		if (!growCagePetListItemWidget.Pet.HasValue)
		{
			if (this.PetAdded != null)
			{
				this.PetAdded();
			}
		}
		else if (this.Selected != null)
		{
			this.Selected(growCagePetListItemWidget.Pet.Value);
		}
	}

	private void OnSkipTaskCheat(Pet pet)
	{
		if (this.SkipTaskCheat != null)
		{
			this.SkipTaskCheat(pet);
		}
	}
}
