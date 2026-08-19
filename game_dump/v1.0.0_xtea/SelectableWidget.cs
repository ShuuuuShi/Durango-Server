using UnityEngine;

public class SelectableWidget : Selectable
{
	[SerializeField]
	private bool _onSelect;

	private PressColorChange _colorComp;

	private bool _isFoundColorComp;

	private UIWidget _widget;

	protected PressColorChange ColorComp
	{
		get
		{
			if (_isFoundColorComp)
			{
				return _colorComp;
			}
			_isFoundColorComp = true;
			_colorComp = ((Component)this).GetComponent<PressColorChange>();
			return _colorComp;
		}
	}

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

	protected override void OnInit()
	{
	}

	protected override void Refresh(bool select)
	{
	}

	protected override void OnSelected(bool select)
	{
		base.OnSelected(select);
		if ((Object)(object)ColorComp != (Object)null)
		{
			ColorComp.Select(select);
		}
	}

	protected override void OnSelectDisable(bool disable)
	{
		base.OnSelectDisable(disable);
		if ((Object)(object)ColorComp != (Object)null)
		{
			ColorComp.Disable(disable);
		}
	}

	private void OnSelect(bool select)
	{
		if (_onSelect)
		{
			base.Select = select;
		}
	}
}
