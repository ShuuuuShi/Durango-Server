using System;
using System.Text;
using Building;
using Crafting;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ExpectResultWidget : UIWidget
{
	[SerializeField]
	private UILabel _helpLabel;

	[SerializeField]
	private GameObject _moreInfoButton;

	[SerializeField]
	private GameObject _helpTouchBox;

	[SerializeField]
	private UIWidget _previewWidget;

	[SerializeField]
	private UISprite _iconResult;

	[SerializeField]
	private UILabel _iconResultLabel;

	[SerializeField]
	private UIModelViewer _previewTexture;

	[SerializeField]
	private TweenerPlayer _rareResultEffect;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private ExpectResultDetailWidget _expectResultDetailWidget;

	[SerializeField]
	private UIWidget _bonusItemWidget;

	[SerializeField]
	private IntSelector _quantitySelector;

	[SerializeField]
	private UILabel _textSuccessRate;

	[SerializeField]
	private UIWidget _remodelingWidget;

	[SerializeField]
	private Color[] _resultLevelRateTextColors;

	[SerializeField]
	private int[] _resultLevelRatePercentages;

	private bool _isCraft;

	private CraftSlotContainer _craft;

	private BuildSlotContainer _build;

	private CraftEstimationInfo? _craftEstimation;

	private ArtifactPreview? _previewMsg;

	private bool IsRemodeling
	{
		get
		{
			if (!_isCraft && _build != null)
			{
				return _build.Blueprint.IsRemodeling;
			}
			return false;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (!Application.isPlaying)
		{
			return;
		}
		UIEventListener uIEventListener = UIEventListener.Get(_helpTouchBox);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(ShowHelpTooltip));
		UIEventListener uIEventListener2 = UIEventListener.Get(_previewWidget.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(ShowPreviewPopup));
		UIEventListener uIEventListener3 = UIEventListener.Get(_bonusItemWidget.gameObject);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (_craft != null)
			{
				int? level = null;
				if (_craftEstimation.HasValue)
				{
					level = _craftEstimation.Value.CraftLevel;
				}
				UIManager.Popup.Tooltip<ReceiveRewardsPopup>().ShowRecipeBonusInfo(_craft.Recipe.Id, level);
			}
		});
		IntSelector quantitySelector = _quantitySelector;
		quantitySelector.ValueChanged = (Action)Delegate.Combine(quantitySelector.ValueChanged, new Action(OnQuantityChanged));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_previewMsg = null;
	}

	public void Set(CraftSlotContainer slotContainer)
	{
		_isCraft = true;
		_craft = slotContainer;
		RefreshHelpLabel();
		_remodelingWidget.gameObject.SetActive(value: false);
		if (_craft.TechSupportBaseSlotInfo == null)
		{
			int max = Mathf.Max(1, _craft.CalcMaxQuantity());
			_quantitySelector.gameObject.SetActive(value: true);
			_quantitySelector.Set(_craft.Quantity, 1, max);
		}
		else
		{
			_quantitySelector.gameObject.SetActive(value: false);
		}
		BonusPrototypes[] array = SingletonDict<string, BonusPrototypes[]>.Get(slotContainer.Recipe.Id);
		if (array != null && array.Length != 0)
		{
			_bonusItemWidget.gameObject.SetActive(value: true);
		}
		else
		{
			_bonusItemWidget.gameObject.SetActive(value: false);
		}
	}

	public void Set(BuildSlotContainer slotContainer)
	{
		_isCraft = false;
		_build = slotContainer;
		RefreshHelpLabel();
		_remodelingWidget.gameObject.SetActive(slotContainer.Blueprint.IsRemodeling);
		_quantitySelector.gameObject.SetActive(value: false);
		_bonusItemWidget.gameObject.SetActive(value: false);
	}

	public void Refresh()
	{
		if (_isCraft)
		{
			_expectResultDetailWidget.Set(_craft);
		}
		else
		{
			_expectResultDetailWidget.Set(_build);
		}
		UpdateLayout();
	}

	public void ClearEstimation()
	{
		if (_isCraft)
		{
			SetCraftEstimation(null);
		}
		else
		{
			SetBuildEstimation(null);
		}
		_expectResultDetailWidget.ClearEstimation();
	}

	public void SetPreviewTextureMode()
	{
		_iconResult.gameObject.SetActive(value: false);
		_iconResultLabel.gameObject.SetActive(value: false);
		_previewTexture.gameObject.SetActive(value: true);
		_moreInfoButton.SetActive(value: true);
		_previewWidget.height = 215;
		UpdateLayout();
	}

	public void SetIconMode(bool expectPreviewTexture, bool isBig = false)
	{
		_iconResult.gameObject.SetActive(value: true);
		_previewTexture.gameObject.SetActive(value: false);
		_moreInfoButton.SetActive(value: false);
		_iconResultLabel.gameObject.SetActive(expectPreviewTexture);
		_iconResult.alpha = ((!expectPreviewTexture) ? 1f : 0.2f);
		_iconResultLabel.text = T._("모든 재료를 선택하면 미리보기 가능");
		_previewWidget.height = ((!isBig) ? 120 : 215);
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		GetComponent<RectLayoutComponent>().UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	public void SetCraftEstimation(CraftEstimationInfo? info)
	{
		_craftEstimation = info;
		CraftEstimation? estimation = (info.HasValue ? info.Value.CraftEstimation : null);
		Crafting.Recipe recipe = ((_craft != null) ? _craft.Recipe : null);
		string text = ((recipe != null) ? recipe.Name : string.Empty);
		int? resultLevel = null;
		bool isImmuneToTime = false;
		bool isTimeLimited = false;
		if (estimation.HasValue)
		{
			CraftEstimation value = estimation.Value;
			Prototype itemPrototype = PrototypeYaml.GetItemPrototype(value.PrototypeId, value.Level);
			if (itemPrototype != null)
			{
				_iconResult.spriteName = itemPrototype.Icon;
				isImmuneToTime = itemPrototype.ImmuneToTime;
				isTimeLimited = itemPrototype.TimeLimited;
			}
			resultLevel = value.Level;
			text = value.Name;
		}
		else
		{
			_iconResult.spriteName = ((recipe != null) ? recipe.Icon : string.Empty);
		}
		string text2 = ((_craft != null) ? CreateLevelText(resultLevel, _craft.GetAverageMaterialsLevel(0)) : string.Empty);
		int num = (recipe?.Count ?? 0) * ((_craft == null) ? 1 : _craft.Quantity);
		if (num > 1)
		{
			_textName.text = string.Format("{0} {1} [size=24][FFFFFF7F]/ {2}[-][/size]", text, text2, T._("{0}개", num));
		}
		else
		{
			_textName.text = text + " " + text2;
		}
		if (estimation.HasValue)
		{
			_textSuccessRate.text = $"{estimation.Value.SuccessRate:P0}  <em>[icon=icon_question_big]</em>";
		}
		else
		{
			_textSuccessRate.text = "-  <em>[icon=icon_question_big]</em>";
		}
		_expectResultDetailWidget.SetEstimation(estimation, recipe as RecipeReform, isImmuneToTime, isTimeLimited);
		if (!estimation.HasValue || estimation.Value.UnrevealedRareTagCount == 0)
		{
			_rareResultEffect.gameObject.SetActive(value: false);
			return;
		}
		_rareResultEffect.gameObject.SetActive(value: true);
		_rareResultEffect.Play();
	}

	public void SetBuildEstimation(BuildEstimation? estimation)
	{
		Building.Blueprint blueprint = ((_build != null) ? _build.Blueprint : null);
		string text = ((blueprint != null) ? blueprint.Name : string.Empty);
		int? resultLevel = null;
		if (estimation.HasValue)
		{
			resultLevel = estimation.Value.Level;
			SetPreview(estimation.Value.ArtifactPreview);
		}
		_iconResult.spriteName = ((blueprint != null) ? blueprint.Icon : string.Empty);
		string text2 = ((_build != null) ? CreateLevelText(resultLevel, _build.GetAverageMaterialsLevel(0)) : string.Empty);
		_textName.text = text + " " + text2;
		_expectResultDetailWidget.SetEstimation(estimation);
		if (!estimation.HasValue || estimation.Value.UnrevealedRareTagCount == 0)
		{
			_rareResultEffect.gameObject.SetActive(value: false);
		}
		else
		{
			_rareResultEffect.gameObject.SetActive(value: true);
			_rareResultEffect.Play();
		}
		_textSuccessRate.text = $"{1f:P0}  <em>[icon=icon_question_big]</em>";
	}

	public void SetRemodelingEstimation([NotNull] Artifact artifact, ArtifactPreview? artifactPreview)
	{
		SetPreview(artifactPreview);
		_iconResult.spriteName = ((artifact.Blueprint != null) ? artifact.Blueprint.Icon : string.Empty);
		_textName.text = string.Format("{0} {1}", (artifact.Blueprint != null) ? artifact.Blueprint.Name : string.Empty, NGUIText.EncodeColor(T._("{0:lv:}", artifact.ArtifactState.Level), PresetColor.UIYellow));
		_textSuccessRate.text = $"{1f:P0}  <em>[icon=icon_question_big]</em>";
		_expectResultDetailWidget.SetEstimation(artifact);
		_rareResultEffect.gameObject.SetActive(value: false);
	}

	public void SetTechSupportEstimation(TechSupportBaseSlotInfo slotInfo)
	{
		TechSupportTarget target = slotInfo?.Target ?? default(TechSupportTarget);
		RecipeReform reformRecipe = TechSupportSystem.GetReformRecipe(target.GetReformSlot());
		Prototype prototype = ((target.Item == null) ? null : PrototypeYaml.GetItemPrototype(target.Item.PrototypeId, target.Item.Level));
		_iconResult.spriteName = ((prototype == null) ? string.Empty : prototype.Icon);
		if (target.Item != null)
		{
			_textName.text = string.Format("{0} {1}", target.Item.Name, NGUIText.EncodeColor(T._("{0:lv:}", target.Item.Level), PresetColor.UIYellow));
		}
		else
		{
			_textName.text = string.Empty;
		}
		_textSuccessRate.text = $"{1f:P0}  <em>[icon=icon_question_big]</em>";
		_expectResultDetailWidget.SetEstimation(GameSystem<TechSupportSystem>.Instance().GetEstimate(target), reformRecipe);
		_rareResultEffect.gameObject.SetActive(value: false);
	}

	private void SetPreview(ArtifactPreview? artifactPreview)
	{
		if (artifactPreview.HasValue)
		{
			SetPreviewTextureMode();
			ArtifactPreview value = artifactPreview.Value;
			_previewMsg = value;
			_previewTexture.SetArtifactModel(new UIModelViewer.ArtifactArguments
			{
				Display = value.Display,
				Size = value.Size,
				Rotation = value.Rotation,
				IsModular = value.IsModular
			}, new UIModelViewer.Arguments
			{
				CameraAngle = 35f,
				Rotation = -45f
			});
		}
		else
		{
			_previewMsg = null;
			SetIconMode(expectPreviewTexture: false, isBig: true);
		}
	}

	private void RefreshHelpLabel()
	{
		_helpLabel.text = ((!IsRemodeling) ? T._("예상 결과") : T._("개조 결과"));
	}

	private void ShowHelpTooltip(GameObject obj)
	{
		string text = ((!_isCraft) ? ((!IsRemodeling) ? T._("결과물은 예상 결과물과 다를 수 있습니다.") : T._("투입된 재료에 따라 건물 외형이 변경됩니다.")) : ((_craft == null || _craft.TechSupportBaseSlotInfo == null) ? MakeCraftSuccessRateHelpText() : T._("장비의 개조 슬롯 결과만 표시됩니다.")));
		if (!string.IsNullOrEmpty(text))
		{
			UIWidget childSprite = UIUtility.GetChildSprite(_textSuccessRate, 0);
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(null, text, 500);
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Horizontal;
			widgetTooltipControl.Show(childSprite, Vector2.zero, 10f);
		}
	}

	private void ShowPreviewPopup(GameObject go)
	{
		if (_previewMsg.HasValue)
		{
			UIManager.Popup.Tooltip<ModelPreviewPopup>().Show(_previewMsg.Value, T._("미리보기"));
		}
	}

	private string MakeCraftSuccessRateHelpText()
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		Crafting.Recipe recipe = ((_craft != null) ? _craft.Recipe : null);
		if (recipe != null && recipe.RequiredAbility.HasValue)
		{
			value.Append("<kv>");
			value.AppendFormat("key={0},", T._("사용 능력"));
			value.AppendFormat("value=<em>{0}</em>", recipe.RequiredAbility.Value.GetName());
			value.Append("</kv>");
			value.Append("<br>10</br>");
		}
		if (_craftEstimation.HasValue && _craftEstimation.Value.CraftEstimation.HasValue)
		{
			CraftEstimation value2 = _craftEstimation.Value.CraftEstimation.Value;
			value.Append("<kv>");
			value.AppendFormat("key={0},", T._("난이도"));
			value.AppendFormat("value=<em>{0:0}</em>", value2.RequiredAbilityValue);
			value.Append("</kv>");
			value.Append("<br>10</br>");
			value.Append("<kv>");
			value.AppendFormat("key={0},", T._("성공률"));
			value.AppendFormat("value=<em>{0:P0}</em>", value2.SuccessRate);
			value.Append("</kv>");
			value.Append("<br>10</br>");
			value.Append("<kv>");
			value.AppendFormat("key={0},", T._("대성공률"));
			value.AppendFormat("value=<em>{0:P1}</em>", value2.GreatSuccessRate);
			value.Append("</kv>");
		}
		if (value.Length > 0)
		{
			value.Append("<hr/>");
		}
		value.Append("<li>");
		value.Append(T._("성공률은 제작자의 제작능력, 제작법의 난이도, 도구 수준 등에 영향을 받습니다."));
		value.Append("</li>");
		value.Append("<br>10</br>");
		value.Append("<li>");
		value.Append(T._("결과물의 예상 결과물과 다를 수 있습니다."));
		value.Append("</li>");
		value.Append("<br>10</br>");
		value.Append("<li>");
		value.Append(T._("제작 실패시 결과물이 없을 수도 있습니다."));
		value.Append("</li>");
		return value.ToString();
	}

	private string CreateLevelText(int? resultLevel, float averageMaterialLevel)
	{
		string text;
		Color c;
		if (resultLevel.HasValue)
		{
			int percentage = ((!(averageMaterialLevel > 0f)) ? 100 : Mathf.CeilToInt((float)resultLevel.Value / averageMaterialLevel * 100f));
			text = resultLevel.Value.ToString();
			c = UIUtility.GetValueByPercentage(percentage, _resultLevelRatePercentages, _resultLevelRateTextColors);
		}
		else
		{
			text = "?";
			c = PresetColor.UIYellow;
		}
		return NGUIText.EncodeColor(T._("{0:lv:}", text), c);
	}

	private void OnQuantityChanged()
	{
		if (_isCraft && _craft != null)
		{
			_craft.SetQuantity(_quantitySelector.Value);
		}
	}
}
