using UnityEngine;

namespace Durango.UI;

public class ChattingLine : ChattingLineBase
{
	[SerializeField]
	private int _textLeftMargin;

	[SerializeField]
	private int _textRightMargin;

	protected override void SetName(string playerName)
	{
		base.SetName(playerName);
		Vector3 localPosition = NameLabel.transform.localPosition;
		if (!string.IsNullOrEmpty(NameLabel.text))
		{
			localPosition = NameLabel.transform.localPosition + Vector3.right * (NameLabel.width + _textLeftMargin);
		}
		TextLabel.transform.localPosition = localPosition;
		UpdateTextLabelWidth();
	}

	protected override void OnUpdateButtons()
	{
		base.OnUpdateButtons();
		UpdateTextLabelWidth();
	}

	private void UpdateTextLabelWidth()
	{
		int num = _textRightMargin + GetRightButtonMargin();
		TextLabel.width = (int)((float)base.Widget.width - TextLabel.transform.localPosition.x - (float)num);
		TextLabel.ProcessText();
	}
}
