using System;
using System.Collections.Generic;
using Building;
using Crafting;
using Durango.Logic.Item;
using Durango.Logic.Timer;
using Durango.MotionInfo;
using Durango.Render.Camera;
using Durango.Render.Particle;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Item;
using UnityEngine;

namespace Durango.UI;

public class CraftGroupBase : UIBase, IScreenResizeReceiver
{
	public enum Mode
	{
		Invalid,
		Craft,
		Build,
		Remodeling,
		TechSupport
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	protected RecipeStepSelectWidget _recipeStepSelectVerticalWidget;

	[SerializeField]
	private MaterialSelectWidget _materialSelectWidget;

	[SerializeField]
	private ExpectResultWidget _estimateResultWidget;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private UILabel _craftWarning;

	[SerializeField]
	private ParticleType _entrustedCraftParticle;

	[SerializeField]
	private SoundEventType _entrustedCraftSound;

	[SerializeField]
	private SoundEventType _craftSucceedSound;

	private SlotContainer _slotContainer;

	private Mode _currentMode;

	private Action _onRequestEstimateResult;

	private Action _onPutMaterials;

	private Action _onBuild;

	protected virtual RecipeStepSelectWidget RecipeStepSelectWidget => _recipeStepSelectVerticalWidget;

	private void Awake()
	{
		_openCloseSound = UISound.GroupType.Craft;
		SetChildrenActive(activated: false);
	}

	private void Start()
	{
		base.OnOpenSucceed += delegate
		{
			_materialSelectWidget.ResetpositionItemList();
		};
		_materialSelectWidget.ItemSelectionUpdated += MaterialSelectWidget_ItemSelectionUpdated;
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(OnConfirmButtonClick));
		GameSystem<BuildSystem>.Instance().ArtifactOccupied += System_ArtifactOccupied;
		GameSystem<CraftSystem>.Instance().EntrustedCraftStarted += OnStartEntrustedCraft;
		GameSystem<CraftSystem>.Instance().CraftSucceed += OnSuccessCraft;
		GameSystem<CraftSystem>.Instance().TechSupportSucceed += OnSuccessTechSupport;
		GameSystem<CraftSystem>.Instance().CraftFailed += OnFailCraft;
		GameSystem<CraftSystem>.Instance().CraftingTimer.Started += OnStartCraftTimer;
	}

	public void Open(BuildSlotContainer slotContainer, Action onRequestEstimateResult = null, Action onPutMaterials = null, Action onBuild = null)
	{
		SetSlotContainer(slotContainer);
		_onRequestEstimateResult = onRequestEstimateResult;
		_onPutMaterials = onPutMaterials;
		_onBuild = onBuild;
		Open();
		Refresh();
		RequestEstimateResult(firstTime: true);
	}

	public bool Open(Recipe recipe, Artifact workbench, bool quickFill, TechSupportTarget? techSupportTarget = null)
	{
		if (recipe.HasRequiredWorkbench && workbench == null)
		{
			return false;
		}
		CraftSlotContainer slotContainer = GameSystem<CraftSystem>.Instance().SlotContainer;
		slotContainer.Set(recipe, workbench, GameSystem<InventorySystem>.Instance().PlayerInventory, techSupportTarget);
		if (quickFill)
		{
			slotContainer.QuickFill();
		}
		SetSlotContainer(slotContainer, techSupportTarget.HasValue);
		_onRequestEstimateResult = null;
		_onPutMaterials = null;
		_onBuild = null;
		Open();
		Refresh();
		RequestEstimateResult(firstTime: true);
		return true;
	}

	public Transform GetSelectableItemTranform()
	{
		ItemIconWidget firstSelectableEnabledItemOrNull = _materialSelectWidget.GetFirstSelectableEnabledItemOrNull();
		if (firstSelectableEnabledItemOrNull != null)
		{
			return firstSelectableEnabledItemOrNull.transform;
		}
		return null;
	}

	public Transform GetNextRecipeSlotTransfrom()
	{
		RecipeSlotWidget nextRecipeSlotWidget = RecipeStepSelectWidget.GetNextRecipeSlotWidget();
		if (nextRecipeSlotWidget != null)
		{
			return nextRecipeSlotWidget.transform;
		}
		return null;
	}

	public Transform GetButtonTransform()
	{
		return _confirmButton.transform;
	}

	public SlotContainer GetSlotContainer()
	{
		return _slotContainer;
	}

