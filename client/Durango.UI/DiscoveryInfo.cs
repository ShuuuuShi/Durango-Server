using System;
using UnityEngine;

namespace Durango.UI;

public abstract class DiscoveryInfo : MonoBehaviour
{
	[SerializeField]
	protected RectLayout _layout;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	protected UIWidget _nodesWidget;

	[SerializeField]
	protected ListObjectPool _nodes;

	private bool _isFolded;

	private string _countString;

	public event Action LayoutUpdated;

	public abstract void ShowUnknown();

	private void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_countLabel.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			_isFolded = !_isFolded;
			_nodesWidget.gameObject.SetActive(!_isFolded);
			_layout.UpdateLayout();
			if (this.LayoutUpdated != null)
			{
				this.LayoutUpdated();
			}
			Refresh();
		});
	}

	private void OnEnable()
	{
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void Refresh()
	{
		_countLabel.text = string.Format("{0}  [c=ui_dark_gray][icon={1}:0.65][-]", _countString, (!_isFolded) ? "img_arrow_up" : "img_arrow_down");
	}

	protected void SetCountLabel(string content)
	{
		_countString = content;
		Refresh();
	}
}
