using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using APNGLib;
using Building_;
using InteractionData;
using ItemSystem;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Estate;
using Shared.MessageBoard;
using Shared.System;
using TimerData;
using UnityEngine;

public class BuildGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private GameObject _backSprite;

	[SerializeField]
	private GameObject _mainContainer;

	[SerializeField]
	private RecipeStepSelectWidget _recipeStepSelectWidget;

	[SerializeField]
	private MaterialSelectWidget _materialSelectWidget;

	[SerializeField]
	private BuildExpectResultWidget _buildEstimateResultWidget;

	[SerializeField]
	private DefaultSelectableButton _buttonBuild;

	[SerializeField]
	private SizeSelector _crackInvestmentSelector;

	private BuildSlotContainer _slotContainer;

	private Action _onRequestEstimateResult;

	private Action _onPutMaterials;

	private Action _onBuild;

	private bool _closeReset;

	private void Awake()
	{
		SetOpenCloseSound("Sound/Effect/UI/UI_Menu_Build_Open_01.wav", "Sound/Effect/UI/UI_Menu_Build_Close_01.wav");
		base.OnClose();
	}

	private void Start()
	{
		base.OnOpenSucceed += ItemCraftingGroup_OnOpenSucceed;
		_materialSelectWidget.ItemSelectionUpdated += MaterialSelectWidget_ItemSelectionUpdated;
		_titleWidget.OnClose += Reset;
		UIEventListener.Get(((Component)_buttonBuild).gameObject).onClick = ButtonBuild_OnClick;
		AddInteractionHandler();
	}

	private void OnEnable()
	{
		BuildSystem buildSystem = GameSystem<BuildSystem>.Instance();
		buildSystem.BuildStarted += System_BuildStarted;
		buildSystem.DestructedReplied += System_DestructedReplied;
		buildSystem.ArtifactOccupied += System_ArtifactOccupied;
	}

	private void OnDisable()
	{
		BuildSystem buildSystem = GameSystem<BuildSystem>.Instance();
		buildSystem.BuildStarted -= System_BuildStarted;
		buildSystem.DestructedReplied -= System_DestructedReplied;
		buildSystem.ArtifactOccupied -= System_ArtifactOccupied;
	}

	public void Open(BuildSlotContainer slotContainer, Action onRequestEstimateResult = null, Action onPutMaterials = null, Action onBuild = null, bool closeReset = true)
	{
		SetSlotContainer(slotContainer);
		_onRequestEstimateResult = onRequestEstimateResult;
		_onPutMaterials = onPutMaterials;
		_onBuild = onBuild;
		_closeReset = closeReset;
		Open();
		Refresh();
	}

	public Transform GetSelectableItemTranform()
	{
		ItemIcon2 firstSelectableEnabledItemOrNull = _materialSelectWidget.GetFirstSelectableEnabledItemOrNull();
		return (!((Object)(object)firstSelectableEnabledItemOrNull != (Object)null)) ? null : ((Component)firstSelectableEnabledItemOrNull).transform;
	}

	public Transform GetNextRecipeSlotTransfrom()
	{
		RecipeSlotWidget nextRecipeSlotWidget = _recipeStepSelectWidget.GetNextRecipeSlotWidget();
		return (!((Object)(object)nextRecipeSlotWidget != (Object)null)) ? null : ((Component)nextRecipeSlotWidget).transform;
	}

	public Transform GetBuildButtonTransform()
	{
		return ((Component)_buttonBuild).transform;
	}

	private void SetSlotContainer(BuildSlotContainer slotContainer)
	{
		if (_slotContainer != null)
		{
			_slotContainer.SlotChanged -= SlotContainer_SlotChanged;
			_slotContainer.ExpectedResultUpdated -= System_ExpectedResultUpdated;
			_slotContainer.SlotMaterialUpdated -= OnSlotMaterialUpdated;
		}
		_slotContainer = slotContainer;
		if (_slotContainer != null)
		{
			_slotContainer.SlotChanged += SlotContainer_SlotChanged;
			_slotContainer.ExpectedResultUpdated += System_ExpectedResultUpdated;
			_slotContainer.SlotMaterialUpdated += OnSlotMaterialUpdated;
			_recipeStepSelectWidget.Set(_slotContainer);
			_materialSelectWidget.Set(_slotContainer);
			_buildEstimateResultWidget.Set(_slotContainer);
		}
	}

	private void Refresh()
	{
		_recipeStepSelectWidget.Refresh();
		_materialSelectWidget.Refresh();
		_buildEstimateResultWidget.Refresh();
		RefreshBuildButton();
	}

	private void RefreshBuildButton()
	{
		BuildSlotContainer slotContainer = _slotContainer;
		_buttonBuild.Disable = false;
		switch (slotContainer.State)
		{
		case BuildSlotContainer.BuildState.CanQuickFill:
			_buttonBuild.Text = T._("자동 채우기");
			break;
		case BuildSlotContainer.BuildState.MaterialsNotReady:
			_buttonBuild.Text = T._("재료 넣기");
			_buttonBuild.Disable = true;
			break;
		case BuildSlotContainer.BuildState.ReadyToPutMaterials:
			_buttonBuild.Text = T._("재료 넣기");
			break;
		case BuildSlotContainer.BuildState.ReadyToPutMaterialsAndBuild:
		case BuildSlotContainer.BuildState.ReadyToBuild:
			_buttonBuild.Text = T._("건설");
			_buttonBuild.Disable = !slotContainer.Blueprint.Available;
			break;
		}
	}

	private void AddInteractionHandler()
	{
		BuildSystem system = GameSystem<BuildSystem>.Instance();
		InteractionSystem interactionSystem = GameSystem<InteractionSystem>.Instance();
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.BuildArtifact, OnInteractionBuildArtifact);
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.CompleteArtifact, delegate(InteractionObject target)
		{
			Artifact artifact2 = target.GetTargetComponent<Artifact>();
			if (!((Object)(object)artifact2 == (Object)null))
			{
				EstateInfo? estate = artifact2.ArtifactState.Estate;
				if (estate.HasValue && estate.Value.Type == OwnerType.ClanEstate)
				{
					ClanSystem.GetClanTerritoryCosts(delegate(Costs costs)
					{
						if (costs._Costs != null)
						{
							Dictionary<Currency, int>.Enumerator enumerator = costs._Costs.GetEnumerator();
							if (enumerator.MoveNext())
							{
								KeyValuePair<Currency, int> current = enumerator.Current;
								UIManager.MessageBox.Show(T._("<em>{0}</em> 부족 자금을 사용하여 영토를 선언합니다.", ItemSystem.Inventory.CurrencyFormat(current.Value, current.Key)), delegate(bool ok)
								{
									if (ok)
									{
										BuildSystem.CompleteArtifact(artifact2);
									}
								});
							}
						}
					});
				}
				else
				{
					BuildSystem.CompleteArtifact(artifact2);
				}
			}
		});
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.DestructArtifact, delegate(InteractionObject target)
		{
			system.DestructArtifact(target.GetTargetComponent<Artifact>());
		});
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.RepairArtifact, delegate(InteractionObject target)
		{
			system.RequestRepairCost(target.GetTargetComponent<Artifact>(), InteractionSystem.CurrentMenu.Disabled);
		});
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.SetAsHome, delegate(InteractionObject target)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			BuildSystem.SetAsHome(target.EntityId, new Point2(target.Tile));
		});
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.SetAsBase, delegate(InteractionObject target)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			BuildSystem.SetAsBase(target.EntityId, new Point2(target.Tile));
		});
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.OpenTrap, delegate(InteractionObject target)
		{
			BuildSystem.ArtifactAction("open_trap", target.GetTargetComponent<Artifact>());
		});
		interactionSystem.AddInteractionHandler(InteractionData.Interaction.CraftingItem, OnInteractionCraftingItem);
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.ScribbleText, OnInteractionScribbleText);
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.ScribbleDrawing, OnInteractionScribbleDrawing);
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.Rename, OnInteractionRename);
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.ActivateMarket, OnInteractionActivateMarket);
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.Fire, delegate(InteractionObject target)
		{
			BuildSystem.FireBurnable(target.GetTargetComponent<Artifact>());
		});
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.Extinguish, delegate(InteractionObject target)
		{
			BuildSystem.ExtinguishBurnable(target.GetTargetComponent<Artifact>());
		});
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.Invest, OnInvestToCrack);
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.BuildTutorialBoat, delegate(InteractionObject target)
		{
			Artifact targetComponent2 = target.GetTargetComponent<Artifact>();
			if (!((Object)(object)targetComponent2 == (Object)null))
			{
				TutorialIslandSystem tutorialIslandSystem = GameSystem<TutorialIslandSystem>.Instance();
				tutorialIslandSystem.UpdateTutorialBoatSlots();
				Open(tutorialIslandSystem.BoatSlots, tutorialIslandSystem.RequestEstimateResult, tutorialIslandSystem.PutTutorialBoatMaterials, tutorialIslandSystem.Build, closeReset: false);
			}
		});
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.DepartTutorial, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			if (!((Object)(object)targetComponent == (Object)null))
			{
				GameSystem<TutorialIslandSystem>.Instance().SendDepartTutorial(targetComponent);
			}
		});
		interactionSystem.AddInteractionHandler(InteractionData.Interaction.SkipPostprocess, delegate(InteractionObject o)
		{
			Artifact artifact = o.GetTargetComponent<Artifact>();
			if (!((Object)(object)artifact == (Object)null))
			{
				int cost = GameSystem<BuildSystem>.Instance().GetSkipPostprocessCost(artifact);
				if (cost >= 0)
				{
					GameSystem<InventorySystem>.Instance().PlayerInventory.ShowPayConfirm(cost, Currency.Gem, T._("즉시 완료에는 {0:가} 필요합니다.\n현재 보유량 {1}"), delegate(bool ok)
					{
						if (ok)
						{
							GameSystem<BuildSystem>.Instance().SkipPostprocessCost(artifact, cost);
						}
					});
				}
			}
		});
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.Capsulate, delegate(InteractionObject o)
		{
			GameSystem<BuildSystem>.Instance().CapsulateArtifact(o.GetTargetComponent<Artifact>());
		});
		interactionSystem.AddInteractionHandler(Shared.System.Interaction.DeclareWar, delegate(InteractionObject target)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			interactionSystem.DeclareWar(new Point2(target.Tile));
		});
	}

	private void OnInteractionBuildArtifact(InteractionObject target)
	{
		Artifact targetComponent = target.GetTargetComponent<Artifact>();
		if (!((Object)(object)targetComponent == (Object)null))
		{
			BuildSystem buildSystem = GameSystem<BuildSystem>.Instance();
			buildSystem.InteractionBuildArtifact(targetComponent);
		}
	}

	private void OnInteractionCraftingItem(InteractionObject target)
	{
		Artifact artifact = target.GetTargetComponent<Artifact>();
		if ((Object)(object)artifact == (Object)null)
		{
			return;
		}
		ulong id = InteractionSystem.CurrentMenu.Id;
		UIManager.MessageBox.Show(T._("진행 중인 제작을 취소하시겠습니까? 취소하면 제작 중인 물건은 사라집니다"), delegate(bool ok)
		{
			if (ok)
			{
				Connections.Frontend.Send(new CancelCrafting
				{
					EntityId = artifact.EntityId,
					Tile = artifact.WorldTile,
					CraftingId = id
				});
			}
		});
	}

	private void OnInteractionScribbleText(InteractionObject target)
	{
		Artifact messageBoard = target.GetTargetComponent<Artifact>();
		if ((Object)(object)messageBoard != (Object)null && messageBoard.BuildCompleted)
		{
			TextInputWidget textInput = UIManager.Popup.TextInput;
			textInput.Show(delegate(string message)
			{
				BuildSystem.Scribble(messageBoard, Drawing.Text, Encoding.UTF8.GetBytes(message));
			}, T._("표지판에 남길 글을 적어주세요."), null, isMultiline: true);
		}
	}

	private void OnInteractionScribbleDrawing(InteractionObject target)
	{
		Artifact messageBoard = target.GetTargetComponent<Artifact>();
		if ((Object)(object)messageBoard == (Object)null || !messageBoard.BuildCompleted)
		{
			return;
		}
		DrawPixelGroup drawPixelGroup = UIManager.FindScript<DrawPixelGroup>();
		Scribblable scribblable = messageBoard.Blueprint.Scribblable;
		if (scribblable == null || !scribblable.Canvas)
		{
			return;
		}
		Point2 canvasSize = scribblable.CanvasSize;
		int limitFrame = scribblable.LimitFrame;
		int num = messageBoard.GetTag("color_scribble")?.Level ?? 1;
		drawPixelGroup.Open(canvasSize.x, canvasSize.y, limitFrame, delegate(List<Texture2D> list)
		{
			byte[] array;
			if (list.Count == 1)
			{
				array = list[0].EncodeToPNG();
			}
			else
			{
				APNG aPNG = APNGAssembler.ToPNG(list, 1f);
				Stream stream = aPNG.ToStream();
				array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
			}
			BuildSystem.Scribble(messageBoard, Drawing.Canvas, array);
		}, ColorTable.ReadColorTable($"color_board_L{num:00}.raw"));
	}

	private void OnInvestToCrack(InteractionObject target)
	{
		Artifact artifact = target.GetTargetComponent<Artifact>();
		Crack? crack = artifact.ArtifactState.Crack;
		if (!crack.HasValue)
		{
			return;
		}
		int remainInvestment = crack.Value.RequiredInvestment - crack.Value.CurrentInvestment;
		long balance = GameSystem<InventorySystem>.Instance().PlayerInventory.GetBalance(Currency.TStone);
		int max = (int)Math.Min(Math.Max(balance, crack.Value.InvestmentUnit), remainInvestment);
		((Component)_crackInvestmentSelector).gameObject.SetActive(true);
		_crackInvestmentSelector.Set(crack.Value.InvestmentUnit, crack.Value.InvestmentUnit, crack.Value.InvestmentUnit, max, isTStone: true);
		UIManager.MessageBox.Show(T._("이 곳에 묻을 티스톤의 양을 선택해주세요.\n필요한 티스톤이 전부 모이면 크레이터가 열립니다.\n티스톤을 묻으면 크레이터가 열릴 때 알림을 받을 수 있습니다.\n현재 보유량 {0}", ItemSystem.Inventory.CurrencyFormat(balance, Currency.TStone)), ((Component)_crackInvestmentSelector).GetComponent<UIWidget>(), delegate(bool ok)
		{
			if (ok && _crackInvestmentSelector.Value != 0)
			{
				if (balance < _crackInvestmentSelector.Value)
				{
					UIManager.SystemMsg(T._("가지고 있는 티스톤보다 더 많이 묻을 수 없습니다"));
				}
				else
				{
					Connections.Frontend.Send(new InvestToCrack
					{
						EntityId = artifact.EntityId,
						Tile = artifact.WorldTile,
						Amount = _crackInvestmentSelector.Value
					}).On<OK>(delegate
					{
						UIManager.SystemMsg(T._("{0} 티스톤을 묻었습니다", ItemSystem.Inventory.CurrencyFormat(_crackInvestmentSelector.Value, Currency.TStone)));
						if (remainInvestment <= _crackInvestmentSelector.Value)
						{
							UIManager.SystemMsg(T._("크레이터가 열렸습니다. 잠시 후 자연물이 워프되어 옵니다."));
						}
					});
				}
			}
		});
	}

	private void OnInteractionRename(InteractionObject target)
	{
		Artifact artifact = target.GetTargetComponent<Artifact>();
		if ((Object)(object)artifact != (Object)null && artifact.BuildCompleted)
		{
			TextInputWidget textInput = UIManager.Popup.TextInput;
			textInput.Show(delegate(string message)
			{
				BuildSystem.Rename(artifact, message);
			}, T._("변경할 이름을 적으세요."), null, isMultiline: true);
		}
	}

	private void OnInteractionActivateMarket(InteractionObject target)
	{
		Artifact artifact = target.GetTargetComponent<Artifact>();
		if ((Object)(object)artifact != (Object)null && artifact.BuildCompleted)
		{
			TextInputWidget textInput = UIManager.Popup.TextInput;
			textInput.Show(delegate(string message)
			{
				BuildSystem.ActiavteMarket(artifact, message);
			}, T._("가판대의 이름을 지어주세요."), null, isMultiline: true);
		}
	}

	private void RequestEstimateResult()
	{
		if (_onRequestEstimateResult != null)
		{
			_onRequestEstimateResult();
		}
		else
		{
			GameSystem<BuildSystem>.Instance().RequestEstimateResult();
		}
	}

	private void DoBuildSystemWork(string keyMessageBox, Action work)
	{
		if (_slotContainer.HasLockedItem())
		{
			UIManager.MessageBox.Show(LocalizeSystem.Get(keyMessageBox), delegate(bool ok)
			{
				if (ok)
				{
					work();
				}
			});
		}
		else
		{
			work();
		}
	}

	private void PutMaterials()
	{
		if (_onPutMaterials != null)
		{
			_onPutMaterials();
		}
		else
		{
			GameSystem<BuildSystem>.Instance().PutMaterials();
		}
	}

	private void Build()
	{
		if (_onBuild != null)
		{
			_onBuild();
		}
		else
		{
			GameSystem<BuildSystem>.Instance().Build();
		}
	}

	protected override bool OnOpen()
	{
		KSingleton<PlayerController>.Instance().WaterFlowRegisterSet.Add("Build");
		KSingleton<PlayerController>.Instance().MoveStarted += BuildGroup_MoveStarted;
		return base.OnOpen();
	}

	protected override bool OnClose()
	{
		KSingleton<PlayerController>.Instance().WaterFlowRegisterSet.Remove("Build");
		KSingleton<PlayerController>.Instance().MoveStarted -= BuildGroup_MoveStarted;
		SetSlotContainer(null);
		_onRequestEstimateResult = null;
		_onPutMaterials = null;
		_onBuild = null;
		return base.OnClose();
	}

	private void MaterialSelectWidget_ItemSelectionUpdated()
	{
		BuildSlotContainer slotContainer = _slotContainer;
		RequestEstimateResult();
		if (slotContainer.CurrentSlot != null)
		{
			_recipeStepSelectWidget.RefreshSlot(slotContainer.CurrentSlot.Index);
		}
		_recipeStepSelectWidget.RefreshProgressPercentage();
		RefreshBuildButton();
	}

	private void ItemCraftingGroup_OnOpenSucceed()
	{
		_materialSelectWidget.RepositionItemList();
	}

	private void ButtonBuild_OnClick(GameObject go)
	{
		BuildSlotContainer slotContainer = _slotContainer;
		switch (slotContainer.State)
		{
		case BuildSlotContainer.BuildState.CanQuickFill:
			slotContainer.QuickFill();
			RequestEstimateResult();
			Refresh();
			break;
		case BuildSlotContainer.BuildState.MaterialsNotReady:
			UIManager.SystemMsg(T._("재료가 선택되지 않았습니다"));
			break;
		case BuildSlotContainer.BuildState.ReadyToPutMaterials:
			DoBuildSystemWork("#build_put_warning_include_locked_item", delegate
			{
				PutMaterials();
				UIBase.CloseAllUI();
			});
			break;
		case BuildSlotContainer.BuildState.ReadyToPutMaterialsAndBuild:
			DoBuildSystemWork("#build_warning_include_locked_item", delegate
			{
				PutMaterials();
				Build();
				UIBase.CloseAllUI();
			});
			break;
		case BuildSlotContainer.BuildState.ReadyToBuild:
			DoBuildSystemWork("#build_warning_include_locked_item", delegate
			{
				Build();
				UIBase.CloseAllUI();
			});
			break;
		}
	}

	private void SlotContainer_SlotChanged(int previousIndex)
	{
		BuildSlotContainer slotContainer = _slotContainer;
		_recipeStepSelectWidget.RefreshSlot(previousIndex);
		if (slotContainer.CurrentSlot != null && slotContainer.CurrentSlot.Index != previousIndex)
		{
			_recipeStepSelectWidget.RefreshSlot(slotContainer.CurrentSlot.Index);
		}
		_materialSelectWidget.Refresh();
	}

	private void System_BuildStarted(float duration)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		BuildSlotContainer slotContainer = GameSystem<BuildSystem>.Instance().SlotContainer;
		if (!((Object)(object)slotContainer.Artifact == (Object)null))
		{
			Blueprint blueprint = slotContainer.Blueprint;
			IconProgressGauge iconProgressGauge = TimerData.Timer.Play<IconProgressGauge>(new TimerData.Timer("Building", duration));
			iconProgressGauge.SetTarget(((Component)slotContainer.Artifact).gameObject, Vector3.up * 200f);
			iconProgressGauge.SetIcon(blueprint.Icon);
			KSingleton<PlayerController>.Instance().RotateToPosition(slotContainer.Artifact.InteractionPosition);
			MotionMap.Instance().GetBuildMotion(slotContainer.Blueprint.Id, null, out var motion, out var equip);
			KSingleton<PlayerController>.Instance().Motion(equip: equip, motionState: motion, time: iconProgressGauge.RemainTime());
			DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.Construct, blueprint.LocalizedName);
		}
	}

	private void System_ExpectedResultUpdated(SlotContainer slotContainer)
	{
		_buildEstimateResultWidget.Refresh();
	}

	private void System_DestructedReplied(Artifact artifact, Destructing msg)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		TimerData.Timer timer = new TimerData.Timer("destruct", msg.Duration);
		IconProgressGauge iconProgressGauge = TimerData.Timer.Play<IconProgressGauge>(timer);
		string icon = IconMap.Get(Shared.System.Interaction.DestructArtifact);
		Color color = InteractionMenuData.InteractionMenuColor(Shared.System.Interaction.DestructArtifact);
		iconProgressGauge.SetIcon(icon, color);
		byte toolType = msg.ToolType;
		KSingleton<PlayerController>.Instance().Motion(toolType switch
		{
			1 => "Onehand_Destroy", 
			2 => "Twohand_Destroy", 
			_ => "Barehand_Destroy", 
		}, iconProgressGauge.RemainTime());
		DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.Destruct, artifact.LocalizedName);
	}

	private void OnSlotMaterialUpdated()
	{
		if (base.IsOpen)
		{
			Refresh();
		}
	}

	private void System_ArtifactOccupied()
	{
		Open(GameSystem<BuildSystem>.Instance().SlotContainer);
	}

	private void BuildGroup_MoveStarted()
	{
		Reset();
	}

	private void Reset()
	{
		if (_closeReset && _slotContainer != null)
		{
			_slotContainer.Dispose();
		}
		Close();
	}
}
