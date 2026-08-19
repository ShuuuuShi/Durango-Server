namespace Durango.UI;

public class DefaultToolDatum : ToolDatum
{
	public override bool IsRadioButton => true;

	public override bool IsCheckBoxButton => false;

	protected override bool IsDrawable => false;

	public override bool HasNodeStylePreview => false;

	public override bool HasStyle(int offset)
	{
		return false;
	}
}
