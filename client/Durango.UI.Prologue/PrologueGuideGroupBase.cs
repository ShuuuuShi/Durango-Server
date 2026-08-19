using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Durango.Prologue;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Prologue;

public abstract class PrologueGuideGroupBase : UIBase
{
	private static bool _isFirstTime = true;

	[SerializeField]
	private GameObject _clickObject;

	[SerializeField]
	private GameObject _guideGroup;

	[SerializeField]
	private UILabel _msgLabel;

	[SerializeField]
	private UILabel _systemMsgLabel;

	[SerializeField]
	private GameObject _portraitsGroup;

	[SerializeField]
	private TextAsset _prologueGuideFile;

	private ICoroutineBinder _guideExecutionSequenceBinder;

	[SerializeField]
	private SoundEventType _itemGetSound;

	private bool _isSystemMsg;

	private List<string> _msgTokenList;

	private int _currentMsgIndex;

	private float _currentMsgShowTime;

	private float _msgDuration;

	private readonly List<GameObject> _portraits = new List<GameObject>();

	protected bool DoNotFinishByTouch;

	private bool _hidableCaption;

	protected readonly Regex NameTagToken = new Regex(".+?:(.+)");

	private readonly Regex _portraitToken = new Regex("\\[#(.+?)\\]");

	private uint _voiceInstanceId;

	private PrologueGuideSystem.PrologueGuideOnFinish _lastOnFinish;

	protected TypeWriterEffect Typewriter;

	private bool _readyToNext;

	public TextAsset PrologueGuideFile => _prologueGuideFile;

