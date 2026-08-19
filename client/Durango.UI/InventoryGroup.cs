using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.Logic.Timer;
using Durango.Render.Particle;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI;

[Uri("Inventory")]
public class InventoryGroup : UIBase
{
	[Serializable]
	private struct CurrencyEffect
	{
		public ParticleType Particle;

		public string Bone;
	}

	[SerializeField]
	private UITitle _titleWidget;

	[CanBeNull]
	[SerializeField]
	private Transform _inventorySizeContainer;

	[SerializeField]
	private InventoryContainerBase _inventory;

	[SerializeField]
	[EnumList(typeof(Currency), false, 0, -1)]
	private CurrencyEffect[] _currencyParticle;

	private readonly List<string> _expiredItems = new List<string>();

	public static bool IsShow { get; private set; }

	private void Awake()
	{
		_openCloseSound = UISound.GroupType.Inventory;
		base.TryClose();
	}

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("가방"));
		if (_inventorySizeContainer != null)
		{
			_titleWidget.Object.SetTitleNext(_inventorySizeContainer, Vector2.zero);
		}
		base.OnOpenSucceed += Opened;
		base.OnCloseSucceed += Closed;
		GameSystem<InventorySystem>.Instance().BalanceChanged += OnUpdateBalance;
		GameSystem<InventorySystem>.Instance().UseItemStarted += OnStartUseItem;
		GameSystem<InventorySystem>.Instance().PlayerItemExpired += OnItemExpired;
		Singleton<PlayerController>.Instance().MoveStarted += delegate
		{
			GameSystem<InventorySystem>.Instance().TrackingInventory.Reset();
			SetNormalInventory();
		};
		AddInteractionHandler();
	}

	protected override bool TryClose()
	{
		if (!_inventory.OnClose())
		{
			return false;
		}
		return base.TryClose();
	}

	private void Opened()
	{
		if (_expiredItems.Count != 0)
		{
			string text = _expiredItems[0];
			int count = _expiredItems.Count;
			UIManager.SystemMsg((count <= 1) ? T._("<em>{0}</em>{0:-이} 파괴되었습니다.", text) : T._("<em>{0}</em> 외 <em>{1}</em>개의 아이템이 파괴되었습니다.", text, count - 1), 4f);
			_expiredItems.Clear();
		}
	}

	private void Closed()
	{
		UIManager.Popup.Tooltip<InventoryFilterPopup>().Hide();
		IsShow = false;
	}

	private void AddInteractionHandler()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SetReviveReward, delegate
		{
			OpenDeadMode();
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Feeding, delegate(InteractionObject pet)
		{
			OpenPetFeedingPopup(pet.EntityId);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.OpenPetInven, delegate(InteractionObject obj)
		{
			OpenPetInventory(obj.EntityId);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Inventory, delegate(InteractionObject target)
		{
			Artifact targetComponent6 = target.GetTargetComponent<Artifact>();
			if (!(targetComponent6 == null))
			{
				OpenArtifactInventory(targetComponent6, onlyTakeOut: false);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Take, delegate(InteractionObject target)
		{
			Artifact targetComponent5 = target.GetTargetComponent<Artifact>();
			if (!(targetComponent5 == null))
			{
				Durango.Logic.Item.Inventory playerInven = GameSystem<InventorySystem>.Instance().PlayerInventory;
				if (playerInven.Capacity < playerInven.CurrentSize())
				{
					UIManager.SystemMsg(T._("가방에 공간이 없습니다."));
				}
				else
				{
					GameSystem<InventorySystem>.Instance().SetArtifactInventory(targetComponent5, onlyTakeOut: true);
					PopupItemSelector popup = UIManager.Popup.FindTooltip<PopupItemSelector>();
					PropKey prop = targetComponent5.GetPropKey();
					string artifactName = targetComponent5.GetName();
					Util.ItemListDelegate itemListDelegate = delegate(IList<ItemData> items)
					{
						int capacity = playerInven.Capacity;
						int num = playerInven.CurrentSize();
						int num2 = 0;
						if (items != null)
						{
							num2 += items.Sum((ItemData item) => item.Size);
						}
						string text = ((num2 <= 0) ? $"{num} / {capacity}" : $"<em>{num + num2}</em> / {capacity}");
						popup.Title(artifactName, text);
					};
					popup.TargetInventory().SelectableCount(playerInven.Capacity - playerInven.CurrentSize(), (ItemData item) => item.Size).ConfirmText(T._("줍기"))
						.AutoFillText(T._("모두 선택"))
						.OnChanged(itemListDelegate)
						.OnConfirmed(delegate(IList<ItemData> items)
						{
							if (KUtility.GetSize(items) != 0)
							{
								string[] ids = Util.ItemsToIds(items);
								InventorySystem.TakeOutItems(prop.EntityId, prop.Tile, ids, delegate(bool success)
								{
									if (success)
									{
										IconMoveEffectGroup iconMoveEffectGroup = UIManager.FindScript<IconMoveEffectGroup>();
										if (!(iconMoveEffectGroup == null))
										{
											string[] array = ids;
											foreach (string id in array)
											{
												ItemData itemData = playerInven.Find(id);
												if (itemData != null)
												{
													iconMoveEffectGroup.Register(IconMoveEffectWidget.EffectType.PutIn, itemData.Icon, Vector3.zero, iconMoveEffectGroup.LootingEndPos);
												}
											}
										}
									}
								});
							}
						})
						.AddOnFinished(delegate
						{
							Durango.Logic.Item.Inventory trackingInventory = GameSystem<InventorySystem>.Instance().TrackingInventory;
							if (trackingInventory.OwnerId == prop.EntityId)
							{
								trackingInventory.Reset();
							}
						});
					itemListDelegate(null);
					popup.Show();
					GameSystem<InventorySystem>.Instance().TrackingInventory.Request();
				}
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.PutInLiquidFertilizer, delegate(InteractionObject target)
		{
			Artifact targetComponent4 = target.GetTargetComponent<Artifact>();
			if (!(targetComponent4 == null))
			{
				OpenArtifactInventory(targetComponent4, onlyTakeOut: false);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.BrokenInventory, delegate(InteractionObject target)
		{
			Artifact targetComponent3 = target.GetTargetComponent<Artifact>();
			if (!(targetComponent3 == null))
			{
				OpenArtifactInventory(targetComponent3, onlyTakeOut: true);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Trough, delegate(InteractionObject target)
		{
			Artifact targetComponent2 = target.GetTargetComponent<Artifact>();
			if (!(targetComponent2 == null))
			{
				OpenArtifactInventory(targetComponent2, onlyTakeOut: false);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.UseWarehouse, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			if (!(targetComponent == null))
			{
				OpenWarehouseInventory(targetComponent);
			}
		});
	}

	[ExposedInEditor(null)]
	private void OnUpdateBalance(Currency type, long currency, long delta)
	{
		UIManager.SystemMsg("test");
		if (delta <= 0 || type == Currency.Coin)
		{
			return;
		}
		UIManager.FindScript<IndicatorGroup>().Show(Durango.Logic.Item.Inventory.GetIcon(type), Durango.Logic.Item.Inventory.CurrencyFormat(delta));
		if ((int)type < _currencyParticle.Length)
		{
			CurrencyEffect currencyEffect = _currencyParticle[(int)type];
			if (!string.IsNullOrEmpty(currencyEffect.Particle))
			{
				ParticleManager.Emit(PlayerBehavior.LocalPlayer.gameObject, currencyEffect.Particle, currencyEffect.Bone, follow: false);
			}
		}
	}

	private void OnItemExpired(ItemExpired expired)
	{
		if (expired.ItemNames == null)
		{
			return;
		}
		if (UIManager.IsLoadingCurtain)
		{
			foreach (KeyValuePair<string, string> itemName in expired.ItemNames)
			{
				_expiredItems.Add(itemName.Value);
			}
			return;
		}
		KeyValuePair<string, string> keyValuePair = expired.ItemNames.FirstOrDefault();
		int count = expired.ItemNames.Count;
		UIManager.SystemMsg((count <= 1) ? T._("<em>{0}</em>{0:-이} 파괴되었습니다.", keyValuePair.Value) : T._("<em>{0}</em> 외 <em>{1}</em>개의 아이템이 파괴되었습니다.", keyValuePair.Value, count - 1), 4f);
	}

	private void OnStartUseItem(ItemData item, StartTimer msg)
	{
		float num = msg.Time + msg.AdditionalTime;
		float ratio = msg.Current / num;
		Durango.Logic.Timer.Timer timer = new Durango.Logic.Timer.Timer(msg.EntityId, msg.Subject, num, ratio);
		IconProgressGauge iconProgressGauge = Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer);
		iconProgressGauge.AddIcon(item.Icon);
	}

	public void OpenEatFoodPopup()
	{
		PopupItemSelector popupItemSelector = UIManager.Popup.Tooltip<PopupItemSelector>();
		popupItemSelector.Title(T._("음식 먹기")).Filter((ItemData data) => data.HasTag("eatable") || data.HasTag("drinkable")).OnConfirmed(delegate(ItemData item)
		{
			if (item != null)
			{
				UIManager.MessageBox.ShowLockConfirm(item, delegate
				{
					GameSystem<InventorySystem>.Instance().UseItem(item);
				});
			}
		});
		popupItemSelector.Show();
	}

	public void OpenPetFeedingPopup(string id)
	{
		Pet? pet = Singleton<PetManager>.Instance().GetPet(id);
		if (!pet.HasValue)
		{
			return;
		}
		PetItemInteractionPopup petItemInteractionPopup = UIManager.Popup.Tooltip<PetItemInteractionPopup>();
		petItemInteractionPopup.SetAsFeeding(pet.Value, null, delegate(List<ItemData> items)
		{
			if (KUtility.GetSize(items) != 0)
			{
				UIManager.MessageBox.ShowLockConfirm(items, delegate(string[] itemIds)
				{
					PetManager.FeedPet(id, itemIds, delegate(bool ok)
					{
						if (ok)
						{
							UIManager.SystemMsg(T._("동물이 먹이를 먹었습니다"));
						}
					});
				});
			}
		});
		petItemInteractionPopup.Show();
	}

	private void OpenArtifactInventory(Artifact artifact, bool onlyTakeOut)
	{
		GameSystem<InventorySystem>.Instance().SetArtifactInventory(artifact, onlyTakeOut);
		_inventory.SetInventoryMode(Durango.Logic.Item.Inventory.InventoryMode.Exchange);
		base.Open();
	}

	public override bool Open()
	{
		SetNormalInventory();
		IsShow = true;
		return base.Open();
	}

	private void SetNormalInventory()
	{
		VehicleBase vehicle = PlayerBehavior.LocalPlayer.Driver.Vehicle;
		if (vehicle != null && vehicle is VehiclePet)
		{
			GameSystem<InventorySystem>.Instance().SetReinsInventory(vehicle.EntityId);
		}
		_inventory.SetInventoryMode(Durango.Logic.Item.Inventory.InventoryMode.Normal);
	}

	private void OpenWarehouseInventory(Artifact artifact)
	{
		GameSystem<InventorySystem>.Instance().SetWarehouseInventory(artifact);
		_inventory.SetInventoryMode(Durango.Logic.Item.Inventory.InventoryMode.Exchange);
		base.Open();
	}

	private void OpenPetInventory(string id)
	{
		GameSystem<InventorySystem>.Instance().SetReinsInventory(id);
		_inventory.SetInventoryMode(Durango.Logic.Item.Inventory.InventoryMode.Exchange);
		base.Open();
	}

	private void OpenDeadMode()
	{
		_inventory.SetInventoryMode(Durango.Logic.Item.Inventory.InventoryMode.Dead);
		base.Open();
		UIManager.MessageBox.Show(T._("도움을 요청하며 제시할 보상을 골라주십시오."));
	}

	public void OpenAndSelectItem(string item)
	{
		base.Open();
		_inventory.ItemList.SelectItem(item, sendEvent: true, scrollTo: true);
	}

	public ItemIconWidget FindItem(string id)
	{
		return _inventory.ItemList.FindIcon(id);
	}

	public Transform GetUseButtonTransform()
	{
		return _inventory.Buttons.GetUseButton().transform;
	}
}
