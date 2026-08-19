using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using L10N;
using PlayGuide;
using Shared.Faction;
using UnityEngine;

public class PlayGuideGroup : UIBase
{
	[Serializable]
	public class FactionPortraitPair
	{
		public FactionType Faction;

		public Material Portrait;
	}

	[SerializeField]
	private UIWidget _container;

	[SerializeField]
	private UISprite _guideTextBack;

	[SerializeField]
	private GameObject _guideGroup;

	[SerializeField]
	private UITexture _guidePortait;

	[SerializeField]
	private Material[] _guidePortaits;

	[SerializeField]
	private FactionPortraitPair[] _factionPortraits;

	[SerializeField]
	private UILabel _msgLabel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _systemMsgLabel;

	[SerializeField]
	private AudioClipType _radioSignalAudio;

	[SerializeField]
	private UIWidget _touchHand;

	private GuideEvent _guideEvent;

	private int _guideTextTopOffset;

	private IList<string> _messages;

	private int _currentMsgIndex;

	private float _msgShowTimeRemain;

	private bool _readyToNextGuideMsg;

	private TypeWriterEffect _typewriter;

	private void Awake()
	{
		_guideTextTopOffset = _guideTextBack.topAnchor.absolute;
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_guideTextBack).gameObject);
		uIEventListener.onPress = delegate(GameObject go, bool state)
		{
			OnPressGuideWidget(state);
		};
		_typewriter = ((Component)_msgLabel).gameObject.AddComponent<TypeWriterEffect>();
		((Behaviour)_typewriter).enabled = false;
		_typewriter.Finished += TypeWriter_Finished;
		SoundManager.Cache(_radioSignalAudio);
		GameSystem<PlayGuideSystem>.Instance().Command.Ready += PlayGuideSystem_Ready;
		GameSystem<PlayGuideSystem>.Instance().PostEventSet += PlayGuideSystem_PostEventSet;
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += CombatSystem_ChangedCombatMode;
		base.OnClose();
	}

	private void OnDestroy()
	{
		GameSystem<PlayGuideSystem>.Instance().Command.Ready -= PlayGuideSystem_Ready;
		GameSystem<PlayGuideSystem>.Instance().PostEventSet -= PlayGuideSystem_PostEventSet;
		GameSystem<CombatSystem>.Instance().ChangedCombatMode -= CombatSystem_ChangedCombatMode;
	}

	[UsedImplicitly]
	private void OnPortraitMode(bool isPortraitMode)
	{
		_container.topAnchor.absolute = ((!isPortraitMode) ? 150 : 300);
		UIUtility.UpdateAnchors(((Component)_container).transform);
	}

	private void TypeWriter_Finished()
	{
		((Component)_touchHand).gameObject.SetActive(true);
	}

	private void PlayGuideSystem_Ready()
	{
		ShowNextGuideMsg();
	}

	private void PlayGuideSystem_PostEventSet(IList<string> msgList, [NotNull] GuideEvent guideEvent)
	{
		if (KUtility.GetSize(msgList) == 0)
		{
			ClearGuideMsg(notifySystem: false);
			return;
		}
		_messages = msgList;
		_guideEvent = guideEvent;
		_currentMsgIndex = -1;
		SetUIActive();
		if (!GameSystem<CombatSystem>.Instance().CombatMode)
		{
			Show();
		}
	}

	private void CombatSystem_ChangedCombatMode(bool combatMode)
	{
		if (combatMode)
		{
			((Component)this).gameObject.SetActive(false);
			CloseBlur();
			return;
		}
		((Component)this).gameObject.SetActive(true);
		if (HasGuideMsg())
		{
			Show();
		}
	}

	private void Update()
	{
		if (_messages != null && !GameSystem<PlayGuideSystem>.Instance().PauseUpdate)
		{
			_msgShowTimeRemain -= Time.deltaTime;
			if (_guideEvent.MsgDuration > 0f && _msgShowTimeRemain <= 0f)
			{
				ShowNextGuideMsg();
			}
		}
	}

	private void OnPressGuideWidget(bool pressed)
	{
		if (pressed)
		{
			_readyToNextGuideMsg = !((Behaviour)_typewriter).enabled;
			_typewriter.TypingSpeed = 0.01f;
			return;
		}
		if (_readyToNextGuideMsg)
		{
			ShowNextGuideMsg();
		}
		_typewriter.TypingSpeed = 0.03f;
	}

	protected override bool OnClose()
	{
		CloseBlur();
		return base.OnClose();
	}

	private void SetUIActive()
	{
		bool flag = _messages.Count > 0 && !string.IsNullOrEmpty(_messages[0]);
		_guideGroup.SetActive(!_guideEvent.IsSystem && flag);
		((Component)_systemMsgLabel).gameObject.SetActive(_guideEvent.IsSystem && flag);
		_guideTextBack.topAnchor.absolute = _guideTextTopOffset;
		_guideTextBack.UpdateAnchors();
		UpdatePortrait();
	}

	public Material GetPortraitMaterial(ShowPortrait showPortrait, FactionType factionType)
	{
		bool flag = showPortrait == ShowPortrait.Faction && factionType != FactionType.Invalid;
		bool flag2 = showPortrait != ShowPortrait.None && !flag;
		if (flag)
		{
			int num = _factionPortraits.Length;
			for (int i = 0; i < num; i++)
			{
				if (_factionPortraits[i].Faction == factionType)
				{
					return _factionPortraits[i].Portrait;
				}
			}
		}
		else if (flag2)
		{
			return _guidePortaits[(int)showPortrait];
		}
		return null;
	}

	private void UpdatePortrait()
	{
		Material portraitMaterial = GetPortraitMaterial(_guideEvent.ShowPortrait, _guideEvent.Faction);
		if ((Object)(object)portraitMaterial != (Object)null)
		{
			_guidePortait.material = portraitMaterial;
		}
		((Component)_guidePortait).gameObject.SetActive((Object)(object)portraitMaterial != (Object)null);
		if (_guideEvent.NameTag == null || _guideEvent.NameTag.Trim().Length == 0 || (Object)(object)portraitMaterial != (Object)null)
		{
			((Component)_nameLabel).gameObject.SetActive(false);
			return;
		}
		((Component)_nameLabel).gameObject.SetActive(true);
		_nameLabel.text = _guideEvent.NameTag;
	}

	private void Show()
	{
		Open();
		if (_guideEvent.IsBlur)
		{
			OpenBlur();
		}
		if (!_guideEvent.IsSystem && _guideEvent.PlayAudio)
		{
			SoundManager.Play((string)_radioSignalAudio, loop: false, default(SoundManager.PitchRange));
		}
		if (_currentMsgIndex == -1)
		{
			ShowNextGuideMsg();
		}
	}

	private void OpenBlur()
	{
	}

	private static void CloseBlur()
	{
	}

	private bool OnPressBlur(bool pressed)
	{
		OnPressGuideWidget(pressed);
		return true;
	}

	public bool HasGuideMsg()
	{
		return _messages != null && _messages.Count > 0 && _currentMsgIndex < _messages.Count;
	}

	private void ClearGuideMsg(bool notifySystem = true)
	{
		_currentMsgIndex = -1;
		_guideEvent = null;
		_messages = null;
		_msgShowTimeRemain = -1f;
		Close();
		if (notifySystem)
		{
			GameSystem<PlayGuideSystem>.Instance().OnGuideMsgFinished();
		}
	}

	private void ShowNextGuideMsg()
	{
		if (_messages == null)
		{
			return;
		}
		_currentMsgIndex++;
		if (_currentMsgIndex >= _messages.Count)
		{
			ClearGuideMsg();
			return;
		}
		string text = T._(_messages[_currentMsgIndex]);
		if (!string.IsNullOrEmpty(_guideEvent.OverrideColorRGB))
		{
			text = $"[{_guideEvent.OverrideColorRGB}]{text}[-]";
		}
		if (_guideEvent.IsSystem)
		{
			_systemMsgLabel.text = text;
			UIUtility.UpdateAnchors(((Component)_systemMsgLabel).transform);
		}
		else
		{
			_msgLabel.leftAnchor.absolute = ((!((Component)_nameLabel).gameObject.activeSelf) ? (-_touchHand.leftAnchor.absolute) : (_nameLabel.rightAnchor.absolute + 40));
			_msgLabel.UpdateAnchors();
			_msgLabel.text = text;
			_readyToNextGuideMsg = false;
			_typewriter.TypingSpeed = 0.03f;
			_typewriter.Reset();
			((Behaviour)_typewriter).enabled = true;
			((Component)_touchHand).gameObject.SetActive(false);
		}
		AddToChat();
		_msgShowTimeRemain = _guideEvent.MsgDuration;
	}

	private void AddToChat()
	{
		string speakerName = (_guideEvent.IsSystem ? string.Empty : ((!string.IsNullOrEmpty(_guideEvent.NameTag)) ? _guideEvent.NameTag : T._("[ffbf00]K[-]")));
		GameSystem<SocialSystem>.Instance().AddSystemChat(T._(_messages[_currentMsgIndex]), speakerName);
	}
}
