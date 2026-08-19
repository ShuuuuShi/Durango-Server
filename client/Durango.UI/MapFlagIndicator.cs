using UnityEngine;

namespace Durango.UI;

public class MapFlagIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _flagSprite;

	[SerializeField]
	private UITexture _emblemTexture;

	[SerializeField]
	private UIWidget _timerWidget;

	[SerializeField]
	private UILabel _timerLabel;

	private float _nextUpdateTimer;

	public override float VisibleZoom => 2f;

	public string ClanId { get; private set; }

	public void SetOwnerClan(string clanId)
	{
		SetColor((!ClanSystem.IsMyClan(clanId)) ? PresetColor.ClanFlag : PresetColor.PlayerClanFlag);
		Reset();
		ClanId = clanId;
		ClanSystem.GetEmblem(clanId, OnEmblem);
		RefreshState();
	}

	private void Reset()
	{
		_emblemTexture.mainTexture = null;
		_emblemTexture.gameObject.SetActive(value: false);
		ActivateWarWidget(activated: false);
		_nextUpdateTimer = 0f;
	}

	private void SetColor(Color color)
	{
		_flagSprite.color = color;
	}

	private void OnEmblem(Point2 pos)
	{
		if (pos.x < 0 || pos.y < 0)
		{
			_emblemTexture.gameObject.SetActive(value: false);
			return;
		}
		_emblemTexture.gameObject.SetActive(value: true);
		EmblemTexture.Set(_emblemTexture, pos);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (_nextUpdateTimer > 0f && Time.time > _nextUpdateTimer)
		{
			RefreshState();
		}
	}

	protected override void OnHide(bool isHide)
	{
		if (!isHide)
		{
			RefreshState();
		}
	}

	private void RefreshState()
	{
		if (!string.IsNullOrEmpty(ClanId) && !base.IsHidden)
		{
			SetColor((!ClanSystem.IsMyClan(ClanId)) ? PresetColor.ClanFlag : PresetColor.PlayerClanFlag);
			ActivateWarWidget(activated: false);
			_nextUpdateTimer = 0f;
		}
	}

	private void ActivateWarWidget(bool activated)
	{
		_timerLabel.gameObject.SetActive(activated);
		_timerWidget.gameObject.SetActive(activated);
	}
}
