using System;
using System.Collections.Generic;
using Durango.Logic.PlayGuide;
using Durango.Network;
using Durango.Render.Camera;
using Durango.UI;
using Durango.UI.Prologue;
using Durango.Utils;
using Durango.Utils.Extensions;
using Messages;
using Newtonsoft.Json;
using Shared.Battle;
using UnityEngine;

namespace Durango.Prologue;

public class PrologueGuideSystem : GameSystem<PrologueGuideSystem>
{
	public enum PrologueGuideState
	{
		Init,
		OnTouchCharacter,
		OnChooseCharacter,
		MoveGuideSuccess,
		BeginPlay,
		DirectionCafeteria,
		RequireDrink,
		RequireFood,
		LostAndFound,
		LostAndFoundSuccess,
		ThanksToYou,
		AfterEatAndDrink,
		RequestFromClerk,
		GetEncyclopedia,
		ReturnToSeat,
		SeeDinoDoll,
		TakenKid,
		GetAxe,
		GetAxeSuccess,
		ReturnToSeatSuccess,
		AttackRaptor,
		OnBeginAutoBattle,
		LearnDodge,
		VirtualStickGuideSuccess
	}

	public class MsgInfo
	{
		public float MsgDuration;

		public bool IsSystem;

		public string LocalKey;

		public string Portrait;

		public string NameTag;

		public bool PlaySnd;

		public bool DoNotFinishByTouch;

		public bool HidableCaption;

		public bool ShowConfirmShortcut;
	}

	internal class PrologueAddTodoItem
	{
		public string Id;

		public string LocalKey;

		public int Progress;
	}

	public class PrologueGuideOnFinish
	{
		public float Delay;

		public string Next;

		public string[] CustomCmds;
	}

	internal class MyVector3
	{
		public float X;

		public float Y;

		public float Z;

		public Vector3 ToVector3()
		{
			return new Vector3(X, Y, Z);
		}
	}

	internal class Helper
	{
		public string Type = string.Empty;

		public string Id = string.Empty;

		public float X;

		public float Y;

		public bool Flip;

		public float Rotate = -1f;
	}

	public class GuideMask
	{
		public string Type = string.Empty;

		public string Id = string.Empty;

		public float OffsetX;

		public float OffsetY;
	}

	internal class PrologueGuideEvent
	{
		public string Name;

		public int Priority;

		public MsgInfo Msg;

		public PrologueAddTodoItem[] AddToDo;

		public string[] CompleteToDo;

		public string[] CustomCmds;

		public MyVector3 ArrowTarget;

		public GuideMask GuideMask;

		public PrologueGuideOnFinish OnFinish;
	}

	internal class PrologueGuideJson
	{
		public PrologueGuideEvent[] Events;
	}

	private int _curPriority;

	private Dictionary<string, Action> _registeredEvents = new Dictionary<string, Action>();

	private Dictionary<string, Action<Dictionary<string, string>>> _registeredEventsWithParams = new Dictionary<string, Action<Dictionary<string, string>>>();

	private PrologueGuideJson _guideJson;

	private PrologueGuideGroupBase _uiGroup;

	protected PrologueGuideGroupBase UIGroup
	{
		get
		{
			if (_uiGroup == null)
			{
				_uiGroup = UIManager.FindScript<PrologueGuideGroupBase>();
			}
			return _uiGroup;
		}
	}

	public void Init()
	{
		RegisterEventCommands();
		LoadGuideFile();
		SetNextGuide("Init");
		UIGroup.gameObject.SetActive(value: true);
	}

