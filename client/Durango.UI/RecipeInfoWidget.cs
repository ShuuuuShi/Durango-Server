using System;
using System.Collections.Generic;
using Building;
using Crafting;
using Durango.Logic.Item;
using Durango.Logic.Skill;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Shared.Ability;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class RecipeInfoWidget : MonoBehaviour, IUIInitializable
{
	public struct SlotStruct
	{
		public string Name;

		public int Count;

		public int RequiredCount;

		public int RequiredLevel;

		public OrTagFilter Tags;

		public OrTagFilter Materials;

		public SlotSourceInfo[] SourceInfos;
	}

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private Selectable _likeButton;

	[SerializeField]
	private GameObject _likeIcon;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private UIWidget _descriptionWidget;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UIWidget _timeWidget;

	[SerializeField]
	private UILabel _timeKeyLabel;

	[SerializeField]
	private UILabel _timeValueLabel;

	[SerializeField]
	private UIWidget _conditionsWidget;

	[SerializeField]
	private ListObjectPool _conditionsItems;

	[SerializeField]
	private UIWidget _sizeSelectorWidget;

	[SerializeField]
	private IntSelector _xSelector;

	[SerializeField]
	private IntSelector _ySelector;

	[SerializeField]
	private UIWidget _materialWidget;

	[SerializeField]
	private ListObjectPool _materialItemWidgets;

	[SerializeField]
	private UIWidget _buttonWidget;

	[SerializeField]
	private SelectableButton _nextButton;

	private string _id;

	private AnimationWidget _animWidget;

	private readonly List<SlotStruct> _recipeSlotInfos = new List<SlotStruct>();

	private readonly List<int> _recipeSlotCount = new List<int>();

	private int _descriptionPadding;

	private bool _likeIconAnimation;

	private BlurTexture _blurTexture;

	private AnimationWidget AnimWidget
	{
		get
		{
			if (_animWidget == null)
			{
				_animWidget = GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	public Point2 Size => new Point2(_xSelector.Value, _ySelector.Value);

	public event Action<int, int> BuildSizeChanged;

	public event Action Confirmed;

	public event Action LikeButtonClicked;

	public bool Show(RecipeSystem.RecipeType recipeType, string id, bool reset)
	{
		base.gameObject.SetActive(value: true);
		AnimWidget.Alpha = 1f;
		if (_blurTexture != null)
		{
			_blurTexture.Show(show: true);
		}
		switch (recipeType)
		{
		case RecipeSystem.RecipeType.Crafting:
		{
			Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(id);
			if (recipe != null)
			{
				ShowRecipeDetailInfo(recipe, reset);
				return true;
			}
			break;
		}
		case RecipeSystem.RecipeType.Building:
		{
			Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(id);
			if (blueprint != null)
			{
				ShowBlueprintDetailInfo(blueprint, reset);
				return true;
			}
			break;
		}
		}
		return false;
	}

	public void Hide()
	{
		AnimWidget.Alpha = 0f;
		if (_blurTexture != null)
		{
			_blurTexture.Show(show: false);
		}
	}

	public Transform GetButtonTransform()
	{
		return _buttonWidget.transform;
	}

	void IUIInitializable.Init()
	{
		_descriptionPadding = _descriptionWidget.height - _descriptionLabel.height;
		SelectableButton nextButton = _nextButton;
		nextButton.Clicked = (Action)Delegate.Combine(nextButton.Clicked, new Action(OnConfirm));
		IntSelector xSelector = _xSelector;
		xSelector.ValueChanged = (Action)Delegate.Combine(xSelector.ValueChanged, new Action(OnBuildSizeChange));
		IntSelector ySelector = _ySelector;
		ySelector.ValueChanged = (Action)Delegate.Combine(ySelector.ValueChanged, new Action(OnBuildSizeChange));
		Selectable likeButton = _likeButton;
		likeButton.Clicked = (Action)Delegate.Combine(likeButton.Clicked, (Action)delegate
		{
			_likeIconAnimation = true;
			if (this.LikeButtonClicked != null)
			{
				this.LikeButtonClicked();
			}
		});
		_materialItemWidgets.Init(delegate(GameObject obj)
		{
			obj.GetComponent<RecipeMaterialInfoWidget>().PinClicked += OnPinToggle;
		});
		AnimWidget.SetAlpha(0f, useTween: false);
		_blurTexture = GetComponent<BlurTexture>();
		_scrollView.Reposition(resetPosition: true);
	}

	private void OnConfirm()
	{
		if (this.Confirmed != null)
		{
			this.Confirmed();
		}
	}

	private void OnPinToggle()
	{
		string trackingTarget = GameSystem<RecipeSystem>.Instance().TrackingTarget;
		string text = _id;
		if (trackingTarget == text)
		{
			text = null;
		}
		GameSystem<RecipeSystem>.Instance().SetTrackingTarget(text);
		UpdatePin();
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(null, (!string.IsNullOrEmpty(text)) ? T._("할 일 목록에 고정되었습니다.") : T._("할 일 목록에서 해제되었습니다."));
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Show(5f);
	}

	private void ShowRecipeDetailInfo(Crafting.Recipe recipe, bool reset)
	{
		_id = recipe.Id;
		SetTitle(recipe.Name);
		SetLike(recipe.Like);
		SetDescription(recipe.Description);
		float time = ((!recipe.Entrusts) ? 0f : recipe.DurationWait);
		SetRemainTime(time, T._("[icon=icon_clock_small:1.5] [D0CBC0]제작 시간[-]"));
		bool flag = !recipe.HasRequiredWorkbench || GameSystem<RecipeSystem>.Instance().FindNearestAvailableWorkbench(recipe) != null;
		RecipeSystem.HasMaterials(recipe, out var hasTool, _recipeSlotCount);
		BeginConditionSetting();
		using (Reusable<List<KeyValuePair<string, string>>> reusable = ReusableList<KeyValuePair<string, string>>.Pop())
		{
			List<KeyValuePair<string, string>> value = reusable.Value;
			if (recipe.MaxLevel > 0)
			{
				value.Add(new KeyValuePair<string, string>(T._("완성품 최대 레벨"), T._("<em>{0:lv:}</em>", recipe.MaxLevel)));
			}
			if (recipe.RequiredAbility.HasValue)
			{
				value.Add(new KeyValuePair<string, string>(T._("관련 능력"), $"<em>{recipe.RequiredAbility.Value.GetName()}</em>"));
			}
			Node ownerSkill = recipe.GetOwnerSkill();
			if (ownerSkill != null)
			{
				value.Add(new KeyValuePair<string, string>(T._("스킬 계열"), string.Format("<em>[icon={1}:1.5]{0}</em>", ownerSkill.Category.GetName(), IconMap.Get(ownerSkill.Category))));
			}
			AddCondition(value, new Color(0f, 0f, 0f, 0.4f));
			value.Clear();
			if (!string.IsNullOrEmpty(recipe.RequiredRecipe))
			{
				Crafting.Recipe recipe2 = GameSystem<RecipeSystem>.Instance().GetRecipe(recipe.RequiredRecipe);
				bool flag2 = recipe2?.HasRequiredRecipe() ?? false;
				value.Add(new KeyValuePair<string, string>(string.Format("[c][icon={1}][/c] {0}", T._("요구 제작법"), (!flag2) ? "button_checkbox_normal" : "button_checkbox_selected"), (recipe2 != null) ? recipe2.Name : recipe.RequiredRecipe));
			}
			if (recipe.DeductModifiableCount)
			{
				if (recipe is RecipeReform)
				{
					value.Add(new KeyValuePair<string, string>(T._("개조 슬롯 필요"), string.Empty));
				}
				else
				{
					value.Add(new KeyValuePair<string, string>(string.Format("[icon=bg_itemview_work_white] {0}", T._("가공횟수 소모")), "1"));
				}
			}
			if (recipe.HasRequiredWorkbench)
			{
				value.Add(new KeyValuePair<string, string>(string.Format("[c][icon={1}][/c] {0}", T._("제작대"), (!flag) ? "button_checkbox_normal" : "button_checkbox_selected"), Durango.Logic.Item.Util.LocalizedTagRequiredMsg(recipe.AllowedWorkbench)));
			}
			AddCondition(value, new Color(0f, 0f, 0f, 0.2f));
		}
		EndConditionSetting();
		List<SlotStruct> recipeSlotInfos = _recipeSlotInfos;
		recipeSlotInfos.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(recipe.Slots); i < size; i++)
		{
			Crafting.RecipeSlot recipeSlot = recipe.Slots[i];
			recipeSlotInfos.Add(new SlotStruct
			{
				Name = recipeSlot.Name,
				Count = _recipeSlotCount[i],
				RequiredCount = recipeSlot.Count,
				RequiredLevel = recipeSlot.RequiredLevel,
				Tags = recipeSlot.AllowedTags,
				Materials = recipeSlot.AllowedMaterials,
				SourceInfos = recipeSlot.SlotSourceInfos
			});
		}
		SlotStruct toolInfo;
		if (recipe.HasRequiredTool)
		{
			SlotStruct slotStruct = default(SlotStruct);
			slotStruct.Name = Durango.Logic.Item.Util.LocalizedTagRequiredMsg(recipe.AllowedTool, showLevel: false);
			slotStruct.Count = (hasTool ? 1 : 0);
			slotStruct.RequiredCount = 1;
			slotStruct.RequiredLevel = ((recipe.AllowedTool.Count > 0) ? recipe.AllowedTool[0].Level : 0);
			slotStruct.Tags = recipe.AllowedTool;
			toolInfo = slotStruct;
		}
		else
		{
			toolInfo = default(SlotStruct);
		}
		SetNonResiable();
		SetMaterials(recipeSlotInfos, toolInfo);
		SetNextButton(T._("제작"));
		UpdatePin();
		UIUtility.UpdateAnchors(base.transform);
		_scrollView.Reposition(reset, !reset);
	}

	private void ShowBlueprintDetailInfo(Building.Blueprint blueprint, bool reset)
	{
		_id = blueprint.Id;
		SetTitle(blueprint.Name);
		SetLike(blueprint.Like);
		SetDescription(blueprint.Description);
		SetRemainTime(0f, null);
		BeginConditionSetting();
		using (Reusable<List<KeyValuePair<string, string>>> reusable = ReusableList<KeyValuePair<string, string>>.Pop())
		{
			List<KeyValuePair<string, string>> value = reusable.Value;
			if (blueprint.MaxLevel > 0)
			{
				value.Add(new KeyValuePair<string, string>(T._("완성품 최대 레벨"), T._("<em>{0:lv:}</em>", blueprint.MaxLevel)));
			}
			if (blueprint.RequiredAbility.HasValue)
			{
				value.Add(new KeyValuePair<string, string>(T._("관련 능력"), $"<em>{blueprint.RequiredAbility.Value.GetName()}</em>"));
			}
			Node ownerSkill = blueprint.GetOwnerSkill();
			if (ownerSkill != null)
			{
				value.Add(new KeyValuePair<string, string>(T._("스킬 계열"), string.Format("<em>[icon={1}:1.5]{0}</em>", ownerSkill.Category.GetName(), IconMap.Get(ownerSkill.Category))));
			}
			AddCondition(value, new Color(0f, 0f, 0f, 0.4f));
			value.Clear();
			if (!string.IsNullOrEmpty(blueprint.RequiredBlueprint))
			{
				Building.Blueprint blueprint2 = GameSystem<RecipeSystem>.Instance().GetBlueprint(blueprint.RequiredBlueprint);
				bool flag = blueprint2?.HasRequiredBlueprint() ?? false;
				value.Add(new KeyValuePair<string, string>(string.Format("[c][icon={1}][/c] {0}", T._("요구 건설법"), (!flag) ? "button_checkbox_normal" : "button_checkbox_selected"), (blueprint2 != null) ? blueprint2.Name : blueprint.RequiredBlueprint));
			}
			AddCondition(value, new Color(0f, 0f, 0f, 0.2f));
		}
		EndConditionSetting();
		if (blueprint.IsSizeVariable)
		{
			int num = blueprint.Size.x;
			int num2 = blueprint.Size.y;
			if (blueprint.IsModular)
			{
				int num3 = (int)GameSystem<StatisticsSystem>.Instance().GetDeriveds(Derived.MaxModularSize);
				num = ((num > num3) ? num3 : num);
				num2 = ((num2 > num3) ? num3 : num2);
			}
			SetResizable(num, num2);
		}
		else
		{
			SetNonResiable();
		}
		RecipeSystem.HasMaterials(blueprint, Size, out var hasTool, _recipeSlotCount);
		List<SlotStruct> recipeSlotInfos = _recipeSlotInfos;
		recipeSlotInfos.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(blueprint.Slots); i < size; i++)
		{
			Building.BlueprintSlot blueprintSlot = blueprint.Slots[i];
			int num4 = 1;
			if (blueprint.IsSizeVariable)
			{
				num4 = blueprintSlot.GetSlotCountModifier(Size);
			}
			recipeSlotInfos.Add(new SlotStruct
			{
				Name = blueprintSlot.Name,
				Count = _recipeSlotCount[i],
				RequiredCount = blueprintSlot.Count * num4,
				RequiredLevel = blueprintSlot.RequiredLevel,
				Tags = blueprintSlot.AllowedTags,
				Materials = blueprintSlot.AllowedMaterials,
				SourceInfos = blueprintSlot.SlotSourceInfos
			});
		}
		SlotStruct toolInfo;
		if (blueprint.AllowedToolTag.Count > 0)
		{
			SlotStruct slotStruct = default(SlotStruct);
			slotStruct.Name = Durango.Logic.Item.Util.LocalizedTagRequiredMsg(blueprint.AllowedToolTag, showLevel: false);
			slotStruct.Count = (hasTool ? 1 : 0);
			slotStruct.RequiredCount = 1;
			slotStruct.RequiredLevel = ((blueprint.AllowedToolTag.Count > 0) ? blueprint.AllowedToolTag[0].Level : 0);
			slotStruct.Tags = blueprint.AllowedToolTag;
			toolInfo = slotStruct;
		}
		else
		{
			toolInfo = default(SlotStruct);
		}
		SetMaterials(recipeSlotInfos, toolInfo);
		SetNextButton(T._("건설"));
		UpdatePin();
		UIUtility.UpdateAnchors(base.transform);
		_scrollView.Reposition(reset, !reset);
	}

	private void SetTitle(string title)
	{
		_titleLabel.text = title;
	}

	private void SetLike(bool like)
	{
		_likeIcon.gameObject.SetActive(like);
		if (like)
		{
			UITweener component = _likeIcon.GetComponent<UITweener>();
			if (_likeIconAnimation)
			{
				component.ResetToBeginning();
				component.PlayForward();
			}
			else
			{
				component.Sample(1f, isFinished: true);
				component.enabled = false;
			}
		}
		_likeIconAnimation = false;
	}

	private void UpdatePin()
	{
		if (_materialItemWidgets.Count != 0)
		{
			_materialItemWidgets[0].GetComponent<RecipeMaterialInfoWidget>().SetPinButton(GameSystem<RecipeSystem>.Instance().TrackingTarget == _id);
		}
	}

	private void SetDescription(string description)
	{
		if (string.IsNullOrEmpty(description))
		{
			_descriptionWidget.gameObject.SetActive(value: false);
			return;
		}
		_descriptionWidget.gameObject.SetActive(value: true);
		_descriptionLabel.text = description;
		_descriptionWidget.height = (int)_descriptionLabel.printedSize.y + _descriptionPadding;
	}

	private void SetRemainTime(float time, string keyText)
	{
		if (time <= 0f)
		{
			_timeWidget.gameObject.SetActive(value: false);
			return;
		}
		_timeWidget.gameObject.SetActive(value: true);
		_timeKeyLabel.text = keyText;
		_timeValueLabel.text = TimedeltaFormatter.Format(time);
	}

	private void BeginConditionSetting()
	{
		_conditionsItems.BeginLoad();
	}

	private void AddCondition(List<KeyValuePair<string, string>> value, Color bgColor)
	{
		if (KUtility.GetSize(value) != 0)
		{
			RecipeInfoConditionWidget component = _conditionsItems.GetNext().GetComponent<RecipeInfoConditionWidget>();
			component.Set(value);
			component.SetBackgroundColor(bgColor);
		}
	}

	private void EndConditionSetting()
	{
		_conditionsItems.EndLoad();
		if (_conditionsItems.Count == 0)
		{
			_conditionsWidget.gameObject.SetActive(value: false);
			return;
		}
		_conditionsWidget.gameObject.SetActive(value: true);
		int height = Mathf.CeilToInt(UIUtility.WidgetsReposition(_conditionsItems, _conditionsWidget, Vector3.down));
		_conditionsWidget.height = height;
	}

	private void SetResizable(int xMax, int yMax)
	{
		if (xMax <= 1 && yMax <= 1)
		{
			SetNonResiable();
			return;
		}
		_sizeSelectorWidget.gameObject.SetActive(value: true);
		_xSelector.Set(_xSelector.Value, 1, xMax);
		_ySelector.Set(_ySelector.Value, 1, yMax);
	}

	private void SetNonResiable()
	{
		_sizeSelectorWidget.gameObject.SetActive(value: false);
	}

	private void SetMaterials([NotNull] IList<SlotStruct> slots, SlotStruct toolInfo)
	{
		bool flag = !string.IsNullOrEmpty(toolInfo.Name);
		int num = slots.Count + (flag ? 1 : 0);
		_materialWidget.gameObject.SetActive(num > 0);
		if (num > 0)
		{
			_materialItemWidgets.Set((!flag) ? 1 : 2);
			RecipeMaterialInfoWidget component = _materialItemWidgets[0].GetComponent<RecipeMaterialInfoWidget>();
			component.Set(T._("재료"), slots);
			if (flag)
			{
				RecipeMaterialInfoWidget component2 = _materialItemWidgets[1].GetComponent<RecipeMaterialInfoWidget>();
				component2.Set(T._("도구"), new SlotStruct[1] { toolInfo });
				component2.SetPinButton(null);
			}
			int num2 = 0;
			int i = 0;
			for (int count = _materialItemWidgets.Count; i < count; i++)
			{
				num2 += _materialItemWidgets[i].GetComponent<UIWidget>().height;
			}
			_materialWidget.height = num2;
			_materialItemWidgets.Reposition(Vector3.down);
		}
	}

	private void SetNextButton(string text, bool enable = true)
	{
		// The PC recipe-detail prefab can keep this footer inactive after it was
		// hidden by an earlier screen state. Merely changing Text/Disabled does
		// not reactivate a Unity GameObject, leaving no visible way to continue.
		_buttonWidget.gameObject.SetActive(value: true);
		_nextButton.gameObject.SetActive(value: true);
		_nextButton.Text = text;
		_nextButton.Disabled = !enable;
	}

	private void OnBuildSizeChange()
	{
		if (this.BuildSizeChanged != null)
		{
			this.BuildSizeChanged(_xSelector.Value, _ySelector.Value);
		}
	}
}
