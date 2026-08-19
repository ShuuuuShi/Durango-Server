using UnityEngine;

namespace Durango.UI;

public class CharacterWidget_PC : CharacterWidgetBase
{
	[SerializeField]
	private UILabel _levelLabel;

	protected override string MakeNameText(string playerName, int freq)
	{
		return $"[F5F1EB]{playerName}[-] [size=20][ADABA6]#{freq:0000} kHz[-][/size]";
	}

	protected override void SetExp(int level, int current, int currentMax)
	{
		base.SetExp(level, current, currentMax);
		_levelLabel.text = level.ToString();
		_expLabel.text = $"<em>{current}</em> <weak>/</weak> {currentMax}";
	}
}
