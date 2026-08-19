using L10N;

namespace Durango.UI;

public class CharacterWidget : CharacterWidgetBase
{
	protected override void SetExp(int level, int current, int currentMax)
	{
		base.SetExp(level, current, currentMax);
		_expLabel.text = T._("{0:lv:}  (<em>{1}</em> <weak>/</weak> {2})", level, current, currentMax);
	}
}
