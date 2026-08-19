using System;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class DomesticCagePetListWidget : MonoBehaviour, IUIInitializable
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

	public event Action<DomesticationInfo> Selected;

	public event Action ReinAdded;

	public event Action<DomesticationInfo> SkipProgressCheat;

	void IUIInitializable.Init()
	{
		_titleLabel.text = string.Format("{0} [icon=img_loading_unknown_question1]", T._("축사 동물"));
		UIEventListener uIEventListener = UIEventListener.Get(_titleWidget.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate(GameObject go)
		{
			OnClickTitle(go);
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(_titleWidget.gameObject);
		uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, TooltipBase.ToHover(OnClickTitle));
		_petList.Nodes.Init(delegate(GameObject obj)
		{
			Selectable component = obj.GetComponent<Selectable>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickPetItem));
			obj.GetComponent<DomesticCagePetListItemWidget>().SkipProgressCheat += OnSkipProgressCheat;
		});
	}

	private void OnDisable()
	{
		_resetFlag = false;
	}

	public void Set(Artifact artifact)
	{
		if (artifact.ArtifactState.DomesticCage.HasValue)
		{
			DomesticCage value = artifact.ArtifactState.DomesticCage.Value;
			_capacityLabel.text = $"{(value.Size - value.RemainSize).ToString().ToEncodedColor(PresetColor.UIYellow)} / {value.Size.ToString().ToEncodedColor(PresetColor.UIMoreLightGray)}";
			_petList.Nodes.BeginLoad();
			if (value.RemainSize > 0)
			{
				DomesticCagePetListItemWidget component = _petList.Nodes.GetNext().GetComponent<DomesticCagePetListItemWidget>();
				component.SetAsAddable();
			}
			DomesticationInfo[] reins = value.Reins;
			int i = 0;
			for (int size = KUtility.GetSize(reins); i < size; i++)
			{
				DomesticationInfo rein = reins[i];
				DomesticCagePetListItemWidget component2 = _petList.Nodes.GetNext().GetComponent<DomesticCagePetListItemWidget>();
				component2.Set(rein);
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
			DomesticCagePetListItemWidget component = _petList.Nodes[i].GetComponent<DomesticCagePetListItemWidget>();
			component.SetSelect(component.Rein.HasValue && component.Rein.Value.ItemId == id);
		}
	}

	public void PlayYammyAnimation(string id)
	{
		for (int i = 0; i < _petList.Nodes.Count; i++)
		{
			DomesticCagePetListItemWidget component = _petList.Nodes[i].GetComponent<DomesticCagePetListItemWidget>();
			if (component.Rein.HasValue && component.Rein.Value.ItemId == id)
			{
				component.PlayYammyAnimation();
			}
		}
	}

	private void OnClickPetItem()
	{
		DomesticCagePetListItemWidget component = Selectable.Current.GetComponent<DomesticCagePetListItemWidget>();
		if (component == null)
		{
			return;
		}
		if (!component.Rein.HasValue)
		{
			if (this.ReinAdded != null)
			{
				this.ReinAdded();
			}
		}
		else if (this.Selected != null)
		{
			this.Selected(component.Rein.Value);
		}
	}

	private void OnSkipProgressCheat(DomesticationInfo target)
	{
		if (this.SkipProgressCheat != null)
		{
			this.SkipProgressCheat(target);
		}
	}

	private WidgetTooltipControl OnClickTitle(GameObject go)
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Set(null, T._("포획한 동물은 길들일 수 있으며 야생 동물보다 우월한 속성을 지닐 수 있습니다. 다만 길들이기에 실패하면 동물을 잃게됩니다."), 400);
		UIWidget childSprite = UIUtility.GetChildSprite(_titleLabel, "img_loading_unknown_question1");
		if (childSprite == null)
		{
			widgetTooltipControl.Show(10f);
		}
		else
		{
			widgetTooltipControl.Show(childSprite, Vector2.zero, 10f);
		}
		return widgetTooltipControl;
	}
}
