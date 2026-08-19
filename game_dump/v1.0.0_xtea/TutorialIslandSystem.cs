using System;
using System.Collections.Generic;
using Building_;
using K1Network;
using L10N;
using Messages;
using PlayGuide;
using Player;
using Shared.Region;
using UnityEngine;

public class TutorialIslandSystem : GameSystem<TutorialIslandSystem>
{
	private Artifact _tutorialBoat;

	private bool _hasSession;

	private bool _registPreInteraction;

	private TutorialSession _tutorialSession;

	private DepartTutorialReady _defaultDepartInfo;

	private readonly ToDoCollection _boatToDoCollection = new ToDoCollection
	{
		NPCType = NPCType.Chief,
		ToDoList = new List<ToDoBase>()
	};

	private readonly BuildSlotContainer _boatSlots = new BuildSlotContainer();

	public BuildSlotContainer BoatSlots => _boatSlots;

	public event Action<Artifact> ReadyToDepartBootcamp;

	public event Action<TutorialSession> TutorialBoatSessionUpdated;

	private void Awake()
	{
		Connections.Frontend.On<AppearTutorialBoat>(OnAppearTutorialBoatMsg);
		Connections.Frontend.On<TutorialBoatSessions>(OnTutorialBoatSessions);
		Connections.Frontend.On<DepartTutorialReady>(OnReadyDepartTutorial);
		Connections.Frontend.On<TutorialBoatMaterialUpdated>(OnMaterialUpdated);
	}

	private void OnTutorialBoatSessions(TutorialBoatSessions msg, PacketHeader header)
	{
		OnTutorialBoatSessions(msg);
	}

	private void OnTutorialBoatSessions(TutorialBoatSessions msg)
	{
		bool hasSession = _hasSession;
		_hasSession = TryGetTutorialSession(msg.Sessions, out _tutorialSession);
		RegistOnPostTouchInteractionTarget();
		UpdateTutorialBoatTooltip();
		UpdateTutorialBoatSlots();
		if (hasSession != _hasSession)
		{
			OnInitSession();
		}
		_boatSlots.OnSlotMaterialUpdate();
		if (this.TutorialBoatSessionUpdated != null)
		{
			this.TutorialBoatSessionUpdated(_tutorialSession);
		}
	}

	private void OnAppearTutorialBoatMsg(AppearTutorialBoat msg, PacketHeader header)
	{
		Artifact artifact = KSingleton<StaticObjectManager>.Instance().AddArtifact(msg.EntityId, msg.Tile, msg.EntityType, msg.Rotation, new Point2(msg.Size.Key, msg.Size.Value), Mathf.Max(msg.Size.Key, msg.Size.Value));
		artifact.FounderId = msg.FounderEntityId;
		artifact.UpdateDisplay(msg.Display);
		GameSystem<BuildSystem>.Instance().UpdateArtifactData(artifact, msg.States, msg.Tags);
		OnTutorialBoatSessions(msg.Status);
		_tutorialBoat = artifact;
		RegistOnPostTouchInteractionTarget();
		TerrainA6.OnInitTerrain(OnInitTutorialBoat);
	}

	private void OnMaterialUpdated(TutorialBoatMaterialUpdated msg, PacketHeader header)
	{
		if (_tutorialSession.SessionId != msg.SessionId)
		{
			return;
		}
		string text = string.Empty;
		for (int i = 0; i < msg.Materials.Length; i++)
		{
			KeyValuePair<string, int> keyValuePair = msg.Materials[i];
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += LocalizeSystem.Format("#tutorial_boat_material", T._(keyValuePair.Key), keyValuePair.Value.ToString());
		}
		if (text.Length > 0)
		{
			string text2 = ((msg.PlayerName == null) ? T._("[89D2FF]리더 같은 이[-]") : msg.PlayerName);
			string comment = LocalizeSystem.Format("#tutorial_boat_material_updated", text2, text);
			UIManager.SystemMsg(comment, 4f);
		}
	}

	private void OnReadyDepartTutorial(DepartTutorialReady msg, PacketHeader header)
	{
		_defaultDepartInfo = msg;
		OnReadyDepartAncora();
	}

