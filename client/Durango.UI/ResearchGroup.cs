using System;
using Durango.Logic;
using Durango.UI.Control;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Laboratory;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("Research")]
public class ResearchGroup : UIBase
{
	[SerializeField]
	private UITitle _title;

	[SerializeField]
	private ResearchPageWidget _researchPage;

	private Artifact _target;

	private bool _resetFlag;

	private AvailablePersonalResearch? _research;

	private void Start()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.PersonalResearch, delegate(InteractionObject obj)
		{
			Artifact targetComponent = obj.GetTargetComponent<Artifact>();
			if (!(targetComponent == null))
			{
				Open(targetComponent);
			}
		});
		GameSystem<StatusEffectSystem>.Instance().StatusEffectsUpdated += OnStatusEffectUpdate;
		_researchPage.ResearchStarted += OnStartResearch;
		SetChildrenActive(activated: false);
	}

	public override bool Open()
	{
		throw new NotSupportedException();
	}

	protected override bool TryOpen()
	{
		_researchPage.GetComponent<UIWidget>().alpha = 0f;
		_resetFlag = true;
		_research = null;
		return base.TryOpen();
	}

	public void Open([NotNull] Artifact artifact)
	{
		_title.Object.SetTitle(artifact.LocalizedName);
		base.Open();
		_target = artifact;
		UpdateResearchList();
	}

	private void UpdateResearchList()
	{
		if (_target == null)
		{
			return;
		}
		UIManager.Popup.LoadingRing.AttachToWidget(_researchPage.gameObject);
		PropKey prop = _target.GetPropKey();
		ResearchSystem.GetAvailablePersonalResearch(prop, delegate(AvailablePersonalResearch? result)
		{
			if (base.IsOpened && !(prop.EntityId != _target.EntityId))
			{
				_research = result;
				UIManager.Popup.LoadingRing.DetachFromWidget(_researchPage.gameObject);
				_researchPage.GetComponent<UIWidget>().alpha = 1f;
				_researchPage.Set(result, _resetFlag);
				_resetFlag = false;
			}
		});
	}

	private void OnStatusEffectUpdate(Durango.Logic.StatusEffects effects)
	{
		if (base.IsOpened)
		{
			AvailablePersonalResearch? research = _research;
			if (research.HasValue && !(effects.EntityId != GameManager.PlayerId))
			{
				_researchPage.UpdateResearchState();
			}
		}
	}

	private void OnStartResearch(string key)
	{
		if (_target == null)
		{
			return;
		}
		PersonalResearch info = ((!string.IsNullOrEmpty(key)) ? SingletonDict<string, PersonalResearch>.Get(key) : null);
		if (info == null)
		{
			return;
		}
		PropKey propKey = _target.GetPropKey();
		Action<bool> onOkCancel = delegate(bool ok)
		{
			if (ok)
			{
				ResearchSystem.StartPersonalResearch(propKey, key, delegate(bool success)
				{
					if (success)
					{
						UIManager.Alarm.ShowNotify(T._("{0} 효과를 받았습니다.", info.Name), "icon_option_effect", major: false);
					}
				});
			}
		};
		MessageBox messageBox = UIManager.MessageBox;
		if (_research.HasValue)
		{
			ResearchCategory category = _research.Value.GetCategory();
			string currentPersonalResearch = ResearchSystem.GetCurrentPersonalResearch(category);
			PersonalResearch personalResearch = ((!string.IsNullOrEmpty(currentPersonalResearch)) ? SingletonDict<string, PersonalResearch>.Get(currentPersonalResearch) : null);
			if (personalResearch != null)
			{
				if (currentPersonalResearch == key)
				{
					messageBox.ShowPayConfirm(info.Amount, info.Currency, string.Format("{0}\n{1}\n{2}", T._("이미 <em>{0}</em> 효과를 받고 있습니다.", info.Name), T._("다시 연구하면 지속시간이 최대로 초기화 됩니다."), T._("계속 진행 하시겠습니까?")), onOkCancel);
					return;
				}
				messageBox.ShowPayConfirm(info.Amount, info.Currency, T._("<em>{0}</em> 효과로 변경하시겠습니까?", info.Name), T._("<alert><alert_icon/> {0} 효과가 취소됩니다.</alert>", personalResearch.Name), onOkCancel);
				return;
			}
		}
		messageBox.ShowPayConfirm(info.Amount, info.Currency, T._("<em>{0}</em> 효과를 받습니다.", info.Name), string.Format("{0}\n{1}", T._("<alert_icon/> 효과는 {0}동안 지속됩니다.", TimedeltaFormatter.Format(info.Duration)), T._("<alert_icon/> 다른 효과를 선택하면 진행 중인 효과는 취소됩니다.")), onOkCancel);
	}
}
