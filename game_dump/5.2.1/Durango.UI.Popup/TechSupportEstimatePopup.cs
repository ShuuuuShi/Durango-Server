using System;
using System.Collections.Generic;
using System.Linq;
using Crafting;
using Durango.Logic.Item;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class TechSupportEstimatePopup : TooltipBase
{
	private enum WorkState
	{
		Hidden,
		Initializing,
		Idle,
		RequestEstimate,
		ShowResult
	}

	[SerializeField]
	private UISprite _iconEstimate;

	[SerializeField]
	private UILabel _textInfo;

	[SerializeField]
	private UIWidget _tags;

	[SerializeField]
	private TechSupportTag _techSupportTagBase;

	[SerializeField]
	private SelectableButton _primaryButton;

	[SerializeField]
	private SelectableButton _secondaryButton;

	[SerializeField]
	private RectLayout _layout;

	[SerializeField]
	private SpriteData _noEstimateSprite;

	[SerializeField]
	private SpriteData _estimateSprite;

	[SerializeField]
	private float _showTagFinishDelay = 1f;

	private WorkState _currentState;

	private PropKey _propKey;

	private TechSupportTarget _target;

	private int _requestCount;

	private TechSupportEstimate? _estimate;

	private ReformSlot? _reformSlot;

	private ReformTechSupport _yamlTechSupport;

	private ListObjectPool<TechSupportTag> _tagItems;

	private bool _nowLoading;

	private bool _buttonAssignChanged;

	private SelectableButton _issueButton;

	private SelectableButton _closeButton;

	public override bool DragLock => true;

	protected override void Start()
	{
		base.Start();
		GameSystem<TechSupportSystem>.Instance().EstimateUpdated += TechSupportSystem_EstimateUpdated;
		_tagItems = new ListObjectPool<TechSupportTag>();
		_tagItems.BaseObject = _techSupportTagBase;
		_tagItems.UseBase = true;
		_tagItems.Init(delegate(TechSupportTag tagItem)
		{
			tagItem.Init();
			tagItem.LockButtonClicked += TagItem_LockButtonClicked;
		});
		SelectableButton primaryButton = _primaryButton;
		primaryButton.Clicked = (Action)Delegate.Combine(primaryButton.Clicked, new Action(PrimaryButton_Clicked));
		SelectableButton secondaryButton = _secondaryButton;
		secondaryButton.Clicked = (Action)Delegate.Combine(secondaryButton.Clicked, new Action(SecondaryButton_Clicked));
	}

	public void Set(PropKey propKey, TechSupportTarget target)
	{
		_propKey = propKey;
		_target = target;
		TechSupportEstimateInfo? estimateInfo = GameSystem<TechSupportSystem>.Instance().GetEstimateInfo(_target);
		if (estimateInfo.HasValue)
		{
			_requestCount = estimateInfo.Value.RequestCount;
			_estimate = estimateInfo.Value.Estimate;
		}
		else
		{
			_requestCount = 0;
			_estimate = null;
		}
		_reformSlot = _target.GetReformSlot();
		RecipeReform reformRecipe = TechSupportSystem.GetReformRecipe(_reformSlot);
		_yamlTechSupport = ((reformRecipe == null) ? null : SingletonDict<string, ReformTechSupport>.Instance.Get(reformRecipe.Id));
		_currentState = WorkState.Initializing;
	}

	protected override void FillData()
	{
		RefreshButtonAssign();
		RefreshEstimate();
		RefreshTags();
		RefreshButtonsText();
		_currentState = WorkState.Idle;
		HideLoadingRing();
	}

	protected override void UpdateLayout()
	{
		_layout.UpdateLayout();
		base.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
	}

	protected override void OnHide()
	{
		_currentState = WorkState.Hidden;
		base.OnHide();
	}

	private void RefreshButtonAssign(bool change = false)
	{
		_buttonAssignChanged = change;
		if (_buttonAssignChanged)
		{
			_issueButton = _primaryButton;
			_closeButton = _secondaryButton;
		}
		else
		{
			_issueButton = _secondaryButton;
			_closeButton = _primaryButton;
		}
	}

	private void RefreshEstimate()
	{
		if (_estimate.HasValue)
		{
			_estimateSprite.Set(_iconEstimate);
			_textInfo.text = ((_currentState == WorkState.ShowResult) ? T._("<em>견적서 발급 완료</em>") : T._("<em>견적서</em>"));
		}
		else
		{
			_noEstimateSprite.Set(_iconEstimate);
			_textInfo.text = T._("[c=ui_light_gray]견적서 없음[-]");
		}
	}

	private void RefreshButtonsText()
	{
		if (_estimate.HasValue)
		{
			_issueButton.SetClickSound(UISound.ClickType.ButtonHighlight);
			_issueButton.Text = GetEstimateButtonText(reissue: true, _buttonAssignChanged);
			_closeButton.SetClickSound((!_buttonAssignChanged) ? UISound.ClickType.ButtonHighlight : UISound.ClickType.TechSupport);
			_closeButton.Text = ((!_buttonAssignChanged) ? T._("취소") : T._("확인"));
		}
		else
		{
			_issueButton.SetClickSound(UISound.ClickType.ButtonHighlight);
			_issueButton.Text = GetEstimateButtonText(reissue: false);
			_closeButton.SetClickSound(UISound.ClickType.ButtonHighlight);
			_closeButton.Text = T._("취소");
		}
	}

	private void RefreshButtonsEnableState()
	{
		_issueButton.Disabled = _nowLoading || !HasUnlockedTags();
		_closeButton.Disabled = _nowLoading;
	}

	private void RefreshTags()
	{
		_tagItems.BeginLoad();
		if (_reformSlot.HasValue && _yamlTechSupport != null)
		{
			if (_estimate.HasValue)
			{
				AddTechSupportTagWithResult(_tagItems, _reformSlot.Value, _estimate.Value, _yamlTechSupport);
			}
			else
			{
				AddTechSupportTag(_tagItems, _reformSlot.Value, _yamlTechSupport);
			}
		}
		_tagItems.EndLoad();
		_tags.height = (int)UIUtility.WidgetsReposition(_tagItems, _tags, Vector3.down);
	}

	private void IssueEstimate()
	{
		if (_currentState != WorkState.Idle)
		{
			return;
		}
		if (_yamlTechSupport != null && NotEnoughRandomPieces(_yamlTechSupport))
		{
			PresetCurrencyWidget.ChargeCurrency(Currency.RPiece);
			return;
		}
		if (_estimate.HasValue)
		{
			string text = T._("<em>견적서</em>를 다시 요청하시겠습니까?");
			if (HasWorthfulEstimate())
			{
				text = T._("[B82E2EFF]삭제[-]될 견적서에 현재보다 [B82E2EFF]높은 레벨의 속성[-]이 존재합니다.\n") + text;
			}
			UIManager.MessageBox.Show(text, T._("<alert_icon/> 이전 견적서는 삭제됩니다."), delegate(bool ok)
			{
				if (ok)
				{
					RequestNewEstimate();
				}
			}, GetEstimateButtonText(reissue: true));
			return;
		}
		UIManager.MessageBox.Show(T._("<em>견적서</em>를 요청하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				RequestNewEstimate();
			}
		}, GetEstimateButtonText(reissue: false));
	}

	private void RequestNewEstimate()
	{
		if (_currentState == WorkState.Idle && _target.Item != null)
		{
			_currentState = WorkState.RequestEstimate;
			ShowLoadingRing();
			GameSystem<TechSupportSystem>.Instance().RequestNewEstimate(_propKey, _target, GetLockedTags());
		}
	}

	private bool HasUnlockedTags()
	{
		return _tagItems.FirstOrDefault((TechSupportTag t) => !t.IsLocked) != null;
	}

	private int GetLockedTagsCount()
	{
		return _tagItems.Count((TechSupportTag t) => t.IsLocked);
	}

	private string[] GetLockedTags()
	{
		return (from t in _tagItems
			where t.IsLocked
			select t.TagId).ToArray();
	}

	private void ShowRequestedResult()
	{
		_currentState = WorkState.ShowResult;
		float delay = 0f;
		if (_estimate.HasValue)
		{
			delay = SetTagItemsToFinished(_tagItems, _estimate.Value, _showTagFinishDelay);
		}
		KUtility.DelayedCall(this, delegate
		{
			if (_currentState == WorkState.ShowResult)
			{
				RefreshButtonAssign(change: true);
				RefreshEstimate();
				RefreshButtonsText();
				HideLoadingRing();
				_currentState = WorkState.Idle;
			}
		}, delay);
	}

	private bool HasWorthfulEstimate()
	{
		if (_reformSlot.HasValue && _estimate.HasValue)
		{
			Messages.Tag[] tags = _reformSlot.Value.Tags;
			for (int i = 0; i < tags.Length; i++)
			{
				Messages.Tag tag = tags[i];
				int level = tag.Level;
				int level2 = TechSupportTag.GetTag(_estimate.Value.Tags, tag.Id).Level;
				bool? flag = TagYaml.IsTagImproved(tag.Id, level, level2);
				if (flag.HasValue && flag.Value)
				{
					return true;
				}
			}
		}
		return false;
	}

	private string GetEstimateButtonText(bool reissue, bool hideCost = false)
	{
		string text = ((!reissue) ? T._("견적서 요청") : T._("재요청"));
		if (hideCost)
		{
			return text;
		}
		return text + "  " + GetTotalCostText();
	}

	private string GetTotalCostText()
	{
		if (_yamlTechSupport != null && HasUnlockedTags())
		{
			string text = Durango.Logic.Item.Inventory.CurrencyFormat(_yamlTechSupport.RandomNumberPiece, Currency.RPiece);
			string techSupportCostText = GetTechSupportCostText(_requestCount, GetLockedTagsCount());
			return "[preset=round_box?" + text + "   " + techSupportCostText + "]";
		}
		return string.Empty;
	}

	private void ShowLoadingRing()
	{
		_issueButton.ShowLoadingRing(show: true);
		_nowLoading = true;
		RefreshButtonsEnableState();
	}

	private void HideLoadingRing()
	{
		_issueButton.ShowLoadingRing(show: false);
		_closeButton.ShowLoadingRing(show: false);
		_nowLoading = false;
		RefreshButtonsEnableState();
	}

	private static void AddTechSupportTag(ListObjectPool<TechSupportTag> tagItems, ReformSlot reformSlot, [NotNull] ReformTechSupport yamlTechSupport)
	{
		int num = 0;
		Messages.Tag[] tags = reformSlot.Tags;
		for (int i = 0; i < tags.Length; i++)
		{
			Messages.Tag tag = tags[i];
			TechSupportTag next = tagItems.GetNext();
			next.ShowSeperator(num++ > 0);
			next.SetBeforeOnly(tag.Id, tag.Level, reformSlot.TagRareness.Get(tag.Id, (TagLevelRareness)0), TechSupportTag.GetMaxLevelFromTechSupport(tag.Id, yamlTechSupport), hideAfterText: false);
		}
	}

	private static void AddTechSupportTagWithResult(ListObjectPool<TechSupportTag> tagItems, ReformSlot reformSlot, TechSupportEstimate estimate, [NotNull] ReformTechSupport yamlTechSupport)
	{
		int num = 0;
		Messages.Tag[] tags = reformSlot.Tags;
		for (int i = 0; i < tags.Length; i++)
		{
			Messages.Tag tag = tags[i];
			TechSupportTag next = tagItems.GetNext();
			next.ShowSeperator(num++ > 0);
			next.SetAll(tag.Id, tag.Level, reformSlot.TagRareness.Get(tag.Id, (TagLevelRareness)0), TechSupportTag.GetTag(estimate.Tags, tag.Id).Level, estimate.TagRareness.Get(tag.Id, (TagLevelRareness)0), TechSupportTag.GetMaxLevelFromTechSupport(tag.Id, yamlTechSupport));
		}
	}

	private static float SetTagItemsToFinished(ListObjectPool<TechSupportTag> tagItems, TechSupportEstimate estimate, float delay)
	{
		float num = delay;
		for (int i = 0; i < tagItems.Count; i++)
		{
			TechSupportTag techSupportTag = tagItems.Get<TechSupportTag>(i);
			if (!(techSupportTag == null))
			{
				if (techSupportTag.IsLocked)
				{
					techSupportTag.UpdateAfter(TechSupportTag.GetTag(estimate.Tags, techSupportTag.TagId).Level, estimate.TagRareness.Get(techSupportTag.TagId, (TagLevelRareness)0));
					continue;
				}
				techSupportTag.UpdateToFinished(TechSupportTag.GetTag(estimate.Tags, techSupportTag.TagId).Level, estimate.TagRareness.Get(techSupportTag.TagId, (TagLevelRareness)0), num);
				num += delay;
			}
		}
		return num;
	}

	private static string GetTechSupportCostText(int requestedCount, int lockedTagsCount)
	{
		Yaml.Cost reformTechSupportEstimate = Singleton<CostsYaml>.Instance.ReformTechSupportEstimate;
		reformTechSupportEstimate.SetAmountParams(new KeyValuePair<string, object>("request_count", requestedCount), new KeyValuePair<string, object>("locked_tags_count", lockedTagsCount));
		return Durango.Logic.Item.Inventory.CurrencyFormat(reformTechSupportEstimate.GetAmount(), reformTechSupportEstimate.Currency);
	}

	private static bool NotEnoughRandomPieces([NotNull] ReformTechSupport yamlTechSupport)
	{
		long num = yamlTechSupport.RandomNumberPiece;
		return InventorySystem.Wallet.GetBalance(Currency.RPiece) < num;
	}

	private void TechSupportSystem_EstimateUpdated(string itemId, TechSupportEstimateResult? result)
	{
		if (_currentState == WorkState.RequestEstimate)
		{
			if (_target.Item != null && _target.Item.Id == itemId && result.HasValue && result.Value.Estimate.Index == _target.ReformSlotIndex)
			{
				_requestCount = result.Value.RequestCount;
				_estimate = result.Value.Estimate;
				ShowRequestedResult();
			}
			else
			{
				Hide();
			}
		}
	}

	private void TagItem_LockButtonClicked(TechSupportTag tagItem)
	{
		if (_currentState == WorkState.Idle)
		{
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
			tagItem.IsLocked = !tagItem.IsLocked;
			RefreshButtonsText();
			RefreshButtonsEnableState();
		}
	}

	private void PrimaryButton_Clicked()
	{
		if (_buttonAssignChanged)
		{
			IssueEstimate();
		}
		else
		{
			Hide();
		}
	}

	private void SecondaryButton_Clicked()
	{
		if (_buttonAssignChanged)
		{
			Hide();
		}
		else
		{
			IssueEstimate();
		}
	}
}
