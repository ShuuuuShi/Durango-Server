using ClanData;
using L10N;
using Player;
using UnityEngine;

public class ClanMemberNode : SelectableWidget
{
	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private UISprite _portraitBg;

	[SerializeField]
	private UISpriteLabel _nameLabel;

	[SerializeField]
	private UISpriteLabel _levelLabel;

	[SerializeField]
	private UISpriteLabel _skillLabel;

	[SerializeField]
	private UISpriteLabel _connectionLabel;

	[SerializeField]
	private UISpriteLabel _rankLabel;

	private Texture _maskTexture;

	public Clan Clan { get; private set; }

	public Member Member { get; private set; }

	protected override void OnInit()
	{
		base.OnInit();
		_maskTexture = _portraitTexture.mainTexture;
	}

	public void Set(Clan clan, Member member)
	{
		Init();
		Clan = clan;
		Member = member;
		_nameLabel.alpha = 0f;
		_levelLabel.alpha = 0f;
		_skillLabel.alpha = 0f;
		_connectionLabel.alpha = 0f;
		_portraitTexture.alpha = 0f;
		SetRoleInfos();
		_rankLabel.alpha = 0f;
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(member.EntityId, ResponsePlayerInfo);
	}

	private void SetRoleInfos()
	{
		if (Member != null && Clan.TryGetRole(Member.RoleId, out var role))
		{
			_rankLabel.text = role.Name;
		}
		else
		{
			_rankLabel.text = T._("가입 대기자");
		}
	}

	private void ResponsePlayerInfo(PlayerInfo player)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		_nameLabel.text = player.Name;
		_levelLabel.text = player.Level.ToString();
		_skillLabel.text = string.Empty;
		_connectionLabel.text = ((!player.Online) ? TimerSystem.TimeToString(Connections.Frontend.GetPredictedServerTime() - player.DisconnectedAt) : player.Region.Name);
		PortraitBuilder.Argument portraitArgument = player.GetPortraitArgument();
		portraitArgument.Mask = _maskTexture;
		_portraitTexture.mainTexture = null;
		PortraitBuilder.Set(portraitArgument, _portraitTexture);
		_portraitBg.color = portraitArgument.BgColor;
		_nameLabel.alpha = 1f;
		_levelLabel.alpha = 1f;
		_skillLabel.alpha = 1f;
		_connectionLabel.alpha = 1f;
		_rankLabel.alpha = 1f;
		_portraitTexture.alpha = 1f;
	}
}
