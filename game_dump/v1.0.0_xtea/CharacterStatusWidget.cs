using Shared.Ability;
using UnityEngine;

public class CharacterStatusWidget : MonoBehaviour
{
	[SerializeField]
	private KScrollView _nodes;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public void SetData(Derived[] keys, int[] values)
	{
		_nodes.Nodes.Set((keys != null) ? keys.Length : 0);
		int i = 0;
		for (int count = _nodes.Nodes.Count; i < count; i++)
		{
			KeyValueLabel component = _nodes.Nodes[i].GetComponent<KeyValueLabel>();
			string text = LocalizeUtil.Get(keys[i]);
			component.Set(text, values[i].ToString());
		}
		_nodes.Reposition();
	}
}
