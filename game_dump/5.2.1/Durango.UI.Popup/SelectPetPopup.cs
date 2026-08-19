using System;
using System.Collections.Generic;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class SelectPetPopup : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private UILabel _capacityLabel;

	[SerializeField]
	private UIWidget _petListContainer;

	[SerializeField]
	private KScrollView _petList;

	[SerializeField]
	private SelectableButton _cancelButton;

	[SerializeField]
	private SelectableButton _confirmButton;

	private string _titleText;

	private string _infoText;

	private Pair<int, int>? _capacity;

	private IEnumerable<Pet> _pets;

	private Action<Pet> _onConfirm;

	private Pet? _selected;

	private bool _reset = true;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		base.OnAwake();
		SelectableButton cancelButton = _cancelButton;
		cancelButton.Clicked = (Action)Delegate.Combine(cancelButton.Clicked, new Action(OnCancel));
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(OnConfirm));
		_petList.Nodes.Init(delegate(GameObject obj)
		{
			Selectable component = obj.GetComponent<Selectable>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickItem));
		});
		ResetArguments();
	}

	protected override void OnHide()
	{
		base.OnHide();
		ResetArguments();
		UIManager.Popup.FindTooltip<ItemInfoTooltip>().Hide();
	}

	private void ResetArguments()
	{
		_titleText = null;
		_infoText = null;
		_capacity = null;
		_pets = null;
		_onConfirm = null;
		_selected = null;
		_cancelButton.Text = T._("취소");
		_confirmButton.Text = T._("확인");
		_reset = true;
	}

	public SelectPetPopup SetTitle(string text)
	{
		_titleText = text;
		return this;
	}

	public SelectPetPopup SetInfo(string text)
	{
		_infoText = text;
		return this;
	}

	public SelectPetPopup SetCapacity(int current, int max)
	{
		_capacity = new Pair<int, int>(current, max);
		return this;
	}

	public SelectPetPopup SetList(IEnumerable<Pet> pets)
	{
		_pets = pets;
		return this;
	}

	public SelectPetPopup SetOnConfirm(Action<Pet> onConfirm)
	{
		_onConfirm = onConfirm;
		return this;
	}

	public SelectPetPopup SetConfirmButtonText(string text)
	{
		_confirmButton.Text = text;
		return this;
	}

	protected override void FillData()
	{
		_titleLabel.text = _titleText;
		_infoLabel.text = _infoText;
		_petList.Nodes.BeginLoad();
		if (_pets != null)
		{
			foreach (Pet pet in _pets)
			{
				_petList.Nodes.GetNext().GetComponent<SelectPetItemWidget>().Set(pet);
			}
		}
		_petList.Nodes.EndLoad();
		Pet? selected = _selected;
		SelectPet(selected.HasValue ? _selected.Value.EntityId : null);
	}

	protected override void UpdateLayout()
	{
		_capacityLabel.gameObject.SetActive(_capacity.HasValue);
		_infoLabel.gameObject.SetActive(!string.IsNullOrEmpty(_infoText));
		RectLayoutComponent component = GetComponent<RectLayoutComponent>();
		int safeWidth = UIManager.SafeWidth;
		int safeHeight = UIManager.SafeHeight;
		safeWidth = Mathf.Min(safeWidth - 200, 600);
		safeHeight -= 120;
		component.UpdateLayout(safeWidth, safeHeight);
		_petList.Panel.UpdateAnchors();
		_petList.UpdateLayout();
		float num = (float)_petListContainer.height - _petList.ContentsLength;
		if (num > 0f)
		{
			component.UpdateLayout(safeWidth, Mathf.Max(400f, (float)safeHeight - num));
			_petList.Panel.UpdateAnchors();
		}
		if (_reset)
		{
			_petList.MoveTo(0f, instant: true);
		}
		else
		{
			_petList.MoveTo(_petList.CurrentOffset, instant: false);
		}
		_reset = false;
	}

	protected override void OnTryConfirmOnModal()
	{
		OnConfirm();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}

	protected override SelectableButton GetCancelButton(out bool showShortcut)
	{
		showShortcut = true;
		return _cancelButton;
	}

	private void OnCancel()
	{
		Hide();
	}

	private void OnConfirm()
	{
		Action<Pet> onConfirm = _onConfirm;
		Pet? selected = _selected;
		Hide();
		if (selected.HasValue)
		{
			onConfirm?.Invoke(selected.Value);
		}
	}

	private void OnClickItem()
	{
		SelectPetItemWidget selectPetItemWidget = Selectable.Current as SelectPetItemWidget;
		if (!(selectPetItemWidget == null))
		{
			SelectPet(selectPetItemWidget.Pet.EntityId);
		}
	}

	private void SelectPet(string id)
	{
		_selected = null;
		for (int i = 0; i < _petList.Nodes.Count; i++)
		{
			SelectPetItemWidget component = _petList.Nodes[i].GetComponent<SelectPetItemWidget>();
			if (component.Pet.EntityId == id)
			{
				component.Selected = true;
				_selected = component.Pet;
			}
			else
			{
				component.Selected = false;
			}
		}
		if (_selected.HasValue)
		{
			ItemInfoTooltip itemInfoTooltip = UIManager.Popup.Tooltip<ItemInfoTooltip>();
			itemInfoTooltip.Set(_selected.Value);
			if (itemInfoTooltip.IsVisible)
			{
				itemInfoTooltip.Refresh();
			}
			else
			{
				itemInfoTooltip.AutoPosition = false;
				itemInfoTooltip.Widget.height = base.Widget.height;
				itemInfoTooltip.Show();
				itemInfoTooltip.HideIgnoreParent = base.transform;
				itemInfoTooltip.SetPosition(base.Widget, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-10f, 0f));
			}
		}
		else
		{
			ItemInfoTooltip itemInfoTooltip2 = UIManager.Popup.FindTooltip<ItemInfoTooltip>();
			if (itemInfoTooltip2.IsVisible)
			{
				itemInfoTooltip2.Hide();
			}
		}
		if (_capacity.HasValue)
		{
			int item = _capacity.Value.Item1;
			int item2 = _capacity.Value.Item2;
			if (_selected.HasValue)
			{
				_capacityLabel.text = string.Format("{0}   <em>{1}</em>/{2}", T._("축사 크기"), item + _selected.Value.Stat.Size, item2);
			}
			else
			{
				_capacityLabel.text = string.Format("{0}   {1}/{2}", T._("축사 크기"), item, item2);
			}
		}
	}
}
