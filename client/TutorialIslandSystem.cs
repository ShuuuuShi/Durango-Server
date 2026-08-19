using System;
using System.Collections;
using System.Collections.Generic;
using Building;
using Durango.Logic.PlayGuide;
using Durango.Network;
using Durango.Render.Camera;
using Durango.Terrain;
using Durango.UI;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using InteractionData;
using L10N;
using Messages;
using UnityEngine;

public class TutorialIslandSystem : GameSystem<TutorialIslandSystem>
{
	public class TutorialBoatToDo : ToDoBase
	{
		private readonly string _slotId;

		public TutorialBoatToDo(string id, int count)
		{
			_slotId = id;
			base.TargetProgress = count;
		}

		public override void OnAddItem()
		{
			TutorialIslandSystem tutorialIslandSystem = GameSystem<TutorialIslandSystem>.Instance();
			tutorialIslandSystem.TutorialSessionUpdated += OnUpdateTutorialTutorialSession;
			OnUpdateTutorialTutorialSession(tutorialIslandSystem.TutorialSession);
		}

		public override void OnRemoveItem()
		{
			GameSystem<TutorialIslandSystem>.Instance().TutorialSessionUpdated -= OnUpdateTutorialTutorialSession;
		}

		public void OnUpdateTutorialTutorialSession(TutorialSession session)
		{
			base.IsCompleted = false;
			CallProgressChange((session.Materials != null) ? session.Materials.Get(_slotId, 0) : 0);
		}
	}

	private Artifact _tutorialBoat;

	private bool _hasSession;

	private bool _registeredPreTouchTarget;

	private readonly BuildSlotContainer _boatSlots = new BuildSlotContainer();

	private static readonly Action EmptyAction = delegate
	{
	};

	public TutorialSession TutorialSession { get; private set; }

	public event Action<TutorialSession> TutorialSessionUpdated;

