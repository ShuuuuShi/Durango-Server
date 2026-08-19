using System;
using System.Collections.Generic;
using Durango.Logic.Timer;
using Durango.Network;
using Durango.UI.Popup;
using Durango.Utils;
using InteractionData;
using Messages;
using Shared.Ability;
using Shared.Battle;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class ContextActionGroupBase : UIBase
{
	[SerializeField]
	protected ContextActionButtonsBase _actionButtons;

	protected Vector3 _baseActionPos;

	private readonly List<InteractionMenuData> _actionList = new List<InteractionMenuData>();

	private float _touchLockTime;

	private readonly Observable<bool> _isShow = new Observable<bool>();

	public Observable<bool> IsShow => _isShow;

	protected virtual void Start()
	{
		_actionButtons.MenuClicked += OnClickMenuButton;
		PlayerBehavior.LocalPlayer.TileChanged += delegate
		{
			RefreshActionList();
		};
		PlayerBehavior.LocalPlayer.Driver.RidingStateChanged += RefreshActionList;
		PlayerBehavior.LocalPlayer.Driver.VehicleChanged += RefreshActionList;
		PlayerBehavior.LocalPlayer.Died += delegate
		{
			RefreshActionList();
		};
		PlayerBehavior.LocalPlayer.Revived += delegate
		{
			RefreshActionList();
		};
		Observable<bool> isMoving = PlayerBehavior.LocalPlayer.IsMoving;
		isMoving.Changed = (Action<bool>)Delegate.Combine(isMoving.Changed, (Action<bool>)delegate
		{
			RefreshActionList();
		});
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += delegate
		{
			RefreshActionList();
		};
		Observable<double> warpholeSearchedAt = GameSystem<InteractionSystem>.Instance().WarpholeSearchedAt;
		warpholeSearchedAt.Changed = (Action<double>)Delegate.Combine(warpholeSearchedAt.Changed, new Action<double>(RefreshSearchWarpholeCooltime));
		GameSystem<TimerSystem>.Instance().StartSubjectProgress += OnStartSubjectProgress;
		GameSystem<TimerSystem>.Instance().FinishedSubjectProgress += OnFinishSubjectProgress;
		Singleton<PetManager>.Instance().PetActiveSkillUsed += OnPetActiveSkillUsed;
		Singleton<PetManager>.Instance().PetActiveSkillCanceled += OnPetActiveSkillCanceled;
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		_baseActionPos = -Vector3.one;
	}

	private void OnStartSubjectProgress(string subject)
	{
		SetVisible(visible: false, "Timer");
	}

	private void OnFinishSubjectProgress(string subject, bool isInterrupted)
	{
		SetVisible(visible: true, "Timer", 0.3f);
	}

	public void OnClickMenuButton(InteractionMenuData menu)
	{
		if (!(Time.time < _touchLockTime))
		{
			GameSystem<InteractionSystem>.Instance().DoNoneTargetAction(menu);
			_touchLockTime = Time.time + 0.5f;
		}
	}

	protected void ShowTooltip(ContextActionButtonBase button, bool show)
	{
		if (show)
		{
			if (!string.IsNullOrEmpty(button.Description))
			{
				ActionTooltipBase actionTooltipBase = UIManager.Popup.Tooltip<ActionTooltipBase>();
				actionTooltipBase.Set(button);
				Vector2 offset;
				if (UIManager.IsPortraitScreen)
				{
					actionTooltipBase.Direction = TooltipBase.TooltipDirection.Vertical;
					offset = new Vector2(0f, 10f);
				}
				else
				{
					actionTooltipBase.Direction = TooltipBase.TooltipDirection.Horizontal;
					offset = new Vector2(10f, 0f);
				}
				actionTooltipBase.Show(button.gameObject, offset);
			}
		}
		else
		{
			ActionTooltipBase actionTooltipBase2 = UIManager.Popup.FindTooltip<ActionTooltipBase>();
			actionTooltipBase2.Hide();
		}
	}

	public void RefreshActionList()
	{
		if (GameSystem<CombatSystem>.Instance().CombatMode)
		{
			_actionList.Clear();
		}
		else
		{
			GameSystem<InteractionSystem>.Instance().GetContextActionList(_actionList);
		}
		_actionButtons.SetActions(_actionList);
		_isShow.Value = _actionList.Count > 0;
	}

	private void RefreshSearchWarpholeCooltime(double searchedAt)
	{
		double until = searchedAt + (double)GameSystem<StatisticsSystem>.Instance().GetDeriveds(Derived.PoiSearchingCooldownTime);
		_actionButtons.SetActionCooltime(Interaction.SearchWarphole, null, searchedAt, until);
	}

	private void OnPetActiveSkillUsed(PetActiveSkillUsed msg)
	{
		PetManager petManager = Singleton<PetManager>.Instance();
		string playerPetId = petManager.GetPlayerPetId();
		if (string.IsNullOrEmpty(playerPetId))
		{
			return;
		}
		PetSkillStates playerPetSkillStates = petManager.PlayerPetSkillStates;
		PetSkillStates.State state = playerPetSkillStates.GetState(msg.SkillId);
		Yaml.PetActiveSkill petActiveSkill = ((state != null) ? PetActiveSkills.Get(state.Id, state.Rank) : null);
		if (petActiveSkill != null)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			double until = predictedServerTime + petActiveSkill.Cooltime;
			_actionButtons.SetActionCooltime(Interaction.PetActiveSkill, msg.SkillId, predictedServerTime, until);
			PetAI petObject = petManager.GetPetObject(playerPetId);
			if (!string.IsNullOrEmpty(petActiveSkill.CategoryIcon) && petActiveSkill.Duration > 0f && petObject != null)
			{
				Durango.Logic.Timer.Timer timer = new Durango.Logic.Timer.Timer(playerPetId, null, petActiveSkill.Duration);
				ActionProgressGauge actionProgressGauge = Durango.Logic.Timer.Timer.Play<ActionProgressGauge>(timer);
				actionProgressGauge.Set(petActiveSkill.CategoryIcon);
				Transform bodyPartTransform = petObject.TargetAnimal.GetBodyPartTransform(BodyPart.Head);
				actionProgressGauge.SetTarget(bodyPartTransform.gameObject, Vector3.up * 150f);
			}
		}
	}

	private void OnPetActiveSkillCanceled(PetActiveSkillCanceled msg)
	{
		_actionButtons.ClearActionCooltime(Interaction.PetActiveSkill, msg.SkillId);
		string playerPetId = Singleton<PetManager>.Instance().GetPlayerPetId();
		if (!string.IsNullOrEmpty(playerPetId))
		{
			GameSystem<TimerSystem>.Instance().Stop(playerPetId, null);
		}
	}

	public Transform GetActionTransform(Interaction interaction, out int index)
	{
		index = _actionButtons.GetActionButtonIndex(interaction);
		ContextActionButtonBase actionButton = _actionButtons.GetActionButton(interaction);
		return (!(actionButton != null)) ? null : actionButton.transform;
	}
}
