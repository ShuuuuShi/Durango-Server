using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using Shared.Quest;
using UnityEngine;

namespace Durango.UI;

public class QuestBottomRewardWidget : MonoBehaviour
{
	public Action<GameObject, string, int> QuestRewardRequested;

	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private UILabel _targetScoreLabel;

	[SerializeField]
	private UIWidget _buttonRoot;

	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private UILabel _supLabel;

	[SerializeField]
	private UISprite _checkedSprite;

	[SerializeField]
	private UISprite _frameSprite;

	[SerializeField]
	private UISprite _arrowSprite;

	[SerializeField]
	private GameObject _notification;

	[SerializeField]
	private GlitteringDots _glitteringEffect;

	[SerializeField]
	private GameObject _goodEffect;

	private QuestScoreReward _scoreReward;

	private string _category;

	private EffectWidget _effectObject;

	private float _defaultScoreX;

	private float _defaultButtonX;

	private ReceiveRewardsPopup.ItemArgument _current;

	public UIWidget Widget => _widget;

	private void Awake()
	{
		_defaultScoreX = _targetScoreLabel.transform.localPosition.x;
		_defaultButtonX = _buttonRoot.transform.localPosition.x;
		_notification.SetActive(value: false);
		_glitteringEffect.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_buttonRoot.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick));
	}

	public void SetData(string category, QuestScoreReward reward, bool isGood, bool isHide, bool immediate)
	{
		_category = category;
		_scoreReward = reward;
		if (isHide || reward.State == QuestScoreRewardState.Invalid)
		{
			isGood = false;
			_buttonRoot.alpha = 0f;
		}
		else
		{
			_buttonRoot.alpha = 1f;
		}
		SetState(reward.State, immediate);
		_targetScoreLabel.text = reward.QuestScore.ToString();
		_goodEffect.gameObject.SetActive(isGood);
		List<ReceiveRewardsPopup.ItemArgument> list = new List<ReceiveRewardsPopup.ItemArgument>();
		ReceiveRewardsPopup.AddRewardedItems(list, reward.Reward, isBonus: false);
		if (list.Count == 0)
		{
			_iconTexture.SetIcon(string.Empty);
			_countLabel.text = string.Empty;
			_supLabel.text = string.Empty;
			return;
		}
		ReceiveRewardsPopup.ItemArgument itemArgument = (_current = list.First());
		if (itemArgument.IconColor.HasValue)
		{
			_iconTexture.SetIcon(itemArgument.Icon, itemArgument.IconColor);
		}
		else
		{
			_iconTexture.SetIcon(itemArgument.Icon, itemArgument.IconRTable, itemArgument.IconGTable, itemArgument.IconBTable);
		}
		_countLabel.text = itemArgument.Amount.ToString("N0", T.Culture);
		_supLabel.text = itemArgument.Sup;
	}

	public void PlayAnim(bool isHide, bool immediate = false)
	{
		float alpha = ((!isHide) ? 1f : 0f);
		Vector3 localPosition = _targetScoreLabel.transform.localPosition;
		localPosition.x = ((!isHide) ? 0f : (_targetScoreLabel.printedSize.x - 12f));
		localPosition.x += _defaultScoreX;
		Vector3 localPosition2 = _buttonRoot.transform.localPosition;
		localPosition2.x = ((!isHide) ? 0f : (_targetScoreLabel.printedSize.x - 12f));
		localPosition2.x += _defaultButtonX;
		if (immediate)
		{
			_targetScoreLabel.transform.localPosition = localPosition;
			_buttonRoot.alpha = alpha;
			_buttonRoot.transform.localPosition = localPosition2;
			return;
		}
		TweenPosition tweenPosition = TweenPosition.Begin(_targetScoreLabel.gameObject, 1f, localPosition);
		tweenPosition.method = UITweener.Method.EaseInOut;
		tweenPosition.delay = 1f;
		tweenPosition.PlayForward();
		tweenPosition = TweenPosition.Begin(_buttonRoot.gameObject, 1f, localPosition2);
		tweenPosition.method = UITweener.Method.EaseInOut;
		tweenPosition.delay = 1f;
		tweenPosition.PlayForward();
		TweenAlpha tweenAlpha = TweenAlpha.Begin(_buttonRoot.gameObject, 0.8f, alpha);
		tweenAlpha.method = UITweener.Method.EaseOut;
		tweenAlpha.delay = 1f;
		tweenAlpha.PlayForward();
	}

	private void SetState(QuestScoreRewardState state, bool immediate)
	{
		bool flag = state == QuestScoreRewardState.Available;
		bool flag2 = state == QuestScoreRewardState.Taken;
		Color color = ((!flag) ? PresetColor.QuestGray : Color.white);
		_frameSprite.color = color;
		_arrowSprite.color = color;
		_targetScoreLabel.color = color;
		AddButtonEffect(flag ? PresetButton.Effect.Emphasis : PresetButton.Effect.None);
		_notification.SetActive(flag);
		if (flag2)
		{
			if (immediate)
			{
				_checkedSprite.transform.localScale = Vector3.one;
			}
			else if (!_checkedSprite.gameObject.activeSelf)
			{
				_checkedSprite.transform.localScale = new Vector3(1.33f, 1.33f, 1.33f);
				TweenScale tweenScale = TweenScale.Begin(_checkedSprite.gameObject, 0.33f, Vector3.one);
				tweenScale.method = UITweener.Method.EaseOut;
				tweenScale.PlayForward();
			}
		}
		_checkedSprite.gameObject.SetActive(flag2);
	}

	private void OnClick(GameObject go)
	{
		if (_scoreReward.State != QuestScoreRewardState.Available)
		{
			if (_current.ItemPrototype.HasValue)
			{
				KeyValuePair<string, int> value = _current.ItemPrototype.Value;
				ItemInfoTooltip itemInfoTooltip = UIManager.Popup.Tooltip<ItemInfoTooltip>();
				itemInfoTooltip.Set(value.Key, value.Value);
				itemInfoTooltip.Direction = TooltipBase.TooltipDirection.Vertical;
				itemInfoTooltip.Show();
				return;
			}
			string text = _current.GetSubText();
			if (!string.IsNullOrEmpty(text))
			{
				if (!string.IsNullOrEmpty(_current.Icon))
				{
					text = $"[icon={_current.Icon}] {text}";
				}
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				widgetTooltipControl.Set(_current.Title, text, 300);
				widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
				widgetTooltipControl.Show(5f);
			}
		}
		else if (QuestRewardRequested != null)
		{
			QuestRewardRequested(go, _category, _scoreReward.QuestScore);
		}
	}

	private void AddButtonEffect(PresetButton.Effect effect)
	{
		if (effect == PresetButton.Effect.None)
		{
			if (_effectObject != null)
			{
				_effectObject.gameObject.SetActive(value: false);
			}
			return;
		}
		if (_effectObject != null)
		{
			_effectObject.gameObject.SetActive(value: true);
			_glitteringEffect.Play();
			return;
		}
		EffectWidget effect2 = SelectableButtonStyle.GetEffect(effect);
		if (!(effect2 == null))
		{
			_effectObject = _buttonRoot.gameObject.AddChild(effect2.gameObject).GetComponent<EffectWidget>();
			if (_effectObject.gameObject.layer != base.gameObject.layer)
			{
				NGUITools.SetLayer(_effectObject.gameObject, base.gameObject.layer);
			}
			_effectObject.width = _buttonRoot.width;
			_effectObject.height = _buttonRoot.height;
			_effectObject.depth = _widget.depth + 100;
			_glitteringEffect.transform.parent = _effectObject.transform;
			_glitteringEffect.gameObject.SetActive(value: true);
			_glitteringEffect.Play();
		}
	}
}
