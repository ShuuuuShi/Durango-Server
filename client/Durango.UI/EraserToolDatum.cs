using UnityEngine;

namespace Durango.UI;

public class EraserToolDatum : PenToolDatum
{
	protected override bool IsDrawable => false;

	public override bool HasNodeStylePreview => true;

	public override Color ChangeColorByTool(Color curColor)
	{
		return Color.clear;
	}
}
