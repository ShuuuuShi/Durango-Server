using System;
using System.Collections.Generic;
using System.Text;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Ability;
using Shared.Animal;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class DomesticationRewardPopup : TooltipBase
{
	private const float MaximumInferorStatScrollHeightRatio = 3.2f;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private TweenerPlayer _rankEffect;

	[SerializeField]
	private TweenerPlayer _rankSpecialEffect;

	[SerializeField]
	private UISprite _petThumbnail;

	[SerializeField]
	private UILabel _itemName;

	[SerializeField]
	private UILabel _itemDesc;

	[SerializeField]
	private VerticalLayoutWidget _priorStatus;

	[SerializeField]
	private UILabel _tagsTitle;

	[SerializeField]
	private UIWidget _tagsArea;

	[SerializeField]
	private KScrollView _tagsScroll;

	[SerializeField]
	private UIWidget _inferiorStatusArea;

	[SerializeField]
	private KScrollView _inferiorStatusScroll;

	[SerializeField]
	private UIWidget _buttonWidgets;

	[SerializeField]
	private SelectableButton _cancelButton;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private RectLayoutComponent _layout;

	private readonly Derived[] _priorAbilities = new Derived[3]
	{
		Derived.InventoryCapacity,
		Derived.LifeMax,
		Derived.Speed
	};

	private readonly Derived[] _inferiorAbilities = new Derived[4]
	{
		Derived.LifeSpan,
		Derived.Attack,
		Derived.Defense,
		Derived.Accuracy
	};

	private int _animalType;

	private int _level;

	private DomesticationResult _result;

	private string _cancelText;

	private string _confirmText;

	private Action _confirmAction;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_titleLabel.text = T._("길들이기에 성공하였습니다!");
		_cancelButton.Clicked = Hide;
		_tagsTitle.text = T._("속성 발견");
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(OnConfirm));
		ResetArguments();
	}

	protected override void OnHide()
	{
		base.OnHide();
		ResetArguments();
	}

	private void ResetArguments()
	{
		_animalType = 0;
		_level = 0;
		_cancelText = null;
		_confirmText = null;
		_confirmAction = null;
	}

	public DomesticationRewardPopup SetType(int type)
	{
		_animalType = type;
		return this;
	}

	public DomesticationRewardPopup SetLevel(int level)
	{
		_level = level;
		return this;
	}

	public DomesticationRewardPopup SetResult(DomesticationResult result)
	{
		_result = result;
		return this;
	}

	public DomesticationRewardPopup SetCancelText(string text)
	{
		_cancelText = text;
		return this;
	}

	public DomesticationRewardPopup SetConfirm(string text, Action action)
	{
		_confirmText = text;
		_confirmAction = action;
		return this;
	}

	protected override void FillData()
	{
		Animal animal = SingletonDict<int, Animal>.Get(_animalType);
		_rankEffect.gameObject.SetActive(value: false);
		_rankSpecialEffect.gameObject.SetActive(value: false);
		if (_result.Rank.HasValue)
		{
			PetRank value = _result.Rank.Value;
			string eventName;
			TweenerPlayer tweenerPlayer;
			if (value == PetRank.S)
			{
				eventName = "ui_tame_result_success_srank";
				tweenerPlayer = _rankSpecialEffect;
			}
			else
			{
				eventName = "ui_tame_result_success";
				tweenerPlayer = _rankEffect;
			}
			SoundManager.PlayEvent(eventName);
			Transform transform = KUtility.FindTransformByName(tweenerPlayer.gameObject, "RankLabel");
			if (transform != null)
			{
				UILabel component = transform.GetComponent<UILabel>();
				if (component != null)
				{
					component.text = _result.Rank.ToString();
				}
			}
			tweenerPlayer.gameObject.SetActive(value: true);
			tweenerPlayer.Play();
		}
		_itemName.text = ((animal != null) ? animal.Name.ToString() : string.Empty);
		using (Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop())
		{
			StringBuilder value2 = reusable.Value;
			if (_level > 0)
			{
				if (value2.Length > 0)
				{
					value2.Append("  <bar/>  ");
				}
				value2.Append(LocalizeUtil.FormatLevel(_level));
			}
			_itemDesc.text = value2.ToString();
		}
		_petThumbnail.spriteName = ((animal != null) ? animal.Portrait : string.Empty);
		ShowPriorStat(_result);
		ShowTags(_result);
		ShowInferiorStat(_result);
		if (_cancelButton.gameObject.SetActiveAnd(!string.IsNullOrEmpty(_cancelText)))
		{
			_cancelButton.Text = _cancelText;
		}
		if (_confirmButton.gameObject.SetActiveAnd(!string.IsNullOrEmpty(_confirmText)))
		{
			_confirmButton.Text = _confirmText;
		}
		_buttonWidgets.gameObject.SetActive(_confirmButton.gameObject.activeSelf || _cancelButton.gameObject.activeSelf);
	}

	protected override void UpdateLayout()
	{
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	protected override void OnTryConfirmOnModal()
	{
		OnConfirm();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}

	private void OnConfirm()
	{
		if (_confirmAction != null)
		{
			_confirmAction();
		}
		Hide();
	}

	private void ShowPriorStat(DomesticationResult result)
	{
		List<Pair<string, string>> priorStatData = new List<Pair<string, string>>();
		int i = 0;
		for (int size = KUtility.GetSize(_priorAbilities); i < size; i++)
		{
			Derived type = _priorAbilities[i];
			priorStatData.Add(GetStatData(result, type));
		}
		List<Pair<string, string>> list = priorStatData;
		string item = T._("성장 횟수");
		int? milestoneCount = result.MilestoneCount;
		list.Add(new Pair<string, string>(item, (!milestoneCount.HasValue) ? T._("없음") : result.MilestoneCount.Value.ToString()));
		_priorStatus.SetGrids(priorStatData, delegate(Pair<string, string> data, DomesticationStatDiffWidget obj, int idx)
		{
			obj.Set(data.Item1, data.Item2, idx != priorStatData.Count);
		});
	}

	private void ShowTags(DomesticationResult result)
	{
		Dictionary<string, int> tags = result.Tags;
		if (tags == null || tags.Count == 0)
		{
			_tagsArea.gameObject.SetActive(value: false);
			return;
		}
		_tagsArea.gameObject.SetActive(value: true);
		_tagsScroll.Nodes.BeginLoad();
		foreach (string key in tags.Keys)
		{
			Yaml.Tag tag = SingletonDict<string, Yaml.Tag>.Instance.Get(key);
			if (tag != null)
			{
				UILabel component = _tagsScroll.Nodes.GetNext().GetComponent<UILabel>();
				component.text = T._("{0} {1:lv:}", tag.Name, tags[key]);
			}
		}
		_tagsScroll.Nodes.EndLoad();
	}

	private void ShowInferiorStat(DomesticationResult result)
	{
		_inferiorStatusScroll.Nodes.BeginLoad();
		for (int i = 0; i < _inferiorAbilities.Length; i++)
		{
			DomesticationStatDiffWidget component = _inferiorStatusScroll.Nodes.GetNext().GetComponent<DomesticationStatDiffWidget>();
			Pair<string, string> statData = GetStatData(result, _inferiorAbilities[i]);
			component.Set(statData.Item1, statData.Item2, showSeperator: false);
		}
		_inferiorStatusScroll.Nodes.EndLoad();
		int height = _inferiorStatusScroll.Nodes.BaseObject.GetComponent<UIWidget>().height;
		_inferiorStatusArea.height = (int)((float)height * Mathf.Min(_inferiorAbilities.Length, 3.2f));
	}

	private static Pair<string, string> GetStatData(DomesticationResult result, Derived type)
	{
		float num = result.Stat[type];
		float num2 = result.Original[type];
		string text;
		string arg;
		if (type == Derived.LifeSpan)
		{
			text = TimedeltaFormatter.Format(num);
			arg = TimedeltaFormatter.Format(num - num2);
		}
		else
		{
			text = Mathf.RoundToInt(num).ToString();
			arg = Mathf.RoundToInt(num - num2).ToString("+0;-#");
		}
		return new Pair<string, string>(type.GetName(), (num != num2) ? $"{text} <em>{arg}</em>" : text);
	}
}
