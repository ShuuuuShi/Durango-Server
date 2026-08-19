using Durango.Logic.Social;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class ChatInputLineTextWidget : UIWidget
{
	[SerializeField]
	private UILabel _textLabel;

	public void Set(ChatStruct chat)
	{
		_textLabel.text = string.Format("[{1}]{0}[-] [{3}]{2} [c][888888][size=16]{4}[/size][-][/c]", chat.Name, NGUIText.EncodeColor(chat.GetNameColor()), chat.FindText(), NGUIText.EncodeColor(chat.GetMsgColor(Color.white)), Times.Timeago(chat.Time));
	}
}
