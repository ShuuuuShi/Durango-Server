using Durango.Logic.Timer;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class BuildPostprocessHelpTooltip : TooltipBase
{
	[SerializeField]
	private UIWidget _portraitsContainer;

	[SerializeField]
	private BuildPostprocessPortrait _portraitBase;

	[SerializeField]
	private GameObject _buttonHelp;

	[SerializeField]
	private UISprite _timerUpperSprite;

	[SerializeField]
	private Transform _timerTickArrow;

	[SerializeField]
	private UILabel _timerLabel;

	[SerializeField]
	private RectLayout _layout;

	private ListObjectPool<BuildPostprocessPortrait> _portraits = new ListObjectPool<BuildPostprocessPortrait>();

	private Artifact _artifact;

	private int _remainTick;

	protected override void Start()
	{
		base.Start();
		UIEventListener.Get(_buttonHelp).onClick = OnClickButtonHelp;
		_portraits.BaseObject = _portraitBase;
		_portraits.Init(delegate(BuildPostprocessPortrait comp)
		{
			comp.Clicked = OnClickPortrait;
		});
	}

	public void Set(Artifact artifact)
	{
		_artifact = artifact;
		_remainTick = 0;
	}

	protected override void FillData()
	{
		if (_artifact.ArtifactState.Postprocess.HasValue)
		{
			string[] helpers = _artifact.ArtifactState.Postprocess.Value.Helpers;
			_portraits.Set(KUtility.GetSize(helpers));
			for (int i = 0; i < _portraits.Count; i++)
			{
				_portraits[i].Set(helpers[i]);
			}
		}
	}

	protected override void UpdateLayout()
	{
		UpdatePortraitsLayout();
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (_artifact != null)
		{
			Durango.Logic.Timer.Timer postProcessTimer = _artifact.PostProcessTimer;
			if (postProcessTimer == null || postProcessTimer.IsStop)
			{
				Hide();
			}
			else
			{
				UpdateTimer(postProcessTimer);
			}
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		base.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
	}

	protected override void OnHide()
	{
		base.OnHide();
		_artifact = null;
	}

	private void UpdateTimer(Durango.Logic.Timer.Timer timer)
	{
		float num = (timer.Now - timer.Since) / timer.Duration;
		_timerTickArrow.localEulerAngles = Vector3.back * 360f * num;
		_timerUpperSprite.fillAmount = 1f - num;
		int num2 = Mathf.CeilToInt(timer.Remain);
		if (num2 != _remainTick)
		{
			_remainTick = num2;
			_timerLabel.text = TimedeltaFormatter.Format(_remainTick);
		}
	}

	private void UpdatePortraitsLayout()
	{
		UIWidget portraitsContainer = _portraitsContainer;
		Point2 baseSize = new Point2(_portraitBase.GetComponent<UIWidget>().localSize);
		int num = portraitsContainer.width % baseSize.x;
		int num2 = portraitsContainer.width / baseSize.x;
		portraitsContainer.height = Mathf.CeilToInt((float)(_portraits.Count + 1) / (float)num2) * baseSize.y + num;
		Vector3 vector = portraitsContainer.localCorners[1];
		vector += new Vector3(num, -num) * 0.5f;
		for (int i = 0; i < _portraits.Count; i++)
		{
			_portraits[i].GetComponent<UIWidget>().SetPosition(vector + GetPortraitOffset(baseSize, num2, i), 0f, 1f);
		}
		_buttonHelp.GetComponent<UIWidget>().SetPosition(vector + GetPortraitOffset(baseSize, num2, _portraits.Count), 0f, 1f);
	}

	private static Vector3 GetPortraitOffset(Point2 baseSize, int countPerLine, int index)
	{
		return new Vector3(baseSize.x * (index % countPerLine), baseSize.y * (-index / countPerLine));
	}

	private void OnClickPortrait(BuildPostprocessPortrait comp)
	{
		if (comp.Player.Valid)
		{
			PlayerInfoPopup playerInfoPopup = UIManager.Popup.Tooltip<PlayerInfoPopup>();
			playerInfoPopup.AutoPosition = false;
			playerInfoPopup.Set(comp.Player);
			playerInfoPopup.Show();
			playerInfoPopup.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
			base.HideIgnoreParent = playerInfoPopup.transform;
		}
	}

	private void OnClickButtonHelp(GameObject go)
	{
		if (_artifact != null)
		{
			HelpPostprocess helpPostprocess = default(HelpPostprocess);
			helpPostprocess.EntityId = _artifact.EntityId;
			helpPostprocess.Tile = _artifact.WorldTile;
			HelpPostprocess msg = helpPostprocess;
			Connections.Frontend.Send(msg).On(delegate(HelpedPostprocess helped, PacketHeader _)
			{
				UIManager.SystemMsg(T._("마무리 시간이 {0} 감소했습니다. (오늘의 남은 도움 횟수 {1}회)", TimedeltaFormatter.Format(Mathf.CeilToInt(0f - helped.Timedelta)), helped.LeftHelpableCount.ToString()));
			});
		}
		Hide();
	}
}
