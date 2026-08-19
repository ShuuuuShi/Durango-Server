using Durango.Logic.Estate;
using Durango.UI.Control;
using Shared.Estate;
using UnityEngine;

namespace Durango.UI;

public class AccessRightNode : SelectableWidget
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _descriptionLabel;

	private AccessRights _right;

	private OwnerType _owner = OwnerType.Invalid;

	public AccessRights Right => _right;

	public void Set(AccessRights right)
	{
		if (_right != right)
		{
			_right = right;
			UpdateLabels();
		}
	}

	public void Set(OwnerType owner)
	{
		if (_owner != owner)
		{
			_owner = owner;
			UpdateLabels();
		}
	}

	private void UpdateLabels()
	{
		if (_right != 0 && _owner != OwnerType.Invalid)
		{
			_titleLabel.text = Util.GetName(_owner, Right);
			_descriptionLabel.text = Util.GetDescription(_owner, Right);
		}
	}
}
