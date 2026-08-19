using System;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.UI.Control;
using L10N;
using Shared.Laboratory;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class ClanResearchPopup : TooltipBase
{
	private const string BlurKey = "ClanResearchPopup";

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private KWidgetScrollView _scrollViewForStatusEffects;

	[SerializeField]
	private UILabel _textStatusEffect;

	[SerializeField]
	private UILabel _textAmount;

	[SerializeField]
	private UILabel _textDuration;

	[SerializeField]
	private SelectableButton _button;

	[SerializeField]
	private RectLayout _layout;

	private string _laboratoryId;

	private Point2 _laboratoryTile;

	private string _researchId;

	protected override void Start()
	{
		base.Start();
		SelectableButton button = _button;
		button.Clicked = (Action)Delegate.Combine(button.Clicked, new Action(ButtonClicked));
		_layout.UpdateLayout();
	}

	public void Show(string laboratoryId, Point2 laboratoryTile, string researchId)
	{
		ClanResearch clanResearch = SingletonDict<string, ClanResearch>.Get(researchId);
		if (clanResearch != null)
		{
			_laboratoryId = laboratoryId;
			_laboratoryTile = laboratoryTile;
			_researchId = researchId;
			_textName.text = clanResearch.Name;
			_textStatusEffect.text = GetStatusEffectText(clanResearch.Effect);
			_textAmount.text = Inventory.CurrencyFormat(clanResearch.Amount, clanResearch.Currency);
			_textDuration.text = TimedeltaFormatter.Format(clanResearch.Duration, 2, "min");
			_scrollViewForStatusEffects.ResetPosition();
			Show();
		}
	}

	public static string GetStatusEffectText(ResearchEffect researchEffect)
	{
		StatusEffectTemplate statusEffectTemplate = StatusEffectTemplateYaml.GetStatusEffectTemplate(researchEffect.StatusEffectId, researchEffect.Level);
		if (statusEffectTemplate != null)
		{
			string text = StatusEffect.EffectsText(statusEffectTemplate.GetEffects(researchEffect.Level));
			if (string.IsNullOrEmpty(text))
			{
				text = statusEffectTemplate.Description;
			}
			return (text + "\n\n" + GetStatusEffectApplyLimits(researchEffect)).Trim();
		}
		return string.Empty;
	}

	private static string GetStatusEffectApplyLimits(ResearchEffect researchEffect)
	{
		return researchEffect.ApplyLimits switch
		{
			EffectApplyLimits.ClanTerritory => T._("부족 영토 내에서만 적용"), 
			EffectApplyLimits.Always => T._("어디에서나 적용"), 
			_ => string.Empty, 
		};
	}

	private void ButtonClicked()
	{
		GameSystem<ResearchSystem>.Instance().StartClanResearch(_laboratoryId, _laboratoryTile, _researchId);
		Hide();
	}

	protected override void OnShow()
	{
		base.OnShow();
		BlurController.BlurOn("ClanResearchPopup", BlurController.Mask.UI);
	}

	protected override void OnHide()
	{
		base.OnHide();
		BlurController.BlurOff("ClanResearchPopup");
	}

	protected override void OnTryConfirmOnModal()
	{
		ButtonClicked();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _button;
	}
}
