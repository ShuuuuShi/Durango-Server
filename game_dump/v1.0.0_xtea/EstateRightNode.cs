using Shared.Estate;
using UnityEngine;

public class EstateRightNode : SelectableWidget
{
	[SerializeField]
	private GameObject _checkUpper;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private GameObject _separator;

	public AccessRights Right { get; private set; }

	public void Set(AccessRights right)
	{
		Right = right;
		string key = LocalizeUtil.GetKey(right);
		_titleLabel.text = LocalizeSystem.Get(key);
		_descriptionLabel.text = LocalizeSystem.Get($"{key}_description");
	}

	public void EnableSeparator(bool enable)
	{
		_separator.gameObject.SetActive(enable);
	}
}