	private void Awake()
	{
		Connections.Frontend.On<AppearTutorialBoat>(OnAppearTutorialBoat);
		Connections.Frontend.On<TutorialBoatSessions>(OnTutorialBoatSessions);
		Connections.Frontend.On<DepartTutorialReady>(OnDepartTutorialReady);
		Connections.Frontend.On<TutorialBoatMaterialUpdated>(OnTutorialBoatMaterialUpdated);
		InteractionSystem interactionSystem = GameSystem<InteractionSystem>.Instance();
		interactionSystem.AddInteractionHandler(Interaction.BuildTutorialBoat, delegate(InteractionObject target)
		{
			Artifact targetComponent2 = target.GetTargetComponent<Artifact>();
			if (!(targetComponent2 == null))
			{
				UpdateTutorialBoatSlots();
				CraftGroupBase craftGroupBase = UIManager.FindScript<CraftGroupBase>();
				craftGroupBase.Open(_boatSlots, EmptyAction, SendPutTutorialBoatMaterials, EmptyAction);
			}
		});
		interactionSystem.AddInteractionHandler(Interaction.DepartTutorial, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			if (!(targetComponent == null))
			{
				SendDepartTutorial(targetComponent);
			}
		});
	}

	private void OnAppearTutorialBoat(AppearTutorialBoat msg, PacketHeader header)
	{
		Artifact artifact = Singleton<ArtifactManager>.Instance().Add(msg.EntityId, msg.Tile, msg.EntityType, msg.Rotation, msg.Size, msg.Floor, msg.Stories, msg.HasRoof, msg.Height);
		artifact.FounderId = msg.FounderEntityId;
		artifact.UpdateDisplay(msg.Display);
		artifact.SetArtifactState(msg.States, header.Time);
		artifact.SetTagList(msg.Tags._Tags);
		SetTutorialBoatSessions(msg.Status);
		_tutorialBoat = artifact;
		RegisterPreTouchTarget();
		Singleton<TerrainBase>.Instance().OnReady(OnInitTutorialBoat);
	}

	private void OnInitTutorialBoat()
	{
		UpdateTutorialBoatTooltip();
		InitTutorialBoatSlots();
	}

	private void UpdateTutorialBoatTooltip()
	{
		if (_tutorialBoat == null || !_hasSession)
		{
			return;
		}
		TutorialBoatTooltip tutorialBoatTooltip = UIManager.Popup.Tooltip<TutorialBoatTooltip>();
		Blueprint blueprint = _tutorialBoat.Blueprint;
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		int i = 0;
		for (int num = blueprint.Slots.Length; i < num; i++)
		{
			BlueprintSlot blueprintSlot = blueprint.Slots[i];
			int num2 = TutorialSession.Materials.Get(blueprintSlot.Id, 0);
			string key;
			string value;
			if (num2 == blueprintSlot.Count)
			{
				key = string.Format("{1}{0}[-]", blueprintSlot.Name, UIManager.ColorBBCode(PresetColor.UILightGray));
				value = string.Format("{2}{0} / {1}[-]", num2, blueprintSlot.Count, UIManager.ColorBBCode(PresetColor.UILightGray));
			}
			else
			{
				key = string.Format("{1}{0}[-]", blueprintSlot.Name, UIManager.ColorBBCode(PresetColor.UIWhite));
				value = string.Format("{2}{3}{0}[-] / {4}{1}[-]", num2, blueprintSlot.Count, UIManager.ColorBBCode(PresetColor.UILightGray), UIManager.ColorBBCode(PresetColor.UIYellow), UIManager.ColorBBCode(PresetColor.UIWhite));
			}
			list.Add(new KeyValuePair<string, string>(key, value));
		}
		tutorialBoatTooltip.Set(_tutorialBoat, T._("탈출용 뗏목"), list);
		tutorialBoatTooltip.Show(3600f);
	}

	private void UpdateTutorialBoatSlots()
	{
		if (TutorialSession.Materials != null)
		{
			_boatSlots.SetPrevAssignedItemsDummyCount(TutorialSession.Materials);
		}
	}

	private void InitTutorialBoatSlots()
	{
		_boatSlots.Set(_tutorialBoat, _tutorialBoat.Blueprint, GameSystem<InventorySystem>.Instance().PlayerInventory);
		UpdateTutorialBoatSlots();
	}

	private void OnTutorialBoatSessions(TutorialBoatSessions msg, PacketHeader header)
	{
		SetTutorialBoatSessions(msg);
	}

	private void SetTutorialBoatSessions(TutorialBoatSessions msg)
	{
		bool hasSession = _hasSession;
		_hasSession = UpdateTutorialSession(msg.Sessions);
		RegisterPreTouchTarget();
		UpdateTutorialBoatTooltip();
		UpdateTutorialBoatSlots();
		if (hasSession != _hasSession)
		{
			OnInitSession();
		}
		_boatSlots.OnSlotMaterialUpdate();
		if (this.TutorialSessionUpdated != null)
		{
			this.TutorialSessionUpdated(TutorialSession);
		}
	}

	private void OnInitSession()
	{
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		if (target != null)
		{
			GameSystem<InteractionSystem>.Instance().SendTouchMsg();
		}
	}

	private void OnDepartTutorialReady(DepartTutorialReady msg, PacketHeader header)
	{
		GameSystem<PlayGuideSystem>.Instance().ReloadFlow("depart_tutorial_flow", delegate
		{
			StartCoroutine(CoFadeAndSendDepartTutorialFor(msg));
		});
		GameSystem<PlayGuideSystem>.Instance().BeginFlow("depart_tutorial_flow");
	}

	private IEnumerator CoFadeAndSendDepartTutorialFor(DepartTutorialReady msg)
	{
		BlankLoadingCurtain curtain = UIManager.ShowLoadingCurtain<BlankLoadingCurtain>();
		while (curtain.Widget.alpha < 1f)
		{
			yield return null;
		}
		SendDepartTutorialFor(msg.TargetRegionId, msg.EntryPointOffset);
	}

	private void OnTutorialBoatMaterialUpdated(TutorialBoatMaterialUpdated msg, PacketHeader header)
	{
		if (TutorialSession.SessionId != msg.SessionId)
		{
			return;
		}
		string text = string.Empty;
		for (int i = 0; i < msg.Materials.Length; i++)
		{
			Pair<string, int> pair = msg.Materials[i];
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += T._("{0} x{1}", T._(pair.Item1), pair.Item2);
		}
		if (text.Length > 0)
		{
			string text2 = ((msg.PlayerName == null) ? T._("[89D2FF]리더 같은 이[-]") : msg.PlayerName);
			string comment = T._("{0}님이 {1}를 뗏목 재료로 넣었습니다", text2, text);
			UIManager.SystemMsg(comment, 4f);
		}
	}

	private bool UpdateTutorialSession(TutorialSession[] sessions)
	{
		int i = 0;
		for (int size = KUtility.GetSize(sessions); i < size; i++)
		{
			if (sessions[i].Players != null && sessions[i].Players.Contains(GameManager.PlayerId))
			{
				TutorialSession = sessions[i];
				return true;
			}
		}
		TutorialSession = default(TutorialSession);
		return false;
	}

	private void RegisterPreTouchTarget()
	{
		if (!_registeredPreTouchTarget && (_tutorialBoat != null || _hasSession))
		{
			_registeredPreTouchTarget = true;
			GameSystem<InteractionSystem>.Instance().PreTouchTarget += InteractionSystem_PreTouchTarget;
		}
	}

	private void InteractionSystem_PreTouchTarget(InteractionObject obj, ref bool result)
	{
		Artifact targetComponent = obj.GetTargetComponent<Artifact>();
		if (!(targetComponent == null) && !(_tutorialBoat == null) && !(targetComponent != _tutorialBoat) && !_hasSession)
		{
			SendParticipateTutorialBoat(_tutorialBoat.EntityId, _tutorialBoat.WorldTile);
			result = true;
		}
	}

	private void SendParticipateTutorialBoat(string entityId, Point2 tile)
	{
		Connections.Frontend.Send(new ParticipateTutorialBoat
		{
			EntityId = entityId,
			Tile = tile
		});
	}

	private void SendPutTutorialBoatMaterials()
	{
		Connections.Frontend.Send(new PutMaterialsIntoTutorialBoat
		{
			EntityId = _tutorialBoat.EntityId,
			Tile = _tutorialBoat.WorldTile,
			Materials = _boatSlots.CreateFirstMaterialsDictionary(canFinished: false)
		});
		UIManager.SystemMsg(T._("뗏목 재료를 일부 넣었습니다."));
		IconMoveEffectGroup iconMoveEffectGroup = UIManager.FindScript<IconMoveEffectGroup>();
		if (!(iconMoveEffectGroup == null))
		{
			BuildSlotContainer boatSlots = _boatSlots;
			Vector3 pos = ((boatSlots != null && !(boatSlots.Artifact == null)) ? MainCamera.WorldToNGUIPos(boatSlots.Artifact.InteractionPosition) : MainCamera.WorldToNGUIPos(PlayerBehavior.LocalPlayer.CurrentPosition));
			iconMoveEffectGroup.RegisterSlotContainer(boatSlots, pos);
		}
	}

	public void SendDepartTutorial(Artifact tutorialBoatOrPort)
	{
		Connections.Frontend.Send(new DepartTutorial
		{
			EntityId = tutorialBoatOrPort.EntityId,
			Tile = tutorialBoatOrPort.WorldTile
		});
	}

	public void SendDepartTutorialFor(string regionId, int offset = -1)
	{
		DepartTutorialFor msg = default(DepartTutorialFor);
		msg.TargetRegionId = regionId;
		msg.EntryPointOffset = offset;
		Connections.Frontend.Send(msg);
	}
}
