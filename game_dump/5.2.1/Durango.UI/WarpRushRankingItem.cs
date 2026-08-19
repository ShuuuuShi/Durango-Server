using Durango.Logic.WarpRush;
using Durango.Player;
using Durango.UI.Popup;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class WarpRushRankingItem : MonoBehaviour
{
	[SerializeField]
	protected UITexture _portraitTexture;

	[SerializeField]
	private Texture _portraitMaskTexture;

	[SerializeField]
	private UILabel _rankingLabel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _resourceLabel;

	[SerializeField]
	private GameObject _portraitBorder;

	[SerializeField]
	private GameObject _selector;

	[SerializeField]
	private GameObject _separator;

	public string EntityId { get; private set; }

	public void Set(int rank, Record record)
	{
		EntityId = record.EntityId;
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(record.EntityId, delegate(PlayerInfo info)
		{
			if (!(EntityId != record.EntityId) && info.Valid)
			{
				SetPortrait(info.GetPortraitArgument());
			}
		});
		_rankingLabel.text = GetRankText(rank);
		_nameLabel.text = record.Name;
		_resourceLabel.text = record.GetScoreText();
		_portraitBorder.SetActive(value: false);
		_selector.SetActive(value: false);
	}

	public void SetMyRecord(int rank, string scoreText, bool visibleSeparator = true)
	{
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		EntityId = localPlayer.EntityId;
		_rankingLabel.text = "<em>" + GetRankText(rank) + "</em>";
		_nameLabel.text = "<em>" + localPlayer.GetName() + "</em>";
		_resourceLabel.text = scoreText;
		_portraitBorder.SetActive(value: true);
		_selector.SetActive(value: true);
		_separator.SetActive(visibleSeparator);
		SetPortrait(PlayerBehavior.LocalPlayer.GetPortraitArgument());
	}

	private void SetPortrait(PortraitBuilder.Argument portrait)
	{
		portrait.Mask = _portraitMaskTexture;
		PortraitBuilder.Set(portrait, _portraitTexture);
	}

	private void OnClick()
	{
		if (!(EntityId == PlayerBehavior.LocalPlayer.EntityId))
		{
			ShowProfileTooltip();
		}
	}

	private static string GetRankText(int rank)
	{
		return rank switch
		{
			1 => $"[size=40][ffd85b][icon=icon_laurel] [i]{rank}[/i] [icon=icon_laurel?flip=Horizontally][-][/size]", 
			2 => $"[i][size=40][cbcbcb]{rank}[-][/size][/i]", 
			3 => $"[i][size=40][986a4e]{rank}[-][/size][/i]", 
			_ => $"[i]{rank}[/i]", 
		};
	}

	protected void ShowProfileTooltip()
	{
		PlayerInfoPopup.RequestShow(EntityId, delegate(PlayerInfoPopup tooltip)
		{
			tooltip.AutoPosition = false;
			tooltip.Show();
			tooltip.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
		});
	}
}
