using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class CenterFixedScrollBar : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _nodes;

	[CanBeNull]
	[SerializeField]
	private UIWidget _separator;

	[CanBeNull]
	[SerializeField]
	private UIWidget _frontSeparator;

	[CanBeNull]
	[SerializeField]
	private UIWidget _rearSeparator;

	[SerializeField]
	private KScrollView _targetScrollView;

	private readonly ListObjectPool<UIWidget> _separators = new ListObjectPool<UIWidget>();

	private readonly List<UIWidget> _widgets = new List<UIWidget>();

	private Vector3 _padding;

	private float _unitWidth;

	public ListObjectPool Nodes => _nodes;

	private void Awake()
	{
		_separators.BaseObject = _separator;
		_separators.UseBase = true;
	}

	private void Update()
	{
		if (!(_unitWidth <= 0f))
		{
			Vector3 vector = new Vector3(_targetScrollView.CurrentOffset * _unitWidth, 0f, 0f);
			base.transform.localPosition = _padding - vector;
		}
	}

	public void UpdateLayout()
	{
		if (Nodes.Count <= 1)
		{
			_unitWidth = 0f;
			return;
		}
		_widgets.Clear();
		_separators.BeginLoad();
		if (_frontSeparator != null)
		{
			_widgets.Add(_frontSeparator);
		}
		foreach (GameObject node in Nodes)
		{
			if (node != Nodes[0] && _separator != null)
			{
				_widgets.Add(_separators.GetNext());
			}
			_widgets.Add(node.GetComponent<UIWidget>());
		}
		if (_rearSeparator != null)
		{
			_widgets.Add(_rearSeparator);
		}
		_separators.EndLoad();
		int num = ((!(_separator == null)) ? _separator.width : 0);
		int width = Nodes.BaseObject.GetComponent<UIWidget>().width;
		UIWidget component = _targetScrollView.Nodes.BaseObject.GetComponent<UIWidget>();
		_unitWidth = (float)(num + width) / (float)component.width;
		UIWidget component2 = base.transform.parent.GetComponent<UIWidget>();
		int num2 = ((!(_frontSeparator == null)) ? _frontSeparator.width : 0);
		float x = (float)component2.width * 0.5f - ((float)width * 0.5f + (float)num2);
		_padding = new Vector3(x, 0f, 0f);
		UIWidget component3 = GetComponent<UIWidget>();
		component3.width = (int)UIUtility.WidgetsReposition(_widgets, component3, Vector3.right);
	}
}