	private void RegisterEventCommands()
	{
		RegisterEventCommand("AddStatusEffect", AddStatusEffect);
		RegisterEventCommand("BeginIntreaction", BeginIntreaction);
		RegisterEventCommand("EndInteraction", EndInteraction);
		RegisterEventCommand("HideInteraction", HideInteraction);
		RegisterEventCommand("OnAfterFind", OnAfterFind);
		RegisterEventCommand("RemoveColliderWithKid", RemoveColliderWithKid);
		RegisterEventCommand("OnSitDownKid", OnSitDownKid);
		RegisterEventCommand("ShowToDoLists", ShowToDoLists);
		RegisterEventCommand("HideToDoLists", HideToDoLists);
		RegisterEventCommand("RemoveAllTodoLists", RemoveAllTodoLists);
		RegisterEventCommand("PlayerMoveLock", PlayerMoveLock);
		RegisterEventCommand("PlayerMoveUnlock", PlayerMoveUnlock);
		RegisterEventCommand("ClearDragTarget", ClearDragTarget);
		RegisterEventCommand("GoPhase2", GoPhase2);
		RegisterEventCommand("PauseTime", PauseTime);
		RegisterEventCommand("ResumeTime", ResumeTime);
		RegisterEventCommand("PrepareVirtualStickMoveGuide", PrepareVirtualStickMoveGuide);
		RegisterEventCommand("PreparePCMoveGuide", PreparePCMoveGuide);
		RegisterEventCommand("EndMoveGuides", EndMoveGuides);
		RegisterEventCommand("DelayedHideToDoLists", DelayedHideToDoLists);
		RegisterEventCommand("CameraFocusIn_Kid", CameraFocusIn_Kid);
		RegisterEventCommand("CameraFocusOut", CameraFocusOut);
		RegisterEventCommand("RemoveToDoItem", RemoveToDoItem);
		RegisterEventCommand("SetPlayerAtackable", SetPlayerAttackable);
	}

	private void LoadGuideFile()
	{
		TextAsset prologueGuideFile = UIGroup.PrologueGuideFile;
		if ((bool)prologueGuideFile)
		{
			_guideJson = JsonConvert.DeserializeObject<PrologueGuideJson>(prologueGuideFile.text);
		}
	}

	private PrologueGuideEvent FindEventFromName(string nextEventName)
	{
		int num = _guideJson.Events.Length;
		for (int i = 0; i < num; i++)
		{
			if (_guideJson.Events[i].Name == nextEventName)
			{
				return _guideJson.Events[i];
			}
		}
		return null;
	}

	public void OnPreEndGuide()
	{
		_curPriority = 0;
	}

	public void SetNextGuide(PrologueGuideState nextState)
	{
		SetNextGuide(nextState.ToString());
	}

	public void SetNextGuide(string nextEventName)
	{
		PrologueGuideEvent prologueGuideEvent = FindEventFromName(nextEventName);
		if (prologueGuideEvent == null)
		{
			Debug.LogError(nextEventName + "이 지정되어있지 않습니다.");
			return;
		}
		HideGuideMask();
		if (prologueGuideEvent.Priority < _curPriority)
		{
			return;
		}
		_curPriority = prologueGuideEvent.Priority;
		if (prologueGuideEvent.Msg != null)
		{
			ShowMsg(prologueGuideEvent.Msg);
		}
		if (prologueGuideEvent.AddToDo != null)
		{
			int num = prologueGuideEvent.AddToDo.Length;
			for (int i = 0; i < num; i++)
			{
				GameSystem<PrologueToDoListSystem>.Instance().AddToDoItem(CreateToDo(prologueGuideEvent.AddToDo[i].Id, prologueGuideEvent.AddToDo[i].LocalKey, prologueGuideEvent.AddToDo[i].Progress));
			}
		}
		if (prologueGuideEvent.CompleteToDo != null)
		{
			int num2 = prologueGuideEvent.CompleteToDo.Length;
			for (int j = 0; j < num2; j++)
			{
				GameSystem<PrologueToDoListSystem>.Instance().SetCompleted(prologueGuideEvent.CompleteToDo[j], completed: true);
			}
		}
		if (prologueGuideEvent.CustomCmds != null)
		{
			DispatchCustomCmds(prologueGuideEvent.CustomCmds);
		}
		if (prologueGuideEvent.ArrowTarget != null)
		{
			SetArrowTarget(prologueGuideEvent.ArrowTarget.ToVector3());
		}
		if (prologueGuideEvent.GuideMask != null)
		{
			SetGuideMask(prologueGuideEvent.GuideMask);
		}
		if (prologueGuideEvent.OnFinish != null)
		{
			UIGroup.SetOnFinishDisplayMsg(prologueGuideEvent.OnFinish);
		}
	}

