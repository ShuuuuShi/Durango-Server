using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public abstract class ItemContextBase : UIWidget
{
	[SerializeField]
	protected UIWidget _header;

	[SerializeField]
	protected UIWidget _body;

	[SerializeField]
	private UILabel _headerText;

	[SerializeField]
	private GameObject _expandButton;

	private bool _initialized;

	private int _headerHeight;

	public bool IsExpanded { get; private set; }

	public string HeaderText
	{
		get
		{
			return (!(_headerText != null)) ? string.Empty : _headerText.text;
		}
		protected set
		{
			if (_headerText != null)
			{
				_headerText.text = value;
			}
		}
	}

	public int HeaderTextWidth => (_headerText != null) ? ((int)_headerText.printedSize.x) : 0;

	public event Action<ItemContextBase> OnExpandChanged;

	public virtual void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			_headerHeight = _header.height;
			UIEventListener uIEventListener = UIEventListener.Get(_header.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnHeaderClick));
		}
	}

	public void SetExpand(bool expand, bool instant)
	{
		IsExpanded = expand;
		base.height = _headerHeight + GetContextHeight(expand);
		if (expand)
		{
			_body.gameObject.SetActive(value: true);
		}
		if (instant)
		{
			_body.gameObject.SetActive(expand);
			_body.alpha = ((!expand) ? 0f : 1f);
			_expandButton.transform.localEulerAngles = Vector3.forward * ((!expand) ? 180f : 0f);
		}
		else
		{
			AnimationWidget.Get(_body.gameObject, 0.2f, 0f, deactiveWhenFadeout: true).Alpha = ((!expand) ? 0f : 1f);
			TweenRotation.Begin(_expandButton, 0.2f, Quaternion.Euler(Vector3.forward * ((!expand) ? 180f : 0f)));
		}
	}

	protected virtual int GetContextHeight(bool show)
	{
		return show ? _body.height : 0;
	}

	private void OnHeaderClick(GameObject go)
	{
		if (this.OnExpandChanged != null)
		{
			this.OnExpandChanged(this);
		}
	}
}
