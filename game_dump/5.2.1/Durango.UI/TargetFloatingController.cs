using UnityEngine;

namespace Durango.UI;

public class TargetFloatingController : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private TargetFloatingNode _baseNode;

	private ListObjectPool<TargetFloatingNode> _nodes;

	private int _baseDepth;

	void IUIInitializable.Init()
	{
		_baseDepth = _baseNode.GetComponent<UIPanel>().depth;
		_nodes = new ListObjectPool<TargetFloatingNode>();
		_nodes.BaseObject = _baseNode;
		_nodes.UseBase = true;
		_nodes.Init(delegate(TargetFloatingNode node)
		{
			node.Initialize();
		});
		_nodes.Clear();
	}

	private void LateUpdate()
	{
		for (int i = 0; i < _nodes.Count; i++)
		{
			TargetFloatingNode targetFloatingNode = _nodes[i];
			if (!targetFloatingNode.IsValid())
			{
				Release(i);
				i--;
			}
			else
			{
				targetFloatingNode.UpdateTick();
				targetFloatingNode.SetDepth(_baseDepth + i);
			}
		}
	}

	public TargetFloatingNode MakeOrAdd(string key)
	{
		int num = IndexOf(key);
		if (num != -1)
		{
			return _nodes[num];
		}
		TargetFloatingNode targetFloatingNode = _nodes.Add();
		targetFloatingNode.Make(key);
		return targetFloatingNode;
	}

	private int IndexOf(string key)
	{
		for (int i = 0; i < _nodes.Count; i++)
		{
			if (_nodes[i].Key == key)
			{
				return i;
			}
		}
		return -1;
	}

	public void Release(string key)
	{
		int num = IndexOf(key);
		if (num != -1)
		{
			Release(num);
		}
	}

	private void Release(int index)
	{
		_nodes[index].Release();
		_nodes.Swap(index, _nodes.Count - 1);
		_nodes.Set(_nodes.Count - 1);
	}
}
