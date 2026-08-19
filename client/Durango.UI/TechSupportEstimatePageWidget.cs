using System;
using System.Collections.Generic;
using System.Linq;
using Crafting;
using Durango.Logic.Item;
using Durango.Network;
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

public class TechSupportEstimatePageWidget : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private GameObject _notSelected;

	[SerializeField]
	private Selectable _techSupportHelpButton;

	[SerializeField]
	private GameObject _cardNewsButton;

	[SerializeField]
	private GameObject _upper;

	[SerializeField]
	private UISprite _iconDecoration;

	[SerializeField]
	private UILabel _textDecorationName;

	[SerializeField]
	private UILabel _textItemName;

	[SerializeField]
	private SelectableButton _removeButton;

	[SerializeField]
	private UISprite _iconEstimate;

	[SerializeField]
	private UILabel _textEstimate;

	[SerializeField]
	private UILabel _textRemainTime;

	[SerializeField]
	private SelectableButton _issueButton;

	[SerializeField]
	private TechSupportEstimateEffectsAndMaterialsWidget _estimateEffectsAndMaterialsWidget;

	[SerializeField]
	private GameObject _lower;

	[SerializeField]
	private GameObject _noEstimate;

	[SerializeField]
	private GameObject _reform;

	[SerializeField]
	private SelectableButton _reformButton;

	[SerializeField]
	private SpriteData _noEstimateSprite;

	[SerializeField]
	private SpriteData _estimateSprite;

	public PropKey PropKey { get; private set; }

	public TechSupportTarget Target { get; private set; }

	void IUIInitializable.Init()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_cardNewsButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(CardNewsButton_Clicked));
		_removeButton.Icon = "button_delete";
		_removeButton.CanClickWhenDisabled = true;
		SelectableButton removeButton = _removeButton;
		removeButton.Clicked = (Action)Delegate.Combine(removeButton.Clicked, new Action(RemoveButton_Clicked));
		_issueButton.CanClickWhenDisabled = true;
		SelectableButton issueButton = _issueButton;
		issueButton.Clicked = (Action)Delegate.Combine(issueButton.Clicked, new Action(IssueButton_Clicked));
		_reformButton.CanClickWhenDisabled = true;
		SelectableButton reformButton = _reformButton;
		reformButton.Clicked = (Action)Delegate.Combine(reformButton.Clicked, new Action(ReformButton_Clicked));
		TechSupportSystem techSupportSystem = GameSystem<TechSupportSystem>.Instance();
		techSupportSystem.EstimatesLoadCompleted += TechSupportSystem_EstimatesLoadCompleted;
		Selectable techSupportHelpButton = _techSupportHelpButton;
		techSupportHelpButton.Clicked = (Action)Delegate.Combine(techSupportHelpButton.Clicked, (Action)delegate
		{
			UIManager.Popup.Tooltip<TechSupportListPopup>().Show();
		});
		_notSelected.GetComponent<RectLayoutComponent>().UpdateOnSizeChange();
	}

	public void SetArtifact([NotNull] Artifact artifact)
	{
		PropKey = artifact.GetPropKey();
	}

	public void SetItem(TechSupportTarget target)
	{
		Target = target;
		Refresh();
	}

	public void Refresh()
	{
		if (GameSystem<TechSupportSystem>.Instance().EstimatesLoaded)
		{
			if (Target.Item != null)
			{
				_upper.SetActive(value: true);
				_estimateEffectsAndMaterialsWidget.gameObject.SetActive(value: true);
				_lower.SetActive(value: true);
				_notSelected.SetActive(value: false);
				TechSupportEstimate? estimate = GameSystem<TechSupportSystem>.Instance().GetEstimate(Target);
				ReformSlot? reformSlot = Target.GetReformSlot();
				RecipeReform reformRecipe = TechSupportSystem.GetReformRecipe(reformSlot);
				RefreshItem();
				RefreshDecoration(reformRecipe, reformSlot);
				RefreshEstimate(estimate);
				_reformButton.Disabled = OptionSystem.IsShutdownTechSupport();
				_estimateEffectsAndMaterialsWidget.Refresh(reformRecipe, reformSlot, estimate);
			}
			else
			{
				_upper.SetActive(value: false);
				_estimateEffectsAndMaterialsWidget.gameObject.SetActive(value: false);
				_lower.SetActive(value: false);
				_notSelected.SetActive(value: true);
			}
			UIManager.Popup.LoadingRing.DetachFromWidget(base.gameObject);
		}
		else
		{
			_upper.SetActive(value: false);
			_estimateEffectsAndMaterialsWidget.gameObject.SetActive(value: false);
			_lower.SetActive(value: false);
			_notSelected.SetActive(value: false);
			UIManager.Popup.LoadingRing.AttachToWidget(base.gameObject);
		}
	}

	public static void ShowShutdownWarningMsg()
	{
		UIManager.SystemMsg(T._("해당 시스템은 점검 중이며 이용이 불가능합니다."));
	}

	private void RefreshItem()
	{
		if (Target.Item != null)
		{
			_textItemName.text = T._("{0:lv:}  {1}", Target.Item.Level, Target.Item.Name);
		}
	}

	private void RefreshDecoration(RecipeReform recipe, ReformSlot? reformSlot)
	{
		if (recipe != null)
		{
			_iconDecoration.spriteName = recipe.Icon;
			if (reformSlot.HasValue && !string.IsNullOrEmpty(reformSlot.Value.Decorator))
			{
				_textDecorationName.text = $"{recipe.RecipeNameForSlot} ({reformSlot.Value.Decorator})";
			}
			else
			{
				_textDecorationName.text = recipe.RecipeNameForSlot;
			}
		}
		_removeButton.Disabled = OptionSystem.IsShutdownResetReformSlot();
	}

	private void RefreshEstimate(TechSupportEstimate? estimate)
	{
		if (estimate.HasValue)
		{
			_noEstimate.SetActive(value: false);
			_reform.SetActive(value: true);
			_estimateSprite.Set(_iconEstimate);
			_textEstimate.text = T._("<em>견적서</em>");
			_textRemainTime.text = GetRemainTimeText(estimate.Value.ValidUntil);
			_issueButton.Text = T._("재요청");
			_issueButton.SetStyle(PresetButton.Style.Border);
		}
		else
		{
			_noEstimate.SetActive(value: true);
			_reform.SetActive(value: false);
			_noEstimateSprite.Set(_iconEstimate);
			_textEstimate.text = T._("견적서 없음");
			_textRemainTime.text = string.Empty;
			_issueButton.Text = T._("견적서 요청");
			_issueButton.SetStyle(PresetButton.Style.Solid);
		}
		_issueButton.Disabled = OptionSystem.IsShutdownTechSupportEstimate();
	}

	private void OpenCraftGroupForTechSupport()
	{
		if (Target.Item == null)
		{
			return;
		}
		RecipeReform reformRecipe = TechSupportSystem.GetReformRecipe(Target.GetReformSlot());
		if (reformRecipe != null)
		{
			Artifact artifact = Durango.Utils.Singleton<ArtifactManager>.Instance().Find(PropKey.EntityId);
			if (artifact != null)
			{
				UIManager.FindScript<CraftGroupBase>().Open(reformRecipe, artifact, quickFill: false, Target);
			}
		}
	}

	private IEnumerable<KeyValuePair<string, string>> GetReformWarnings()
	{
		TechSupportEstimate? estimate = GameSystem<TechSupportSystem>.Instance().GetEstimate(Target);
		ReformSlot? reformSlot = Target.GetReformSlot();
		if (!estimate.HasValue || !reformSlot.HasValue)
		{
			yield break;
		}
		Messages.Tag[] resultTags = estimate.Value.Tags;
		Messages.Tag[] tags = reformSlot.Value.Tags;
		for (int i = 0; i < tags.Length; i++)
		{
			Messages.Tag reformTag = tags[i];
			int newLevel = TechSupportTag.GetTag(resultTags, reformTag.Id).Level;
			bool? improved = TagYaml.IsTagImproved(reformTag.Id, reformTag.Level, newLevel);
			if (improved.HasValue && !improved.Value)
			{
				string key = T._("<tag>{0}</tag>", reformTag.Id);
				string value = T._("<em>{0:lv:}</em>    [preset=animation_arrow]    <em>{1:lv:}</em>  [c=ui_red][icon=img_pet_arrow_down][-]", reformTag.Level, newLevel);
				yield return new KeyValuePair<string, string>(key, value);
			}
		}
	}

	private static string GetRemainTimeText(double expiredAt)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		return T._("[icon=icon_skill_time]  {0} 남음", TimedeltaFormatter.Format(expiredAt - predictedServerTime, 2, "min"));
	}

	private void CardNewsButton_Clicked(GameObject go)
	{
		CardNewsPopup cardNewsPopup = UIManager.Popup.Tooltip<CardNewsPopup>();
		if (cardNewsPopup.Load("techsupport_help"))
		{
			cardNewsPopup.Show();
		}
	}

	private void RemoveButton_Clicked()
	{
		if (OptionSystem.IsShutdownResetReformSlot())
		{
			ShowShutdownWarningMsg();
		}
		else
		{
			if (Target.Item == null)
			{
				return;
			}
			Yaml.Cost resetReformSlot = Yaml.Util.Singleton<CostsYaml>.Instance.ResetReformSlot;
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
			UIManager.MessageBox.Show(T._("개조를 제거하시겠습니까?"), T._("개조 제거시 개조 슬롯이 완전히 비워집니다.\n지금까지 받은 기술 지원 결과와 견적서는 복구되지 않습니다."), delegate(bool ok)
			{
				if (ok)
				{
					GameSystem<TechSupportSystem>.Instance().RemoveDecoration(PropKey, Target);
				}
			}, string.Format("{0}  {1}", T._("제거"), Durango.Logic.Item.Inventory.CurrencyEmphasisFormat(resetReformSlot.GetAmount(), resetReformSlot.Currency)));
		}
	}

	private void IssueButton_Clicked()
	{
		if (OptionSystem.IsShutdownTechSupportEstimate())
		{
			ShowShutdownWarningMsg();
		}
		else if (Target.Item != null)
		{
			TechSupportEstimatePopup techSupportEstimatePopup = UIManager.Popup.Tooltip<TechSupportEstimatePopup>();
			techSupportEstimatePopup.Set(PropKey, Target);
			techSupportEstimatePopup.Show();
			techSupportEstimatePopup.AddOnFinished(Refresh);
		}
	}

	private void ReformButton_Clicked()
	{
		if (OptionSystem.IsShutdownTechSupport())
		{
			ShowShutdownWarningMsg();
			return;
		}
		IEnumerable<KeyValuePair<string, string>> reformWarnings = GetReformWarnings();
		bool flag = Target.Item != null && Target.Item.Tradable;
		if (!reformWarnings.Any() && !flag)
		{
			OpenCraftGroupForTechSupport();
			return;
		}
		MessageBox messageBox = UIManager.MessageBox;
		foreach (KeyValuePair<string, string> item in reformWarnings)
		{
			messageBox.AddKeyValueInfo(item.Key, item.Value);
		}
		string subText = null;
		string mainText;
		if (flag)
		{
			mainText = T._("기술 지원을 받은 장비는 더 이상 거래할 수 없게 됩니다. 계속하시겠습니까?");
			subText = ((!reformWarnings.Any()) ? null : T._("<alert><alert_icon/> 하락하는 속성이 있습니다.</alert>"));
		}
		else
		{
			mainText = T._("하락하는 속성이 있습니다. 진행하시겠습니까?");
		}
		messageBox.Show(mainText, subText, delegate(bool ok)
		{
			if (ok)
			{
				OpenCraftGroupForTechSupport();
			}
		});
	}

	private void TechSupportSystem_EstimatesLoadCompleted()
	{
		Refresh();
	}
}
