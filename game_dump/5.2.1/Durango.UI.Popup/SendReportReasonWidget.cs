using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class SendReportReasonWidget : SelectableWidget
{
	[SerializeField]
	private UILabel _text;

	private SendReportSystem.PlayerReportCategory _category;

	public Action<SendReportSystem.PlayerReportCategory, string> ReasonClicked;

	public void Set(SendReportSystem.PlayerReportCategory category, string text)
	{
		if (Clicked == null)
		{
			Clicked = delegate
			{
				if (ReasonClicked != null)
				{
					ReasonClicked(_category, _text.text);
				}
			};
		}
		_category = category;
		_text.text = text;
	}
}
