using Durango.Render.Camera;
using UnityEngine;

namespace Durango.UI;

public class PlayerFloatingControl : MonoBehaviour
{
	[SerializeField]
	private float _bottomOffset;

	[SerializeField]
	private float _topOffset;

	[SerializeField]
	private Transform _top;

	[SerializeField]
	private UILabel _nametagLabel;

	[SerializeField]
	private UISprite _separator;

	[SerializeField]
	private UILabel _titletagLabel;

	[SerializeField]
	private UILabel _clantagLabel;

	[SerializeField]
	private GameObject _iconBg;

	[SerializeField]
	private GameObject _drawIcon;

	[SerializeField]
	private UISprite _floatingIcon;

	private Vector3 _baseClanLabelPos;

	public PlayerBehavior Target { get; set; }

	private void Awake()
	{
		_baseClanLabelPos = _clantagLabel.transform.localPosition;
	}

	public void Process(bool hideLocalPlayer)
	{
		bool flag = Target.WillBeRendered && Target.GetVisible() && (!Target.IsLocalPlayer || !hideLocalPlayer);
		base.gameObject.SetActive(flag);
		if (flag)
		{
			SetDrawIconVisible(Target.WorldLineRenderer.IsDrawing());
			Vector3 floatingUIPosition = Target.FloatingUIPosition;
			Vector3 world = floatingUIPosition + Vector3.down * _bottomOffset;
			Vector3 world2 = floatingUIPosition + Vector3.up * _topOffset;
			world = MainCamera.WorldToNGUIPos(world);
			base.transform.localPosition = world;
			if (_iconBg.activeSelf || _floatingIcon.gameObject.activeSelf)
			{
				world2 = MainCamera.WorldToNGUIPos(world2);
				_top.localPosition = world2 - world;
			}
		}
	}

	public void SetDrawIconVisible(bool visible)
	{
		_iconBg.SetActive(visible);
		_drawIcon.SetActive(visible);
	}

	public void SetClan(PlayerBehavior player)
	{
		if (player.HasClan)
		{
			_clantagLabel.gameObject.SetActive(value: true);
			_clantagLabel.text = $"<{player.Clan.ClanName}>";
		}
		else
		{
			_clantagLabel.gameObject.SetActive(value: false);
		}
		RefreshBottomLayout();
	}

	public void SetClanColor(Color c)
	{
		_clantagLabel.color = c;
	}

	public void SetTitle(string title)
	{
		if (string.IsNullOrEmpty(title))
		{
			_titletagLabel.gameObject.SetActive(value: false);
		}
		else
		{
			_titletagLabel.gameObject.SetActive(value: true);
			_titletagLabel.text = title;
		}
		RefreshBottomLayout();
	}

	public void SetTitleColor(Color c)
	{
		_titletagLabel.color = c;
	}

	public void SetName(string nameTag)
	{
		_nametagLabel.text = nameTag;
		UIUtility.UpdateAnchors(_nametagLabel.transform);
	}

	public void SetNameColor(Color c)
	{
		_nametagLabel.color = c;
	}

	public void SetFloatingIcon(string icon)
	{
		_floatingIcon.gameObject.SetActive(!string.IsNullOrEmpty(icon));
		_floatingIcon.spriteName = icon;
	}

	private void RefreshBottomLayout()
	{
		_clantagLabel.transform.localPosition = ((!_titletagLabel.gameObject.activeSelf) ? _titletagLabel.transform.localPosition : _baseClanLabelPos);
		_separator.enabled = _clantagLabel.gameObject.activeSelf || _titletagLabel.gameObject.activeSelf;
	}
}