	protected virtual void Awake()
	{
		if (_clickObject != null)
		{
			UIEventListener.Get(_clickObject).onPress = OnClickObjectPressed;
		}
		base.gameObject.SetActive(value: false);
		int childCount = base.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			base.transform.GetChild(i).gameObject.SetActive(value: false);
		}
		_portraits.Clear();
		int childCount2 = _portraitsGroup.transform.childCount;
		for (int j = 0; j < childCount2; j++)
		{
			GameObject gameObject = _portraitsGroup.transform.GetChild(j).gameObject;
			_portraits.Add(gameObject);
			gameObject.SetActive(value: false);
			UIEventListener.Get(gameObject).onClick = delegate
			{
				if (DoNotFinishByTouch)
				{
					if (_hidableCaption)
					{
						HideCaptionOnly();
					}
				}
				else
				{
					ShowNextGuideMsg();
				}
			};
		}
		Typewriter = _msgLabel.gameObject.AddComponent<TypeWriterEffect>();
		Typewriter.enabled = false;
		SoundManager.PrepareEvent(_itemGetSound);
	}

	protected void OnClickObjectPressed(GameObject go, bool pressed)
	{
		if (pressed)
		{
			_readyToNext = !Typewriter.enabled;
			Typewriter.SetFastFoward(fastFoward: true);
			return;
		}
		if (_readyToNext)
		{
			if (DoNotFinishByTouch)
			{
				if (_clickObject != null)
				{
					_clickObject.SetActive(value: false);
				}
				if (_hidableCaption)
				{
					HideCaptionOnly();
				}
				return;
			}
			ShowNextGuideMsg();
		}
		Typewriter.SetFastFoward(fastFoward: false);
	}

	private void HideCaptionOnly()
	{
		DeactivateAllPortraits();
		Close();
	}

	protected virtual void DeactivateAllPortraits()
	{
		int count = _portraits.Count;
		for (int i = 0; i < count; i++)
		{
			_portraits[i].SetActive(value: false);
		}
	}

	private GameObject FindPortraitByName(string portrait)
	{
		if (string.IsNullOrEmpty(portrait))
		{
			return null;
		}
		int count = _portraits.Count;
		for (int i = 0; i < count; i++)
		{
			if (_portraits[i].name == portrait)
			{
				return _portraits[i];
			}
		}
		return null;
	}

	private void ActivatePortrait(string portraitName, bool deactivateOthers = true)
	{
		if (deactivateOthers)
		{
			DeactivateAllPortraits();
		}
		if (portraitName != null && !(portraitName == "None"))
		{
			Match match = NameTagToken.Match(portraitName);
			if (match.Success)
			{
				ActivateNameTag(match.Groups[1].Value);
			}
			GameObject gameObject = FindPortraitByName(portraitName);
			if (!(gameObject == null))
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	protected virtual void Update()
	{
		if (_msgTokenList != null && _msgDuration > 0f && Time.time - _currentMsgShowTime > _msgDuration)
		{
			ShowNextGuideMsg(byClick: false);
		}
	}

	private void SetUIActive(PrologueGuideSystem.MsgInfo msg, bool isSelectMsg)
	{
		_guideGroup.SetActive(!msg.IsSystem);
		_systemMsgLabel.gameObject.SetActive(msg.IsSystem);
		_msgLabel.gameObject.SetActive(!isSelectMsg);
		ActivatePortrait(msg.Portrait);
		if (!string.IsNullOrEmpty(msg.NameTag))
		{
			ActivateNameTag(msg.NameTag);
		}
		DoNotFinishByTouch = msg.DoNotFinishByTouch;
		_hidableCaption = msg.HidableCaption;
		OnSetUIActive(msg);
	}

	protected virtual void OnSetUIActive(PrologueGuideSystem.MsgInfo msg)
	{
	}

	protected abstract void ActivateNameTag(string nameTag, bool deactivateOthers = true);

	private List<string> GetGuideTokens(string tokenBase)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < 10; i++)
		{
			string text = tokenBase + "_" + i;
			if (LocalizeSystem.Has(text))
			{
				list.Add(text);
			}
		}
		return list;
	}

	public void ShowGuideMsg(string msgLocalizeKey, bool isSystemMsg, float msgDuration)
	{
		ShowGuideMsg(new PrologueGuideSystem.MsgInfo
		{
			LocalKey = msgLocalizeKey,
			IsSystem = isSystemMsg,
			MsgDuration = msgDuration
		});
	}

	public void ShowGuideMsg(PrologueGuideSystem.MsgInfo msg)
	{
		List<string> guideTokens = GetGuideTokens(msg.LocalKey);
		_msgDuration = msg.MsgDuration;
		if (guideTokens == null || guideTokens.Count == 0)
		{
			_msgTokenList = null;
			_currentMsgIndex = -1;
			DeactivateAllPortraits();
			Close();
			return;
		}
		Open();
		SetUIActive(msg, isSelectMsg: false);
		if (msg.PlaySnd)
		{
			SoundManager.PlayEvent(_itemGetSound);
		}
		_isSystemMsg = msg.IsSystem;
		_msgTokenList = guideTokens;
		_currentMsgIndex = -1;
		ShowNextGuideMsg(byClick: false);
	}

	public void ClearGuideMsg(bool wantClearDelayedMessage)
	{
		GameSystem<PrologueGuideSystem>.Instance().OnPreEndGuide();
		_currentMsgIndex = -1;
		_msgTokenList = null;
		_currentMsgShowTime = -1f;
		DeactivateAllPortraits();
		Close();
		if (_isFirstTime && !_isSystemMsg)
		{
			_isFirstTime = false;
		}
		if (wantClearDelayedMessage)
		{
			this.StopCoroutine(_guideExecutionSequenceBinder);
			_lastOnFinish = null;
		}
		DispathOnFinish();
	}

	private void DispathOnFinish()
	{
		if (_lastOnFinish != null)
		{
			this.StartCoroutine(ref _guideExecutionSequenceBinder, ExecutedDelayedGuide(_lastOnFinish));
		}
		_lastOnFinish = null;
	}

	private IEnumerator ExecutedDelayedGuide(PrologueGuideSystem.PrologueGuideOnFinish onFinish)
	{
		yield return new WaitForSeconds(onFinish.Delay);
		if (onFinish.CustomCmds != null)
		{
			GameSystem<PrologueGuideSystem>.Instance().DispatchCustomCmds(onFinish.CustomCmds);
		}
		if (onFinish.Next != null)
		{
			GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(onFinish.Next);
		}
	}

	public void SetOnFinishDisplayMsg(PrologueGuideSystem.PrologueGuideOnFinish onFinish)
	{
		if (_lastOnFinish != null)
		{
			DispathOnFinish();
		}
		_lastOnFinish = onFinish;
		if (_msgTokenList == null)
		{
			DispathOnFinish();
		}
	}

	public void ShowNextGuideMsg(bool byClick = true)
	{
		if (_msgTokenList == null)
		{
			return;
		}
		if (byClick)
		{
			StopGuideVoice();
		}
		_currentMsgIndex++;
		if (_currentMsgIndex >= _msgTokenList.Count)
		{
			ClearGuideMsg(wantClearDelayedMessage: false);
			return;
		}
		string text = _msgTokenList[_currentMsgIndex];
		string text2 = LocalizeSystem.Get(text);
		if (_isSystemMsg)
		{
			_systemMsgLabel.text = ConditionalText.Format(text2);
		}
		else
		{
			Match match = _portraitToken.Match(text2);
			if (match.Success)
			{
				string value = match.Groups[1].Value;
				ActivatePortrait(value);
				text2 = _portraitToken.Replace(text2, string.Empty);
			}
			SetGuideMsg(text2);
			PlayGuideVoice(text);
		}
		_currentMsgShowTime = Time.time;
	}

	protected virtual void SetGuideMsg(string msgTxt)
	{
		_msgLabel.text = ConditionalText.Format(msgTxt);
		_readyToNext = false;
		Typewriter.Reset();
		Typewriter.enabled = true;
		if (_clickObject != null)
		{
			_clickObject.SetActive(value: true);
		}
	}

	private void PlayGuideVoice(string token)
	{
		StopGuideVoice();
		string eventName = token.Substring(1);
		if (SoundManager.HasEvent(eventName))
		{
			_voiceInstanceId = SoundManager.PlayEvent(eventName, SoundPosition.Empty, exclusive: true);
		}
	}

	private void StopGuideVoice()
	{
		SoundManager.StopEvent(_voiceInstanceId);
		_voiceInstanceId = 0u;
	}
}
