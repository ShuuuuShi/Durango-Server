using ClanData;
using L10N;
using UnityEngine;

public class ClanListNode : SelectableWidget
{
	[SerializeField]
	private UITexture _emblemSprite;

	[SerializeField]
	private GameObject _noEmblem;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _numberLabel;

	public void Set(Clan clan)
	{
		SetEmblem(null);
		_nameLabel.text = null;
		_levelLabel.text = null;
		_numberLabel.text = null;
		ClanSystem.GetClanInfo(clan, SetData);
	}

	private void SetData(Clan clan)
	{
		clan.GetEmblem(SetEmblem);
		_nameLabel.text = clan.Name;
		_levelLabel.text = T.Format("{0:lv:}", clan.Level);
		_numberLabel.text = $"{clan.MemberCount} / {clan.Capacity}";
	}

	private void SetEmblem(Texture2D texture)
	{
		if ((Object)(object)texture == (Object)null)
		{
			_noEmblem.gameObject.SetActive(true);
			((Component)_emblemSprite).gameObject.SetActive(false);
		}
		else
		{
			_noEmblem.gameObject.SetActive(false);
			((Component)_emblemSprite).gameObject.SetActive(true);
			_emblemSprite.mainTexture = (Texture)(object)texture;
		}
	}
}
