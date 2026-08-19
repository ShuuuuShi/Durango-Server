using UnityEngine;

namespace Durango.UI;

public abstract class ToolDatum
{
	public bool IsSelected;

	public ToolType Tool { get; private set; }

	public ToolType PreviousDrawableTool { get; private set; }

	protected abstract bool IsDrawable { get; }

	public string IconKey { get; private set; }

	public abstract bool IsRadioButton { get; }

	public abstract bool IsCheckBoxButton { get; }

	public abstract bool HasNodeStylePreview { get; }

	public abstract bool HasStyle(int offset);

	public virtual bool TrySwapStyle(int offset)
	{
		return false;
	}

	public static ToolDatum Create(ToolType elem, string iconKey)
	{
		ToolDatum toolDatum = null;
		toolDatum = elem switch
		{
			ToolType.Pen => new PenToolDatum(), 
			ToolType.Eraser => new EraserToolDatum(), 
			ToolType.Brush => new BrushToolDatum(), 
			ToolType.Bucket => new BucketTollDatum(), 
			ToolType.Grid => new CheckBoxToolDatum(), 
			_ => new DefaultToolDatum(), 
		};
		toolDatum.Tool = elem;
		toolDatum.IconKey = iconKey;
		return toolDatum;
	}

	public virtual Color ChangeColorByTool(Color curColor)
	{
		return curColor;
	}

	public void SetPreviousDrawableTool(ToolDatum data)
	{
		PreviousDrawableTool = ((data != null && data.IsDrawable) ? data.Tool : ToolType.Pen);
	}
}
