using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayGuide;
using Shared.Battle;
using UnityEngine;

public class PrologueGuideSystem : UISystem<PrologueGuideSystem, PrologueGuideGroup>
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
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
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

		public Helper Helper;

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

	public void Init()
	{
		RegisterEventCommands();
		LoadGuideFile();
		SetNextGuide("Init");
		((Component)base.UIGroup).gameObject.SetActive(true);
	}

	private void RegisterEventCommands()
	{
		RegisterEventCommand("AddStatusEffect", AddStatusEffect);
		RegisterEventCommand("BeginIntreaction", BeginIntreaction);
		RegisterEventCommand("OnAfterFind", OnAfterFind);
		RegisterEventCommand("RemoveColliderWithKid", RemoveColliderWithKid);
		RegisterEventCommand("OnSitDownKid", OnSitDownKid);
		RegisterEventCommand("HideToDoLists", HideToDoLists);
		RegisterEventCommand("RemoveAllTodoLists", RemoveAllTodoLists);
		RegisterEventCommand("PlayerMoveLock", PlayerMoveLock);
		RegisterEventCommand("PlayerMoveUnlock", PlayerMoveUnlock);
		RegisterEventCommand("ClearDragTarget", ClearDragTarget);
		RegisterEventCommand("GoPhase2", GoPhase2);
		RegisterEventCommand("PauseTime", PauseTime);
		RegisterEventCommand("ResumeTime", ResumeTime);
		RegisterEventCommand("PrepareVirtualStickMoveGuide", PrepareVirtualStickMoveGuide);
		RegisterEventCommand("EndMoveGuides", EndMoveGuides);
		RegisterEventCommand("CameraFocusIn_Kid", CameraFocusIn_Kid);
		RegisterEventCommand("CameraFocusOut", CameraFocusOut);
		RegisterEventCommand("RemoveToDoItem", RemoveToDoItem);
		RegisterEventCommand("SetPlayerAtackable", SetPlayerAtackable);
	}

	private void LoadGuideFile()
	{
		TextAsset prologueGuideFile = base.UIGroup.PrologueGuideFile;
		if (Object.op_Implicit((Object)(object)prologueGuideFile))
		{
			_guideJson = JsonConvert.DeserializeObject<PrologueGuideJson>(prologueGuideFile.text);
		}
	}

	public void ShowSysMsg(string sysmsg, float duration)
	{
		base.UIGroup.ShowGuideMsg(sysmsg, isSystemMsg: true, duration);
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

	public void SetNextGuide(string nextEventName)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		PrologueGuideEvent prologueGuideEvent = FindEventFromName(nextEventName);
		if (prologueGuideEvent == null)
		{
			Debug.LogError((object)(nextEventName + "이 지정되어있지 않습니다."));
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
		if (prologueGuideEvent.Helper != null)
		{
			SetHelper(prologueGuideEvent.Helper);
		}
		if (prologueGuideEvent.GuideMask != null)
		{
			SetGuideMask(prologueGuideEvent.GuideMask);
		}
		if (prologueGuideEvent.OnFinish != null)
		{
			base.UIGroup.SetOnFinishDisplayMsg(prologueGuideEvent.OnFinish);
		}
	}

	private void ShowMsg(MsgInfo msg)
	{
		if (!string.IsNullOrEmpty(msg.LocalKey))
		{
			base.UIGroup.ShowGuideMsg(msg);
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
				Debug.LogError((object)("Invalid Custom Command: " + customCmds[i]));
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
			Debug.LogError((object)("Invalid Event : " + cmdName));
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
			Debug.LogError((object)("Invalid Event : " + cmdName));
		}
	}

	private void SetArrowTarget(Vector3 arrowTargetPos)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		KSingleton<UIManager>.Instance().PlayGuideHelper.SetArrowTarget(arrowTargetPos);
	}

	private void SetHelper(Helper helper)
	{
		if (helper != null)
		{
			switch (helper.Type)
			{
			case "click":
			{
				ClickTargetLocator locator = ClickTargetFactory.Create("ui", new Dictionary<string, ClickTargetData> { 
				{
					"current",
					new ClickTargetData
					{
						id = helper.Id,
						x = helper.X,
						y = helper.Y,
						flip = helper.Flip,
						rotate = helper.Rotate
					}
				} });
				KSingleton<UIManager>.Instance().PlayGuideHelper.EnableClickTarget(locator);
				break;
			}
			}
		}
	}

	public void RemoveClickHelper()
	{
		KSingleton<UIManager>.Instance().PlayGuideHelper.DisableClickTarget();
	}

	public void HideGuideMask()
	{
		((Component)UIManager.FindScript<GuideMaskGroup>()).gameObject.SetActive(false);
	}

	public void SetGuideMask(GuideMask guideMask, bool helperOnly = false)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		if (guideMask == null || guideMask.Id == null)
		{
			return;
		}
		Transform val = null;
		Vector3 val2 = default(Vector3);
		if (guideMask.Id == "Player")
		{
			val2 = MainCamera.WorldToScreenPos(PlayerBehavior.LocalPlayer.GetBodyPartTransform(BodyPart.Body).position);
			val2.y = (float)Screen.height - val2.y;
			val2 *= MainCamera.NGUIScale();
			val2.y = 0f - val2.y;
		}
		else if (guideMask.Id == "Enemy")
		{
			GameObject val3 = PlayerBehavior.LocalPlayer.Target;
			if ((Object)(object)val3 == (Object)null)
			{
				val3 = KSingleton<ObjectManager>.Instance().FindObject(PrologueAIRaptor.FakeEntityId);
				PlayerBehavior.LocalPlayer.Target = val3;
			}
			AnimalBehavior component = val3.GetComponent<AnimalBehavior>();
			val2 = MainCamera.WorldToScreenPos((!((Object)(object)component != (Object)null)) ? val3.transform.position : component.InteractionPosition);
			val2.y = (float)Screen.height - val2.y;
			val2 *= MainCamera.NGUIScale();
			val2.y = 0f - val2.y;
		}
		else if (guideMask.Id == "VirtualStick")
		{
			((Vector3)(ref val2))._002Ector(-100f, -100f, 0f);
		}
		else
		{
			val = KSingleton<UIManager>.Instance().FindTransform(guideMask.Id);
			((Component)val).gameObject.SetActive(true);
			val2 = MainCamera.NGUIPosToScreenPos(MainCamera.NGUILocalPositionToNGUIPosition(val.localPosition, val.parent));
			val2.y = (float)Screen.height - val2.y;
			val2 *= MainCamera.NGUIScale();
			val2.y = 0f - val2.y;
		}
		val2.x += guideMask.OffsetX;
		val2.y += guideMask.OffsetY;
		GuideMaskGroup guideMaskGroup = UIManager.FindScript<GuideMaskGroup>();
		guideMaskGroup.SetTouchPos(val2);
		((Component)guideMaskGroup).gameObject.SetActive(true);
		guideMaskGroup.SetHoldHandShow(guideMask.Type == "Hold");
		guideMaskGroup.SetTouchHandShow(guideMask.Type == "Click" || guideMask.Type == "Select");
		guideMaskGroup.SetVirtualStickDemoShow(guideMask.Type == "VirtualStick");
		switch (guideMask.Type)
		{
		case "Click":
			if ((Object)(object)val != (Object)null)
			{
				UIEventListener listener = ((Component)val).GetComponent<ActionButton>().Listener;
				listener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(listener.onClick, new UIEventListener.VoidDelegate(ActionClicked));
			}
			break;
		case "Select":
			GameSystem<InteractionSystem>.Instance().PreTouchTarget += TargetSelected;
			break;
		}
		guideMaskGroup.HelperOnly(helperOnly);
	}

	private void ActionClicked(GameObject go)
	{
		UIEventListener listener = go.GetComponent<ActionButton>().Listener;
		listener.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(listener.onClick, new UIEventListener.VoidDelegate(ActionClicked));
		base.UIGroup.ShowNextGuideMsg();
	}

	private void TargetSelected(InteractionObject obj, ref bool result)
	{
		if (!((Object)(object)obj.Target != (Object)(object)PlayerBehavior.LocalPlayer.Target))
		{
			GameSystem<InteractionSystem>.Instance().PreTouchTarget -= TargetSelected;
			HideGuideMask();
		}
	}

	public void SetNextGuide(PrologueGuideState nextState)
	{
		SetNextGuide(nextState.ToString());
	}

	public void ForceClearGuideMsg()
	{
		base.UIGroup.ClearGuideMsg();
	}

	private void AddStatusEffect()
	{
		KSingleton<PrologueManager>.Instance().AddStatusEffects();
	}

	private void BeginIntreaction()
	{
		KSingleton<PrologueManager>.Instance().BeginIntreaction = true;
		KSingleton<PlayerController>.Instance().EndMove();
	}

	private void OnAfterFind()
	{
		KSingleton<DialogsManager>.Instance()._triggerDialogAfterEvent.SetActive(true);
		KSingleton<DialogsManager>.Instance()._triggerDialogAfterEvent.SendMessage("BeginEvent");
	}

	private void RemoveColliderWithKid()
	{
		KSingleton<DialogsManager>.Instance()._colliderWithKid.SetActive(false);
	}

	private void OnSitDownKid()
	{
		KSingleton<DialogsManager>.Instance()._triggerDialogActingSitDown.SetActive(true);
		KSingleton<DialogsManager>.Instance()._triggerDialogActingSitDown.SendMessage("BeginEvent");
	}

	private void HideToDoLists()
	{
		UIManager.FindScript<PrologueToDoListGroup>().HideToDoList();
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
		KSingleton<PlayerController>.Instance().MoveLock = true;
		KSingleton<PlayerController>.Instance().StopMove();
		KSingleton<PlayerController>.Instance().EndMove();
		KSingleton<UIManager>.Instance().PlayGuideHelper.ShowArrowTargetIfEnabled(bVisible: false);
	}

	private void PlayerMoveUnlock()
	{
		KSingleton<PlayerController>.Instance().MoveLock = false;
		KSingleton<UIManager>.Instance().PlayGuideHelper.ShowArrowTargetIfEnabled(bVisible: true);
	}

	private void ClearDragTarget()
	{
	}

	private ToDoBase CreateToDo(string eventName, string localKey, int progress = 0)
	{
		ManualToDo manualToDo = new ManualToDo();
		manualToDo.Key = eventName;
		manualToDo.LocalText = LocalizeSystem.Get(localKey);
		manualToDo.TargetProgress = progress;
		return manualToDo;
	}

	private void CameraFocusIn_Kid(Dictionary<string, string> paramerers)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		float num = paramerers.Get("panningTime").ToFloat();
		float zoomRatio = paramerers.Get("fovRatio").ToFloat();
		float num2 = paramerers.Get("duration").ToFloat();
		KSingleton<CameraController>.Instance().SetCameraTargetPos(KSingleton<DialogsManager>.Instance()._npcKid.transform.position, num, zoomRatio, num);
		if (num2 > 0f)
		{
			((MonoBehaviour)this).Invoke("BeginCameraPanning", num2);
		}
	}

	private void CameraFocusOut(Dictionary<string, string> paramerers)
	{
		float num = paramerers.Get("returningTime").ToFloat();
		KSingleton<CameraController>.Instance().ResetCameraTarget(num, num);
	}

	private void GoPhase2()
	{
		KSingleton<PrologueManager>.Instance().DoPhase2();
	}

	private void PauseTime()
	{
		Time.timeScale = 0f;
	}

	private void ResumeTime()
	{
		Time.timeScale = 1f;
	}

	private void SetPlayerAtackable(Dictionary<string, string> paramerers)
	{
		float num = paramerers.Get("attackable").ToFloat();
		PrologueManager.PlayerBattleAi.SetAtackable(num > 0f);
	}

	private void PrepareVirtualStickMoveGuide()
	{
		KSingleton<PlayerController>.Instance().ResetTouchEvents();
		KSingleton<PlayerController>.Instance().AllowJoystickMove = false;
		KSingleton<PlayerController>.Instance().AllowVirtualStickMove = true;
		KSingleton<PlayerController>.Instance().MoveStarted += PlayerControll_VirtualStickMoveStarted;
	}

	private void PlayerControll_VirtualStickMoveStarted()
	{
		KSingleton<PlayerController>.Instance().MoveStarted -= PlayerControll_VirtualStickMoveStarted;
		SetNextGuide(PrologueGuideState.VirtualStickGuideSuccess);
	}

	private void EndMoveGuides()
	{
		KSingleton<PlayerController>.Instance().AllowJoystickMove = true;
		KSingleton<PlayerController>.Instance().AllowVirtualStickMove = true;
	}

	public void SkipPrologue()
	{
		RemoveClickHelper();
		Close();
		KSingleton<PlayerController>.Instance().MoveStarted -= PlayerControll_VirtualStickMoveStarted;
	}
}
