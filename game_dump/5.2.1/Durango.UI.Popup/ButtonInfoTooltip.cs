using System.Text;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Popup;

public class ButtonInfoTooltip : TooltipBase
{
	[SerializeField]
	private UILabel _description;

	public int CreateCode { get; set; }

	public void Set(InputCommand inputCommand, string description = null)
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder stringBuilder = reusable;
		stringBuilder.AppendFormat("<tooltip_box><shortcut_label>{0}", inputCommand);
		if (string.IsNullOrEmpty(description))
		{
			stringBuilder.Append("</shortcut_label></tooltip_box>");
		}
		else
		{
			stringBuilder.AppendFormat(",{0}</shortcut_label></tooltip_box>", description);
		}
		_description.text = stringBuilder.ToString();
	}

	public void Set(string description)
	{
		_description.text = "<tooltip_box>" + description + "</tooltip_box>";
	}

	public void SetPosition(Vector3 pos)
	{
		base.TargetPos = pos;
		UpdatePosition();
	}
}
