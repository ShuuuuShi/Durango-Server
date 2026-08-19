using Shared.Clan;
using UnityEngine;

public class MemberRolePermissionNode : Selectable
{
	[SerializeField]
	private UISprite _checkSprite;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _commentLable;

	[SerializeField]
	private GameObject _separator;

	protected override void OnInit()
	{
	}

	protected override void Refresh(bool isSelect)
	{
		((Component)_checkSprite).gameObject.SetActive(isSelect);
	}

	public void Set(Permissions permission)
	{
		string key = LocalizeUtil.GetKey(permission);
		_titleLabel.text = LocalizeSystem.Get(key);
		_commentLable.text = LocalizeSystem.Get($"{key}_description");
	}

	public void EnableSeparator(bool enable)
	{
		_separator.SetActive(enable);
	}
}
