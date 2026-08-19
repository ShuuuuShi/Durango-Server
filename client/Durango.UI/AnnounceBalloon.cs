using Durango.Player;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

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

	[CanBeNull]
	[SerializeField]
	private UIMaskedSprite _sprite;

	[SerializeField]
	private GameObject _iconSystem;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private TweenAlpha _tweenAlpha;

	private int _initialSpriteSize;

	private bool _showAlways;

	private float _timeToBlink;

	private float _timeToFinish;

	private bool _hasText;

	public AnnounceType Type { get; private set; }

	public string EntityId { get; private set; }

	public Vector2 TilePosition { get; private set; }

	public bool IsShow { get; set; }

	private void Awake()
	{
		if (!(_sprite == null))
		{
			_initialSpriteSize = _sprite.width;
			_sprite.MaskedSprite = "target_masking_bg";
		}
	}

	public void Show(Vector2 tilePos, string entityId, string text, AnnounceType type, AnnounceBalloonMeta meta)
	{
		base.gameObject.SetActive(value: true);
		IsShow = true;
		Type = type;
		EntityId = entityId;
		TilePosition = tilePos;
		SetIcons(EntityId, meta);
		SetText(text);
		UpdateShow(meta);
		UpdateTween();
	}

	public void Process()
	{
		if (_showAlways)
		{
			return;
		}
		float time = Time.time;
		if (time > _timeToBlink)
		{
			if (!_tweenAlpha.enabled)
			{
				_tweenAlpha.tweenFactor = 0f;
				_tweenAlpha.PlayForward();
			}
			if (time > _timeToFinish)
			{
				IsShow = false;
			}
		}
	}

	public void SetTitleVisible(bool visible)
	{
		if (_hasText)
		{
			_textName.gameObject.SetActive(visible);
		}
	}

	private void SetIcons(string entityId, AnnounceBalloonMeta meta)
	{
		if (!string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(meta._icon.sprite) && !string.IsNullOrEmpty(meta._iconEffect.sprite))
		{
			meta._icon.Set(_icon);
			meta._iconEffect.Set(_iconEffect);
			_icon.gameObject.SetActive(value: true);
		}
		else
		{
			_icon.gameObject.SetActive(value: false);
		}
	}

	private void ResetIcons()
	{
		if (_sprite != null)
		{
			_sprite.gameObject.SetActive(value: false);
		}
		_texture.cachedGameObject.SetActive(value: false);
	}

	public void SetPortrait(PlayerInfo info)
	{
		ResetIcons();
		if (!string.IsNullOrEmpty(info.EntityId))
		{
			_texture.gameObject.SetActive(value: true);
			if (_iconSystem != null)
			{
				_iconSystem.SetActive(value: false);
			}
			PortraitBuilder.Argument portraitArgument = info.GetPortraitArgument();
			portraitArgument.Mask = _textureMask;
			PortraitBuilder.Set(portraitArgument, _texture);
		}
		else
		{
			_texture.gameObject.SetActive(value: false);
			if (_iconSystem != null)
			{
				_iconSystem.SetActive(value: true);
			}
		}
	}

	public void SetSprite(string entityId, string spriteName, int spriteSize)
	{
		ResetIcons();
		if (_sprite == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(entityId))
		{
			_sprite.gameObject.SetActive(value: true);
			spriteSize = ((spriteSize != 0) ? spriteSize : _initialSpriteSize);
			_sprite.SetDimensions(spriteSize, spriteSize);
			if (_iconSystem != null)
			{
				_iconSystem.SetActive(value: false);
			}
			_sprite.spriteName = spriteName;
		}
		else
		{
			_sprite.gameObject.SetActive(value: false);
			if (_iconSystem != null)
			{
				_iconSystem.SetActive(value: true);
			}
		}
	}

	private void SetText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			_hasText = false;
			_textName.gameObject.SetActive(value: false);
		}
		else
		{
			_hasText = true;
			_textName.gameObject.SetActive(value: true);
			_textName.text = text;
		}
	}

	private void UpdateShow(AnnounceBalloonMeta meta)
	{
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
	}

	private void UpdateTween()
	{
		_tweenAlpha.ResetToBeginning();
		_tweenAlpha.enabled = false;
	}
}
