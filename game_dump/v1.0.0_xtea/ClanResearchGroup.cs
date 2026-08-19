using System;
using InteractionData;
using ItemSystem;
using L10N;
using Messages;
using Shared.Laboratory;
using Shared.StatusEffect;
using TimerData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ClanResearchGroup : UIBase
{
	private const string prefix = "Research";

	[SerializeField]
	private GameObject _container;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private UIScrollView _scrollViewForStatusEffects;

	[SerializeField]
	private UILabel _textStatusEffect;

	[SerializeField]
	private UISpriteLabel _textAmount;

	[SerializeField]
	private UILabel _textDuration;

	[SerializeField]
	private DefaultSelectableButton _button;

	[SerializeField]
	private GameObject _touchBox;

	private string _researchId;

	private Laboratory _laboratory;

	private void Awake()
	{
		_container.gameObject.SetActive(false);
		DefaultSelectableButton button = _button;
		button.Clicked = (Action)Delegate.Combine(button.Clicked, new Action(ButtonClicked));
		UIEventListener uIEventListener = UIEventListener.Get(_touchBox);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, (UIEventListener.BoolDelegate)delegate(GameObject go, bool press)
		{
			if (!press)
			{
				ForceClose();
			}
		});
	}

	private void Start()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResearchPlant, delegate(InteractionObject target)
		{
			InteractionResearch(Interaction.ResearchPlant, target);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResearchAnimal, delegate(InteractionObject target)
		{
			InteractionResearch(Interaction.ResearchAnimal, target);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResearchMine, delegate(InteractionObject target)
		{
			InteractionResearch(Interaction.ResearchMine, target);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResearchClothes, delegate(InteractionObject target)
		{
			InteractionResearch(Interaction.ResearchClothes, target);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResearchTool, delegate(InteractionObject target)
		{
			InteractionResearch(Interaction.ResearchTool, target);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResearchCook, delegate(InteractionObject target)
		{
			InteractionResearch(Interaction.ResearchCook, target);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResearchConstruction, delegate(InteractionObject target)
		{
			InteractionResearch(Interaction.ResearchConstruction, target);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResearchSurvival, delegate(InteractionObject target)
		{
			InteractionResearch(Interaction.ResearchSurvival, target);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResearchEcology, delegate(InteractionObject target)
		{
			InteractionResearch(Interaction.ResearchEcology, target);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResearchAttack, delegate(InteractionObject target)
		{
			InteractionResearch(Interaction.ResearchAttack, target);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResearchRecovery, delegate(InteractionObject target)
		{
			InteractionResearch(Interaction.ResearchRecovery, target);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResearchDefense, delegate(InteractionObject target)
		{
			InteractionResearch(Interaction.ResearchDefense, target);
		});
	}

	private void InteractionResearch(Interaction interaction, InteractionObject target)
	{
		Artifact targetComponent = target.GetTargetComponent<Artifact>();
		if ((Object)(object)targetComponent == (Object)null)
		{
			return;
		}
		Laboratory laboratory = targetComponent.GetArtifactComponent<Laboratory>();
		if (laboratory == null)
		{
			return;
		}
		laboratory.RefreshResearchState(delegate
		{
			if (laboratory.GetNowResearching())
			{
				UIManager.SystemMsg(T._("이미 연구가 진행 중입니다."));
			}
			else
			{
				Open(interaction, laboratory);
			}
		});
	}

	private void Open(Interaction interaction, Laboratory laboratory)
	{
		string clanResearchId = GetClanResearchId(interaction);
		ClanResearch clanResearch = SingletonDict<string, ClanResearch>.Get(clanResearchId);
		if (clanResearch != null)
		{
			_researchId = clanResearchId;
			_laboratory = laboratory;
			_icon.spriteName = clanResearch.icon;
			_textName.text = clanResearch.name;
			_textStatusEffect.text = GetStatusEffectText(clanResearch.effect);
			_textAmount.text = ItemSystem.Inventory.CurrencyFormat(clanResearch.amount, clanResearch.currency);
			_textDuration.text = TimerSystem.TimeToString(clanResearch.duration, TimePeriod.Min);
			Open();
		}
	}

	private static string GetClanResearchId(Interaction interaction)
	{
		string text = interaction.ToString();
		if (text.Length > "Research".Length)
		{
			return text.Substring("Research".Length).ToLower();
		}
		return null;
	}

	private static string GetStatusEffectText(ResearchEffect researchEffect)
	{
		StatusEffectTemplate statusEffectTemplate = StatusEffectTemplateYaml.GetStatusEffectTemplate(researchEffect.status_effect_id, 1);
		if (statusEffectTemplate != null)
		{
			string arg = ((statusEffectTemplate.type == EffectType.Modifier && statusEffectTemplate.effects.Count > 0) ? StatusEffectsControl.ModifiersText(statusEffectTemplate.effects) : ((statusEffectTemplate.type != EffectType.Survival || statusEffectTemplate.effects.Count <= 0) ? ((string)statusEffectTemplate.description) : StatusEffectsControl.SurvivalEffectText(statusEffectTemplate.effects)));
			return $"{arg}\n{GetStatusEffectApplyLimits(researchEffect)}";
		}
		return string.Empty;
	}

	private static string GetStatusEffectApplyLimits(ResearchEffect researchEffect)
	{
		return researchEffect.apply_limits switch
		{
			EffectApplyLimits.ClanTerritory => T._("부족 영토 내에서만 적용"), 
			EffectApplyLimits.Always => T._("어디에서나 적용"), 
			_ => string.Empty, 
		};
	}

	protected override bool OnOpen()
	{
		_container.gameObject.SetActive(true);
		_scrollViewForStatusEffects.ResetPosition();
		return true;
	}

	protected override bool OnClose()
	{
		_container.gameObject.SetActive(false);
		return true;
	}

	private void ButtonClicked()
	{
		Connections.Frontend.Send(new StartResearch
		{
			EntityId = _laboratory.EntityId,
			Tile = _laboratory.WorldTile,
			Id = _researchId
		}).On<OK>(delegate
		{
			_laboratory.RefreshResearchState();
		});
		Close();
	}
}
