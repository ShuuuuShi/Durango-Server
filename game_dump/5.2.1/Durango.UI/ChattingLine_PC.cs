using Durango.Logic.Social;
using L10N;
using Shared.Chat;
using UnityEngine;

namespace Durango.UI;

public class ChattingLine_PC : ChattingLineBase
{
	[SerializeField]
	private UILabel _tagLabel;

	[SerializeField]
	[Tooltip("태그와 이름 사이 간격")]
	private int _paddingBetweenTagName;

	[SerializeField]
	private int _chatRightPadding;

	private float _spaceWidth;

	private Collider[] _colliders;

	protected override void Awake()
	{
		base.Awake();
		_colliders = GetComponentsInChildren<Collider>();
		TextLabel.text = " ";
		_spaceWidth = TextLabel.printedSize.x;
		TextLabel.text = string.Empty;
	}

	protected override void SetName(string playerName)
	{
		string text = ((!string.IsNullOrEmpty(playerName)) ? (playerName + " : ") : string.Empty);
		base.SetName(text);
	}

	public void EnableColliders(bool isEnable)
	{
		Collider[] colliders = _colliders;
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = isEnable;
		}
	}

	public override void SetChat(ChatStruct chat, bool isAllChat = false)
	{
		if (isAllChat && chat.Type != ChannelType.System && chat.Type != ChannelType.Conversation)
		{
			string text = NGUIText.EncodeColor(chat.GetMsgColor(Color.white));
			string text2 = chat.Type.GetName();
			_tagLabel.text = "[" + text + "]" + text2 + "[-]";
			SetNameLabelPosX(_tagLabel.transform.localPosition.x + (float)_tagLabel.width + (float)_paddingBetweenTagName);
			_tagLabel.gameObject.SetActive(value: true);
		}
		else
		{
			SetNameLabelPosX(_tagLabel.transform.localPosition.x);
			_tagLabel.gameObject.SetActive(value: false);
		}
		base.SetChat(chat, isAllChat);
		UIUtility.UpdateAnchors(base.transform);
	}

	private void SetNameLabelPosX(float x)
	{
		Vector3 localPosition = NameLabel.transform.localPosition;
		localPosition.x = x;
		NameLabel.transform.localPosition = localPosition;
	}

	protected override void SetText(ChatStruct chat)
	{
		base.SetText(chat);
		if (!string.IsNullOrEmpty(NameLabel.text))
		{
			int totalWidth = (int)((NameLabel.transform.localPosition.x + (float)NameLabel.width - (float)TextLabel.leftAnchor.absolute) / _spaceWidth);
			TextLabel.text = string.Empty.PadLeft(totalWidth) + TextLabel.text;
		}
	}

	protected override void OnUpdateButtons()
	{
		base.OnUpdateButtons();
		TextLabel.rightAnchor.absolute = -GetRightButtonMargin() - _chatRightPadding;
		TextLabel.UpdateAnchors();
		TextLabel.ProcessText();
	}
}
