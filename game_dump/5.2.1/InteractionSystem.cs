using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Durango.Logic.Estate;
using Durango.Logic.Interactions;
using Durango.Logic.Timer;
using Durango.Network;
using Durango.Prologue;
using Durango.Terrain;
using Durango.UI;
using Durango.UI.Popup;
using Durango.Utils;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Estate;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class InteractionSystem : GameSystem<InteractionSystem>
{
	public delegate void InteractionHandler(InteractionObject obj);

	public delegate void PreTouchDelegate(InteractionObject obj, ref bool result);

	[Flags]
	public enum IgnoreInteractionFlags
	{
		None = 0,
		HoveringAirBalloon = 1
	}

	public const float MaxTargetDistance = 2000f;

	public const float CheckDistanceProp = 800f;

	public const float CheckDistanceAnimal = 2000f;

	public static InteractionMenuData CurrentMenu;

	private static readonly HashSet<int> NearHashSet;

	private IgnoreInteractionFlags _ignoreInteraction;

	private InteractionObject _target;

	private readonly Observable<double> _warpholeSearchedAt = new Observable<double>();

	private bool _targetChanged;

	private readonly InteractionMenuList _menuList = new InteractionMenuList();

	private float _touchedValidTime;

	private readonly Dictionary<int, InteractionHandler> _interactionHandlers = new Dictionary<int, InteractionHandler>();

	private PredictTimer _drawTimer;

	private PredictTimer _washBodyTimer;

	private PredictTimer _drinkWaterTimer;

	private PredictTimer _lookAroundTimer;

	private Action<List<InteractionMenuData>> _contextActionFinder;

	private readonly ReservationQueue _reservationQueue = new ReservationQueue();

	private readonly ArtifactInteractions _artifactInteractions = new ArtifactInteractions();

	[CompilerGenerated]
	private static Action<List<InteractionMenuData>> cache0;

	[CompilerGenerated]
	private static Func<GameObject, GameObject> cache1;

	[CompilerGenerated]
	private static Func<GameObject, GameObject> cache2;

	[CompilerGenerated]
	private static Func<GameObject, GameObject> cache3;

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
				_targetChanged = true;
				LastInteractionTarget = value;
			}
			_reservationQueue.Clear();
			if (this.InteractionTargetSelected != null)
			{
				this.InteractionTargetSelected(Target);
			}
		}
	}

	public Observable<double> WarpholeSearchedAt => _warpholeSearchedAt;

	public InteractionObject LastInteractionTarget { get; private set; }

	public InteractionMenuList MenuList => _menuList;

	public Touched LastTouched { get; private set; }

	public ReservationQueue ReservationQueue => _reservationQueue;

	public event PreTouchDelegate PreTouchTarget;

	public event Action<InteractionMenuList, InteractionObject> PostTouched;

	public event Action<InteractionObject> InteractionTargetSelected;

	public event Action<string> OnTouchItemSucceed;

	public event Action<Interaction> Executed;

	private void Start()
	{
		RegisterContextActionFinder(DefaultContextActionFinder);
		_menuList.TimerEnded += delegate
		{
			if (_reservationQueue.Count > 0)
			{
				InteractionMenuData menu = _reservationQueue.Pop();
				TryTargetInteraction(menu);
			}
		};
		_reservationQueue.Init();
		_artifactInteractions.Init();
		Durango.Utils.Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			_drawTimer = new PredictTimer(GameManager.PlayerId, "DrawWater");
			_drawTimer.Started += delegate(PredictTimer timer)
			{
				timer.SetMotion("Water_Gain");
				Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer.Timer).AddIcon(CurrentMenu.Icon);
			};
			_washBodyTimer = new PredictTimer(GameManager.PlayerId, "WashBody");
			_washBodyTimer.Started += delegate(PredictTimer timer)
			{
				timer.SetMotion("Water_Wash");
				Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer.Timer).AddIcon(IconMap.Get(Interaction.WashBody));
			};
			_drinkWaterTimer = new PredictTimer(GameManager.PlayerId, "DrinkWater");
			_drinkWaterTimer.Started += delegate(PredictTimer timer)
			{
				timer.SetMotion("Barehand_DrinkRiver");
				Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer.Timer).AddIcon(IconMap.Get(Interaction.DrinkWater));
			};
			_lookAroundTimer = new PredictTimer(GameManager.PlayerId, "LookAround");
			_lookAroundTimer.Started += delegate(PredictTimer timer)
			{
				timer.SetMotion("Emotion_Browse");
				Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer.Timer).AddIcon(IconMap.Get(Interaction.LookAroundArtifact));
			};
		};
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(OnReady);
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
			else if (_touchedValidTime > 0f && _touchedValidTime < Time.time)
			{
				SendTouchMsg();
			}
		}
	}

	private void OnReady()
	{
		Connections.Frontend.Send(default(GetLastSearchedTime)).On(delegate(LastSearchedTime msg, PacketHeader _)
		{
			_warpholeSearchedAt.Value = msg.SearchedAt;
		});
	}

	public void AddInteractionHandler(Interaction interaction, InteractionHandler handler)
	{
		_interactionHandlers[(int)interaction] = handler;
	}

	public void RegisterContextActionFinder(Action<List<InteractionMenuData>> finder)
	{
		_contextActionFinder = (Action<List<InteractionMenuData>>)Delegate.Combine(_contextActionFinder, finder);
	}

	public static GameObject MovableInteractionObjectFilter(GameObject o)
	{
		if (ObjectIdentifier.IsTargetableEnemy(o, includePets: true) || ObjectIdentifier.IsTargetablePlayer(o))
		{
			return o;
		}
		return null;
	}

	public static GameObject PropInteractionObjectFilter(GameObject o)
	{
		bool flag = false;
		Artifact componentInParent = o.GetComponentInParent<Artifact>();
		if (componentInParent != null)
		{
			if (flag)
			{
				return componentInParent.gameObject;
			}
			if (componentInParent.InteractionType == Artifact.Interaction.Normal)
			{
				return componentInParent.gameObject;
			}
			if (componentInParent.InteractionType == Artifact.Interaction.TouchAndContext)
			{
				TileObject tileObject = PlayerBehavior.LocalPlayer.GetTileObject();
				if (tileObject == null || tileObject.Artifact == null || tileObject.Artifact.InteractionType != Artifact.Interaction.TouchAndContext)
				{
					return componentInParent.gameObject;
				}
			}
			return null;
		}
		NaturalObject componentInParent2 = o.GetComponentInParent<NaturalObject>();
		if (componentInParent2 != null)
		{
			return componentInParent2.gameObject;
		}
		GameObject gameObject = SelectableObject.FindSelectable(o);
		if (gameObject != null)
		{
			return gameObject;
		}
		VehicleProp component = o.GetComponent<VehicleProp>();
		if (component != null && component.IsInteractionMenuVisible)
		{
			return o;
		}
		if (ObjectIdentifier.IsDeadBody(o))
		{
			return o;
		}
		return null;
	}

	public static GameObject ImmovableObjectFilter(GameObject o)
	{
		ImmovableBase componentInParent = o.GetComponentInParent<ImmovableBase>();
		if (componentInParent != null)
		{
			return componentInParent.gameObject;
		}
		return null;
	}

	public static GameObject ArtifactObjectFilter(GameObject o)
	{
		Artifact componentInParent = o.GetComponentInParent<Artifact>();
		if (componentInParent != null)
		{
			return componentInParent.gameObject;
		}
		return null;
	}

	public static GameObject CombatTargetObjectFilter(GameObject o)
	{
		bool flag = CombatSystem.IsPvPEnabled();
		bool includePets = flag;
		if (ObjectIdentifier.IsTargetableEnemy(o, includePets))
		{
			return o;
		}
		if (ObjectIdentifier.IsTargetablePlayer(o))
		{
			return o;
		}
		if (ObjectIdentifier.IsLocalPlayersPet(o))
		{
			return o;
		}
		if (!flag)
		{
			return null;
		}
		Artifact componentInParent = o.GetComponentInParent<Artifact>();
		if (componentInParent != null && componentInParent.Durability != null && componentInParent.Durability.When(0f) > 0.0)
		{
			EstateInfo estateInfo = EstateSystem.GetEstateInfo(componentInParent.WorldTile);
			if (estateInfo != null && estateInfo.License.Type == OwnerType.ClanWarphole && !estateInfo.License.IsProtected() && !ClanSystem.IsMyClanOrAlliance(estateInfo.License.OwnerId))
			{
				return componentInParent.gameObject;
			}
		}
		return null;
	}

	public static void SearchMovableObjects([NotNull] ICollection<GameObject> collection)
	{
		GetNearObjectsInternal(collection, LayerHelper.DefaultMask, 2000f, MovableInteractionObjectFilter);
	}

	public static void SearchPropObjects([NotNull] ICollection<GameObject> collection)
	{
		GetNearObjectsInternal(collection, LayerHelper.PropMask, 800f, PropInteractionObjectFilter);
	}

	public static void SearchCombatTargetObjects([NotNull] ICollection<GameObject> collection)
	{
		GetNearObjectsInternal(collection, LayerHelper.InteractionMask, 2000f, CombatTargetObjectFilter);
	}

	public static void GetNearObjectsInternal([NotNull] ICollection<GameObject> collection, int mask, float checkDistance, Func<GameObject, GameObject> filter = null)
	{
		GetNearObjectsInternal(PlayerBehavior.LocalPlayer.CurrentPosition, collection, mask, checkDistance, filter);
	}

	public static void GetNearObjectsInternal(Vector3 pos, [NotNull] ICollection<GameObject> collection, int mask, float checkDistance, Func<GameObject, GameObject> filter = null)
	{
		int count;
		Collider[] array = Collisions.OverlapSphere(pos, checkDistance, mask, out count);
		NearHashSet.Clear();
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = array[i].gameObject;
			if (filter != null)
			{
				gameObject = filter(gameObject);
			}
			if (!(gameObject == null))
			{
				int hashCode = gameObject.GetHashCode();
				if (!NearHashSet.Contains(hashCode))
				{
					collection.Add(gameObject);
					NearHashSet.Add(hashCode);
				}
			}
		}
	}

	public void SetInteractionTarget(InteractionObject target)
	{
		if (IsIgnoreInteraction())
		{
			return;
		}
		_menuList.Reset();
		Target = target;
		if (Target == null)
		{
			_menuList.Apply();
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
		UISound.PlayClick(UISound.ClickType.InteractionTarget);
		switch (Target.ObjectType)
		{
		case InteractionObject.Type.Animal:
		case InteractionObject.Type.Prop:
			SendTouchMsg();
			break;
		case InteractionObject.Type.PrologueSelectCharacter:
		{
			TriggerPrologueSelectCharacter targetComponent2 = Target.GetTargetComponent<TriggerPrologueSelectCharacter>();
			if ((bool)targetComponent2)
			{
				targetComponent2.Select();
			}
			break;
		}
		case InteractionObject.Type.PropSelectableByClient:
		{
			SelectableObject targetComponent3 = Target.GetTargetComponent<SelectableObject>();
			if ((bool)targetComponent3)
			{
				targetComponent3.InteractionTouched();
			}
			break;
		}
		case InteractionObject.Type.Vehicle:
		{
			VehicleBase targetComponent = Target.GetTargetComponent<VehicleBase>();
			if ((bool)targetComponent)
			{
				targetComponent.InteractionTouched();
			}
			break;
		}
		}
	}

	public void ShowClientMenuList(GameObject obj = null)
	{
		if (obj == null)
		{
			obj = PlayerBehavior.LocalPlayer.gameObject;
		}
		InteractionObject target = new InteractionObject(obj);
		Target = target;
		_menuList.Apply();
	}

	public void SelectTargetInteraction(Interaction action)
	{
		int num = MenuList.IndexOf(action);
		if (num != -1)
		{
			SelectTargetInteractionMenu(MenuList[num]);
		}
		else if (Target != null)
		{
			InteractionMenuData menu = new InteractionMenuData(action);
			SelectTargetInteractionMenu(menu);
		}
	}

	public void SelectTargetInteractionMenu(InteractionMenuData menu, bool selectAll = false)
	{
		if (InteractionMenuData.IsRidableAction(menu.Action))
		{
			TryTargetInteraction(menu, selectAll);
			if (!InteractionMenuData.IsKeepInteractionMenuAction(menu.Action))
			{
				SetInteractionTarget(null);
			}
		}
		else
		{
			if (PlayerBehavior.LocalPlayer.Driver.IsWaitForUnmountMotionFinish)
			{
				return;
			}
			VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
			{
				TryTargetInteraction(menu, selectAll);
				if (!InteractionMenuData.IsKeepInteractionMenuAction(menu.Action))
				{
					SetInteractionTarget(null);
				}
			});
		}
	}

	private void TryTargetInteraction(InteractionMenuData menu, bool selectAll = false)
	{
		if (InteractionMenuData.IsQueueableAction(menu.Action))
		{
			_reservationQueue.TryGetQueueItems(menu.Action, menu.Id, out var items);
			int num = items?.Count ?? 0;
			int num2 = ((!selectAll) ? 1 : (menu.Count - num));
			if (IsQueueingActionDoing())
			{
				_reservationQueue.Push(menu, num2);
				return;
			}
			DoTargetInteraction(menu);
			num2--;
			_reservationQueue.Push(menu, num2);
		}
		else
		{
			if (_reservationQueue.Count > 0)
			{
				_reservationQueue.Clear();
			}
			DoTargetInteraction(menu);
		}
	}

	private bool IsQueueingActionDoing()
	{
		if (!_menuList.HasPlayingTimer())
		{
			return GameSystem<GatheringSystem>.Instance().IsGathering;
		}
		return true;
	}

	private void DoTargetInteraction(InteractionMenuData menu)
	{
		if (Target != null && Target.IsValid())
		{
			CurrentMenu = menu;
			if (menu.Action == Interaction.Collect)
			{
				GameSystem<GatheringSystem>.Instance().Gathering(menu.Id);
				return;
			}
			InteractionHandler interactionHandler = GetInteractionHandler(menu);
			ExecuteInteraction(menu.Action, Target, interactionHandler);
		}
	}

	[NotNull]
	private InteractionHandler GetInteractionHandler(InteractionMenuData menu)
	{
		InteractionHandler interactionHandler = _interactionHandlers.Get((int)menu.Action);
		if (interactionHandler == null)
		{
			interactionHandler = delegate(InteractionObject obj)
			{
				GameObject gameObject = SelectableObject.FindSelectable(obj.Target);
				if (!(gameObject != null) || !gameObject.GetComponent<SelectableObject>().MenuClicked(obj.Target, menu))
				{
					menu.Action.ToString();
				}
			};
		}
		return interactionHandler;
	}

	private void ExecuteInteraction(Interaction action, [NotNull] InteractionObject target, [NotNull] InteractionHandler handler)
	{
		if (InteractionMenuData.IsRangeInteractionMenuAction(action) || target.EntityId == GameManager.PlayerId || target.ObjectType == InteractionObject.Type.PrologueSelectCharacter)
		{
			OnInteractionExecuted(action);
			handler(target);
			return;
		}
		GameObject target2 = target.Target;
		float distance = target.CalcInteractionDistance();
		Durango.Utils.Singleton<PlayerController>.Instance().MoveToTarget(target2, delegate
		{
			OnInteractionExecuted(action);
			handler(target);
		}, distance);
	}

	public void SendTouchMsg()
	{
		_touchedValidTime = 0f;
		if (Target == null)
		{
			return;
		}
		Connections.Frontend.Send(new Messages.Touch
		{
			EntityId = Target.EntityId,
			Tile = new Point2((int)Target.Tile.x, (int)Target.Tile.y),
			EntityType = (ushort)Target.EntityType
		}).On<Touched>(TouchedReceived).All(delegate
		{
			LoadingRingWidget loadingRing = UIManager.Popup.LoadingRing;
			if (loadingRing.AttachMode == LoadingRingWidget.Mode.InteractionTarget)
			{
				loadingRing.Hide();
			}
		});
		UIManager.Popup.LoadingRing.AttachToInteractionTarget();
	}

	private void TouchedReceived(Touched msg, PacketHeader header)
	{
		if (Target != null)
		{
			LastTouched = msg;
			_touchedValidTime = GetTouchedValidTime(msg);
			Target.EntityId = msg.EntityId;
			RefreshInteractionMenu();
		}
	}

	private float GetTouchedValidTime(Touched touched)
	{
		double num = 0.0;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (touched.Workbench.HasValue)
		{
			Messages.Crafting[] craftings = touched.Workbench.Value.Craftings;
			int i = 0;
			for (int size = KUtility.GetSize(craftings); i < size; i++)
			{
				Messages.Crafting crafting = craftings[i];
				double num2 = crafting.Since + (double)crafting.Duration;
				if (predictedServerTime < num2)
				{
					num = ((!(num > 0.0)) ? num2 : Math.Min(num2, num));
				}
			}
		}
		if (num > 0.0)
		{
			return Times.UnixTimeToUnityTime(num);
		}
		return 0f;
	}

	public void RefreshInteractionMenu()
	{
		if (_targetChanged)
		{
			_targetChanged = false;
			_menuList.Reset();
		}
		else
		{
			_menuList.Clear();
		}
		for (int i = 0; i < LastTouched.Interactions.Length; i++)
		{
			Interaction action = (Interaction)LastTouched.Interactions[i];
			InteractionMenuData data = new InteractionMenuData(action);
			_menuList.Add(data);
		}
		for (int j = 0; j < LastTouched.DisabledInteractions.Length; j++)
		{
			Interaction action2 = (Interaction)LastTouched.DisabledInteractions[j];
			InteractionMenuData data2 = new InteractionMenuData(action2);
			data2.Disabled = true;
			_menuList.Add(data2);
		}
		for (int k = 0; k < LastTouched.AccessDeniedInteractions.Length; k++)
		{
			Interaction action3 = (Interaction)LastTouched.AccessDeniedInteractions[k];
			InteractionMenuData data3 = new InteractionMenuData(action3);
			data3.Disabled = true;
			data3.AccessDenied = true;
			_menuList.Add(data3);
		}
		string text = LastTouched.EntityName;
		if (LastTouched.Level > 0 && LastTouched.Collectible.Generators.Length != 0)
		{
			int level = GameSystem<StatisticsSystem>.Instance().Level;
			Color c = StatisticsSystem.RelativeLevelColor(LastTouched.Level - level);
			text += T._("\n{1}{0:lv:}[-]", LastTouched.Level, UIManager.ColorBBCode(c));
		}
		_menuList.Name = text;
		GameSystem<GatheringSystem>.Instance().SetCollectible(LastTouched.Collectible, _menuList);
		if (this.PostTouched != null)
		{
			this.PostTouched(_menuList, Target);
		}
		_menuList.Apply();
		if (this.OnTouchItemSucceed != null)
		{
			string text2 = LastTouched.PrototypeId;
			if (!string.IsNullOrEmpty(LastTouched.Collectible.CollectibleId))
			{
				text2 = LastTouched.Collectible.CollectibleId;
			}
			if (string.IsNullOrEmpty(text2) && Target.EntityType > 0)
			{
				text2 = Target.EntityType.ToString();
			}
			if (Target.GetTargetComponent<PlayerBehavior>() != null)
			{
				text2 = "PC";
			}
			this.OnTouchItemSucceed(text2);
		}
	}

	public void GetContextActionList(List<InteractionMenuData> result)
	{
		result.Clear();
		if (!PlayerBehavior.LocalPlayer.IsAlive || GameManager.Region.IsPvpIsland())
		{
			return;
		}
		if (PlayerBehavior.LocalPlayer.Driver.IsRidingKindOf<VehicleAirBalloon>())
		{
			PlayerBehavior.LocalPlayer.Driver.Vehicle.ContextActionFinder(result);
			return;
		}
		if (_contextActionFinder != null)
		{
			_contextActionFinder(result);
		}
		result.Sort();
	}

	public void DoNoneTargetAction(InteractionMenuData menu)
	{
		CurrentMenu = menu;
		Interaction action = menu.Action;
		if (!InteractionMenuData.IsMovingAction(action))
		{
			Durango.Utils.Singleton<PlayerController>.Instance().StopMove();
		}
		if (!InteractionMenuData.IsRidableAction(action) && PlayerBehavior.LocalPlayer.IsRiding)
		{
			VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
			{
				ExecuteInteraction(action);
			});
		}
		else if (InteractionMenuData.IsVehicleAction(action) && !PlayerBehavior.LocalPlayer.IsRiding)
		{
			if (!(PlayerBehavior.LocalPlayer.Driver.Vehicle == null))
			{
				Durango.Utils.Singleton<PlayerController>.Instance().MoveToTarget(PlayerBehavior.LocalPlayer.Driver.Vehicle.gameObject, delegate
				{
					ExecuteInteraction(action);
				});
			}
		}
		else
		{
			ExecuteInteraction(action);
		}
	}

	private void ExecuteInteraction(Interaction action)
	{
		if (!GameSystem<CombatSystem>.Instance().CombatMode)
		{
			InteractionHandler interactionHandler = _interactionHandlers.Get((int)action);
			if (interactionHandler == null)
			{
				Debug.LogError($"Cannot find action method from {action}");
			}
			else
			{
				interactionHandler(null);
			}
			OnInteractionExecuted(action);
		}
	}

	private void OnInteractionExecuted(Interaction interaction)
	{
		if (this.Executed != null)
		{
			this.Executed(interaction);
		}
	}

	private static void DefaultContextActionFinder(List<InteractionMenuData> result)
	{
		Role role = GameManager.Region.Role();
		if (role != Role.Invalid || role != Role.Tutorial || role != Role.Safehouse)
		{
			if (!PlayerBehavior.LocalPlayer.IsRiding && (TerrainWater.WaterDepthLevel)PlayerBehavior.LocalPlayer.WaterDepthLevel < TerrainWater.WaterDepthLevel.Waist && (bool)PlayerBehavior.LocalPlayer.IsMoving)
			{
				result.Add(Interaction.Dash);
			}
			result.Add(Interaction.WarpToPort);
			result.Add(Interaction.SearchWarphole);
			result.Add(Interaction.CaptureScreenShot);
		}
		BiomeContextAction(result);
		TileObjectContextAction(result);
	}

	private static void BiomeContextAction(List<InteractionMenuData> result)
	{
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		Biome biome = localPlayer.GetBiome();
		foreach (KeyValuePair<string, PutInContainerInfo> putInContainerInfo in Yaml.Util.Singleton<Constants>.Instance.PutInContainerInfos)
		{
			if (putInContainerInfo.Value.Biomes != null && Array.IndexOf(putInContainerInfo.Value.Biomes, biome) != -1)
			{
				result.Add(new InteractionMenuData(Interaction.SelectDrawContainer)
				{
					Id = putInContainerInfo.Key,
					Icon = "act_scoopupwater"
				});
			}
		}
		TerrainWater.WaterDepthLevel waterDepthLevel = localPlayer.WaterDepthLevel;
		if ((byte)localPlayer.Floor == 0 && waterDepthLevel <= TerrainWater.WaterDepthLevel.Waist)
		{
			if (Durango.Terrain.Util.IsWater(biome))
			{
				result.Add(Interaction.WashBody);
			}
			if (Durango.Terrain.Util.IsDrinkable(biome))
			{
				result.Add(Interaction.DrinkWater);
			}
		}
	}

	private static void TileObjectContextAction(List<InteractionMenuData> result)
	{
		Point2 currentTile = PlayerBehavior.LocalPlayer.CurrentTile;
		TileObject tileObject = Durango.Utils.Singleton<TerrainBase>.Instance().GetTileObject(currentTile, warning: false);
		if (tileObject == null || !(tileObject.Artifact != null))
		{
			return;
		}
		Artifact.Interaction interactionType = tileObject.Artifact.InteractionType;
		if (interactionType == Artifact.Interaction.Context || interactionType == Artifact.Interaction.TouchAndContext)
		{
			InteractionMenuData item = new InteractionMenuData(Interaction.InteractionArtifact);
			item.Icon = tileObject.Artifact.ContextIcon;
			result.Add(item);
			if (tileObject.Artifact.ArtifactState.InteriorMood.HasValue && !string.IsNullOrEmpty(tileObject.Artifact.ArtifactState.InteriorMood.Value.SelectedId))
			{
				InteractionMenuData item2 = new InteractionMenuData(Interaction.LookAroundArtifact);
				result.Add(item2);
			}
		}
		int? value = tileObject.Artifact.Stories.Value;
		if (value.HasValue)
		{
			byte value2 = PlayerBehavior.LocalPlayer.Floor.Value;
			if (value2 + 1 < value.Value)
			{
				result.Add(Interaction.ToUpstair);
			}
			if (value2 - 1 >= 0)
			{
				result.Add(Interaction.ToDownstair);
			}
		}
	}

	public void SearchWarpholes(Action<SearchedPOIs> onSuccess)
	{
		Connections.Frontend.Send(default(SearchPOIs)).On(delegate(SearchedPOIs msg, PacketHeader _)
		{
			if (onSuccess != null)
			{
				onSuccess(msg);
			}
			_warpholeSearchedAt.Value = msg.SearchedAt;
		});
	}

	public void ToggleIgnoreInteraction(IgnoreInteractionFlags flag, bool on)
	{
		if (on)
		{
			_ignoreInteraction |= flag;
		}
		else
		{
			_ignoreInteraction &= ~flag;
		}
	}

	public bool IsIgnoreInteraction()
	{
		return _ignoreInteraction != IgnoreInteractionFlags.None;
	}

	public void Draw<TV>(TV msg)
	{
		_drawTimer.Play(Connections.Frontend.Ping + 10f);
		bool confirming = false;
		Connections.Frontend.Send(msg).On(delegate(Messages.Timer m, PacketHeader _)
		{
			_drawTimer.Play(m.Duration);
		}).On(delegate(EnergyWarning warningMsg, PacketHeader header)
		{
			_drawTimer.Pause();
			confirming = true;
			LowEnergyWarning.Show(warningMsg, header, delegate(LowEnergyWarning.Result result)
			{
				if (result == LowEnergyWarning.Result.IgnoreWarning)
				{
					_drawTimer.Play(Connections.Frontend.Ping + 10f);
				}
				else
				{
					_drawTimer.Stop();
				}
				confirming = false;
			});
		})
			.Rest(delegate
			{
				if (confirming)
				{
					LowEnergyWarning.Hide();
					confirming = false;
				}
				_drawTimer.Stop();
			});
	}

	public void WashBody()
	{
		_washBodyTimer.Play(Connections.Frontend.Ping + 10f);
		Connections.Frontend.Send(default(WashBody)).On(delegate(Messages.Timer msg, PacketHeader _)
		{
			_washBodyTimer.Play(msg.Duration);
		}).Rest(delegate
		{
			_washBodyTimer.Stop();
		});
	}

	public void DrinkWater()
	{
		_drinkWaterTimer.Play(Connections.Frontend.Ping + 10f);
		Connections.Frontend.Send(default(DrinkWater)).On(delegate(Messages.Timer msg, PacketHeader _)
		{
			_drinkWaterTimer.Play(msg.Duration);
		}).Rest(delegate
		{
			_drinkWaterTimer.Stop();
		});
	}

	public void ArtifactLookAround(Point2 tile, Artifact artifact)
	{
		_lookAroundTimer.Play(Connections.Frontend.Ping + 10f);
		Connections.Frontend.Send(new LookAroundMood
		{
			EntityId = artifact.EntityId,
			Tile = tile
		}).On(delegate(Messages.Timer msg, PacketHeader _)
		{
			_lookAroundTimer.Play(msg.Duration);
		}).Rest(delegate
		{
			_lookAroundTimer.Stop();
		});
	}

	static InteractionSystem()
	{
		NearHashSet = new HashSet<int>();
	}
}
