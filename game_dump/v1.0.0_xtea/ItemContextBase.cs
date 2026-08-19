using System;
using UnityEngine;

public abstract class ItemContextBase : MonoBehaviour
{
	public static bool ResetFlag;

	[SerializeField]
	private UILabel _headerText;

	[SerializeField]
	private GameObject _expandButton;

	[SerializeField]
	protected UIWidget _header;

	[SerializeField]
	protected UIWidget _body;

	private bool _initialized;

	private bool _expanded;

	private int _headerHeight;

	public string Id { get; protected set; }

	public string HeaderText
	{
		get
		{
			return (!((Object)(object)_headerText != (Object)null)) ? string.Empty : _headerText.text;
		}
		protected set
		{
			if ((Object)(object)_headerText != (Object)null)
			{
				_headerText.text = value;
			}
		}
	}

	public UIWidget Widget { get; private set; }

	public bool IsExpanded
	{
		get
		{
			return _expanded;
		}
		set
		{
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			_expanded = value;
			ShowContext(value);
			Widget.height = _headerHeight + GetContextHeight(_expanded);
			if (_expanded)
			{
				((Component)_body).gameObject.SetActive(true);
			}
			if (ResetFlag)
			{
				((Component)_body).gameObject.SetActive(_expanded);
				_body.alpha = ((!_expanded) ? 0f : 1f);
				_expandButton.transform.localEulerAngles = Vector3.forward * ((!_expanded) ? 180f : 0f);
			}
			else
			{
				AnimationWidget.Get(((Component)_body).gameObject, 0.2f, 0f, deactiveWhenFadeout: true).Alpha = ((!_expanded) ? 0f : 1f);
				TweenRotation.Begin(_expandButton, 0.2f, Quaternion.Euler(Vector3.forward * ((!_expanded) ? 180f : 0f)));
			}
		}
	}

	public event Action<ItemContextBase> OnExpandChanged;

	public void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			Widget = ((Component)this).GetComponent<UIWidget>();
			_headerHeight = _header.height;
			UIEventListener uIEventListener = UIEventListener.Get(((Component)_header).gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(header_onClick));
			OnInit();
		}
	}

	protected virtual void OnInit()
	{
	}

	protected virtual int GetContextHeight(bool show)
	{
		return show ? _body.height : 0;
	}

	protected virtual void ShowContext(bool show)
	{
	}

	private void header_onClick(GameObject go)
	{
		IsExpanded = !IsExpanded;
		if (this.OnExpandChanged != null)
		{
			this.OnExpandChanged(this);
		}
	}
}
