using ClanData;
using UnityEngine;

public class EditClanInfoGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private MemberRoleList _roleList;

	private void Start()
	{
		_titleWidget.OnBack += Close;
		_titleWidget.OnClose += UIBase.CloseAllUI;
		base.OnClose();
	}

	public void Open(Clan clan)
	{
		_roleList.Set(clan);
		Open();
		_titleWidget.ShowBackButton((Object)(object)UIBase.FullScreenUI != (Object)null, instant: true);
	}
}
