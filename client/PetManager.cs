using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Offline;
using Durango.Terrain;
using Durango.UI;
using Durango.UI.Popup;
using Durango.Utils;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Ability;
using Shared.Animal;
using Shared.Pet;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class PetManager : Durango.Utils.Singleton<PetManager>
{
	private readonly Dictionary<string, PetAI> _petObjects = new Dictionary<string, PetAI>();

	private readonly List<Messages.Pet> _petsWaitingMaster = new List<Messages.Pet>();

	private readonly Dictionary<string, Messages.Pet> _pets = new Dictionary<string, Messages.Pet>();

	private string _playerPetId;

	private readonly PetSkillStates _playerPetSkillStates = new PetSkillStates();

	[CanBeNull]
	private GrazingPetManager _grazingPetManager;

	[CompilerGenerated]
	private static Durango.Network.Connection.MessageHandler<RecommendedRecipes> cache0;

	public PlayerContext playercontext;

	[NotNull]
	public PetSkillStates PlayerPetSkillStates => _playerPetSkillStates;

	public event Action AirBalloonUnmounted;

	public event Action<AnimalBehavior> PetAppeared;

	public event Action<AnimalBehavior> PetDisappeared;

	public event Action<Messages.Pet, Messages.Pet> PetChanged;

	public event Action<DomesticationResult> OnDomesticationResult;

	public event Action<PetActiveSkillUsed> PetActiveSkillUsed;

	public event Action<PetActiveSkillCanceled> PetActiveSkillCanceled;

	private void Start()
	{
		Durango.Utils.Singleton<GameManager>.Instance().PreReconnect += GameManager_PreReconnect;
		Durango.Utils.Singleton<PlayerManager>.Instance().PlayerAppeared += PlayerManager_PlayerAppeared;
		AddInterractionHandler();
		Connections.Frontend.On<AppearPet>(OnAppearPetMsg);
		Connections.Frontend.On<DisappearPet>(OnDisappearPetMsg);
		Connections.Frontend.On(delegate(Messages.Pet msg, PacketHeader header)
		{
			if (GetPet(msg.EntityId).HasValue)
			{
				ProcessPetMsg(msg);
			}
		});
		Connections.Frontend.On<PetActiveSkillUsed>(OnPetActiveSkillUsed);
		Connections.Frontend.On<PetActiveSkillCanceled>(OnPetActiveSkillCanceled);
		Connections.Frontend.On(delegate(FeedingSuccess msg, PacketHeader header)
		{
			PetAI petObject = GetPetObject(msg.PetId);
			if (petObject != null)
			{
				petObject.EatOut();
			}
		});
		Connections.Frontend.On<UnmountAirBalloon>(delegate
		{
			VehicleBase localVehicleBase = GetLocalVehicleBase();
			if (!(localVehicleBase == null))
			{
				UnmountAirbaloon(localVehicleBase);
			}
		});
		Connections.Frontend.On(delegate(DomesticationResult msg, PacketHeader header)
		{
			if (this.OnDomesticationResult != null)
			{
				this.OnDomesticationResult(msg);
			}
		});
		Connections.Frontend.On<GrazedPets>(OnGrazedPets);
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			if (GameManager.Region.Role() == Role.Personal || GameManager.ClusterMode != 0)
			{
				Connections.Frontend.Send(default(GetGrazedPets));
			}
		});
		GameSystem<InteractionSystem>.Instance().RegisterContextActionFinder(ContextActionFinder);
	}

	private void Update()
	{
		if (_grazingPetManager != null)
		{
			_grazingPetManager.Update();
		}
	}

	private void OnGrazedPets(GrazedPets pets, PacketHeader header)
	{
		if (_grazingPetManager != null || KUtility.GetSize(pets.Data) != 0)
		{
			if (_grazingPetManager == null)
			{
				_grazingPetManager = new GrazingPetManager();
			}
			_grazingPetManager.Set(pets.Data);
		}
	}

	private void GameManager_PreReconnect()
	{
		foreach (PetAI value in _petObjects.Values)
		{
			if (value != null)
			{
				value.RemovePet();
			}
		}
		_petObjects.Clear();
		_petsWaitingMaster.Clear();
		_pets.Clear();
	}

	private void OnAppearPetMsg(AppearPet msg, PacketHeader header)
	{
		if (msg.PetData.HasValue)
		{
			bool num = _pets.ContainsKey(msg.EntityId);
			ProcessPetMsg(msg.PetData.Value);
			if (!num)
			{
				AddNewPet(msg.PetData.Value, msg.IsAlive);
				_playerPetId = msg.EntityId;
			}
		}
	}

	private void AddNewPet(Messages.Pet msg, bool isAlive)
	{
		_ = msg;
		string prefabPath = AnimalYaml.GetPrefabPath(msg.GetAnimalType());
		Durango.Utils.Singleton<AssetBundleManager>.Instance().RequestAsset(prefabPath, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			Vector3 spawnPosition = new Vector3(0f, UnityEngine.Random.Range(0f, 360f), 0f);
			AnimalBehavior animalBehavior = InstantiateAnimalObject(asset, spawnPosition, msg.EntityId, msg.GetAnimalType(), isAlive);
			UpdatePet(msg, animalBehavior);
			PetAI component = animalBehavior.GetComponent<PetAI>();
			if (!TrySetTamerPlayer(component, msg))
			{
				_petsWaitingMaster.Add(msg);
			}
			_petObjects[msg.EntityId] = component;
			animalBehavior.ShowEyeTrail(msg.HasEyeTrail());
			if (this.PetAppeared != null)
			{
				this.PetAppeared(animalBehavior);
			}
		});
	}

	public static AnimalBehavior InstantiateAnimalObject(UnityEngine.Object asset, Vector3 spawnPosition, string entityId, int animalId, bool isAlive)
	{
		if (!ValidatePetObject(asset as GameObject))
		{
			return null;
		}
		Vector3 euler = new Vector3(0f, UnityEngine.Random.Range(0f, 360f), 0f);
		GameObject gameObject = UnityEngine.Object.Instantiate(asset, spawnPosition, Quaternion.Euler(euler)) as GameObject;
		if (gameObject == null)
		{
			return null;
		}
		PetAI petAI = gameObject.AddMissingComponent<PetAI>();
		AnimalBehavior component = gameObject.GetComponent<AnimalBehavior>();
		component.EntityId = entityId;
		component.EntityTypeId = animalId;
		component.SetAlive(isAlive, fromInit: true);
		petAI.Init(animalId);
		return component;
	}

	private void ProcessPetMsg(Messages.Pet msg)
	{
		if (!string.IsNullOrEmpty(msg.EntityId))
		{
			Messages.Pet arg = _pets.Get(msg.EntityId);
			_pets[msg.EntityId] = msg;
			PetAI petObject = GetPetObject(msg.EntityId);
			if (petObject != null)
			{
				UpdatePet(msg, petObject.TargetAnimal);
			}
			if (msg.TamerEntityId == PlayerBehavior.LocalPlayer.EntityId)
			{
				_playerPetSkillStates.Set(msg);
			}
			if (!string.IsNullOrEmpty(arg.EntityId) && this.PetChanged != null)
			{
				this.PetChanged(arg, msg);
			}
		}
	}

	private static bool TrySetTamerPlayer([NotNull] PetAI petAi, Messages.Pet msg)
	{
		PlayerBehavior playerIncludeLocalPlayer = Durango.Utils.Singleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(msg.TamerEntityId);
		if (playerIncludeLocalPlayer == null)
		{
			return false;
		}
		if (playerIncludeLocalPlayer.IsLocalPlayer && playerIncludeLocalPlayer.Driver.IsVehicleKindOf<VehicleAirBalloon>())
		{
			return false;
		}
		playerIncludeLocalPlayer.Driver.SetVehicle(petAi.GetComponent<VehicleBase>(), playSpawnMotion: true);
		petAi.SetMaster(playerIncludeLocalPlayer.gameObject, msg.IsBoarding);
		return true;
	}

	private void PlayerManager_PlayerAppeared(PlayerBehavior player)
	{
		for (int num = _petsWaitingMaster.Count - 1; num >= 0; num--)
		{
			Messages.Pet msg = _petsWaitingMaster[num];
			if (!(msg.TamerEntityId != playercontext.AppearPlayer.EntityId))
			{
				PetAI petAI = _petObjects.Get(msg.EntityId);
				if (petAI == null)
				{
					_petsWaitingMaster.RemoveAt(num);
					break;
				}
				if (TrySetTamerPlayer(petAI, msg))
				{
					_petsWaitingMaster.RemoveAt(num);
				}
			}
		}
	}

	private static bool ValidatePetObject(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetComponent<VehiclePet>() == null)
		{
			Debug.LogError("Requires components : Vehicle");
			return false;
		}
		if (obj.GetComponent<AnimalBehavior>() == null)
		{
			Debug.LogError("Requires components : Animal");
			return false;
		}
		return true;
	}

	private void UpdatePet(Messages.Pet msg, [NotNull] AnimalBehavior animal)
	{
		animal.SetName(msg.Name);
		PetStats stat = msg.Stat;
		animal.Level = msg.Statistics.Level;
		animal.SetSurvivalGauge(stat.Life, null);
		animal.SetOld(stat.IsOld);
		VehiclePet component = animal.GetComponent<VehiclePet>();
		component.MoveSpeed = msg.Statistics.DerivedAbilities.Get(Derived.Speed, 0f);
		component.IsRidable = SingletonDict<int, Yaml.Pet>.Get(msg.EntityType)?.IsRidable ?? false;
		animal.GetComponent<PetAI>().SetHungryGauge(stat.Hungry);
	}

	[CanBeNull]
	public PetAI GetPetObject(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		return _petObjects.Get(id);
	}

	public Messages.Pet? GetPet(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		if (_pets.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public string GetPlayerPetId()
	{
		return _playerPetId;
	}

	public bool HandleMoveMsg(Move msg)
	{
		Messages.Pet? pet = GetPet(msg.EntityId);
		if (!pet.HasValue)
		{
			return false;
		}
		PetAI petObject = GetPetObject(msg.EntityId);
		if (petObject == null)
		{
			return false;
		}
		if (!pet.Value.IsBoarding)
		{
			petObject.TargetAnimal.HandleMoveMsg(msg);
		}
		return true;
	}

	private void OnDisappearPetMsg(DisappearPet msg, PacketHeader header)
	{
		if (msg.EntityId == _playerPetId)
		{
			_playerPetSkillStates.Set(null);
			_playerPetId = null;
		}
		_pets.Remove(msg.EntityId);
		PetAI petObj = GetPetObject(msg.EntityId);
		_petObjects.Remove(msg.EntityId);
		if (petObj == null)
		{
			return;
		}
		for (int i = 0; i < _petsWaitingMaster.Count; i++)
		{
			if (_petsWaitingMaster[i].EntityId == msg.EntityId)
			{
				_petsWaitingMaster.RemoveAt(i);
				break;
			}
		}
		PlayerBehavior playerIncludeLocalPlayer = Durango.Utils.Singleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(msg.TamerEntityId);
		if (playerIncludeLocalPlayer != null)
		{
			Driver driver = playerIncludeLocalPlayer.Driver;
			if (driver.Vehicle != null && driver.Vehicle.EntityId == msg.EntityId)
			{
				driver.ReturnVehicle(playReturnMotion: true, delegate
				{
					if (petObj != null)
					{
						petObj.Return();
					}
					if (this.PetDisappeared != null)
					{
						this.PetDisappeared(petObj.GetComponent<AnimalBehavior>());
					}
				});
				return;
			}
		}
		if (petObj != null)
		{
			petObj.Return();
		}
		if (this.PetDisappeared != null)
		{
			this.PetDisappeared(petObj.GetComponent<AnimalBehavior>());
		}
	}

	private void OnPetActiveSkillUsed(PetActiveSkillUsed msg, PacketHeader header)
	{
		_playerPetSkillStates.Used(msg.SkillId);
		if (this.PetActiveSkillUsed != null)
		{
			this.PetActiveSkillUsed(msg);
		}
	}

	private void OnPetActiveSkillCanceled(PetActiveSkillCanceled msg, PacketHeader header)
	{
		_playerPetSkillStates.Canceled(msg.SkillId);
		if (this.PetActiveSkillCanceled != null)
		{
			this.PetActiveSkillCanceled(msg);
		}
	}

	private void AddInterractionHandler()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Mount, delegate(InteractionObject target)
		{
			VehicleBase localVehicleBase7 = GetLocalVehicleBase(target);
			if (!(localVehicleBase7 == null))
			{
				AnimalBehavior component3 = localVehicleBase7.GetComponent<AnimalBehavior>();
				if (!component3)
				{
					Debug.LogError("Airballoon must use with Interaction.MountAirBalloon");
				}
				else if (!component3.IsAlive)
				{
					UIManager.SystemMsg(T._("죽어있는 대상에게 탑승할 수 없습니다."));
				}
				else
				{
					PetAI component4 = component3.GetComponent<PetAI>();
					if (component4 != null && component4.Hungry == PetAI.HungryState.NoRide)
					{
						UIManager.SystemMsg(T._("동물이 배가 고파 탈 수 없습니다."));
						UIManager.Inventory.OpenPetFeedingPopup(component3.EntityId);
					}
					else
					{
						Connections.Frontend.Send(default(Mount));
					}
				}
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Dismount, delegate(InteractionObject target)
		{
			VehicleBase localVehicleBase6 = GetLocalVehicleBase(target);
			if (!(localVehicleBase6 == null))
			{
				if ((bool)localVehicleBase6.GetComponent<AnimalBehavior>())
				{
					Connections.Frontend.Send(default(Unmount));
				}
				else
				{
					Debug.LogError("Airballoon must use with Interaction.DismountAirBalloon");
				}
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ReturnPet, delegate(InteractionObject target)
		{
			VehicleBase localVehicleBase5 = GetLocalVehicleBase(target);
			if (!(localVehicleBase5 == null))
			{
				ReturnMyPet(localVehicleBase5.EntityId);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RenamePet, delegate(InteractionObject target)
		{
			VehicleBase localVehicleBase4 = GetLocalVehicleBase(target);
			if (!(localVehicleBase4 == null))
			{
				AnimalBehavior animal = localVehicleBase4.GetComponent<AnimalBehavior>();
				if ((bool)animal)
				{
					UIManager.Popup.Tooltip<TextInputPopup>().Show(delegate(string newName)
					{
						RenamePet(animal.EntityId, newName);
					}, T._("새로운 이름을 적어주세요"), animal.GetName());
				}
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ResurrectPet, delegate(InteractionObject target)
		{
			VehicleBase localVehicleBase3 = GetLocalVehicleBase(target);
			if (!(localVehicleBase3 == null))
			{
				AnimalBehavior component2 = localVehicleBase3.GetComponent<AnimalBehavior>();
				if ((bool)component2)
				{
					ResurrectPet(component2.EntityId);
				}
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.OpenPetMenu, delegate(InteractionObject target)
		{
			VehicleBase localVehicleBase2 = GetLocalVehicleBase(target);
			if (!(localVehicleBase2 == null))
			{
				AnimalBehavior component = localVehicleBase2.GetComponent<AnimalBehavior>();
				if (!(component == null))
				{
					UIManager.FindScript<PetGroup>().Open(component.EntityId);
				}
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RideBalloon, delegate
		{
			Yaml.Cost cost = Yaml.Util.Singleton<CostsYaml>.Instance.BalloonTicket;
			Action<bool> onOkCancel = delegate(bool ok)
			{
				if (ok)
				{
					Wallet wallet = InventorySystem.Wallet;
					cost.Payable(wallet, out var byVoucher, out var byCurrency);
					if (byVoucher || byCurrency)
					{
						if (PlayerBehavior.LocalPlayer.Driver.IsVehicleKindOf<VehiclePet>())
						{
							ReturnMyPet(PlayerBehavior.LocalPlayer.Driver.Vehicle.EntityId);
						}
						Connections.Frontend.Send(new MountAirBalloon
						{
							WithVoucher = byVoucher
						});
					}
					else
					{
						UIManager.SystemMsg(T._("{0:이} 필요합니다.", cost.CostToString(wallet)));
					}
				}
			};
			string comment = T._("열기구에 탑승하시겠습니까?");
			string subText = T._("[icon=icon_make_alert] 한 번 내리면 다시 탈 수 없습니다.\n[icon=icon_make_alert] 게임 접속과 상관없이 15분 경과 후에는 자동으로 열기구에서 내려집니다.\n[icon=icon_make_alert] 부적합한 위치에 있었다면 출발한 항구로 되돌아옵니다.");
			UIManager.MessageBox.ShowCostConfirm(cost, comment, subText, onOkCancel);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.MountAirBalloon, delegate(InteractionObject target)
		{
			VehicleBase localVehicleBase = GetLocalVehicleBase(target);
			if (!(localVehicleBase == null))
			{
				PlayerBehavior.LocalPlayer.Driver.Mount(localVehicleBase, delegate
				{
					Connections.Frontend.Send(default(MountAirBalloon));
				});
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.DismountAirBalloon, delegate(InteractionObject target)
		{
			VehicleAirBalloon balloon = GetLocalVehicleBase(target) as VehicleAirBalloon;
			if (balloon == null)
			{
				UIManager.SystemMsg(T._("현재 열기구에 탑승중이 아닙니다."), 2f);
			}
			else if (!balloon.OnLandingArea)
			{
				UIManager.SystemMsg(T._("현재 위치에서는 착륙이 불가능합니다."), 2f);
			}
			else
			{
				UIManager.MessageBox.Show(T._("열기구에서 한 번 내리면 다시 탈 수 없습니다.\n현재 위치에서 내리겠습니까?"), delegate(bool ok)
				{
					if (ok)
					{
						UnmountAirbaloon(balloon);
					}
				});
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.MountVehicle, delegate(InteractionObject target)
		{
			VehicleProp vehicleProp = GetLocalVehicleBase(target) as VehicleProp;
			if (!(vehicleProp == null))
			{
				Artifact artifact2 = vehicleProp.GetArtifact();
				if (!(artifact2 == null))
				{
					Connections.Frontend.Send(new MountVehicle
					{
						EntityId = artifact2.EntityId,
						Tile = artifact2.WorldTile
					});
				}
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.AddProjectileToVehicle, delegate(InteractionObject target)
		{
			Artifact artifact = target.GetTargetComponent<Artifact>();
			if (!(artifact == null) && artifact.ArtifactState.Catapult.HasValue)
			{
				CatapultState catapult = artifact.ArtifactState.Catapult.Value;
				PopupItemSelector selector = UIManager.Popup.Tooltip<PopupItemSelector>();
				selector.Filter(delegate(ItemData item)
				{
					int j = 0;
					for (int size = KUtility.GetSize(catapult.RequiredProjectileTags); j < size; j++)
					{
						if (item.HasTag(catapult.RequiredProjectileTags[j]))
						{
							return true;
						}
					}
					return false;
				}).ConfirmText(T._("투척물 넣기")).SelectableCount(-1)
					.OnConfirmed(delegate(IList<ItemData> list)
					{
						if (list != null)
						{
							UIManager.MessageBox.ShowLockConfirm(list, delegate(string[] elems)
							{
								InventorySystem.PutInItems(artifact.EntityId, artifact.WorldTile, elems);
							});
						}
					})
					.Title(string.Format("{0} [icon=icon_question_big]", T._("투척물 주머니")), $"{catapult.RemainedProjectilesSize} / {catapult.MaxProjectilesSize}")
					.TitleClicked(delegate
					{
						WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
						string[] array = new string[KUtility.GetSize(catapult.RequiredProjectileTags)];
						for (int i = 0; i < array.Length; i++)
						{
							Yaml.Tag tag = SingletonDict<string, Yaml.Tag>.Get(catapult.RequiredProjectileTags[i]);
							if (tag == null)
							{
								array[i] = catapult.RequiredProjectileTags[i];
							}
							else
							{
								array[i] = tag.Name;
							}
						}
						widgetTooltipControl.Set(null, T._("투척기에는 {0:l:{}|, }만 들어갈 수 있습니다.", new object[1] { array }), 300);
						widgetTooltipControl.AutoPosition = false;
						widgetTooltipControl.Show();
						widgetTooltipControl.HideArrow();
						widgetTooltipControl.Widget.SetPosition(selector.Widget.GetPosition(0f, 1f) + Vector3.up * 10f, 0f, 0f);
					});
				selector.Show();
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.PetActiveSkill, delegate
		{
			if (GetPet(GetPlayerPetId()).HasValue)
			{
				string id = InteractionSystem.CurrentMenu.Id;
				UsePetActiveSkill(id);
			}
		});
	}

	[CanBeNull]
	private static VehicleBase GetLocalVehicleBase(InteractionObject interactionTarget = null)
	{
		if (interactionTarget == null || interactionTarget.Target == null)
		{
			return PlayerBehavior.LocalPlayer.Driver.Vehicle;
		}
		return interactionTarget.Target.GetComponentInChildren<VehicleBase>();
	}

	public static void ReleasePet(string id, Action onSuccess = null)
	{
		VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
		{
			ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = Connections.Frontend.Send(new ReleasePet
			{
				PetId = id
			});
			if (onSuccess != null)
			{
				replyMessageHandlerRegistrar.On<OK>(delegate
				{
					onSuccess();
				});
			}
		});
	}

	public static void ReleaseReinFromCage(string id, PropKey cage, Action onSuccess = null)
	{
		VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
		{
			ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = Connections.Frontend.Send(new ReleaseReinFromCage
			{
				ItemId = id,
				EntityId = cage.EntityId,
				Tile = cage.Tile
			});
			if (onSuccess != null)
			{
				replyMessageHandlerRegistrar.On<OK>(delegate
				{
					onSuccess();
				});
			}
		});
	}

	public static void ReinifyPet(string id, string itemId, Action onSuccess = null)
	{
		VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
		{
			ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = Connections.Frontend.Send(new ReinifyPet
			{
				PetId = id,
				ItemId = itemId
			});
			if (onSuccess != null)
			{
				replyMessageHandlerRegistrar.On<OK>(delegate
				{
					onSuccess();
				});
			}
		});
	}

	public static void SpawnMyPet(string id, Action onSuccess = null)
	{
		VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
		{
			if (!(PlayerBehavior.LocalPlayer.GetComponent<Driver>() == null))
			{
				ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = Connections.Frontend.Send(new SpawnPet
				{
					PetId = id
				});
				if (onSuccess != null)
				{
					replyMessageHandlerRegistrar.On<OK>(delegate
					{
						onSuccess();
					});
				}
			}
		});
	}

	public static void ReturnMyPet(string id, Action onSuccess = null)
	{
		VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
		{
			ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = Connections.Frontend.Send(new ReturnPet
			{
				PetId = id
			});
			if (onSuccess != null)
			{
				replyMessageHandlerRegistrar.On<OK>(delegate
				{
					onSuccess();
				});
			}
		});
	}

	public static void RenamePet(string id, string name, Action onSuccess = null)
	{
		ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = Connections.Frontend.Send(new RenamePet
		{
			PetId = id,
			Name = name
		});
		if (onSuccess != null)
		{
			replyMessageHandlerRegistrar.On<OK>(delegate
			{
				onSuccess();
			});
		}
	}

	public void ResurrectPet(string id, Action onSuccess = null)
	{
		PlayerController.MotionUpdater.Motion("Avatar_Crying");
		Connections.Frontend.Send(new ResurrectPet
		{
			PetId = id
		}).On<OK>(delegate
		{
			if (onSuccess != null)
			{
				onSuccess();
			}
			KUtility.DelayedCall(this, delegate
			{
				PlayerController.MotionUpdater.Motion("Avatar_Jump");
			}, 1f);
		}).On<RecommendedRecipes>(OnRecommendedRecipes);
	}

	public static void GrazePets(string[] ids, Action onSuccess = null)
	{
		VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
		{
			Connections.Frontend.Send(new GrazePets
			{
				PetIdsToGraze = ids
			}).All(delegate(Packet packet)
			{
				if (Packet.IsSuccess(packet) && onSuccess != null)
				{
					onSuccess();
				}
			});
		});
	}

	private static void OnRecommendedRecipes(RecommendedRecipes msg, PacketHeader header)
	{
		int num = 0;
		foreach (KeyValuePair<string, int> tag in msg.Tags)
		{
			num = Mathf.Max(tag.Value, num);
		}
		string comment = ((num <= 1) ? T._("다음 중 하나의 아이템이 필요합니다.\n<em>{0}</em>", msg.TagNames) : T._("다음 중 하나의 아이템이 필요합니다.\n{1:lv:}이상 <em>{0}</em>", msg.TagNames, num));
		GatheringSystem.ShowRequireTagPopup(msg.RecipeIds, msg.Tags, num, comment);
	}

	public static void GetPetList([NotNull] Action<PetsInfo?> onResult)
	{
		Connections.Frontend.Send(default(GetPetsInfo));
		Connections.Frontend.On(delegate(PetsInfo msg, PacketHeader header)
		{
			onResult(msg);
		});
	}

	public static void FeedPet(PropKey prop, string target, string[] items, Action<bool> onResult)
	{
		Connections.Frontend.Send(new FeedInCage
		{
			EntityId = prop.EntityId,
			Tile = prop.Tile,
			PetId = target,
			ItemIds = items
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static void FeedPet(string target, string[] items, Action<bool> onResult)
	{
		Connections.Frontend.Send(new Feeding
		{
			PetId = target,
			FoodIds = items
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static void PutInCage(string id, Point2 tile, string petId, Action<bool> onResult)
	{
		Connections.Frontend.Send(new PutInCage
		{
			EntityId = id,
			Tile = tile,
			PetId = petId
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static void TakeOutCage(string id, Point2 tile, string reins, Action<bool> onResult)
	{
		Connections.Frontend.Send(new TakeOutFromCage
		{
			EntityId = id,
			Tile = tile,
			PetId = reins
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	private void ContextActionFinder(List<InteractionMenuData> actions)
	{
		AddPetContextAction(GetPet(GetPlayerPetId()), actions);
		PetAI petObject = GetPetObject(GetPlayerPetId());
		if (!(petObject == null))
		{
			VehicleBase component = petObject.GetComponent<VehicleBase>();
			if (!(component == null))
			{
				component.ContextActionFinder(actions);
			}
		}
	}

	private void AddPetContextAction(Messages.Pet? pet, List<InteractionMenuData> actions)
	{
		if (!pet.HasValue)
		{
			return;
		}
		Messages.Pet value = pet.Value;
		if (KUtility.GetSize(value.Statistics.AvailableActiveSkill) == 0)
		{
			return;
		}
		SkillContext currentSkillContext = GetCurrentSkillContext();
		Messages.PetActiveSkill[] availableActiveSkill = value.Statistics.AvailableActiveSkill;
		for (int i = 0; i < availableActiveSkill.Length; i++)
		{
			Messages.PetActiveSkill petActiveSkill = availableActiveSkill[i];
			Yaml.PetActiveSkill petActiveSkill2 = PetActiveSkills.Get(petActiveSkill.SkillId, petActiveSkill.Rank);
			if (petActiveSkill2 != null && (petActiveSkill2.Context & currentSkillContext) != 0)
			{
				InteractionMenuData item = new InteractionMenuData(Interaction.PetActiveSkill);
				item.Id = petActiveSkill.SkillId;
				item.Name = petActiveSkill2.Name;
				item.Icon = petActiveSkill2.Icon;
				item.SetColor(PresetColor.UIPet);
				actions.Add(item);
			}
		}
	}

	public static SkillContext GetCurrentSkillContext()
	{
		SkillContext skillContext = SkillContext.None;
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		skillContext = ((!GameSystem<CombatSystem>.Instance().CombatMode) ? (skillContext | SkillContext.Idle) : (skillContext | SkillContext.Battle));
		if (localPlayer.Driver.IsRiding)
		{
			skillContext |= SkillContext.Riding;
		}
		if ((TerrainWater.WaterDepthLevel)localPlayer.WaterDepthLevel >= TerrainWater.WaterDepthLevel.Swim)
		{
			skillContext |= SkillContext.Swimming;
		}
		if (!localPlayer.IsAlive)
		{
			skillContext |= SkillContext.Dead;
		}
		Point2 currentTile = localPlayer.CurrentTile;
		TileObject tileObject = Durango.Utils.Singleton<TerrainBase>.Instance().GetTileObject(currentTile, warning: false);
		if (tileObject != null && tileObject.Artifact != null && tileObject.Artifact.GetArtifactComponent<ModularArtifact>() != null)
		{
			skillContext |= SkillContext.InsideBuilding;
		}
		return skillContext;
	}

	public static void UnmountVehicle()
	{
		VehicleProp vehicleProp = GetLocalVehicleBase() as VehicleProp;
		if (!(vehicleProp == null))
		{
			Artifact artifact = vehicleProp.GetArtifact();
			if (!(artifact == null))
			{
				Connections.Frontend.Send(new UnmountVehicle
				{
					EntityId = artifact.EntityId,
					Tile = artifact.WorldTile
				});
			}
		}
	}

	private void UnmountAirbaloon([NotNull] VehicleBase vehicle)
	{
		PlayerBehavior.LocalPlayer.Driver.ReserveUnmount(delegate
		{
			Connections.Frontend.Send(default(UnmountAirBalloon));
			if (this.AirBalloonUnmounted != null)
			{
				this.AirBalloonUnmounted();
			}
		});
	}

	public static void FinishDomestication(string id, Point2 tile, string reinItemId, Action<DomesticationResult?> onResult)
	{
		Connections.Frontend.Send(new FinishDomestication
		{
			EntityId = id,
			Tile = tile,
			ItemId = reinItemId
		}).On(delegate(DomesticationResult msg, PacketHeader header)
		{
			onResult(msg);
		}).Rest(delegate
		{
			onResult(null);
		});
	}

	public static void PutInReinsToCage(string artifactId, Point2 artifactTile, string reinItemId, Action<bool> onResult)
	{
		Connections.Frontend.Send(new PutInReinsToCage
		{
			EntityId = artifactId,
			Tile = artifactTile,
			ItemId = reinItemId
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static void StartPetTask(PropKey cage, string petId, string taskId, Action<bool> onResult)
	{
		Connections.Frontend.Send(new StartPetTask
		{
			EntityId = cage.EntityId,
			Tile = cage.Tile,
			PetId = petId,
			TaskId = taskId
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static void CancelPetTask(PropKey cage, string petId, Action<bool> onResult)
	{
		Connections.Frontend.Send(new CancelPetTask
		{
			EntityId = cage.EntityId,
			Tile = cage.Tile,
			PetId = petId
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static void FinishPetTask(PropKey cage, string petId, Action<bool> onResult)
	{
		Connections.Frontend.Send(new FinishPetTask
		{
			EntityId = cage.EntityId,
			Tile = cage.Tile,
			PetId = petId
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static void StartDomestication(string artifactId, Point2 artifactTile, string reinItemId, Action<bool> onResult)
	{
		Connections.Frontend.Send(new StartDomestication
		{
			EntityId = artifactId,
			Tile = artifactTile,
			ItemId = reinItemId
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static void CancelDomestication(string artifactId, Point2 artifactTile, string reinItemId, Action<bool> onResult)
	{
		Connections.Frontend.Send(new CancelDomestication
		{
			EntityId = artifactId,
			Tile = artifactTile,
			ItemId = reinItemId
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static void TakeOutReinFromCage(string artifactId, Point2 artifactTile, string reinItemId, Action<bool> onResult)
	{
		Connections.Frontend.Send(new TakeOutReinFromCage
		{
			EntityId = artifactId,
			Tile = artifactTile,
			ItemId = reinItemId
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static void PutItemsForDomestication(string artifactId, Point2 artifactTile, string reinItemId, string[] itemIds)
	{
		Connections.Frontend.Send(new PutItemsForDomestication
		{
			EntityId = artifactId,
			Tile = artifactTile,
			ReinId = reinItemId,
			ItemIds = itemIds
		});
	}

	public static void GetMilestoneCandidate(string petId, int milestoneId, [NotNull] Action<MilestoneCandidates?> onResult)
	{
		Connections.Frontend.Send(new GetMilestoneCandidate
		{
			PetId = petId,
			MilestoneId = milestoneId
		}).On(delegate(MilestoneCandidates msg, PacketHeader header)
		{
			onResult(msg);
		}).Rest(delegate
		{
			onResult(null);
		});
	}

	public static void PickMilestone(string petId, Action<MilestoneResult?> onResult)
	{
		Connections.Frontend.Send(new PickMilestone
		{
			PetId = petId
		}).On(delegate(MilestoneResult msg, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(msg);
			}
		}).Rest(delegate
		{
			if (onResult != null)
			{
				onResult(null);
			}
		});
	}

	public static void PickMilestoneAgain(string petId, bool withVoucher, Action<MilestoneResult?> onResult)
	{
		Connections.Frontend.Send(new PickMilestoneAgain
		{
			PetId = petId,
			WithVoucher = withVoucher
		}).On(delegate(MilestoneResult msg, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(msg);
			}
		}).Rest(delegate
		{
			if (onResult != null)
			{
				onResult(null);
			}
		});
	}

	public static void AcceptMilestone(string petId, Action<MilestoneResult?> onResult)
	{
		Connections.Frontend.Send(new AcceptMilestone
		{
			PetId = petId
		}).On(delegate(MilestoneResult msg, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(msg);
			}
		}).Rest(delegate
		{
			if (onResult != null)
			{
				onResult(null);
			}
		});
	}

	public static void DrawActiveSkill(string petId, Action<DrawSkillResult?> onResult)
	{
		Connections.Frontend.Send(new DrawActiveSkill
		{
			PetId = petId
		}).On(delegate(DrawSkillResult msg, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(msg);
			}
		}).Rest(delegate
		{
			if (onResult != null)
			{
				onResult(null);
			}
		});
	}

	public static void RedrawActiveSkill(string petId, Messages.PetActiveSkill skill, bool withVoucher, Action<DrawSkillResult?> onResult)
	{
		Connections.Frontend.Send(new RedrawActiveSkill
		{
			PetId = petId,
			Skill = skill,
			WithVoucher = withVoucher
		}).On(delegate(DrawSkillResult msg, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(msg);
			}
		}).Rest(delegate
		{
			if (onResult != null)
			{
				onResult(null);
			}
		});
	}

	public static void GetAvailableTask(string petId, PropKey cage, Action<AvailableTask?> onResult)
	{
		Connections.Frontend.Send(new GetAvailableTask
		{
			PetId = petId,
			EntityId = cage.EntityId,
			Tile = cage.Tile
		}).On(delegate(AvailableTask msg, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(msg);
			}
		}).Rest(delegate
		{
			if (onResult != null)
			{
				onResult(null);
			}
		});
	}

	public void UsePetActiveSkill(string skillId)
	{
		_playerPetSkillStates.Reserved(skillId);
		Connections.Frontend.Send(new UsePetActiveSkill
		{
			SkillId = skillId
		}).All(delegate(Packet packet)
		{
			if (!Packet.IsSuccess(packet))
			{
				_playerPetSkillStates.Canceled(skillId);
			}
		});
	}

	public static void RevertPetRank(string petId, Action<RevertPetRankCandidate?> onResult)
	{
		Connections.Frontend.Send(new RevertPetRank
		{
			PetId = petId
		}).On(delegate(RevertPetRankCandidate msg, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(msg);
			}
		}).Rest(delegate
		{
			if (onResult != null)
			{
				onResult(null);
			}
		});
	}

	public static void AcceptPetRank(string petId, Action<bool> onResult)
	{
		Connections.Frontend.Send(new AcceptPetRank
		{
			PetId = petId
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static void GetPreviewPet(int entityType, PetRank rank, int level, [NotNull] Action<Messages.Pet?> onResult)
	{
		Connections.Frontend.Send(new GetPreviewPet
		{
			PetEntityType = (ushort)entityType,
			Rank = rank,
			Level = level
		}).On(delegate(Messages.Pet pet, PacketHeader header)
		{
			onResult(pet);
		}).Rest(delegate
		{
			onResult(null);
		});
	}

	public static void GrazedPetToMyPet(string entityId)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("remove_grazing_pet");
		stringBuilder.AppendFormat(" {0}", entityId);
		Connections.Frontend.Send(new Cheat
		{
			_Cheat = stringBuilder.ToString().Trim()
		});
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.Append("grazed_to_pet");
		stringBuilder2.AppendFormat(" {0}", entityId);
		Connections.Frontend.Send(new Cheat
		{
			_Cheat = stringBuilder2.ToString().Trim()
		});
	}
}
