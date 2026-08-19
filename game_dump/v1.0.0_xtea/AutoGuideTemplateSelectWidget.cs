using System;
using System.Collections.Generic;
using AutoGuide;
using JetBrains.Annotations;
using UnityEngine;

public class AutoGuideTemplateSelectWidget : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _nodes;

	[SerializeField]
	private UIWidget _selector;

	private int _prevSelectedIndex;

	public Template SelectedTemplate { get; private set; }

	public event Action Selected;

	public void Set([NotNull] IList<Template> templates)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		_nodes.Init(null);
		int num = Mathf.Min(3, templates.Count);
		_nodes.Set(num);
		UIWidget component = _nodes.BaseObject.GetComponent<UIWidget>();
		GameObject val = null;
		for (int i = 0; i < num; i++)
		{
			Template template = templates[i];
			AutoGuideTemplateNode autoGuideTemplateNode = ((ListObjectPoolBase<GameObject>)_nodes).Get<AutoGuideTemplateNode>(i);
			autoGuideTemplateNode.Set(template);
			((Component)autoGuideTemplateNode).transform.localPosition = new Vector3(0f, (float)(-component.height * i), 0f);
			UIEventListener.Get(((Component)autoGuideTemplateNode).gameObject).onClick = Node_OnClick;
			if (autoGuideTemplateNode.Selected)
			{
				val = ((Component)autoGuideTemplateNode).gameObject;
			}
		}
		if ((Object)(object)val == (Object)null && _prevSelectedIndex < _nodes.Count)
		{
			val = _nodes[_prevSelectedIndex];
		}
		Node_OnClick((!((Object)(object)val != (Object)null)) ? _nodes.BaseObject : val);
	}

	private void Node_OnClick(GameObject go)
	{
		for (int i = 0; i < _nodes.Count; i++)
		{
			AutoGuideTemplateNode autoGuideTemplateNode = ((ListObjectPoolBase<GameObject>)_nodes).Get<AutoGuideTemplateNode>(i);
			autoGuideTemplateNode.Selected = false;
		}
		int num = _nodes.IndexOf(go);
		if (num != -1)
		{
			AutoGuideTemplateNode autoGuideTemplateNode2 = ((ListObjectPoolBase<GameObject>)_nodes).Get<AutoGuideTemplateNode>(num);
			autoGuideTemplateNode2.Selected = true;
			_selector.SetAnchor(((Component)autoGuideTemplateNode2).gameObject);
			SelectedTemplate = autoGuideTemplateNode2.Template;
			_prevSelectedIndex = num;
			if (this.Selected != null)
			{
				this.Selected();
			}
		}
	}
}
