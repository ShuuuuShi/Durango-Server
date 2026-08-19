using MapData;
using Player;
using UnityEngine;

public class AnnounceBalloon : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UISprite _iconEffect;

	[SerializeField]
	private UITexture _texture;

	[SerializeField]
	private Texture _textureMask;

	[SerializeField]
	private GameObject _iconSystem;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private TweenAlpha _tweenAlpha;

	private bool _showAlways;

	private float _timeToBlink;

	private float _timeToFinish;

	public AnnounceType Type { get; private set; }

	public ulong EntityId { get; private set; }

	public Vector2 TilePosition { get; private set; }

	public bool IsHided => !((Component)this).gameObject.activeSelf;

	public void Show(Vector2 tilePos, Vector2 humanePos, PlayerInfo info, AnnounceType type, AnnounceBalloonMeta meta)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		Type = type;
		EntityId = info.EntityId;
		TilePosition = tilePos;
		SetIcons(info, meta);
		SetPortrait(info);
		SetText(info, humanePos);
		if (meta._showDuration > 0f)
		{
			float time = Time.time;
			_timeToBlink = time + Mathf.Max(0f, meta._showDuration - meta._blinkDuration);
			_timeToFinish = time + meta._showDuration;
			_showAlways = false;
		}
		else
		{
			_showAlways = true;
		}
		_tweenAlpha.PlayForward();
		_tweenAlpha.ResetToBeginning();
		((Behaviour)_tweenAlpha).enabled = false;
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void Update()
	{
		if (_showAlways || IsHided)
		{
			return;
		}
		float time = Time.time;
		if (time > _timeToBlink)
		{
			if (!((Behaviour)_tweenAlpha).enabled)
			{
				_tweenAlpha.tweenFactor = 0f;
				_tweenAlpha.PlayForward();
			}
			if (time > _timeToFinish)
			{
				Hide();
			}
		}
	}

	private void SetIcons(PlayerInfo info, AnnounceBalloonMeta meta)
	{
		if (info.EntityId != 0L)
		{
			meta._icon.Set(_icon);
			meta._iconEffect.Set(_iconEffect);
			((Component)_icon).gameObject.SetActive(true);
		}
		else
		{
			((Component)_icon).gameObject.SetActive(false);
		}
	}

	private void SetPortrait(PlayerInfo info)
	{
		if (info.EntityId != 0L)
		{
			((Component)_texture).gameObject.SetActive(true);
			_iconSystem.SetActive(false);
			PortraitBuilder.Argument portraitArgument = info.GetPortraitArgument();
			portraitArgument.Mask = _textureMask;
			PortraitBuilder.Set(portraitArgument, _texture);
		}
		else
		{
			((Component)_texture).gameObject.SetActive(false);
			_iconSystem.SetActive(true);
		}
	}

	private void SetText(PlayerInfo info, Vector2 humanePos)
	{
		_textName.text = $"[FFD85B]{info.Name}[-]\n{GetPositionText(Mathf.RoundToInt(humanePos.x), Mathf.RoundToInt(humanePos.y))}";
	}

	public static string GetPositionText(int x, int y)
	{
		return $"[ffd85b]X[-] {x} [ffd85b]Y[-] {y}";
	}
}
