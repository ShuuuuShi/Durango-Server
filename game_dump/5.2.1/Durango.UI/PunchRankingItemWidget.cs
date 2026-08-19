using Durango.Player;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class PunchRankingItemWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _rankingTextPane;

	[SerializeField]
	private UILabel _textRanking;

	[SerializeField]
	private UIWidget _rightPane;

	[SerializeField]
	private UISprite _iconCrown;

	[SerializeField]
	private UITexture _texturePortrait;

	[SerializeField]
	private Texture _textureMask;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private UILabel _textLevel;

	[SerializeField]
	private UIWidget _verticalLine;

	[SerializeField]
	private UILabel _textTime;

	[SerializeField]
	private UILabel _textScore;

	[SerializeField]
	private Color[] _colorMedals;

	private bool _initialized;

	private Color _colorNoMedal;

	public string UserId { get; private set; }

	public int Score { get; private set; }

	public int RankingIndex { get; private set; }

	public void Init()
	{
		if (!_initialized)
		{
			_colorNoMedal = _textRanking.color;
			_initialized = true;
		}
	}

	public void Refresh(LeaderboardContent content, [NotNull] Durango.Player.PlayerInfo playerInfo, int? rankingIndex = null)
	{
		UserId = content.UserId;
		Score = (content.Damage.HasValue ? content.Damage.Value : 0);
		RankingIndex = (rankingIndex.HasValue ? rankingIndex.Value : 0);
		RefreshPanes(rankingIndex.HasValue);
		SetPlayerInfo(playerInfo);
		SetRankingInfo();
		_textTime.text = Times.Timeago(content.At);
		_textScore.text = ((Score < 0) ? string.Empty : Score.ToString());
		_verticalLine.UpdateAnchors();
		_textTime.UpdateAnchors();
	}

	private void RefreshPanes(bool showRanking)
	{
		_rightPane.leftAnchor.absolute = (showRanking ? _rankingTextPane.width : 0);
		UIUtility.UpdateAnchors(_rightPane.transform);
		_rankingTextPane.gameObject.SetActive(showRanking);
	}

	private void SetPlayerInfo([NotNull] Durango.Player.PlayerInfo playerInfo)
	{
		PortraitBuilder.Argument portraitArgument = playerInfo.GetPortraitArgument();
		portraitArgument.Mask = _textureMask;
		_texturePortrait.gameObject.SetActive(value: true);
		PortraitBuilder.Set(portraitArgument, _texturePortrait);
		_textName.text = playerInfo.Name;
		_textLevel.text = T._("{0:lv:}", playerInfo.Level);
	}

	private void SetRankingInfo()
	{
		Color color;
		if (0 < RankingIndex && RankingIndex <= _colorMedals.Length)
		{
			color = _colorMedals[RankingIndex - 1];
			_iconCrown.gameObject.SetActive(value: true);
			_iconCrown.color = color;
		}
		else
		{
			color = _colorNoMedal;
			_iconCrown.gameObject.SetActive(value: false);
		}
		_textRanking.text = ((0 >= RankingIndex) ? "-" : RankingIndex.ToString());
		_textRanking.color = color;
	}
}
