using UnityEngine;

public class PlayerFloatingUIControl : MonoBehaviour
{
	[SerializeField]
	private float _bottomOffset;

	[SerializeField]
	private float _topOffset;

	[SerializeField]
	private Transform _top;

	[SerializeField]
	private Transform _bottom;

	[SerializeField]
	private UILabel _nametagLabel;

	[SerializeField]
	private UILabel _titletagLabel;

	[SerializeField]
	private UILabel _clantagLabel;

	[SerializeField]
	private UILabel _statusLabel;

	[SerializeField]
	private GameObject _iconBG;

	[SerializeField]
	private GameObject _voiceIcon;

	[SerializeField]
	private GameObject _drawIcon;

	private Vector3 _baseClanLabelPos;

	public PlayerBehavior Target { get; set; }

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_baseClanLabelPos = ((Component)_clantagLabel).transform.localPosition;
	}

	public void Process(bool hideLocalPlayer)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Target.IsVisible && Target.GetRenderEnabled() && (!Target.IsLocalPlayer || !hideLocalPlayer);
		((Component)this).gameObject.SetActive(flag);
		if (flag)
		{
			SetDrawIconVisible(Target.WorldLineRenderer.IsDrawing());
			Vector3 world = Target.CurrentPosition + Vector3.down * _bottomOffset;
			Vector3 world2 = Target.CurrentPosition + Vector3.up * _topOffset;
			world = MainCamera.WorldToNGUIPos(world);
			world2 = MainCamera.WorldToNGUIPos(world2);
			((Component)this).transform.localPosition = world;
			_top.localPosition = world2 - world;
		}
	}

	public void SetVoiceIconVisible(bool visible)
	{
		_iconBG.SetActive(visible);
		_voiceIcon.SetActive(visible);
	}

	public void SetDrawIconVisible(bool visible)
	{
		_iconBG.SetActive(visible);
		_drawIcon.SetActive(visible);
	}

	public void SetClan(string clan)
	{
		if (string.IsNullOrEmpty(clan))
		{
			((Component)_clantagLabel).gameObject.SetActive(false);
		}
		else
		{
			((Component)_clantagLabel).gameObject.SetActive(true);
			_clantagLabel.text = $"<{clan}>";
		}
		RefreshBottomLayout();
	}

	public void SetClanColor(Color c)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_clantagLabel.color = c;
	}

	public void SetTitle(string title)
	{
		if (string.IsNullOrEmpty(title))
		{
			((Component)_titletagLabel).gameObject.SetActive(false);
		}
		else
		{
			((Component)_titletagLabel).gameObject.SetActive(true);
			_titletagLabel.text = title;
		}
		RefreshBottomLayout();
	}

	public void SetTitleColor(Color c)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_titletagLabel.color = c;
	}

	public void SetName(string nameTag)
	{
		_nametagLabel.text = nameTag;
	}

	public void SetNameColor(Color c)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_nametagLabel.color = c;
	}

	public void SetStatus(string status)
	{
		_statusLabel.text = status;
	}

	public void SetStatusColor(Color c)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_statusLabel.color = c;
	}

	private void RefreshBottomLayout()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		((Component)_clantagLabel).transform.localPosition = ((!((Component)_titletagLabel).gameObject.activeSelf) ? ((Component)_titletagLabel).transform.localPosition : _baseClanLabelPos);
	}
}
