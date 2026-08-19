using System;
using System.Linq;
using Durango.Utils.Extensions;

namespace Durango.UI;

public class PenToolDatum : ToolDatum
{
	private static readonly PenType[] Pens = Enum.GetValues(typeof(PenType)).Cast<PenType>().ToArray();

	public PenType PenType;

	public override bool IsRadioButton => true;

	public override bool IsCheckBoxButton => false;

	public override bool HasNodeStylePreview => true;

	protected override bool IsDrawable => true;

	public override bool HasStyle(int offset)
	{
		int num = Pens.IndexOf(PenType);
		if (num < 0)
		{
			return false;
		}
		num += offset;
		return num >= 0 && num <= Pens.Length - 1;
	}

	public override bool TrySwapStyle(int offset)
	{
		if (!HasStyle(offset))
		{
			return false;
		}
		PenType = Pens[Pens.IndexOf(PenType) + offset];
		return true;
	}
}