	private void ShowMsg(MsgInfo msg)
	{
		if (!string.IsNullOrEmpty(msg.LocalKey))
		{
			UIGroup.ShowGuideMsg(msg);
		}
	}

	public void DispatchCustomCmds(string[] customCmds)
	{
		int num = customCmds.Length;
		for (int i = 0; i < num; i++)
		{
			CustomCommand.ExtractParameters(customCmds[i], out var cmd, out var parameters);
			if (string.IsNullOrEmpty(cmd))
			{
				Debug.LogError("Invalid Custom Command: " + customCmds[i]);
			}
			else if (parameters != null && parameters.Count > 0)
			{
				ExecuteCustomCmd(cmd, parameters);
			}
			else
			{
				ExecuteCustomCmd(cmd);
			}
		}
	}

	public void RegisterEventCommand(string cmdName, Action<Dictionary<string, string>> function)
	{
		_registeredEventsWithParams[cmdName] = function;
	}

	public void RegisterEventCommand(string cmdName, Action function)
	{
		_registeredEvents[cmdName] = function;
	}

	private void ExecuteCustomCmd(string cmdName, Dictionary<string, string> parameters)
	{
		if (_registeredEventsWithParams.TryGetValue(cmdName, out var value))
		{
			value(parameters);
		}
		else
		{
			Debug.LogError("Invalid Event : " + cmdName);
		}
	}

	private void ExecuteCustomCmd(string cmdName)
	{
		if (_registeredEvents.TryGetValue(cmdName, out var value))
		{
			value();
		}
		else
		{
			Debug.LogError("Invalid Event : " + cmdName);
		}
	}

	private void SetArrowTarget(Vector3 arrowTargetPos)
	{
		Singleton<PrologueManager>.Instance().PlayGuideHelper.SetTarget(arrowTargetPos);
	}

	public void HideGuideMask()
	{
		UIManager.FindScript<PrologueGuideMaskGroup>().gameObject.SetActive(value: false);
	}

