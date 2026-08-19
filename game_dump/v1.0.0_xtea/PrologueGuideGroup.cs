using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PrologueGuideGroup : UIBase
{
	public enum GuideMsgOption
	{
		Normal = 0,
		PortraitBegin = 1,
		PortraitGirl = 1,
		Speaker = 2,
		Necklace = 3,
		Encyclopedia = 4,
		Clerk = 5
	}

	public delegate void ChangeGuideDelegate(string value, bool isSystem);

	[SerializeField]
	private GameObject _guideTexBack;

	[SerializeField]
	private GameObject _guideGroup;

	[SerializeField]
	private UILabel _msgLabel;

	[SerializeField]
	private UILabel _systemMsgLabel;

	[SerializeField]
	private UILabel _nameTagLabel;

	[SerializeField]
	private GameObject _portraitsGroup;

	[SerializeField]
	private TextAsset _prologueGuideFile;

	[SerializeField]
	private GameObject _touchHand;

	private bool _isSystemMsg;

	private List<string> _msgList;

	private int _currentMsgIndex;

	private float _currentMsgShowTime;

	private float _msgDuration;

	private static bool _isFirstTime = true;

	private List<GameObject> _portraits = new List<GameObject>();

	private bool _doNotFinishByTouch;

	private bool _hidableCaption;

	private readonly Regex _nameTagToken = new Regex(".+?:(.+)");

	private PrologueGuideSystem.PrologueGuideOnFinish _lastOnFinish;

	private readonly Regex _portraitToken = new Regex("\\[#(.+?)\\]");

	public TextAsset PrologueGuideFile => _prologueGuideFile;

	public event ChangeGuideDelegate OnChangeGuideMsg;

	private void Awake()
	{
		UIEventListener.Get(_guideTexBack).onClick = delegate
		{
			if (_doNotFinishByTouch)
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
		((Component)this).gameObject.SetActive(false);
		int childCount = ((Component)this).transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			((Component)((Component)this).transform.GetChild(i)).gameObject.SetActive(false);
		}
		_portraits.Clear();
		int childCount2 = _portraitsGroup.transform.childCount;
		for (int j = 0; j < childCount2; j++)
		{
			GameObject gameObject = ((Component)_portraitsGroup.transform.GetChild(j)).gameObject;
			_portraits.Add(gameObject);
			gameObject.SetActive(false);
			UIEventListener.Get(gameObject).onClick = delegate
			{
				if (_doNotFinishByTouch)
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
	}

	private void HideCaptionOnly()
	{
		DeactivateAllPortraits();
		Close();
	}

	private void DeactivateAllPortraits()
	{
		int count = _portraits.Count;
		for (int i = 0; i < count; i++)
		{
			_portraits[i].SetActive(false);
		}
		((Component)_nameTagLabel).gameObject.SetActive(false);
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
			if (((Object)_portraits[i]).name == portrait)
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
			Match match = _nameTagToken.Match(portraitName);
			if (match.Success)
			{
				ActivateNameTag(match.Groups[1].Value);
			}
			GameObject val = FindPortraitByName(portraitName);
			if (!((Object)(object)val == (Object)null))
			{
				val.SetActive(true);
			}
		}
	}

	private void Update()
	{
		if (_msgList != null && _msgDuration > 0f && Time.time - _currentMsgShowTime > _msgDuration)
		{
			ShowNextGuideMsg();
		}
	}

	private void SetUIActive(PrologueGuideSystem.MsgInfo msg, bool isSelectMsg)
	{
		_guideGroup.SetActive(!msg.IsSystem);
		((Component)_systemMsgLabel).gameObject.SetActive(msg.IsSystem);
		((Component)_msgLabel).gameObject.SetActive(!isSelectMsg);
		ActivatePortrait(msg.Portrait);
		if (!string.IsNullOrEmpty(msg.NameTag))
		{
			ActivateNameTag(msg.NameTag);
		}
		_doNotFinishByTouch = msg.DoNotFinishByTouch;
		_hidableCaption = msg.HidableCaption;
		_touchHand.SetActive(!_doNotFinishByTouch);
	}

	private void ActivateNameTag(string nameTag, bool deactivateOthers = true)
	{
		if (deactivateOthers)
		{
			DeactivateAllPortraits();
		}
		_nameTagLabel.text = LocalizeSystem.Get(nameTag);
		((Component)_nameTagLabel).gameObject.SetActive(true);
	}

	public void ShowSingleGuideMsg(string msg, bool isSystemMsg, float msgDuration)
	{
		ShowGuideMsg(msg, isSystemMsg, msgDuration);
	}

	private List<string> GetGuideTexts(string tokenBase)
	{
		return LocalizeSystem.GetSequences(tokenBase);
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
		List<string> guideTexts = GetGuideTexts(msg.LocalKey);
		_msgDuration = msg.MsgDuration;
		if (guideTexts == null || guideTexts.Count == 0)
		{
			_msgList = null;
			_currentMsgIndex = -1;
			DeactivateAllPortraits();
			Close();
			return;
		}
		Open();
		SetUIActive(msg, isSelectMsg: false);
		if (msg.PlaySnd)
		{
			((Component)this).GetComponent<AudioSource>().Play();
		}
		_isSystemMsg = msg.IsSystem;
		_msgList = guideTexts;
		_currentMsgIndex = -1;
		ShowNextGuideMsg();
	}

	public bool HasGuideMsg()
	{
		return _msgList != null && _msgList.Count > 0 && _currentMsgIndex < _msgList.Count;
	}

	public void ClearGuideMsg()
	{
		GameSystem<PrologueGuideSystem>.Instance().OnPreEndGuide();
		_currentMsgIndex = -1;
		_msgList = null;
		_currentMsgShowTime = -1f;
		DeactivateAllPortraits();
		Close();
		if (_isFirstTime && !_isSystemMsg)
		{
			_isFirstTime = false;
		}
		DispathOnFinish();
	}

	private void DispathOnFinish()
	{
		if (_lastOnFinish != null)
		{
			((MonoBehaviour)this).StartCoroutine(CoDelayedFinish(_lastOnFinish));
		}
		_lastOnFinish = null;
	}

	private IEnumerator CoDelayedFinish(PrologueGuideSystem.PrologueGuideOnFinish onFinish)
	{
		yield return (object)new WaitForSeconds(onFinish.Delay);
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
		if (_msgList == null)
		{
			DispathOnFinish();
		}
	}

	public int GetCurrentMsgIndex()
	{
		return _currentMsgIndex;
	}

	public void ShowNextGuideMsg()
	{
		if (_msgList == null)
		{
			return;
		}
		_currentMsgIndex++;
		if (_currentMsgIndex >= _msgList.Count)
		{
			ClearGuideMsg();
			return;
		}
		if (_isSystemMsg)
		{
			_systemMsgLabel.text = ConditionalText.Format(_msgList[_currentMsgIndex]);
		}
		else
		{
			string text = _msgList[_currentMsgIndex];
			Match match = _portraitToken.Match(text);
			if (match.Success)
			{
				string value = match.Groups[1].Value;
				ActivatePortrait(value);
				text = _portraitToken.Replace(text, string.Empty);
			}
			_msgLabel.text = ConditionalText.Format(text);
		}
		_currentMsgShowTime = Time.time;
		if (this.OnChangeGuideMsg != null)
		{
			this.OnChangeGuideMsg(_msgList[_currentMsgIndex], _isSystemMsg);
		}
	}
}
