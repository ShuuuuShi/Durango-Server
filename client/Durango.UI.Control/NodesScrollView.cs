using UnityEngine;

namespace Durango.UI.Control;

public abstract class NodesScrollView : KScrollViewBase
{
	[SerializeField]
	private ListObjectPool _nodes;

	public ListObjectPool Nodes => _nodes;

	public override UIWidget GetNode(int index)
	{
		return _nodes[index].GetComponent<UIWidget>();
	}

	public override int GetNodeCount()
	{
		return _nodes.Count;
	}
}
