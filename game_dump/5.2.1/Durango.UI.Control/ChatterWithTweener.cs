using System.Text;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Control;

public class ChatterWithTweener : MonoBehaviour
{
	[SerializeField]
	private UILabel[] _toBeChatted;

	private void Awake()
	{
		TweenerPlayer component = GetComponent<TweenerPlayer>();
		if (component != null)
		{
			component.Played += TweenerPlayer_Played;
		}
	}

	private void TweenerPlayer_Played()
	{
		string text;
		using (Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop())
		{
			StringBuilder stringBuilder = reusable;
			for (int i = 0; i < KUtility.GetSize(_toBeChatted); i++)
			{
				if (!(_toBeChatted[i] == null))
				{
					stringBuilder.Append(_toBeChatted[i].text);
					stringBuilder.Append(" ");
				}
			}
			text = stringBuilder.ToString().Trim();
		}
		if (!string.IsNullOrEmpty(text))
		{
			GameSystem<SocialSystem>.Instance().AddSystemChat(text, string.Empty);
		}
	}
}
