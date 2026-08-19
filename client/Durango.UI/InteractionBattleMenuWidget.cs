using Durango.UI.Control;
using InteractionData;
using UnityEngine;

namespace Durango.UI;

public class InteractionBattleMenuWidget : SelectableWidget
{
	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UILabel _shortcut;

	[SerializeField]
	private TweenerPlayer _tweenerPlayer;

	[SerializeField]
	private TweenPosition _tweenPositon;

	[EnumList(typeof(State), false, 0, -1)]
	[SerializeField]
	private Color[] _iconColors;

	public InteractionMenuData Data { get; private set; }

	protected override void OnInit()
	{
		base.OnInit();
		GameSystem<InputSystem>.Instance().On(InputCommand.BeginFight, OnClickHotKey);
	}

	protected override void OnRefresh(State state)
	{
		base.OnRefresh(state);
		_iconTexture.color = ((!(Data.Color != Color.white)) ? _iconColors[(int)state] : Data.Color);
	}

	private void OnClickHotKey(InputCommandMessage message)
	{
		if (base.gameObject.activeInHierarchy)
		{
			OnClick();
		}
	}

	public void Set(InteractionMenuData data)
	{
		Init();
		Data = data;
		_shortcut.text = $"<shortcut_box>{InputCommand.BeginFight}</shortcut_box>";
		_iconTexture.SetIcon(data.Icon);
		UIUtility.UpdateAnchors(base.transform);
		_tweenerPlayer.Play();
		Refresh();
	}

	public void SetRadius(float radius)
	{
		Vector3 to = new Vector3(radius, 0f, 0f);
		_tweenPositon.to = to;
	}
}