	private void OnReadyDepartAncora()
	{
		UIManager.MessageBox.Show(T._("완성된 뗏목을 타고 앙코라에서 다른 섬으로 갈 수 있습니다."), delegate
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			Handheld.PlayFullScreenMovie("Movie/LeaveTheANC_movie.mp4", Color.black, (FullScreenMovieControlMode)3, (FullScreenMovieScalingMode)1);
			DepartTutorial();
		}, T._("다른 섬으로 떠나기"));
	}

	private bool TryGetTutorialSession(TutorialSession[] sessions, out TutorialSession session)
	{
		ulong playerId = GameManager.PlayerId;
		int i = 0;
		for (int size = KUtility.GetSize(sessions); i < size; i++)
		{
			if (sessions[i].Players != null && Array.IndexOf(sessions[i].Players, playerId) != -1)
			{
				session = sessions[i];
				return true;
			}
		}
		session = default(TutorialSession);
		return false;
	}

	private void RegistOnPostTouchInteractionTarget()
	{
		if (!_registPreInteraction && ((Object)(object)_tutorialBoat != (Object)null || _hasSession))
		{
			_registPreInteraction = true;
			GameSystem<InteractionSystem>.Instance().PreTouchTarget += PreTouchInteractionTarget;
		}
	}

	private void PreTouchInteractionTarget(InteractionObject obj, ref bool result)
	{
		Artifact targetComponent = obj.GetTargetComponent<Artifact>();
		if (!((Object)(object)targetComponent == (Object)null) && !((Object)(object)_tutorialBoat == (Object)null) && !((Object)(object)targetComponent != (Object)(object)_tutorialBoat) && !_hasSession)
		{
			ParticipateTutorialBoat(_tutorialBoat.EntityId, _tutorialBoat.WorldTile);
			result = true;
		}
	}

	private void ParticipateTutorialBoat(ulong entityId, Point2 tile)
	{
		Connections.Frontend.Send(new ParticipateTutorialBoat
		{
			EntityId = entityId,
			Tile = tile
		});
	}

	private void OnInitSession()
	{
		MakeTutorialBoatToDo();
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		if (target != null)
		{
			GameSystem<InteractionSystem>.Instance().SendTouchMsg();
		}
	}

	public void DepartTutorial()
	{
		Connections.Frontend.Send(new DepartTutorialFor
		{
			EntryPointOffset = _defaultDepartInfo.EntryPointOffset,
			TargetRegionId = _defaultDepartInfo.TargetRegionId
		});
	}

	public void DepartTutorialFor(Player.PlayerInfo friend = null)
	{
		DepartTutorialFor msg = default(DepartTutorialFor);
		if (friend == null)
		{
			msg.TargetRegionId = _defaultDepartInfo.TargetRegionId;
			msg.EntryPointOffset = _defaultDepartInfo.EntryPointOffset;
		}
		else
		{
			msg.TargetRegionId = friend.ReturningRegion.Id;
			msg.EntryPointOffset = -1;
		}
		Connections.Frontend.Send(msg);
	}

	private void OnInitTutorialBoat()
	{
		UpdateTutorialBoatTooltip();
		MakeTutorialBoatToDo();
		InitTutorialBoatSlots();
	}

	private void UpdateTutorialBoatTooltip()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_tutorialBoat == (Object)null || !_hasSession)
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
			int num2 = _tutorialSession.Materials.Get(blueprintSlot.Id, 0);
			string key;
			string value;
			if (num2 == blueprintSlot.RequiredCount)
			{
				key = string.Format("{1}{0}[-]", blueprintSlot.LocalizedName, UIManager.ColorBBCode(UIManager.UILightGray));
				value = string.Format("{2}{0} / {1}[-]", num2, blueprintSlot.RequiredCount, UIManager.ColorBBCode(UIManager.UILightGray));
			}
			else
			{
				key = string.Format("{1}{0}[-]", blueprintSlot.LocalizedName, UIManager.ColorBBCode(UIManager.UIWhite));
				value = string.Format("{2}{3}{0}[-] / {4}{1}[-]", num2, blueprintSlot.RequiredCount, UIManager.ColorBBCode(UIManager.UILightGray), UIManager.ColorBBCode(UIManager.UIYellow), UIManager.ColorBBCode(UIManager.UIWhite));
			}
			list.Add(new KeyValuePair<string, string>(key, value));
		}
		tutorialBoatTooltip.Set(_tutorialBoat, T._("탈출용 뗏목"), list);
		tutorialBoatTooltip.Show(3600f);
	}

	public void UpdateTutorialBoatSlots()
	{
		if (_tutorialSession.Materials != null)
		{
			_boatSlots.SetPrevAssignedItemsDummyCount(_tutorialSession.Materials);
		}
	}

	private void MakeTutorialBoatToDo()
	{
		if (!_hasSession || (Object)(object)_tutorialBoat == (Object)null || _boatToDoCollection.ToDoList.Count > 0)
		{
			return;
		}
		_boatToDoCollection.Title = T._("재료를 채집해서 뗏목에 넣기");
		Blueprint blueprint = _tutorialBoat.Blueprint;
		int i = 0;
		for (int num = blueprint.Slots.Length; i < num; i++)
		{
			BlueprintSlot slot = blueprint.Slots[i];
			TutorialBoatToDo tutorialBoatToDo = new TutorialBoatToDo();
			tutorialBoatToDo.Set(slot);
			_boatToDoCollection.ToDoList.Add(tutorialBoatToDo);
		}
		GameSystem<ToDoListSystem>.Instance().Add(_boatToDoCollection);
		int j = 0;
		for (int count = _boatToDoCollection.ToDoList.Count; j < count; j++)
		{
			if (_boatToDoCollection.ToDoList[j] is TutorialBoatToDo tutorialBoatToDo2)
			{
				tutorialBoatToDo2.OnUpdateTutorialBoatSession(_tutorialSession);
			}
		}
	}

	private void InitTutorialBoatSlots()
	{
		_boatSlots.Set(_tutorialBoat, GameSystem<InventorySystem>.Instance().PlayerInventory);
		UpdateTutorialBoatSlots();
	}

	public void PutTutorialBoatMaterials()
	{
		Connections.Frontend.Send(new PutMaterialsIntoTutorialBoat
		{
			EntityId = _tutorialBoat.EntityId,
			Tile = _tutorialBoat.WorldTile,
			Materials = _boatSlots.CreateMaterialsDictionary()
		});
		_boatSlots.Set(_tutorialBoat, GameSystem<InventorySystem>.Instance().PlayerInventory);
	}

	public void RequestEstimateResult()
	{
	}

	public void Build()
	{
	}

	public void SendDepartTutorial(Artifact tutorialBoatOrPort)
	{
		switch (KSingleton<GameManager>.Instance().Region.Role())
		{
		case Role.Tutorial:
			Connections.Frontend.Send(new DepartTutorial
			{
				EntityId = tutorialBoatOrPort.EntityId,
				Tile = tutorialBoatOrPort.WorldTile
			});
			break;
		case Role.Bootcamp:
			if (this.ReadyToDepartBootcamp != null)
			{
				this.ReadyToDepartBootcamp(tutorialBoatOrPort);
			}
			break;
		}
	}
}
