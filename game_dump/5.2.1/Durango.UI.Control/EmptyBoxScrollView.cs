using System;

namespace Durango.UI.Control;

public abstract class EmptyBoxScrollView : KScrollViewBase
{
	protected abstract float Size { get; }

	public override UIWidget GetNode(int index)
	{
		throw new NotImplementedException();
	}

	protected override int CalcNodeIndex(float offset)
	{
		return (int)(offset / Size);
	}

	protected override float GetNodeSize(int index)
	{
		return Size;
	}

	protected override float OnUpdateLayout(bool instant)
	{
		return (float)GetNodeCount() * Size;
	}
}
