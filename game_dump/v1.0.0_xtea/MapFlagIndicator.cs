using ClanData;
using L10N;
using UnityEngine;

public class MapFlagIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _flagSprite;

	[SerializeField]
	private UITexture _emblemTexture;

	[SerializeField]
	private UISprite _stateSprite;

	[SerializeField]
	private UIWidget _timerWidget;

	[SerializeField]
	private UILabel _timerLabel;

	public void SetOwnerClan(ClanTerritory clan, Color color)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		SetTarget(clan.Grid * 8 + Point2.one * 8 / 2);
		SetColor(color);
		Reset();
		ClanSystem.GetClanInfo(clan.ClanId, OnOwnerClan);
	}

	private void Reset()
	{
		_emblemTexture.alpha = 0f;
		_emblemTexture.mainTexture = null;
		((Component)_stateSprite).gameObject.SetActive(false);
		((Component)_timerWidget).gameObject.SetActive(false);
	}

	private void OnOwnerClan(Clan clan)
	{
		SetTooltip(clan.Name);
		clan.GetEmblem(SetTexture);
		clan.GetClanWarState(out var state, out var remain);
		switch (state)
		{
		case ClanWarState.WarmUp:
		case ClanWarState.Match:
		case ClanWarState.RematchBreak:
			((Component)_stateSprite).gameObject.SetActive(true);
			((Component)_timerWidget).gameObject.SetActive(true);
			_stateSprite.spriteName = IconMap.Get(state);
			LabelUpdater.Set(_timerLabel, new SyncString(delegate(out string text, out float period)
			{
				text = T.Format("{0:sec:}", remain);
				period = 1f;
			}));
			break;
		}
	}

	private void SetColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_flagSprite.color = color;
	}

	private void SetTexture(Texture texture)
	{
		_emblemTexture.alpha = 1f;
		_emblemTexture.mainTexture = texture;
	}
}
