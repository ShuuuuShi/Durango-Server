using L10N;

namespace Durango.UI;

public class CraterProgressGauge : TimerProgressGauge
{
	protected override string GetLabelText(double remainTick)
	{
		string text = TimedeltaFormatter.Format(remainTick);
		return T._("{0} 후 닫힘", text);
	}
}