	private void SetSlotContainer(SlotContainer slotContainer, bool byTechSupport = false)
	{
		if (_slotContainer != null)
		{
			_slotContainer.SlotChanged -= SlotContainer_SlotChanged;
			_slotContainer.SlotMaterialUpdated -= OnSlotMaterialUpdated;
			_slotContainer.QuantityChanged -= OnRecipeQuantityChanged;
		}
		_slotContainer = slotContainer;
		if (_slotContainer == null)
		{
			_titleWidget.Object.SetTitle(string.Empty);
			_currentMode = Mode.Invalid;
			return;
		}
		_slotContainer.SlotChanged += SlotContainer_SlotChanged;
		_slotContainer.SlotMaterialUpdated += OnSlotMaterialUpdated;
		_slotContainer.QuantityChanged += OnRecipeQuantityChanged;
		RecipeStepSelectWidget.Set(_slotContainer);
		_materialSelectWidget.Set(_slotContainer);
		if (_slotContainer is BuildSlotContainer)
		{
			BuildSlotContainer buildSlotContainer = _slotContainer as BuildSlotContainer;
			_estimateResultWidget.Set(buildSlotContainer);
			Blueprint blueprint = buildSlotContainer.Blueprint;
			if (blueprint.IsRemodeling)
			{
				_titleWidget.Object.SetTitle(blueprint.Name);
				_currentMode = Mode.Remodeling;
			}
			else
			{
				_titleWidget.Object.SetTitle(T._("건설"));
				_currentMode = Mode.Build;
			}
		}
		else if (_slotContainer is CraftSlotContainer)
		{
			_estimateResultWidget.Set(_slotContainer as CraftSlotContainer);
			if (byTechSupport)
			{
				_titleWidget.Object.SetTitle(T._("기술 지원 요청"));
				_currentMode = Mode.TechSupport;
			}
			else
			{
				_titleWidget.Object.SetTitle(T._("제작"));
				_currentMode = Mode.Craft;
			}
		}
		else
		{
			_currentMode = Mode.Invalid;
		}
	}

	private void Refresh()
	{
		RecipeStepSelectWidget.Refresh();
		_materialSelectWidget.Refresh();
		_estimateResultWidget.Refresh();
		RefreshButton();
		RequestEstimateResult();
	}

	private void RefreshButton()
	{
		switch (_currentMode)
		{
		case Mode.Craft:
			RefreshCraftOrTechSupportButton(byTechSupport: false);
			break;
		case Mode.Build:
			RefreshBuildButton();
			break;
		case Mode.Remodeling:
			RefreshRemodelingButton();
			break;
		case Mode.TechSupport:
			RefreshCraftOrTechSupportButton(byTechSupport: true);
			break;
		}
	}

	private void RefreshBuildButton()
	{
		if (!(_slotContainer is BuildSlotContainer buildSlotContainer))
		{
			return;
		}
		PresetButton.Effect effect = PresetButton.Effect.None;
		bool disabled = false;
		UISound.ClickType clickSound = UISound.ClickType.ButtonHighlight;
		switch (buildSlotContainer.State)
		{
		case BuildSlotContainer.BuildState.CanQuickFill:
			_confirmButton.Text = T._("자동 채우기");
			clickSound = UISound.ClickType.AutoFill;
			break;
		case BuildSlotContainer.BuildState.ToolNotReady:
		case BuildSlotContainer.BuildState.MaterialsNotReady:
			_confirmButton.Text = T._("재료 넣기");
			disabled = true;
			break;
		case BuildSlotContainer.BuildState.ReadyToPutMaterials:
			_confirmButton.Text = T._("재료 넣기");
			break;
		case BuildSlotContainer.BuildState.ReadyToPutMaterialsAndBuild:
		case BuildSlotContainer.BuildState.ReadyToBuild:
			_confirmButton.Text = T._("건설");
			if (buildSlotContainer.CanBuild())
			{
				effect = PresetButton.Effect.Emphasis;
			}
			else
			{
				disabled = true;
			}
			break;
		}
		_confirmButton.Disabled = disabled;
		_confirmButton.SetClickSound(clickSound);
		_confirmButton.SetEffect(effect);
	}