	public void SetGuideMask(GuideMask guideMask, bool helperOnly = false)
	{
		if (guideMask == null || guideMask.Id == null)
		{
			return;
		}
		Transform transform = null;
		Vector3 touchPos;
		if (guideMask.Id == "Player")
		{
			touchPos = MainCamera.WorldToScreenPos(PlayerBehavior.LocalPlayer.GetBodyPartTransform(BodyPart.Body).position);
			touchPos.y = (float)Screen.height - touchPos.y;
			touchPos *= MainCamera.NGUIScale();
			touchPos.y = 0f - touchPos.y;
		}
		else if (guideMask.Id == "Enemy")
		{
			GameObject gameObject = Singleton<ObjectManager>.Instance().FindObject(PrologueAIRaptor.FakeEntityId);
			AnimalBehavior component = gameObject.GetComponent<AnimalBehavior>();
			touchPos = MainCamera.WorldToScreenPos((!(component != null)) ? gameObject.transform.position : component.InteractionPosition);
			touchPos.y = (float)Screen.height - touchPos.y;
			touchPos *= MainCamera.NGUIScale();
			touchPos.y = 0f - touchPos.y;
		}
		else if (guideMask.Id == "VirtualStick")
		{
			touchPos = new Vector3(-100f, -100f, 0f);
		}
		else
		{
			transform = ((!(guideMask.Type == "BattleAction")) ? Singleton<UIManager>.Instance().FindTransform(guideMask.Id) : UIManager.FindScript<CombatGroup>().FindActionButton(guideMask.Id.ToInt()));
			transform.gameObject.SetActive(value: true);
			touchPos = MainCamera.NGUIPosToScreenPos(UIUtility.ToRootPosition(transform.gameObject));
			touchPos.y = (float)Screen.height - touchPos.y;
			touchPos *= MainCamera.NGUIScale();
			touchPos.y = 0f - touchPos.y;
		}
		touchPos.x += guideMask.OffsetX;
		touchPos.y += guideMask.OffsetY;
		PrologueGuideMaskGroup prologueGuideMaskGroup = UIManager.FindScript<PrologueGuideMaskGroup>();
		prologueGuideMaskGroup.HelperOnly(helperOnly);
		prologueGuideMaskGroup.SetTouchPos(touchPos);
		prologueGuideMaskGroup.gameObject.SetActive(value: true);
		prologueGuideMaskGroup.SetType(guideMask.Type);
		switch (guideMask.Type)
		{
		case "Click":
		case "BattleAction":
			if (transform != null)
			{
				UIEventListener uIEventListener = UIEventListener.Get(transform.gameObject);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(ActionClicked));
			}
			break;
		case "Select":
			GameSystem<InteractionSystem>.Instance().PreTouchTarget += TargetSelected;
			break;
		case "View":
			break;
		}
	}

	private void ActionClicked(GameObject go)
	{
		UIEventListener uIEventListener = UIEventListener.Get(go);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener.onClick, new UIEventListener.VoidDelegate(ActionClicked));
		UIGroup.ShowNextGuideMsg();
	}

	private void TargetSelected(InteractionObject obj, ref bool result)
	{
		if (!(obj.EntityId != PrologueAIRaptor.FakeEntityId))
		{
			GameSystem<InteractionSystem>.Instance().PreTouchTarget -= TargetSelected;
			HideGuideMask();
			Connections.Frontend.PushPacket(new BattleBegun
			{
				EntityId = GameManager.PlayerId,
				EnemyId = PrologueAIRaptor.FakeEntityId
			});
			GameSystem<CombatSystem>.Instance().SelectTarget(PrologueAIRaptor.FakeEntityId);
			Singleton<TrainTrexController>.Instance().OnBeginAutoBattle();
			obj.GetTargetComponent<PrologueAIRaptor>().SetAiActivated();
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
			result = true;
		}
	}

	public void ForceClearGuideMsg()
	{
		UIGroup.ClearGuideMsg(wantClearDelayedMessage: true);
	}

	private void AddStatusEffect()
	{
		Singleton<PrologueManager>.Instance().AddStatusEffects();
	}

	private void BeginIntreaction()
	{
		PrologueInteractionButtonGroupBase prologueInteractionButtonGroupBase = UIManager.FindScript<PrologueInteractionButtonGroupBase>();
		if (prologueInteractionButtonGroupBase != null)
		{
			prologueInteractionButtonGroupBase.CanInteraction = true;
		}
	}

	private void EndInteraction()
	{
		PrologueInteractionButtonGroupBase prologueInteractionButtonGroupBase = UIManager.FindScript<PrologueInteractionButtonGroupBase>();
		if (prologueInteractionButtonGroupBase != null)
		{
			prologueInteractionButtonGroupBase.CanInteraction = false;
		}
		PrologueInteractionButtonGroupBase.RefreshInteractions(reset: true);
	}

	private void HideInteraction()
	{
		PrologueInteractionButtonGroupBase.HideInteractionButton();
	}

	private void OnAfterFind()
	{
		Singleton<DialogsManager>.Instance()._triggerDialogAfterEvent.SetActive(value: true);
		Singleton<DialogsManager>.Instance()._triggerDialogAfterEvent.SendMessage("BeginEvent");
	}

	private void RemoveColliderWithKid()
	{
		Singleton<DialogsManager>.Instance()._colliderWithKid.SetActive(value: false);
	}

	private void OnSitDownKid()
	{
		Singleton<DialogsManager>.Instance()._triggerDialogActingSitDown.SetActive(value: true);
		Singleton<DialogsManager>.Instance()._triggerDialogActingSitDown.SendMessage("BeginEvent");
	}

	private void ShowToDoLists()
	{
		UIManager.FindScript<PrologueToDoListGroup>().RestoreToDoList();
	}

	private void HideToDoLists()
	{
		UIManager.FindScript<PrologueToDoListGroup>().HideToDoList();
	}

	private void DelayedHideToDoLists(Dictionary<string, string> parameters)
	{
		float delay = parameters.Get("delayedTime").ToFloat();
		KUtility.DelayedCall(this, UIManager.FindScript<PrologueToDoListGroup>().HideToDoList, delay);
	}

	private void RemoveToDoItem(Dictionary<string, string> parameters)
	{
		string key = parameters.Get("Id");
		ToDoBase toDoBase = GameSystem<PrologueToDoListSystem>.Instance().FindToDo(key);
		if (toDoBase != null)
		{
			GameSystem<PrologueToDoListSystem>.Instance().RemoveItem(toDoBase);
		}
	}

	private void RemoveAllTodoLists()
	{
		GameSystem<PrologueToDoListSystem>.Instance().RemoveAll();
		UIManager.FindScript<PrologueToDoListGroup>().RestoreToDoList();
	}

	private void PlayerMoveLock()
	{
		GameSystem<InputSystem>.Instance().MoveLock = true;
		Singleton<PlayerController>.Instance().StopMove();
		Singleton<PrologueManager>.Instance().PlayGuideHelper.ShowTargetIfEnabled(visible: false);
	}

	private void PlayerMoveUnlock()
	{
		GameSystem<InputSystem>.Instance().MoveLock = false;
		Singleton<PrologueManager>.Instance().PlayGuideHelper.ShowTargetIfEnabled(visible: true);
	}

	private void ClearDragTarget()
	{
	}

	private ToDoBase CreateToDo(string eventName, string localKey, int progress = 0)
	{
		return new ManualToDo
		{
			Key = eventName,
			LocalText = LocalizeSystem.Get(localKey),
			TargetProgress = progress
		};
	}

	private void CameraFocusIn_Kid(Dictionary<string, string> paramerers)
	{
		float duration = paramerers.Get("panningTime").ToFloat();
		float zoomRatio = paramerers.Get("fovRatio").ToFloat();
		Singleton<CameraController>.Instance().Target(Singleton<DialogsManager>.Instance()._npcKid.transform.position + Vector3.up * 50f, duration).ZoomRatio(zoomRatio, duration);
	}

	private void CameraFocusOut(Dictionary<string, string> paramerers)
	{
		float duration = paramerers.Get("returningTime").ToFloat();
		Singleton<CameraController>.Instance().Target(null, duration).Offset(Vector3.zero, duration)
			.ZoomRatio(1f, duration);
	}

	private void GoPhase2()
	{
		Singleton<PrologueManager>.Instance().DoPhase2();
	}

	private void PauseTime()
	{
		Time.timeScale = 0f;
	}

	private void ResumeTime()
	{
		Time.timeScale = 1f;
	}

	private void SetPlayerAttackable(Dictionary<string, string> paramerers)
	{
	}

	private void PrepareVirtualStickMoveGuide()
	{
		GameSystem<InputSystem>.Instance().Touch.ResetTouchEvents();
		Singleton<PlayerController>.Instance().MoveStarted += PlayerControll_VirtualStickMoveStarted;
		GameSystem<InputSystem>.Instance().AllowJoystickMove = false;
		GameSystem<InputSystem>.Instance().AllowVirtualStickMove = true;
	}

	private void PreparePCMoveGuide()
	{
		GameSystem<InputSystem>.Instance().Touch.ResetTouchEvents();
		Singleton<PlayerController>.Instance().StopMove();
		Singleton<PlayerController>.Instance().MoveStarted += PlayerControll_VirtualStickMoveStarted;
		GameSystem<InputSystem>.Instance().AllowJoystickMove = true;
		GameSystem<InputSystem>.Instance().AllowVirtualStickMove = true;
	}

	private void PlayerControll_VirtualStickMoveStarted()
	{
		Singleton<PlayerController>.Instance().MoveStarted -= PlayerControll_VirtualStickMoveStarted;
		SetNextGuide(PrologueGuideState.VirtualStickGuideSuccess);
	}

	private void EndMoveGuides()
	{
		GameSystem<InputSystem>.Instance().AllowJoystickMove = true;
		GameSystem<InputSystem>.Instance().AllowVirtualStickMove = true;
	}

	public void SkipPrologue()
	{
		Singleton<PlayerController>.Instance().MoveStarted -= PlayerControll_VirtualStickMoveStarted;
	}
}
