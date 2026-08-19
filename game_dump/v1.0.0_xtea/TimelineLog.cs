using Building_;
using JetBrains.Annotations;
using Player;
using UnityEngine;

public class TimelineLog : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _ShelvesForIcon;

	[SerializeField]
	private GameObject[] _ShelvesForComment;

	[SerializeField]
	private GameObject _iconArrow;

	[SerializeField]
	private GameObject _widgetPortrait;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private Texture _textureMask;

	[SerializeField]
	private GameObject _widgetArtifact;

	[SerializeField]
	private UISprite _backgroundArtifact;

	[SerializeField]
	private UISprite _iconArtifact;

	[SerializeField]
	private GameObject _widgetCommentGuide;

	[SerializeField]
	private UISpriteLabel _textComment;

	[SerializeField]
	private UISpriteLabel _textTime;

	[SerializeField]
	private UIWidget _dotLine;

	[SerializeField]
	private Color _colorPositive;

	[SerializeField]
	private Color _colorNegative;

	public void SetLog(TimelineLogSystem.LogInfo logInfo)
	{
		SetPortrait(logInfo.PlayerInfo);
		SetArtifact(logInfo.Blueprint, logInfo.IsNegative);
		_textComment.text = logInfo.Text;
		_textTime.text = LocalizeSystem.Format("#artifact_timeline_log_time", TimerSystem.Timeago(logInfo.Time));
		UpdateWidgetLayout(logInfo.PlayerInfo, logInfo.Blueprint);
	}

	public void SetWidth(int width)
	{
		UIWidget component = ((Component)this).GetComponent<UIWidget>();
		if (component.width != width)
		{
			component.width = width;
			_textComment.Label.UpdateAnchors();
			_textTime.Label.UpdateAnchors();
			_dotLine.UpdateAnchors();
		}
	}

	private void SetPortrait([CanBeNull] PlayerInfo playerInfo)
	{
		if (playerInfo != null && playerInfo.Valid)
		{
			PortraitBuilder.Argument portraitArgument = playerInfo.GetPortraitArgument();
			portraitArgument.Mask = _textureMask;
			PortraitBuilder.Set(portraitArgument, _portraitTexture);
		}
	}

	private void SetArtifact([CanBeNull] Blueprint blueprint, bool negative)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (blueprint != null)
		{
			_backgroundArtifact.color = ((!negative) ? _colorPositive : _colorNegative);
			UIUtility.SetSpriteName(_iconArtifact, blueprint.Icon);
		}
	}

	private void UpdateWidgetLayout([CanBeNull] PlayerInfo playerInfo, [CanBeNull] Blueprint blueprint)
	{
		int indexShelf = 0;
		bool flag = playerInfo?.Valid ?? false;
		bool flag2 = blueprint != null;
		UpdateWidgetLayoutByShelves(_widgetPortrait, flag, _ShelvesForIcon, ref indexShelf);
		UpdateWidgetLayoutByShelves(_widgetArtifact, flag2, _ShelvesForIcon, ref indexShelf);
		UpdateWidgetLayoutByShelves(_widgetCommentGuide, active: true, _ShelvesForComment, ref indexShelf);
		_iconArrow.SetActive(flag && flag2);
		_textComment.Label.UpdateAnchors();
	}

	private static void UpdateWidgetLayoutByShelves(GameObject widgetTarget, bool active, GameObject[] shelves, ref int indexShelf)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (active)
		{
			widgetTarget.transform.localPosition = shelves[indexShelf++].transform.localPosition;
			widgetTarget.SetActive(true);
		}
		else
		{
			widgetTarget.SetActive(false);
		}
	}
}