	private void RefreshRemodelingButton()
	{
		if (!(_slotContainer is BuildSlotContainer buildSlotContainer))
		{
			return;
		}
		bool flag = true;
		PresetButton.Effect effect = PresetButton.Effect.None;
		if (buildSlotContainer.State == BuildSlotContainer.BuildState.CanQuickFill)
		{
			_confirmButton.Text = T._("자동 채우기");
			_confirmButton.SetClickSound(UISound.ClickType.AutoFill);
		}
		else
		{
			_confirmButton.Text = T._("개조");
			_confirmButton.SetClickSound(UISound.ClickType.ButtonHighlight);
			if (buildSlotContainer.State == BuildSlotContainer.BuildState.ReadyToPutMaterialsAndBuild)
			{
				effect = PresetButton.Effect.Emphasis;
			}
			else
			{
				flag = false;
			}
		}
		_confirmButton.Disabled = !flag;
		_confirmButton.SetEffect(effect);
	}

	private void RefreshCraftOrTechSupportButton(bool byTechSupport)
	{
		if (!(_slotContainer is CraftSlotContainer craftSlotContainer))
		{
			return;
		}
		bool flag = true;
		PresetButton.Effect effect = PresetButton.Effect.None;
		if (craftSlotContainer.State == CraftSlotContainer.CraftState.CanQuickFill)
		{
			_confirmButton.Text = T._("자동 채우기");
			_confirmButton.SetClickSound(UISound.ClickType.AutoFill);
		}
		else
		{
			_confirmButton.Text = ((!byTechSupport) ? T._("제작") : T._("기술 지원 받기"));
			_confirmButton.SetClickSound(UISound.ClickType.ButtonHighlight);
			if (craftSlotContainer.State == CraftSlotContainer.CraftState.ReadyToCraft)
			{
				effect = PresetButton.Effect.Emphasis;
			}
			else
			{
				flag = false;
			}
		}
		_confirmButton.Disabled = !flag;
		_confirmButton.SetEffect(effect);
	}

	private void RequestEstimateResult(bool firstTime = false)
	{
		_estimateResultWidget.ClearEstimation();
		if (_onRequestEstimateResult != null)
		{
			_onRequestEstimateResult();
			return;
		}
		switch (_currentMode)
		{
		case Mode.Craft:
		{
			_estimateResultWidget.SetIconMode(expectPreviewTexture: false);
			CraftSlotContainer craftSlotContainer2 = (CraftSlotContainer)_slotContainer;
			if (craftSlotContainer2.ReadyToRequestEstimation())
			{
				craftSlotContainer2.GetEstimation(_estimateResultWidget.SetCraftEstimation);
			}
			break;
		}
		case Mode.Build:
		{
			_estimateResultWidget.SetIconMode(expectPreviewTexture: true, isBig: true);
			BuildSlotContainer buildSlotContainer = (BuildSlotContainer)_slotContainer;
			if (buildSlotContainer.ReadyToRequestEstimation())
			{
				buildSlotContainer.GetEstimation(delegate(BuildEstimation? estimation)
				{
					_estimateResultWidget.SetBuildEstimation(estimation);
				});
			}
			break;
		}
		case Mode.Remodeling:
		{
			_estimateResultWidget.SetIconMode(expectPreviewTexture: true, isBig: true);
			BuildSlotContainer buildSlot = (BuildSlotContainer)_slotContainer;
			if (!buildSlot.ReadyToRequestEstimation())
			{
				break;
			}
			buildSlot.GetRemodelingEstimation(delegate(BuildEstimation? estimation)
			{
				ArtifactPreview? artifactPreview = null;
				if (estimation.HasValue)
				{
					artifactPreview = estimation.Value.ArtifactPreview;
				}
				_estimateResultWidget.SetRemodelingEstimation(buildSlot.Artifact, artifactPreview);
			});
			break;
		}
		case Mode.TechSupport:
			if (firstTime)
			{
				CraftSlotContainer craftSlotContainer = (CraftSlotContainer)_slotContainer;
				_estimateResultWidget.SetIconMode(expectPreviewTexture: false);
				_estimateResultWidget.SetTechSupportEstimation(craftSlotContainer.TechSupportBaseSlotInfo);
			}
			break;
		}
	}

	private void DoBuildSystemWork(string messages, Action work)
	{
		ItemData safestItem = _slotContainer.GetSafestItem();
		if (safestItem == null || safestItem.SafeLevel == SafeLevel.None)
		{
			work();
			return;
		}
		string text = ((safestItem.SafeLevel != SafeLevel.Locked) ? T._("<em>임무</em> 수행에 필요한 <em>{0}</em>{0:-이} 포함되어 있습니다.", safestItem.Name) : T._("<em>잠금</em> 설정된 <em>{0}</em>{0:-이} 포함되어 있습니다.", safestItem.Name));
		UIManager.MessageBox.Show(text + " " + messages, delegate(bool ok)
		{
			if (ok)
			{
				work();
			}
		});
	}

