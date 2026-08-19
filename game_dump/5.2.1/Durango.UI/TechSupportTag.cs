using System;
using System.Linq;
using Durango.Logic.Item;
using Durango.UI.Control;
using JetBrains.Annotations;
using Messages;
using Shared.Item;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class TechSupportTag : MonoBehaviour
{
	public enum State
	{
		Idle,
		FinishedForNormal,
		FinishedForRare,
		FinishedForSuperRare
	}

	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private GameObject _seperator;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private UISprite _bgForTextName;

	[SerializeField]
	private UISpriteLabel _textBefore;

	[SerializeField]
	private GameObject _arrows;

	[SerializeField]
	private UILabel _textAfter;

	[SerializeField]
	private UISprite _iconUpDown;

	[EnumList(typeof(State), false, 0, -1)]
	[SerializeField]
	private GameObject[] _widgetsForAfter;

	[SerializeField]
	private UIWidget _lockButton;

	[SerializeField]
	private UISprite _iconLock;

	[SerializeField]
	private SpriteData _spriteUp;

	[SerializeField]
	private SpriteData _spriteDown;

	private bool _initialized;

	private int _beforeLevel;

	private int _afterLevel;

	private TagLevelRareness _beforeRareness;

	private TagLevelRareness _afterRareness;

	private int _maxLevel;

	private State _currentState;

	private bool _isLocked;

	public UIWidget Widget => _widget;

	public string TagId { get; private set; }

	public bool IsLocked
	{
		get
		{
			return _isLocked;
		}
		set
		{
			_isLocked = value;
			RefreshLockState();
		}
	}

	public event Action<TechSupportTag> LockButtonClicked;

	private void Update()
	{
		if (_currentState != 0)
		{
			GameObject gameObject = _widgetsForAfter[(int)_currentState];
			if (gameObject != null && !gameObject.activeSelf)
			{
				SetCurrentState(State.Idle);
			}
		}
	}

	public void Init()
	{
		if (!_initialized)
		{
			if (_lockButton != null)
			{
				UIEventListener uIEventListener = UIEventListener.Get(_lockButton.gameObject);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(LockButton_Clicked));
			}
			_initialized = true;
		}
	}

	public void SetBeforeOnly(string id, int before, TagLevelRareness beforeRareness, int max, bool hideAfterText)
	{
		TagId = id;
		IsLocked = false;
		_beforeLevel = before;
		_afterLevel = -1;
		_beforeRareness = beforeRareness;
		_afterRareness = TagLevelRareness.Invalid;
		_maxLevel = max;
		string maxLv = ((!hideAfterText) ? null : GetMaxLevelText(_maxLevel));
		SetNameText(TagData.GetTagName(id), beforeRareness);
		SetBeforeText(GetColoredLevelText(_beforeLevel, beforeRareness), maxLv);
		RefreshAfterText(hideAfterText);
		SetCurrentState(State.Idle);
	}

	public void SetAll(string id, int before, TagLevelRareness beforeRareness, int after, TagLevelRareness afterRareness, int max = -1)
	{
		TagId = id;
		IsLocked = false;
		_beforeLevel = before;
		_afterLevel = after;
		_beforeRareness = beforeRareness;
		_afterRareness = afterRareness;
		_maxLevel = max;
		SetNameText(TagData.GetTagName(id), beforeRareness);
		SetBeforeText(GetColoredLevelText(_beforeLevel, beforeRareness));
		RefreshAfterText();
		SetCurrentState(State.Idle);
	}

	public void UpdateAfter(int after, TagLevelRareness afterRareness)
	{
		_afterLevel = after;
		_afterRareness = afterRareness;
		RefreshAfterText();
	}

	public void UpdateToFinished(int after, TagLevelRareness afterRareness, float delay)
	{
		KUtility.DelayedCall(this, delegate
		{
			_afterLevel = after;
			_afterRareness = afterRareness;
			State finishedState = GetFinishedState(_afterRareness);
			SoundManager.PlayEvent((finishedState != State.FinishedForRare && finishedState != State.FinishedForSuperRare) ? "ui_reform_result" : "ui_reform_result_srank");
			RefreshAfterText();
			SetCurrentState(finishedState);
		}, delay);
	}

	public void ShowSeperator(bool show)
	{
		SetActiveWidget(_seperator, show);
	}

	public static Messages.Tag GetTag(Messages.Tag[] tags, string id)
	{
		return tags.FirstOrDefault((Messages.Tag tag) => tag.Id == id);
	}

	public static int GetMaxLevelFromTechSupport(string tagId, ReformTechSupport techSupport)
	{
		return techSupport.Tags.Get(tagId)?.MaxLevel ?? 0;
	}

	private void SetCurrentState(State state)
	{
		_currentState = state;
		for (int i = 0; i < _widgetsForAfter.Length; i++)
		{
			SetActiveWidget(_widgetsForAfter[i], i == (int)_currentState);
		}
	}

	private void SetNameText(string text, TagLevelRareness rareness)
	{
		_textName.text = GetTagNameText(text, rareness);
		_bgForTextName.width = (int)_textName.printedSize.x + 20;
	}

	private void SetBeforeText([NotNull] string lv, string maxLv = null)
	{
		_textBefore.text = ((!string.IsNullOrEmpty(maxLv)) ? (lv + " " + maxLv) : lv);
	}

	private void SetAfterText([CanBeNull] string lv, string maxLv = null)
	{
		_textAfter.text = (string.IsNullOrEmpty(lv) ? string.Empty : ((!string.IsNullOrEmpty(maxLv)) ? (lv + " " + maxLv) : lv));
	}

	private void RefreshAfterText(bool hideAfterText = false)
	{
		bool? upDownFlag = null;
		if (hideAfterText)
		{
			SetAfterText(null);
		}
		else if (IsLocked)
		{
			SetAfterText(GetColoredLevelText(_beforeLevel, _beforeRareness), GetMaxLevelText(_maxLevel));
		}
		else if (_afterLevel != -1)
		{
			upDownFlag = TagYaml.IsTagImproved(TagId, _beforeLevel, _afterLevel);
			SetAfterText(GetColoredLevelText(_afterLevel, _afterRareness), GetMaxLevelText(_maxLevel));
		}
		else
		{
			SetAfterText("??", GetMaxLevelText(_maxLevel));
		}
		RefreshUpDownArrows(upDownFlag);
		SetActiveWidget(_arrows, !string.IsNullOrEmpty(_textAfter.text));
	}

	private void RefreshLockState()
	{
		if (_iconLock != null)
		{
			_iconLock.color = ((!IsLocked) ? PresetColor.UILightGray : PresetColor.UIYellow);
		}
		RefreshAfterText();
	}

	private void RefreshUpDownArrows(bool? upDownFlag)
	{
		if (upDownFlag.HasValue)
		{
			_iconUpDown.gameObject.SetActive(value: true);
			if (upDownFlag.Value)
			{
				_spriteUp.Set(_iconUpDown);
			}
			else
			{
				_spriteDown.Set(_iconUpDown);
			}
		}
		else
		{
			_iconUpDown.gameObject.SetActive(value: false);
		}
	}

	private static string GetTagNameText(string name, TagLevelRareness rareness)
	{
		if ((uint)(rareness - 10) > 2u)
		{
			return "[c=ui_light_gray]" + name + "[-]";
		}
		return "<em>" + name + "</em>";
	}

	private static string GetColoredLevelText(int level, TagLevelRareness rareness)
	{
		string text = LocalizeUtil.FormatLevel(level);
		switch (rareness)
		{
		default:
			return "[c=ui_white]" + text + "[-]";
		case TagLevelRareness.Normal:
			return "<em>" + text + "</em>";
		case TagLevelRareness.Rare:
		case TagLevelRareness.SuperRare:
			return "[c=ui_light_green]" + text + "[-]";
		}
	}

	private static string GetMaxLevelText(int max)
	{
		if (max != -1)
		{
			return $"[FFFFFF7F][size=22]/ {max}[/size][-]";
		}
		return null;
	}

	private static State GetFinishedState(TagLevelRareness rareness)
	{
		return rareness switch
		{
			TagLevelRareness.Rare => State.FinishedForRare, 
			TagLevelRareness.SuperRare => State.FinishedForSuperRare, 
			_ => State.FinishedForNormal, 
		};
	}

	private static void SetActiveWidget(GameObject gameObject, bool show)
	{
		if (gameObject != null)
		{
			gameObject.SetActive(show);
		}
	}

	private void LockButton_Clicked(GameObject go)
	{
		if (this.LockButtonClicked != null)
		{
			this.LockButtonClicked(this);
		}
	}
}
