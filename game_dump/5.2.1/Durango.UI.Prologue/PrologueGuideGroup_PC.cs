using System;
using Durango.Prologue;
using L10N;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueGuideGroup_PC : PrologueGuideGroupBase
{
	[SerializeField]
	private GameObject _confirmShortcut;

	[SerializeField]
	private UILabel _confirmLabel;

	[SerializeField]
	private UISprite _guideTextBg;

	[SerializeField]
	private UISprite _talkTextBg;

	private string _nameTxt;

	protected override void Awake()
	{
		base.Awake();
		_confirmLabel.text = T._("다음");
		UIEventListener uIEventListener = UIEventListener.Get(_confirmShortcut.GetComponentInChildren<BoxCollider>().gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(base.OnClickObjectPressed));
	}

	protected override void SetGuideMsg(string msgTxt)
	{
		bool flag = !string.IsNullOrEmpty(_nameTxt);
		if (flag)
		{
			string guideMsg = _nameTxt + " [89D2FF]:[-] " + msgTxt;
			base.SetGuideMsg(guideMsg);
		}
		else
		{
			base.SetGuideMsg(msgTxt);
		}
		_guideTextBg.gameObject.SetActive(!flag);
		_talkTextBg.gameObject.SetActive(flag);
		int absolute = ((!flag) ? (-_guideTextBg.topAnchor.absolute) : (-_talkTextBg.topAnchor.absolute));
		_guideTextBg.GetComponent<UIWidget>().parent.topAnchor.absolute = absolute;
	}

	protected override void DeactivateAllPortraits()
	{
		base.DeactivateAllPortraits();
		_nameTxt = string.Empty;
	}

	protected override void OnSetUIActive(PrologueGuideSystem.MsgInfo msg)
	{
		_confirmShortcut.SetActive(msg.ShowConfirmShortcut);
	}

	protected override void ActivateNameTag(string nameTag, bool deactivateOthers = true)
	{
		_nameTxt = LocalizeSystem.Get(nameTag);
	}

	protected override void Update()
	{
		base.Update();
		if (_confirmShortcut.activeInHierarchy)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				OnClickObjectPressed(null, pressed: true);
			}
			else if (Input.GetKeyUp(KeyCode.Space))
			{
				OnClickObjectPressed(null, pressed: false);
			}
		}
	}
}
