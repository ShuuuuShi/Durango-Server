using System;
using System.Collections.Generic;
using ClanData;
using Crafting;
using Estate;
using InteractionData;
using ItemSystem;
using JetBrains.Annotations;
using K1Network;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Estate;
using Shared.Faction;
using Shared.Item;
using Shared.Laboratory;
using Shared.System;
using TimerData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class InteractionSystem : GameSystem<InteractionSystem>
{
	public delegate void PreTouchDelegate(InteractionObject obj, ref bool result);

	public delegate void InteractionHandler(InteractionObject obj);

	public delegate void ContextActionFinder(ref List<InteractionData.Interaction> actions);

	public const float MaxTargetDistance = 2000f;

	public const float CheckDistanceProp = 800f;

	public const float CheckDistanceAnimal = 2000f;

	public static InteractionMenuData CurrentMenu;

	private InteractionObject _target;

	private bool _isTargetChange;

	private InteractionMenuList _menuList = new InteractionMenuList();

	private readonly Dictionary<int, InteractionHandler> _interactionHandlers = new Dictionary<int, InteractionHandler>();

	private readonly Dictionary<int, InteractionHandler> _subInteractionHandlers = new Dictionary<int, InteractionHandler>();

	private ContextActionFinder _contextActionFinder;

	public InteractionObject Target
	{
		get
		{
			return _target;
		}
		private set
		{
			_target = value;
			if (value != null)
			{
				_isTargetChange = true;
				LastInteractionTarget = value;
			}
			if (this.InteractionTargetSelected != null)
			{
				this.InteractionTargetSelected(Target);
			}
		}
	}

	public InteractionObject LastInteractionTarget { get; private set; }

	public InteractionMenuList MenuList => _menuList;

	public event PreTouchDelegate PreTouchTarget;

	public event Action<InteractionObject> InteractionTargetSelected;

	public event Action<InteractionMenuData> InteractionMenuProcessed;

	public event Action<string> OnTouchItemSucceed;

	public event Action<InteractionData.Interaction> ActionExecuted;

	private void Awake()
	{
		MenuList.MenuTimerFinished += OnFinishMenuTimer;
	}

	private void Update()
	{
		if (Target != null)
		{
			if (!Target.IsValid())
			{
				SetInteractionTarget(null);
			}
			else if (Target.Distance > 2000f)
			{
				SetInteractionTarget(null);
			}
		}
	}

	public void AddInteractionHandler(Shared.System.Interaction interaction, InteractionHandler handler)
	{
		_interactionHandlers[(int)interaction] = handler;
	}

	public void AddInteractionHandler(InteractionData.Interaction interaction, InteractionHandler handler)
	{
		_subInteractionHandlers[(int)interaction] = handler;
	}

	public void RegisterContextActionFinder(ContextActionFinder finder)
	{
		_contextActionFinder = (ContextActionFinder)Delegate.Combine(_contextActionFinder, finder);
	}

	public static GameObject MovableInteractionObjectFilter(GameObject o)
	{
		if (ObjectIdentifier.IsTargetableEnemy(o, includePets: true) || ObjectIdentifier.IsTargetablePlayer(o, filterClan: false))
		{
			return o;
		}
		return null;
	}

	public static GameObject PropInteractionObjectFilter(GameObject o)
	{
		bool flag = false;
		Artifact componentInParent = o.GetComponentInParent<Artifact>();
		if ((Object)(object)componentInParent != (Object)null)
		{
			return (!flag && componentInParent.InteractionDisabled) ? null : ((Component)componentInParent).gameObject;
		}
		NaturalObject componentInParent2 = o.GetComponentInParent<NaturalObject>();
		if ((Object)(object)componentInParent2 != (Object)null)
		{
			return ((Component)componentInParent2).gameObject;
		}
		GameObject val = SelectableObject.FindSelectable(o);
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		if (o.CompareTag("Trap") || ObjectIdentifier.IsDeadBody(o))
		{
			return o;
		}
		return null;
	}

	public static GameObject ImmovableObjectFilter(GameObject o)
	{
		ImmovableBase componentInParent = o.GetComponentInParent<ImmovableBase>();
		return (!((Object)(object)componentInParent != (Object)null)) ? null : ((Component)componentInParent).gameObject;
	}

	public static GameObject ArtifactObjectFilter(GameObject o)
	{
		Artifact componentInParent = o.GetComponentInParent<Artifact>();
		return (!((Object)(object)componentInParent != (Object)null)) ? null : ((Component)componentInParent).gameObject;
	}

	public static GameObject CombatTargetObjectFilter(GameObject o)
	{
		if (ObjectIdentifier.IsTargetableEnemy(o, includePets: false) || ObjectIdentifier.IsTargetablePlayer(o, filterClan: true))
		{
			return o;
		}
		Artifact componentInParent = o.GetComponentInParent<Artifact>();
		if ((Object)(object)componentInParent != (Object)null && !componentInParent.InteractionDisabled && (componentInParent.GetArtifactComponent<EstateFlag>() != null || componentInParent.GetArtifactComponent<Defensive>() != null))
		{
			TileObject tileObject = TerrainA6.GetTileObject(componentInParent.WorldTile);
			if (tileObject != null && GameSystem<ClanSystem>.Instance().GetClanWarState(tileObject.OwnerId) == ClanData.ClanWarState.Match)
			{
				return ((Component)componentInParent).gameObject;
			}
		}
		return null;
	}

	public static void SearchMovableObjects([NotNull] ICollection<GameObject> collection)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetNearObjectsInternal(collection, LayerMask.op_Implicit(LayerHelper.DefaultMask), 2000f, MovableInteractionObjectFilter);
	}

	public static void SearchPropObjects([NotNull] ICollection<GameObject> collection)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetNearObjectsInternal(collection, LayerMask.op_Implicit(LayerHelper.PropMask), 800f, PropInteractionObjectFilter);
	}

	public static void SearchCombatTargetObjects([NotNull] ICollection<GameObject> collection, float distance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetNearObjectsInternal(collection, LayerMask.op_Implicit(LayerHelper.InteractionMask), distance, CombatTargetObjectFilter);
	}

	public static void GetNearObjectsInternal([NotNull] ICollection<GameObject> collection, int mask, float checkDistance, Func<GameObject, GameObject> filter = null)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		GetNearObjectsInternal(PlayerBehavior.LocalPlayer.CurrentPosition, collection, mask, checkDistance, filter);
	}

	public static void GetNearObjectsInternal(Vector3 pos, [NotNull] ICollection<GameObject> collection, int mask, float checkDistance, Func<GameObject, GameObject> filter = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		collection.Clear();
		int count;
		Collider[] array = KCollisionUtility.OverlapSphere(pos, checkDistance, mask, out count);
		for (int i = 0; i < count; i++)
		{
			Collider val = array[i];
			GameObject val2 = ((Component)val).gameObject;
			if (filter != null)
			{
				val2 = filter(val2);
			}
			if (!((Object)(object)val2 == (Object)null) && !collection.Contains(val2))
			{
				collection.Add(val2);
			}
		}
	}

	public void SetInteractionTarget(InteractionObject target)
	{
		MenuList.Reset();
		Target = target;
		if (Target == null)
		{
			MenuList.Apply();
			UIManager.FindScript<CombatGroup>().SetFocusTarget(null);
			return;
		}
		bool result = false;
		if (this.PreTouchTarget != null)
		{
			this.PreTouchTarget(target, ref result);
			if (result)
			{
				return;
			}
		}
		switch (Target.ObjectType)
		{
		case InteractionObject.Type.Animal:
		case InteractionObject.Type.Prop:
			SendTouchMsg();
			break;
		case InteractionObject.Type.PrologueSelectCharacter:
		{
			TriggerPrologueSelectCharacter targetComponent2 = Target.GetTargetComponent<TriggerPrologueSelectCharacter>();
			if (Object.op_Implicit((Object)(object)targetComponent2))
			{
				targetComponent2.Touched();
			}
			break;
		}
		case InteractionObject.Type.PropSelectableByClient:
		{
			SelectableObject targetComponent3 = Target.GetTargetComponent<SelectableObject>();
			if (Object.op_Implicit((Object)(object)targetComponent3))
			{
				targetComponent3.InteractionTouched();
			}
			break;
		}
		case InteractionObject.Type.Vehicle:
		{
			Vehicle targetComponent = Target.GetTargetComponent<Vehicle>();
			if (Object.op_Implicit((Object)(object)targetComponent))
			{
				targetComponent.InteractionTouched();
			}
			break;
		}
		}
	}

	public void SetSelfMenuList(InteractionMenuList menuList)
	{
		InteractionObject target = new InteractionObject(((Component)PlayerBehavior.LocalPlayer).gameObject);
		Target = target;
		menuList.Apply();
	}

	public void SelectInteractionMenu(InteractionMenuData menu)
	{
		if (menu.AccessDenied)
		{
			HandleAccesDenied((Shared.System.Interaction)menu.Action);
			return;
		}
		InteractionObject curTarget = Target;
		if (PlayerBehavior.LocalPlayer.IsRiding)
		{
			Vehicle.RequestUnmountIfRiding(immediately: true, delegate
			{
				SelectInteractionMenuInternal(menu, curTarget);
			});
		}
		else
		{
			SelectInteractionMenuInternal(menu, curTarget);
		}
	}

	public void SelectInteractionMenuInternal(InteractionMenuData menu, InteractionObject target)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (target == null || InteractionMenuData.IsRangeInteractionMenuAction(menu))
		{
			InteractionMenu_Do(menu);
			return;
		}
		if (menu.IsServer && menu.Action == 506)
		{
			GameSystem<GatheringSystem>.Instance().Gathering(menu.GatheringId);
			return;
		}
		if ((Object)(object)target.Target == (Object)null)
		{
			KSingleton<PlayerController>.Instance().MoveToTarget(target.Position, delegate
			{
				InteractionMenu_Do(menu);
			}, 200f);
			return;
		}
		Shared.System.Interaction action = (Shared.System.Interaction)menu.Action;
		if (target.EntityId == GameManager.PlayerId)
		{
			InteractionMenu_Do(menu);
			return;
		}
		if (target.ObjectType == InteractionObject.Type.PrologueSelectCharacter)
		{
			InteractionMenu_Do(menu);
			return;
		}
		switch (action)
		{
		case Shared.System.Interaction.Cage:
		case Shared.System.Interaction.Warp:
			KSingleton<PlayerController>.Instance().MoveToTarget(target.Target, delegate
			{
				InteractionMenu_Do(menu);
			}, 100f);
			return;
		case Shared.System.Interaction.AnimalInspection:
			KSingleton<PlayerController>.Instance().MoveToTarget(target.Target, delegate
			{
				InteractionMenu_Do(menu);
			}, 640f);
			return;
		case Shared.System.Interaction.AnimalFeed:
		case Shared.System.Interaction.AnimalHeal:
		case Shared.System.Interaction.NaturalInspection:
		case Shared.System.Interaction.NaturalWater:
		case Shared.System.Interaction.NaturalCure:
		case Shared.System.Interaction.NaturalPoisonPurify:
		case Shared.System.Interaction.NaturalWaterPurify:
			KSingleton<PlayerController>.Instance().MoveToTarget(target.Target, delegate
			{
				InteractionMenu_Do(menu);
			}, 200f);
			return;
		}
		float distanceThresh = CalcInteractionDistance(target);
		KSingleton<PlayerController>.Instance().MoveToTarget(target.Target, delegate
		{
			InteractionMenu_Do(menu);
		}, distanceThresh);
	}

	private void InteractionMenu_Do(InteractionMenuData menu)
	{
		InteractionObject lastInteractionTarget = LastInteractionTarget;
		if (lastInteractionTarget == null || !lastInteractionTarget.IsValid())
		{
			return;
		}
		CurrentMenu = menu;
		Dictionary<int, InteractionHandler> dictionary = ((!menu.IsServer) ? _subInteractionHandlers : _interactionHandlers);
		if (dictionary.TryGetValue(menu.Action, out var value))
		{
			value(lastInteractionTarget);
		}
		else
		{
			bool flag = false;
			GameObject val = SelectableObject.FindSelectable(lastInteractionTarget.Target);
			if ((Object)(object)val != (Object)null)
			{
				flag = val.GetComponent<SelectableObject>().MenuClicked(lastInteractionTarget.Target, menu);
			}
			if (!flag)
			{
				Type type = ((!menu.IsServer) ? typeof(InteractionData.Interaction) : typeof(Shared.System.Interaction));
				string text = ((!menu.IsServer) ? ((InteractionData.Interaction)menu.Action).ToString() : ((Shared.System.Interaction)menu.Action).ToString());
			}
		}
		if (this.InteractionMenuProcessed != null)
		{
			this.InteractionMenuProcessed(menu);
		}
	}

	private static void HandleAccesDenied(Shared.System.Interaction action)
	{
		List<string> list = GameSystem<EstateSystem>.Instance().Rights.NeededRightNames(action);
		for (int i = 0; i < list.Count; i++)
		{
			list[i] = LocalizeSystem.Get(list[i]);
		}
		string text = string.Empty;
		if (list.Count == 1)
		{
			text = list[0];
		}
		else if (list.Count > 1)
		{
			text = LocalizeSystem.Format("#one_of", string.Join(", ", list.ToArray()));
		}
		UIManager.SystemMsg(LocalizeSystem.Format("#estate_accessright_needed", text), 4f);
	}

	public static float CalcInteractionDistance(InteractionObject target)
	{
		CharacterBehavior targetComponent = target.GetTargetComponent<CharacterBehavior>();
		if ((Object)(object)targetComponent != (Object)null)
		{
			return Mathf.Max(targetComponent.XRadius, targetComponent.YRadius);
		}
		SelectableObject targetComponent2 = target.GetTargetComponent<SelectableObject>();
		if ((Object)(object)targetComponent2 != (Object)null)
		{
			return targetComponent2.InteractableDistance;
		}
		ImmovableBase targetComponent3 = target.GetTargetComponent<ImmovableBase>();
		return (!Object.op_Implicit((Object)(object)targetComponent3)) ? 100f : targetComponent3.InteractionDistance;
	}

	public void SendTouchMsg()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (Target != null)
		{
			Connections.Frontend.Send(new Touch
			{
				EntityId = Target.EntityId,
				Tile = new Point2((int)Target.Tile.x, (int)Target.Tile.y),
				EntityType = (ushort)Target.EntityType
			}).On<Touched>(TouchedReceived);
		}
	}

	private string MakeContactFactionInteractionName(InteractionObject target, Shared.System.Interaction interaction)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		FactionType factionType = GameSystem<MapSystem>.Instance().FindFactionFromCraterTile(new Point2(Target.Tile));
		if (factionType == FactionType.Invalid)
		{
			return null;
		}
		Yaml.Faction value = null;
		if (SingletonDict<FactionType, Yaml.Faction>.Instance.TryGetValue(factionType, out value))
		{
			return T._("{0}에게 {1}", value.name, LocalizeUtil.Get(interaction));
		}
		return null;
	}

	private void TouchedReceived(Touched msg, PacketHeader header)
	{
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		if (Target == null)
		{
			return;
		}
		Target.EntityId = msg.EntityId;
		InteractionMenuList menuList = MenuList;
		if (_isTargetChange)
		{
			_isTargetChange = false;
			menuList.Reset();
		}
		else
		{
			menuList.Clear();
		}
		bool flag = false;
		for (int i = 0; i < msg.Interactions.Length; i++)
		{
			Shared.System.Interaction interaction = (Shared.System.Interaction)msg.Interactions[i];
			InteractionMenuData data = new InteractionMenuData(interaction);
			if (interaction == Shared.System.Interaction.ContactFaction || interaction == Shared.System.Interaction.RecontactFaction)
			{
				string name = MakeContactFactionInteractionName(Target, interaction);
				data.Name = name;
			}
			if (interaction == Shared.System.Interaction.Attack)
			{
				flag = true;
			}
			menuList.Add(data);
		}
		for (int j = 0; j < msg.DisabledInteractions.Length; j++)
		{
			Shared.System.Interaction interaction2 = (Shared.System.Interaction)msg.DisabledInteractions[j];
			string name2 = null;
			if (interaction2 == Shared.System.Interaction.ContactFaction || interaction2 == Shared.System.Interaction.RecontactFaction)
			{
				name2 = MakeContactFactionInteractionName(Target, interaction2);
			}
			InteractionMenuData data2 = new InteractionMenuData(interaction2);
			data2.Name = name2;
			data2.Disabled = true;
			menuList.Add(data2);
		}
		for (int k = 0; k < msg.AccessDeniedInteractions.Length; k++)
		{
			Shared.System.Interaction action = (Shared.System.Interaction)msg.AccessDeniedInteractions[k];
			InteractionMenuData data3 = new InteractionMenuData(action);
			data3.Disabled = true;
			data3.AccessDenied = true;
			menuList.Add(data3);
		}
		UIManager.FindScript<CombatGroup>().SetFocusTarget((!flag) ? null : Target.Target);
		Workbench? workbench = msg.Workbench;
		if (workbench.HasValue)
		{
			Workbench value = msg.Workbench.Value;
			for (int l = 0; l < value.Craftings.Length; l++)
			{
				Messages.Crafting crafting = value.Craftings[l];
				Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(crafting.RecipeId);
				InteractionMenuData data4 = new InteractionMenuData(InteractionData.Interaction.CraftingItem);
				data4.Id = crafting.Id;
				data4.Name = recipe.LocalizedName;
				data4.Icon = recipe.Icon;
				data4.Duration = crafting.Duration;
				double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
				float num = (float)(crafting.Since - predictedServerTime + (double)Time.time);
				float until = num + crafting.Duration;
				data4.SetTimer(new TimerData.Timer(num, until));
				GameSystem<TimerSystem>.Instance().Register(data4.Timer);
				menuList.Add(data4);
			}
			for (int m = 0; m < value.CraftedItems.Length; m++)
			{
				Item item = value.CraftedItems[m];
				InteractionMenuData data5 = new InteractionMenuData(Shared.System.Interaction.Take);
				data5.Id = item.Id;
				data5.Name = item.Name;
				data5.Icon = item.Icon;
				menuList.Add(data5);
			}
		}
		Dispenser? dispenser = msg.Dispenser;
		if (dispenser.HasValue)
		{
			for (int n = 0; n < msg.Dispenser.Value.Items.Length; n++)
			{
				Item item2 = msg.Dispenser.Value.Items[n];
				InteractionMenuData data6 = new InteractionMenuData(Shared.System.Interaction.Take);
				data6.Id = item2.Id;
				data6.Name = item2.Name;
				data6.Icon = item2.Icon;
				menuList.Add(data6);
			}
		}
		Artifact targetComponent = Target.GetTargetComponent<Artifact>();
		if ((Object)(object)targetComponent != (Object)null)
		{
			if (targetComponent.PostProcessTimer != null && !targetComponent.PostProcessTimer.IsStop && !targetComponent.Blueprint.IsClanEstateFlag)
			{
				menuList.Add(new InteractionMenuData(InteractionData.Interaction.SkipPostprocess));
			}
			Messages.EstateInfo? estate = targetComponent.ArtifactState.Estate;
			if (targetComponent.BuildCompleted && estate.HasValue && estate.Value.Type == OwnerType.Player && estate.Value.OwnerId == GameManager.PlayerId)
			{
				ItemData package = PackArtifactSystem.GetPackage();
				if (package == null)
				{
					menuList.Add(new InteractionMenuData(InteractionData.Interaction.StartPakingArtifact));
				}
				else
				{
					switch (package.ArtifactPackage.Status)
					{
					case PackageStatus.Packing:
						menuList.Add(new InteractionMenuData(InteractionData.Interaction.PackingArtifact));
						break;
					case PackageStatus.Sealed:
					case PackageStatus.Unpacking:
						menuList.Add(new InteractionMenuData(InteractionData.Interaction.UnpackArtifact));
						break;
					}
				}
			}
			Laboratory artifactComponent = targetComponent.GetArtifactComponent<Laboratory>();
			if (artifactComponent != null && targetComponent.BuildCompleted)
			{
				AddMenuItemToLaboratory(menuList, artifactComponent);
			}
			InfoTooltip tooltip = UIManager.Popup.Tooltip<InfoTooltip>();
			ArtifactInfoTooltip.Show(tooltip, targetComponent);
		}
		string text = msg.EntityName;
		if (msg.Level > 0)
		{
			int level = GameSystem<StatisticsSystem>.Instance().Level;
			int levelDiff = msg.Level - level;
			Color c = StatisticsSystem.RelativeLevelColor(levelDiff);
			text += T.Format("\n{1}{0:lv:}[-]", msg.Level, UIManager.ColorBBCode(c));
		}
		menuList.Name = text;
		menuList.Apply();
		GameSystem<GatheringSystem>.Instance().SetCollectible(msg.Collectible, refreshInventory: true);
		if (this.OnTouchItemSucceed != null)
		{
			string text2 = msg.PrototypeId;
			if (!string.IsNullOrEmpty(msg.Collectible.CollectibleId))
			{
				text2 = msg.Collectible.CollectibleId;
			}
			if (string.IsNullOrEmpty(text2) && Target.EntityType > 0)
			{
				text2 = Target.EntityType.ToString();
			}
			if ((Object)(object)Target.GetTargetComponent<PlayerBehavior>() != (Object)null)
			{
				text2 = "PC";
			}
			this.OnTouchItemSucceed(text2);
		}
	}

	public void GetPlayerActionList(List<InteractionData.Interaction> result)
	{
		result.Clear();
		if (_contextActionFinder != null)
		{
			_contextActionFinder(ref result);
		}
	}

	public void DoNoneTargetAction(InteractionData.Interaction action)
	{
		if (!IsRidableAction(action) && PlayerBehavior.LocalPlayer.IsRiding)
		{
			Vehicle.RequestUnmountIfRiding(immediately: true, delegate
			{
				DoActionInternal(action);
			});
		}
		else if (IsVehicleAction(action) && !PlayerBehavior.LocalPlayer.IsRiding)
		{
			KSingleton<PlayerController>.Instance().MoveToTarget(((Component)PlayerBehavior.LocalPlayer.Driver.Vehicle).gameObject, delegate
			{
				DoActionInternal(action);
			});
		}
		else
		{
			DoActionInternal(action);
		}
	}

	private bool IsRidableAction(InteractionData.Interaction action)
	{
		if (action == InteractionData.Interaction.SearchWarphole)
		{
			return true;
		}
		return false;
	}

	private bool IsVehicleAction(InteractionData.Interaction action)
	{
		if (InteractionData.Interaction.VehicleInteractionsBegin <= action && action <= InteractionData.Interaction.VehicleInteractionsEnd)
		{
			return true;
		}
		return false;
	}

	private void DoActionInternal(InteractionData.Interaction action)
	{
		if (!GameSystem<CombatSystem>.Instance().CombatMode)
		{
			InteractionHandler interactionHandler = _subInteractionHandlers.Get((int)action);
			if (interactionHandler == null)
			{
				Debug.LogError((object)$"Cannot find action method from {action}");
			}
			else
			{
				interactionHandler(null);
			}
			if (this.ActionExecuted != null)
			{
				this.ActionExecuted(action);
			}
		}
	}

	public void DeclareWar(Point2 position)
	{
		TileObject tileObject = TerrainA6.GetTileObject(position, warning: false);
		if (tileObject == null || tileObject.EstateId == 0L)
		{
			return;
		}
		Estate.EstateInfo estateInfo = GameSystem<EstateSystem>.Instance().GetEstateInfo(tileObject.EstateId);
		if (estateInfo == null || !estateInfo.IsValid())
		{
			return;
		}
		DeclareWar msg = new DeclareWar
		{
			Position = position,
			EstateId = estateInfo.Id,
			ClanId = estateInfo.Owner
		};
		ClanSystem.GetClanInfo(estateInfo.Owner, delegate(Clan clan)
		{
			ClanSystem.GetClanWarCosts(delegate(Costs costs)
			{
				if (costs._Costs != null)
				{
					Dictionary<Currency, int>.Enumerator enumerator = costs._Costs.GetEnumerator();
					if (enumerator.MoveNext())
					{
						KeyValuePair<Currency, int> current = enumerator.Current;
						string[] items = new string[2]
						{
							T._("선포"),
							T._("취소")
						};
						UIManager.MessageBox.Show(T._("{0} 부족의 점령지에 선전포고 하시겠습니까?\n {1} 부족 자금이 필요합니다.", clan.Name, ItemSystem.Inventory.CurrencyFormat(current.Value, current.Key)), delegate(int index)
						{
							if (index == 0)
							{
								Connections.Frontend.Send(msg);
							}
						}, items);
					}
				}
			});
		});
	}

	private void OnFinishMenuTimer(InteractionMenuData menu)
	{
		if (!menu.IsServer)
		{
			switch ((InteractionData.Interaction)menu.Action)
			{
			case InteractionData.Interaction.CraftingItem:
			case InteractionData.Interaction.ResearchPlant:
			case InteractionData.Interaction.ResearchMine:
			case InteractionData.Interaction.ResearchAnimal:
			case InteractionData.Interaction.ResearchTool:
			case InteractionData.Interaction.ResearchClothes:
			case InteractionData.Interaction.ResearchCook:
			case InteractionData.Interaction.ResearchConstruction:
			case InteractionData.Interaction.ResearchSurvival:
			case InteractionData.Interaction.ResearchEcology:
			case InteractionData.Interaction.ResearchAttack:
			case InteractionData.Interaction.ResearchDefense:
			case InteractionData.Interaction.ResearchRecovery:
				SendTouchMsg();
				break;
			}
		}
	}

	public void UpdateSearchWarpholeCooltime()
	{
		Connections.Frontend.Send(default(GetLastSearchedTime)).On(delegate(LastSearchedTime msg, PacketHeader _)
		{
			UIManager.FindScript<ContextActionGroup>().RefreshSearchWarpholeCooltime(msg.SearchedAt);
		});
	}

	private ResearchCategory GetResearchCategoryByArtifactId(string artifactId)
	{
		Array values = Enum.GetValues(typeof(ResearchCategory));
		for (int i = 0; i < values.Length; i++)
		{
			ResearchCategory researchCategory = (ResearchCategory)(int)values.GetValue(i);
			if (artifactId.IndexOf(researchCategory.ToString().ToLower()) != -1)
			{
				return researchCategory;
			}
		}
		return ResearchCategory.Invalid;
	}

	private static bool GetResearchInteraction(string researchId, out InteractionData.Interaction interaction)
	{
		try
		{
			interaction = (InteractionData.Interaction)(int)Enum.Parse(typeof(InteractionData.Interaction), "Research" + researchId, ignoreCase: true);
		}
		catch (ArgumentException)
		{
			interaction = InteractionData.Interaction.SkipPostprocess;
			return false;
		}
		return true;
	}

	private void AddMenuItemToLaboratory(InteractionMenuList menuList, Laboratory laboratory)
	{
		ResearchCategory researchCategoryByArtifactId = GetResearchCategoryByArtifactId(laboratory.Artifact.ArtifactId);
		if (researchCategoryByArtifactId == ResearchCategory.Invalid)
		{
			return;
		}
		bool nowResearching = laboratory.GetNowResearching();
		Dictionary<string, ClanResearch>.Enumerator enumerator = SingletonDict<string, ClanResearch>.Instance.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key = enumerator.Current.Key;
			ClanResearch value = enumerator.Current.Value;
			if (researchCategoryByArtifactId != value.category || !GetResearchInteraction(key, out var interaction))
			{
				continue;
			}
			if (nowResearching)
			{
				if (key == laboratory.ResearchId)
				{
					double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
					float num = (float)(laboratory.ResearchSince - predictedServerTime + (double)Time.time);
					float until = num + (float)value.duration;
					InteractionMenuData data = new InteractionMenuData(interaction);
					data.Duration = value.duration;
					data.SetTimer(new TimerData.Timer(num, until));
					GameSystem<TimerSystem>.Instance().Register(data.Timer);
					menuList.Add(data);
				}
			}
			else
			{
				menuList.Add(new InteractionMenuData(interaction));
			}
		}
	}
}
