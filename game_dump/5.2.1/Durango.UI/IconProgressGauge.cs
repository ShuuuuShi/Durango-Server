using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using UnityEngine;

namespace Durango.UI;

public class IconProgressGauge : ProgressGauge
{
	[SerializeField]
	private ListObjectPool _progressNodes;

	[SerializeField]
	private ListObjectPool _plusIcons;

	[SerializeField]
	private float _hideTime;

	private readonly List<ItemIcon> _icons = new List<ItemIcon>();

	private readonly List<UIWidget> _widgets = new List<UIWidget>();

	public void AddIcon(string icon)
	{
		AddIcon(icon, Color.white);
	}

	public void AddIcon(string icon, Color color)
	{
		AddIcon(new ItemIcon
		{
			Main = icon,
			Colors = new ItemColor(color)
		});
	}

	public void AddIcon(ItemIcon icon)
	{
		_icons.Add(icon);
		Refresh();
	}

	protected override void InitGauge()
	{
	}

	protected override void OnStart()
	{
		Refresh();
	}

	private void Refresh()
	{
		_widgets.Clear();
		_progressNodes.BeginLoad();
		_plusIcons.BeginLoad();
		for (int i = 0; i < _icons.Count; i++)
		{
			ItemIcon icon = _icons[i];
			GameObject next = _progressNodes.GetNext();
			IconProgressGaugeNode component = next.GetComponent<IconProgressGaugeNode>();
			if (component != null)
			{
				component.SetIcon(icon);
			}
			_widgets.Add(next.GetComponent<UIWidget>());
			if (i != _icons.Count - 1)
			{
				GameObject next2 = _plusIcons.GetNext();
				_widgets.Add(next2.GetComponent<UIWidget>());
			}
		}
		_progressNodes.EndLoad();
		_plusIcons.EndLoad();
		float num = _widgets.Sum((UIWidget o) => o.width);
		UIUtility.WidgetsReposition(_widgets, Vector3.right, new Vector3((0f - num) / 2f, 50f));
	}

	protected override void DrawGauge(float ratio)
	{
		foreach (GameObject progressNode in _progressNodes)
		{
			progressNode.GetComponent<IconProgressGaugeNode>().DrawGauge(ratio);
		}
	}

	protected override bool EndedGauge(float timer)
	{
		base.Widget.alpha = Mathf.Min(base.Widget.alpha, Mathf.Clamp01(1f - timer / _hideTime));
		return timer > _hideTime;
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		_icons.Clear();
		_widgets.Clear();
		_progressNodes.Clear();
		_plusIcons.Clear();
	}
}