	private void PutMaterials()
	{
		if (_onPutMaterials != null)
		{
			_onPutMaterials();
			return;
		}
		GameSystem<BuildSystem>.Instance().PutMaterials(delegate
		{
			IconMoveEffectGroup iconMoveEffectGroup = UIManager.FindScript<IconMoveEffectGroup>();
			if (!(iconMoveEffectGroup == null))
			{
				BuildSlotContainer slotContainer = GameSystem<BuildSystem>.Instance().SlotContainer;
				Vector3 pos = ((!(slotContainer.Artifact == null)) ? MainCamera.WorldToNGUIPos(slotContainer.Artifact.InteractionPosition) : MainCamera.WorldToNGUIPos(PlayerBehavior.LocalPlayer.CurrentPosition));
				iconMoveEffectGroup.RegisterSlotContainer(slotContainer, pos);
			}
		});
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

	protected override bool TryOpen()
	{
		Singleton<PlayerController>.Instance().WaterFlowRegisterSet.Add("Build");
		if (_currentMode == Mode.Craft)
		{
			_craftWarning.text = T._("<weak>제작 실패시 재료가 없어집니다.</weak>");
		}
		else
		{
			_craftWarning.text = T._("<em><help>{0}</help> 원하는 외형</em>이 나오려면?", T._("comment='재료의 절반을 초과하는만큼 넣어야 원하는 외형이 나옵니다.\n<weak>ex) 2x2 사이즈의 제브라케라톱스 지붕 벽집을 만들고싶다면,\n총 지붕재 4개 중 3개가 제브라케라톱스 지붕재여야합니다.</weak>',width=0, resize_collider=1"));
		}
		return base.TryOpen();
	}

	protected override bool TryClose()
	{
		if (Singleton<PlayerController>.HasInstance())
		{
			Singleton<PlayerController>.Instance().WaterFlowRegisterSet.Remove("Build");
		}
		return base.TryClose();
	}

	private void MaterialSelectWidget_ItemSelectionUpdated()
	{
		if (_slotContainer != null)
		{
			SlotContainer slotContainer = _slotContainer;
			RequestEstimateResult();
			if (slotContainer.CurrentSlot != null)
			{
				RecipeStepSelectWidget.RefreshSlot(slotContainer.CurrentSlot.Index);
			}
			RecipeStepSelectWidget.RefreshProgressPercentage();
			RefreshButton();
		}
	}

	private void OnConfirmButtonClick()
	{
		switch (_currentMode)
		{
		case Mode.Craft:
			ButtonCraftOrTechSupport_OnClick(Craft);
			break;
		case Mode.Build:
			ButtonBuild_OnClick();
			break;
		case Mode.Remodeling:
			ButtonRemodeling_OnClick();
			break;
		case Mode.TechSupport:
			ButtonCraftOrTechSupport_OnClick(TechSupport);
			break;
		}
	}

	private void ButtonBuild_OnClick()
	{
		BuildSlotContainer buildSlotContainer = _slotContainer as BuildSlotContainer;
		switch (buildSlotContainer.State)
		{
		case BuildSlotContainer.BuildState.CanQuickFill:
			buildSlotContainer.QuickFill();
			Refresh();
			break;
		case BuildSlotContainer.BuildState.MaterialsNotReady:
			UIManager.SystemMsg(T._("재료가 선택되지 않았습니다."));
			break;
		case BuildSlotContainer.BuildState.ReadyToPutMaterials:
			DoBuildSystemWork(T._("재료를 넣으시겠습니까?"), delegate
			{
				UIBase.CloseAllUI();
				PutMaterials();
				UIManager.SystemMsg(T._("재료의 일부를 건설 부지에 넣었습니다."));
			});
			break;
		case BuildSlotContainer.BuildState.ReadyToPutMaterialsAndBuild:
			DoBuildSystemWork(T._("정말로 건설하시겠습니까?"), delegate
			{
				UIBase.CloseAllUI();
				PutMaterials();
				Build();
			});
			break;
		case BuildSlotContainer.BuildState.ReadyToBuild:
			DoBuildSystemWork(T._("정말로 건설하시겠습니까?"), delegate
			{
				UIBase.CloseAllUI();
				Build();
			});
			break;
		case BuildSlotContainer.BuildState.ToolNotReady:
			break;
		}
	}

	private void ButtonRemodeling_OnClick()
	{
		BuildSlotContainer buildSlotContainer = _slotContainer as BuildSlotContainer;
		switch (buildSlotContainer.State)
		{
		case BuildSlotContainer.BuildState.CanQuickFill:
			buildSlotContainer.QuickFill();
			Refresh();
			break;
		case BuildSlotContainer.BuildState.ReadyToPutMaterialsAndBuild:
			DoBuildSystemWork(T._("정말로 개조하시겠습니까?"), delegate
			{
				UIBase.CloseAllUI();
				GameSystem<BuildSystem>.Instance().Remodeling();
			});
			break;
		}
	}

	private void ButtonCraftOrTechSupport_OnClick(Action action)
	{
		if (!GameSystem<CraftSystem>.Instance().CraftingTimer.Timer.IsStop)
		{
			return;
		}
		CraftSlotContainer slotContainer = GameSystem<CraftSystem>.Instance().SlotContainer;
		switch (slotContainer.State)
		{
		case CraftSlotContainer.CraftState.CanQuickFill:
			slotContainer.QuickFill();
			Refresh();
			break;
		case CraftSlotContainer.CraftState.ToolNotReady:
			UIManager.SystemMsg(T._("도구가 선택되지 않았습니다."));
			break;
		case CraftSlotContainer.CraftState.MaterialsNotReady:
			UIManager.SystemMsg(T._("재료가 선택되지 않았습니다."));
			break;
		case CraftSlotContainer.CraftState.ReadyToCraft:
		{
			ItemData safestItem = slotContainer.GetSafestItem();
			if (safestItem == null || safestItem.SafeLevel == SafeLevel.None)
			{
				CraftOrTechSupport(action);
				break;
			}
			string mainText = ((safestItem.SafeLevel != SafeLevel.Locked) ? T._("<em>임무</em> 수행에 필요한 <em>{0}</em>{0:-이} 포함되어 있습니다. 정말로 제작하시겠습니까?", safestItem.Name) : T._("<em>잠금</em> 설정된 <em>{0}</em>{0:-이} 포함되어 있습니다. 정말로 제작하시겠습니까?", safestItem.Name));
			UIManager.MessageBox.Show(mainText, delegate(bool ok)
			{
				if (ok)
				{
					CraftOrTechSupport(action);
				}
			});
			break;
		}
		}
	}

	private void CraftOrTechSupport(Action action)
	{
		if (PlayerBehavior.LocalPlayer.IsRiding)
		{
			UIBase.CloseAllUI();
			VehicleBase.RequestUnmountIfRiding(immediately: true, action);
		}
		else
		{
			action();
		}
	}

	private void Craft()
	{
		UIBase.CloseAllUI();
		GameSystem<CraftSystem>.Instance().Craft();
	}

	private void TechSupport()
	{
		UIBase.CloseAllUI();
		GameSystem<CraftSystem>.Instance().TechSupport();
	}

	private void SlotContainer_SlotChanged(int previousIndex)
	{
		SlotContainer slotContainer = _slotContainer;
		RecipeStepSelectWidget.RefreshSlot(previousIndex);
		if (slotContainer.CurrentSlot != null && slotContainer.CurrentSlot.Index != previousIndex)
		{
			RecipeStepSelectWidget.RefreshSlot(slotContainer.CurrentSlot.Index);
		}
		_materialSelectWidget.Refresh();
		_materialSelectWidget.ResetpositionItemList();
	}

	private void OnSlotMaterialUpdated()
	{
		if (base.IsOpened)
		{
			Refresh();
		}
	}

	private void OnRecipeQuantityChanged()
	{
		if (base.IsOpened)
		{
			Refresh();
		}
	}

	private void System_ArtifactOccupied()
	{
		Open(GameSystem<BuildSystem>.Instance().SlotContainer);
	}

	private void OnStartEntrustedCraft()
	{
		UIManager.SystemMsg(T._("제작을 시작했습니다."));
		CraftSystem.CraftQueueItem lastCraftData = GameSystem<CraftSystem>.Instance().LastCraftData;
		if (!(lastCraftData.Workbench == null))
		{
			ParticleManager.Emit(_entrustedCraftParticle, lastCraftData.Workbench.Center, Quaternion.identity);
			SoundManager.PlayEvent(_entrustedCraftSound);
		}
	}

	private void OnSuccessCraft(string recipeId, Crafted crafted)
	{
		if (crafted.Result == Result.GreatSuccess)
		{
			PlayerController.MotionUpdater.Motion("Craft_Success");
		}
		SoundManager.PlayEvent(_craftSucceedSound);
		if (crafted.Items == null || crafted.Items.Length == 0)
		{
			return;
		}
		using Reusable<HashSet<string>> reusable = ReusableHashSet<string>.Pop();
		Item[] items = crafted.Items;
		for (int i = 0; i < items.Length; i++)
		{
			Item item = items[i];
			reusable.Value.Add(item.Prototype);
		}
		foreach (string item3 in reusable.Value)
		{
			int num = 0;
			Item? item2 = null;
			Item[] items2 = crafted.Items;
			for (int j = 0; j < items2.Length; j++)
			{
				Item value = items2[j];
				if (!(value.Prototype != item3))
				{
					if (!item2.HasValue)
					{
						item2 = value;
					}
					num++;
				}
			}
			if (item2.HasValue)
			{
				Item value2 = item2.Value;
				string main = ((num <= 1) ? T._("{1:lv:} <em>{0}</em> 제작 성공", value2.Name, value2.Level) : T._("{1:lv:} <em>{0}</em> 제작 성공 <em>x{2}개</em>", value2.Name, value2.Level, num));
				string sub = Util.ItemQualityString(value2);
				UIManager.Alarm.RewardAlarm(new AlarmRewardQueue.Args
				{
					Main = main,
					Sub = sub,
					Icon = new ItemIcon(value2)
				}, AlarmGroup.RewardEffectType.Craft);
			}
		}
	}

	private static void OnFailCraft(string recipeId, ActionInfo actionInfo)
	{
		PlayerController.MotionUpdater.Motion("Craft_Fail");
		Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(recipeId);
		string main = T._("<em>{0}</em> 제작 실패", recipe.Name);
		string sub = Util.ActionInfoDetailString(actionInfo, craft: true);
		UIManager.Alarm.RewardAlarm(new AlarmRewardQueue.Args
		{
			Main = main,
			Sub = sub,
			Icon = recipe.Icon
		}, AlarmGroup.RewardEffectType.CraftFail);
	}

	private static void OnStartCraftTimer(PredictTimer timer)
	{
		CraftSystem.CraftQueueItem lastCraftData = GameSystem<CraftSystem>.Instance().LastCraftData;
		Recipe recipe = lastCraftData.Recipe;
		ItemWindowProgressGauge itemWindowProgressGauge = Durango.Logic.Timer.Timer.Play<ItemWindowProgressGauge>(timer.Timer);
		itemWindowProgressGauge.ClearItems();
		foreach (KeyValuePair<string, ItemData[]> material in lastCraftData.Materials)
		{
			itemWindowProgressGauge.AddItems(material.Value);
		}
		if (lastCraftData.Tool != null)
		{
			itemWindowProgressGauge.AddItem(lastCraftData.Tool);
		}
		itemWindowProgressGauge.Set(recipe.Name);
		if (lastCraftData.Workbench != null)
		{
			Singleton<PlayerController>.Instance().RotateToPosition(lastCraftData.Workbench.InteractionPosition, snap: true);
		}
		ItemColor color = default(ItemColor);
		MotionMap.Instance().GetCraftMotion(lastCraftData.Recipe.Id, (!(lastCraftData.Workbench != null)) ? string.Empty : lastCraftData.Workbench.BlueprintId, lastCraftData.CreateMaterialsTags(), out var motion, out var equip);
		if (equip == null)
		{
			ItemData tool = lastCraftData.Tool;
			if (tool != null)
			{
				equip = tool.GetModel(PlayerBehavior.LocalPlayer.IsMale);
				color = tool.Colors;
			}
		}
		timer.SetMotion(motion, equip, color);
		Vector3 pos = MainCamera.WorldToNGUIPos(PlayerBehavior.LocalPlayer.CurrentPosition);
		UIManager.FindScript<IconMoveEffectGroup>().RegisterSlotContainer(lastCraftData, pos);
	}

	private void OnSuccessTechSupport(ItemData item)
	{
		string main = T._("{0:lv:} {1}", item.Level, item.Name);
		string sub = T._("기술지원");
		UIManager.Alarm.RewardAlarm(new AlarmRewardQueue.Args
		{
			Main = main,
			Sub = sub,
			Icon = item.Icon
		}, AlarmGroup.RewardEffectType.Craft);
	}

	public virtual void OnChangeScreenSize()
	{
	}
}
