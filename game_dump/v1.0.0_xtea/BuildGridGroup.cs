using System;
using Building_;
using UnityEngine;

public class BuildGridGroup : UIBase
{
	[SerializeField]
	private SelectGridPanel _selectGridPanel;

	private Action _buildSiteConfirm;

	private Blueprint _blueprint;

	private Point2 _size;

	private void Start()
	{
		_selectGridPanel.ConfirmedBlueprint += SelectGridPanel_ConfirmedBlueprint;
		_selectGridPanel.Canceled += SelectGridPanel_Canceled;
		base.OnClose();
	}

	public void Open(Blueprint blueprint, Action onConfirm = null)
	{
		Open(blueprint, blueprint.Size, onConfirm);
	}

	public void Open(Blueprint blueprint, Point2? size, Action onConfirm = null)
	{
		_blueprint = blueprint;
		_size = (size.HasValue ? size.Value : blueprint.Size);
		_buildSiteConfirm = onConfirm;
		Open();
	}

	private void SelectGridPanel_ConfirmedBlueprint(Blueprint blueprint)
	{
		UIBase.CloseAllUI();
		if (_buildSiteConfirm != null)
		{
			_buildSiteConfirm();
		}
		else
		{
			GameSystem<BuildSystem>.Instance().ConstructionSiteSelect(blueprint);
		}
	}

	private void SelectGridPanel_Canceled()
	{
		Close();
	}

	protected override bool OnOpen()
	{
		_selectGridPanel.Show(_blueprint, _size);
		return true;
	}

	protected override bool OnClose()
	{
		_selectGridPanel.Hide();
		return true;
	}
}
