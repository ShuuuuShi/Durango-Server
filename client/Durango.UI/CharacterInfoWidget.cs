using System;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class CharacterInfoWidget : UIWidget
{
	[SerializeField]
	private CharacterWidgetBase _characterWidget;

	[SerializeField]
	private CharacterAbilityWidget _abilityWidget;

	[SerializeField]
	private GameObject _openProfileTouchBox;

	[SerializeField]
	private GameObject _openClanUITouchBox;

	[SerializeField]
	private GameObject _honorFlagSelector;

	[SerializeField]
	private UILabel _honorFlagLabel;

	[SerializeField]
	private SelectableButton _moreAbilityButton;

	protected override void OnStart()
	{
		base.OnStart();
		if (Application.isPlaying)
		{
			UIEventListener.Get(_openClanUITouchBox).onClick = delegate
			{
				UIManager.FindScript<ClanGroup>().Open();
			};
			UIEventListener uIEventListener = UIEventListener.Get(_openProfileTouchBox);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
			{
				PlayerInfoPopup.RequestShow(PlayerBehavior.LocalPlayer.EntityId);
			});
			UIEventListener uIEventListener2 = UIEventListener.Get(_honorFlagSelector);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
			{
				CharacterInfoGroup.SetHonorFlagSelector(SetHonorFlagSelector_OnSelected);
			});
			_moreAbilityButton.Text = T._("상세 능력치");
			SelectableButton moreAbilityButton = _moreAbilityButton;
			moreAbilityButton.Clicked = (Action)Delegate.Combine(moreAbilityButton.Clicked, (Action)delegate
			{
				UIManager.FindScript<CharacterStatusGroup>().Open();
			});
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			GameSystem<StatisticsSystem>.Instance().ExpGained += OnUpdateExp;
			GameSystem<ClanSystem>.Instance().ClanInfoUpdated += OnUpdateClan;
			GameSystem<StatisticsSystem>.Instance().StatisticsUpdated += RefreshAbility;
			SetHonorFlag((!(PlayerBehavior.LocalPlayer != null)) ? null : PlayerBehavior.LocalPlayer.Display.Accessory);
			_characterWidget.Refresh();
			_abilityWidget.Refersh();
			UpdateLayout();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			GameSystem<StatisticsSystem>.Instance().ExpGained -= OnUpdateExp;
			GameSystem<ClanSystem>.Instance().ClanInfoUpdated -= OnUpdateClan;
			GameSystem<StatisticsSystem>.Instance().StatisticsUpdated -= RefreshAbility;
		}
	}

	private void SetHonorFlag(string id)
	{
		Accessory accessory = ((!string.IsNullOrEmpty(id)) ? SingletonDict<string, Accessory>.Get(id) : null);
		if (accessory == null)
		{
			_honorFlagLabel.text = T._("깃발 없음");
		}
		else
		{
			_honorFlagLabel.text = accessory.Name;
		}
		SelectableWidget component = _honorFlagSelector.GetComponent<SelectableWidget>();
		if (component == null)
		{
			_honorFlagLabel.color = ((accessory != null) ? PresetColor.UIYellow : new Color(1f, 1f, 1f, 0.5f));
		}
		else
		{
			component.Selected = accessory != null;
		}
	}

	private void SetHonorFlagSelector_OnSelected(string id)
	{
		GameSystem<EquipSystem>.Instance().AttachAccessory(id);
		SetHonorFlag(id);
	}

	private void OnUpdateExp(ExpGained exp)
	{
		if (!(exp.EntityId != PlayerBehavior.LocalPlayer.EntityId))
		{
			_characterWidget.Refresh();
			UpdateLayout();
		}
	}

	private void OnUpdateClan()
	{
		_characterWidget.Refresh();
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		GetComponent<RectLayoutComponent>().UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void RefreshAbility()
	{
		_abilityWidget.Refersh();
		UpdateLayout();
	}
}
