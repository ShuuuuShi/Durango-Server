using System;
using System.Linq;
using Durango.Utils.Extensions;

namespace Durango.UI;

public class BrushToolDatum : ToolDatum
{
	private static readonly BrushType[] Brushes = Enum.GetValues(typeof(BrushType)).Cast<BrushType>().ToArray();

	public BrushType BrushType;

	private bool _isDrawable;

	public override bool IsCheckBoxButton => false;

	public override bool HasNodeStylePreview => true;

	protected override bool IsDrawable => true;

	public override bool IsRadioButton => true;

	public override bool HasStyle(int offset)
	{
		int num = Brushes.IndexOf(BrushType);
		if (num < 0)
		{
			return false;
		}
		num += offset;
		return num >= 0 && num <= Brushes.Length - 1;
	}

	public override bool TrySwapStyle(int offset)
	{
		if (!HasStyle(offset))
		{
			return false;
		}
		BrushType = Brushes[Brushes.IndexOf(BrushType) + offset];
		return true;
	}
}
